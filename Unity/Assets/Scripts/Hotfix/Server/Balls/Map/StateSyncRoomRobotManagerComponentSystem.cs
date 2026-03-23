using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(StateSyncRoomRobotManagerComponent))]
    [FriendOf(typeof(StateSyncRoomRobotManagerComponent))]
    [FriendOf(typeof(BTComponent))]
    [FriendOf(typeof(SkillComponent))]
    public static partial class StateSyncRoomRobotManagerComponentSystem
    {
        private const float CombatMoveRange = 2.5f;
        private const float RetreatHpRatio = 0.3f;
        private const float RetreatRange = 6f;
        private const float TargetSearchRange = 25f;

        [EntitySystem]
        private static void Awake(this StateSyncRoomRobotManagerComponent self)
        {
        }

        [EntitySystem]
        private static void FixedUpdate(this StateSyncRoomRobotManagerComponent self)
        {
            StateSyncRoom room = self.GetParent<StateSyncRoom>();
            if (room == null || room.IsDisposed || room.Status != RoomStatus.Playing)
            {
                return;
            }

            StateSyncRoomServerComponent roomServerComponent = room.GetComponent<StateSyncRoomServerComponent>();
            if (roomServerComponent == null || roomServerComponent.IsDisposed)
            {
                return;
            }

            foreach (long robotPlayerId in self.RobotPlayerIds)
            {
                StateSyncRoomPlayer roomPlayer = roomServerComponent.GetChild<StateSyncRoomPlayer>(robotPlayerId);
                Unit unit = roomPlayer?.Unit;
                if (roomPlayer == null || roomPlayer.IsDisposed || unit == null || unit.IsDisposed)
                {
                    continue;
                }

                self.TickMatchRobot(unit);
            }
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
            BTComponent btComponent = unit?.GetComponent<BTComponent>();
            bool needRecreate = unit == null
                    || unit.IsDisposed
                    || unit.ConfigId != ConstValue.StateSyncMatchRobotUnitConfigId
                    || btComponent == null
                    || !string.Equals(btComponent.TreePackageKey, ConstValue.StateSyncMatchRobotBehaviorTree, System.StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(btComponent.TreeIdOrName, ConstValue.StateSyncMatchRobotBehaviorTree, System.StringComparison.OrdinalIgnoreCase);
            if (needRecreate)
            {
                unit?.Dispose();
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
            roomPlayer.LastSyncedPosition = spawnPosition;
            roomPlayer.LastSyncedForward = spawnForward;
            roomPlayer.HasSyncedTransform = true;
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

        private static void TickMatchRobot(this StateSyncRoomRobotManagerComponent self, Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null || numericComponent.GetAsLong(NumericType.Hp) <= 0)
            {
                unit.GetComponent<PlayerMoveComponent>()?.StopMove();
                return;
            }

            SkillCastComponent skillCastComponent = unit.GetComponent<SkillCastComponent>();
            if (skillCastComponent != null && skillCastComponent.IsCasting())
            {
                unit.GetComponent<PlayerMoveComponent>()?.StopMove();
                return;
            }

            CombatStateComponent combatStateComponent = unit.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null && combatStateComponent.IsInControl())
            {
                unit.GetComponent<PlayerMoveComponent>()?.StopMove();
                return;
            }

            Unit target = TargetSelectHelper.FindNearestCombatTarget(unit, TargetSearchRange);
            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            if (target == null)
            {
                targetComponent?.ClearTarget();
                unit.GetComponent<PlayerMoveComponent>()?.StopMove();
                return;
            }

            targetComponent?.SetTarget(target.Id);

            float distance = TargetSelectHelper.GetDistance(unit, target);
            float hpRatio = BTCombatHelper.GetHpRatio(unit);
            PlayerMoveComponent playerMoveComponent = unit.GetComponent<PlayerMoveComponent>();
            if (playerMoveComponent == null)
            {
                return;
            }

            if (hpRatio <= RetreatHpRatio && distance < RetreatRange)
            {
                float3 retreatDirection = unit.Position - target.Position;
                if (math.lengthsq(retreatDirection) <= 0.0001f)
                {
                    retreatDirection = -unit.Forward;
                }

                unit.Forward = math.normalizesafe(retreatDirection, new float3(0, 0, 1));
                playerMoveComponent.StartMove();
                return;
            }

            if (distance > CombatMoveRange)
            {
                BTCombatHelper.FaceTarget(unit, target);
                playerMoveComponent.StartMove();
                return;
            }

            BTCombatHelper.FaceTarget(unit, target);
            playerMoveComponent.StopMove();
        }
    }
}
