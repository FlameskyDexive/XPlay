namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CombatHit_PlayUnitAnimation : AEvent<Scene, CombatHit>
    {
        protected override async ETTask Run(Scene scene, CombatHit args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            unit.GetComponent<CombatAnimStateComponent>()?.PlayHit();
            await ETTask.CompletedTask;
        }
    }
}
