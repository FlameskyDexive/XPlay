namespace ET
{
    [FriendOf(typeof(ActionEvent))]
    [ActionEvent(SceneType.RoomRoot, EActionEventType.RemoveBuff)]
    public class ActionEventRemoveBuff : IActionEvent
    {
        public void Run(ActionEvent actionEvent, EventType.ActionEventData args)
        {
            RemoveBuffActionEventData eventData = actionEvent?.ActionEventConfig?.EventData as RemoveBuffActionEventData;
            if (eventData == null || eventData.BuffId <= 0)
            {
                return;
            }

            EActionEventTargetRule defaultRule = ActionEventTargetHelper.GetDefaultTargetRule(actionEvent);
            EActionEventTargetRule targetRule = ActionEventTargetHelper.GetTargetRule(eventData.TargetRule, defaultRule);
            if (!ActionEventTargetHelper.TryResolveTarget(actionEvent, args, targetRule, out Unit target))
            {
                return;
            }

            target.GetComponent<BuffComponent>()?.DispelBuff(eventData.BuffId);
        }
    }
}
