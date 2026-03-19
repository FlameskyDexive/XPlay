namespace ET
{
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(BuffComponent))]
    public static class BuffModifierHelper
    {
        public static void ApplyModifierLayers(this Buff self, int layerDelta)
        {
            if (layerDelta == 0 || self.BuffConfig?.ModifierData is not ChangeNumericBuffModifierData modifierData)
            {
                return;
            }

            if (modifierData.NumericType <= 0 || modifierData.Delta == 0)
            {
                return;
            }

            NumericComponent numericComponent = self.Unit?.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            long currentValue = numericComponent.GetByKey(modifierData.NumericType);
            long delta = (long)modifierData.Delta * layerDelta;
            numericComponent.Set(modifierData.NumericType, currentValue + delta);
        }

        public static void RemoveAllModifierLayers(this Buff self)
        {
            if (self.LayerCount == 0)
            {
                return;
            }

            self.ApplyModifierLayers(-(int)self.LayerCount);
        }

        public static void RefreshDuration(this Buff self)
        {
            self.StartTime = TimeInfo.Instance.ServerNow();
            if (self.BuffConfig.TriggerInterval > 0)
            {
                int firstTriggerDelay = self.BuffConfig.PeriodicStartDelayMs > 0
                    ? self.BuffConfig.PeriodicStartDelayMs
                    : self.BuffConfig.TriggerInterval;
                self.NextTriggerTime = self.StartTime + firstTriggerDelay;
                return;
            }

            self.NextTriggerTime = long.MaxValue;
        }

        public static void RemoveCurrentBuff(this Buff self, EBuffRemoveReason removeReason = EBuffRemoveReason.Remove)
        {
            self.RemoveReason = removeReason;
            BuffComponent buffComponent = self.GetParent<BuffComponent>();
            buffComponent?.BuffDic.Remove(self.BuffId);
            self.Dispose();
        }
    }
}
