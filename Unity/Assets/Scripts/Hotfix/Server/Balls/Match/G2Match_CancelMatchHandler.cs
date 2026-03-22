namespace ET.Server
{
    [MessageLocationHandler(SceneType.Match)]
    public class G2Match_CancelMatchHandler : MessageLocationHandler<Scene, G2Match_CancelMatch, Match2G_CancelMatch>
    {
        protected override async ETTask Run(Scene scene, G2Match_CancelMatch request, Match2G_CancelMatch response)
        {
            MatchComponent matchComponent = scene.GetComponent<MatchComponent>();
            if (matchComponent == null)
            {
                response.Error = ErrorCode.ERR_NotFoundComponent;
                response.Message = "MatchComponent not found";
                return;
            }

            long playerId = request.PlayerId;
            bool removed = matchComponent.CancelStateSyncMatch(playerId);

            response.Error = ErrorCode.ERR_Success;
            response.Message = removed ? "Cancel match success" : "Player is not in matching";
            await ETTask.CompletedTask;
        }
    }
}
