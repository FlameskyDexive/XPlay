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

            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;

            self.State = ECombatState.Idle;
            self.SubState = ECombatSubState.Idle;
            self.StateEndTime = now;
            self.CurrentCastSkillId = 0;
            self.InterruptLevel = 0;
            self.TagMask &= ~(long)ECombatTag.Dead;
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
        }

        public static bool SetMoving(this CombatStateComponent self, long now = 0)
        {
            if (self == null || self.State == ECombatState.Dead || self.State == ECombatState.Casting)
            {
                return false;
            }

            if (now == 0)
            {
                now = TimeInfo.Instance.ServerNow();
            }

            if (self.State == ECombatState.Idle && self.SubState == ECombatSubState.Move)
            {
                return false;
            }

            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Idle;
            self.SubState = ECombatSubState.Move;
            self.StateEndTime = now;
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
            return true;
        }

        public static bool TrySetState(this CombatStateComponent self, ECombatSubState targetSubState, long now = 0)
        {
            return self.TrySetState(new CombatStateChangeRequest { TargetSubState = targetSubState }, out _, now);
        }

        public static bool TrySetState(this CombatStateComponent self, CombatStateChangeRequest request, out ECombatStateChangeResult result, long now = 0)
        {
            if (self == null)
            {
                result = ECombatStateChangeResult.InvalidState;
                return false;
            }

            if (now == 0)
            {
                now = TimeInfo.Instance.ServerNow();
            }

            switch (request.TargetSubState)
            {
                case ECombatSubState.Idle:
                {
                    if (self.State == ECombatState.Idle && self.SubState == ECombatSubState.Idle)
                    {
                        result = ECombatStateChangeResult.Success;
                        return true;
                    }

                    self.SetIdle(now);
                    result = ECombatStateChangeResult.Success;
                    return true;
                }
                case ECombatSubState.Move:
                {
                    if (self.State == ECombatState.Idle && self.SubState == ECombatSubState.Move)
                    {
                        result = ECombatStateChangeResult.Success;
                        return true;
                    }

                    result = self.SetMoving(now) ? ECombatStateChangeResult.Success : ECombatStateChangeResult.InvalidState;
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.CastPoint:
                case ECombatSubState.ActiveWindow:
                case ECombatSubState.Recover:
                {
                    result = self.TrySetCastingSubState(request.TargetSubState, request.SkillId, request.TargetUnitId, now)
                        ? ECombatStateChangeResult.Success
                        : ECombatStateChangeResult.InvalidState;
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.Dead:
                {
                    if (self.State == ECombatState.Dead && self.SubState == ECombatSubState.Dead)
                    {
                        result = ECombatStateChangeResult.Success;
                        return true;
                    }

                    self.MarkDead();
                    result = ECombatStateChangeResult.Success;
                    return true;
                }
                default:
                    result = ECombatStateChangeResult.InvalidState;
                    return false;
            }
        }

        public static bool TryRestoreIdleFromMove(this CombatStateComponent self, long now = 0)
        {
            if (self == null || self.State == ECombatState.Dead || self.State == ECombatState.Casting)
            {
                return false;
            }

            if (self.State != ECombatState.Idle || self.SubState != ECombatSubState.Move)
            {
                return false;
            }

            self.SetIdle(now);
            return true;
        }

        public static void BeginCast(this CombatStateComponent self, int skillConfigId, long targetUnitId, long castPointTime, long recoverEndTime)
        {
            long now = TimeInfo.Instance.ServerNow();
            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Casting;
            self.SubState = castPointTime > now ? ECombatSubState.CastPoint : ECombatSubState.ActiveWindow;
            self.StateEndTime = recoverEndTime;
            self.CurrentCastSkillId = skillConfigId;
            self.SetCurrentTarget(targetUnitId);
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
        }

        public static void EnterActiveWindow(this CombatStateComponent self, long recoverEndTime)
        {
            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Casting;
            self.SubState = ECombatSubState.ActiveWindow;
            self.StateEndTime = recoverEndTime;
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
        }

        public static void EnterRecover(this CombatStateComponent self, long recoverEndTime)
        {
            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Casting;
            self.SubState = ECombatSubState.Recover;
            self.StateEndTime = recoverEndTime;
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
        }

        public static void FinishCast(this CombatStateComponent self, long now = 0)
        {
            self.SetIdle(now);
        }

        public static void MarkHit(this CombatStateComponent self, int interruptLevel = 0)
        {
            self.LastHitTime = TimeInfo.Instance.ServerNow();
            self.InterruptLevel = interruptLevel;
            EventSystem.Instance.Publish(self.Scene(), new CombatHit
            {
                Unit = self.GetParent<Unit>(),
                InterruptLevel = interruptLevel,
                HitTime = self.LastHitTime,
            });
        }

        public static void SetCurrentTarget(this CombatStateComponent self, long targetUnitId)
        {
            self.CurrentTargetId = targetUnitId;
        }

        public static void MarkDead(this CombatStateComponent self)
        {
            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Dead;
            self.SubState = ECombatSubState.Dead;
            self.StateEndTime = TimeInfo.Instance.ServerNow();
            self.CurrentCastSkillId = 0;
            self.AddTag(ECombatTag.Dead);
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
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

        private static bool TrySetCastingSubState(this CombatStateComponent self, ECombatSubState targetSubState, int skillId, long targetUnitId, long now)
        {
            if (self == null || self.State == ECombatState.Dead)
            {
                return false;
            }

            if (self.State == ECombatState.Casting && self.SubState == targetSubState)
            {
                return true;
            }

            ECombatState oldState = self.State;
            ECombatSubState oldSubState = self.SubState;
            self.State = ECombatState.Casting;
            self.SubState = targetSubState;
            if (skillId > 0)
            {
                self.CurrentCastSkillId = skillId;
            }

            if (targetUnitId != 0)
            {
                self.CurrentTargetId = targetUnitId;
            }

            self.StateEndTime = self.StateEndTime > now ? self.StateEndTime : now;
            ++self.StateVersion;
            self.PublishStateChanged(oldState, oldSubState);
            return true;
        }

        private static void PublishStateChanged(this CombatStateComponent self, ECombatState oldState, ECombatSubState oldSubState)
        {
            if (self == null || (oldState == self.State && oldSubState == self.SubState))
            {
                return;
            }

            EventSystem.Instance.Publish(self.Scene(), new CombatStateChanged
            {
                Unit = self.GetParent<Unit>(),
                OldState = oldState,
                OldSubState = oldSubState,
                NewState = self.State,
                NewSubState = self.SubState,
                CurrentCastSkillId = self.CurrentCastSkillId,
                StateVersion = self.StateVersion,
            });
        }
    }
}
