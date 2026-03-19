namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class CombatAnimStateComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public ECombatAnimState CurrentBaseState;
        public ECombatAnimState CurrentOverlayState;
        public bool IsInitialized;
    }
}
