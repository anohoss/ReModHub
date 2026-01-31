namespace ReModHub
{
    internal interface IGameManifestRepository
    {
        Task<int> RefreshAsync(CancellationToken cancellationToken);

        void PopulateGameManifests(List<GameManifest> results, Predicate<GameManifest> predicate);

        bool FindGameManifest(GameReference reference, out GameManifest? manifest);
    }
}
