namespace ET
{
    [EnableClass]
    public sealed partial class SkillConfig
    {
        public long MpCost { get; private set; }

        public bool RequiresTarget { get; private set; }

        public EActionEventTargetRule PrimaryTargetRule { get; private set; }

        partial void PostResolve()
        {
            this.MpCost = 0;
            this.RequiresTarget = false;
            this.PrimaryTargetRule = EActionEventTargetRule.CurrentOrSelf;

            if (this.ActionEventIds == null)
            {
                return;
            }

            for (int index = 0; index < this.ActionEventIds.Count; ++index)
            {
                ActionEventConfig actionEventConfig = ActionEventConfigCategory.Instance.GetOrDefault(this.ActionEventIds[index]);
                if (actionEventConfig == null)
                {
                    continue;
                }

                switch (actionEventConfig.EventData)
                {
                    case ChangeNumericActionEventData changeNumericActionEventData:
                    {
                        EActionEventTargetRule targetRule = GetTargetRule(changeNumericActionEventData.TargetRule, EActionEventTargetRule.CurrentTarget);
                        this.CollectTargetRule(targetRule);
                        if (changeNumericActionEventData.NumericType == NumericType.Mp
                            && changeNumericActionEventData.Delta < 0
                            && targetRule == EActionEventTargetRule.Self)
                        {
                            this.MpCost += -changeNumericActionEventData.Delta;
                        }

                        break;
                    }
                    case AddBuffActionEventData addBuffActionEventData:
                    {
                        this.CollectTargetRule(GetTargetRule(addBuffActionEventData.TargetRule, EActionEventTargetRule.CurrentTarget));
                        break;
                    }
                    case RemoveBuffActionEventData removeBuffActionEventData:
                    {
                        this.CollectTargetRule(GetTargetRule(removeBuffActionEventData.TargetRule, EActionEventTargetRule.CurrentTarget));
                        break;
                    }
                }
            }
        }

        private void CollectTargetRule(EActionEventTargetRule targetRule)
        {
            if (targetRule == EActionEventTargetRule.CurrentTarget || targetRule == EActionEventTargetRule.ExplicitTarget)
            {
                this.RequiresTarget = true;
            }

            if (this.PrimaryTargetRule == EActionEventTargetRule.CurrentOrSelf || this.PrimaryTargetRule == EActionEventTargetRule.Self)
            {
                this.PrimaryTargetRule = targetRule;
            }
        }

        private static EActionEventTargetRule GetTargetRule(int targetRuleValue, EActionEventTargetRule defaultRule)
        {
            return targetRuleValue >= (int)EActionEventTargetRule.Self && targetRuleValue <= (int)EActionEventTargetRule.ExplicitTarget
                ? (EActionEventTargetRule)targetRuleValue
                : defaultRule;
        }
    }
}
