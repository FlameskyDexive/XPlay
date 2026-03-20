using System;
using System.Collections.Generic;

namespace ET
{
    public static class BTGraphBuilder
    {
        public static BTRoot Build(BTExecutionSession session)
        {
            if (session == null || session.EntryDefinition == null)
            {
                return null;
            }

            int nextRuntimeNodeId = 1;
            HashSet<string> buildStack = new(StringComparer.OrdinalIgnoreCase);
            BTNode rootNode = BuildNode(session, session.EntryDefinition, session.EntryDefinition.RootNodeId, ref nextRuntimeNodeId, buildStack);
            return rootNode as BTRoot;
        }

        private static BTNode BuildNode(BTExecutionSession session, BTDefinition tree, string nodeId, ref int nextRuntimeNodeId, HashSet<string> buildStack)
        {
            if (session == null || tree == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            BTNodeData definition = tree.GetNode(nodeId);
            if (definition == null)
            {
                return null;
            }

            BTNode node = BTRuntimeNodeFactory.Create(definition);
            if (node == null)
            {
                return null;
            }

            node.RuntimeNodeId = nextRuntimeNodeId++;
            node.SourceNodeId = definition.NodeId ?? string.Empty;
            node.TreeId = tree.TreeId ?? string.Empty;
            node.TreeName = tree.TreeName ?? string.Empty;
            node.Definition = definition;
            session.Nodes[node.RuntimeNodeId] = node;

            foreach (string childId in definition.ChildIds)
            {
                BTNode child = BuildNode(session, tree, childId, ref nextRuntimeNodeId, buildStack);
                if (child != null)
                {
                    node.Children.Add(child);
                }
            }

            if (node is BTSubTreeCall subTreeCall)
            {
                if (subTreeCall.Definition is not BTSubTreeNodeData subTreeNodeData)
                {
                    return node;
                }

                BTDefinition subTree = session.ResolveTree(subTreeNodeData.SubTreeId, subTreeNodeData.SubTreeName);
                if (subTree == null)
                {
                    return node;
                }

                string guardKey = !string.IsNullOrWhiteSpace(subTree.TreeId) ? subTree.TreeId : subTree.TreeName;
                if (!string.IsNullOrWhiteSpace(guardKey))
                {
                    if (!buildStack.Add(guardKey))
                    {
                        throw new Exception($"behavior tree subtree cycle detected: {guardKey}");
                    }
                }

                try
                {
                    subTreeCall.SubTreeRoot = BuildNode(session, subTree, subTree.RootNodeId, ref nextRuntimeNodeId, buildStack);
                }
                finally
                {
                    if (!string.IsNullOrWhiteSpace(guardKey))
                    {
                        buildStack.Remove(guardKey);
                    }
                }
            }

            return node;
        }
    }
}
