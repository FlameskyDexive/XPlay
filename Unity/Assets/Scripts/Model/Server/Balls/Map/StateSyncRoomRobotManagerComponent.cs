using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(StateSyncRoom))]
    public class StateSyncRoomRobotManagerComponent : Entity, IAwake, IFixedUpdate, IDestroy
    {
        public HashSet<long> RobotPlayerIds { get; set; } = new();

        public Dictionary<long, int> RobotAvatarIndexMap { get; set; } = new();
    }
}
