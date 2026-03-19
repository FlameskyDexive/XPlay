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

        public static ListComponent<Unit> QueryRangeDamageTargets(Unit owner, ActionEventBaseData eventData)
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

        private static bool TryBuildHitArea(Unit owner, ActionEventBaseData eventData, out CombatHitArea area)
        {
            area = default;
            if (eventData == null)
            {
                return false;
            }

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
            switch (eventData)
            {
                case RangeDamageActionEventData rangeDamageActionEventData:
                {
                    float radius = ToMeters(rangeDamageActionEventData.Radius);
                    area = new CombatHitArea(EColliderType.Circle, origin, radius, 0f, 0f, 0f);
                    return radius > 0f;
                }
                case ShapeRangeDamageActionEventData shapeRangeDamageActionEventData:
                {
                    float forwardOffset = ToMeters(shapeRangeDamageActionEventData.ForwardOffset);
                    float rightOffset = ToMeters(shapeRangeDamageActionEventData.RightOffset);
                    Vector2 center = origin + forward * forwardOffset + right * rightOffset;
                    switch (shapeRangeDamageActionEventData.ColliderType)
                    {
                        case EColliderType.Circle:
                        {
                            float radius = ToMeters(shapeRangeDamageActionEventData.Radius);
                            area = new CombatHitArea(EColliderType.Circle, center, radius, 0f, 0f, 0f);
                            return radius > 0f;
                        }
                        case EColliderType.Box:
                        {
                            float width = ToMeters(shapeRangeDamageActionEventData.Width);
                            float height = ToMeters(shapeRangeDamageActionEventData.Height);
                            float extraAngle = math.radians(shapeRangeDamageActionEventData.ExtraAngleDeg);
                            float angle = MathHelper.Angle(new float3(0f, 0f, 1f), new float3(owner.Forward.x, 0f, owner.Forward.z)) + extraAngle;
                            area = new CombatHitArea(EColliderType.Box, center, 0f, width * 0.5f, height * 0.5f, angle);
                            return width > 0f && height > 0f;
                        }
                        default:
                            return false;
                    }
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
