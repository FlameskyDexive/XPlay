namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterMyUnitCreate_BTDemo : AEvent<Scene, AfterMyUnitCreate>
    {
        protected override async ETTask Run(Scene scene, AfterMyUnitCreate args)
        {
            Scene root = scene.Root();
            if (root.SceneType != SceneType.Demo)
            {
                await ETTask.CompletedTask;
                return;
            }

            Unit unit = args.unit;
            if (unit == null || unit.IsDisposed)
            {
                await ETTask.CompletedTask;
                return;
            }

            AttachAITest(unit);

            await ETTask.CompletedTask;
        }

        private static void AttachAITest(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            BTComponent behaviorTreeComponent = unit.GetComponent<BTComponent>();
            if (behaviorTreeComponent == null)
            {
                unit.AddComponent<BTComponent, string, string>("AITest", "AITest");
                return;
            }

            behaviorTreeComponent.Reload("AITest", "AITest");
        }
    }

    [Event(SceneType.Current)]
    public class AfterUnitCreate_BTDemoCombatAI : AEvent<Scene, AfterUnitCreate>
    {
        protected override async ETTask Run(Scene scene, AfterUnitCreate args)
        {
            Scene root = scene.Root();
            if (root.SceneType != SceneType.Demo)
            {
                await ETTask.CompletedTask;
                return;
            }

            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed || unit.Type() != EUnitType.Monster)
            {
                await ETTask.CompletedTask;
                return;
            }

            BTComponent behaviorTreeComponent = unit.GetComponent<BTComponent>();
            if (behaviorTreeComponent == null)
            {
                unit.AddComponent<BTComponent, string, string>("AITest", "AITest");
            }
            else
            {
                behaviorTreeComponent.Reload("AITest", "AITest");
            }

            await ETTask.CompletedTask;
        }
    }
}
