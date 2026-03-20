using System;
using System.Collections.Generic;

namespace ET
{
    public static class BTCompiledTreeUtility
    {
        public static BTDefinition RequireTree(BTPackage package, string treeId, string treeName = "")
        {
            BTDefinition definition = package?.GetTree(treeId);
            if (definition != null)
            {
                return definition;
            }

            definition = package?.GetTree(treeName);
            if (definition != null)
            {
                return definition;
            }

            throw new Exception($"behavior tree not found: id={treeId}, name={treeName}");
        }

        public static BTNodeData RequireNodeDefinition(BTDefinition tree, string nodeId)
        {
            BTNodeData definition = tree?.GetNode(nodeId);
            if (definition != null)
            {
                return definition;
            }

            throw new Exception($"behavior tree node definition not found: tree={tree?.TreeName}, nodeId={nodeId}");
        }

        public static BTNode InitializeNode(BTNode node, ref int nextRuntimeNodeId, BTDefinition tree, BTNodeData definition, Dictionary<int, BTNode> nodes)
        {
            if (node == null)
            {
                throw new Exception($"behavior tree runtime node create failed: {definition?.GetType().FullName}");
            }

            node.RuntimeNodeId = nextRuntimeNodeId++;
            node.SourceNodeId = definition?.NodeId ?? string.Empty;
            node.TreeId = tree?.TreeId ?? string.Empty;
            node.TreeName = tree?.TreeName ?? string.Empty;
            node.Definition = definition;
            nodes[node.RuntimeNodeId] = node;
            return node;
        }
    }
}
