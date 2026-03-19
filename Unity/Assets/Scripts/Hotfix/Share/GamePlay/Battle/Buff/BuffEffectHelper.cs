using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(Buff))]
    public static class BuffEffectHelper
    {
        public static void ApplyStartEffect(this Buff self)
        {
            switch (self.BuffConfig?.EffectData)
            {
                case ChangeNumericBuffEffectData changeNumeric when self.BuffConfig.TriggerInterval <= 0:
                    self.ApplyNumericEffect(changeNumeric);
                    break;
                case RelativeNumericBuffEffectData relativeNumeric when self.BuffConfig.TriggerInterval <= 0:
                    self.ApplyRelativeNumericEffect(relativeNumeric);
                    break;
                case ActionEventBuffEffectData actionEvent:
                    self.RunActionEvents(actionEvent.StartActionEventIds);
                    break;
            }
        }

        public static void ApplyTriggerEffect(this Buff self)
        {
            switch (self.BuffConfig?.EffectData)
            {
                case ChangeNumericBuffEffectData changeNumeric:
                    self.ApplyNumericEffect(changeNumeric);
                    break;
                case RelativeNumericBuffEffectData relativeNumeric:
                    self.ApplyRelativeNumericEffect(relativeNumeric);
                    break;
                case ActionEventBuffEffectData actionEvent:
                    self.RunActionEvents(actionEvent.TriggerActionEventIds);
                    break;
            }
        }

        public static void ApplyEndEffect(this Buff self)
        {
            if (self.BuffConfig?.EffectData is ActionEventBuffEffectData actionEvent)
            {
                self.RunActionEvents(actionEvent.EndActionEventIds);

                switch (self.RemoveReason)
                {
                    case EBuffRemoveReason.Expire:
                        self.RunActionEvents(actionEvent.ExpireActionEventIds);
                        break;
                    case EBuffRemoveReason.Dispel:
                        self.RunActionEvents(actionEvent.DispelActionEventIds);
                        break;
                    case EBuffRemoveReason.Remove:
                        self.RunActionEvents(actionEvent.RemoveActionEventIds);
                        break;
                }
            }
        }

        private static void ApplyNumericEffect(this Buff self, ChangeNumericBuffEffectData effectData)
        {
            if (effectData.NumericType <= 0 || effectData.Delta == 0)
            {
                return;
            }

            Unit target = self.Unit;
            if (target == null || target.IsDisposed)
            {
                return;
            }

            Unit source = self.GetSourceUnitOrOwner();
            long delta = (long)effectData.Delta * Math.Max(1, (int)self.LayerCount);
            EInterruptLevel interruptLevel = effectData.InterruptLevel >= (int)EInterruptLevel.None &&
                effectData.InterruptLevel <= (int)EInterruptLevel.Fatal
                ? (EInterruptLevel)effectData.InterruptLevel
                : EInterruptLevel.None;
            BattleHelper.ApplyNumericDelta(source, target, effectData.NumericType, delta, interruptLevel);
        }

        private static void ApplyRelativeNumericEffect(this Buff self, RelativeNumericBuffEffectData effectData)
        {
            if (effectData.NumericType <= 0 || effectData.BaseNumericType <= 0 || effectData.Ratio == 0)
            {
                return;
            }

            Unit target = self.Unit;
            if (target == null || target.IsDisposed)
            {
                return;
            }

            NumericComponent numericComponent = target.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            long baseValue = self.BuffConfig?.SnapshotPolicy == EBuffSnapshotPolicy.Snapshot
                ? self.EffectSnapshotBaseValue
                : numericComponent.GetAsLong(effectData.BaseNumericType);
            if (baseValue == 0)
            {
                return;
            }

            long delta = baseValue * effectData.Ratio / 10000;
            if (delta == 0)
            {
                return;
            }

            Unit source = self.GetSourceUnitOrOwner();
            delta *= Math.Max(1, (int)self.LayerCount);
            EInterruptLevel interruptLevel = effectData.InterruptLevel >= (int)EInterruptLevel.None &&
                effectData.InterruptLevel <= (int)EInterruptLevel.Fatal
                ? (EInterruptLevel)effectData.InterruptLevel
                : EInterruptLevel.None;
            BattleHelper.ApplyNumericDelta(source, target, effectData.NumericType, delta, interruptLevel);
        }

        private static Unit GetSourceUnitOrOwner(this Buff self)
        {
            if (self.SourceUnitId == 0)
            {
                return self.Unit;
            }

            return self.Root()?.GetComponent<UnitComponent>()?.Get(self.SourceUnitId) ?? self.Unit;
        }

        private static void RunActionEvents(this Buff self, List<int> actionEventIds)
        {
            if (actionEventIds == null || actionEventIds.Count == 0)
            {
                return;
            }

            for (int index = 0; index < actionEventIds.Count; ++index)
            {
                int actionEventId = actionEventIds[index];
                if (actionEventId <= 0 || ActionEventConfigCategory.Instance.GetOrDefault(actionEventId) == null)
                {
                    continue;
                }

                self.CreateActionEvent(actionEventId);
            }
        }
    }
}
