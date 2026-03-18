namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class TargetComponent : Entity, IAwake, ITransfer, IDestroy
    {
        public long CurrentTargetId;
        public long LastTargetId;
        public bool LockTarget;
        public long AssistTargetId;
    }
}
