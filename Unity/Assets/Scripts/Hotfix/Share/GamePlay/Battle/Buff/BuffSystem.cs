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
        public static void Awake(this Buff self, BuffApplyRequest request)
        {
            self.BuffId = request.BuffId;
            self.SourceUnitId = request.SourceUnitId;
            self.SourceSkillConfigId = request.SourceSkillConfigId;
            self.GroupId = self.BuffConfig?.Goup ?? 0;
            self.LayerCount = 1;
            self.ApplyModifierLayers(1);
            self.CaptureSnapshot();
            self.UpdateStrengthScore();
            self.ApplyGrantedTags();
            self.InitBuff();
            Log.Info($"buff apply success unit:{self.Unit?.Id ?? 0} buff:{self.BuffId} source:{self.SourceUnitId} group:{self.GroupId} tags:{self.BuffConfig?.TagGrantMask ?? 0}");
        }
        [EntitySystem]
        public static void Destroy(this Buff self)
        {
            Log.Info($"buff destroy unit:{self.Unit?.Id ?? 0} buff:{self.BuffId} reason:{self.RemoveReason}");
            self.ApplyEndEffect();
            self.RemoveAllModifierLayers();
            self.RefreshGrantedTags();
        }
        /// <summary>
        /// 每帧更新检测buff的周期、触发事件等. 如果表现层需要获取当前buff的剩余时间进度等，此处更新
        /// </summary>
        /// <param name="self"></param>
        [EntitySystem]
        public static void FixedUpdate(this Buff self)
        {
            BuffConfig buffConfig = self.BuffConfig;
            if (buffConfig == null)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerNow();
            if (now > self.StartTime + buffConfig.Duration)
            {
                self.LifeTimeout();
                return;
            }

            if (buffConfig.TriggerInterval > 0 && now >= self.NextTriggerTime)
            {
                self.TriggerBuff();
                self.NextTriggerTime = now + buffConfig.TriggerInterval;
            }
        }

        public static void LifeTimeout(this Buff self)
        {
            if (self.LayerCount == 0)
            {
                self.RemoveCurrentBuff(EBuffRemoveReason.Expire);
                return;
            }

            //layerCount > 0时，减少层数量，重新计时buff
            self.ApplyModifierLayers(-1);
            --self.LayerCount;
            if (self.LayerCount > 0)
            {
                self.RefreshDuration();
                return;
            }
            //移除Buff
            self.RemoveCurrentBuff(EBuffRemoveReason.Expire);
        }
        public static void InitBuff(this Buff self)
        {
            self.ApplyStartEffect();
            self.RefreshDuration();
        }

        /// <summary>
        /// 触发buff行为
        /// </summary>
        /// <param name="self"></param>
        public static void TriggerBuff(this Buff self)
        {
            self.ApplyTriggerEffect();
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
