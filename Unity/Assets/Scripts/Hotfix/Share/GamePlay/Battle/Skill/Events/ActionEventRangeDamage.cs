namespace ET
{
    /// <summary>
    /// 执行范围伤害技能事件
    /// </summary>
    [FriendOf(typeof(ActionEvent))]
    [ActionEvent(SceneType.RoomRoot, EActionEventType.RangeDamage)]
    public class ActionEventRangeDamage : IActionEvent
    {
        public void Run(ActionEvent actionEvent, EventType.ActionEventData args)
        {
            Unit owner = args.owner;
            if (owner == null)
            {
                return;
            }

            RangeDamageActionEventData eventData = actionEvent?.ActionEventConfig?.EventData as RangeDamageActionEventData;
            if (eventData == null)
            {
                return;
            }

            using ListComponent<Unit> targets = CombatResolverHelper.QueryRangeDamageTargets(owner, eventData);
            for (int index = 0; index < targets.Count; ++index)
            {
                Unit target = targets[index];
                if (target == null || target.IsDisposed)
                {
                    continue;
                }

                CombatResolverHelper.ResolveHit(owner, target);
            }
        }
    }
}
