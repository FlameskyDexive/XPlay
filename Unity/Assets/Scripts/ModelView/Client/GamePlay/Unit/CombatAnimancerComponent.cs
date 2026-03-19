using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class CombatAnimancerComponent : Entity, IAwake, IDestroy
    {
        public AnimancerComponent Animancer;
        public Animator Animator;
        public Dictionary<string, AnimationClip> ClipMap = new();

        public string IdleClipName;
        public string MoveClipName;
        public string CastPointClipName;
        public string CastActiveClipName;
        public string HitClipName;
        public string DeadClipName;

        public string CurrentBaseClipName;
        public string CurrentOverlayClipName;
        public float OverlayEndTime;
        public bool OverlayPersistent;
    }
}
