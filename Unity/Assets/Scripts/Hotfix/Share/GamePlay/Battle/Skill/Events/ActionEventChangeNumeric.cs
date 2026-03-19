namespace ET
{
    [FriendOf(typeof(ActionEvent))]
    [ActionEvent(SceneType.RoomRoot, EActionEventType.ChangeNumeric)]
    public class ActionEventChangeNumeric : IActionEvent
    {
        public void Run(ActionEvent actionEvent, EventType.ActionEventData args)
        {
            Unit owner = args.owner;
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            ChangeNumericActionEventData eventData = actionEvent?.ActionEventConfig?.EventData as ChangeNumericActionEventData;
            if (eventData == null)
            {
                return;
            }

            EActionEventTargetRule defaultRule = ActionEventTargetHelper.GetDefaultTargetRule(actionEvent);
            EActionEventTargetRule targetRule = ActionEventTargetHelper.GetTargetRule(eventData.TargetRule, defaultRule);
            if (!ActionEventTargetHelper.TryResolveTarget(actionEvent, args, targetRule, out Unit target))
            {
                return;
            }

            EInterruptLevel interruptLevel = eventData.InterruptLevel >= (int)EInterruptLevel.None && eventData.InterruptLevel <= (int)EInterruptLevel.Fatal
                ? (EInterruptLevel)eventData.InterruptLevel
                : EInterruptLevel.None;
            BattleHelper.ApplyNumericDelta(owner, target, eventData.NumericType, eventData.Delta, interruptLevel);
        }
    }
}
