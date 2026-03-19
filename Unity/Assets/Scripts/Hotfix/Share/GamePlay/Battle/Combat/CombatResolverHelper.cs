using System;
using System.Collections.Generic;
using System.Numerics;
using Box2DSharp.Collision;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(CollisionComponent))]
    public static class CombatResolverHelper
    {
        private readonly struct CombatHitArea
        {
            public readonly EColliderType ColliderType;
            public readonly Vector2 Center;
            public readonly float Radius;
            public readonly float HalfWidth;
            public readonly float HalfHeight;
            public readonly float Angle;

            public CombatHitArea(EColliderType colliderType, Vector2 center, float radius, float halfWidth, float halfHeight, float angle)
            {
                ColliderType = colliderType;
                Center = center;
                Radius = radius;
                HalfWidth = halfWidth;
                HalfHeight = halfHeight;
                Angle = angle;
            }
        }

        public static void ResolveHit(Unit from, Unit to, EHitFromType hitType = EHitFromType.Skill_Normal, Unit bullet = null)
        {
            BattleHelper.HitSettle(from, to, hitType, bullet);
        }

        public static ListComponent<Unit> QueryRangeDamageTargets(Unit owner, RangeDamageActionEventData eventData)
        {
            ListComponent<Unit> targets = ListComponent<Unit>.Create();
            if (owner == null || owner.IsDisposed || eventData == null)
            {
                return targets;
            }

            if (!TryBuildHitArea(owner, eventData, out CombatHitArea area))
            {
                return targets;
            }

            Shape shape = CreateShape(area);
            if (shape == null)
            {
                return targets;
            }

            Transform transform = new Transform(area.Center, area.Angle);
            UnitComponent unitComponent = owner.Root()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return targets;
            }

            GJkProfile gjkProfile = new GJkProfile();
            foreach (Entity child in unitComponent.Children.Values)
            {
                if (child is not Unit target || target.IsDisposed)
                {
                    continue;
                }

                if (!TargetSelectHelper.IsValidCombatTarget(owner, target))
                {
                    continue;
                }

                CollisionComponent collisionComponent = target.GetComponent<CollisionComponent>();
                if (collisionComponent?.Body != null && collisionComponent.Body.FixtureList.Count > 0)
                {
                    if (!CollisionUtils.TestOverlap(shape, 0, collisionComponent.Body.FixtureList[0].Shape, 0, transform, collisionComponent.Body.GetTransform(), gjkProfile))
                    {
                        continue;
                    }
                }
                else if (!ContainsPoint(area, ToVector2(target.Position)))
                {
                    continue;
                }

                targets.Add(target);
            }

            return targets;
        }

        private static bool TryBuildHitArea(Unit owner, RangeDamageActionEventData eventData, out CombatHitArea area)
        {
            area = default;
            if (eventData == null)
            {
                return false;
            }

            int radiusMilli = eventData.Radius;
            if (radiusMilli <= 0)
            {
                return false;
            }

            List<int> parameters = new List<int>(1) { radiusMilli };

            Vector2 origin = ToVector2(owner.Position);
            Vector2 forward = ToVector2(owner.Forward);
            if (forward.LengthSquared() <= 0.0001f)
            {
                forward = new Vector2(0f, 1f);
            }
            else
            {
                forward = Vector2.Normalize(forward);
            }

            Vector2 right = new Vector2(forward.Y, -forward.X);
            bool encodedShape = parameters.Count > 1 && parameters[0] >= (int)EColliderType.Circle && parameters[0] <= (int)EColliderType.Box;
            if (!encodedShape)
            {
                float legacyRadius = ToMeters(parameters[0]);
                area = new CombatHitArea(EColliderType.Circle, origin, legacyRadius, 0f, 0f, 0f);
                return legacyRadius > 0f;
            }

            EColliderType colliderType = (EColliderType)parameters[0];
            switch (colliderType)
            {
                case EColliderType.Circle:
                {
                    float radius = parameters.Count > 1 ? ToMeters(parameters[1]) : 0f;
                    float forwardOffset = parameters.Count > 2 ? ToMeters(parameters[2]) : 0f;
                    float rightOffset = parameters.Count > 3 ? ToMeters(parameters[3]) : 0f;
                    Vector2 center = origin + forward * forwardOffset + right * rightOffset;
                    area = new CombatHitArea(EColliderType.Circle, center, radius, 0f, 0f, 0f);
                    return radius > 0f;
                }
                case EColliderType.Box:
                {
                    float width = parameters.Count > 1 ? ToMeters(parameters[1]) : 0f;
                    float height = parameters.Count > 2 ? ToMeters(parameters[2]) : 0f;
                    float forwardOffset = parameters.Count > 3 ? ToMeters(parameters[3]) : 0f;
                    float rightOffset = parameters.Count > 4 ? ToMeters(parameters[4]) : 0f;
                    float extraAngle = parameters.Count > 5 ? math.radians(parameters[5]) : 0f;
                    Vector2 center = origin + forward * forwardOffset + right * rightOffset;
                    float angle = MathHelper.Angle(new float3(0f, 0f, 1f), new float3(owner.Forward.x, 0f, owner.Forward.z)) + extraAngle;
                    area = new CombatHitArea(EColliderType.Box, center, 0f, width * 0.5f, height * 0.5f, angle);
                    return width > 0f && height > 0f;
                }
                default:
                    return false;
            }
        }

        private static Shape CreateShape(CombatHitArea area)
        {
            switch (area.ColliderType)
            {
                case EColliderType.Circle:
                    return new CircleShape { Radius = area.Radius, Position = Vector2.Zero };
                case EColliderType.Box:
                {
                    PolygonShape shape = new PolygonShape();
                    shape.SetAsBox(area.HalfWidth, area.HalfHeight, Vector2.Zero, 0f);
                    return shape;
                }
                default:
                    return null;
            }
        }

        private static bool ContainsPoint(CombatHitArea area, Vector2 point)
        {
            switch (area.ColliderType)
            {
                case EColliderType.Circle:
                    return Vector2.DistanceSquared(area.Center, point) <= area.Radius * area.Radius;
                case EColliderType.Box:
                {
                    Vector2 local = Rotate(point - area.Center, -area.Angle);
                    return MathF.Abs(local.X) <= area.HalfWidth && MathF.Abs(local.Y) <= area.HalfHeight;
                }
                default:
                    return false;
            }
        }

        private static Vector2 Rotate(Vector2 value, float angle)
        {
            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);
            return new Vector2(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);
        }

        private static float ToMeters(int milliUnits)
        {
            return milliUnits / 1000f;
        }

        private static Vector2 ToVector2(float3 value)
        {
            return new Vector2(value.x, value.z);
        }
    }
}
