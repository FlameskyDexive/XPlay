using System.Collections;
using System.Collections.Generic;
using ET.EventType;

namespace ET
{
    [FriendOf(typeof(ActionEvent))]
    public static class ActionEventHelper
    {
        public static void CreateActionEvent(this Buff self, int actionEventId, bool isAutoRun = true)
        {

            ActionEvent actionEvent = self.AddChild<ActionEvent, int, int, EActionEventSourceType>(actionEventId, 0, EActionEventSourceType.Buff);

            if (isAutoRun)
                RunActionEvent(actionEvent);
        }
        public static void CreateActionEvent(this BulletComponent self, int actionEventId, bool isAutoRun = true)
        {
            ActionEvent actionEvent = self.AddChild<ActionEvent, int, int, EActionEventSourceType>(actionEventId, 0, EActionEventSourceType.Bullet);
            if(isAutoRun)
                RunActionEvent(actionEvent);
        }

        public static void CreateActionEvent(this BulletComponent self, int actionEventId, Unit target, bool isAutoRun = true)
        {
            ActionEvent actionEvent = self.AddChild<ActionEvent, int, int, EActionEventSourceType>(actionEventId, 0, EActionEventSourceType.Bullet);
            if (isAutoRun)
            {
                RunActionEvent(actionEvent, target);
            }
        }

        private static void RunActionEvent(ActionEvent actionEvent)
        {
            RunActionEvent(actionEvent, null);
        }

        private static void RunActionEvent(ActionEvent actionEvent, Unit target)
        {
            ActionEventComponent.Instance.Run(actionEvent, new ActionEventData()
            {
                actionEventType = actionEvent.ActionEventType,
                owner = actionEvent.OwnerUnit,
                target = target,
            });
            actionEvent.Dispose();
        }
    }
}
