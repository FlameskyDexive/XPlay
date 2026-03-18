using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(Skill))]
    [FriendOf(typeof(Skill))]
    public static partial class SkillSystem
    {
        [EntitySystem]
        public static void Awake(this Skill self, int skillId, int skillLevel)
        {
            self.SkillId = skillId;
            self.SkillLevel = skillLevel;
            self.CD = self.SkillConfig.CD > 0 ? self.SkillConfig.CD : 1000;
            self.AddComponent<SkillTimelineComponent, int, int>(skillId, skillLevel);
        }

        [EntitySystem]
        public static void Destroy(this Skill self)
        {
        }

        public static Unit GetOwnerUnit(this Skill self)
        {
            return self.GetParent<SkillComponent>().Unit;
        }

        public static bool IsInCd(this Skill self)
        {
            if (self.SpellStartTime + self.CD > TimeInfo.Instance.ServerNow())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 开始释放技能
        /// </summary>
        public static void StartSpell(this Skill self)
        {
            self.SpellStartTime = TimeInfo.Instance.ServerNow();
            self.SpellEndTime = self.SpellStartTime + self.SkillConfig.Life;
            self.GetComponent<SkillTimelineComponent>().StartPlay();
        }

        public static void CancelSpell(this Skill self)
        {
            self.SpellEndTime = TimeInfo.Instance.ServerNow();
            self.GetComponent<SkillTimelineComponent>().ClearEvents();
        }
    }
}
