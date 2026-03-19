
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{

    [EntitySystemOf(typeof(BulletComponent))]
    [FriendOf(typeof(BulletComponent))]
    public static partial class BulletComponentSystem
    {
        [EntitySystem]
        public static void Destroy(this BulletComponent self)
        {
            self.HitActionEventIds.Clear();
        }
        [EntitySystem]
        public static void Awake(this BulletComponent self)
        {
            self.EndTime = long.MaxValue;
        }

        public static void Init(this BulletComponent self, Skill skill, Unit owner)
        {
            self.Init(skill, owner, null, 1000);
        }

        public static void Init(this BulletComponent self, Skill skill, Unit owner, List<int> hitActionEventIds)
        {
            self.Init(skill, owner, hitActionEventIds, 1000);
        }

        public static void Init(this BulletComponent self, Skill skill, Unit owner, List<int> hitActionEventIds, int lifeMs)
        {
            self.OwnerSkill = skill;
            self.OwnerUnit = owner;
            self.EndTime = TimeInfo.Instance.ServerNow() + (lifeMs > 0 ? lifeMs : 1000);
            self.HitActionEventIds.Clear();
            if (hitActionEventIds == null || hitActionEventIds.Count == 0)
            {
                return;
            }

            for (int index = 0; index < hitActionEventIds.Count; ++index)
            {
                int actionEventId = hitActionEventIds[index];
                if (actionEventId > 0)
                {
                    self.HitActionEventIds.Add(actionEventId);
                }
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="self"></param>
        public static void FixedUpdate(this BulletComponent self)
        {
            if (TimeInfo.Instance.ServerNow() > self.EndTime)
            {
                self.Root().GetComponent<UnitComponent>()?.Remove(self.GetParent<Unit>().Id);
            }
        }

    }
}
