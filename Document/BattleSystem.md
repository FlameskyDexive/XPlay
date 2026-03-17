# RPG 战斗系统设计方案

## 1. 文档目的

本文档基于当前项目已有的：

- 行为树系统
- 技能系统
- Buff 系统
- ET 分层与 ECS 规范
- 客户端单位视图体系
- Animancer Pro 动画插件

设计一套适用于 RPG 的实时战斗系统。

目标不是推翻现有实现，而是在现有基础上进行体系化升级，形成一套：

- 符合 ET 规范
- 服务端权威
- 渲染与逻辑分离
- 支持技能 / Buff / 控制 / 受击 / 死亡
- 支持行为树 AI 决策
- 支持 Animancer Pro 动画驱动
- 支持配置扩展与持续迭代

的正式战斗底座。

---

## 2. 战斗定位

### 2.1 战斗形态

本方案采用：

- 服务端权威
- 实时 ARPG
- 非锁步
- 逻辑时间轴驱动
- 客户端负责表现反馈

### 2.2 适用范围

适用于：

- 玩家实时战斗
- 怪物 AI 战斗
- 单体技能 / 范围技能 / 投射物技能
- 持续施法 / 引导技能 / 蓄力技能
- Buff / Debuff / 控制 / 位移
- PvE 与 PvP 的统一战斗底座

### 2.3 非目标

当前方案不包含：

- 锁步战斗重构
- 回合制战斗
- 根运动驱动权威位移
- 完整连招编辑器
- 战斗回放系统

---

## 3. 设计原则

### 3.1 ET ECS 规范

严格遵守 ET 架构约束：

- `Entity` 只保存数据，不写业务逻辑
- `System` 只负责编排与生命周期入口
- 复杂逻辑统一下沉到 `Helper`
- `Model` 放共享数据定义
- `Hotfix` 放逻辑实现
- `ModelView` 放客户端表现数据
- `HotfixView` 放客户端表现逻辑

### 3.2 渲染逻辑分离

这是本方案的核心原则之一：

- 逻辑层只产生战斗状态和战斗结果
- 表现层只消费这些结果并驱动动画、特效、音效、材质、镜头和 UI

补充说明：

- 客户端正式动画驱动实现统一采用 `Animancer Pro`
- 逻辑层不允许直接引用 `Animancer`
- `Animancer` 仅允许出现在 `ModelView` / `HotfixView`
- 逻辑层只能写入动画语义状态或表现 Cue，不能直接播放 `Transition`

明确禁止：

- 在逻辑层直接调用 `AnimancerComponent` 或 `Animator`
- 在动画事件里直接做权威命中结算
- 在客户端表现层直接改权威 HP / Buff / 状态

### 3.3 服务端权威

服务端负责：

- 技能释放是否合法
- 技能时间轴推进
- 命中判定
- 伤害 / 治疗 / Buff / Debuff / 控制
- 位移与受击结果
- 死亡与复活

客户端负责：

- 输入采集
- 施法预反馈
- 动画与表现播放
- 特效 / 音效 / 镜头 / UI 展示

### 3.4 配置驱动

必须坚持：

- 战斗规则尽可能配置化
- 技能效果通过配置装配
- Buff 规则通过配置装配
- 表现 Cue 通过配置装配
- 禁止基于技能 ID / Buff ID 写大面积硬编码分支

### 3.5 AI 与玩家统一战斗链路

玩家与 AI 的区别只在于输入源不同：

- 玩家由输入组件发起施法请求
- AI 由行为树节点发起施法请求

二者最终都进入同一套：

- 目标选择
- 施法校验
- 技能时间轴
- Buff 处理
- 命中结算
- 状态切换
- 表现事件

---

## 4. 当前系统基线

### 4.1 已有基础

当前项目已经具备以下良好基础：

#### 行为树

现有行为树系统已经具备：

- `BTComponent` 挂载入口
- 黑板系统
- 运行时 Session
- 异步节点恢复
- typed leaf 扩展能力
- Editor 导出与运行时解耦

说明 AI 决策层不需要重做，只需要新增战斗相关节点。

#### 技能系统

现有技能系统已经具备：

- `SkillComponent`
- `Skill`
- `SkillTimelineComponent`
- `ActionEvent`
- 技能 CD
- 基于百分比的技能事件触发时间轴

说明“技能 = 时间轴 + 事件”的方向是正确的。

#### Buff 系统

现有 Buff 系统已经具备：

- `BuffComponent`
- `Buff`
- 生命周期
- 持续时间
- 周期触发
- `Start` / `Trigger` / `End` 事件

#### 动画基础

当前仓库已导入 `Animancer Pro`，可直接作为正式表现层动画方案。

### 4.2 当前主要问题

虽然已有基础，但距离完整 RPG 战斗系统仍存在明显缺口：

#### 技能系统缺少

- 统一施法请求
- 前摇 / 后摇 / 引导
- 打断
- 技能队列
- 公共冷却
- 资源消耗
- 状态限制
- 目标合法性检测

#### Buff 系统缺少

- 明确施加来源
- 叠层策略
- 分组冲突规则
- 驱散规则
- 标签授予与阻塞
- 快照策略

#### 结算系统缺少

- 命中 / 闪避
- 暴击
- 护盾
- 伤害加成与减伤
- 控制免疫
- 统一战斗结果发布

#### 表现层缺少

- 战斗语义状态机
- 上下身分层动作
- 高优先级受击覆盖
- 倒地 / 起身 / 死亡完整链路
- 通过配置映射动作资源而非写死字符串

---

## 5. 总体架构

### 5.1 主流程

```text
玩家输入 / AI行为树
        ↓
施法请求 SkillCastRequest
        ↓
施法校验 SkillValidation
        ↓
战斗状态切换 CombatState
        ↓
技能时间轴 SkillTimeline
        ↓
逻辑事件分发 CombatActionDispatcher
        ↓
命中 / 伤害 / Buff / 控制 / 位移
        ↓
战斗结果 CombatDelta
        ↓
表现 Cue CombatCue
        ↓
客户端 Animancer / 特效 / 音效 / UI
```

### 5.2 分层职责

#### Model

职责：

- 战斗数据定义
- 枚举
- 配置映射
- Entity / Component 数据字段
- 公共 DTO

#### Hotfix

职责：

- 技能释放校验
- 战斗状态推进
- 时间轴推进
- Buff 生命周期
- 目标 / 仇恨 / 控制
- 命中与伤害结算
- 行为树战斗节点

#### ModelView

职责：

- 客户端表现态数据组件
- Animancer 资源引用与运行时缓存
- 表现队列缓存

#### HotfixView

职责：

- Animancer 驱动
- Transition 播放与层管理
- 技能动画、受击动画、死亡动画切换
- 特效 / 音效 / 镜头反馈
- 材质切换
- UI 刷新

---

## 6. 模块拆分

### 6.1 逻辑模块

#### 战斗核心

- `CombatStateComponent`
- `SkillCastComponent`
- `TargetComponent`
- `ThreatComponent`

#### 技能

- `SkillComponent`
- `Skill`
- `SkillTimelineComponent`
- `SkillCastRequest`

#### Buff

- `BuffComponent`
- `Buff`
- `BuffApplyRequest`
- `BuffModifier`

#### 结算

- `CombatResolverHelper`
- `DamageResolveHelper`
- `HealResolveHelper`
- `ControlResolveHelper`

#### 目标与仇恨

- `TargetSelectHelper`
- `ThreatHelper`

### 6.2 表现模块

#### 动画

- `AnimancerViewComponent`
- `CombatAnimStateComponent`
- `CombatAnimRuntimeComponent`
- `CombatAnimDriverSystem`
- `CombatCueAnimancerExecutor`

#### Cue

- `CombatCueComponent`
- `CombatCueData`
- `CombatCueDispatcher`
- `EffectCuePlayer`
- `SoundCuePlayer`
- `CameraCuePlayer`

#### UI

- `BattleHudComponent`
- `BuffBarView`
- `CastBarView`
- `CombatFloatTextView`

---

## 7. 单位运行时组件模型

### 7.1 每个可战斗单位固定组件

#### 逻辑层

- `NumericComponent`
- `CombatStateComponent`
- `SkillComponent`
- `SkillCastComponent`
- `BuffComponent`
- `TargetComponent`
- `ThreatComponent`

#### 表现层

- `GameObjectComponent`
- `AnimancerViewComponent`
- `CombatAnimStateComponent`
- `CombatAnimRuntimeComponent`
- `CombatCueComponent`

#### AI 单位额外

- `BTComponent`

---

## 8. 核心组件设计

### 8.1 CombatStateComponent

负责描述单位当前的权威战斗状态。

建议字段：

- `State`
- `SubState`
- `TagMask`
- `StateVersion`
- `StateEndTime`
- `CurrentCastSkillId`
- `CurrentTargetId`
- `LastHitTime`
- `InterruptLevel`

### 8.2 SkillCastComponent

负责记录当前施法过程中的运行时上下文。

建议字段：

- `CurrentSkillId`
- `CurrentSkillConfigId`
- `CurrentCastSeq`
- `TargetUnitId`
- `AimPoint`
- `AimDirection`
- `CastStartTime`
- `CastPointTime`
- `RecoverEndTime`
- `NextGlobalCdEndTime`
- `QueuedRequest`

### 8.3 TargetComponent

负责当前目标与辅助目标管理。

建议字段：

- `CurrentTargetId`
- `LastTargetId`
- `LockTarget`
- `AssistTargetId`

### 8.4 ThreatComponent

负责怪物 / AI 的仇恨值管理。

建议字段：

- `ThreatMap`
- `PrimaryTargetId`
- `LastThreatUpdateTime`

### 8.5 CombatAnimStateComponent

负责客户端表现层的语义动画状态，是逻辑层与 Animancer 表现层之间的桥接组件。

建议字段：

- `DesiredLocomotionState`
- `DesiredCombatState`
- `DesiredCombatSubState`
- `DesiredSkillAnimId`
- `DesiredHitAnimId`
- `LocomotionSpeed`
- `FacingMode`
- `StateVersion`
- `BaseStateDirty`
- `UpperStateDirty`
- `OverlayStateDirty`

逻辑层不直接操作 `AnimancerComponent`，而是写入该组件，由表现层决定：

- 播放哪个 `Transition`
- 使用哪个 `Layer`
- 以何种优先级切换
- 动作结束后回退到何状态

### 8.6 CombatAnimRuntimeComponent

负责缓存 Animancer 运行时播放句柄。

建议字段：

- `CurrentBaseState`
- `CurrentUpperState`
- `CurrentOverlayState`
- `CurrentPriority`
- `LastStateVersion`
- `IsLockedByDeath`
- `IsLockedByKnockDown`

### 8.7 CombatCueComponent

负责待播放表现事件队列。

建议字段：

- `PendingCues`

---

## 9. 数值系统设计

### 9.1 继续沿用 NumericComponent

本方案不新开第二套属性系统，继续使用现有 `NumericComponent` 作为底座。

### 9.2 建议新增数值项

在现有 `Hp`、`MaxHp`、`Attack`、`Speed` 基础上补充：

- `Defense`
- `MagicDefense`
- `CritChance`
- `CritDamage`
- `CritResist`
- `HitRate`
- `Dodge`
- `AttackSpeed`
- `CastSpeed`
- `DamageBonusPct`
- `DamageTakenPct`
- `HealingBonusPct`
- `Shield`
- `Tenacity`
- `Mana`
- `MaxMana`
- `Energy`
- `MaxEnergy`

### 9.3 Buff 修改数值方式

Buff 不直接写死改 `Hp` 或 `Attack`，统一通过以下通道修饰：

- `Base`
- `Add`
- `Pct`
- `FinalAdd`
- `FinalPct`

---

## 10. 技能系统设计

### 10.1 技能释放统一入口

当前 `SpellSkill` 仅保留为兼容壳，正式入口统一升级为：

- `TryRequestCast`
- `StartCast`
- `TickCast`
- `InterruptCast`
- `FinishCast`

### 10.2 施法请求对象

新增 `SkillCastRequest`：

- `SkillSlot`
- `SkillId`
- `TargetUnitId`
- `AimPoint`
- `AimDirection`
- `ClientCastSeq`
- `PressedTime`

### 10.3 技能释放流程

1. 发起请求
2. 施法校验
3. 进入施法状态
4. 推进到释放点
5. 时间轴触发逻辑事件
6. 进入后摇恢复

### 10.4 技能阶段划分

每个技能统一拆分为五段：

- `Request`
- `PreCast`
- `CastPoint`
- `ActiveWindow`
- `Recover`

### 10.5 技能类型

建议支持：

- `Instant`
- `CastPoint`
- `Channel`
- `Charge`
- `Toggle`

### 10.6 技能校验失败原因

统一定义：

- `InCd`
- `NoResource`
- `Dead`
- `Controlled`
- `NoTarget`
- `OutOfRange`
- `InvalidState`
- `BlockedByTag`

---

## 11. SkillTimeline 设计

### 11.1 基本原则

继续保留技能时间轴方案，但升级为：

- 逻辑轨
- 表现轨

### 11.2 逻辑轨

逻辑轨负责：

- 伤害
- 治疗
- 加 Buff
- 移除 Buff
- 投射物
- 位移
- 控制
- 驱散

### 11.3 表现轨

表现轨负责：

- 动画 Cue
- 特效
- 音效
- 镜头震动
- 材质切换
- UI 浮字

### 11.4 核心原则

命中结算永远由逻辑时间轴驱动，不由动画事件驱动。

---

## 12. Buff 系统设计

### 12.1 Buff 核心定位

Buff 是挂在 `Unit` 身上的独立运行时实体，负责：

- 状态授予
- 周期触发
- 数值修饰
- 附加逻辑
- 生命周期管理

### 12.2 Buff 运行时字段扩展

建议新增：

- `SourceUnitId`
- `SourceSkillId`
- `CasterSnapshotVersion`
- `ExpireTime`
- `NextTickTime`
- `LayerCount`
- `GroupId`
- `AddPolicy`
- `SnapshotPolicy`
- `TagGrantMask`
- `TagBlockMask`
- `ModifierHandles`

### 12.3 生命周期

统一生命周期：

- `OnApply`
- `OnTick`
- `OnExpire`
- `OnRemove`
- `OnDispel`

### 12.4 叠层策略

统一支持：

- `RefreshDuration`
- `RejectNew`
- `StackAndRefresh`
- `StackOnly`
- `ReplaceByStronger`

### 12.5 快照规则

支持：

- `Snapshot`
- `Dynamic`

---

## 13. 命中与结算系统设计

### 13.1 统一结算入口

统一使用：

- `CombatResolverHelper`
- `DamageResolveHelper`
- `HealResolveHelper`
- `BuffResolveHelper`
- `ControlResolveHelper`

### 13.2 统一结算顺序

1. 目标有效性检查
2. 命中判定
3. 暴击判定
4. 基础伤害 / 治疗值计算
5. 防御与修正乘区结算
6. 护盾处理
7. HP 变更
8. 死亡判定
9. 发布战斗结果与表现 Cue

### 13.3 统一战斗结果类型

建议支持：

- `Damage`
- `Heal`
- `CritDamage`
- `Dodge`
- `Block`
- `Immune`
- `ShieldBroken`

---

## 14. 控制系统设计

建议支持：

- `Silence`
- `Stun`
- `Root`
- `Fear`
- `KnockBack`
- `KnockDown`
- `Freeze`
- `Taunt`

统一打断等级：

- `None`
- `Soft`
- `Hard`
- `Fatal`

---

## 15. 动画驱动设计（Animancer Pro 版）

### 15.1 总体原则

本方案的动画驱动不再以 `Animator Controller 参数 + Trigger` 作为主入口，而是统一改为：

- `AnimancerComponent`
- `Animancer Transition`
- `Animancer Layer`
- `Animancer FSM`

进行驱动。

核心原则如下：

- 逻辑层不直接引用 `Animancer`
- 权威战斗逻辑不依赖动画事件
- 动画只负责表现，不负责伤害 / Buff / 命中
- 所有战斗表现都通过语义状态或 Cue 进入 View 层
- 玩家与 AI 共用同一套逻辑链路，但动画都在客户端表现层执行

### 15.2 为什么采用 Animancer Pro

相较于传统 Animator 参数驱动，Animancer Pro 更适合 RPG 战斗系统，原因包括：

- 更适合技能、受击、倒地、起身、死亡等短时动作切换
- 更适合上下身分层播放
- 更适合通过 `Transition` 资产管理技能动作资源
- 更适合在代码中精确控制播放、淡入淡出、停止和回退
- 更容易把逻辑状态映射为表现动作，减少 Controller 图膨胀

因此本方案中：

- `Animator Controller` 不再作为主要业务状态机
- `Animancer` 成为正式战斗动画播放入口
- `AnimatorComponent` 退化为兼容层，不再是主战斗 API

### 15.3 分层边界

#### 逻辑层

逻辑层包括：

- `Model`
- `Hotfix`

逻辑层只负责：

- 技能施法状态
- 命中与结算
- Buff 与控制
- 战斗状态变化
- 生成表现语义和 Cue

逻辑层禁止：

- 直接调用 `AnimancerComponent.Play`
- 直接访问 `Transition`
- 直接播放动画、特效、音效

#### 表现层

表现层包括：

- `ModelView`
- `HotfixView`

表现层负责：

- 持有 `AnimancerComponent`
- 持有动画资源映射
- 根据战斗状态切换动画
- 播放技能动作、受击动作、死亡动作
- 播放 Cue 对应的特效、音效、镜头反馈

### 15.4 动画资源模型

统一采用 `Transition` 资源驱动，不再以字符串动画名为主驱动方式。

建议新增资源配置：

- `UnitAnimancerConfig`
- `SkillAnimancerCueMap`
- `HitAnimancerCueMap`

#### UnitAnimancerConfig

为某一类单位定义完整动画资源集合，建议包含：

- `IdleTransition`
- `MoveTransition` 或 `LocomotionMixer`
- `RunTransition`
- `DeathTransition`
- `ReviveTransition`
- `HitLightTransition`
- `HitHeavyTransition`
- `KnockDownTransition`
- `GetUpTransition`
- `UpperBodyMask`
- `SkillTransitions`

#### SkillAnimancerCueMap

用于把技能动作语义映射为具体 `Transition`，建议字段：

- `SkillAnimId`
- `CastStart`
- `CastLoop`
- `CastRelease`
- `Recover`
- `UpperBodyOnly`
- `FadeDuration`
- `CanInterruptByHitLight`
- `CanInterruptByHitHeavy`

#### HitAnimancerCueMap

用于受击动作映射，建议字段：

- `HitAnimId`
- `Transition`
- `Priority`
- `UseOverlayLayer`
- `CanBreakCasting`
- `AutoReturn`

### 15.5 表现层组件设计

#### AnimancerViewComponent

职责：

- 持有单位视图上的 `AnimancerComponent`
- 持有单位对应的 `UnitAnimancerConfig`
- 封装各层播放入口

建议字段：

- `AnimancerComponent Animancer`
- `UnitAnimancerConfig Config`
- `bool IsInitialized`

#### CombatAnimStateComponent

职责：

- 接收逻辑层同步来的动画语义状态
- 作为 View 层动画驱动输入

#### CombatAnimRuntimeComponent

职责：

- 缓存当前正在播放的 `Animancer State`
- 管理优先级与中断
- 处理锁定状态，如死亡或倒地锁定

### 15.6 Animancer 层设计

建议统一使用三层：

#### Layer 0：Base / Locomotion

负责：

- `Idle`
- `Move`
- `Run`
- `Death`
- `GetUp`
- `KnockDown`

#### Layer 1：Upper Body Combat

负责：

- `CastStart`
- `CastLoop`
- `CastRelease`
- 普攻动作
- 上半身技能动作

#### Layer 2：Overlay / Hit Reaction

负责：

- `HitLight`
- `HitHeavy`
- `KnockBack`
- 临时高优先级覆盖动作

### 15.7 Animancer FSM 设计

Animancer FSM 在本方案中只承担客户端表现状态机职责，不承担权威战斗逻辑职责。

建议建立三套状态机：

- `BaseStateMachine`
- `UpperBodyStateMachine`
- `OverlayStateMachine`

建议状态类型：

- `IdleAnimState`
- `MoveAnimState`
- `CastStartAnimState`
- `CastLoopAnimState`
- `CastReleaseAnimState`
- `HitLightAnimState`
- `HitHeavyAnimState`
- `KnockDownAnimState`
- `GetUpAnimState`
- `DeathAnimState`

优先级建议固定为：

1. `Death`
2. `KnockDown / GetUp`
3. `HitHeavy`
4. `HitLight`
5. `CastRelease`
6. `CastLoop`
7. `CastStart`
8. `Move`
9. `Idle`

### 15.8 技能动画驱动方式

每个技能统一拆分为：

- `CastStart`
- `CastLoop`
- `CastRelease`
- `Recover`

逻辑层通过：

- `DesiredSkillAnimId`
- `DesiredCombatState`
- `DesiredCombatSubState`

表达语义。

表现层根据 `SkillAnimId` 查找 `SkillAnimancerCueMap`，再决定：

- 播放哪个 `Transition`
- 使用 Base 层还是 Upper Body 层
- 使用多长淡入
- 是否允许轻受击打断
- 动作结束后回到什么状态

关键原则：

- 技能命中时间点仍由技能时间轴决定，而不是由 Animancer 播放结束决定
- `Animancer Events` / `OnEnd` 只允许用于动作播放结束回退、清理上层状态和本地表现串联
- 禁止用 Animancer 事件做权威伤害结算、Buff 生效、命中判定、位移权威结果

### 15.9 受击、倒地、死亡动画

建议统一区分：

- `HitLight`
- `HitHeavy`
- `KnockBack`
- `KnockDown`
- `Death`

播放规则：

- `HitLight` 使用 Overlay 层，短时覆盖
- `HitHeavy` 使用 Overlay 层或更高优先级覆盖
- `KnockDown` 切换到 Base 层并锁定
- `Death` 抢占所有层并锁定，直到复活

### 15.10 移动动画

移动不再主要依赖 Animator 参数 `Speed` 去驱动复杂状态图，而改为：

- `Locomotion Transition`
- 或 `MixerTransition`

推荐：

- 有资源时使用 `LinearMixerTransition` / 2D Mixer
- 资源不足时先保留 `Idle + Run` 两态切换

### 15.11 Animancer API 设计

旧的：

- `Play(MotionType)`
- `SetTrigger`
- `SetFloatValue`
- `SetIntValue`

不再作为正式战斗动画 API。

新的正式 API 统一收口到 View 层：

#### AnimancerViewComponentSystem

建议提供：

- `Initialize`
- `PlayBase`
- `PlayUpper`
- `PlayOverlay`
- `StopUpper`
- `StopOverlay`
- `SetLayerWeight`

#### CombatAnimDriverHelper

建议提供：

- `ApplyLocomotionState`
- `ApplyCombatState`
- `ApplyHitReaction`
- `ApplyDeathState`
- `ResolveSkillTransition`
- `ResolveHitTransition`

#### CombatCueAnimancerExecutor

建议提供：

- `ExecuteAnimationCue`
- `ExecuteUpperBodySkillCue`
- `ExecuteOverlayHitCue`

所有这些 API 的输入都应是：

- `SkillAnimId`
- `HitAnimId`
- `CueId`
- `ECombatState`
- `ECombatSubState`
- `ITransition`

而不是字符串动画名。

### 15.12 单位视图初始化要求

单位视图创建后，应统一完成以下初始化：

1. 挂载 `GameObjectComponent`
2. 获取或创建 `AnimancerComponent`
3. 挂载 `AnimancerViewComponent`
4. 挂载 `CombatAnimStateComponent`
5. 挂载 `CombatAnimRuntimeComponent`
6. 挂载 `CombatCueComponent`
7. 加载对应的 `UnitAnimancerConfig`
8. 播放默认 `IdleTransition`

正式动画入口是：

- `AnimancerViewComponent`

而不是旧的：

- `AnimatorComponent`

### 15.13 与逻辑层的接口边界

逻辑层输出的仍然是：

- 当前战斗主状态
- 当前战斗子状态
- 当前技能动作语义
- 当前受击动作语义
- 当前表现 Cue

表现层负责把这些语义翻译成：

- `Transition`
- `Layer`
- `FadeDuration`
- `Animancer State`
- `FSM` 状态切换

### 15.14 验收标准

引入 Animancer 后，至少满足以下验收标准：

- 单位进入场景后默认能播放 `Idle`
- 移动状态能平滑切换 Locomotion
- 技能前摇、引导、释放、收招动作正常播放
- 受击能正确打断或覆盖相应表现层状态
- 死亡会抢占所有层并锁定
- 动画丢帧或播放异常不影响服务端命中和结算
- 所有技能/受击动画都通过配置映射，而不是硬编码字符串

---

## 16. 表现事件系统设计

### 16.1 为什么拆表现事件

当前 `ActionEvent` 不适合继续同时承载逻辑与表现，因此后续统一拆成：

- 逻辑事件
- 表现 Cue

### 16.2 逻辑事件

逻辑事件由服务端处理，例如：

- 造成伤害
- 加 Buff
- 驱散
- 位移
- 召唤投射物
- 控制

### 16.3 表现 Cue

表现 Cue 由客户端处理，例如：

- 通过 `Animancer` 播放技能动作
- 播放受击动作
- 播放特效
- 播放音效
- 相机震动
- 材质变化
- UI 浮字

动画类 Cue 不直接携带字符串动画名，而应携带：

- `CueId`
- `SkillAnimId`
- `HitAnimId`
- `LayerPolicy`

客户端再通过本地 `Animancer` 资源映射查找对应的 `Transition`。

### 16.4 Cue 类型

建议定义：

- `Animation`
- `Effect`
- `Sound`
- `CameraShake`
- `Material`
- `UI`

---

## 17. AI 与行为树整合设计

### 17.1 总体原则

行为树只负责：

- 找目标
- 判断距离
- 选择技能
- 决定是否释放
- 决定是否追击 / 撤退

行为树不负责：

- 直接播放动画
- 直接扣血
- 直接改 Buff

### 17.2 黑板键约定

建议统一以下黑板键：

- `Combat.TargetId`
- `Combat.TargetDistance`
- `Combat.HasTarget`
- `Combat.SelectedSkillId`
- `Combat.SelectedSkillSlot`
- `Combat.CanCast`
- `Combat.InCast`
- `Combat.InControl`
- `Combat.HpRatio`
- `Combat.IsDead`
- `Combat.NeedRetreat`

### 17.3 建议新增 typed leaf

- `BTFindCombatTarget`
- `BTValidateCombatTarget`
- `BTClearInvalidTarget`
- `BTMoveToCombatRange`
- `BTFaceTarget`
- `BTSelectSkill`
- `BTCanCastSelectedSkill`
- `BTCastSelectedSkill`
- `BTWaitCastComplete`
- `BTNeedRetreat`

---

## 18. 网络同步设计

### 18.1 输入侧

延续现有 `OperateInfo` 方向做扩展，建议新增：

- `SkillSlot`
- `TargetUnitId`
- `AimPoint`
- `AimDirection`
- `ClientCastSeq`
- `CastFlags`

### 18.2 服务端回包拆分

建议拆成三类：

#### CastAck

只回施法者：

- `ClientCastSeq`
- `ServerCastSeq`
- `Accepted`
- `RejectReason`
- `ServerStartTime`

#### CombatDelta

广播战斗结果：

- 状态变化
- HP 变化
- Buff 变化
- 目标变化
- 位移修正

#### CombatCueNotify

广播表现事件：

- `CueType`
- `CueId`
- `SourceId`
- `TargetId`
- `Socket`
- `PlayTime`

---

## 19. 配置设计

### 19.1 SkillConfig

在现有基础上新增：

- `TargetType`
- `CastType`
- `CostType`
- `CostValue`
- `Range`
- `MinRange`
- `Angle`
- `CastPointMs`
- `RecoverMs`
- `GcdMs`
- `QueueWindowMs`
- `CanMoveDuringCast`
- `CanRotateDuringCast`
- `InterruptRule`
- `CooldownGroupId`
- `ChargeMax`
- `ChargeRecoverMs`
- `AnimStateId`

### 19.2 BuffConfig

在现有基础上新增：

- `AddPolicy`
- `RemovePolicy`
- `SnapshotPolicy`
- `TagGrantMask`
- `TagBlockMask`
- `CanDispel`
- `KeepOnDeath`
- `PeriodicStartDelayMs`

### 19.3 ActionEventConfig

扩展为统一事件壳配置，新增：

- `Domain`
- `PayloadType`
- `PayloadId`
- `TargetRule`
- `SocketName`
- `SelectRule`

### 19.4 效果载荷配置表

建议新增：

- `DamageEffectConfig`
- `HealEffectConfig`
- `ApplyBuffEffectConfig`
- `RemoveBuffEffectConfig`
- `ProjectileEffectConfig`
- `DisplacementEffectConfig`
- `ControlEffectConfig`

### 19.5 表现载荷配置表

建议新增：

- `AnimationCueConfig`
- `EffectCueConfig`
- `SoundCueConfig`
- `CameraCueConfig`
- `MaterialCueConfig`

---

## 20. 实施阶段建议

### 阶段一：战斗状态骨架

目标：

- 补齐 `CombatStateComponent`
- 补齐 `SkillCastComponent`
- 把技能释放从 `SpellSkill` 升级为统一施法请求

### 阶段二：时间轴与结算升级

目标：

- SkillTimeline 双轨化
- 统一结算入口
- 替换 demo 级伤害流程

### 阶段三：Buff 规则化

目标：

- 叠层
- 刷新
- 替换
- 分组
- 驱散
- 快照

### 阶段四：Animancer 表现系统接入

目标：

- `AnimancerViewComponent`
- `CombatAnimStateComponent`
- `CombatAnimRuntimeComponent`
- `CombatCueComponent`
- Animancer `Layer` / `Transition` / `FSM` 驱动体系

重点内容：

- 用 `Animancer Pro` 替换原先基于 Animator 参数的主动画 API
- 建立 `Base / Upper / Overlay` 三层播放体系
- 建立客户端表现状态机
- 打通技能动作、受击动作、死亡动作与 Cue 播放

### 阶段五：行为树战斗节点

目标：

- 新增战斗 typed leaf
- 打通 AI 战斗行为

### 阶段六：网络协议与调试

目标：

- `Ack / Delta / Cue` 协议
- 战斗日志
- 黑板观测
- 技能时间轴调试输出

---

## 21. 测试与验收场景

### 21.1 技能释放

- 技能前摇、释放点、后摇完整生效
- CD 与 GCD 正确
- 距离不足时拒绝释放
- 无目标技能可正常释放

### 21.2 打断与控制

- 前摇中被眩晕可打断
- 霸体技能不被软打断
- 引导技能中途失去目标会终止
- 死亡会终止当前技能

### 21.3 Buff

- Buff 叠层正确
- Buff 刷新时间正确
- 分组冲突正确
- 驱散规则正确
- 死亡清理正确

### 21.4 结算

- 命中 / 闪避正确
- 暴击正确
- 护盾优先吸收
- 治疗不超过最大生命
- 死亡正确触发

### 21.5 动画表现

- 单位进入场景后能正确初始化 `AnimancerComponent`
- 默认 `IdleTransition` 正常播放
- 移动时 Locomotion 正常切换
- 施法起手 / 引导 / 释放 / 收招动作由 `Animancer` 正确驱动
- 受击动画可按优先级覆盖技能动作
- 死亡动画优先级最高
- 上下身分层动作能正常共存
- 客户端掉帧或动画异常不影响服务端权威结算

### 21.6 AI

- AI 能自动找目标
- AI 能追击到释放距离
- AI 能根据冷却选择技能
- AI 在控制中暂停施法
- AI 在目标死亡后切换目标

---

## 22. 风险与注意事项

### 22.1 不能继续混用逻辑与表现事件

若不拆逻辑事件与表现 Cue，后续技能越多越难维护。

### 22.2 不能让 Animancer 反驱动命中

如果命中依赖动画事件：

- 网络延迟下会错
- 掉帧时会错
- 客户端作弊风险高
- 服务端无法保持权威

### 22.3 不能大量技能 ID 硬编码

必须以：

- 配置
- Payload
- Helper
- 通用规则

为核心。

### 22.4 行为树只做决策

行为树节点不应直接绕过技能系统做伤害与 Buff，否则玩家与 AI 将出现两套战斗规则。

### 22.5 Animancer 只允许作为表现实现

Animancer 是表现层实现手段，不应污染 `Model` 与 `Hotfix` 权威逻辑模型。

---

## 23. 结论

本方案基于当前项目已存在的：

- 行为树运行时
- 技能时间轴
- Buff 生命周期
- ET 分层结构
- Animancer Pro 插件

提出一套完整的 RPG 实时战斗体系。

该体系的核心是：

- 服务端权威
- 逻辑时间轴驱动
- 表现与逻辑严格分离
- 玩家与 AI 共用同一战斗链路
- 行为树负责决策，技能系统负责执行，Animancer 负责客户端动画表现

通过本方案，可以在不推翻现有结构的前提下，逐步将当前战斗实现升级为一套可扩展、可维护、可配置、可联机的正式 RPG 战斗系统底座。

---

## 24. 默认约定

本文档默认采用以下前提：

- 战斗模式为服务端权威实时 ARPG
- 继续沿用 `NumericComponent`
- 继续沿用 `Skill` / `Buff` / `ActionEvent` 实体概念
- 行为树继续使用现有框架并新增 typed leaf
- 不使用根运动做权威逻辑
- 不做锁步
- 不做回合制
- 所有表现通过 Cue 系统进入客户端渲染层
- 所有权威逻辑只能在服务端战斗链路中完成
- 客户端正式动画驱动统一采用 `Animancer Pro`
