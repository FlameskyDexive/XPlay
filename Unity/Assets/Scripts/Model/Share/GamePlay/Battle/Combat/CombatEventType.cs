namespace ET
{
    public struct CombatStateChanged
    {
        public Unit Unit;
        public ECombatState OldState;
        public ECombatSubState OldSubState;
        public ECombatState NewState;
        public ECombatSubState NewSubState;
        public int CurrentCastSkillId;
        public int StateVersion;
    }

    public struct CombatHit
    {
        public Unit Unit;
        public int InterruptLevel;
        public long HitTime;
    }
}
