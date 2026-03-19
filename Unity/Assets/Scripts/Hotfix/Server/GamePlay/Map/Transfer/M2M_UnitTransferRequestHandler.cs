using System;
using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    [FriendOf(typeof(MonsterSpawnMarkerComponent))]
    public class M2M_UnitTransferRequestHandler: MessageHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse>
    {
        protected override async ETTask Run(Scene scene, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = MongoHelper.Deserialize<Unit>(request.Unit);

            unitComponent.AddChild(unit);
            unitComponent.Add(unit);

            foreach (byte[] bytes in request.Entitys)
            {
                Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                unit.AddComponent(entity);
            }

            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = new float3(-10, 0, -10);

            unit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
            m2CStartSceneChange.SceneInstanceId = scene.InstanceId;
            m2CStartSceneChange.SceneName = scene.Name;
            MapMessageHelper.SendToClient(unit, m2CStartSceneChange);

            // 通知客户端创建My Unit
            M2C_CreateMyUnit m2CCreateUnits = M2C_CreateMyUnit.Create();
            m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
            MapMessageHelper.SendToClient(unit, m2CCreateUnits);

            // 加入aoi
            unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

            EnsureConfiguredMonsters(scene, unit);

            // 解锁location，可以接收发给Unit的消息
            await scene.Root().GetComponent<LocationProxyComponent>().UnLock(LocationType.Unit, unit.Id, request.OldActorId, unit.GetActorId());
        }

        private static void EnsureConfiguredMonsters(Scene scene, Unit playerUnit)
        {
            if (scene == null || playerUnit == null || playerUnit.IsDisposed || playerUnit.Type() != EUnitType.Player)
            {
                return;
            }

            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            MonsterSpawnRuntimeComponent runtimeComponent = scene.GetComponent<MonsterSpawnRuntimeComponent>();
            if (runtimeComponent == null)
            {
                runtimeComponent = scene.AddComponent<MonsterSpawnRuntimeComponent>();
            }

            foreach (MonsterSpawnConfig spawnConfig in MonsterSpawnConfigCategory.Instance.GetAll().Values)
            {
                if (!string.Equals(spawnConfig.SceneName, scene.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                EnsureConfiguredMonster(scene, unitComponent, runtimeComponent, playerUnit, spawnConfig);
            }
        }

        private static void EnsureConfiguredMonster(Scene scene, UnitComponent unitComponent, MonsterSpawnRuntimeComponent runtimeComponent, Unit playerUnit, MonsterSpawnConfig spawnConfig)
        {
            float3 spawnPosition = new float3(spawnConfig.SpawnX, spawnConfig.SpawnY, spawnConfig.SpawnZ);
            int existingCount = 0;

            foreach (Entity child in unitComponent.Children.Values)
            {
                if (child is not Unit unit || unit.IsDisposed || unit.Type() != EUnitType.Monster)
                {
                    continue;
                }

                MonsterSpawnMarkerComponent markerComponent = unit.GetComponent<MonsterSpawnMarkerComponent>();
                if (markerComponent == null || !markerComponent.MatchesSpawnConfig(spawnConfig.Id))
                {
                    continue;
                }

                ++existingCount;
                MapMessageHelper.NoticeUnitAdd(playerUnit, unit);
            }

            if (spawnConfig.SpawnOnce && runtimeComponent.HasSpawned(spawnConfig.Id))
            {
                return;
            }

            int needSpawnCount = Math.Max(0, spawnConfig.Count - existingCount);
            for (int index = existingCount; index < existingCount + needSpawnCount; ++index)
            {
                float3 monsterSpawnPosition = spawnPosition + new float3(spawnConfig.OffsetX * index, spawnConfig.OffsetY * index, spawnConfig.OffsetZ * index);
                Unit monster = UnitFactory.CreateMonster(scene, IdGenerater.Instance.GenerateId(), spawnConfig.UnitConfigId, spawnConfig.SkillIds, spawnConfig.BehaviorTreeName, spawnConfig.Id, index);
                if (monster == null || monster.IsDisposed)
                {
                    continue;
                }

                monster.Position = monsterSpawnPosition;
                float3 lookDirection = playerUnit.Position - monster.Position;
                if (math.lengthsq(lookDirection) > 0.0001f)
                {
                    monster.Forward = math.normalize(lookDirection);
                }

                foreach (Entity child in unitComponent.Children.Values)
                {
                    if (child is not Unit unit || unit.IsDisposed || unit.Type() != EUnitType.Player)
                    {
                        continue;
                    }

                    MapMessageHelper.NoticeUnitAdd(unit, monster);
                }
            }

            if (spawnConfig.SpawnOnce && (existingCount > 0 || needSpawnCount > 0))
            {
                runtimeComponent.MarkSpawned(spawnConfig.Id);
            }
        }
    }
}
