using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    public static class ProjectileHelper
    {
        private const float DEFAULT_SPEED = 10f;
        private const int DEFAULT_LIFE_MS = 1000;
        private const float DEFAULT_RADIUS = 0.2f;
        private const int DEFAULT_COUNT = 1;
        private const float DEFAULT_SPREAD_ANGLE = 0f;

        private readonly struct ProjectileSpawnOptions
        {
            public readonly int BulletConfigId;
            public readonly float Speed;
            public readonly int LifeMs;
            public readonly float Radius;
            public readonly int Count;
            public readonly float SpreadAngleDeg;
            public readonly List<int> HitActionEventIds;

            public ProjectileSpawnOptions(int bulletConfigId, float speed, int lifeMs, float radius, int count, float spreadAngleDeg, List<int> hitActionEventIds)
            {
                BulletConfigId = bulletConfigId;
                Speed = speed;
                LifeMs = lifeMs;
                Radius = radius;
                Count = count;
                SpreadAngleDeg = spreadAngleDeg;
                HitActionEventIds = hitActionEventIds;
            }
        }

        public static void SpawnProjectiles(ActionEvent actionEvent, Unit owner)
        {
#if DOTNET
            if (owner == null || owner.IsDisposed || actionEvent == null || actionEvent.IsDisposed)
            {
                return;
            }

            BulletActionEventData eventData = actionEvent.ActionEventConfig?.EventData as BulletActionEventData;
            if (!TryParseSpawnOptions(eventData, out ProjectileSpawnOptions options))
            {
                return;
            }

            Skill ownerSkill = actionEvent.OwnerSkill;
            if (ownerSkill == null || ownerSkill.IsDisposed)
            {
                return;
            }

            Scene scene = actionEvent.Scene();
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            float3 baseForward = math.normalizesafe(owner.Forward, new float3(0, 0, 1));
            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            createUnits.Units = new List<UnitInfo>();

            for (int index = 0; index < options.Count; ++index)
            {
                float3 forward = RotateForward(baseForward, GetSpreadAngle(index, options.Count, options.SpreadAngleDeg));
                Unit bullet = Server.UnitFactory.CreateBullet(
                    scene,
                    IdGenerater.Instance.GenerateId(),
                    ownerSkill,
                    options.BulletConfigId,
                    forward,
                    options.Speed,
                    options.LifeMs,
                    options.Radius,
                    options.HitActionEventIds);
                if (bullet == null || bullet.IsDisposed)
                {
                    continue;
                }

                createUnits.Units.Add(Server.UnitHelper.CreateUnitInfo(bullet));
            }

            if (createUnits.Units.Count > 0)
            {
                Server.MapMessageHelper.SendToClient(owner, createUnits);
            }
            else
            {
                createUnits.Dispose();
            }
#endif
        }

        private static bool TryParseSpawnOptions(BulletActionEventData eventData, out ProjectileSpawnOptions options)
        {
            options = default;
            if (eventData == null || eventData.BulletConfigId <= 0)
            {
                return false;
            }

            float speed = eventData.Speed > 0 ? eventData.Speed / 1000f : DEFAULT_SPEED;
            int lifeMs = eventData.LifeMs > 0 ? eventData.LifeMs : DEFAULT_LIFE_MS;
            float radius = eventData.Radius > 0 ? eventData.Radius / 1000f : DEFAULT_RADIUS;
            int count = eventData.Count > 0 ? eventData.Count : DEFAULT_COUNT;
            float spreadAngleDeg = eventData.SpreadAngleDeg > 0 ? eventData.SpreadAngleDeg : DEFAULT_SPREAD_ANGLE;
            List<int> hitEvents = CollectHitActionEventIds(eventData.HitActionEventIds);
            options = new ProjectileSpawnOptions(eventData.BulletConfigId, speed, lifeMs, radius, count, spreadAngleDeg, hitEvents);
            return true;
        }

        private static List<int> CollectHitActionEventIds(List<int> parameters)
        {
            List<int> hitActionEventIds = null;
            if (parameters == null || parameters.Count == 0)
            {
                return hitActionEventIds;
            }

            hitActionEventIds = new List<int>();
            for (int index = 0; index < parameters.Count; ++index)
            {
                if (parameters[index] > 0)
                {
                    hitActionEventIds.Add(parameters[index]);
                }
            }

            return hitActionEventIds;
        }

        private static float GetSpreadAngle(int index, int count, float totalSpreadAngleDeg)
        {
            if (count <= 1 || totalSpreadAngleDeg <= 0f)
            {
                return 0f;
            }

            float startAngle = -totalSpreadAngleDeg * 0.5f;
            float stepAngle = totalSpreadAngleDeg / (count - 1);
            return startAngle + stepAngle * index;
        }

        private static float3 RotateForward(float3 forward, float angleDeg)
        {
            if (math.abs(angleDeg) < 0.001f)
            {
                return forward;
            }

            quaternion rotation = quaternion.RotateY(math.radians(angleDeg));
            return math.normalizesafe(math.rotate(rotation, forward), forward);
        }
    }
}
