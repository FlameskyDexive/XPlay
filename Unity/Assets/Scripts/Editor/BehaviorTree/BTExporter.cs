using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class BTExporter
    {
        public static string GetPackageKey(BTAsset rootAsset)
        {
            if (rootAsset == null)
            {
                throw new ArgumentNullException(nameof(rootAsset));
            }

            rootAsset.EnsureInitialized();
            return Path.GetFileNameWithoutExtension(rootAsset.ExportRelativePath);
        }

        public static object BuildPackage(BTAsset rootAsset)
        {
            if (rootAsset == null)
            {
                throw new ArgumentNullException(nameof(rootAsset));
            }

            rootAsset.EnsureInitialized();
            HashSet<BTAsset> visitedAssets = new();
            List<object> trees = new();
            HashSet<string> treeIds = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> treeNames = new(StringComparer.OrdinalIgnoreCase);

            Collect(rootAsset, visitedAssets, trees, treeIds, treeNames);

            object package = BTEditorRuntimeBridge.CreateInstance("ET.BTPackage");
            BTEditorRuntimeBridge.SetValue(package, "PackageId", rootAsset.TreeId);
            BTEditorRuntimeBridge.SetValue(package, "PackageName", rootAsset.TreeName);
            BTEditorRuntimeBridge.SetValue(package, "EntryTreeId", rootAsset.TreeId);
            BTEditorRuntimeBridge.SetValue(package, "EntryTreeName", rootAsset.TreeName);

            IList packageTrees = BTEditorRuntimeBridge.GetList(package, "Trees");
            foreach (object tree in trees)
            {
                packageTrees.Add(tree);
            }

            return package;
        }

        public static byte[] BuildBytes(BTAsset rootAsset)
        {
            object package = BuildPackage(rootAsset);
            return BTEditorRuntimeBridge.SerializePackage(package);
        }

        public static string ExportToFile(BTAsset rootAsset)
        {
            return ExportToFiles(rootAsset).ClientFullPath;
        }

        public static BTExportResult ExportToFiles(BTAsset rootAsset)
        {
            byte[] bytes = BuildBytes(rootAsset);
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? string.Empty;
            string clientFullPath = Path.GetFullPath(Path.Combine(projectRoot, rootAsset.ExportRelativePath));
            string clientDirectory = Path.GetDirectoryName(clientFullPath) ?? string.Empty;
            if (!Directory.Exists(clientDirectory))
            {
                Directory.CreateDirectory(clientDirectory);
            }

            string serverFileName = Path.GetFileName(rootAsset.ExportRelativePath);
            string serverFullPath = Path.GetFullPath(Path.Combine(projectRoot, "..", BTBytesLoader.ServerBehaviorTreeBytesDir, serverFileName));
            string serverDirectory = Path.GetDirectoryName(serverFullPath) ?? string.Empty;
            if (!Directory.Exists(serverDirectory))
            {
                Directory.CreateDirectory(serverDirectory);
            }

            File.WriteAllBytes(clientFullPath, bytes);
            File.WriteAllBytes(serverFullPath, bytes);
            AssetDatabase.Refresh();
            return new BTExportResult(clientFullPath, serverFullPath);
        }

        public readonly struct BTExportResult
        {
            public BTExportResult(string clientFullPath, string serverFullPath)
            {
                this.ClientFullPath = clientFullPath;
                this.ServerFullPath = serverFullPath;
            }

            public string ClientFullPath { get; }

            public string ServerFullPath { get; }
        }

        private static void Collect(BTAsset asset, HashSet<BTAsset> visitedAssets, List<object> trees, HashSet<string> treeIds, HashSet<string> treeNames)
        {
            if (!visitedAssets.Add(asset))
            {
                return;
            }

            ValidateAsset(asset, treeIds, treeNames);
            trees.Add(BuildDefinition(asset));

            foreach (BTEditorNodeData node in asset.Nodes)
            {
                if (node.NodeKind != BTNodeKind.SubTree || node.SubTreeAsset == null)
                {
                    continue;
                }

                node.SubTreeAsset.EnsureInitialized();
                node.SyncSubTreeInfo();
                Collect(node.SubTreeAsset, visitedAssets, trees, treeIds, treeNames);
            }
        }

        private static void ValidateAsset(BTAsset asset, HashSet<string> treeIds, HashSet<string> treeNames)
        {
            if (string.IsNullOrWhiteSpace(asset.TreeId))
            {
                throw new InvalidOperationException($"BehaviorTree asset '{asset.name}' missing TreeId.");
            }

            if (string.IsNullOrWhiteSpace(asset.TreeName))
            {
                throw new InvalidOperationException($"BehaviorTree asset '{asset.name}' missing TreeName.");
            }

            if (!treeIds.Add(asset.TreeId))
            {
                throw new InvalidOperationException($"Duplicate BehaviorTree TreeId: {asset.TreeId}");
            }

            if (!treeNames.Add(asset.TreeName))
            {
                throw new InvalidOperationException($"Duplicate BehaviorTree TreeName: {asset.TreeName}");
            }

            if (asset.GetRootNode() == null)
            {
                throw new InvalidOperationException($"BehaviorTree asset '{asset.name}' missing Root node.");
            }

            HashSet<string> nodeIds = new(StringComparer.OrdinalIgnoreCase);
            foreach (BTEditorNodeData node in asset.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.NodeId))
                {
                    throw new InvalidOperationException($"BehaviorTree asset '{asset.name}' has node without NodeId.");
                }

                if (!nodeIds.Add(node.NodeId))
                {
                    throw new InvalidOperationException($"BehaviorTree asset '{asset.name}' contains duplicate NodeId: {node.NodeId}");
                }
            }
        }

        private static object BuildDefinition(BTAsset asset)
        {
            object definition = BTEditorRuntimeBridge.CreateInstance("ET.BTDefinition");
            BTEditorRuntimeBridge.SetValue(definition, "TreeId", asset.TreeId);
            BTEditorRuntimeBridge.SetValue(definition, "TreeName", asset.TreeName);
            BTEditorRuntimeBridge.SetValue(definition, "Description", asset.Description);
            BTEditorRuntimeBridge.SetValue(definition, "RootNodeId", asset.RootNodeId);

            IList blackboardEntries = BTEditorRuntimeBridge.GetList(definition, "BlackboardEntries");
            foreach (BTBlackboardEntryData entry in asset.BlackboardEntries)
            {
                blackboardEntries.Add(entry.Clone());
            }

            IList nodes = BTEditorRuntimeBridge.GetList(definition, "Nodes");
            foreach (BTEditorNodeData node in asset.Nodes)
            {
                nodes.Add(BuildNode(node));
            }

            return definition;
        }

        private static object BuildNode(BTEditorNodeData node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            BTEditorUtility.SyncNodeDescriptor(node);
            return BTEditorRuntimeNodeFactory.CreateFromEditorNode(node);
        }
    }

    internal sealed class BTCodeWriter
    {
        private readonly System.Text.StringBuilder stringBuilder = new();
        private int indent;

        public void Line(string text = "")
        {
            if (text.Length == 0)
            {
                this.stringBuilder.AppendLine();
                return;
            }

            for (int index = 0; index < this.indent; ++index)
            {
                this.stringBuilder.Append("    ");
            }

            this.stringBuilder.AppendLine(text);
        }

        public void OpenBlock(string header)
        {
            this.Line(header);
            this.Line("{");
            ++this.indent;
        }

        public void CloseBlock(string suffix = "")
        {
            if (this.indent > 0)
            {
                --this.indent;
            }

            this.Line($"}}{suffix}");
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }
    }

    public static class BTCSharpExporter
    {
        private const string GeneratedRelativeDirectory = "Assets/Scripts/Hotfix/Share/Module/BehaviorTree/Generate";

        private sealed class TreeExportInfo
        {
            public object RuntimeTree;
            public string TreeId;
            public string TreeName;
            public string RootNodeId;
            public readonly Dictionary<string, object> Nodes = new(StringComparer.OrdinalIgnoreCase);
        }

        public static string GetCompiledExportRelativePath(BTAsset rootAsset)
        {
            if (rootAsset == null)
            {
                throw new ArgumentNullException(nameof(rootAsset));
            }

            rootAsset.EnsureInitialized();
            return $"{GeneratedRelativeDirectory}/BT_{SanitizeIdentifier(GetCodeIdentifier(rootAsset.TreeName, rootAsset.TreeId))}.g.cs";
        }

        public static string ExportToFile(BTAsset rootAsset)
        {
            if (rootAsset == null)
            {
                throw new ArgumentNullException(nameof(rootAsset));
            }

            object runtimePackage = BTExporter.BuildPackage(rootAsset);
            string source = BuildSource(rootAsset, runtimePackage);
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? string.Empty;
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, GetCompiledExportRelativePath(rootAsset)));
            string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, source);
            AssetDatabase.Refresh();
            return fullPath;
        }

        private static string BuildSource(BTAsset rootAsset, object runtimePackage)
        {
            List<TreeExportInfo> treeInfos = BuildTreeInfos(runtimePackage);
            Dictionary<string, TreeExportInfo> treeInfosById = new();
            Dictionary<string, TreeExportInfo> treeInfosByName = new(StringComparer.OrdinalIgnoreCase);
            foreach (TreeExportInfo treeInfo in treeInfos)
            {
                if (!string.IsNullOrWhiteSpace(treeInfo.TreeId))
                {
                    treeInfosById[treeInfo.TreeId] = treeInfo;
                }

                if (!string.IsNullOrWhiteSpace(treeInfo.TreeName))
                {
                    treeInfosByName[treeInfo.TreeName] = treeInfo;
                }
            }

            string providerClassName = $"BTCompiledProvider_{SanitizeIdentifier(GetCodeIdentifier(rootAsset.TreeName, rootAsset.TreeId))}";
            string packageKey = BTExporter.GetPackageKey(rootAsset);
            BTCodeWriter writer = new();
            writer.Line("using System.Collections.Generic;");
            writer.Line();
            writer.OpenBlock("namespace ET");
            writer.Line("[BTCompiledTreeProvider]");
            writer.OpenBlock($"public sealed class {providerClassName} : IBTCompiledTreeProvider");
            writer.Line($"private const string PackageKeyValue = {ToLiteral(packageKey)};");
            writer.Line("private static BTCompiledTreeTemplate template;");
            writer.Line();
            writer.Line("public string PackageKey => PackageKeyValue;");
            writer.Line("public BTCompiledTreeTemplate Template => template ??= BuildTemplate();");
            writer.Line();
            writer.OpenBlock("private static BTCompiledTreeTemplate BuildTemplate()");
            EmitObjectCreation(writer, "package", runtimePackage);
            writer.Line("BTCompiledTreeTemplate templateValue = new(PackageKeyValue, package);");
            foreach (TreeExportInfo treeInfo in treeInfos)
            {
                writer.Line($"{GetBuildEntryMethodName(treeInfo)}(templateValue);");
            }

            writer.Line("return templateValue;");
            writer.CloseBlock();

            foreach (TreeExportInfo treeInfo in treeInfos)
            {
                EmitBuildEntryMethod(writer, treeInfo, treeInfosById, treeInfosByName);
            }

            writer.CloseBlock();
            writer.CloseBlock();
            return writer.ToString();
        }

        private static void EmitBuildEntryMethod(BTCodeWriter writer, TreeExportInfo entryTree, Dictionary<string, TreeExportInfo> treeInfosById,
            Dictionary<string, TreeExportInfo> treeInfosByName)
        {
            writer.Line();
            writer.OpenBlock($"private static void {GetBuildEntryMethodName(entryTree)}(BTCompiledTreeTemplate template)");
            writer.Line("Dictionary<int, BTNode> nodes = new();");
            writer.Line("int nextRuntimeNodeId = 1;");

            Dictionary<TreeExportInfo, string> treeVariableNames = new();
            int variableSequence = 0;
            string rootVariableName = EmitRuntimeNode(writer, entryTree, entryTree.RootNodeId, treeVariableNames, treeInfosById, treeInfosByName,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase), ref variableSequence);
            string entryTreeVariableName = EnsureTreeVariable(writer, entryTree, treeVariableNames);
            writer.Line($"template.AddEntry({entryTreeVariableName}, (BTRoot){rootVariableName}, nodes);");
            writer.CloseBlock();
        }

        private static string EmitRuntimeNode(BTCodeWriter writer, TreeExportInfo treeInfo, string nodeId, Dictionary<TreeExportInfo, string> treeVariableNames,
            Dictionary<string, TreeExportInfo> treeInfosById, Dictionary<string, TreeExportInfo> treeInfosByName, HashSet<string> subtreeStack, ref int variableSequence)
        {
            if (!treeInfo.Nodes.TryGetValue(nodeId, out object definition))
            {
                throw new InvalidOperationException($"BehaviorTree export node not found: tree={treeInfo.TreeName}, nodeId={nodeId}");
            }

            string treeVariableName = EnsureTreeVariable(writer, treeInfo, treeVariableNames);
            int currentIndex = ++variableSequence;
            string definitionVariableName = $"definition_{currentIndex}_{SanitizeIdentifier(nodeId)}";
            string nodeVariableName = $"node_{currentIndex}_{SanitizeIdentifier(nodeId)}";
            writer.Line($"BTNodeData {definitionVariableName} = BTCompiledTreeUtility.RequireNodeDefinition({treeVariableName}, {ToLiteral(nodeId)});");
            writer.Line($"BTNode {nodeVariableName} = BTCompiledTreeUtility.InitializeNode(BTRuntimeNodeFactory.Create({definitionVariableName}), ref nextRuntimeNodeId, {treeVariableName}, {definitionVariableName}, nodes);");

            IList childIds = BTEditorRuntimeBridge.GetList(definition, "ChildIds");
            foreach (object childIdObject in childIds)
            {
                string childId = childIdObject as string ?? string.Empty;
                string childVariableName = EmitRuntimeNode(writer, treeInfo, childId, treeVariableNames, treeInfosById, treeInfosByName, subtreeStack, ref variableSequence);
                writer.Line($"{nodeVariableName}.Children.Add({childVariableName});");
            }

            if (string.Equals(definition.GetType().Name, "BTSubTreeNodeData", StringComparison.Ordinal))
            {
                string subTreeId = BTEditorRuntimeBridge.GetValue(definition, "SubTreeId", string.Empty);
                string subTreeName = BTEditorRuntimeBridge.GetValue(definition, "SubTreeName", string.Empty);
                TreeExportInfo subTreeInfo = ResolveTreeInfo(treeInfosById, treeInfosByName, subTreeId, subTreeName);
                string guardKey = !string.IsNullOrWhiteSpace(subTreeInfo.TreeId) ? subTreeInfo.TreeId : subTreeInfo.TreeName;
                if (!string.IsNullOrWhiteSpace(guardKey) && !subtreeStack.Add(guardKey))
                {
                    throw new InvalidOperationException($"BehaviorTree subtree cycle detected: {guardKey}");
                }

                try
                {
                    string subTreeRootVariableName = EmitRuntimeNode(writer, subTreeInfo, subTreeInfo.RootNodeId, treeVariableNames, treeInfosById, treeInfosByName,
                        subtreeStack, ref variableSequence);
                    writer.Line($"((BTSubTreeCall){nodeVariableName}).SubTreeRoot = {subTreeRootVariableName};");
                }
                finally
                {
                    if (!string.IsNullOrWhiteSpace(guardKey))
                    {
                        subtreeStack.Remove(guardKey);
                    }
                }
            }

            return nodeVariableName;
        }

        private static string EnsureTreeVariable(BTCodeWriter writer, TreeExportInfo treeInfo, Dictionary<TreeExportInfo, string> treeVariableNames)
        {
            if (treeVariableNames.TryGetValue(treeInfo, out string variableName))
            {
                return variableName;
            }

            variableName = $"tree_{SanitizeIdentifier(GetCodeIdentifier(treeInfo.TreeName, treeInfo.TreeId))}";
            treeVariableNames[treeInfo] = variableName;
            writer.Line($"BTDefinition {variableName} = BTCompiledTreeUtility.RequireTree(template.Package, {ToLiteral(treeInfo.TreeId)}, {ToLiteral(treeInfo.TreeName)});");
            return variableName;
        }

        private static TreeExportInfo ResolveTreeInfo(Dictionary<string, TreeExportInfo> treeInfosById, Dictionary<string, TreeExportInfo> treeInfosByName,
            string treeId, string treeName)
        {
            if (!string.IsNullOrWhiteSpace(treeId) && treeInfosById.TryGetValue(treeId, out TreeExportInfo treeInfoById))
            {
                return treeInfoById;
            }

            if (!string.IsNullOrWhiteSpace(treeName) && treeInfosByName.TryGetValue(treeName, out TreeExportInfo treeInfoByName))
            {
                return treeInfoByName;
            }

            throw new InvalidOperationException($"BehaviorTree subtree not found: id={treeId}, name={treeName}");
        }

        private static string GetBuildEntryMethodName(TreeExportInfo treeInfo)
        {
            return $"BuildEntry_{SanitizeIdentifier(GetCodeIdentifier(treeInfo.TreeName, treeInfo.TreeId))}";
        }

        private static string GetCodeIdentifier(string treeName, string treeId)
        {
            return string.IsNullOrWhiteSpace(treeName) ? treeId : treeName;
        }

        private static List<TreeExportInfo> BuildTreeInfos(object runtimePackage)
        {
            List<TreeExportInfo> treeInfos = new();
            IList trees = BTEditorRuntimeBridge.GetList(runtimePackage, "Trees");
            foreach (object runtimeTree in trees)
            {
                TreeExportInfo treeInfo = new()
                {
                    RuntimeTree = runtimeTree,
                    TreeId = BTEditorRuntimeBridge.GetValue(runtimeTree, "TreeId", string.Empty),
                    TreeName = BTEditorRuntimeBridge.GetValue(runtimeTree, "TreeName", string.Empty),
                    RootNodeId = BTEditorRuntimeBridge.GetValue(runtimeTree, "RootNodeId", string.Empty),
                };

                IList nodes = BTEditorRuntimeBridge.GetList(runtimeTree, "Nodes");
                foreach (object node in nodes)
                {
                    string nodeId = BTEditorRuntimeBridge.GetValue(node, "NodeId", string.Empty);
                    treeInfo.Nodes[nodeId] = node;
                }

                treeInfos.Add(treeInfo);
            }

            return treeInfos;
        }

        private static void EmitObjectCreation(BTCodeWriter writer, string variableName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            writer.Line($"{GetTypeName(value.GetType())} {variableName} = new();");
            EmitObjectAssignments(writer, variableName, value);
        }

        private static void EmitObjectAssignments(BTCodeWriter writer, string variableName, object value)
        {
            foreach (FieldInfo fieldInfo in GetOrderedFields(value.GetType()))
            {
                object fieldValue = fieldInfo.GetValue(value);
                if (IsSimpleValue(fieldInfo.FieldType))
                {
                    writer.Line($"{variableName}.{fieldInfo.Name} = {ToLiteral(fieldValue, fieldInfo.FieldType)};");
                    continue;
                }

                if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
                {
                    if (fieldValue is not IList list)
                    {
                        continue;
                    }

                    for (int index = 0; index < list.Count; ++index)
                    {
                        object item = list[index];
                        Type itemType = item?.GetType();
                        if (itemType == null)
                        {
                            continue;
                        }

                        if (IsSimpleValue(itemType))
                        {
                            writer.Line($"{variableName}.{fieldInfo.Name}.Add({ToLiteral(item, itemType)});");
                            continue;
                        }

                        string itemVariableName = $"{variableName}_{fieldInfo.Name}_{index}";
                        EmitObjectCreation(writer, itemVariableName, item);
                        writer.Line($"{variableName}.{fieldInfo.Name}.Add({itemVariableName});");
                    }

                    continue;
                }

                if (fieldValue == null)
                {
                    writer.Line($"{variableName}.{fieldInfo.Name} = null;");
                    continue;
                }

                string childVariableName = $"{variableName}_{fieldInfo.Name}";
                EmitObjectCreation(writer, childVariableName, fieldValue);
                writer.Line($"{variableName}.{fieldInfo.Name} = {childVariableName};");
            }
        }

        private static IEnumerable<FieldInfo> GetOrderedFields(Type type)
        {
            List<Type> hierarchy = new();
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                hierarchy.Insert(0, currentType);
                currentType = currentType.BaseType;
            }

            foreach (Type hierarchyType in hierarchy)
            {
                FieldInfo[] fields = hierarchyType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (!fieldInfo.IsStatic)
                    {
                        yield return fieldInfo;
                    }
                }
            }
        }

        private static bool IsSimpleValue(Type type)
        {
            return type == typeof(string)
                   || type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(float)
                   || type == typeof(bool)
                   || type.IsEnum;
        }

        private static string GetTypeName(Type type)
        {
            return type.Name;
        }

        private static string ToLiteral(object value)
        {
            return ToLiteral(value, value?.GetType() ?? typeof(string));
        }

        private static string ToLiteral(object value, Type type)
        {
            if (value == null)
            {
                return "null";
            }

            if (type == typeof(string))
            {
                return $"\"{EscapeString((string)value)}\"";
            }

            if (type == typeof(int))
            {
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(long))
            {
                return $"{((long)value).ToString(CultureInfo.InvariantCulture)}L";
            }

            if (type == typeof(float))
            {
                return $"{((float)value).ToString("R", CultureInfo.InvariantCulture)}f";
            }

            if (type == typeof(bool))
            {
                return (bool)value ? "true" : "false";
            }

            if (type.IsEnum)
            {
                return $"{type.Name}.{Enum.GetName(type, value)}";
            }

            throw new InvalidOperationException($"Unsupported literal type: {type.FullName}");
        }

        private static string EscapeString(string value)
        {
            return (value ?? string.Empty)
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t");
        }

        private static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Generated";
            }

            char[] buffer = value.ToCharArray();
            for (int index = 0; index < buffer.Length; ++index)
            {
                if (!char.IsLetterOrDigit(buffer[index]))
                {
                    buffer[index] = '_';
                }
            }

            string sanitized = new(buffer);
            if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            {
                sanitized = $"_{sanitized}";
            }

            return sanitized;
        }
    }
}
