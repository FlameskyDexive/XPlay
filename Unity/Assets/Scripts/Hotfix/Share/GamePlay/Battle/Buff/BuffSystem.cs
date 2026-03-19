using System;
using System.Collections.Generic;

namespace ET
{

    [EntitySystemOf(typeof(Buff))]
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(NumericComponent))]
    public static partial class BuffSystem
    {

        [EntitySystem]
        public static void Awake(this Buff self, int BuffId)
        {
            self.BuffId = BuffId;
            //常规buff添加则立即出发一次，时间到销毁。如果有触发间隔，则间隔固定的时间再次出发buff行为
            self.InitBuff();
        }
        [EntitySystem]
        public static void Destroy(this Buff self)
        {

            if (self.BuffConfig?.EndEvents?.Count > 0)
            {
                foreach (int eventId in self.BuffConfig.EndEvents)
                {
                    self.CreateActionEvent(eventId);
                }
            }

        }
        /// <summary>
        /// 每帧更新检测buff的周期、触发事件等. 如果表现层需要获取当前buff的剩余时间进度等，此处更新
        /// </summary>
        /// <param name="self"></param>
        [EntitySystem]
        public static void FixedUpdate(this Buff self)
        {
            long now = TimeInfo.Instance.ServerNow();
            if (now > self.StartTime + self.BuffConfig.Duration)
            {
                self.LifeTimeout();
                return;
            }

            if (self.BuffConfig.TriggerInterval > 0 && now >= self.NextTriggerTime)
            {
                self.TriggerBuff();
                self.NextTriggerTime = now + self.BuffConfig.TriggerInterval;
            }
        }

        public static void LifeTimeout(this Buff self)
        {
            //layerCount > 0时，减少层数量，重新计时buff
            --self.LayerCount;
            if (self.LayerCount > 0)
            {
                self.RefreshDuration();
                return;
            }
            //移除Buff
            self.GetParent<BuffComponent>().RemoveBuff(self.BuffId);
        }
        public static void InitBuff(this Buff self)
        {

            if (self.BuffConfig?.StartEvents?.Count > 0)
            {
                foreach (int eventId in self.BuffConfig.StartEvents)
                {
                    self.CreateActionEvent(eventId);
                }
            }

            self.RefreshDuration();
        }

        public static void RefreshDuration(this Buff self)
        {
            self.StartTime = TimeInfo.Instance.ServerNow();
            self.NextTriggerTime = self.BuffConfig.TriggerInterval > 0
                ? self.StartTime + self.BuffConfig.TriggerInterval
                : long.MaxValue;
        }

        /// <summary>
        /// 触发buff行为
        /// </summary>
        /// <param name="self"></param>
        public static void TriggerBuff(this Buff self)
        {
            //如果buff携带可触发技能事件，则触发事件

            if (self.BuffConfig?.TriggerEvents?.Count > 0)
            {
                foreach (int eventId in self.BuffConfig.TriggerEvents)
                {
                    self.CreateActionEvent(eventId);
                }
            }

        }

        /// <summary>
        /// 触发技能事件
        /// </summary>
        /// <param name="self"></param>
        public static void TriggerEvent(this Buff self)
        {
            
        }
        

        public static Unit GetOwnerUnit(this Buff self)
        {
            return self.Unit;
        }
        
    }

   
}
