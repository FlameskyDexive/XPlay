namespace ET.Client
{
    [EntitySystemOf(typeof(CombatAnimStateComponent))]
    [FriendOf(typeof(CombatAnimStateComponent))]
    [FriendOf(typeof(CombatStateComponent))]
    public static partial class CombatAnimStateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CombatAnimStateComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this CombatAnimStateComponent self)
        {
            self.CurrentBaseState = ECombatAnimState.None;
            self.CurrentOverlayState = ECombatAnimState.None;
            self.IsInitialized = false;
        }

        [EntitySystem]
        private static void Update(this CombatAnimStateComponent self)
        {
            self.GetParent<Unit>().GetComponent<CombatAnimancerComponent>()?.TickOverlay();
        }

        public static void InitializeFromLogicState(this CombatAnimStateComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent == null)
            {
                return;
            }

            self.ApplyCombatState(combatStateComponent.State, combatStateComponent.SubState, true);
        }

        public static void ApplyCombatState(this CombatAnimStateComponent self, ECombatState state, ECombatSubState subState, bool forceReplay = false)
        {
            CombatAnimancerComponent combatAnimancerComponent = self.GetParent<Unit>().GetComponent<CombatAnimancerComponent>();
            if (combatAnimancerComponent == null || !combatAnimancerComponent.IsReady())
            {
                return;
            }

            ECombatAnimState targetAnimState = ToAnimState(state, subState);
            if (targetAnimState == ECombatAnimState.Dead)
            {
                combatAnimancerComponent.StopOverlay();
                combatAnimancerComponent.PlayOverlayState(ECombatAnimState.Dead, true);
                self.CurrentBaseState = ECombatAnimState.Dead;
                self.CurrentOverlayState = ECombatAnimState.Dead;
                self.IsInitialized = true;
                return;
            }

            if (self.CurrentOverlayState == ECombatAnimState.Dead)
            {
                self.CurrentOverlayState = ECombatAnimState.None;
            }

            if (!self.IsInitialized || self.CurrentBaseState != targetAnimState || forceReplay)
            {
                combatAnimancerComponent.PlayBaseState(targetAnimState, forceReplay);
                self.CurrentBaseState = targetAnimState;
                self.IsInitialized = true;
            }
        }

        public static void PlayHit(this CombatAnimStateComponent self)
        {
            if (self.CurrentBaseState == ECombatAnimState.Dead)
            {
                return;
            }

            CombatAnimancerComponent combatAnimancerComponent = self.GetParent<Unit>().GetComponent<CombatAnimancerComponent>();
            if (combatAnimancerComponent == null || !combatAnimancerComponent.IsReady())
            {
                return;
            }

            self.CurrentOverlayState = ECombatAnimState.Hit;
            combatAnimancerComponent.PlayOverlayState(ECombatAnimState.Hit);
        }

        private static ECombatAnimState ToAnimState(ECombatState state, ECombatSubState subState)
        {
            if (state == ECombatState.Dead || subState == ECombatSubState.Dead)
            {
                return ECombatAnimState.Dead;
            }

            if (state == ECombatState.Casting)
            {
                return subState == ECombatSubState.CastPoint ? ECombatAnimState.CastPoint : ECombatAnimState.CastActive;
            }

            return subState == ECombatSubState.Move ? ECombatAnimState.Move : ECombatAnimState.Idle;
        }
    }
}
