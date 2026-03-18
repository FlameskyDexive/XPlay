using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class SkillCastComponent : Entity, IAwake, IFixedUpdate, ITransfer, IDestroy
    {
        public long CurrentSkillId;
        public int CurrentSkillConfigId;
        public int CurrentCastSeq;
        public long TargetUnitId;
        public float3 AimPoint;
        public float3 AimDirection;
        public long CastStartTime;
        public long CastPointTime;
        public long RecoverEndTime;
        public long NextGlobalCdEndTime;
        public SkillCastRequest QueuedRequest;
        public bool HasQueuedRequest;
    }
}
