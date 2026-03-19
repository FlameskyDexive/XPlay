using Unity.Mathematics;

namespace ET
{
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
    }
}
