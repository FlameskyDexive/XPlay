using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(CombatStateComponent))]
    public static class BTCombatHelper
    {
        public static bool TryGetCombatUnit(this BTExecutionContext self, out Unit unit)
        {
            unit = null;
            if (!self.TryGetOwner(out unit) || unit == null || unit.IsDisposed)
            {
                return false;
            }

            return true;
        }

        public static void SyncCombatBlackboard(this BTExecutionContext self, Unit unit)
        {
            if (self?.Blackboard == null || unit == null || unit.IsDisposed)
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long hp = numericComponent?.GetAsLong(NumericType.Hp) ?? 0;
            long maxHp = numericComponent?.GetAsLong(NumericType.MaxHp) ?? 0;
            bool isDead = maxHp <= 0 ? hp <= 0 : hp <= 0;

            self.Blackboard.Set(BTCombatBlackboardKeys.IsDead, isDead);
            self.Blackboard.Set(BTCombatBlackboardKeys.HpRatio, GetHpRatio(unit));

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
            self.Blackboard.Set(BTCombatBlackboardKeys.InCast, skillCastComponent != null && skillCastComponent.IsCasting());
            self.Blackboard.Set(BTCombatBlackboardKeys.InControl, combatStateComponent != null && combatStateComponent.IsInControl());

            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            long targetId = targetComponent != null ? targetComponent.GetCurrentTargetId() : 0;
            if (TargetSelectHelper.TryGetTarget(unit, targetId, out Unit target) && TargetSelectHelper.IsValidCombatTarget(unit, target))
            {
                self.Blackboard.Set(BTCombatBlackboardKeys.TargetId, target.Id);
                self.Blackboard.Set(BTCombatBlackboardKeys.TargetDistance, TargetSelectHelper.GetDistance(unit, target));
                self.Blackboard.Set(BTCombatBlackboardKeys.HasTarget, true);
            }
            else
            {
                self.Blackboard.Remove(BTCombatBlackboardKeys.TargetId);
                self.Blackboard.Remove(BTCombatBlackboardKeys.TargetDistance);
                self.Blackboard.Set(BTCombatBlackboardKeys.HasTarget, false);
            }
        }

        public static void SetCombatTarget(this BTExecutionContext self, Unit unit, Unit target)
        {
            if (self?.Blackboard == null || unit == null || target == null)
            {
                return;
            }

            unit.GetComponent<TargetComponent>()?.SetTarget(target.Id);
            self.Blackboard.Set(BTCombatBlackboardKeys.TargetId, target.Id);
            self.Blackboard.Set(BTCombatBlackboardKeys.TargetDistance, TargetSelectHelper.GetDistance(unit, target));
            self.Blackboard.Set(BTCombatBlackboardKeys.HasTarget, true);
        }

        public static void ClearCombatTarget(this BTExecutionContext self, Unit unit)
        {
            if (self?.Blackboard == null)
            {
                return;
            }

            unit?.GetComponent<TargetComponent>()?.ClearTarget();
            self.Blackboard.Remove(BTCombatBlackboardKeys.TargetId);
            self.Blackboard.Remove(BTCombatBlackboardKeys.TargetDistance);
            self.Blackboard.Set(BTCombatBlackboardKeys.HasTarget, false);
        }

        public static bool TryResolveCombatTarget(this BTExecutionContext self, Unit unit, out Unit target)
        {
            target = null;
            if (self?.Blackboard == null || unit == null || unit.IsDisposed)
            {
                return false;
            }

            long targetId = self.Blackboard.Get<long>(BTCombatBlackboardKeys.TargetId, 0);
            if (targetId == 0)
            {
                TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
                if (targetComponent != null)
                {
                    targetId = targetComponent.GetCurrentTargetId();
                }
            }

            if (!TargetSelectHelper.TryGetTarget(unit, targetId, out target))
            {
                return false;
            }

            return true;
        }

        public static bool TryResolveValidCombatTarget(this BTExecutionContext self, Unit unit, out Unit target, float maxRange = float.MaxValue)
        {
            target = null;
            if (!self.TryResolveCombatTarget(unit, out target))
            {
                self.ClearCombatTarget(unit);
                return false;
            }

            if (!TargetSelectHelper.IsValidCombatTarget(unit, target, maxRange))
            {
                self.ClearCombatTarget(unit);
                return false;
            }

            self.SetCombatTarget(unit, target);
            return true;
        }

        public static void FaceTarget(Unit unit, Unit target)
        {
            if (unit == null || target == null || unit.IsDisposed || target.IsDisposed)
            {
                return;
            }

            float3 direction = target.Position - unit.Position;
            if (math.lengthsq(direction) <= 0.0001f)
            {
                return;
            }

            unit.Forward = math.normalize(direction);
        }

        public static bool TrySelectSkill(this BTExecutionContext self, Unit unit, int preferredSlot, out Skill skill, out int slot)
        {
            skill = null;
            slot = -1;
            if (self?.Blackboard == null || unit == null || unit.IsDisposed)
            {
                return false;
            }

            SkillComponent skillComponent = unit.GetComponent<SkillComponent>();
            SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
            if (skillComponent == null || skillCastComponent == null)
            {
                return false;
            }

            if (preferredSlot >= 0 && skillComponent.TryGetSkill(ESkillAbstractType.ActiveSkill, preferredSlot, out skill))
            {
                slot = preferredSlot;
                return CacheSelectedSkill(self, unit, skill, slot, skillCastComponent);
            }

            for (int index = 0; index < 8; ++index)
            {
                if (!skillComponent.TryGetSkill(ESkillAbstractType.ActiveSkill, index, out skill))
                {
                    break;
                }

                slot = index;
                if (CacheSelectedSkill(self, unit, skill, slot, skillCastComponent))
                {
                    return true;
                }
            }

            for (int index = 0; index < 8; ++index)
            {
                if (!skillComponent.TryGetSkill(ESkillAbstractType.NormalAttack, index, out skill))
                {
                    break;
                }

                slot = index;
                if (CacheSelectedSkill(self, unit, skill, slot, skillCastComponent))
                {
                    return true;
                }
            }

            self.ClearSelectedSkill();
            return false;
        }

        public static bool TryGetSelectedSkill(this BTExecutionContext self, Unit unit, out Skill skill, out int slot)
        {
            skill = null;
            slot = self?.Blackboard?.Get<int>(BTCombatBlackboardKeys.SelectedSkillSlot, -1) ?? -1;
            if (self?.Blackboard == null || unit == null || unit.IsDisposed)
            {
                return false;
            }

            int skillId = self.Blackboard.Get<int>(BTCombatBlackboardKeys.SelectedSkillId, 0);
            if (skillId == 0)
            {
                return false;
            }

            return unit.GetComponent<SkillComponent>()?.TryGetSkill(skillId, out skill) == true && skill != null;
        }

        public static void ClearSelectedSkill(this BTExecutionContext self)
        {
            if (self?.Blackboard == null)
            {
                return;
            }

            self.Blackboard.Remove(BTCombatBlackboardKeys.SelectedSkillId);
            self.Blackboard.Remove(BTCombatBlackboardKeys.SelectedSkillSlot);
            self.Blackboard.Set(BTCombatBlackboardKeys.CanCast, false);
        }

        public static bool CanCastSelectedSkill(this BTExecutionContext self, Unit unit, out ESkillCastResult result)
        {
            result = ESkillCastResult.SkillNotFound;
            if (!self.TryGetSelectedSkill(unit, out Skill skill, out _))
            {
                self.Blackboard?.Set(BTCombatBlackboardKeys.CanCast, false);
                return false;
            }

            SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
            if (skillCastComponent == null)
            {
                self.Blackboard?.Set(BTCombatBlackboardKeys.CanCast, false);
                result = ESkillCastResult.InvalidState;
                return false;
            }

            result = skillCastComponent.ValidateCast(skill);
            self.Blackboard?.Set(BTCombatBlackboardKeys.CanCast, result == ESkillCastResult.Success);
            return result == ESkillCastResult.Success;
        }

        public static bool TryBuildStateChangeRequest(this BTExecutionContext self, Unit unit, ECombatSubState targetSubState,
            out CombatStateChangeRequest request, out ECombatStateChangeResult result)
        {
            request = new CombatStateChangeRequest
            {
                TargetSubState = targetSubState,
            };
            result = ECombatStateChangeResult.InvalidState;

            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent == null)
            {
                return false;
            }

            if (combatStateComponent.State == ECombatState.Dead || combatStateComponent.HasAnyTag(ECombatTag.Dead))
            {
                result = ECombatStateChangeResult.Dead;
                return targetSubState == ECombatSubState.Dead;
            }

            switch (targetSubState)
            {
                case ECombatSubState.Idle:
                {
                    if (combatStateComponent.State == ECombatState.Casting)
                    {
                        return false;
                    }

                    result = combatStateComponent.HasAnyTag(GetStateSwitchBlockTags())
                        ? ECombatStateChangeResult.BlockedByTag
                        : ECombatStateChangeResult.Success;
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.Move:
                {
                    if (combatStateComponent.State == ECombatState.Casting)
                    {
                        return false;
                    }

                    if (combatStateComponent.HasAnyTag(GetStateSwitchBlockTags()))
                    {
                        result = ECombatStateChangeResult.BlockedByTag;
                        return false;
                    }

                    result = unit.GetComponent<PlayerMoveComponent>() != null ? ECombatStateChangeResult.Success : ECombatStateChangeResult.InvalidState;
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.CastPoint:
                {
                    if (!self.TryGetSelectedSkill(unit, out Skill skill, out _))
                    {
                        result = ECombatStateChangeResult.SkillNotFound;
                        return false;
                    }

                    request.SkillId = skill.SkillConfig.Id;
                    request.TargetUnitId = self.Blackboard?.Get<long>(BTCombatBlackboardKeys.TargetId, 0) ?? 0;
                    SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
                    if (skillCastComponent == null)
                    {
                        return false;
                    }

                    SkillCastRequest skillCastRequest = new SkillCastRequest
                    {
                        SkillId = skill.SkillConfig.Id,
                        TargetUnitId = request.TargetUnitId,
                        AimPoint = unit.Position,
                        AimDirection = unit.Forward,
                        PressedTime = TimeInfo.Instance.ServerNow(),
                    };

                    result = MapCastResult(skillCastComponent.ValidateCast(skill, skillCastRequest));
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.ActiveWindow:
                case ECombatSubState.Recover:
                {
                    request.SkillId = combatStateComponent.CurrentCastSkillId;
                    request.TargetUnitId = combatStateComponent.CurrentTargetId;
                    result = combatStateComponent.State == ECombatState.Casting
                        ? ECombatStateChangeResult.Success
                        : ECombatStateChangeResult.InvalidState;
                    return result == ECombatStateChangeResult.Success;
                }
                case ECombatSubState.Dead:
                {
                    NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                    result = numericComponent != null && numericComponent.GetAsLong(NumericType.Hp) <= 0
                        ? ECombatStateChangeResult.Success
                        : ECombatStateChangeResult.InvalidState;
                    return result == ECombatStateChangeResult.Success;
                }
                default:
                    return false;
            }
        }

        public static bool TryStartMove(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            PlayerMoveComponent playerMoveComponent = unit.GetComponent<PlayerMoveComponent>();
            if (playerMoveComponent == null)
            {
                return false;
            }

            playerMoveComponent.StartMove();
            return true;
        }

        public static void StopMove(Unit unit)
        {
            unit?.GetComponent<PlayerMoveComponent>()?.StopMove();
        }

        public static float GetHpRatio(Unit unit)
        {
            NumericComponent numericComponent = unit?.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return 0f;
            }

            long maxHp = numericComponent.GetAsLong(NumericType.MaxHp);
            if (maxHp <= 0)
            {
                return 0f;
            }

            return math.clamp((float)numericComponent.GetAsLong(NumericType.Hp) / maxHp, 0f, 1f);
        }

        private static bool CacheSelectedSkill(BTExecutionContext self, Unit unit, Skill skill, int slot, SkillCastComponent skillCastComponent)
        {
            if (skill == null)
            {
                return false;
            }

            self.Blackboard.Set(BTCombatBlackboardKeys.SelectedSkillId, skill.SkillConfig.Id);
            self.Blackboard.Set(BTCombatBlackboardKeys.SelectedSkillSlot, slot);
            bool canCast = skillCastComponent.ValidateCast(skill) == ESkillCastResult.Success;
            self.Blackboard.Set(BTCombatBlackboardKeys.CanCast, canCast);
            return canCast;
        }

        private static ECombatTag GetStateSwitchBlockTags()
        {
            return ECombatTag.HardControl | ECombatTag.Frozen | ECombatTag.Stiff | ECombatTag.Airborne;
        }

        private static ECombatStateChangeResult MapCastResult(ESkillCastResult result)
        {
            return result switch
            {
                ESkillCastResult.Success => ECombatStateChangeResult.Success,
                ESkillCastResult.SkillNotFound => ECombatStateChangeResult.SkillNotFound,
                ESkillCastResult.NoTarget => ECombatStateChangeResult.NoTarget,
                ESkillCastResult.InCd => ECombatStateChangeResult.InCd,
                ESkillCastResult.Dead => ECombatStateChangeResult.Dead,
                ESkillCastResult.Controlled => ECombatStateChangeResult.Controlled,
                ESkillCastResult.BlockedByTag => ECombatStateChangeResult.BlockedByTag,
                ESkillCastResult.InsufficientMp => ECombatStateChangeResult.InsufficientMp,
                _ => ECombatStateChangeResult.InvalidState,
            };
        }
    }
}
