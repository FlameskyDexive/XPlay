namespace ET.Client
{
    public static class BTClientDemoFactory
    {
        public static byte[] CreateAITestBytes()
        {
            BTDefinition tree = new()
            {
                TreeId = "demo.shared.ai_test",
                TreeName = "AITest",
                Description = "Shared client/server combat demo behavior tree.",
                RootNodeId = "root",
            };

            tree.Nodes.Add(new BTRootNodeData
            {
                NodeId = "root",
                Title = "Root",
                ChildIds = { "repeat" },
            });

            tree.Nodes.Add(new BTRepeaterNodeData
            {
                NodeId = "repeat",
                Title = "Repeat",
                ChildIds = { "main_selector" },
            });

            tree.Nodes.Add(new BTSelectorNodeData
            {
                NodeId = "main_selector",
                Title = "Combat Main",
                ChildIds = { "control_sequence", "retreat_sequence", "combat_sequence", "idle_wait" },
            });

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "control_sequence",
                Title = "Control Pause",
                ChildIds = { "in_control", "stop_move_control", "control_wait" },
            });

            tree.Nodes.Add(CreateCondition("in_control", "In Control", BTCombatNodeTypes.InControl, nameof(BTInControl)));
            tree.Nodes.Add(CreateAction("stop_move_control", "Stop Move", BTCombatNodeTypes.StopMove, nameof(BTStopMove)));
            tree.Nodes.Add(new BTWaitNodeData
            {
                NodeId = "control_wait",
                Title = "Control Wait",
                WaitMilliseconds = 200,
            });

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "retreat_sequence",
                Title = "Retreat",
                ChildIds = { "need_retreat", "retreat_target_selector", "retreat_action" },
            });

            BTConditionNodeData needRetreat = CreateCondition("need_retreat", "Need Retreat", BTCombatNodeTypes.NeedRetreat, nameof(BTNeedRetreat));
            needRetreat.Arguments.Add(CreateFloatArgument("hpRatioThreshold", 0.3f));
            tree.Nodes.Add(needRetreat);

            tree.Nodes.Add(new BTSelectorNodeData
            {
                NodeId = "retreat_target_selector",
                Title = "Retreat Target",
                ChildIds = { "retreat_validate_target", "retreat_find_target_sequence" },
            });

            BTConditionNodeData retreatValidateTarget = CreateCondition("retreat_validate_target", "Validate Retreat Target", BTCombatNodeTypes.ValidateCombatTarget, nameof(BTValidateCombatTarget));
            retreatValidateTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));
            tree.Nodes.Add(retreatValidateTarget);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "retreat_find_target_sequence",
                Title = "Acquire Retreat Target",
                ChildIds = { "clear_invalid_target_retreat", "find_target_retreat" },
            });

            tree.Nodes.Add(CreateAction("clear_invalid_target_retreat", "Clear Invalid Target", BTCombatNodeTypes.ClearInvalidTarget, nameof(BTClearInvalidTarget)));

            BTActionNodeData findTargetRetreat = CreateAction("find_target_retreat", "Find Target", BTCombatNodeTypes.FindCombatTarget, nameof(BTFindCombatTarget));
            findTargetRetreat.Arguments.Add(CreateFloatArgument("maxRange", 25f));
            tree.Nodes.Add(findTargetRetreat);

            BTActionNodeData retreatAction = CreateAction("retreat_action", "Retreat From Target", BTCombatNodeTypes.RetreatFromCombatTarget, nameof(BTRetreatFromCombatTarget));
            retreatAction.Arguments.Add(CreateFloatArgument("range", 6f));
            retreatAction.Arguments.Add(CreateIntArgument("tickIntervalMs", 100));
            retreatAction.Arguments.Add(CreateIntArgument("timeoutMs", 3000));
            tree.Nodes.Add(retreatAction);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "combat_sequence",
                Title = "Combat",
                ChildIds = { "combat_target_selector", "move_to_range", "stop_move_before_cast", "face_target", "select_skill", "can_cast", "cast_skill", "wait_cast_complete" },
            });

            tree.Nodes.Add(new BTSelectorNodeData
            {
                NodeId = "combat_target_selector",
                Title = "Acquire Target",
                ChildIds = { "validate_target", "find_target_sequence" },
            });

            BTConditionNodeData validateTarget = CreateCondition("validate_target", "Validate Target", BTCombatNodeTypes.ValidateCombatTarget, nameof(BTValidateCombatTarget));
            validateTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));
            tree.Nodes.Add(validateTarget);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "find_target_sequence",
                Title = "Find Target Sequence",
                ChildIds = { "clear_invalid_target", "find_target" },
            });

            tree.Nodes.Add(CreateAction("clear_invalid_target", "Clear Invalid Target", BTCombatNodeTypes.ClearInvalidTarget, nameof(BTClearInvalidTarget)));

            BTActionNodeData findTarget = CreateAction("find_target", "Find Target", BTCombatNodeTypes.FindCombatTarget, nameof(BTFindCombatTarget));
            findTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));
            tree.Nodes.Add(findTarget);

            BTActionNodeData moveToRange = CreateAction("move_to_range", "Move To Range", BTCombatNodeTypes.MoveToCombatRange, nameof(BTMoveToCombatRange));
            moveToRange.Arguments.Add(CreateFloatArgument("range", 2.5f));
            moveToRange.Arguments.Add(CreateIntArgument("tickIntervalMs", 100));
            tree.Nodes.Add(moveToRange);

            tree.Nodes.Add(CreateAction("stop_move_before_cast", "Stop Move", BTCombatNodeTypes.StopMove, nameof(BTStopMove)));
            tree.Nodes.Add(CreateAction("face_target", "Face Target", BTCombatNodeTypes.FaceTarget, nameof(BTFaceTarget)));

            BTActionNodeData selectSkill = CreateAction("select_skill", "Select Skill", BTCombatNodeTypes.SelectSkill, nameof(BTSelectSkill));
            selectSkill.Arguments.Add(CreateIntArgument("preferredSlot", -1));
            tree.Nodes.Add(selectSkill);

            tree.Nodes.Add(CreateCondition("can_cast", "Can Cast", BTCombatNodeTypes.CanCastSelectedSkill, nameof(BTCanCastSelectedSkill)));
            tree.Nodes.Add(CreateAction("cast_skill", "Cast Skill", BTCombatNodeTypes.CastSelectedSkill, nameof(BTCastSelectedSkill)));

            BTActionNodeData waitCastComplete = CreateAction("wait_cast_complete", "Wait Cast Complete", BTCombatNodeTypes.WaitCastComplete, nameof(BTWaitCastComplete));
            waitCastComplete.Arguments.Add(CreateIntArgument("timeoutMs", 5000));
            waitCastComplete.Arguments.Add(CreateIntArgument("pollIntervalMs", 100));
            tree.Nodes.Add(waitCastComplete);

            tree.Nodes.Add(new BTWaitNodeData
            {
                NodeId = "idle_wait",
                Title = "Idle Wait",
                WaitMilliseconds = 250,
            });

            BTPackage package = new()
            {
                PackageId = tree.TreeId,
                PackageName = tree.TreeName,
                EntryTreeId = tree.TreeId,
                EntryTreeName = tree.TreeName,
                Trees = { tree },
            };

            return BTSerializer.Serialize(package);
        }

        private static BTActionNodeData CreateAction(string nodeId, string title, string typeId, string handlerName)
        {
            return new BTActionNodeData
            {
                NodeId = nodeId,
                Title = title,
                TypeId = typeId,
                ActionHandlerName = handlerName,
            };
        }

        private static BTConditionNodeData CreateCondition(string nodeId, string title, string typeId, string handlerName)
        {
            return new BTConditionNodeData
            {
                NodeId = nodeId,
                Title = title,
                TypeId = typeId,
                ConditionHandlerName = handlerName,
            };
        }

        private static BTArgumentData CreateIntArgument(string name, int value)
        {
            return new BTArgumentData
            {
                Name = name,
                Value = new BTSerializedValue
                {
                    ValueType = BTValueType.Integer,
                    IntValue = value,
                },
            };
        }

        private static BTArgumentData CreateFloatArgument(string name, float value)
        {
            return new BTArgumentData
            {
                Name = name,
                Value = new BTSerializedValue
                {
                    ValueType = BTValueType.Float,
                    FloatValue = value,
                },
            };
        }
    }
}
