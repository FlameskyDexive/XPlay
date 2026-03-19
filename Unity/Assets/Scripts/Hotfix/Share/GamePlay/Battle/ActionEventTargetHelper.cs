using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(ActionEvent))]
    [FriendOf(typeof(TargetComponent))]
    public static class ActionEventTargetHelper
    {
        public static EActionEventTargetRule GetTargetRule(int targetRuleValue, EActionEventTargetRule defaultRule)
        {
            return targetRuleValue >= (int)EActionEventTargetRule.Self && targetRuleValue <= (int)EActionEventTargetRule.ExplicitTarget
                ? (EActionEventTargetRule)targetRuleValue
                : defaultRule;
        }

        public static EActionEventTargetRule GetDefaultTargetRule(ActionEvent actionEvent)
        {
            if (actionEvent == null)
            {
                return EActionEventTargetRule.CurrentTarget;
            }

            return actionEvent.SourceType switch
            {
                EActionEventSourceType.Buff => EActionEventTargetRule.Self,
                EActionEventSourceType.Bullet => EActionEventTargetRule.ExplicitTarget,
                _ => EActionEventTargetRule.CurrentTarget,
            };
        }

        public static bool TryResolveTarget(ActionEvent actionEvent, EventType.ActionEventData args, EActionEventTargetRule targetRule, out Unit target)
        {
            target = null;
            Unit owner = args.owner;
            switch (targetRule)
            {
                case EActionEventTargetRule.Self:
                    target = owner;
                    return target != null && !target.IsDisposed;
                case EActionEventTargetRule.CurrentTarget:
                    return TryResolveCurrentTarget(owner, args.target, out target);
                case EActionEventTargetRule.CurrentOrSelf:
                    if (TryResolveCurrentTarget(owner, args.target, out target))
                    {
                        return true;
                    }

                    target = owner;
                    return target != null && !target.IsDisposed;
                case EActionEventTargetRule.ExplicitTarget:
                    target = args.target;
                    return target != null && !target.IsDisposed;
                default:
                    return false;
            }
        }

        private static bool TryResolveCurrentTarget(Unit owner, Unit explicitTarget, out Unit target)
        {
            target = explicitTarget;
            if (target != null && !target.IsDisposed)
            {
                return true;
            }

            target = null;
            if (owner == null || owner.IsDisposed)
            {
                return false;
            }

            TargetComponent targetComponent = owner.GetComponent<TargetComponent>();
            if (targetComponent == null || targetComponent.CurrentTargetId == 0)
            {
                return false;
            }

            return TargetSelectHelper.TryGetTarget(owner, targetComponent.CurrentTargetId, out target);
        }
    }
}
