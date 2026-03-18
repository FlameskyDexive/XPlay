namespace ET
{
    [EntitySystemOf(typeof(TargetComponent))]
    [FriendOf(typeof(TargetComponent))]
    public static partial class TargetComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TargetComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TargetComponent self)
        {
            self.CurrentTargetId = 0;
            self.LastTargetId = 0;
            self.AssistTargetId = 0;
            self.LockTarget = false;
        }

        public static void SetTarget(this TargetComponent self, long targetUnitId, bool lockTarget = false)
        {
            self.LastTargetId = self.CurrentTargetId;
            self.CurrentTargetId = targetUnitId;
            self.LockTarget = lockTarget;

            Unit unit = self.GetParent<Unit>();
            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null)
            {
                combatStateComponent.SetCurrentTarget(targetUnitId);
            }
        }

        public static void ClearTarget(this TargetComponent self)
        {
            self.LastTargetId = self.CurrentTargetId;
            self.CurrentTargetId = 0;
            self.LockTarget = false;

            Unit unit = self.GetParent<Unit>();
            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null)
            {
                combatStateComponent.SetCurrentTarget(0);
            }
        }

        public static long GetCurrentTargetId(this TargetComponent self)
        {
            return self.CurrentTargetId;
        }
    }
}
