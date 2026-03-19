namespace ET
{
    [ActionEvent(SceneType.RoomRoot, EActionEventType.Bullet)]
    [FriendOf(typeof(ActionEvent))]
    public class ActionEventBullet : IActionEvent
    {
        public void Run(ActionEvent actionEvent, EventType.ActionEventData args)
        {
            Unit owner = args.owner;
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            ProjectileHelper.SpawnProjectiles(actionEvent, owner);
        }
    }
}
