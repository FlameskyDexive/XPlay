using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;

namespace ET.Server
{
    public static partial class UnitFactory
    {
        public static Unit Create(Scene scene, long id, EUnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case EUnitType.Player:
                {
                    Unit unit = unitComponent.AddChildWithId<Unit, int>(id, 1001);
                    unit.AddComponent<PlayerMoveComponent>();
                    AddCombatCollider(unit);

                    PlayerNumericConfig numericConfig = PlayerNumericConfigCategory.Instance.GetOrDefault(unit.ConfigId);
                    NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                    numericComponent.Set(NumericType.Speed, numericConfig?.Speed > 0 ? numericConfig.Speed : 5f);
                    numericComponent.Set(NumericType.AOI, 15000);
                    numericComponent.SetNoEvent(NumericType.MaxHp, numericConfig?.Hp > 0 ? numericConfig.Hp : unit.Config().Weight);
                    numericComponent.SetNoEvent(NumericType.Hp, numericConfig?.Hp > 0 ? numericConfig.Hp : unit.Config().Weight);
                    numericComponent.SetNoEvent(NumericType.Attack, numericConfig?.Attack > 0 ? numericConfig.Attack : 10);

                    unitComponent.Add(unit);
                    unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

                    unit.AddComponent<CombatStateComponent>();
                    unit.AddComponent<SkillCastComponent>();
                    unit.AddComponent<TargetComponent>();
                    unit.AddComponent<ThreatComponent>();
                    unit.AddComponent<BuffComponent>();
                    unit.AddComponent<SkillComponent, List<int>>(new List<int>() { 1001, 1002 });
                    return unit;
                }
                case EUnitType.Monster:
                {
                    return CreateMonster(scene, id, 3001, new[] { 1001 }, "AITest");
                }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }

        public static Unit CreateMonster(Scene scene, long id, int configId, IReadOnlyCollection<int> skillIds, string behaviorTreeName, int spawnConfigId = 0, int spawnIndex = 0)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = unitComponent.AddChildWithId<Unit, int>(id, configId);
            unit.AddComponent<PlayerMoveComponent>();
            AddCombatCollider(unit);

            MonsterNumericConfig numericConfig = MonsterNumericConfigCategory.Instance.GetOrDefault(configId);
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            numericComponent.Set(NumericType.Speed, numericConfig?.Speed > 0 ? numericConfig.Speed : 4f);
            numericComponent.Set(NumericType.AOI, 15000);
            numericComponent.SetNoEvent(NumericType.MaxHp, numericConfig?.Hp > 0 ? numericConfig.Hp : 120);
            numericComponent.SetNoEvent(NumericType.Hp, numericConfig?.Hp > 0 ? numericConfig.Hp : 120);
            numericComponent.SetNoEvent(NumericType.Attack, numericConfig?.Attack > 0 ? numericConfig.Attack : 8);

            unitComponent.Add(unit);
            unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

            unit.AddComponent<CombatStateComponent>();
            unit.AddComponent<SkillCastComponent>();
            unit.AddComponent<TargetComponent>();
            unit.AddComponent<ThreatComponent>();
            unit.AddComponent<BuffComponent>();

            List<int> runtimeSkillIds = new List<int>();
            if (skillIds != null)
            {
                foreach (int skillId in skillIds)
                {
                    if (skillId > 0)
                    {
                        runtimeSkillIds.Add(skillId);
                    }
                }
            }

            if (runtimeSkillIds.Count > 0)
            {
                unit.AddComponent<SkillComponent, List<int>>(runtimeSkillIds);
            }
            else
            {
                unit.AddComponent<SkillComponent>();
            }

            byte[] behaviorTreeBytes = LoadBehaviorTreeBytes(behaviorTreeName);
            if (behaviorTreeBytes != null && behaviorTreeBytes.Length > 0)
            {
                unit.AddComponent<BTComponent, byte[], string>(behaviorTreeBytes, behaviorTreeName);
            }

            if (spawnConfigId > 0)
            {
                unit.AddComponent<MonsterSpawnMarkerComponent, int, int>(spawnConfigId, spawnIndex);
            }

            return unit;
        }

        public static Unit CreateBullet(Scene scene, long id, Skill ownerSkill, int config, float3 forward, float speed, int lifeMs, float radius, List<int> hitActionEventIds)
        {
            Log.Info("create bullet");
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit owner = ownerSkill.Unit;
            Unit bullet = unitComponent.AddChildWithId<Unit, int>(id, config);
            MoveComponent moveComponent = bullet.AddComponent<MoveComponent>();
            bullet.Position = owner.Position;
            bullet.Forward = math.normalizesafe(forward, owner.Forward);
            bullet.AddComponent<AOIEntity, int, float3>(15 * 1000, bullet.Position);
            float colliderRadius = radius > 0f ? radius : 0.2f;
            bullet.AddComponent<CollisionComponent>().AddCollider(EColliderType.Circle, Vector2.One * colliderRadius, Vector2.Zero, true, bullet);
            bullet.AddComponent<BulletComponent>().Init(ownerSkill, owner, hitActionEventIds, lifeMs);
            NumericComponent numericComponent = bullet.AddComponent<NumericComponent>();
            float moveSpeed = speed > 0f ? speed : 10f;
            numericComponent.Set(NumericType.Speed, moveSpeed);
            numericComponent.Set(NumericType.AOI, 15000);
            numericComponent.SetNoEvent(NumericType.MaxHp, 1);
            numericComponent.SetNoEvent(NumericType.Hp, 1);
            float3 targetPoint = bullet.Position + bullet.Forward * numericComponent.GetAsFloat(NumericType.Speed) * math.max(0.1f, lifeMs / 1000f);
            List<float3> paths = new List<float3>();
            paths.Add(bullet.Position);
            paths.Add(targetPoint);
            moveComponent.MoveToAsync(paths, numericComponent.GetAsFloat(NumericType.Speed)).Coroutine();

            return bullet;
        }

        private static void AddCombatCollider(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            if (unit.GetComponent<CollisionComponent>() != null)
            {
                return;
            }

            float radius = Math.Clamp(unit.Config().Weight / 100f, 0.45f, 1.25f);
            unit.AddComponent<CollisionComponent>().AddCollider(EColliderType.Circle, new Vector2(radius, radius), Vector2.Zero, true, unit);
        }

        private static byte[] LoadBehaviorTreeBytes(string behaviorTreeName)
        {
            if (!string.IsNullOrWhiteSpace(behaviorTreeName))
            {
                byte[] bytes = BTLoader.Instance.LoadBytes(behaviorTreeName, false);
                if (bytes != null && bytes.Length > 0)
                {
                    return bytes;
                }

                if (string.Equals(behaviorTreeName, "AITest", StringComparison.OrdinalIgnoreCase))
                {
                    bytes = ET.Client.BTClientDemoFactory.CreateAITestBytes();
                    if (bytes != null && bytes.Length > 0)
                    {
                        return bytes;
                    }
                }
            }

            return null;
        }
    }
}
