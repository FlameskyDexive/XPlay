using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ET
{
    public sealed class BTSerializationBenchmarkTests
    {
        [Test]
        public void BehaviorTree_AITest_CompiledTemplate_Parity()
        {
            object package = InvokeHotfixStatic("ET.Client.BTClientDemoFactory", "CreateAITestPackage");
            object template = InvokeModelStatic("ET.BTCompiledTreeTemplateBuilder", "Build", "AITest", package);

            Assert.That(package, Is.Not.Null);
            Assert.That(template, Is.Not.Null);

            object templatePackage = GetValue(template, "Package");
            Assert.That(templatePackage, Is.Not.Null);

            int treeCount = BTEditorRuntimeBridge.GetList(templatePackage, "Trees").Count;
            Assert.That(treeCount, Is.EqualTo(BTEditorRuntimeBridge.GetList(package, "Trees").Count));

            object entry = GetEntryByTreeName(template, "AITest");
            Assert.That(entry, Is.Not.Null);

            object definition = GetValue(entry, "Definition");
            object root = GetValue(entry, "Root");
            IDictionary nodes = GetValue(entry, "Nodes") as IDictionary;

            Assert.That(definition, Is.Not.Null);
            Assert.That(root, Is.Not.Null);
            Assert.That(root.GetType().Name, Is.EqualTo("BTRoot"));
            Assert.That(nodes, Is.Not.Null);
            Assert.That(nodes.Count, Is.EqualTo(BTEditorRuntimeBridge.GetList(definition, "Nodes").Count));
        }

        [Test]
        public void BehaviorTree_AITest_CompiledVsBytes_Benchmark()
        {
            const int iterations = 80;
            const int warmupIterations = 5;

            for (int index = 0; index < warmupIterations; ++index)
            {
                object warmupPackage = InvokeHotfixStatic("ET.Client.BTClientDemoFactory", "CreateAITestPackage");
                object warmupTemplate = InvokeModelStatic("ET.BTCompiledTreeTemplateBuilder", "Build", "AITest", warmupPackage);
                Assert.That(warmupTemplate, Is.Not.Null);
                byte[] warmupBytes = InvokeHotfixStatic("ET.Client.BTClientDemoFactory", "CreateAITestBytes") as byte[];
                object warmupRoundTrip = BTEditorRuntimeBridge.DeserializePackage(warmupBytes);
                Assert.That(warmupRoundTrip, Is.Not.Null);
            }

            Stopwatch compiledWatch = Stopwatch.StartNew();
            object template = null;
            for (int index = 0; index < iterations; ++index)
            {
                object package = InvokeHotfixStatic("ET.Client.BTClientDemoFactory", "CreateAITestPackage");
                template = InvokeModelStatic("ET.BTCompiledTreeTemplateBuilder", "Build", "AITest", package);
            }

            compiledWatch.Stop();

            byte[] bytes = InvokeHotfixStatic("ET.Client.BTClientDemoFactory", "CreateAITestBytes") as byte[];
            Stopwatch bytesWatch = Stopwatch.StartNew();
            object roundTripPackage = null;
            for (int index = 0; index < iterations; ++index)
            {
                roundTripPackage = BTEditorRuntimeBridge.DeserializePackage(bytes);
            }

            bytesWatch.Stop();

            object compiledPackage = GetValue(template, "Package");
            int compiledTreeCount = BTEditorRuntimeBridge.GetList(compiledPackage, "Trees").Count;
            int compiledNodeCount = CountNodes(compiledPackage);
            int bytesTreeCount = BTEditorRuntimeBridge.GetList(roundTripPackage, "Trees").Count;
            int bytesNodeCount = CountNodes(roundTripPackage);

            TestContext.WriteLine($"BehaviorTree compiled template benchmark: trees={compiledTreeCount}, nodes={compiledNodeCount}, iterations={iterations}, totalMs={compiledWatch.Elapsed.TotalMilliseconds:F3}, avgMs={compiledWatch.Elapsed.TotalMilliseconds / iterations:F4}");
            TestContext.WriteLine($"BehaviorTree bytes deserialize benchmark: trees={bytesTreeCount}, nodes={bytesNodeCount}, bytes={bytes?.Length ?? 0}, iterations={iterations}, totalMs={bytesWatch.Elapsed.TotalMilliseconds:F3}, avgMs={bytesWatch.Elapsed.TotalMilliseconds / iterations:F4}");
        }

        [Test]
        public void BehaviorTree_NinoSerializeDeserialize_Benchmark()
        {
            const int treeCount = 24;
            const int iterations = 80;
            const int warmupIterations = 5;

            BTAsset asset = CreateBenchmarkAsset(treeCount);

            for (int i = 0; i < warmupIterations; ++i)
            {
                byte[] warmupBytes = BTExporter.BuildBytes(asset);
                object warmupPackage = BTEditorRuntimeBridge.DeserializePackage(warmupBytes);
                Assert.That(GetTreeCount(warmupPackage), Is.GreaterThan(0));
            }

            byte[] serializedBytes = null;
            Stopwatch serializeWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                serializedBytes = BTExporter.BuildBytes(asset);
            }

            serializeWatch.Stop();

            object roundTripPackage = null;
            Stopwatch deserializeWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; ++i)
            {
                roundTripPackage = BTEditorRuntimeBridge.DeserializePackage(serializedBytes);
            }

            deserializeWatch.Stop();

            int treeTotal = GetTreeCount(roundTripPackage);
            int nodeTotal = CountNodes(roundTripPackage);
            TestContext.WriteLine($"BehaviorTree serialize benchmark: trees={treeTotal}, nodes={nodeTotal}, bytes={serializedBytes.Length}, iterations={iterations}, totalMs={serializeWatch.Elapsed.TotalMilliseconds:F3}, avgMs={serializeWatch.Elapsed.TotalMilliseconds / iterations:F4}");
            TestContext.WriteLine($"BehaviorTree deserialize benchmark: trees={treeTotal}, nodes={nodeTotal}, bytes={serializedBytes.Length}, iterations={iterations}, totalMs={deserializeWatch.Elapsed.TotalMilliseconds:F3}, avgMs={deserializeWatch.Elapsed.TotalMilliseconds / iterations:F4}");

            ScriptableObject.DestroyImmediate(asset);
        }

        private static BTAsset CreateBenchmarkAsset(int treeCount)
        {
            BTAsset asset = ScriptableObject.CreateInstance<BTAsset>();
            asset.name = "BehaviorTreeSerializationBenchmark";
            asset.TreeId = "benchmark.behavior_tree.package";
            asset.TreeName = "BehaviorTreeSerializationBenchmark";
            asset.Description = "BehaviorTree benchmark asset";
            asset.EnsureInitialized();
            asset.Nodes.Clear();
            asset.BlackboardEntries.Clear();

            BTEditorNodeData root = new() { NodeId = "root", NodeKind = BTNodeKind.Root, Title = "Root" };
            BTEditorNodeData current = root;
            asset.Nodes.Add(root);

            for (int index = 0; index < treeCount; ++index)
            {
                BTEditorNodeData sequence = new() { NodeId = $"seq_{index}", NodeKind = BTNodeKind.Sequence, Title = $"Sequence {index}" };
                current.ChildIds.Add(sequence.NodeId);
                asset.Nodes.Add(sequence);

                BTEditorNodeData log = new()
                {
                    NodeId = $"log_{index}",
                    NodeKind = BTNodeKind.Action,
                    NodeTypeId = BTBuiltinNodeTypes.Log,
                    HandlerName = "Log",
                    Title = $"Log {index}",
                    Arguments = { CreateStringArgument("message", $"benchmark log {index}") },
                };
                BTEditorNodeData wait = new() { NodeId = $"wait_{index}", NodeKind = BTNodeKind.Wait, Title = $"Wait {index}", WaitMilliseconds = 50 + index };
                sequence.ChildIds.Add(log.NodeId);
                sequence.ChildIds.Add(wait.NodeId);
                asset.Nodes.Add(log);
                asset.Nodes.Add(wait);
                current = sequence;
            }

            asset.RootNodeId = root.NodeId;
            return asset;
        }

        private static BTArgumentData CreateStringArgument(string name, string value)
        {
            return new BTArgumentData
            {
                Name = name,
                Value = new BTSerializedValue
                {
                    ValueType = BTValueType.String,
                    StringValue = value,
                },
            };
        }

        private static int GetTreeCount(object package)
        {
            return BTEditorRuntimeBridge.GetList(package, "Trees").Count;
        }

        private static int CountNodes(object package)
        {
            int count = 0;
            foreach (object tree in BTEditorRuntimeBridge.GetList(package, "Trees"))
            {
                count += BTEditorRuntimeBridge.GetList(tree, "Nodes").Count;
            }

            return count;
        }

        private static object InvokeHotfixStatic(string typeName, string methodName, params object[] arguments)
        {
            return InvokeStatic("Unity.Hotfix", typeName, methodName, arguments);
        }

        private static object InvokeModelStatic(string typeName, string methodName, params object[] arguments)
        {
            return InvokeStatic("Unity.Model", typeName, methodName, arguments);
        }

        private static object InvokeStatic(string assemblyName, string typeName, string methodName, params object[] arguments)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(currentAssembly => currentAssembly.GetName().Name == assemblyName);
            Assert.That(assembly, Is.Not.Null, $"Assembly not found: {assemblyName}");

            Type type = assembly.GetType(typeName);
            Assert.That(type, Is.Not.Null, $"Type not found: {typeName}");

            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, $"Method not found: {typeName}.{methodName}");

            return method.Invoke(null, arguments);
        }

        private static object GetValue(object target, string memberName)
        {
            FieldInfo fieldInfo = target?.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(target);
            }

            PropertyInfo propertyInfo = target?.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return propertyInfo?.GetValue(target);
        }

        private static object GetEntryByTreeName(object template, string treeName)
        {
            object entries = GetValue(template, "EntriesByTreeName");
            MethodInfo tryGetValue = entries?.GetType().GetMethod("TryGetValue");
            Assert.That(tryGetValue, Is.Not.Null);

            object[] args = { treeName, null };
            bool found = (bool)tryGetValue.Invoke(entries, args);
            return found ? args[1] : null;
        }
    }
}
