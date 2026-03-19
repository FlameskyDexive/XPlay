using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class BTDemoExportEditor
    {
        [MenuItem("ET/AI/Export Demo AITest.bytes", false, 1008)]
        public static void ExportDemoAITest()
        {
            byte[] bytes = CreateAITestBytes();
            string clientFilePath = Path.Combine(BTBytesLoader.ClientBehaviorTreeBytesDir, "AITest.bytes");
            string clientDirectory = Path.GetDirectoryName(clientFilePath);
            if (!Directory.Exists(clientDirectory))
            {
                Directory.CreateDirectory(clientDirectory);
            }

            string serverFilePath = Path.Combine(BTBytesLoader.ServerBehaviorTreeBytesDir, "AITest.bytes");
            string serverDirectory = Path.GetDirectoryName(serverFilePath);
            if (!Directory.Exists(serverDirectory))
            {
                Directory.CreateDirectory(serverDirectory);
            }

            File.WriteAllBytes(clientFilePath, bytes);
            File.WriteAllBytes(serverFilePath, bytes);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("BehaviorTree", $"Exported demo files:\n{clientFilePath}\n{serverFilePath}", "OK");
        }

        private static byte[] CreateAITestBytes()
        {
            BTAsset asset = ScriptableObject.CreateInstance<BTAsset>();
            asset.name = "AITest";
            asset.TreeId = "demo.shared.ai_test";
            asset.TreeName = "AITest";
            asset.Description = "Shared client/server combat demo behavior tree.";
            asset.EnsureInitialized();
            asset.Nodes.Clear();
            asset.BlackboardEntries.Clear();

            BTEditorNodeData root = CreateNode("root", "Root", BTNodeKind.Root, new Vector2(480, 60));
            root.ChildIds.Add("repeat");

            BTEditorNodeData repeat = CreateNode("repeat", "Repeat", BTNodeKind.Repeater, new Vector2(480, 180));
            repeat.ChildIds.Add("main_selector");

            BTEditorNodeData mainSelector = CreateNode("main_selector", "Combat Main", BTNodeKind.Selector, new Vector2(480, 320));
            mainSelector.ChildIds.Add("control_sequence");
            mainSelector.ChildIds.Add("retreat_sequence");
            mainSelector.ChildIds.Add("combat_sequence");
            mainSelector.ChildIds.Add("idle_wait");

            BTEditorNodeData controlSequence = CreateNode("control_sequence", "Control Pause", BTNodeKind.Sequence, new Vector2(120, 500));
            controlSequence.ChildIds.Add("in_control");
            controlSequence.ChildIds.Add("stop_move_control");
            controlSequence.ChildIds.Add("control_wait");

            BTEditorNodeData inControl = CreateConditionNode("in_control", "In Control", "combat.condition.in_control", "BTInControl", new Vector2(40, 680));
            BTEditorNodeData stopMoveControl = CreateActionNode("stop_move_control", "Stop Move", "combat.action.stop_move", "BTStopMove", new Vector2(180, 680));
            BTEditorNodeData controlWait = CreateWaitNode("control_wait", "Control Wait", 200, new Vector2(320, 680));

            BTEditorNodeData retreatSequence = CreateNode("retreat_sequence", "Retreat", BTNodeKind.Sequence, new Vector2(480, 500));
            retreatSequence.ChildIds.Add("need_retreat");
            retreatSequence.ChildIds.Add("retreat_target_selector");
            retreatSequence.ChildIds.Add("retreat_action");

            BTEditorNodeData needRetreat = CreateConditionNode("need_retreat", "Need Retreat", "combat.condition.need_retreat", "BTNeedRetreat", new Vector2(380, 680));
            needRetreat.Arguments.Add(CreateFloatArgument("hpRatioThreshold", 0.3f));

            BTEditorNodeData retreatTargetSelector = CreateNode("retreat_target_selector", "Retreat Target", BTNodeKind.Selector, new Vector2(560, 680));
            retreatTargetSelector.ChildIds.Add("retreat_validate_target");
            retreatTargetSelector.ChildIds.Add("retreat_find_target_sequence");

            BTEditorNodeData retreatValidateTarget = CreateConditionNode("retreat_validate_target", "Validate Retreat Target", "combat.condition.validate_target", "BTValidateCombatTarget", new Vector2(480, 860));
            retreatValidateTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));

            BTEditorNodeData retreatFindTargetSequence = CreateNode("retreat_find_target_sequence", "Acquire Retreat Target", BTNodeKind.Sequence, new Vector2(680, 860));
            retreatFindTargetSequence.ChildIds.Add("clear_invalid_target_retreat");
            retreatFindTargetSequence.ChildIds.Add("find_target_retreat");

            BTEditorNodeData clearInvalidTargetRetreat = CreateActionNode("clear_invalid_target_retreat", "Clear Invalid Target", "combat.action.clear_invalid_target", "BTClearInvalidTarget", new Vector2(620, 1040));
            BTEditorNodeData findTargetRetreat = CreateActionNode("find_target_retreat", "Find Target", "combat.action.find_target", "BTFindCombatTarget", new Vector2(780, 1040));
            findTargetRetreat.Arguments.Add(CreateFloatArgument("maxRange", 25f));

            BTEditorNodeData retreatAction = CreateActionNode("retreat_action", "Retreat From Target", "combat.action.retreat_from_target", "BTRetreatFromCombatTarget", new Vector2(820, 680));
            retreatAction.Arguments.Add(CreateFloatArgument("range", 6f));
            retreatAction.Arguments.Add(CreateIntArgument("tickIntervalMs", 100));
            retreatAction.Arguments.Add(CreateIntArgument("timeoutMs", 3000));

            BTEditorNodeData combatSequence = CreateNode("combat_sequence", "Combat", BTNodeKind.Sequence, new Vector2(900, 500));
            combatSequence.ChildIds.Add("combat_target_selector");
            combatSequence.ChildIds.Add("move_to_range");
            combatSequence.ChildIds.Add("stop_move_before_cast");
            combatSequence.ChildIds.Add("face_target");
            combatSequence.ChildIds.Add("select_skill");
            combatSequence.ChildIds.Add("can_cast");
            combatSequence.ChildIds.Add("cast_skill");
            combatSequence.ChildIds.Add("wait_cast_complete");

            BTEditorNodeData combatTargetSelector = CreateNode("combat_target_selector", "Acquire Target", BTNodeKind.Selector, new Vector2(980, 680));
            combatTargetSelector.ChildIds.Add("validate_target");
            combatTargetSelector.ChildIds.Add("find_target_sequence");

            BTEditorNodeData validateTarget = CreateConditionNode("validate_target", "Validate Target", "combat.condition.validate_target", "BTValidateCombatTarget", new Vector2(900, 860));
            validateTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));

            BTEditorNodeData findTargetSequence = CreateNode("find_target_sequence", "Find Target Sequence", BTNodeKind.Sequence, new Vector2(1100, 860));
            findTargetSequence.ChildIds.Add("clear_invalid_target");
            findTargetSequence.ChildIds.Add("find_target");

            BTEditorNodeData clearInvalidTarget = CreateActionNode("clear_invalid_target", "Clear Invalid Target", "combat.action.clear_invalid_target", "BTClearInvalidTarget", new Vector2(1040, 1040));
            BTEditorNodeData findTarget = CreateActionNode("find_target", "Find Target", "combat.action.find_target", "BTFindCombatTarget", new Vector2(1200, 1040));
            findTarget.Arguments.Add(CreateFloatArgument("maxRange", 25f));

            BTEditorNodeData moveToRange = CreateActionNode("move_to_range", "Move To Range", "combat.action.move_to_range", "BTMoveToCombatRange", new Vector2(1280, 680));
            moveToRange.Arguments.Add(CreateFloatArgument("range", 2.5f));
            moveToRange.Arguments.Add(CreateIntArgument("tickIntervalMs", 100));

            BTEditorNodeData stopMoveBeforeCast = CreateActionNode("stop_move_before_cast", "Stop Move", "combat.action.stop_move", "BTStopMove", new Vector2(1420, 680));
            BTEditorNodeData faceTarget = CreateActionNode("face_target", "Face Target", "combat.action.face_target", "BTFaceTarget", new Vector2(1560, 680));

            BTEditorNodeData selectSkill = CreateActionNode("select_skill", "Select Skill", "combat.action.select_skill", "BTSelectSkill", new Vector2(1700, 680));
            selectSkill.Arguments.Add(CreateIntArgument("preferredSlot", -1));

            BTEditorNodeData canCast = CreateConditionNode("can_cast", "Can Cast", "combat.condition.can_cast_selected_skill", "BTCanCastSelectedSkill", new Vector2(1840, 680));
            BTEditorNodeData castSkill = CreateActionNode("cast_skill", "Cast Skill", "combat.action.cast_selected_skill", "BTCastSelectedSkill", new Vector2(1980, 680));

            BTEditorNodeData waitCastComplete = CreateActionNode("wait_cast_complete", "Wait Cast Complete", "combat.action.wait_cast_complete", "BTWaitCastComplete", new Vector2(2120, 680));
            waitCastComplete.Arguments.Add(CreateIntArgument("timeoutMs", 5000));
            waitCastComplete.Arguments.Add(CreateIntArgument("pollIntervalMs", 100));

            BTEditorNodeData idleWait = CreateWaitNode("idle_wait", "Idle Wait", 250, new Vector2(1240, 500));

            asset.Nodes.Add(root);
            asset.Nodes.Add(repeat);
            asset.Nodes.Add(mainSelector);
            asset.Nodes.Add(controlSequence);
            asset.Nodes.Add(inControl);
            asset.Nodes.Add(stopMoveControl);
            asset.Nodes.Add(controlWait);
            asset.Nodes.Add(retreatSequence);
            asset.Nodes.Add(needRetreat);
            asset.Nodes.Add(retreatTargetSelector);
            asset.Nodes.Add(retreatValidateTarget);
            asset.Nodes.Add(retreatFindTargetSequence);
            asset.Nodes.Add(clearInvalidTargetRetreat);
            asset.Nodes.Add(findTargetRetreat);
            asset.Nodes.Add(retreatAction);
            asset.Nodes.Add(combatSequence);
            asset.Nodes.Add(combatTargetSelector);
            asset.Nodes.Add(validateTarget);
            asset.Nodes.Add(findTargetSequence);
            asset.Nodes.Add(clearInvalidTarget);
            asset.Nodes.Add(findTarget);
            asset.Nodes.Add(moveToRange);
            asset.Nodes.Add(stopMoveBeforeCast);
            asset.Nodes.Add(faceTarget);
            asset.Nodes.Add(selectSkill);
            asset.Nodes.Add(canCast);
            asset.Nodes.Add(castSkill);
            asset.Nodes.Add(waitCastComplete);
            asset.Nodes.Add(idleWait);
            asset.RootNodeId = root.NodeId;

            byte[] bytes = BTExporter.BuildBytes(asset);
            ScriptableObject.DestroyImmediate(asset);
            return bytes;
        }

        private static BTEditorNodeData CreateNode(string nodeId, string title, BTNodeKind nodeKind, Vector2 position)
        {
            return new BTEditorNodeData
            {
                NodeId = nodeId,
                NodeKind = nodeKind,
                Title = title,
                Position = new Rect(position.x, position.y, 220, 80),
            };
        }

        private static BTEditorNodeData CreateWaitNode(string nodeId, string title, int waitMilliseconds, Vector2 position)
        {
            BTEditorNodeData node = CreateNode(nodeId, title, BTNodeKind.Wait, position);
            node.WaitMilliseconds = waitMilliseconds;
            return node;
        }

        private static BTEditorNodeData CreateActionNode(string nodeId, string title, string nodeTypeId, string handlerName, Vector2 position)
        {
            BTEditorNodeData node = CreateNode(nodeId, title, BTNodeKind.Action, position);
            node.NodeTypeId = nodeTypeId;
            node.HandlerName = handlerName;
            return node;
        }

        private static BTEditorNodeData CreateConditionNode(string nodeId, string title, string nodeTypeId, string handlerName, Vector2 position)
        {
            BTEditorNodeData node = CreateNode(nodeId, title, BTNodeKind.Condition, position);
            node.NodeTypeId = nodeTypeId;
            node.HandlerName = handlerName;
            return node;
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
