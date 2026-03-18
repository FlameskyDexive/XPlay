namespace ET
{
    [EntitySystemOf(typeof(ThreatComponent))]
    [FriendOf(typeof(ThreatComponent))]
    public static partial class ThreatComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ThreatComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ThreatComponent self)
        {
            self.ThreatMap.Clear();
            self.PrimaryTargetId = 0;
            self.LastThreatUpdateTime = 0;
        }

        public static void AddThreat(this ThreatComponent self, long targetUnitId, long threatValue)
        {
            if (targetUnitId == 0 || threatValue == 0)
            {
                return;
            }

            if (!self.ThreatMap.TryGetValue(targetUnitId, out long currentThreat))
            {
                currentThreat = 0;
            }

            currentThreat += threatValue;
            self.ThreatMap[targetUnitId] = currentThreat;
            self.LastThreatUpdateTime = TimeInfo.Instance.ServerNow();

            if (self.PrimaryTargetId == 0 || currentThreat >= self.GetThreat(self.PrimaryTargetId))
            {
                self.PrimaryTargetId = targetUnitId;
            }
        }

        public static long GetThreat(this ThreatComponent self, long targetUnitId)
        {
            if (!self.ThreatMap.TryGetValue(targetUnitId, out long threatValue))
            {
                return 0;
            }

            return threatValue;
        }

        public static void RemoveThreat(this ThreatComponent self, long targetUnitId)
        {
            if (!self.ThreatMap.Remove(targetUnitId))
            {
                return;
            }

            if (self.PrimaryTargetId == targetUnitId)
            {
                self.PrimaryTargetId = 0;
            }

            self.LastThreatUpdateTime = TimeInfo.Instance.ServerNow();
        }

        public static void ClearThreat(this ThreatComponent self)
        {
            self.ThreatMap.Clear();
            self.PrimaryTargetId = 0;
            self.LastThreatUpdateTime = TimeInfo.Instance.ServerNow();
        }
    }
}
