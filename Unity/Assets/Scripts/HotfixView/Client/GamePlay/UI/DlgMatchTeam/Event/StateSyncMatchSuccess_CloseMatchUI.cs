namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(DlgMatchTeam))]
    public class StateSyncMatchSuccess_CloseMatchUI : AEvent<Scene, StateSyncMatchSuccess>
    {
        protected override async ETTask Run(Scene scene, StateSyncMatchSuccess args)
        {
            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            DlgMatchTeam dlgMatchTeam = uiComponent?.GetDlgLogic<DlgMatchTeam>();
            if (dlgMatchTeam != null && !dlgMatchTeam.IsDisposed)
            {
                dlgMatchTeam.IsMatching = false;
                dlgMatchTeam.View.ECountDownText.text = string.Empty;
            }

            uiComponent?.CloseWindow(WindowID.WindowID_MatchTeam);
            uiComponent?.CloseWindow(WindowID.WindowID_Lobby);
            await ETTask.CompletedTask;
        }
    }
}
