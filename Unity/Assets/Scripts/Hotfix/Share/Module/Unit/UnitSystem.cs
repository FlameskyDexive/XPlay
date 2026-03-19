namespace ET
{
    [EntitySystemOf(typeof(Unit))]
    public static partial class UnitSystem
    {
        [EntitySystem]
        private static void Awake(this Unit self, int configId)
        {
            self.ConfigId = configId;
        }

        public static UnitConfig Config(this Unit self)
        {
            return UnitConfigCategory.Instance.Get(self.ConfigId);
        }
        
        public static EUnitType Type(this Unit self)
        {
            return self.Config().Type;
        }
    }

    [EntitySystemOf(typeof(MonsterSpawnRuntimeComponent))]
    [FriendOf(typeof(MonsterSpawnRuntimeComponent))]
    public static partial class MonsterSpawnRuntimeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MonsterSpawnRuntimeComponent self)
        {
            self.SpawnedConfigIds.Clear();
        }

        [EntitySystem]
        private static void Destroy(this MonsterSpawnRuntimeComponent self)
        {
            self.SpawnedConfigIds.Clear();
        }

        public static bool HasSpawned(this MonsterSpawnRuntimeComponent self, int spawnConfigId)
        {
            return self != null && self.SpawnedConfigIds.Contains(spawnConfigId);
        }

        public static void MarkSpawned(this MonsterSpawnRuntimeComponent self, int spawnConfigId)
        {
            self?.SpawnedConfigIds.Add(spawnConfigId);
        }
    }

    [EntitySystemOf(typeof(MonsterSpawnMarkerComponent))]
    [FriendOf(typeof(MonsterSpawnMarkerComponent))]
    public static partial class MonsterSpawnMarkerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MonsterSpawnMarkerComponent self, int spawnConfigId, int spawnIndex)
        {
            self.SpawnConfigId = spawnConfigId;
            self.SpawnIndex = spawnIndex;
        }

        [EntitySystem]
        private static void Destroy(this MonsterSpawnMarkerComponent self)
        {
            self.SpawnConfigId = 0;
            self.SpawnIndex = 0;
        }

        public static bool MatchesSpawnConfig(this MonsterSpawnMarkerComponent self, int spawnConfigId)
        {
            return self != null && self.SpawnConfigId == spawnConfigId;
        }
    }
}
