using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(CombatStateComponent))]
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

            CombatStateComponent combatStateComponent = self.GetParent<Unit>()?.GetComponent<CombatStateComponent>();
            if (combatStateComponent != null && buffConfig.TagBlockMask != 0 && combatStateComponent.HasAnyTag((ECombatTag)buffConfig.TagBlockMask))
            {
                Log.Info($"buff apply blocked by tags unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{request.BuffId} blockMask:{buffConfig.TagBlockMask} currentTags:{combatStateComponent.TagMask}");
                return false;
            }

            self.RemoveConflictingGroupBuffs(buffConfig.Goup, request.BuffId);

            if (!self.BuffDic.TryGetValue(request.BuffId, out EntityRef<Buff> buffRef) || buffRef == null)
            {
                Buff buff = self.AddChild<Buff, BuffApplyRequest>(request);
                self.BuffDic[request.BuffId] = buff;
                return true;
            }

            Buff existBuff = buffRef;
            if (existBuff == null || existBuff.IsDisposed)
            {
                self.BuffDic.Remove(request.BuffId);
                return self.AddBuff(request);
            }

            existBuff.UpdateStrengthScore();
            uint maxLayer = (uint)(buffConfig.MaxLayer > 0 ? buffConfig.MaxLayer : 1);

            switch (buffConfig.AddPolicy)
            {
                case EBuffAddPolicy.RejectNew:
                    Log.Info($"buff apply rejected by add policy unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{request.BuffId} policy:{buffConfig.AddPolicy}");
                    return false;
                case EBuffAddPolicy.ReplaceByStronger:
                {
                    long incomingStrength = BuffPolicyHelper.CalculateStrengthScore(buffConfig, self.GetParent<Unit>());
                    if (incomingStrength <= existBuff.StrengthScore)
                    {
                        Log.Info($"buff apply rejected by stronger existing buff unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{request.BuffId} incoming:{incomingStrength} existing:{existBuff.StrengthScore}");
                        return false;
                    }

                    Log.Info($"buff replace by stronger unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{request.BuffId} incoming:{incomingStrength} existing:{existBuff.StrengthScore}");
                    self.RemoveBuffInternal(request.BuffId, EBuffRemoveReason.Remove);
                    return self.AddBuff(request);
                }
                case EBuffAddPolicy.RefreshDuration:
                    UpdateBuffSource(existBuff, request, buffConfig.Goup);
                    existBuff.RefreshDuration();
                    self.BuffDic[request.BuffId] = existBuff;
                    return true;
                case EBuffAddPolicy.StackOnly:
                    UpdateBuffSource(existBuff, request, buffConfig.Goup);
                    if (existBuff.LayerCount < maxLayer)
                    {
                        ++existBuff.LayerCount;
                        existBuff.ApplyModifierLayers(1);
                        existBuff.UpdateStrengthScore();
                    }

                    self.BuffDic[request.BuffId] = existBuff;
                    return true;
                case EBuffAddPolicy.StackAndRefresh:
                default:
                    UpdateBuffSource(existBuff, request, buffConfig.Goup);
                    if (existBuff.LayerCount < maxLayer)
                    {
                        ++existBuff.LayerCount;
                        existBuff.ApplyModifierLayers(1);
                        existBuff.UpdateStrengthScore();
                    }

                    existBuff.RefreshDuration();
                    self.BuffDic[request.BuffId] = existBuff;
                    return true;
            }
        }

        private static void UpdateBuffSource(Buff buff, BuffApplyRequest request, int groupId)
        {
            if (request.SourceUnitId != 0)
            {
                buff.SourceUnitId = request.SourceUnitId;
            }

            if (request.SourceSkillConfigId != 0)
            {
                buff.SourceSkillConfigId = request.SourceSkillConfigId;
            }

            buff.GroupId = groupId;
        }

        public static bool RemoveBuff(this BuffComponent self, int buffId = 0)
        {
            if (!self.BuffDic.TryGetValue(buffId, out EntityRef<Buff> buffRef))
            {
                return false;
            }

            Buff buff = buffRef;
            if (buff == null)
            {
                self.BuffDic.Remove(buffId);
                return false;
            }

            if (!CanRemoveByPolicy(buff.BuffConfig))
            {
                Log.Info($"buff remove blocked by remove policy unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{buffId} policy:{buff.BuffConfig?.RemovePolicy}");
                return false;
            }

            return self.RemoveBuffInternal(buffId, EBuffRemoveReason.Remove);
        }

        public static bool DispelBuff(this BuffComponent self, int buffId = 0)
        {
            if (!self.BuffDic.TryGetValue(buffId, out EntityRef<Buff> buffRef))
            {
                return false;
            }

            Buff buff = buffRef;
            if (buff == null)
            {
                self.BuffDic.Remove(buffId);
                return false;
            }

            if (!CanDispelByPolicy(buff.BuffConfig))
            {
                Log.Info($"buff dispel blocked by policy unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{buffId} policy:{buff.BuffConfig?.RemovePolicy} canDispel:{buff.BuffConfig?.CanDispel}");
                return false;
            }

            Log.Info($"buff dispel request accepted unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{buffId}");
            return self.RemoveBuffInternal(buffId, EBuffRemoveReason.Dispel);
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
                self.RemoveBuffInternal(buffIds[index], EBuffRemoveReason.Remove);
            }
        }

        public static void ClearAllBuffsOnDeath(this BuffComponent self)
        {
            using ListComponent<int> buffIds = ListComponent<int>.Create();
            foreach ((int buffId, EntityRef<Buff> buffRef) in self.BuffDic)
            {
                Buff buff = buffRef;
                if (buff == null)
                {
                    buffIds.Add(buffId);
                    continue;
                }

                if (buff.BuffConfig?.KeepOnDeath ?? false)
                {
                    continue;
                }

                buffIds.Add(buffId);
            }

            for (int index = 0; index < buffIds.Count; ++index)
            {
                self.RemoveBuffInternal(buffIds[index], EBuffRemoveReason.Remove);
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
                self.RemoveBuffInternal(removeIds[index], EBuffRemoveReason.Remove);
            }
        }

        private static bool RemoveBuffInternal(this BuffComponent self, int buffId, EBuffRemoveReason removeReason)
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

            if (buff.RemoveReason == EBuffRemoveReason.None)
            {
                buff.RemoveReason = removeReason;
            }

            Log.Info($"buff remove internal unit:{self.GetParent<Unit>()?.Id ?? 0} buff:{buffId} reason:{buff.RemoveReason}");

            self.Remove(buff.Id);
            return true;
        }

        private static bool CanRemoveByPolicy(BuffConfig buffConfig)
        {
            return buffConfig != null && buffConfig.RemovePolicy == EBuffRemovePolicy.RemoveOrDispel;
        }

        private static bool CanDispelByPolicy(BuffConfig buffConfig)
        {
            return buffConfig != null
                && buffConfig.RemovePolicy != EBuffRemovePolicy.ExpireOnly
                && buffConfig.CanDispel;
        }
    }
}
