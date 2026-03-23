using System.Collections.Generic;

namespace ET
{
    [BTCompiledTreeProvider]
    public sealed class BTCompiledProvider_ai_monster_01 : IBTCompiledTreeProvider
    {
        private const string PackageKeyValue = "ai_monster_01";
        private static BTCompiledTreeTemplate template;

        public string PackageKey => PackageKeyValue;
        public BTCompiledTreeTemplate Template => template ??= BuildTemplate();

        private static BTCompiledTreeTemplate BuildTemplate()
        {
            BTPackage package = new();
            package.PackageId = "5d422de7984f4c4ea0c5a5f1d9abfb26";
            package.PackageName = "ai_monster_01";
            package.EntryTreeId = "5d422de7984f4c4ea0c5a5f1d9abfb26";
            package.EntryTreeName = "ai_monster_01";
            BTDefinition package_Trees_0 = new();
            package_Trees_0.TreeId = "5d422de7984f4c4ea0c5a5f1d9abfb26";
            package_Trees_0.TreeName = "ai_monster_01";
            package_Trees_0.Description = "";
            package_Trees_0.RootNodeId = "fb8cdf410a644eb6b9a73d6a26fb975d";
            BTRootNodeData package_Trees_0_Nodes_0 = new();
            package_Trees_0_Nodes_0.NodeId = "fb8cdf410a644eb6b9a73d6a26fb975d";
            package_Trees_0_Nodes_0.Title = "Root";
            package_Trees_0_Nodes_0.NodeKind = BTNodeKind.Root;
            package_Trees_0_Nodes_0.ChildIds.Add("5a8b257bff56485ea14994e2eb6ba77e");
            package_Trees_0_Nodes_0.Comment = "";
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_0);
            BTRepeaterNodeData package_Trees_0_Nodes_1 = new();
            package_Trees_0_Nodes_1.NodeId = "5a8b257bff56485ea14994e2eb6ba77e";
            package_Trees_0_Nodes_1.Title = "Repeater";
            package_Trees_0_Nodes_1.NodeKind = BTNodeKind.Repeater;
            package_Trees_0_Nodes_1.ChildIds.Add("4c82ef7295d74935a3f92523120371a3");
            package_Trees_0_Nodes_1.Comment = "";
            package_Trees_0_Nodes_1.MaxLoopCount = 0;
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_1);
            BTSequenceNodeData package_Trees_0_Nodes_2 = new();
            package_Trees_0_Nodes_2.NodeId = "4c82ef7295d74935a3f92523120371a3";
            package_Trees_0_Nodes_2.Title = "Sequence";
            package_Trees_0_Nodes_2.NodeKind = BTNodeKind.Sequence;
            package_Trees_0_Nodes_2.ChildIds.Add("475e275efde24a498117b016508a5a79");
            package_Trees_0_Nodes_2.ChildIds.Add("c7f1f0a60d5b4208b9f6e6b1c45d5b2c");
            package_Trees_0_Nodes_2.ChildIds.Add("9e36eb277f4a40d185a4d4a10bdf012c");
            package_Trees_0_Nodes_2.Comment = "";
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_2);
            BTActionNodeData package_Trees_0_Nodes_3 = new();
            package_Trees_0_Nodes_3.NodeId = "9e36eb277f4a40d185a4d4a10bdf012c";
            package_Trees_0_Nodes_3.Title = "Move To Range";
            package_Trees_0_Nodes_3.NodeKind = BTNodeKind.Action;
            package_Trees_0_Nodes_3.Comment = "";
            package_Trees_0_Nodes_3.TypeId = "combat.action.move_to_range";
            package_Trees_0_Nodes_3.ActionHandlerName = "BTMoveToCombatRange";
            BTArgumentData package_Trees_0_Nodes_3_Arguments_0 = new();
            package_Trees_0_Nodes_3_Arguments_0.Name = "range";
            BTSerializedValue package_Trees_0_Nodes_3_Arguments_0_Value = new();
            package_Trees_0_Nodes_3_Arguments_0_Value.ValueType = BTValueType.Float;
            package_Trees_0_Nodes_3_Arguments_0_Value.IntValue = 0;
            package_Trees_0_Nodes_3_Arguments_0_Value.LongValue = 0L;
            package_Trees_0_Nodes_3_Arguments_0_Value.FloatValue = 2.5f;
            package_Trees_0_Nodes_3_Arguments_0_Value.BoolValue = false;
            package_Trees_0_Nodes_3_Arguments_0_Value.StringValue = "";
            package_Trees_0_Nodes_3_Arguments_0.Value = package_Trees_0_Nodes_3_Arguments_0_Value;
            package_Trees_0_Nodes_3.Arguments.Add(package_Trees_0_Nodes_3_Arguments_0);
            BTArgumentData package_Trees_0_Nodes_3_Arguments_1 = new();
            package_Trees_0_Nodes_3_Arguments_1.Name = "tickIntervalMs";
            BTSerializedValue package_Trees_0_Nodes_3_Arguments_1_Value = new();
            package_Trees_0_Nodes_3_Arguments_1_Value.ValueType = BTValueType.Integer;
            package_Trees_0_Nodes_3_Arguments_1_Value.IntValue = 100;
            package_Trees_0_Nodes_3_Arguments_1_Value.LongValue = 0L;
            package_Trees_0_Nodes_3_Arguments_1_Value.FloatValue = 0f;
            package_Trees_0_Nodes_3_Arguments_1_Value.BoolValue = false;
            package_Trees_0_Nodes_3_Arguments_1_Value.StringValue = "";
            package_Trees_0_Nodes_3_Arguments_1.Value = package_Trees_0_Nodes_3_Arguments_1_Value;
            package_Trees_0_Nodes_3.Arguments.Add(package_Trees_0_Nodes_3_Arguments_1);
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_3);
            BTActionNodeData package_Trees_0_Nodes_4 = new();
            package_Trees_0_Nodes_4.NodeId = "c7f1f0a60d5b4208b9f6e6b1c45d5b2c";
            package_Trees_0_Nodes_4.Title = "Find Target";
            package_Trees_0_Nodes_4.NodeKind = BTNodeKind.Action;
            package_Trees_0_Nodes_4.Comment = "";
            package_Trees_0_Nodes_4.TypeId = "combat.action.find_target";
            package_Trees_0_Nodes_4.ActionHandlerName = "BTFindCombatTarget";
            BTArgumentData package_Trees_0_Nodes_4_Arguments_0 = new();
            package_Trees_0_Nodes_4_Arguments_0.Name = "maxRange";
            BTSerializedValue package_Trees_0_Nodes_4_Arguments_0_Value = new();
            package_Trees_0_Nodes_4_Arguments_0_Value.ValueType = BTValueType.Float;
            package_Trees_0_Nodes_4_Arguments_0_Value.IntValue = 0;
            package_Trees_0_Nodes_4_Arguments_0_Value.LongValue = 0L;
            package_Trees_0_Nodes_4_Arguments_0_Value.FloatValue = 25f;
            package_Trees_0_Nodes_4_Arguments_0_Value.BoolValue = false;
            package_Trees_0_Nodes_4_Arguments_0_Value.StringValue = "";
            package_Trees_0_Nodes_4_Arguments_0.Value = package_Trees_0_Nodes_4_Arguments_0_Value;
            package_Trees_0_Nodes_4.Arguments.Add(package_Trees_0_Nodes_4_Arguments_0);
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_4);
            BTWaitNodeData package_Trees_0_Nodes_5 = new();
            package_Trees_0_Nodes_5.NodeId = "475e275efde24a498117b016508a5a79";
            package_Trees_0_Nodes_5.Title = "Wait";
            package_Trees_0_Nodes_5.NodeKind = BTNodeKind.Wait;
            package_Trees_0_Nodes_5.Comment = "";
            package_Trees_0_Nodes_5.WaitMilliseconds = 100;
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_5);
            package.Trees.Add(package_Trees_0);
            BTCompiledTreeTemplate templateValue = new(PackageKeyValue, package);
            BuildEntry_ai_monster_01(templateValue);
            return templateValue;
        }

        private static void BuildEntry_ai_monster_01(BTCompiledTreeTemplate template)
        {
            Dictionary<int, BTNode> nodes = new();
            int nextRuntimeNodeId = 1;
            BTDefinition tree_ai_monster_01 = BTCompiledTreeUtility.RequireTree(template.Package, "5d422de7984f4c4ea0c5a5f1d9abfb26", "ai_monster_01");
            BTNodeData definition_1_fb8cdf410a644eb6b9a73d6a26fb975d = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "fb8cdf410a644eb6b9a73d6a26fb975d");
            BTNode node_1_fb8cdf410a644eb6b9a73d6a26fb975d = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_1_fb8cdf410a644eb6b9a73d6a26fb975d), ref nextRuntimeNodeId, tree_ai_monster_01, definition_1_fb8cdf410a644eb6b9a73d6a26fb975d, nodes);
            BTNodeData definition_2__5a8b257bff56485ea14994e2eb6ba77e = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "5a8b257bff56485ea14994e2eb6ba77e");
            BTNode node_2__5a8b257bff56485ea14994e2eb6ba77e = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_2__5a8b257bff56485ea14994e2eb6ba77e), ref nextRuntimeNodeId, tree_ai_monster_01, definition_2__5a8b257bff56485ea14994e2eb6ba77e, nodes);
            BTNodeData definition_3__4c82ef7295d74935a3f92523120371a3 = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "4c82ef7295d74935a3f92523120371a3");
            BTNode node_3__4c82ef7295d74935a3f92523120371a3 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_3__4c82ef7295d74935a3f92523120371a3), ref nextRuntimeNodeId, tree_ai_monster_01, definition_3__4c82ef7295d74935a3f92523120371a3, nodes);
            BTNodeData definition_4__475e275efde24a498117b016508a5a79 = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "475e275efde24a498117b016508a5a79");
            BTNode node_4__475e275efde24a498117b016508a5a79 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_4__475e275efde24a498117b016508a5a79), ref nextRuntimeNodeId, tree_ai_monster_01, definition_4__475e275efde24a498117b016508a5a79, nodes);
            node_3__4c82ef7295d74935a3f92523120371a3.Children.Add(node_4__475e275efde24a498117b016508a5a79);
            BTNodeData definition_5_c7f1f0a60d5b4208b9f6e6b1c45d5b2c = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "c7f1f0a60d5b4208b9f6e6b1c45d5b2c");
            BTNode node_5_c7f1f0a60d5b4208b9f6e6b1c45d5b2c = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_5_c7f1f0a60d5b4208b9f6e6b1c45d5b2c), ref nextRuntimeNodeId, tree_ai_monster_01, definition_5_c7f1f0a60d5b4208b9f6e6b1c45d5b2c, nodes);
            node_3__4c82ef7295d74935a3f92523120371a3.Children.Add(node_5_c7f1f0a60d5b4208b9f6e6b1c45d5b2c);
            BTNodeData definition_6__9e36eb277f4a40d185a4d4a10bdf012c = BTCompiledTreeUtility.RequireNodeDefinition(tree_ai_monster_01, "9e36eb277f4a40d185a4d4a10bdf012c");
            BTNode node_6__9e36eb277f4a40d185a4d4a10bdf012c = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_6__9e36eb277f4a40d185a4d4a10bdf012c), ref nextRuntimeNodeId, tree_ai_monster_01, definition_6__9e36eb277f4a40d185a4d4a10bdf012c, nodes);
            node_3__4c82ef7295d74935a3f92523120371a3.Children.Add(node_6__9e36eb277f4a40d185a4d4a10bdf012c);
            node_2__5a8b257bff56485ea14994e2eb6ba77e.Children.Add(node_3__4c82ef7295d74935a3f92523120371a3);
            node_1_fb8cdf410a644eb6b9a73d6a26fb975d.Children.Add(node_2__5a8b257bff56485ea14994e2eb6ba77e);
            template.AddEntry(tree_ai_monster_01, (BTRoot)node_1_fb8cdf410a644eb6b9a73d6a26fb975d, nodes);
        }
    }
}
