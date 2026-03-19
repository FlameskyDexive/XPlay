namespace ET
{
    [FriendOf(typeof(CombatStateComponent))]
    public static class BattleHelper 
    {
        public static void HitSettle(Unit from, Unit to, EHitFromType hitType = EHitFromType.Skill_Normal, Unit bullet = null)
        {
            switch (hitType)
            {
                case EHitFromType.Skill_Normal:
                case EHitFromType.Skill_Bullet:
                    ResolveDamage(from, to, hitType);
                    break;
            }
        }

        private static void ResolveDamage(Unit from, Unit to, EHitFromType hitType)
        {
            if (from == null || to == null || from.IsDisposed || to.IsDisposed || from.Id == to.Id)
            {
                return;
            }

            NumericComponent attackerNumeric = from.GetComponent<NumericComponent>();
            NumericComponent defenderNumeric = to.GetComponent<NumericComponent>();
            if (attackerNumeric == null || defenderNumeric == null)
            {
                return;
            }

            int currentHp = defenderNumeric.GetAsInt(NumericType.Hp);
            if (currentHp <= 0)
            {
                return;
            }

            int damage = attackerNumeric.GetAsInt(NumericType.Attack);
            if (damage < 0)
            {
                damage = 0;
            }

            int finalHp = currentHp - damage;
            if (finalHp < 0)
            {
                finalHp = 0;
            }

            defenderNumeric.Set(NumericType.Hp, finalHp);
            CombatStateComponent combatStateComponent = to.GetComponent<CombatStateComponent>();
            combatStateComponent?.MarkHit((int)EInterruptLevel.Soft);
            TryInterruptCast(to, combatStateComponent, EInterruptLevel.Soft);
            to.GetComponent<ThreatComponent>()?.AddThreat(from.Id, damage);

            if (finalHp <= 0)
            {
                to.GetComponent<SkillCastComponent>()?.InterruptCast();
                to.GetComponent<BuffComponent>()?.ClearAllBuffs();
                to.GetComponent<CombatStateComponent>()?.MarkDead();
            }

            Log.Info($"hit settle, from:{from.Id}, to:{to.Id}, value:{damage}, finalHp:{finalHp}, hitType:{hitType}");
            EventSystem.Instance.Publish(to.Root(), new HitResult { hitResultType = EHitResultType.Damage, value = damage });
        }

        public static void ApplyNumericDelta(Unit from, Unit to, int numericType, long delta, EInterruptLevel interruptLevel = EInterruptLevel.None)
        {
            if (to == null || to.IsDisposed || delta == 0)
            {
                return;
            }

            NumericComponent targetNumeric = to.GetComponent<NumericComponent>();
            if (targetNumeric == null)
            {
                return;
            }

            long oldValue = targetNumeric.GetAsLong(numericType);
            long newValue = oldValue + delta;
            if (numericType == NumericType.Hp)
            {
                long maxHp = targetNumeric.GetAsLong(NumericType.MaxHp);
                if (maxHp > 0)
                {
                    newValue = System.Math.Clamp(newValue, 0, maxHp);
                }
                else if (newValue < 0)
                {
                    newValue = 0;
                }
            }

            if (newValue == oldValue)
            {
                return;
            }

            targetNumeric.Set(numericType, newValue);
            if (numericType != NumericType.Hp)
            {
                return;
            }

            long changedValue = newValue - oldValue;
            if (changedValue < 0)
            {
                int damage = (int)(-changedValue);
                CombatStateComponent combatStateComponent = to.GetComponent<CombatStateComponent>();
                combatStateComponent?.MarkHit((int)interruptLevel);
                TryInterruptCast(to, combatStateComponent, interruptLevel);
                to.GetComponent<ThreatComponent>()?.AddThreat(from?.Id ?? 0, damage);

                if (newValue <= 0)
                {
                    to.GetComponent<SkillCastComponent>()?.InterruptCast();
                    to.GetComponent<BuffComponent>()?.ClearAllBuffs();
                    combatStateComponent?.MarkDead();
                }

                EventSystem.Instance.Publish(to.Root(), new HitResult { hitResultType = EHitResultType.Damage, value = damage });
                return;
            }

            EventSystem.Instance.Publish(to.Root(), new HitResult { hitResultType = EHitResultType.RecoverBlood, value = (int)changedValue });
        }

        private static void TryInterruptCast(Unit target, CombatStateComponent combatStateComponent, EInterruptLevel interruptLevel)
        {
            if (target == null || combatStateComponent == null || interruptLevel == EInterruptLevel.None)
            {
                return;
            }

            if (combatStateComponent.State != ECombatState.Casting)
            {
                return;
            }

            ECombatTag currentTags = (ECombatTag)combatStateComponent.TagMask;
            if (interruptLevel == EInterruptLevel.Soft && (currentTags & ECombatTag.SuperArmor) != 0)
            {
                return;
            }

            SkillCastComponent skillCastComponent = target.GetComponent<SkillCastComponent>();
            if (skillCastComponent == null || !skillCastComponent.IsCasting())
            {
                return;
            }

            skillCastComponent.InterruptCast();
        }
    }
}
