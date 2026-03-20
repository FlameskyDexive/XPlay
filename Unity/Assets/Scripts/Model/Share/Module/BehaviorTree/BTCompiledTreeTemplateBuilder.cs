using System;
using System.Collections.Generic;

namespace ET
{
    public static class BTCompiledTreeTemplateBuilder
    {
        public static BTCompiledTreeTemplate Build(string packageKey, BTPackage package)
        {
            if (package == null)
            {
                return null;
            }

            BTCompiledTreeTemplate template = new(packageKey, package);
            foreach (BTDefinition tree in package.Trees)
            {
                Dictionary<int, BTNode> nodes = new();
                int nextRuntimeNodeId = 1;
                HashSet<string> buildStack = new(StringComparer.OrdinalIgnoreCase);
                BTNode rootNode = BuildNode(package, tree, tree.RootNodeId, nodes, ref nextRuntimeNodeId, buildStack);
                if (rootNode is not BTRoot root)
                {
                    throw new Exception($"behavior tree compiled root invalid: {tree.TreeName}");
                }

                template.AddEntry(tree, root, nodes);
            }

            return template;
        }

        private static BTNode BuildNode(BTPackage package, BTDefinition tree, string nodeId, Dictionary<int, BTNode> nodes, ref int nextRuntimeNodeId,
            HashSet<string> buildStack)
        {
            if (package == null || tree == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            BTNodeData definition = tree.GetNode(nodeId);
            if (definition == null)
            {
                return null;
            }

            BTNode node = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create(definition), ref nextRuntimeNodeId, tree, definition, nodes);
            foreach (string childId in definition.ChildIds)
            {
                BTNode child = BuildNode(package, tree, childId, nodes, ref nextRuntimeNodeId, buildStack);
                if (child != null)
                {
                    node.Children.Add(child);
                }
            }

            if (node is not BTSubTreeCall subTreeCall || definition is not BTSubTreeNodeData subTreeNodeData)
            {
                return node;
            }

            BTDefinition subTree = BTCompiledTreeUtility.RequireTree(package, subTreeNodeData.SubTreeId, subTreeNodeData.SubTreeName);
            string guardKey = !string.IsNullOrWhiteSpace(subTree.TreeId) ? subTree.TreeId : subTree.TreeName;
            if (!string.IsNullOrWhiteSpace(guardKey) && !buildStack.Add(guardKey))
            {
                throw new Exception($"behavior tree subtree cycle detected: {guardKey}");
            }

            try
            {
                subTreeCall.SubTreeRoot = BuildNode(package, subTree, subTree.RootNodeId, nodes, ref nextRuntimeNodeId, buildStack);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(guardKey))
                {
                    buildStack.Remove(guardKey);
                }
            }

            return node;
        }
    }
}
