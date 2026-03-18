using System;
using System.Collections.Generic;
using ET.EventType;

namespace ET
{
    [FriendOf(typeof(SkillComponent))]
    [EntitySystemOf(typeof(SkillTimelineComponent))]
    [FriendOf(typeof(SkillTimelineComponent))]
    [FriendOf(typeof(ActionEvent))]
    public static partial class SkillTimelineComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillTimelineComponent self, int skillId, int skillLevel)
        {
            self.Skillconfig = SkillConfigCategory.Instance.Get(skillId, skillLevel);
        }

        /// <summary>
        /// 固定帧驱动
        /// </summary>
        [EntitySystem]
        public static void FixedUpdate(this SkillTimelineComponent self)
        {
            using (ListComponent<long> list = ListComponent<long>.Create())
            {
                long timeNow = TimeInfo.Instance.ServerNow();
                foreach ((long key, Entity value) in self.Children)
                {
                    ActionEvent actionEvent = (ActionEvent)value;

                    if (timeNow > actionEvent.EventTriggerTime)
                    {
                        ActionEventComponent.Instance.Run(actionEvent,
                            new ActionEventData() { actionEventType = actionEvent.ActionEventType, owner = actionEvent.OwnerUnit });
                        list.Add(key);
                    }
                }

                foreach (long id in list)
                {
                    self.Remove(id);
                }
            }
        }

        public static void StartPlay(this SkillTimelineComponent self)
        {
            self.ClearEvents();
            self.StartSpellTime = TimeInfo.Instance.ServerNow();
            self.InitEvents();
        }

        public static void ClearEvents(this SkillTimelineComponent self)
        {
            if (self.Children == null || self.Children.Count == 0)
            {
                return;
            }

            using (ListComponent<long> list = ListComponent<long>.Create())
            {
                foreach ((long key, Entity _) in self.Children)
                {
                    list.Add(key);
                }

                foreach (long id in list)
                {
                    self.Remove(id);
                }
            }
        }

        public static void InitEvents(this SkillTimelineComponent self)
        {
            try
            {
                for (int i = 0; i < self.Skillconfig.ActionEventIds.Count; i++)
                {
                    int actionEventId = self.Skillconfig.ActionEventIds[i];
                    ActionEventConfig actionEventConfig = ActionEventConfigCategory.Instance.Get(actionEventId);
                    if (actionEventConfig == null)
                    {
                        continue;
                    }

#if DOTNET
                    if (actionEventConfig.IsClientOnly)
                    {
                        continue;
                    }
#endif

                    int triggerTime = self.Skillconfig.ActionEventTriggerPercent[i] * self.Skillconfig.Life / 100;
                    self.AddChild<ActionEvent, int, int, EActionEventSourceType>(actionEventId, triggerTime, EActionEventSourceType.Skill);
                }
            }
            catch (Exception e)
            {
                Log.Error($"事件id与事件触发时间百分比数量不一致， 技能id：{self.Skillconfig.Id}, lv:{self.Skillconfig.Level} \n{e}");
            }
        }

        private static void Remove(this SkillTimelineComponent self, long id)
        {
            if (!self.Children.TryGetValue(id, out Entity skillEvent))
            {
                return;
            }

            skillEvent.Dispose();
            self.Children.Remove(id);
        }
    }
}
