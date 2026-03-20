namespace ET.Client
{
    public static class BTClientDemoFactory
    {
        public static BTPackage CreateAITestPackage()
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
                ChildIds = { "in_control", "set_idle_control", "stop_move_control", "control_wait" },
            });

            tree.Nodes.Add(CreateCondition("in_control", "In Control", BTCombatNodeTypes.InControl, nameof(BTInControl)));
            tree.Nodes.Add(CreateCombatStateAction("set_idle_control", "Set Idle", ECombatSubState.Idle));
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
                ChildIds = { "need_retreat", "retreat_target_selector", "set_move_retreat", "retreat_action" },
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
            tree.Nodes.Add(CreateCombatStateAction("set_move_retreat", "Set Move", ECombatSubState.Move));
            tree.Nodes.Add(retreatAction);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "combat_sequence",
                Title = "Combat",
                ChildIds = { "combat_target_selector", "set_move_combat", "move_to_range", "select_skill", "cast_flow_selector" },
            });

            tree.Nodes.Add(new BTSelectorNodeData
            {
                NodeId = "cast_flow_selector",
                Title = "Cast Flow",
                ChildIds =
                {
                    "cast_execute_sequence",
                    "cast_retry_out_of_range",
                    "cast_retry_no_target",
                    "cast_retry_insufficient_mp",
                    "cast_retry_cd",
                    "cast_retry_controlled",
                    "cast_retry_blocked",
                },
            });

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_execute_sequence",
                Title = "Cast Execute",
                ChildIds =
                {
                    "set_idle_before_cast",
                    "stop_move_before_cast",
                    "face_target",
                    "set_cast_state",
                    "cast_skill",
                    "wait_cast_complete",
                },
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
            tree.Nodes.Add(CreateCombatStateAction("set_move_combat", "Set Move", ECombatSubState.Move));
            tree.Nodes.Add(moveToRange);

            tree.Nodes.Add(CreateCombatStateAction("set_idle_before_cast", "Set Idle", ECombatSubState.Idle));
            tree.Nodes.Add(CreateAction("stop_move_before_cast", "Stop Move", BTCombatNodeTypes.StopMove, nameof(BTStopMove)));
            tree.Nodes.Add(CreateAction("face_target", "Face Target", BTCombatNodeTypes.FaceTarget, nameof(BTFaceTarget)));
            tree.Nodes.Add(CreateCombatStateAction("set_cast_state", "Set CastPoint", ECombatSubState.CastPoint));

            BTActionNodeData selectSkill = CreateAction("select_skill", "Select Skill", BTCombatNodeTypes.SelectSkill, nameof(BTSelectSkill));
            selectSkill.Arguments.Add(CreateIntArgument("preferredSlot", -1));
            tree.Nodes.Add(selectSkill);

            tree.Nodes.Add(CreateCondition("can_cast", "Can Cast", BTCombatNodeTypes.CanCastSelectedSkill, nameof(BTCanCastSelectedSkill)));
            tree.Nodes.Add(CreateAction("cast_skill", "Cast Skill", BTCombatNodeTypes.CastSelectedSkill, nameof(BTCastSelectedSkill)));

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_out_of_range",
                Title = "Retry Out Of Range",
                ChildIds = { "check_result_out_of_range", "set_move_retry", "move_to_range_retry" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_out_of_range", "Result Out Of Range", ECombatStateChangeResult.OutOfRange));
            tree.Nodes.Add(CreateCombatStateAction("set_move_retry", "Set Move Retry", ECombatSubState.Move));
            BTActionNodeData moveToRangeRetry = CreateAction("move_to_range_retry", "Move To Range Retry", BTCombatNodeTypes.MoveToCombatRange, nameof(BTMoveToCombatRange));
            moveToRangeRetry.Arguments.Add(CreateFloatArgument("range", 2.5f));
            moveToRangeRetry.Arguments.Add(CreateIntArgument("tickIntervalMs", 100));
            tree.Nodes.Add(moveToRangeRetry);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_no_target",
                Title = "Retry No Target",
                ChildIds = { "check_result_no_target", "clear_invalid_target_retry", "find_target_retry" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_no_target", "Result No Target", ECombatStateChangeResult.NoTarget));
            tree.Nodes.Add(CreateAction("clear_invalid_target_retry", "Clear Invalid Target Retry", BTCombatNodeTypes.ClearInvalidTarget, nameof(BTClearInvalidTarget)));
            BTActionNodeData findTargetRetry = CreateAction("find_target_retry", "Find Target Retry", BTCombatNodeTypes.FindCombatTarget, nameof(BTFindCombatTarget));
            findTargetRetry.Arguments.Add(CreateFloatArgument("maxRange", 25f));
            tree.Nodes.Add(findTargetRetry);

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_insufficient_mp",
                Title = "Retry Insufficient Mp",
                ChildIds = { "check_result_insufficient_mp", "idle_wait" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_insufficient_mp", "Result Insufficient Mp", ECombatStateChangeResult.InsufficientMp));

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_cd",
                Title = "Retry In Cd",
                ChildIds = { "check_result_in_cd", "idle_wait" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_in_cd", "Result In Cd", ECombatStateChangeResult.InCd));

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_controlled",
                Title = "Retry Controlled",
                ChildIds = { "check_result_controlled", "control_wait" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_controlled", "Result Controlled", ECombatStateChangeResult.Controlled));

            tree.Nodes.Add(new BTSequenceNodeData
            {
                NodeId = "cast_retry_blocked",
                Title = "Retry Blocked",
                ChildIds = { "check_result_blocked", "control_wait" },
            });
            tree.Nodes.Add(CreateStateChangeResultCondition("check_result_blocked", "Result Blocked", ECombatStateChangeResult.BlockedByTag));

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

            return package;
        }

        public static byte[] CreateAITestBytes()
        {
            return BTSerializer.Serialize(CreateAITestPackage());
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

        private static BTActionNodeData CreateCombatStateAction(string nodeId, string title, ECombatSubState state)
        {
            BTActionNodeData actionNodeData = CreateAction(nodeId, title, BTCombatNodeTypes.SetCombatState, nameof(BTSetCombatState));
            actionNodeData.Arguments.Add(CreateIntArgument("state", (int)state));
            return actionNodeData;
        }

        private static BTConditionNodeData CreateStateChangeResultCondition(string nodeId, string title, ECombatStateChangeResult result)
        {
            BTConditionNodeData conditionNodeData = CreateCondition(nodeId, title, BTCombatNodeTypes.CheckStateChangeResult, nameof(BTCheckStateChangeResult));
            conditionNodeData.Arguments.Add(CreateIntArgument("result", (int)result));
            return conditionNodeData;
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
