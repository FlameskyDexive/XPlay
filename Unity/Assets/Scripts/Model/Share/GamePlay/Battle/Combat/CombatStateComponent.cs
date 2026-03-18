namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class CombatStateComponent : Entity, IAwake, ITransfer, IDestroy
    {
        public ECombatState State;
        public ECombatSubState SubState;
        public long TagMask;
        public int StateVersion;
        public long StateEndTime;
        public int CurrentCastSkillId;
        public long CurrentTargetId;
        public long LastHitTime;
        public int InterruptLevel;
    }
}
