namespace ET
{
    public static class BTCombatNodeTypes
    {
        public const string FindCombatTarget = "combat.action.find_target";
        public const string ValidateCombatTarget = "combat.condition.validate_target";
        public const string ClearInvalidTarget = "combat.action.clear_invalid_target";
        public const string SetCombatState = "combat.action.set_state";
        public const string StopMove = "combat.action.stop_move";
        public const string MoveToCombatRange = "combat.action.move_to_range";
        public const string RetreatFromCombatTarget = "combat.action.retreat_from_target";
        public const string FaceTarget = "combat.action.face_target";
        public const string SelectSkill = "combat.action.select_skill";
        public const string CheckStateChangeResult = "combat.condition.check_state_change_result";
        public const string CanCastSelectedSkill = "combat.condition.can_cast_selected_skill";
        public const string CastSelectedSkill = "combat.action.cast_selected_skill";
        public const string WaitCastComplete = "combat.action.wait_cast_complete";
        public const string InControl = "combat.condition.in_control";
        public const string NeedRetreat = "combat.condition.need_retreat";
    }

    public static class BTCombatBlackboardKeys
    {
        public const string TargetId = "Combat.TargetId";
        public const string TargetDistance = "Combat.TargetDistance";
        public const string HasTarget = "Combat.HasTarget";
        public const string SelectedSkillId = "Combat.SelectedSkillId";
        public const string SelectedSkillSlot = "Combat.SelectedSkillSlot";
        public const string CanCast = "Combat.CanCast";
        public const string InCast = "Combat.InCast";
        public const string InControl = "Combat.InControl";
        public const string HpRatio = "Combat.HpRatio";
        public const string IsDead = "Combat.IsDead";
        public const string NeedRetreat = "Combat.NeedRetreat";
        public const string StateChangeResult = "Combat.StateChangeResult";
    }
}
