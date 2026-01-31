namespace ReModHub
{
    internal interface IGameLauncherService
    {
        GameProcess? TryGetRunningProcess(string profileId);

        GameProcess? StopGame(string profileId);

        GameProcess? StartGame(GameProfile profile);
    }
}
