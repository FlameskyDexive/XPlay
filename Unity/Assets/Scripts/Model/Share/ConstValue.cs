namespace ET
{
    public static partial class ConstValue
    {
        public const string RouterHttpHost = "127.0.0.1";
        public const int RouterHttpPort = 30300;
        public const int SessionTimeoutTime = 30 * 1000;
        public const int StateSyncMatchTimeoutTime = 10 * 1000;
        public const string StateSyncMatchRobotBehaviorTree = "ai_monster_01";
        public const int StateSyncMatchRobotUnitConfigId = 3001;
        public const int StateSyncMatchRobotSkillId = 1001;
        public const int StateSyncMatchRobotSpawnMinDistance = 8;
        public const int StateSyncMatchRobotSpawnMaxDistance = 12;

        /// <summary>
        /// 不同玩法匹配人数配表即可
        /// </summary>
        public const int StateSyncMatchCount = 2;

    }
}
