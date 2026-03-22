using System;
using Unity.Mathematics;

namespace ET
{
    [BTNodeHandler]
    public sealed class BTFindCombatTargetAction : ABTNodeHandler<BTFindCombatTarget>
    {
        protected override BTExecResult Run(BTFindCombatTarget node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            float maxRange = math.max(0f, context.GetFloatArgument(node.Definition, "maxRange", 30f));
            Unit target = TargetSelectHelper.FindNearestCombatTarget(unit, maxRange <= 0f ? float.MaxValue : maxRange);
            if (target == null)
            {
                if (ShouldTrace(context))
                {
                    Log.Info($"[MatchRobotAI][{context.TreeName}] target not found unit:{unit.Id} maxRange:{maxRange}");
                }
                context.ClearCombatTarget(unit);
                return BTExecResult.Failure;
            }

            if (ShouldTrace(context))
            {
                float distance = TargetSelectHelper.GetDistance(unit, target);
                Log.Info($"[MatchRobotAI][{context.TreeName}] target found unit:{unit.Id} target:{target.Id} distance:{distance:F2}");
            }
            context.SetCombatTarget(unit, target);
            return BTExecResult.Success;
        }

        private static bool ShouldTrace(BTExecutionContext context)
        {
            return context != null && string.Equals(context.TreeName, ConstValue.StateSyncMatchRobotBehaviorTree, StringComparison.OrdinalIgnoreCase);
        }
    }

    [BTNodeHandler]
    public sealed class BTValidateCombatTargetCondition : ABTNodeHandler<BTValidateCombatTarget>
    {
        protected override BTExecResult Run(BTValidateCombatTarget node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            context.SyncCombatBlackboard(unit);
            float maxRange = math.max(0f, context.GetFloatArgument(node.Definition, "maxRange", 0f));
            float checkRange = maxRange <= 0f ? float.MaxValue : maxRange;
            return context.TryResolveValidCombatTarget(unit, out _, checkRange) ? BTExecResult.Success : BTExecResult.Failure;
        }
    }

    [BTNodeHandler]
    public sealed class BTClearInvalidTargetAction : ABTNodeHandler<BTClearInvalidTarget>
    {
        protected override BTExecResult Run(BTClearInvalidTarget node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            if (!context.TryResolveValidCombatTarget(unit, out _))
            {
                context.ClearCombatTarget(unit);
            }

            return BTExecResult.Success;
        }
    }

    [BTNodeHandler]
    public sealed class BTFaceTargetAction : ABTNodeHandler<BTFaceTarget>
    {
        protected override BTExecResult Run(BTFaceTarget node, BTEnv env)
        {
            BTExecutionContext context = env.BindContext(node);
            if (!context.TryGetCombatUnit(out Unit unit))
            {
                return BTExecResult.Failure;
            }

            if (!context.TryResolveValidCombatTarget(unit, out Unit target))
            {
                return BTExecResult.Failure;
            }

            BTCombatHelper.FaceTarget(unit, target);
            return BTExecResult.Success;
        }
    }
}
