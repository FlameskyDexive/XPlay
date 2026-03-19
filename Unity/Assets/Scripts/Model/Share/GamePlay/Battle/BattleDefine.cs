using System;
using Unity.Mathematics;

namespace ET
{
    public static partial class BattleDefine
    {
    }

    public enum EActionEventSourceType
    {
        Skill = 0,
        Buff = 1,
        Bullet = 2,
    }

    /*/// <summary>
    /// 技能事件类型
    /// </summary>
    public enum EActionEventType : byte
    {
        /// <summary>
        /// 范围伤害
        /// </summary>
        RangeDamage = 1,
        /// <summary>
        /// 子弹
        /// </summary>
        Bullet = 2,
        /// <summary>
        /// 添加buff
        /// </summary>
        AddBuff = 3,
        /// <summary>
        /// 移除buff
        /// </summary>
        RemoveBuff = 4,
        /// <summary>
        /// 隐身
        /// </summary>
        Stealth = 5,
    }

    /// <summary>
    /// 技能抽象类型
    /// </summary>
    public enum ESkillAbstractType : byte
    {
        /// <summary>
        /// 普攻
        /// </summary>
        NormalAttack = 1,
        /// <summary>
        /// 主动技能
        /// </summary>
        ActiveSkill = 2,
        /// <summary>
        /// 被动技能
        /// </summary>
        PassiveSkill = 3,
        /// <summary>
        /// 武器技能
        /// </summary>
        WeaponSkill = 4,
        /// <summary>
        /// 坐骑技能
        /// </summary>
        MountSkill = 5,
    }
    */

    /// <summary>
    /// 输入操作类型
    /// </summary>
    public enum EInputType : byte
    {
        Key,
        KeyDown,
        KeyUp,
    }

    public enum EOperateStatus : byte
    {
        Success = 0,
        Error = 1,
    }

    public enum EOperateType : byte
    {
        Move = 0,
        Jump = 1,
        Attack = 2,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
    }

    /*public enum EColliderType: byte
    {
        Circle,
        Box,
    }*/

    public enum EHitFromType : byte
    {
        /// <summary>
        /// 普通技能命中（范围伤害等等）
        /// </summary>
        Skill_Normal,
        /// <summary>
        /// 子弹技能命中
        /// </summary>
        Skill_Bullet,
        /// <summary>
        /// buff伤害
        /// </summary>
        Buff,
    }

    public enum EHitResultType : byte
    {
        /// <summary>
        /// 伤害扣血
        /// </summary>
        Damage,
        /// <summary>
        /// 回血
        /// </summary>
        RecoverBlood,
        /// <summary>
        /// 闪避
        /// </summary>
        Doge,
        /// <summary>
        /// 暴击
        /// </summary>
        Crit,
    }

    public enum ECombatState : byte
    {
        None = 0,
        Idle = 1,
        Casting = 2,
        Dead = 3,
    }

    public enum ECombatSubState : byte
    {
        None = 0,
        Idle = 1,
        Request = 2,
        CastPoint = 3,
        ActiveWindow = 4,
        Recover = 5,
        Dead = 6,
    }

    [Flags]
    public enum ECombatTag : long
    {
        None = 0,
        Silence = 1L << 0,
        SoftControl = 1L << 1,
        HardControl = 1L << 2,
        SuperArmor = 1L << 3,
        Dead = 1L << 4,
    }

    public enum ESkillCastResult : byte
    {
        Success = 0,
        SkillNotFound = 1,
        InCd = 2,
        Dead = 3,
        InvalidState = 4,
        NoTarget = 5,
        Controlled = 6,
        OutOfRange = 7,
        BlockedByTag = 8,
    }

    public enum EInterruptLevel : byte
    {
        None = 0,
        Soft = 1,
        Hard = 2,
        Fatal = 3,
    }

    public enum EActionEventTargetRule : byte
    {
        Self = 0,
        CurrentTarget = 1,
        CurrentOrSelf = 2,
        ExplicitTarget = 3,
    }

    public struct SkillCastRequest
    {
        public int SkillSlot;
        public int SkillId;
        public long TargetUnitId;
        public float3 AimPoint;
        public float3 AimDirection;
        public int ClientCastSeq;
        public long PressedTime;
    }

    public struct BuffApplyRequest
    {
        public int BuffId;
        public long SourceUnitId;
        public int SourceSkillConfigId;
    }
}
