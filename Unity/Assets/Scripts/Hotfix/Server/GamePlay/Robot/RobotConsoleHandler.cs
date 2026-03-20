using System;
using System.Reflection;

namespace ET.Server
{
    [FriendOf(typeof(TargetComponent))]
    [FriendOf(typeof(CombatStateComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    [ConsoleHandler(ConsoleMode.Robot)]
    public class RobotConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(Fiber fiber, ModeContex contex, string content)
        {
            string[] ss = content.Split(" ");
            switch (ss[0])
            {
                case ConsoleMode.Robot:
                    break;

                case "Run":
                {
                    int caseType = int.Parse(ss[1]);

                    try
                    {
                        Log.Debug($"run case start: {caseType}");
                        await EventSystem.Instance.Invoke<RobotInvokeArgs, ETTask>(caseType, new RobotInvokeArgs() { Fiber = fiber, Content = content });
                        Log.Debug($"run case finish: {caseType}");
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"run case error: {caseType}\n{e}");
                    }
                    break;
                }
                case "RunAll":
                {
                    FieldInfo[] fieldInfos = typeof (RobotCaseType).GetFields();
                    foreach (FieldInfo fieldInfo in fieldInfos)
                    {
                        int caseType = (int)fieldInfo.GetValue(null);
                        if (caseType > RobotCaseType.MaxCaseType)
                        {
                            Log.Debug($"case > {RobotCaseType.MaxCaseType}: {caseType}");
                            break;
                        }
                        try
                        {
                            Log.Debug($"run case start: {caseType}");
                            await EventSystem.Instance.Invoke<RobotInvokeArgs, ETTask>(caseType, new RobotInvokeArgs() { Fiber = fiber, Content = content});
                            Log.Debug($"---------run case finish: {caseType}");
                        }
                        catch (Exception e)
                        {
                            Log.Debug($"run case error: {caseType}\n{e}");
                            break;
                        }
                    }
                    break;
                }
                case "LoadTree":
                {
                    string treeName = ss.Length > 1 ? ss[1] : "AITest";
                    BTPackage package = null;
                    if (BTCompiledTreeRegistry.Instance.TryGetTemplate(treeName, out BTCompiledTreeTemplate template))
                    {
                        package = template.Package;
                    }

                    package ??= BTLoader.Instance.LoadPackage(treeName, false);
                    if (package == null)
                    {
                        Log.Debug($"load behavior tree failed: {treeName}");
                        break;
                    }

                    BTDefinition entryTree = package.GetEntryTree();
                    Log.Debug($"behavior tree loaded: {treeName}, package={package.PackageName}, treeCount={package.Trees.Count}, entry={entryTree?.TreeName}, nodeCount={entryTree?.Nodes.Count ?? 0}");
                    break;
                }
                case "RunTree":
                {
                    string fileName = ss.Length > 1 ? ss[1] : "AITest";
                    string treeName = ss.Length > 2 ? ss[2] : "AITest";
                    BTExecutionSession session = null;
                    if (BTCompiledTreeRegistry.Instance.TryGetTemplate(fileName, out BTCompiledTreeTemplate template))
                    {
                        session = BTRuntime.Create(fiber.Root, template, treeName);
                    }

                    if (session == null)
                    {
                        byte[] bytes = await BTLoader.Instance.LoadBytesAsync(fileName, false);
                        if (bytes == null || bytes.Length == 0)
                        {
                            Log.Debug($"run behavior tree failed, bytes empty: {fileName}");
                            break;
                        }

                        session = BTRuntime.Create(fiber.Root, bytes, treeName);
                    }

                    if (session == null)
                    {
                        Log.Debug($"run behavior tree failed: {fileName}/{treeName}");
                        break;
                    }

                    BTFlowDriver.RunRoot(session);
                    await fiber.Root.GetComponent<TimerComponent>().WaitAsync(2200);
                    BTFlowDriver.Dispose(session);
                    Log.Debug($"behavior tree run finish: {fileName}/{treeName}");
                    break;
                }
                case "BuffDemo":
                {
                    await RunBuffDemo(fiber, ss);
                    break;
                }
            }
            await ETTask.CompletedTask;
        }

        private static async ETTask RunBuffDemo(Fiber fiber, string[] args)
        {
            Scene root = fiber.Root;
            Scene currentScene = root.CurrentScene();
            if (currentScene == null)
            {
                Log.Console("BuffDemo fail: currentScene is null");
                return;
            }

            ET.Unit myUnit = ET.Client.UnitHelper.GetMyUnitFromClientScene(root);
            if (myUnit == null || myUnit.IsDisposed)
            {
                Log.Console("BuffDemo fail: my unit is null");
                return;
            }

            string action = args.Length > 1 ? args[1] : "run";
            switch (action)
            {
                case "target":
                    SetNearestTarget(myUnit);
                    PrintBuffState(myUnit, true);
                    break;
                case "skill1":
                    SetNearestTarget(myUnit);
                    SendOperate(root, EOperateType.Skill1);
                    Log.Console("BuffDemo: queued skill1");
                    break;
                case "skill2":
                    SetNearestTarget(myUnit);
                    SendOperate(root, EOperateType.Skill2);
                    Log.Console("BuffDemo: queued skill2");
                    break;
                case "inspect":
                    PrintBuffState(myUnit, true);
                    break;
                case "run":
                default:
                    SetNearestTarget(myUnit);
                    PrintBuffState(myUnit, true);
                    SendOperate(root, EOperateType.Skill1);
                    await root.GetComponent<TimerComponent>().WaitAsync(300);
                    SendOperate(root, EOperateType.Skill2);
                    await root.GetComponent<TimerComponent>().WaitAsync(1500);
                    PrintBuffState(myUnit, true);
                    break;
            }
        }

        private static void SendOperate(Scene root, EOperateType operateType)
        {
            Type sessionComponentType = Type.GetType("ET.Client.SessionComponent, Unity.Model")
                ?? Type.GetType("ET.Client.SessionComponent, Model");
            Entity sessionComponent = sessionComponentType != null ? root.GetComponent(sessionComponentType) : null;
            object session = sessionComponentType?.GetProperty("Session")?.GetValue(sessionComponent);
            if (session == null)
            {
                Log.Console($"BuffDemo fail: SessionComponent missing for operate {operateType}");
                return;
            }

            OperateInfo operateInfo = OperateInfo.Create();
            operateInfo.OperateType = (int)operateType;
            operateInfo.InputType = (int)EInputType.KeyDown;
            C2Room_Operation c2RoomOperation = C2Room_Operation.Create();
            c2RoomOperation.OperateInfos = new System.Collections.Generic.List<OperateInfo> { operateInfo };
            session.GetType().GetMethod("Send", new[] { typeof(IMessage) })?.Invoke(session, new object[] { c2RoomOperation });
        }

        private static void SetNearestTarget(ET.Unit myUnit)
        {
            TargetComponent targetComponent = myUnit.GetComponent<TargetComponent>();
            if (targetComponent == null)
            {
                Log.Console("BuffDemo fail: target component missing");
                return;
            }

            ET.Unit target = TargetSelectHelper.FindNearestCombatTarget(myUnit);
            if (target == null)
            {
                Log.Console("BuffDemo fail: no combat target found");
                return;
            }

            targetComponent.SetTarget(target.Id);
            Log.Console($"BuffDemo target set: self={myUnit.Id} target={target.Id}");
        }

        private static void PrintBuffState(ET.Unit myUnit, bool inspectTarget)
        {
            ET.Unit inspectUnit = myUnit;
            if (inspectTarget)
            {
                long targetId = myUnit.GetComponent<TargetComponent>()?.CurrentTargetId ?? 0;
                if (targetId != 0 && TargetSelectHelper.TryGetTarget(myUnit, targetId, out ET.Unit target))
                {
                    inspectUnit = target;
                }
            }

            CombatStateComponent combatStateComponent = inspectUnit.GetComponent<CombatStateComponent>();
            BuffComponent buffComponent = inspectUnit.GetComponent<BuffComponent>();
            Log.Console($"BuffDemo inspect unit={inspectUnit.Id} tags={(ECombatTag)(combatStateComponent?.TagMask ?? 0)} buffCount={buffComponent?.BuffDic.Count ?? 0}");
            if (buffComponent == null || buffComponent.BuffDic.Count == 0)
            {
                return;
            }

            foreach ((int buffId, EntityRef<Buff> buffRef) in buffComponent.BuffDic)
            {
                Buff buff = buffRef;
                BuffConfig buffConfig = buff?.BuffConfig;
                Log.Console($"  buff id={buffId} name={buffConfig?.Name} layer={buff?.LayerCount ?? 0} reason={buff?.RemoveReason ?? EBuffRemoveReason.None} tags={buffConfig?.TagGrantMask ?? 0}");
            }
        }
    }
}
