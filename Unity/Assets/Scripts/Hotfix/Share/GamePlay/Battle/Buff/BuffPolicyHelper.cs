using System;

namespace ET
{
    [FriendOf(typeof(Buff))]
    public static class BuffPolicyHelper
    {
        public static void CaptureSnapshot(this Buff self)
        {
            if (self.BuffConfig?.SnapshotPolicy != EBuffSnapshotPolicy.Snapshot)
            {
                self.EffectSnapshotBaseValue = 0;
                return;
            }

            if (self.BuffConfig.EffectData is not RelativeNumericBuffEffectData effectData)
            {
                self.EffectSnapshotBaseValue = 0;
                return;
            }

            NumericComponent numericComponent = self.Unit?.GetComponent<NumericComponent>();
            self.EffectSnapshotBaseValue = numericComponent?.GetAsLong(effectData.BaseNumericType) ?? 0;
        }

        public static void UpdateStrengthScore(this Buff self)
        {
            self.StrengthScore = CalculateStrengthScore(self.BuffConfig, self.Unit, self.EffectSnapshotBaseValue, self.LayerCount);
        }

        public static long CalculateStrengthScore(BuffConfig buffConfig, Unit target, long effectSnapshotBaseValue = 0, uint layerCount = 1)
        {
            if (buffConfig == null)
            {
                return 0;
            }

            long layers = Math.Max(1L, layerCount);
            long strength = 0;

            if (buffConfig.ModifierData is ChangeNumericBuffModifierData modifierData)
            {
                strength += Math.Abs((long)modifierData.Delta * layers);
            }

            switch (buffConfig.EffectData)
            {
                case ChangeNumericBuffEffectData effectData:
                    strength += Math.Abs((long)effectData.Delta * layers);
                    break;
                case RelativeNumericBuffEffectData relativeEffect:
                {
                    long baseValue = buffConfig.SnapshotPolicy == EBuffSnapshotPolicy.Snapshot
                        ? effectSnapshotBaseValue
                        : target?.GetComponent<NumericComponent>()?.GetAsLong(relativeEffect.BaseNumericType) ?? 0;
                    strength += Math.Abs(baseValue * relativeEffect.Ratio / 10000 * layers);
                    break;
                }
            }

            return strength;
        }
    }
}
