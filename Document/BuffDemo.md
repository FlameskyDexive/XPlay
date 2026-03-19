# BuffDemo 验收说明

## 1. 文档目的

本文档用于说明当前项目里新增的 Buff 规则化演示链如何使用、如何观察、以及如何验收。

本演示链覆盖了以下能力：

- Buff 多态 `EffectData` / `ModifierData`
- `AddPolicy`
- `SnapshotPolicy`
- `RemovePolicy`
- `TagGrantMask`
- `TagBlockMask`
- `CanDispel`
- `KeepOnDeath`
- `PeriodicStartDelayMs`
- `OnExpire` / `OnRemove` / `OnDispel`

---

## 2. 演示资源一览

### 2.1 Buff 配置

- `8015`：测试-沉默
- `8016`：测试-霸体
- `8017`：测试-眩晕

### 2.2 ActionEvent 配置

- `9`：`apply_silence_target`
- `10`：`apply_superarmor_self`
- `11`：`apply_stun_target`
- `12`：`dispel_stun_target`
- `13`：`apply_superarmor_target`

### 2.3 Skill 配置

- `1003`：目标霸体
- `1004`：状态连击

### 2.4 默认玩家技能顺序

当前玩家默认技能顺序已调整为：

- 主动技能 1：`1003` 目标霸体
- 主动技能 2：`1004` 状态连击
- 后续保留旧技能：`1001`、`1002`

服务端默认装配位置：

- `Unity/Assets/Scripts/Hotfix/Server/GamePlay/Map/Unit/UnitFactory.cs`

---

## 3. Buff 规则语义

### 3.1 沉默 Buff `8015`

- 授予标签：`Silence`
- 移除策略：`RemoveOrDispel`
- 可驱散：`true`

效果：目标无法释放技能。

### 3.2 霸体 Buff `8016`

- 授予标签：`SuperArmor`
- 移除策略：`ExpireOnly`
- 可驱散：`false`

效果：目标获得霸体；普通移除与驱散都无效，只能自然到期。

### 3.3 眩晕 Buff `8017`

- 授予标签：`HardControl`
- 阻断标签：`SuperArmor`
- 移除策略：`DispelOnly`
- 可驱散：`true`

效果：

- 对普通目标可生效
- 对已有霸体的目标会被 `TagBlockMask` 阻止添加
- 添加后不能普通移除，只能驱散或自然结束以外的内部强制移除逻辑处理

---

## 4. 演示技能语义

### 4.1 技能 `1003`：目标霸体

时间轴：

- `10%` 时触发事件 `13`

效果：

- 给当前目标施加 `8016` 霸体 Buff

### 4.2 技能 `1004`：状态连击

时间轴：

- `10%` 时触发事件 `9`
- `40%` 时触发事件 `11`
- `80%` 时触发事件 `12`

效果：

- 先给当前目标施加沉默
- 再尝试施加眩晕
- 最后尝试驱散眩晕

因此它可覆盖两种典型场景：

1. **目标无霸体**
   - 沉默成功
   - 眩晕成功
   - 驱散成功

2. **目标已有霸体**
   - 沉默成功
   - 眩晕被 `TagBlockMask` 阻断
   - 驱散眩晕时通常找不到该 Buff，或不会产生驱散成功日志

---

## 5. Robot 控制台命令

### 5.1 进入 Robot 模式

先进入控制台 `Robot` 模式：

```text
Robot
```

进入后提示符会变成：

```text
Robot>
```

### 5.2 BuffDemo 子命令

在 `Robot>` 模式下可执行：

```text
BuffDemo target
BuffDemo skill1
BuffDemo skill2
BuffDemo inspect
BuffDemo run
```

### 5.3 子命令说明

- `BuffDemo target`
  - 自动选取最近的可战斗目标
  - 打印当前目标的标签与 Buff 状态

- `BuffDemo skill1`
  - 自动选目标
  - 发送主动技能 1（目标霸体）操作

- `BuffDemo skill2`
  - 自动选目标
  - 发送主动技能 2（状态连击）操作

- `BuffDemo inspect`
  - 打印当前目标的标签与 Buff 状态

- `BuffDemo run`
  - 自动选目标
  - 先发技能 1，再延时发技能 2
  - 最后打印当前目标的标签与 Buff 状态

---

## 6. 预期日志

### 6.1 控制台日志关键字

`BuffDemo` 命令会输出：

- `BuffDemo target set`
- `BuffDemo inspect`
- `BuffDemo: queued skill1`
- `BuffDemo: queued skill2`

### 6.2 服务端 Buff 日志关键字

当前 Buff 规则链增加了以下关键日志：

- `action event add buff`
- `buff apply success`
- `buff apply blocked by tags`
- `buff apply rejected by add policy`
- `buff remove blocked by remove policy`
- `buff dispel request accepted`
- `buff dispel blocked by policy`
- `buff remove internal`
- `buff destroy`

---

## 7. 验收场景

### 场景 A：验证霸体阻断眩晕

执行：

```text
Robot
BuffDemo run
```

预期：

1. 技能 1 先给目标上 `8016` 霸体
2. 技能 2 的沉默 `8015` 成功施加
3. 技能 2 的眩晕 `8017` 被 `SuperArmor` 阻断
4. 日志中可看到：
   - `buff apply success`（霸体）
   - `buff apply success`（沉默）
   - `buff apply blocked by tags`（眩晕）

### 场景 B：验证眩晕可被驱散

执行顺序：

```text
Robot
BuffDemo target
BuffDemo skill2
BuffDemo inspect
```

建议前提：目标身上没有霸体。

预期：

1. 沉默成功
2. 眩晕成功
3. 后续驱散眩晕成功
4. 日志中可看到：
   - `buff apply success`（沉默）
   - `buff apply success`（眩晕）
   - `buff dispel request accepted`
   - `buff destroy ... reason:Dispel`

### 场景 C：验证霸体不可驱散

执行顺序：

```text
Robot
BuffDemo target
BuffDemo skill1
BuffDemo inspect
```

然后如果后续通过普通移除或驱散尝试处理 `8016`，预期：

- 普通移除被 `ExpireOnly` 拒绝
- 驱散被 `CanDispel=false` / `RemovePolicy` 拒绝

可关注日志：

- `buff remove blocked by remove policy`
- `buff dispel blocked by policy`

---

## 8. inspect 输出解释

`BuffDemo inspect` 会打印：

- `unit=xxx`
- `tags=...`
- `buffCount=...`

若存在 Buff，还会逐条打印：

- `buff id`
- `name`
- `layer`
- `reason`
- `tags`

用于快速确认：

- 当前目标身上有什么 Buff
- 当前目标战斗标签是否正确授予
- Buff 最终是如何退出的（`Remove` / `Expire` / `Dispel`）

---

## 9. 常见问题

### 9.1 `BuffDemo fail: no combat target found`

说明当前地图里没有可选中的最近战斗目标。

检查：

- 是否已经进图
- 附近是否有 Monster / Player 单位
- 目标是否已死亡

### 9.2 `BuffDemo fail: currentScene is null`

说明机器人客户端还没有成功进入当前场景。

### 9.3 只看到 `queued skill`，但没有 Buff 日志

检查：

- 是否成功发送到房间
- 当前目标是否为空
- 技能是否在 CD
- 目标是否死亡

### 9.4 看到 `buff apply blocked by tags`

说明该 Buff 被 `TagBlockMask` 挡住了，这属于预期行为，不一定是 Bug。

---

## 10. 相关文件

- `Document/BuffDemo.md`
- `Document/BattleSystem.md`
- `Unity/Assets/Config/Excel/Datas/BuffConfig.xlsx`
- `Unity/Assets/Config/Excel/Datas/ActionEventConfig.xlsx`
- `Unity/Assets/Config/Excel/Datas/SkillConfig.xlsx`
- `Unity/Assets/Scripts/Hotfix/Server/GamePlay/Robot/RobotConsoleHandler.cs`
- `Unity/Assets/Scripts/Hotfix/Server/GamePlay/Map/Unit/UnitFactory.cs`
- `Unity/Assets/Scripts/Hotfix/Share/GamePlay/Battle/Buff/BuffComponentSystem.cs`

