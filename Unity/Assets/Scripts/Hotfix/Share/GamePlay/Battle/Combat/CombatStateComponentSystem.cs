namespace ET
{
    [EntitySystemOf(typeof(CombatStateComponent))]
    [FriendOf(typeof(CombatStateComponent))]
    public static partial class CombatStateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CombatStateComponent self)
        {
            self.SetIdle(TimeInfo.Instance.ServerNow());
        }

        [EntitySystem]
        private static void Destroy(this CombatStateComponent self)
        {
            self.TagMask = 0;
            self.CurrentCastSkillId = 0;
            self.CurrentTargetId = 0;
        }

        public static void SetIdle(this CombatStateComponent self, long now = 0)
        {
            if (now == 0)
            {
                now = TimeInfo.Instance.ServerNow();
            }

            self.State = ECombatState.Idle;
            self.SubState = ECombatSubState.Idle;
            self.StateEndTime = now;
            self.CurrentCastSkillId = 0;
            self.InterruptLevel = 0;
            self.TagMask &= ~(long)ECombatTag.Dead;
            ++self.StateVersion;
        }

        public static void BeginCast(this CombatStateComponent self, int skillConfigId, long targetUnitId, long castPointTime, long recoverEndTime)
        {
            long now = TimeInfo.Instance.ServerNow();
            self.State = ECombatState.Casting;
            self.SubState = castPointTime > now ? ECombatSubState.CastPoint : ECombatSubState.ActiveWindow;
            self.StateEndTime = recoverEndTime;
            self.CurrentCastSkillId = skillConfigId;
            self.SetCurrentTarget(targetUnitId);
            ++self.StateVersion;
        }

        public static void EnterActiveWindow(this CombatStateComponent self, long recoverEndTime)
        {
            self.State = ECombatState.Casting;
            self.SubState = ECombatSubState.ActiveWindow;
            self.StateEndTime = recoverEndTime;
            ++self.StateVersion;
        }

        public static void EnterRecover(this CombatStateComponent self, long recoverEndTime)
        {
            self.State = ECombatState.Casting;
            self.SubState = ECombatSubState.Recover;
            self.StateEndTime = recoverEndTime;
            ++self.StateVersion;
        }

        public static void FinishCast(this CombatStateComponent self, long now = 0)
        {
            self.SetIdle(now);
        }

        public static void MarkHit(this CombatStateComponent self, int interruptLevel = 0)
        {
            self.LastHitTime = TimeInfo.Instance.ServerNow();
            self.InterruptLevel = interruptLevel;
        }

        public static void SetCurrentTarget(this CombatStateComponent self, long targetUnitId)
        {
            self.CurrentTargetId = targetUnitId;
        }

        public static void MarkDead(this CombatStateComponent self)
        {
            self.State = ECombatState.Dead;
            self.SubState = ECombatSubState.Dead;
            self.StateEndTime = TimeInfo.Instance.ServerNow();
            self.CurrentCastSkillId = 0;
            self.AddTag(ECombatTag.Dead);
            ++self.StateVersion;
        }

        public static bool HasAnyTag(this CombatStateComponent self, ECombatTag tags)
        {
            return self != null && (((ECombatTag)self.TagMask) & tags) != 0;
        }

        public static void AddTag(this CombatStateComponent self, ECombatTag tags)
        {
            if (self == null)
            {
                return;
            }

            self.TagMask |= (long)tags;
        }

        public static void RemoveTag(this CombatStateComponent self, ECombatTag tags)
        {
            if (self == null)
            {
                return;
            }

            self.TagMask &= ~(long)tags;
        }

        public static bool IsInControl(this CombatStateComponent self)
        {
            if (self == null)
            {
                return false;
            }

            return self.State == ECombatState.Dead || self.HasAnyTag(ECombatTag.SoftControl | ECombatTag.HardControl);
        }

        public static bool CanCast(this CombatStateComponent self)
        {
            if (self == null)
            {
                return true;
            }

            if (self.State == ECombatState.Dead)
            {
                return false;
            }

            return !self.HasAnyTag(ECombatTag.Silence | ECombatTag.HardControl);
        }
    }
}
