using System;
using System.Collections.Generic;

namespace ET.Server
{

    [FriendOf(typeof(MatchComponent))]
    public static partial class MatchComponentSystem
    {
        public static async ETTask Match(this MatchComponent self, long playerId)
        {
            if (self.waitMatchPlayers.Contains(playerId))
            {
                return;
            }
            
            self.waitMatchPlayers.Add(playerId);

            if (self.waitMatchPlayers.Count < LSConstValue.MatchCount)
            {
                return;
            }
            
            // 申请一个房间
            StartSceneConfig startSceneConfig = RandomGenerator.RandomArray(StartSceneConfigCategory.Instance.Maps);
            Match2Map_GetRoom match2MapGetRoom = Match2Map_GetRoom.Create();
            foreach (long id in self.waitMatchPlayers)
            {
                match2MapGetRoom.PlayerIds.Add(id);
            }
            
            self.waitMatchPlayers.Clear();

            Scene root = self.Root();
            Map2Match_GetRoom map2MatchGetRoom = await root.GetComponent<MessageSender>().Call(
                startSceneConfig.ActorId, match2MapGetRoom) as Map2Match_GetRoom;

            Match2G_NotifyMatchSuccess match2GNotifyMatchSuccess = Match2G_NotifyMatchSuccess.Create();
            match2GNotifyMatchSuccess.ActorId = map2MatchGetRoom.ActorId;
            MessageLocationSenderComponent messageLocationSenderComponent = root.GetComponent<MessageLocationSenderComponent>();
            
            foreach (long id in match2MapGetRoom.PlayerIds) // 这里发送消息线程不会修改PlayerInfo，所以可以直接使用
            {
                messageLocationSenderComponent.Get(LocationType.Player).Send(id, match2GNotifyMatchSuccess);
                // 等待进入房间的确认消息，如果超时要通知所有玩家退出房间，重新匹配
            }
        }
        public static async ETTask StateSyncMatch(this MatchComponent self, long playerId)
        {
            if (self.waitMatchStateSyncPlayers.Contains(playerId))
            {
                return;
            }

            self.waitMatchStateSyncPlayers.Add(playerId);

            if (self.waitMatchStateSyncPlayers.Count < ConstValue.StateSyncMatchCount)
            {
                self.WaitStateSyncMatchTimeout(playerId).Coroutine();
                return;
            }

            List<long> matchedPlayerIds = self.DequeueStateSyncPlayers(ConstValue.StateSyncMatchCount);
            await self.CreateStateSyncMatchRoom(matchedPlayerIds);
        }

        public static bool CancelStateSyncMatch(this MatchComponent self, long playerId)
        {
            return self.waitMatchStateSyncPlayers.Remove(playerId);
        }

        private static async ETTask WaitStateSyncMatchTimeout(this MatchComponent self, long playerId)
        {
            Scene root = self.Root();
            EntityRef<MatchComponent> selfRef = self;

            await root.GetComponent<TimerComponent>().WaitAsync(ConstValue.StateSyncMatchTimeoutTime);

            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            int index = self.waitMatchStateSyncPlayers.IndexOf(playerId);
            if (index < 0)
            {
                return;
            }

            self.waitMatchStateSyncPlayers.RemoveAt(index);

            List<long> matchedPlayerIds = new List<long>(1) { playerId };
            await self.CreateStateSyncMatchRoom(matchedPlayerIds);
        }

        private static List<long> DequeueStateSyncPlayers(this MatchComponent self, int matchCount)
        {
            int dequeueCount = Math.Min(matchCount, self.waitMatchStateSyncPlayers.Count);
            List<long> matchedPlayerIds = new List<long>(dequeueCount);
            for (int i = 0; i < dequeueCount; ++i)
            {
                matchedPlayerIds.Add(self.waitMatchStateSyncPlayers[i]);
            }

            self.waitMatchStateSyncPlayers.RemoveRange(0, dequeueCount);
            return matchedPlayerIds;
        }

        private static async ETTask CreateStateSyncMatchRoom(this MatchComponent self, List<long> playerIds)
        {
            if (playerIds == null || playerIds.Count == 0)
            {
                return;
            }

            Scene root = self.Root();

            StartSceneConfig startSceneConfig = RandomGenerator.RandomArray(StartSceneConfigCategory.Instance.Maps);
            Match2Map_StateSyncGetRoom match2MapGetRoom = Match2Map_StateSyncGetRoom.Create();
            foreach (long id in playerIds)
            {
                match2MapGetRoom.PlayerIds.Add(id);
            }

            Map2Match_StateSyncGetRoom map2MatchGetRoom = await root.GetComponent<MessageSender>().Call(
                startSceneConfig.ActorId, match2MapGetRoom) as Map2Match_StateSyncGetRoom;

            Match2G_StateSyncNotifyMatchSuccess match2GNotifyMatchSuccess = Match2G_StateSyncNotifyMatchSuccess.Create();
            match2GNotifyMatchSuccess.ActorId = map2MatchGetRoom.ActorId;
            MessageLocationSenderComponent messageLocationSenderComponent = root.GetComponent<MessageLocationSenderComponent>();

            foreach (long id in playerIds)
            {
                messageLocationSenderComponent.Get(LocationType.Player).Send(id, match2GNotifyMatchSuccess);
                // 等待进入房间的确认消息，如果超时要通知所有玩家退出房间，重新匹配
            }
        }
    }

}
