namespace ET
{
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(CombatStateComponent))]
    public static class BuffTagHelper
    {
        public static void ApplyGrantedTags(this Buff self)
        {
            long tagGrantMask = self.BuffConfig?.TagGrantMask ?? 0;
            if (tagGrantMask == 0)
            {
                return;
            }

            CombatStateComponent combatStateComponent = self.Unit?.GetComponent<CombatStateComponent>();
            combatStateComponent?.AddTag((ECombatTag)tagGrantMask);
        }

        public static void RefreshGrantedTags(this Buff self)
        {
            self.GetParent<BuffComponent>()?.RefreshGrantedTags();
        }

        public static void RefreshGrantedTags(this BuffComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            CombatStateComponent combatStateComponent = unit?.GetComponent<CombatStateComponent>();
            if (combatStateComponent == null)
            {
                return;
            }

            long preservedMask = combatStateComponent.TagMask & (long)ECombatTag.Dead;
            long grantMask = 0;
            foreach ((int _, EntityRef<Buff> buffRef) in self.BuffDic)
            {
                Buff buff = buffRef;
                if (buff == null || buff.IsDisposed)
                {
                    continue;
                }

                grantMask |= buff.BuffConfig?.TagGrantMask ?? 0;
            }

            combatStateComponent.TagMask = preservedMask | grantMask;
        }
    }
}
