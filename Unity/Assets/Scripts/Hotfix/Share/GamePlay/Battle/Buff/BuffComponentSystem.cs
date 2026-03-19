using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    public static partial class BuffComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BuffComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BuffComponent self)
        {
            foreach (KeyValuePair<int, EntityRef<Buff>> valuePair in self.BuffDic)
            {
                Buff buff = valuePair.Value;
                if (buff == null)
                {
                    continue;
                }

                self.Remove(buff.Id);
            }

            self.BuffDic.Clear();
        }

        private static void AddBuffS(this BuffComponent self, List<int> buffIds)
        {
            foreach (int buffId in buffIds)
            {
                self.AddBuff(buffId);
            }
        }

        public static bool AddBuff(this BuffComponent self, int buffId = 0)
        {
            return self.AddBuff(new BuffApplyRequest { BuffId = buffId });
        }

        public static bool AddBuff(this BuffComponent self, BuffApplyRequest request)
        {
            if (request.BuffId <= 0)
            {
                return false;
            }

            BuffConfig buffConfig = BuffConfigCategory.Instance.GetOrDefault(request.BuffId);
            if (buffConfig == null)
            {
                return false;
            }

            self.RemoveConflictingGroupBuffs(buffConfig.Goup, request.BuffId);

            if (!self.BuffDic.TryGetValue(request.BuffId, out EntityRef<Buff> buffRef) || buffRef == null)
            {
                Buff buff = self.AddChild<Buff, int>(request.BuffId);
                buff.LayerCount = 1;
                buff.SourceUnitId = request.SourceUnitId;
                buff.SourceSkillConfigId = request.SourceSkillConfigId;
                buff.GroupId = buffConfig.Goup;
                self.BuffDic[request.BuffId] = buff;
                return true;
            }

            Buff existBuff = buffRef;
            if (existBuff == null || existBuff.IsDisposed)
            {
                self.BuffDic.Remove(request.BuffId);
                return self.AddBuff(request);
            }

            if (request.SourceUnitId != 0)
            {
                existBuff.SourceUnitId = request.SourceUnitId;
            }

            if (request.SourceSkillConfigId != 0)
            {
                existBuff.SourceSkillConfigId = request.SourceSkillConfigId;
            }

            existBuff.GroupId = buffConfig.Goup;
            uint maxLayer = (uint)(buffConfig.MaxLayer > 0 ? buffConfig.MaxLayer : 1);
            if (existBuff.LayerCount < maxLayer)
            {
                ++existBuff.LayerCount;
            }

            long now = TimeInfo.Instance.ServerNow();
            existBuff.StartTime = now;
            existBuff.NextTriggerTime = buffConfig.TriggerInterval > 0
                ? now + buffConfig.TriggerInterval
                : long.MaxValue;
            self.BuffDic[request.BuffId] = existBuff;
            return true;
        }

        public static bool RemoveBuff(this BuffComponent self, int buffId = 0)
        {
            if (!self.BuffDic.TryGetValue(buffId, out EntityRef<Buff> buffRef))
            {
                return false;
            }

            Buff buff = buffRef;
            self.BuffDic.Remove(buffId);
            if (buff == null)
            {
                return false;
            }

            self.Remove(buff.Id);
            return true;
        }

        public static void ClearAllBuffs(this BuffComponent self)
        {
            using ListComponent<int> buffIds = ListComponent<int>.Create();
            foreach ((int buffId, EntityRef<Buff> _) in self.BuffDic)
            {
                buffIds.Add(buffId);
            }

            for (int index = 0; index < buffIds.Count; ++index)
            {
                self.RemoveBuff(buffIds[index]);
            }
        }

        public static void Remove(this BuffComponent self, long id)
        {
            Buff buff = self.GetChild<Buff>(id);
            buff?.Dispose();
        }

        private static void RemoveConflictingGroupBuffs(this BuffComponent self, int groupId, int excludeBuffId)
        {
            if (groupId == 0)
            {
                return;
            }

            using ListComponent<int> removeIds = ListComponent<int>.Create();
            foreach ((int buffId, EntityRef<Buff> buffRef) in self.BuffDic)
            {
                Buff buff = buffRef;
                if (buff == null || buff.IsDisposed || buffId == excludeBuffId)
                {
                    continue;
                }

                if (buff.BuffConfig?.Goup == groupId)
                {
                    removeIds.Add(buffId);
                }
            }

            for (int index = 0; index < removeIds.Count; ++index)
            {
                self.RemoveBuff(removeIds[index]);
            }
        }
    }
}
