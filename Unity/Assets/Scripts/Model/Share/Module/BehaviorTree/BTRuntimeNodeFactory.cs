using System;

namespace ET
{
    public static class BTRuntimeNodeFactory
    {
        public static BTNode Create(BTNodeData definition)
        {
            switch (definition)
            {
                case BTLogNodeData:
                    return new BTLog();
                case BTSetBlackboardNodeData:
                    return new BTSetBlackboard();
                case BTSetBlackboardIfMissingData:
                    return new BTSetBlackboardIfMissing();
                case BTBlackboardExistsNodeData:
                    return new BTBlackboardExists();
                case BTBlackboardCompareNodeData:
                    return new BTBlackboardCompare();
                case BTPatrolNodeData:
                    return new BTPatrol();
                case BTHasPatrolPathNodeData:
                    return new BTHasPatrolPath();
                case BTRootNodeData:
                    return new BTRoot();
                case BTSequenceNodeData:
                    return new BTSequence();
                case BTSelectorNodeData:
                    return new BTSelector();
                case BTParallelNodeData:
                    return new BTParallel();
                case BTInverterNodeData:
                    return new BTInverter();
                case BTSucceederNodeData:
                    return new BTSucceeder();
                case BTFailerNodeData:
                    return new BTFailer();
                case BTRepeaterNodeData:
                    return new BTRepeater();
                case BTBlackboardConditionNodeData:
                    return new BTBlackboardCondition();
                case BTServiceNodeData serviceNodeData:
                    return CreateServiceCall(serviceNodeData);
                case BTActionNodeData actionNodeData:
                    return CreateActionNode(actionNodeData);
                case BTConditionNodeData conditionNodeData:
                    return CreateConditionNode(conditionNodeData);
                case BTWaitNodeData:
                    return new BTWait();
                case BTSubTreeNodeData:
                    return new BTSubTreeCall();
                default:
                    return null;
            }
        }

        private static BTNode CreateActionNode(BTActionNodeData definition)
        {
            if (string.Equals(definition.TypeId, BTBuiltinNodeTypes.Log, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, "Log", StringComparison.OrdinalIgnoreCase))
            {
                return new BTLog();
            }

            if (string.Equals(definition.TypeId, BTBuiltinNodeTypes.SetBlackboard, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, "SetBlackboard", StringComparison.OrdinalIgnoreCase))
            {
                return new BTSetBlackboard();
            }

            if (string.Equals(definition.TypeId, BTBuiltinNodeTypes.SetBlackboardIfMissing, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, "SetBlackboardIfMissing", StringComparison.OrdinalIgnoreCase))
            {
                return new BTSetBlackboardIfMissing();
            }

            if (string.Equals(definition.TypeId, BTPatrolNodeTypes.Patrol, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, "BTPatrol", StringComparison.OrdinalIgnoreCase))
            {
                return new BTPatrol();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.FindCombatTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTFindCombatTarget), StringComparison.OrdinalIgnoreCase))
            {
                return new BTFindCombatTarget();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.ClearInvalidTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTClearInvalidTarget), StringComparison.OrdinalIgnoreCase))
            {
                return new BTClearInvalidTarget();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.SetCombatState, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTSetCombatState), StringComparison.OrdinalIgnoreCase))
            {
                return new BTSetCombatState();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.StopMove, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTStopMove), StringComparison.OrdinalIgnoreCase))
            {
                return new BTStopMove();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.MoveToCombatRange, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTMoveToCombatRange), StringComparison.OrdinalIgnoreCase))
            {
                return new BTMoveToCombatRange();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.RetreatFromCombatTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTRetreatFromCombatTarget), StringComparison.OrdinalIgnoreCase))
            {
                return new BTRetreatFromCombatTarget();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.FaceTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTFaceTarget), StringComparison.OrdinalIgnoreCase))
            {
                return new BTFaceTarget();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.SelectSkill, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTSelectSkill), StringComparison.OrdinalIgnoreCase))
            {
                return new BTSelectSkill();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.CastSelectedSkill, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTCastSelectedSkill), StringComparison.OrdinalIgnoreCase))
            {
                return new BTCastSelectedSkill();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.WaitCastComplete, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ActionHandlerName, nameof(BTWaitCastComplete), StringComparison.OrdinalIgnoreCase))
            {
                return new BTWaitCastComplete();
            }

            return new BTActionCall();
        }

        private static BTNode CreateConditionNode(BTConditionNodeData definition)
        {
            if (string.Equals(definition.TypeId, BTBuiltinNodeTypes.BlackboardExists, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, "BlackboardExists", StringComparison.OrdinalIgnoreCase))
            {
                return new BTBlackboardExists();
            }

            if (string.Equals(definition.TypeId, BTBuiltinNodeTypes.BlackboardCompare, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, "BlackboardCompare", StringComparison.OrdinalIgnoreCase))
            {
                return new BTBlackboardCompare();
            }

            if (string.Equals(definition.TypeId, BTPatrolNodeTypes.HasPatrolPath, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, "BTHasPatrolPath", StringComparison.OrdinalIgnoreCase))
            {
                return new BTHasPatrolPath();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.ValidateCombatTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, nameof(BTValidateCombatTarget), StringComparison.OrdinalIgnoreCase))
            {
                return new BTValidateCombatTarget();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.CanCastSelectedSkill, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, nameof(BTCanCastSelectedSkill), StringComparison.OrdinalIgnoreCase))
            {
                return new BTCanCastSelectedSkill();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.CheckStateChangeResult, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, nameof(BTCheckStateChangeResult), StringComparison.OrdinalIgnoreCase))
            {
                return new BTCheckStateChangeResult();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.InControl, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, nameof(BTInControl), StringComparison.OrdinalIgnoreCase))
            {
                return new BTInControl();
            }

            if (string.Equals(definition.TypeId, BTCombatNodeTypes.NeedRetreat, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.ConditionHandlerName, nameof(BTNeedRetreat), StringComparison.OrdinalIgnoreCase))
            {
                return new BTNeedRetreat();
            }

            return new BTConditionCall();
        }

        private static BTServiceCall CreateServiceCall(BTServiceNodeData definition)
        {
            return new BTServiceCall();
        }
    }
}
