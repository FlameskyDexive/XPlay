using System.Collections.Generic;

namespace ET
{
    [BTNodeDescriptor]
    public sealed class BTFindCombatTargetNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.find_target";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Find Target";

        public override string HandlerName => "BTFindCombatTarget";

        public override string Description => "Finds the nearest valid combat target and writes it into the combat blackboard keys.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "maxRange",
                DisplayName = "Max Range",
                ValueType = BTValueType.Float,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Float,
                    FloatValue = 30f,
                },
            },
        };
    }

    [BTNodeDescriptor]
    public sealed class BTValidateCombatTargetNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.condition.validate_target";

        public override BTNodeKind NodeKind => BTNodeKind.Condition;

        public override string MenuPath => "Conditions/Combat/Validate Target";

        public override string HandlerName => "BTValidateCombatTarget";

        public override string Description => "Checks whether the current combat target still exists, is alive, and optionally remains inside the desired distance.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "maxRange",
                DisplayName = "Max Range",
                ValueType = BTValueType.Float,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Float,
                    FloatValue = 0f,
                },
            },
        };
    }

    [BTNodeDescriptor]
    public sealed class BTClearInvalidTargetNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.clear_invalid_target";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Clear Invalid Target";

        public override string HandlerName => "BTClearInvalidTarget";

        public override string Description => "Clears the current combat target when the target is no longer valid.";
    }

    [BTNodeDescriptor]
    public sealed class BTMoveToCombatRangeNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.move_to_range";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Move To Range";

        public override string HandlerName => "BTMoveToCombatRange";

        public override string Description => "Moves the unit toward the current combat target until the configured combat range is reached.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "range",
                DisplayName = "Range",
                ValueType = BTValueType.Float,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Float,
                    FloatValue = 2.5f,
                },
            },
            new()
            {
                Name = "tickIntervalMs",
                DisplayName = "Tick Interval",
                ValueType = BTValueType.Integer,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Integer,
                    IntValue = 100,
                },
            },
        };
    }

    [BTNodeDescriptor]
    public sealed class BTFaceTargetNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.face_target";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Face Target";

        public override string HandlerName => "BTFaceTarget";

        public override string Description => "Rotates the unit to face the current combat target.";
    }

    [BTNodeDescriptor]
    public sealed class BTSelectSkillNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.select_skill";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Select Skill";

        public override string HandlerName => "BTSelectSkill";

        public override string Description => "Selects a castable skill and writes the chosen skill id and slot into the combat blackboard.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "preferredSlot",
                DisplayName = "Preferred Slot",
                ValueType = BTValueType.Integer,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Integer,
                    IntValue = -1,
                },
            },
        };
    }

    [BTNodeDescriptor]
    public sealed class BTCanCastSelectedSkillNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.condition.can_cast_selected_skill";

        public override BTNodeKind NodeKind => BTNodeKind.Condition;

        public override string MenuPath => "Conditions/Combat/Can Cast Selected Skill";

        public override string HandlerName => "BTCanCastSelectedSkill";

        public override string Description => "Validates whether the selected skill can currently be cast by the unit.";
    }

    [BTNodeDescriptor]
    public sealed class BTCastSelectedSkillNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.cast_selected_skill";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Cast Selected Skill";

        public override string HandlerName => "BTCastSelectedSkill";

        public override string Description => "Builds a combat cast request from blackboard state and sends it through the unified service-authoritative cast entry.";
    }

    [BTNodeDescriptor]
    public sealed class BTWaitCastCompleteNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.action.wait_cast_complete";

        public override BTNodeKind NodeKind => BTNodeKind.Action;

        public override string MenuPath => "Behaviors/Combat/Wait Cast Complete";

        public override string HandlerName => "BTWaitCastComplete";

        public override string Description => "Waits until the unit finishes its current cast or until the timeout expires.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "timeoutMs",
                DisplayName = "Timeout",
                ValueType = BTValueType.Integer,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Integer,
                    IntValue = 5000,
                },
            },
            new()
            {
                Name = "pollIntervalMs",
                DisplayName = "Poll Interval",
                ValueType = BTValueType.Integer,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Integer,
                    IntValue = 100,
                },
            },
        };
    }

    [BTNodeDescriptor]
    public sealed class BTNeedRetreatNodeDescriptor : ABTNodeDescriptor
    {
        public override string TypeId => "combat.condition.need_retreat";

        public override BTNodeKind NodeKind => BTNodeKind.Condition;

        public override string MenuPath => "Conditions/Combat/Need Retreat";

        public override string HandlerName => "BTNeedRetreat";

        public override string Description => "Returns success when the unit hp ratio is below the configured retreat threshold.";

        public override IReadOnlyList<BTNodeParameterDefinition> Parameters => new List<BTNodeParameterDefinition>
        {
            new()
            {
                Name = "hpRatioThreshold",
                DisplayName = "HP Ratio Threshold",
                ValueType = BTValueType.Float,
                DefaultValue = new BTSerializedValue
                {
                    ValueType = BTValueType.Float,
                    FloatValue = 0.3f,
                },
            },
        };
    }
}
