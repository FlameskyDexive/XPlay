using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(SkillCastComponent))]
    [FriendOf(typeof(SkillCastComponent))]
    public static partial class SkillCastComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillCastComponent self)
        {
            self.AimDirection = new float3(0, 0, 1);
        }

        [EntitySystem]
        private static void Destroy(this SkillCastComponent self)
        {
            self.ResetRuntime();
        }

        [EntitySystem]
        private static void FixedUpdate(this SkillCastComponent self)
        {
            self.TickCast();
        }

        public static ESkillCastResult ValidateCast(this SkillCastComponent self, Skill skill)
        {
            if (skill == null)
            {
                return ESkillCastResult.SkillNotFound;
            }

            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent != null && numericComponent.GetAsInt(NumericType.Hp) <= 0)
            {
                return ESkillCastResult.Dead;
            }

            if (skill.IsInCd())
            {
                return ESkillCastResult.InCd;
            }

            if (self.CurrentSkillId != 0 && TimeInfo.Instance.ServerNow() < self.RecoverEndTime)
            {
                return ESkillCastResult.InvalidState;
            }

            return ESkillCastResult.Success;
        }

        public static bool TryStartCast(this SkillCastComponent self, Skill skill, SkillCastRequest request, out ESkillCastResult result)
        {
            result = self.ValidateCast(skill);
            if (result != ESkillCastResult.Success)
            {
                return false;
            }

            self.StartCast(skill, request);
            return true;
        }

        public static void StartCast(this SkillCastComponent self, Skill skill, SkillCastRequest request)
        {
            long now = TimeInfo.Instance.ServerNow();
            Unit unit = self.GetParent<Unit>();

            self.CurrentSkillId = skill.Id;
            self.CurrentSkillConfigId = skill.SkillConfig.Id;
            self.CurrentCastSeq = request.ClientCastSeq > 0 ? request.ClientCastSeq : self.CurrentCastSeq + 1;
            self.TargetUnitId = request.TargetUnitId;
            self.AimPoint = request.AimPoint;
            if (math.lengthsq(request.AimDirection) > 0.0001f)
            {
                self.AimDirection = math.normalize(request.AimDirection);
            }
            else
            {
                self.AimDirection = unit.Forward;
            }

            self.CastStartTime = now;
            self.CastPointTime = now;
            self.RecoverEndTime = now + skill.SkillConfig.Life;
            self.NextGlobalCdEndTime = now + skill.SkillConfig.CD;
            self.HasQueuedRequest = false;
            self.QueuedRequest = default;

            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            if (targetComponent != null && request.TargetUnitId != 0)
            {
                targetComponent.SetTarget(request.TargetUnitId);
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            combatStateComponent?.BeginCast(skill.SkillConfig.Id, request.TargetUnitId, self.RecoverEndTime);

            skill.StartSpell();
        }

        public static void TickCast(this SkillCastComponent self)
        {
            if (self.CurrentSkillId == 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerNow();
            if (now < self.RecoverEndTime)
            {
                return;
            }

            self.FinishCast();
        }

        public static bool IsCasting(this SkillCastComponent self)
        {
            return self != null && self.CurrentSkillId != 0 && TimeInfo.Instance.ServerNow() < self.RecoverEndTime;
        }

        public static void InterruptCast(this SkillCastComponent self)
        {
            if (self.CurrentSkillId == 0)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            SkillComponent skillComponent = unit.GetComponent<SkillComponent>();
            Skill currentSkill = skillComponent?.GetChild<Skill>(self.CurrentSkillId);
            currentSkill?.CancelSpell();

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            combatStateComponent?.SetIdle(TimeInfo.Instance.ServerNow());

            self.ResetRuntime();
        }

        public static void FinishCast(this SkillCastComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            combatStateComponent?.FinishCast(TimeInfo.Instance.ServerNow());
            self.ResetRuntime();
        }

        private static void ResetRuntime(this SkillCastComponent self)
        {
            self.CurrentSkillId = 0;
            self.CurrentSkillConfigId = 0;
            self.TargetUnitId = 0;
            self.AimPoint = default;
            self.AimDirection = new float3(0, 0, 1);
            self.CastStartTime = 0;
            self.CastPointTime = 0;
            self.RecoverEndTime = 0;
            self.NextGlobalCdEndTime = 0;
            self.HasQueuedRequest = false;
            self.QueuedRequest = default;
        }
    }
}
