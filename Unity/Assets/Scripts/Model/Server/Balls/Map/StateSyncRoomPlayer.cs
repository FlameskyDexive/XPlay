namespace ET.Server
{

    [ChildOf(typeof (StateSyncRoomServerComponent))]
    public class StateSyncRoomPlayer : Entity, IAwake
    {
        public int Progress { get; set; }

        public bool IsOnline { get; set; } = true;

        public bool IsRobot { get; set; }

        public bool IsCreator { get; set; }

        public bool IsReady { get; set; }

        public EntityRef<Unit> Unit { get; set; }
    }
}
