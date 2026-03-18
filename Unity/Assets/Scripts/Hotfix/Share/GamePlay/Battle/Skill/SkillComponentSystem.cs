using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(SkillComponent))]
    [FriendOf(typeof(SkillComponent))]
    public static partial class SkillComponentSystem
    {
        [EntitySystem]
        public static void Awake(this SkillComponent self)
        {
        }

        [EntitySystem]
        public static void Destroy(this SkillComponent self)
        {
            self.IdSkillMap.Clear();
            self.AbstractTypeSkills.Clear();
            self.SkillDic.Clear();
        }

        [EntitySystem]
        public static void Awake(this SkillComponent self, List<int> skillIds)
        {
            foreach (int skillId in skillIds)
            {
                self.AddSkill(skillId);
            }
        }

        /// <summary>
        /// 添加技能
        /// </summary>
        public static Skill AddSkill(this SkillComponent self, int configId, int skillLevel = 1)
        {
            if (!self.IdSkillMap.TryGetValue(configId, out long _))
            {
                Skill skill = self.AddChild<Skill, int, int>(configId, skillLevel);
                self.IdSkillMap.Add(configId, skill.Id);
                SkillConfig skillConfig = SkillConfigCategory.Instance.Get(configId, skillLevel);
                ESkillAbstractType abstractType = (ESkillAbstractType)skillConfig.AbstractType;
                if (!self.AbstractTypeSkills.TryGetValue(abstractType, out List<long> skills))
                {
                    skills = new List<long>();
                    self.AbstractTypeSkills[abstractType] = skills;
                }

                self.AbstractTypeSkills[abstractType].Add(skill.Id);
            }

            return self.GetChild<Skill>(self.IdSkillMap[configId]);
        }

        public static bool TryGetSkill(this SkillComponent self, int configId, out Skill skill)
        {
            if (self.IdSkillMap.TryGetValue(configId, out long skillId))
            {
                skill = self.GetChild<Skill>(skillId);
                return true;
            }

            skill = null;
            return false;
        }

        /// <summary>
        /// 通过技能类型获取技能
        /// </summary>
        public static bool TryGetSkill(this SkillComponent self, ESkillAbstractType abstractType, int index, out Skill skill)
        {
            if (self.AbstractTypeSkills.TryGetValue(abstractType, out List<long> skillIds))
            {
                if (skillIds?.Count > index)
                {
                    skill = self.GetChild<Skill>(skillIds[index]);
                    return true;
                }
            }

            skill = null;
            return false;
        }

        /// <summary>
        /// 释放技能兼容入口
        /// </summary>
        public static bool SpellSkill(this SkillComponent self, ESkillAbstractType absType, int index = 0)
        {
            return self.TryRequestCast(absType, index, out _);
        }

        public static bool TryRequestCast(this SkillComponent self, ESkillAbstractType absType, int index, out ESkillCastResult result)
        {
            if (!self.TryGetSkill(absType, index, out Skill skill))
            {
                result = ESkillCastResult.SkillNotFound;
                return false;
            }

            TargetComponent targetComponent = self.Unit.GetComponent<TargetComponent>();
            SkillCastRequest request = new SkillCastRequest()
            {
                SkillSlot = index,
                SkillId = skill.SkillConfig.Id,
                TargetUnitId = targetComponent != null ? targetComponent.GetCurrentTargetId() : 0,
                AimPoint = self.Unit.Position,
                AimDirection = self.Unit.Forward,
                PressedTime = TimeInfo.Instance.ServerNow(),
            };

            return self.TryRequestCast(request, out result);
        }

        public static bool TryRequestCast(this SkillComponent self, SkillCastRequest request, out ESkillCastResult result)
        {
            if (request.SkillId <= 0)
            {
                result = ESkillCastResult.SkillNotFound;
                return false;
            }

            if (!self.TryGetSkill(request.SkillId, out Skill skill))
            {
                result = ESkillCastResult.SkillNotFound;
                return false;
            }

            SkillCastComponent skillCastComponent = self.Unit.GetComponent<SkillCastComponent>();
            if (skillCastComponent == null)
            {
                result = ESkillCastResult.InvalidState;
                return false;
            }

            if (math.lengthsq(request.AimDirection) <= 0.0001f)
            {
                request.AimDirection = self.Unit.Forward;
            }

            return skillCastComponent.TryStartCast(skill, request, out result);
        }

        public static bool IsDead(this SkillComponent self)
        {
            NumericComponent numericComponent = self.Unit.GetComponent<NumericComponent>();
            return numericComponent != null && numericComponent.GetAsInt(NumericType.Hp) <= 0;
        }
    }
}
