namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CombatStateChanged_PlayUnitAnimation : AEvent<Scene, CombatStateChanged>
    {
        protected override async ETTask Run(Scene scene, CombatStateChanged args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            unit.GetComponent<CombatAnimStateComponent>()?.ApplyCombatState(args.NewState, args.NewSubState,
                args.NewState == ECombatState.Casting && args.NewSubState == ECombatSubState.CastPoint);
            await ETTask.CompletedTask;
        }
    }
}
