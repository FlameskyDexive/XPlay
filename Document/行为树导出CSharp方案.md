# 行为树导出 CSharp 方案

## 目标

- 在行为树编辑器中新增“导出 CSharp 代码”能力。
- 导出的代码直接构建 `BTPackage / BTDefinition / BTNodeData / BTNode`，减少运行时反序列化与重建行为树图的开销。
- 保留现有 `.bytes` 导出链路，运行时优先走 compiled provider，未命中时自动回退 `.bytes`。

## 当前实现结论

- 方案可行，并且已经与当前行为树架构对齐实现为双轨方案。
- 运行时保留现有 `BTExecutionSession / BTEnv / BTDispatcher / Handler` 体系。
- 生成代码不创建新的运行时节点子类，仍然实例化现有标准节点类型，避免 `BTDispatcher` 精确类型分发失配。

## 关键设计

### 1. 双轨加载

- `BTComponent` 同时支持：
  - `byte[] TreeBytes`
  - `string TreePackageKey`
- `BTComponentSystem.StartTree` 的运行顺序：
  1. `TreeBytes` 非空时优先走 bytes
  2. 否则按 `TreePackageKey` 查询 `BTCompiledTreeRegistry`
  3. registry 未命中时回退 `BTLoader.LoadBytes(TreePackageKey)`

### 2. Compiled 基础设施

- `BTCompiledTreeProviderAttribute`
- `IBTCompiledTreeProvider`
- `BTCompiledTreeTemplate`
- `BTCompiledTreeRegistry`
- `BTCompiledTreeTemplateBuilder`
- `BTCompiledTreeUtility`

这些类型放在 `Model/Share/Module/BehaviorTree`，这样 `Hotfix` 与 `Editor` 都可以复用。

### 3. 导出代码生成策略

- 编辑器先复用现有 `BTExporter.BuildPackage` 生成运行时包对象。
- 再由 `BTCSharpExporter` 将运行时包对象转成 `.g.cs`。
- 生成代码中会同时保留：
  - `BTPackage`
  - `BTDefinition`
  - `BTNodeData`
  - `BTNode`
  - 子树 `SubTreeRoot`
  - 每棵树的 `nodes` 映射

### 4. 命名规则

- **CSharp 导出命名改为使用 `TreeName`，不再使用 `TreeId`。**
- 原因：行为树创建/保存阶段不允许存在同名行为树，因此 `TreeName` 足够稳定且可读性更好。
- 该规则作用于：
  - 生成文件名
  - provider 类名
  - 生成方法名
  - 局部 tree 变量名

示例：

- 文件：`BT_AITest.g.cs`
- 类：`BTCompiledProvider_AITest`

## 已实现内容

### 运行时

- `BTComponent` 增加 compiled source 支持。
- `BTRuntimeFactory` 新增 `Create(Entity owner, BTCompiledTreeTemplate template, string treeIdOrName = "")`。
- `BTGraphBuilder` 改为复用 `BTRuntimeNodeFactory`，避免节点类型映射逻辑漂移。
- `BTCompiledTreeRegistry` 按 attribute 扫描 provider 并注册。

### 编辑器

- `BTEditorWindow` 新增：
  - `Export C#`
  - `Export Both`
- Inspector 中展示 compiled 输出路径。
- `BTExporter` 增加 `GetPackageKey`。
- `BTCSharpExporter` 已实现生成 `.g.cs` 文件。

### AITest 样例

- `BTClientDemoFactory` 现在同时提供：
  - `CreateAITestPackage()`
  - `CreateAITestBytes()`
- `AITest` 已接入 compiled provider 种子文件。

## 验证

- 已增加 parity / benchmark 测试：
  - `BehaviorTree_AITest_CompiledTemplate_Parity`
  - `BehaviorTree_AITest_CompiledVsBytes_Benchmark`
- 已执行 `dotnet build ET.sln`，构建通过。

## 后续建议

- 当前 `AITest` 是种子 provider，后续正式使用时可直接在编辑器中导出 `.g.cs`。
- 如果后续希望在 CLI 构建里自动纳入新增 `.g.cs`，可以进一步优化 Unity 生成的 `csproj` 管理方式。
- 如果未来确认所有行为树都走 compiled path，可以再评估是否弱化或移除 `.bytes` 链路。

