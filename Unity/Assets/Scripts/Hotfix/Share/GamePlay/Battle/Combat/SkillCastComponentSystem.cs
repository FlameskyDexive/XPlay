using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(SkillCastComponent))]
    [FriendOf(typeof(SkillCastComponent))]
    [FriendOf(typeof(SkillComponent))]
    [FriendOf(typeof(CombatStateComponent))]
    [FriendOf(typeof(TargetComponent))]
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
            return self.ValidateCast(skill, self.CreateDefaultRequest(skill), false);
        }

        public static ESkillCastResult ValidateCast(this SkillCastComponent self, Skill skill, SkillCastRequest request, bool ignoreCastingState = false)
        {
            if (skill == null)
            {
                return ESkillCastResult.SkillNotFound;
            }

            Unit unit = self.GetParent<Unit>();
            long now = TimeInfo.Instance.ServerNow();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent != null && numericComponent.GetAsInt(NumericType.Hp) <= 0)
            {
                return ESkillCastResult.Dead;
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null)
            {
                if (combatStateComponent.State == ECombatState.Dead || combatStateComponent.HasAnyTag(ECombatTag.Dead))
                {
                    return ESkillCastResult.Dead;
                }

                if (combatStateComponent.HasAnyTag(ECombatTag.SoftControl | ECombatTag.HardControl))
                {
                    return ESkillCastResult.Controlled;
                }

                if (combatStateComponent.HasAnyTag(ECombatTag.Silence))
                {
                    return ESkillCastResult.BlockedByTag;
                }
            }

            if (request.TargetUnitId != 0)
            {
                if (!TargetSelectHelper.TryGetTarget(unit, request.TargetUnitId, out Unit target) || !TargetSelectHelper.IsValidCombatTarget(unit, target))
                {
                    return ESkillCastResult.NoTarget;
                }

                float skillRange = EstimateSkillRange(skill);
                if (skillRange < float.MaxValue && TargetSelectHelper.GetDistance(unit, target) > skillRange)
                {
                    return ESkillCastResult.OutOfRange;
                }
            }

            if (skill.IsInCd())
            {
                return ESkillCastResult.InCd;
            }

            if (!ignoreCastingState && self.NextGlobalCdEndTime > now)
            {
                return ESkillCastResult.InCd;
            }

            if (!ignoreCastingState && self.IsCasting())
            {
                return ESkillCastResult.InvalidState;
            }

            return ESkillCastResult.Success;
        }

        public static bool TryStartCast(this SkillCastComponent self, Skill skill, SkillCastRequest request, out ESkillCastResult result)
        {
            result = self.ValidateCast(skill, request);
            if (result != ESkillCastResult.Success)
            {
                return false;
            }

            self.StartCast(skill, request);
            return true;
        }

        public static bool TryQueueCast(this SkillCastComponent self, Skill skill, SkillCastRequest request, out ESkillCastResult result)
        {
            result = self.ValidateCast(skill, request, true);
            if (result != ESkillCastResult.Success)
            {
                return false;
            }

            self.QueuedRequest = request;
            self.HasQueuedRequest = true;
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
            self.FillRequestAim(skill, ref request);
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
            self.CastPointTime = now + GetCastPointOffsetMs(skill);
            self.RecoverEndTime = now + skill.SkillConfig.Life;
            self.NextGlobalCdEndTime = self.RecoverEndTime;
            self.HasQueuedRequest = false;
            self.QueuedRequest = default;

            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            if (targetComponent != null && request.TargetUnitId != 0)
            {
                targetComponent.LastTargetId = targetComponent.CurrentTargetId;
                targetComponent.CurrentTargetId = request.TargetUnitId;
                targetComponent.LockTarget = false;
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            combatStateComponent?.BeginCast(skill.SkillConfig.Id, request.TargetUnitId, self.CastPointTime, self.RecoverEndTime);

            skill.StartSpell();
        }

        public static void TickCast(this SkillCastComponent self)
        {
            if (self.CurrentSkillId == 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerNow();
            Unit unit = self.GetParent<Unit>();

            if (self.ShouldCancelForInvalidTarget(unit))
            {
                self.InterruptCast();
                return;
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null)
            {
                if (combatStateComponent.SubState == ECombatSubState.CastPoint && now >= self.CastPointTime)
                {
                    combatStateComponent.EnterActiveWindow(self.RecoverEndTime);
                    return;
                }

                if (combatStateComponent.SubState == ECombatSubState.ActiveWindow && now < self.RecoverEndTime)
                {
                    combatStateComponent.EnterRecover(self.RecoverEndTime);
                }
            }

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
            SkillCastRequest queuedRequest = self.QueuedRequest;
            bool hasQueuedRequest = self.HasQueuedRequest;
            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            combatStateComponent?.FinishCast(TimeInfo.Instance.ServerNow());
            self.ResetRuntime();

            if (!hasQueuedRequest)
            {
                return;
            }

            self.TryStartQueuedRequest(queuedRequest);
        }

        private static SkillCastRequest CreateDefaultRequest(this SkillCastComponent self, Skill skill)
        {
            Unit unit = self.GetParent<Unit>();
            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            return new SkillCastRequest
            {
                SkillId = skill?.SkillConfig.Id ?? 0,
                TargetUnitId = targetComponent != null ? targetComponent.CurrentTargetId : 0,
                AimPoint = unit.Position,
                AimDirection = unit.Forward,
                PressedTime = TimeInfo.Instance.ServerNow(),
            };
        }

        private static void TryStartQueuedRequest(this SkillCastComponent self, SkillCastRequest request)
        {
            Unit unit = self.GetParent<Unit>();
            SkillComponent skillComponent = unit.GetComponent<SkillComponent>();
            if (skillComponent == null)
            {
                return;
            }

            if (!skillComponent.IdSkillMap.TryGetValue(request.SkillId, out long skillId))
            {
                return;
            }

            Skill skill = skillComponent.GetChild<Skill>(skillId);
            if (skill == null)
            {
                return;
            }

            self.TryStartCast(skill, request, out _);
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
            self.HasQueuedRequest = false;
            self.QueuedRequest = default;
        }

        private static int GetCastPointOffsetMs(Skill skill)
        {
            if (skill?.SkillConfig?.ActionEventTriggerPercent == null || skill.SkillConfig.ActionEventTriggerPercent.Count == 0)
            {
                return 0;
            }

            int minPercent = 100;
            foreach (int triggerPercent in skill.SkillConfig.ActionEventTriggerPercent)
            {
                minPercent = Math.Min(minPercent, Math.Clamp(triggerPercent, 0, 100));
            }

            return skill.SkillConfig.Life <= 0 ? 0 : skill.SkillConfig.Life * minPercent / 100;
        }

        private static void FillRequestAim(this SkillCastComponent self, Skill skill, ref SkillCastRequest request)
        {
            Unit unit = self.GetParent<Unit>();
            if (request.TargetUnitId != 0 && TargetSelectHelper.TryGetTarget(unit, request.TargetUnitId, out Unit target))
            {
                request.AimPoint = target.Position;
                float3 direction = target.Position - unit.Position;
                if (math.lengthsq(direction) > 0.0001f)
                {
                    request.AimDirection = math.normalize(direction);
                    return;
                }
            }

            if (math.lengthsq(request.AimDirection) <= 0.0001f)
            {
                request.AimDirection = unit.Forward;
            }

            if (math.lengthsq(request.AimPoint - unit.Position) <= 0.0001f)
            {
                request.AimPoint = unit.Position + request.AimDirection;
            }
        }

        private static bool ShouldCancelForInvalidTarget(this SkillCastComponent self, Unit unit)
        {
            if (self.TargetUnitId == 0 || TimeInfo.Instance.ServerNow() >= self.CastPointTime)
            {
                return false;
            }

            if (!TargetSelectHelper.TryGetTarget(unit, self.TargetUnitId, out Unit target) || !TargetSelectHelper.IsValidCombatTarget(unit, target))
            {
                return true;
            }

            SkillComponent skillComponent = unit.GetComponent<SkillComponent>();
            if (skillComponent == null)
            {
                return false;
            }

            Skill skill = skillComponent.GetChild<Skill>(self.CurrentSkillId);
            if (skill == null)
            {
                return false;
            }

            float skillRange = EstimateSkillRange(skill);
            return skillRange < float.MaxValue && TargetSelectHelper.GetDistance(unit, target) > skillRange;
        }

        private static float EstimateSkillRange(Skill skill)
        {
            if (skill?.SkillConfig?.ActionEventIds == null)
            {
                return float.MaxValue;
            }

            float maxRange = 0f;
            bool hasRangeRule = false;
            for (int index = 0; index < skill.SkillConfig.ActionEventIds.Count; ++index)
            {
                ActionEventConfig actionEventConfig = ActionEventConfigCategory.Instance.GetOrDefault(skill.SkillConfig.ActionEventIds[index]);
                if (actionEventConfig == null)
                {
                    continue;
                }

                float range = EstimateActionEventRange(actionEventConfig);
                if (range <= 0f)
                {
                    continue;
                }

                hasRangeRule = true;
                maxRange = Math.Max(maxRange, range);
            }

            return hasRangeRule ? maxRange : float.MaxValue;
        }

        private static float EstimateActionEventRange(ActionEventConfig actionEventConfig)
        {
            if (actionEventConfig?.EventData == null)
            {
                return 0f;
            }

            switch (actionEventConfig.EventData)
            {
                case RangeDamageActionEventData rangeDamageActionEventData:
                {
                    return rangeDamageActionEventData.Radius > 0 ? rangeDamageActionEventData.Radius / 1000f : 0f;
                }
                case BulletActionEventData bulletActionEventData:
                {
                    float speed = bulletActionEventData.Speed > 0 ? bulletActionEventData.Speed / 1000f : 10f;
                    int lifeMs = bulletActionEventData.LifeMs > 0 ? bulletActionEventData.LifeMs : 1000;
                    return speed * lifeMs / 1000f;
                }
                default:
                    return 0f;
            }
        }
    }
}
