using System;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(CombatAnimancerComponent))]
    [FriendOf(typeof(CombatAnimancerComponent))]
    public static partial class CombatAnimancerComponentSystem
    {
        private const int BaseLayerIndex = 0;
        private const int OverlayLayerIndex = 1;

        [EntitySystem]
        private static void Awake(this CombatAnimancerComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            GameObject go = unit.GetComponent<GameObjectComponent>()?.GameObject;
            if (go == null)
            {
                return;
            }

            AnimancerComponent animancer = go.GetComponent<AnimancerComponent>();
            if (animancer == null)
            {
                animancer = go.AddComponent<AnimancerComponent>();
            }

            Animator animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                animator = go.GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                return;
            }

            animancer.Animator = animator;
            animancer.Layers.SetMinCount(2);
            animancer.Layers.SetDebugName(BaseLayerIndex, "Base");
            animancer.Layers.SetDebugName(OverlayLayerIndex, "Overlay");

            self.Animancer = animancer;
            self.Animator = animator;
            self.ClipMap.Clear();

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller?.animationClips == null)
            {
                return;
            }

            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip == null || string.IsNullOrWhiteSpace(clip.name) || self.ClipMap.ContainsKey(clip.name))
                {
                    continue;
                }

                self.ClipMap.Add(clip.name, clip);
            }

            self.IdleClipName = self.ResolveClipName("Idle", "Stand");
            self.MoveClipName = self.ResolveClipName("Run", "Walk", "Move");
            self.CastPointClipName = self.ResolveClipName("Attack", "Cast", "Skill");
            self.CastActiveClipName = self.ResolveClipName("Skill", "Cast", "Attack");
            self.HitClipName = self.ResolveClipName("Damage", "Hit", "Hurt", "Knockback");
            self.DeadClipName = self.ResolveClipName("Death", "Die", "Dead");
        }

        [EntitySystem]
        private static void Destroy(this CombatAnimancerComponent self)
        {
            self.Animancer = null;
            self.Animator = null;
            self.ClipMap.Clear();
            self.IdleClipName = null;
            self.MoveClipName = null;
            self.CastPointClipName = null;
            self.CastActiveClipName = null;
            self.HitClipName = null;
            self.DeadClipName = null;
            self.CurrentBaseClipName = null;
            self.CurrentOverlayClipName = null;
            self.OverlayEndTime = 0f;
            self.OverlayPersistent = false;
        }

        public static bool IsReady(this CombatAnimancerComponent self)
        {
            return self != null && self.Animancer != null && self.Animator != null && self.ClipMap.Count > 0;
        }

        public static void TickOverlay(this CombatAnimancerComponent self)
        {
            if (!self.IsReady() || self.OverlayPersistent || string.IsNullOrEmpty(self.CurrentOverlayClipName))
            {
                return;
            }

            if (Time.time < self.OverlayEndTime)
            {
                return;
            }

            self.StopOverlay();
        }

        public static void PlayBaseState(this CombatAnimancerComponent self, ECombatAnimState animState, bool forceReplay = false)
        {
            AnimationClip clip = self.GetClip(animState);
            if (clip == null)
            {
                return;
            }

            if (!forceReplay && string.Equals(self.CurrentBaseClipName, clip.name, StringComparison.Ordinal))
            {
                return;
            }

            AnimancerState state = self.Animancer.Layers[BaseLayerIndex].Play(clip, 0.15f);
            if (forceReplay)
            {
                state.Time = 0f;
            }

            self.CurrentBaseClipName = clip.name;
        }

        public static void PlayOverlayState(this CombatAnimancerComponent self, ECombatAnimState animState, bool persistent = false)
        {
            AnimationClip clip = self.GetClip(animState);
            if (clip == null)
            {
                if (persistent)
                {
                    self.PlayBaseState(animState, true);
                }

                return;
            }

            AnimancerLayer overlayLayer = self.Animancer.Layers[OverlayLayerIndex];
            AnimancerState state = overlayLayer.Play(clip, 0.05f);
            state.Time = 0f;
            self.CurrentOverlayClipName = clip.name;
            self.OverlayPersistent = persistent;
            self.OverlayEndTime = persistent ? float.MaxValue : Time.time + Math.Max(clip.length, 0.05f);
        }

        public static void StopOverlay(this CombatAnimancerComponent self)
        {
            if (!self.IsReady())
            {
                return;
            }

            self.Animancer.Layers[OverlayLayerIndex].StartFade(0f, 0.1f);
            self.CurrentOverlayClipName = null;
            self.OverlayEndTime = 0f;
            self.OverlayPersistent = false;
        }

        private static AnimationClip GetClip(this CombatAnimancerComponent self, ECombatAnimState animState)
        {
            string clipName = animState switch
            {
                ECombatAnimState.Idle => self.IdleClipName,
                ECombatAnimState.Move => self.MoveClipName,
                ECombatAnimState.CastPoint => self.CastPointClipName,
                ECombatAnimState.CastActive => self.CastActiveClipName,
                ECombatAnimState.Hit => self.HitClipName,
                ECombatAnimState.Dead => self.DeadClipName,
                _ => null,
            };

            if (string.IsNullOrWhiteSpace(clipName))
            {
                return null;
            }

            self.ClipMap.TryGetValue(clipName, out AnimationClip clip);
            return clip;
        }

        private static string ResolveClipName(this CombatAnimancerComponent self, params string[] keywords)
        {
            if (self.ClipMap.Count == 0 || keywords == null || keywords.Length == 0)
            {
                return null;
            }

            foreach (string keyword in keywords)
            {
                foreach (KeyValuePair<string, AnimationClip> pair in self.ClipMap)
                {
                    if (string.Equals(pair.Key, keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Key;
                    }
                }
            }

            foreach (string keyword in keywords)
            {
                foreach (KeyValuePair<string, AnimationClip> pair in self.ClipMap)
                {
                    if (pair.Key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return pair.Key;
                    }
                }
            }

            return null;
        }
    }
}
