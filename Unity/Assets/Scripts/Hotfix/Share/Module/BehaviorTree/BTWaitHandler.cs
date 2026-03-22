using System;

namespace ET
{
    [BTNodeHandler]
    public sealed class BTWaitHandler : ABTNodeHandler<BTWait>
    {
        protected override BTExecResult Run(BTWait node, BTEnv env)
        {
            BTExecutionSession session = env.GetSession();
            session.UpdateTreeContext(node);
            if (BTHandlerUtility.TryGetTerminalResult(session, node, out BTExecResult result))
            {
                return result;
            }

            if (node.Definition is not BTWaitNodeData)
            {
                session.SetState(node, BTNodeState.Failure);
                return BTExecResult.Failure;
            }

            BTNodeRuntimeState state = env.GetState(node);
            if (state.State == BTNodeState.Running)
            {
                return BTExecResult.Running;
            }

            Entity owner = session.Owner;
            TimerComponent timerComponent = owner?.Root()?.GetComponent<TimerComponent>();
            if (timerComponent == null)
            {
                session.SetState(node, BTNodeState.Success);
                return BTExecResult.Success;
            }

            BTCoroutineTokenState tokenState = BTFlowDriver.StartToken(session, node);
            BTExecutionContext context = env.BindContext(node);
            if (ShouldTrace(context))
            {
                Log.Info($"[MatchRobotAI][{context.TreeName}] wait start runtime:{context.RuntimeId} ms:{((BTWaitNodeData)node.Definition).WaitMilliseconds}");
            }
            session.SetState(node, BTNodeState.Running);
            RunWaitAsync(session, node, timerComponent, tokenState.Version).Coroutine();
            return BTExecResult.Running;
        }

        private static async ETTask RunWaitAsync(BTExecutionSession session, BTWait node, TimerComponent timerComponent, long version)
        {
            if (!BTFlowDriver.IsTokenValid(session, node, version, out BTCoroutineTokenState tokenState))
            {
                return;
            }

            try
            {
                if (node.Definition is not BTWaitNodeData definition)
                {
                    BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                    return;
                }

                await timerComponent.WaitAsync(definition.WaitMilliseconds, tokenState.Token);
                if (!BTFlowDriver.IsTokenValid(session, node, version, out _))
                {
                    return;
                }

                BTExecutionContext context = session.Env.BindContext(node);
                if (ShouldTrace(context))
                {
                    Log.Info($"[MatchRobotAI][{context.TreeName}] wait end runtime:{context.RuntimeId} ms:{definition.WaitMilliseconds}");
                }
                BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Success);
            }
            catch (Exception exception)
            {
                if (!BTFlowDriver.IsTokenValid(session, node, version, out _))
                {
                    return;
                }

                session.LogException(exception, node);
                BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
            }
        }

        private static bool ShouldTrace(BTExecutionContext context)
        {
            return context != null && string.Equals(context.TreeName, ConstValue.StateSyncMatchRobotBehaviorTree, StringComparison.OrdinalIgnoreCase);
        }
    }
}
