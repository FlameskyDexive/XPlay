using System;

namespace ET
{
    [BTNodeHandler]
    public sealed class BTSelectSkillAction : ABTNodeHandler<BTSelectSkill>
    {
        protected override BTExecResult Run(BTSelectSkill node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            int preferredSlot = context.GetIntArgument(node.Definition, "preferredSlot", -1);
            if (!context.TrySelectSkill(unit, preferredSlot, out Skill skill, out int slot))
            {
                if (ShouldTrace(context))
                {
                    Log.Info($"[MatchRobotAI][{context.TreeName}] select_skill failed unit:{unit.Id} preferred:{preferredSlot}");
                }
                return BTExecResult.Failure;
            }

            if (ShouldTrace(context))
            {
                bool canCast = context.Blackboard?.Get<bool>(BTCombatBlackboardKeys.CanCast, false) ?? false;
                Log.Info($"[MatchRobotAI][{context.TreeName}] select_skill unit:{unit.Id} skill:{skill?.SkillConfig?.Id ?? 0} slot:{slot} canCast:{canCast}");
            }
            return BTExecResult.Success;
        }

        private static bool ShouldTrace(BTExecutionContext context)
        {
            return context != null && string.Equals(context.TreeName, ConstValue.StateSyncMatchRobotBehaviorTree, StringComparison.OrdinalIgnoreCase);
        }
    }

    [BTNodeHandler]
    public sealed class BTCanCastSelectedSkillCondition : ABTNodeHandler<BTCanCastSelectedSkill>
    {
        protected override BTExecResult Run(BTCanCastSelectedSkill node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            return context.CanCastSelectedSkill(unit, out _) ? BTExecResult.Success : BTExecResult.Failure;
        }
    }

    [BTNodeHandler]
    public sealed class BTCheckStateChangeResultCondition : ABTNodeHandler<BTCheckStateChangeResult>
    {
        protected override BTExecResult Run(BTCheckStateChangeResult node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            int expectedResult = context.GetIntArgument(node.Definition, "result", (int)ECombatStateChangeResult.Success);
            int currentResult = context.Blackboard?.Get<int>(BTCombatBlackboardKeys.StateChangeResult, (int)ECombatStateChangeResult.InvalidState)
                ?? (int)ECombatStateChangeResult.InvalidState;
            return currentResult == expectedResult ? BTExecResult.Success : BTExecResult.Failure;
        }
    }

    [BTNodeHandler]
    public sealed class BTCastSelectedSkillAction : ABTNodeHandler<BTCastSelectedSkill>
    {
        protected override BTExecResult Run(BTCastSelectedSkill node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            if (!context.TryGetSelectedSkill(unit, out Skill skill, out int slot))
            {
                return BTExecResult.Failure;
            }

            long targetUnitId = context.Blackboard.Get<long>(BTCombatBlackboardKeys.TargetId, 0);
            SkillCastRequest request = new SkillCastRequest
            {
                SkillSlot = slot,
                SkillId = skill.SkillConfig.Id,
                TargetUnitId = targetUnitId,
                AimPoint = unit.Position,
                AimDirection = unit.Forward,
                PressedTime = TimeInfo.Instance.ServerNow(),
            };

            bool success = unit.GetComponent<SkillComponent>()?.TryRequestCast(request, out _) == true;
            context.Blackboard.Set(BTCombatBlackboardKeys.CanCast, success);
            context.Blackboard.Set(BTCombatBlackboardKeys.InCast, success);
            return success ? BTExecResult.Success : BTExecResult.Failure;
        }
    }

    [BTNodeHandler]
    public sealed class BTWaitCastCompleteAction : ABTNodeHandler<BTWaitCastComplete>
    {
        protected override BTExecResult Run(BTWaitCastComplete node, BTEnv env)
        {
            BTExecutionSession session = env.GetSession();
            BTNodeRuntimeState state = env.GetState(node);
            if (state.State == BTNodeState.Running)
            {
                return BTExecResult.Running;
            }

            BTCoroutineTokenState tokenState = BTFlowDriver.StartToken(session, node);
            WaitCastCompleteAsync(session, node, tokenState.Version).Coroutine();
            return BTExecResult.Running;
        }

        private static async ETTask WaitCastCompleteAsync(BTExecutionSession session, BTWaitCastComplete node, long version)
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

            int pollIntervalMs = context.GetIntArgument(node.Definition, "pollIntervalMs", 100);
            int timeoutMs = context.GetIntArgument(node.Definition, "timeoutMs", 5000);
            long startTime = TimeInfo.Instance.ServerNow();

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
                SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
                if (skillCastComponent == null || !skillCastComponent.IsCasting())
                {
                    context.Blackboard.Set(BTCombatBlackboardKeys.InCast, false);
                    BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Success);
                    return;
                }

                if (timeoutMs > 0 && TimeInfo.Instance.ServerNow() - startTime >= timeoutMs)
                {
                    BTFlowDriver.Resume(session, node.RuntimeNodeId, BTExecResult.Failure);
                    return;
                }

                await timerComponent.WaitAsync(pollIntervalMs, tokenState.Token);
            }
        }
    }
}
