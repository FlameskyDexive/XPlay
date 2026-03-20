using System.Collections.Generic;

namespace ET
{
    [BTCompiledTreeProvider]
    public sealed class BTCompiledProvider_NewBehaviorTree : IBTCompiledTreeProvider
    {
        private const string PackageKeyValue = "NewBehaviorTree";
        private static BTCompiledTreeTemplate template;

        public string PackageKey => PackageKeyValue;
        public BTCompiledTreeTemplate Template => template ??= BuildTemplate();

        private static BTCompiledTreeTemplate BuildTemplate()
        {
            BTPackage package = new();
            package.PackageId = "ff0e683322414856a7988acf8a68f480";
            package.PackageName = "NewBehaviorTree";
            package.EntryTreeId = "ff0e683322414856a7988acf8a68f480";
            package.EntryTreeName = "NewBehaviorTree";
            BTDefinition package_Trees_0 = new();
            package_Trees_0.TreeId = "ff0e683322414856a7988acf8a68f480";
            package_Trees_0.TreeName = "NewBehaviorTree";
            package_Trees_0.Description = "";
            package_Trees_0.RootNodeId = "e93bc620a9d74075be372bb77a9eb7c4";
            BTRootNodeData package_Trees_0_Nodes_0 = new();
            package_Trees_0_Nodes_0.NodeId = "e93bc620a9d74075be372bb77a9eb7c4";
            package_Trees_0_Nodes_0.Title = "Root";
            package_Trees_0_Nodes_0.NodeKind = BTNodeKind.Root;
            package_Trees_0_Nodes_0.ChildIds.Add("080535af614545edb60201c005a24143");
            package_Trees_0_Nodes_0.Comment = "";
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_0);
            BTWaitNodeData package_Trees_0_Nodes_1 = new();
            package_Trees_0_Nodes_1.NodeId = "bf8654f2c97346769e93422d9edca860";
            package_Trees_0_Nodes_1.Title = "Wait";
            package_Trees_0_Nodes_1.NodeKind = BTNodeKind.Wait;
            package_Trees_0_Nodes_1.Comment = "";
            package_Trees_0_Nodes_1.WaitMilliseconds = 1000;
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_1);
            BTSequenceNodeData package_Trees_0_Nodes_2 = new();
            package_Trees_0_Nodes_2.NodeId = "c940d6ca1e304762b1197958d95b2c8f";
            package_Trees_0_Nodes_2.Title = "Sequence";
            package_Trees_0_Nodes_2.NodeKind = BTNodeKind.Sequence;
            package_Trees_0_Nodes_2.ChildIds.Add("bf8654f2c97346769e93422d9edca860");
            package_Trees_0_Nodes_2.ChildIds.Add("d1a148ec30e747fc9e68646b93950b61");
            package_Trees_0_Nodes_2.ChildIds.Add("fca1ca6359bc4846a91fef405eb27b2d");
            package_Trees_0_Nodes_2.ChildIds.Add("e793d840d7144396835655381abd8a38");
            package_Trees_0_Nodes_2.Comment = "";
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_2);
            BTRepeaterNodeData package_Trees_0_Nodes_3 = new();
            package_Trees_0_Nodes_3.NodeId = "080535af614545edb60201c005a24143";
            package_Trees_0_Nodes_3.Title = "Repeater";
            package_Trees_0_Nodes_3.NodeKind = BTNodeKind.Repeater;
            package_Trees_0_Nodes_3.ChildIds.Add("c940d6ca1e304762b1197958d95b2c8f");
            package_Trees_0_Nodes_3.Comment = "";
            package_Trees_0_Nodes_3.MaxLoopCount = 0;
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_3);
            BTHasPatrolPathNodeData package_Trees_0_Nodes_4 = new();
            package_Trees_0_Nodes_4.NodeId = "d1a148ec30e747fc9e68646b93950b61";
            package_Trees_0_Nodes_4.Title = "Has Patrol Path";
            package_Trees_0_Nodes_4.NodeKind = BTNodeKind.Condition;
            package_Trees_0_Nodes_4.Comment = "";
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_4);
            BTPatrolNodeData package_Trees_0_Nodes_5 = new();
            package_Trees_0_Nodes_5.NodeId = "fca1ca6359bc4846a91fef405eb27b2d";
            package_Trees_0_Nodes_5.Title = "Patrol";
            package_Trees_0_Nodes_5.NodeKind = BTNodeKind.Action;
            package_Trees_0_Nodes_5.Comment = "";
            BTPatrolPointData package_Trees_0_Nodes_5_PatrolPoints_0 = new();
            package_Trees_0_Nodes_5_PatrolPoints_0.X = 1f;
            package_Trees_0_Nodes_5_PatrolPoints_0.Y = 0f;
            package_Trees_0_Nodes_5_PatrolPoints_0.Z = 20f;
            package_Trees_0_Nodes_5.PatrolPoints.Add(package_Trees_0_Nodes_5_PatrolPoints_0);
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_5);
            BTWaitNodeData package_Trees_0_Nodes_6 = new();
            package_Trees_0_Nodes_6.NodeId = "e793d840d7144396835655381abd8a38";
            package_Trees_0_Nodes_6.Title = "Wait";
            package_Trees_0_Nodes_6.NodeKind = BTNodeKind.Wait;
            package_Trees_0_Nodes_6.Comment = "";
            package_Trees_0_Nodes_6.WaitMilliseconds = 1000;
            package_Trees_0.Nodes.Add(package_Trees_0_Nodes_6);
            package.Trees.Add(package_Trees_0);
            BTCompiledTreeTemplate templateValue = new(PackageKeyValue, package);
            BuildEntry_NewBehaviorTree(templateValue);
            return templateValue;
        }

        private static void BuildEntry_NewBehaviorTree(BTCompiledTreeTemplate template)
        {
            Dictionary<int, BTNode> nodes = new();
            int nextRuntimeNodeId = 1;
            BTDefinition tree_NewBehaviorTree = BTCompiledTreeUtility.RequireTree(template.Package, "ff0e683322414856a7988acf8a68f480", "NewBehaviorTree");
            BTNodeData definition_1_e93bc620a9d74075be372bb77a9eb7c4 = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "e93bc620a9d74075be372bb77a9eb7c4");
            BTNode node_1_e93bc620a9d74075be372bb77a9eb7c4 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_1_e93bc620a9d74075be372bb77a9eb7c4), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_1_e93bc620a9d74075be372bb77a9eb7c4, nodes);
            BTNodeData definition_2__080535af614545edb60201c005a24143 = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "080535af614545edb60201c005a24143");
            BTNode node_2__080535af614545edb60201c005a24143 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_2__080535af614545edb60201c005a24143), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_2__080535af614545edb60201c005a24143, nodes);
            BTNodeData definition_3_c940d6ca1e304762b1197958d95b2c8f = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "c940d6ca1e304762b1197958d95b2c8f");
            BTNode node_3_c940d6ca1e304762b1197958d95b2c8f = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_3_c940d6ca1e304762b1197958d95b2c8f), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_3_c940d6ca1e304762b1197958d95b2c8f, nodes);
            BTNodeData definition_4_bf8654f2c97346769e93422d9edca860 = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "bf8654f2c97346769e93422d9edca860");
            BTNode node_4_bf8654f2c97346769e93422d9edca860 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_4_bf8654f2c97346769e93422d9edca860), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_4_bf8654f2c97346769e93422d9edca860, nodes);
            node_3_c940d6ca1e304762b1197958d95b2c8f.Children.Add(node_4_bf8654f2c97346769e93422d9edca860);
            BTNodeData definition_5_d1a148ec30e747fc9e68646b93950b61 = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "d1a148ec30e747fc9e68646b93950b61");
            BTNode node_5_d1a148ec30e747fc9e68646b93950b61 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_5_d1a148ec30e747fc9e68646b93950b61), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_5_d1a148ec30e747fc9e68646b93950b61, nodes);
            node_3_c940d6ca1e304762b1197958d95b2c8f.Children.Add(node_5_d1a148ec30e747fc9e68646b93950b61);
            BTNodeData definition_6_fca1ca6359bc4846a91fef405eb27b2d = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "fca1ca6359bc4846a91fef405eb27b2d");
            BTNode node_6_fca1ca6359bc4846a91fef405eb27b2d = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_6_fca1ca6359bc4846a91fef405eb27b2d), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_6_fca1ca6359bc4846a91fef405eb27b2d, nodes);
            node_3_c940d6ca1e304762b1197958d95b2c8f.Children.Add(node_6_fca1ca6359bc4846a91fef405eb27b2d);
            BTNodeData definition_7_e793d840d7144396835655381abd8a38 = BTCompiledTreeUtility.RequireNodeDefinition(tree_NewBehaviorTree, "e793d840d7144396835655381abd8a38");
            BTNode node_7_e793d840d7144396835655381abd8a38 = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition_7_e793d840d7144396835655381abd8a38), ref nextRuntimeNodeId, tree_NewBehaviorTree, definition_7_e793d840d7144396835655381abd8a38, nodes);
            node_3_c940d6ca1e304762b1197958d95b2c8f.Children.Add(node_7_e793d840d7144396835655381abd8a38);
            node_2__080535af614545edb60201c005a24143.Children.Add(node_3_c940d6ca1e304762b1197958d95b2c8f);
            node_1_e93bc620a9d74075be372bb77a9eb7c4.Children.Add(node_2__080535af614545edb60201c005a24143);
            template.AddEntry(tree_NewBehaviorTree, (BTRoot)node_1_e93bc620a9d74075be372bb77a9eb7c4, nodes);
        }
    }
}
