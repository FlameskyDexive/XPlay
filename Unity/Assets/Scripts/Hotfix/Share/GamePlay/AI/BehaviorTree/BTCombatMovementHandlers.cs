using Unity.Mathematics;

namespace ET
{
    [BTNodeHandler]
    public sealed class BTMoveToCombatRangeAction : ABTNodeHandler<BTMoveToCombatRange>
    {
        protected override BTExecResult Run(BTMoveToCombatRange node, BTEnv env)
        {
            BTExecutionSession session = env.GetSession();
            BTNodeRuntimeState state = env.GetState(node);
            if (state.State == BTNodeState.Running)
            {
                return BTExecResult.Running;
            }

            BTCoroutineTokenState tokenState = BTFlowDriver.StartToken(session, node);
            MoveToCombatRangeAsync(session, node, tokenState.Version).Coroutine();
            return BTExecResult.Running;
        }

        private static async ETTask MoveToCombatRangeAsync(BTExecutionSession session, BTMoveToCombatRange node, long version)
        {
            BTExecutionContext context = session.Env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                return;
            }

            EntityRef<Unit> unitRef = unit;
            TimerComponent timerComponent = unit.Root()?.GetComponent<TimerComponent>();
            if (timerComponent == null)
            {
                BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                return;
            }

            float desiredRange = math.max(0.1f, context.GetFloatArgument(node.Definition, "range", 2.5f));
            int tickIntervalMs = context.GetIntArgument(node.Definition, "tickIntervalMs", 100);

            try
            {
                while (BTFlowDriver.IsTokenValid(session, node, version, out BTCoroutineTokenState tokenState))
                {
                    unit = unitRef;
                    if (unit == null || unit.IsDisposed)
                    {
                        BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                        return;
                    }

                    context = session.Env.BindContext(node);
                    context.SyncCombatBlackboard(unit);
                    if (!context.TryResolveValidCombatTarget(unit, out Unit target))
                    {
                        BTCombatHelper.StopMove(unit);
                        BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                        return;
                    }

                    float distance = TargetSelectHelper.GetDistance(unit, target);
                    context.Blackboard.Set(BTCombatBlackboardKeys.TargetDistance, distance);
                    if (distance <= desiredRange)
                    {
                        BTCombatHelper.StopMove(unit);
                        BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Success);
                        return;
                    }

                    BTCombatHelper.FaceTarget(unit, target);
                    if (!BTCombatHelper.TryStartMove(unit))
                    {
                        BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                        return;
                    }

                    await timerComponent.WaitAsync(tickIntervalMs, tokenState.Token);
                }
            }
            finally
            {
                Unit finalUnit = unitRef;
                if (finalUnit != null && !finalUnit.IsDisposed)
                {
                    BTCombatHelper.StopMove(finalUnit);
                }
            }
        }
    }

    [BTNodeHandler]
    public sealed class BTNeedRetreatCondition : ABTNodeHandler<BTNeedRetreat>
    {
        protected override BTExecResult Run(BTNeedRetreat node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            float threshold = math.clamp(context.GetFloatArgument(node.Definition, "hpRatioThreshold", 0.3f), 0f, 1f);
            bool needRetreat = BTCombatHelper.GetHpRatio(unit) <= threshold;
            context.Blackboard.Set(BTCombatBlackboardKeys.NeedRetreat, needRetreat);
            return needRetreat ? BTExecResult.Success : BTExecResult.Failure;
        }
    }
}
