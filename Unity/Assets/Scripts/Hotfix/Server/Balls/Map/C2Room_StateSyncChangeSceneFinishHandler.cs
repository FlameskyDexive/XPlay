using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    [FriendOf(typeof (StateSyncRoomServerComponent))]
    [FriendOf(typeof(SkillComponent))]
    public class C2Room_StateSyncChangeSceneFinishHandler : MessageHandler<Scene, C2Room_StateSyncChangeSceneFinish>
    {
        protected override async ETTask Run(Scene root, C2Room_StateSyncChangeSceneFinish message)
        {
            StateSyncRoom room = root.GetComponent<StateSyncRoom>();
            if (room == null || room.IsDisposed || room.Status == RoomStatus.Playing)
            {
                return;
            }

            StateSyncRoomServerComponent roomServerComponent = room.GetComponent<StateSyncRoomServerComponent>();
            StateSyncRoomRobotManagerComponent roomRobotManagerComponent = room.GetComponent<StateSyncRoomRobotManagerComponent>();
            StateSyncRoomPlayer roomPlayer = roomServerComponent.GetChild<StateSyncRoomPlayer>(message.PlayerId);
            if (roomPlayer == null || roomPlayer.IsDisposed)
            {
                return;
            }

            roomPlayer.Progress = 100;
            
            if (!roomServerComponent.IsAllPlayerProgress100())
            {
                return;
            }

            room.Status = RoomStatus.Ready;
            EntityRef<Scene> rootRef = root;
            EntityRef<StateSyncRoom> roomRef = room;
            EntityRef<StateSyncRoomServerComponent> roomServerComponentRef = roomServerComponent;
            EntityRef<StateSyncRoomRobotManagerComponent> roomRobotManagerComponentRef = roomRobotManagerComponent;
            await room.Fiber.Root.GetComponent<TimerComponent>().WaitAsync(1000);

            root = rootRef;
            room = roomRef;
            roomServerComponent = roomServerComponentRef;
            roomRobotManagerComponent = roomRobotManagerComponentRef;
            if (root == null || room == null || room.IsDisposed || roomServerComponent == null || roomServerComponent.IsDisposed)
            {
                return;
            }

            if (room.Status == RoomStatus.Playing)
            {
                return;
            }

            Room2C_StateSyncStart room2CStart = Room2C_StateSyncStart.Create();
            room2CStart.StartTime = TimeInfo.Instance.ServerFrameTime();
            foreach (StateSyncRoomPlayer rp in roomServerComponent.Children.Values)
            {
                if (rp == null || rp.IsDisposed)
                {
                    continue;
                }

                float3 spawnPosition = roomRobotManagerComponent != null && roomRobotManagerComponent.IsRobotPlayer(rp.Id)
                        ? GetRobotSpawnPosition()
                        : new float3(RandomGenerator.RandomNumber(-3, 3), 0, RandomGenerator.RandomNumber(-3, 3));
                float3 spawnForward = new float3(0, 0, 1);

                UnitInfo unitInfo = roomRobotManagerComponent != null && roomRobotManagerComponent.IsRobotPlayer(rp.Id)
                        ? roomRobotManagerComponent.CreateRobotUnitInfo(root, rp, spawnPosition, spawnForward)
                        : CreatePlayerUnitInfo(root, rp, spawnPosition, spawnForward);

                if (unitInfo == null)
                {
                    continue;
                }
                room2CStart.UnitInfo.Add(unitInfo);
            }

            room.Status = RoomStatus.Playing;
            room.Init(room2CStart.UnitInfo, room2CStart.StartTime);

            StateSyncRoomMessageHelper.BroadCast(room, room2CStart);
        }

        private static UnitInfo CreatePlayerUnitInfo(Scene root, StateSyncRoomPlayer roomPlayer, float3 spawnPosition, float3 spawnForward)
        {
            Unit unit = roomPlayer.Unit;
            if (unit == null || unit.IsDisposed || unit.ConfigId != ConstValue.DefaultPlayerUnitConfigId)
            {
                unit?.Dispose();
                unit = UnitFactory.Create(root, roomPlayer.Id, EUnitType.Player);
                roomPlayer.Unit = unit;
            }

            unit.Position = spawnPosition;
            unit.Forward = spawnForward;
            roomPlayer.LastSyncedPosition = spawnPosition;
            roomPlayer.LastSyncedForward = spawnForward;
            roomPlayer.HasSyncedTransform = true;

            UnitInfo unitInfo = UnitHelper.CreateUnitInfo(unit);
            SkillComponent skillComponent = unit.GetComponent<SkillComponent>();
            if (skillComponent != null)
            {
                foreach (int skillId in skillComponent.IdSkillMap.Keys)
                {
                    unitInfo.SkillInfo[skillId] = 1;
                }
            }

            PlayerInfo playerInfo = PlayerInfo.Create();
            playerInfo.PlayerId = roomPlayer.Id;
            playerInfo.PlayerName = roomPlayer.Id.ToString();
            playerInfo.AvatarIndex = RandomGenerator.RandomNumber(0, 9);
            unitInfo.PlayerInfo = playerInfo;
            return unitInfo;
        }

        private static float3 GetRobotSpawnPosition()
        {
            int xSign = RandomGenerator.RandomNumber(0, 2) == 0 ? -1 : 1;
            int zSign = RandomGenerator.RandomNumber(0, 2) == 0 ? -1 : 1;
            float x = RandomGenerator.RandomNumber(ConstValue.StateSyncMatchRobotSpawnMinDistance, ConstValue.StateSyncMatchRobotSpawnMaxDistance + 1) * xSign;
            float z = RandomGenerator.RandomNumber(ConstValue.StateSyncMatchRobotSpawnMinDistance, ConstValue.StateSyncMatchRobotSpawnMaxDistance + 1) * zSign;
            return new float3(x, 0, z);
        }
    }
}
