namespace ET
{
    [FriendOf(typeof(ActionEvent))]
    [ActionEvent(SceneType.RoomRoot, EActionEventType.AddBuff)]
    public class ActionEventAddBuff : IActionEvent
    {
        public void Run(ActionEvent actionEvent, EventType.ActionEventData args)
        {
            Unit owner = args.owner;
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            AddBuffActionEventData eventData = actionEvent?.ActionEventConfig?.EventData as AddBuffActionEventData;
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

            bool applied = target.GetComponent<BuffComponent>()?.AddBuff(new BuffApplyRequest
            {
                BuffId = eventData.BuffId,
                SourceUnitId = owner.Id,
                SourceSkillConfigId = actionEvent.SkillConfig?.Id ?? 0,
            }) == true;

            Log.Info($"action event add buff owner:{owner.Id} target:{target.Id} buff:{eventData.BuffId} success:{applied}");
        }
    }
}
