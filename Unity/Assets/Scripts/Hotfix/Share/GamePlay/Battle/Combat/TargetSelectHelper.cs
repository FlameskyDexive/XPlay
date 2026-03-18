using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    public static class TargetSelectHelper
    {
        public static bool TryGetTarget(Unit self, long targetUnitId, out Unit target)
        {
            target = null;
            if (self == null || self.IsDisposed || targetUnitId == 0)
            {
                return false;
            }

            UnitComponent unitComponent = self.Root()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return false;
            }

            target = unitComponent.Get(targetUnitId);
            return target != null && !target.IsDisposed;
        }

        public static bool IsValidCombatTarget(Unit self, Unit target, float maxRange = float.MaxValue)
        {
            if (self == null || target == null || self.IsDisposed || target.IsDisposed || self.Id == target.Id)
            {
                return false;
            }

            if (target.Type() != EUnitType.Player && target.Type() != EUnitType.Monster)
            {
                return false;
            }

            SkillComponent skillComponent = target.GetComponent<SkillComponent>();
            if (skillComponent != null && skillComponent.IsDead())
            {
                return false;
            }

            return GetDistance(self, target) <= maxRange;
        }

        public static float GetDistance(Unit self, Unit target)
        {
            if (self == null || target == null)
            {
                return float.MaxValue;
            }

            return math.distance(self.Position, target.Position);
        }

        public static Unit FindNearestCombatTarget(Unit self, float maxRange = float.MaxValue)
        {
            if (self == null || self.IsDisposed)
            {
                return null;
            }

            UnitComponent unitComponent = self.Root()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return null;
            }

            Unit nearestTarget = null;
            float nearestDistance = maxRange;
            foreach (Entity value in unitComponent.Children.Values)
            {
                Unit candidate = value as Unit;
                if (!IsValidCombatTarget(self, candidate, maxRange))
                {
                    continue;
                }

                float distance = GetDistance(self, candidate);
                if (distance >= nearestDistance)
                {
                    continue;
                }

                nearestDistance = distance;
                nearestTarget = candidate;
            }

            return nearestTarget;
        }
    }
}
