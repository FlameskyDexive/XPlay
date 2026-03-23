using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(StateSyncRoomServerComponent))]
    [FriendOf(typeof(StateSyncRoomServerComponent))]
    public static partial class StateSyncRoomServerComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this StateSyncRoomServerComponent self)
        {
            
        }
        [EntitySystem]
        private static void Awake(this StateSyncRoomServerComponent self, List<long> playerIds)
        {
            foreach (long id in playerIds)
            {
                StateSyncRoomPlayer roomPlayer = self.AddChildWithId<StateSyncRoomPlayer>(id);
            }
        }
        
        [EntitySystem]
        private static void FixedUpdate(this StateSyncRoomServerComponent self)
        {
            self.UpdateRoomPlayer();
        }
        /// <summary>
        /// 暂定每帧同步角色位置/朝向信息
        /// </summary>
        /// <param name="self"></param>
        private static void UpdateRoomPlayer(this StateSyncRoomServerComponent self)
        {
            M2C_SyncUnitTransforms sync = M2C_SyncUnitTransforms.Create();

            foreach (StateSyncRoomPlayer roomPlayer in self.Children.Values)
            {
                if (roomPlayer.IsOnline || roomPlayer.IsRobot)
                {
                    Unit unit = roomPlayer.Unit;
                    if (unit == null)
                    {
                        continue;
                    }

                    if (!HasTransformChanged(roomPlayer, unit))
                    {
                        continue;
                    }

                    TransformInfo info = TransformInfo.Create();
                    info.UnitId = unit.Id;
                    info.Forward = unit.Forward;
                    info.Position = unit.Position;
                    sync.TransformInfos.Add(info);
                    roomPlayer.LastSyncedPosition = unit.Position;
                    roomPlayer.LastSyncedForward = unit.Forward;
                    roomPlayer.HasSyncedTransform = true;
                }
            }

            if (sync.TransformInfos.Count == 0)
            {
                sync.Dispose();
                return;
            }

            StateSyncRoomMessageHelper.BroadCast(self.GetParent<StateSyncRoom>(), sync);
        }

        private static bool HasTransformChanged(StateSyncRoomPlayer roomPlayer, Unit unit)
        {
            if (roomPlayer == null || unit == null || unit.IsDisposed)
            {
                return false;
            }

            if (!roomPlayer.HasSyncedTransform)
            {
                return true;
            }

            return math.lengthsq(unit.Position - roomPlayer.LastSyncedPosition) > 0.0001f
                    || math.lengthsq(unit.Forward - roomPlayer.LastSyncedForward) > 0.0001f;
        }

        public static bool IsAllPlayerProgress100(this StateSyncRoomServerComponent self)
        {
            foreach (StateSyncRoomPlayer roomPlayer in self.Children.Values)
            {
                if (roomPlayer.Progress != 100)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
