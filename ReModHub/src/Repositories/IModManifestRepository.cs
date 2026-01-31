namespace ReModHub
{
    internal interface IModManifestRepository
    {
        Task<int> RefreshAsync(CancellationToken cancellationToken);

        void PopulateModManifests(List<ModManifest> results);

        bool FindModManifest(ModReference reference, out ModManifest? manifest);
    }
}
