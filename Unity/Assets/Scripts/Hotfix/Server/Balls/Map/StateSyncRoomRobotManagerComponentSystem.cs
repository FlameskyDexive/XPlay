using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(StateSyncRoomRobotManagerComponent))]
    [FriendOf(typeof(StateSyncRoomRobotManagerComponent))]
    [FriendOf(typeof(SkillComponent))]
    public static partial class StateSyncRoomRobotManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StateSyncRoomRobotManagerComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this StateSyncRoomRobotManagerComponent self)
        {
            self.RobotPlayerIds.Clear();
            self.RobotAvatarIndexMap.Clear();
        }

        public static void CreateMatchRobots(this StateSyncRoomRobotManagerComponent self, int robotCount)
        {
            if (robotCount <= 0)
            {
                return;
            }

            StateSyncRoom room = self.GetParent<StateSyncRoom>();
            StateSyncRoomServerComponent roomServerComponent = room.GetComponent<StateSyncRoomServerComponent>();
            if (roomServerComponent == null || roomServerComponent.IsDisposed)
            {
                return;
            }

            for (int i = 0; i < robotCount; ++i)
            {
                long robotPlayerId = IdGenerater.Instance.GenerateId();
                StateSyncRoomPlayer roomPlayer = roomServerComponent.AddChildWithId<StateSyncRoomPlayer>(robotPlayerId);
                roomPlayer.IsRobot = true;
                roomPlayer.IsOnline = false;
                roomPlayer.Progress = 100;
                roomPlayer.IsReady = true;

                self.RobotPlayerIds.Add(robotPlayerId);
                self.RobotAvatarIndexMap[robotPlayerId] = RandomGenerator.RandomNumber(0, 9);
            }
        }

        public static bool IsRobotPlayer(this StateSyncRoomRobotManagerComponent self, long playerId)
        {
            return self.RobotPlayerIds.Contains(playerId);
        }

        public static UnitInfo CreateRobotUnitInfo(this StateSyncRoomRobotManagerComponent self,
        Scene root, StateSyncRoomPlayer roomPlayer, float3 spawnPosition, float3 spawnForward)
        {
            if (root == null || roomPlayer == null || roomPlayer.IsDisposed || !self.IsRobotPlayer(roomPlayer.Id))
            {
                return null;
            }

            Unit unit = roomPlayer.Unit;
            if (unit == null || unit.IsDisposed)
            {
                unit = UnitFactory.CreateMonster(root,
                    roomPlayer.Id,
                    ConstValue.StateSyncMatchRobotUnitConfigId,
                    new[] { ConstValue.StateSyncMatchRobotSkillId },
                    ConstValue.StateSyncMatchRobotBehaviorTree);
                roomPlayer.Unit = unit;
                Log.Info($"[MatchRobot] create unit:{roomPlayer.Id} tree:{ConstValue.StateSyncMatchRobotBehaviorTree} config:{ConstValue.StateSyncMatchRobotUnitConfigId}");
            }

            unit.Position = spawnPosition;
            unit.Forward = spawnForward;
            Log.Info($"[MatchRobot] spawn unit:{roomPlayer.Id} position:{spawnPosition} forward:{spawnForward}");

            UnitInfo unitInfo = UnitHelper.CreateUnitInfo(unit);
            self.FillSkillInfo(unitInfo, unit.GetComponent<SkillComponent>());

            PlayerInfo playerInfo = PlayerInfo.Create();
            playerInfo.PlayerId = roomPlayer.Id;
            playerInfo.PlayerName = unit.Config().Name;
            playerInfo.AvatarIndex = self.RobotAvatarIndexMap.TryGetValue(roomPlayer.Id, out int avatarIndex) ? avatarIndex : 0;
            unitInfo.PlayerInfo = playerInfo;
            return unitInfo;
        }

        private static void FillSkillInfo(this StateSyncRoomRobotManagerComponent self, UnitInfo unitInfo, SkillComponent skillComponent)
        {
            if (unitInfo == null || skillComponent == null)
            {
                return;
            }

            foreach (int skillId in skillComponent.IdSkillMap.Keys)
            {
                unitInfo.SkillInfo[skillId] = 1;
            }
        }
    }
}
