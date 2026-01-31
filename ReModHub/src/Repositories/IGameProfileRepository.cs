namespace ReModHub
{
    internal interface IGameProfileRepository
    {
        Task<int> RefreshAsync(CancellationToken cancellationToken);

        void PopulateGameProfiles(List<GameProfile> results);

        bool FindGameProfile(GameReference reference, out GameProfile? profile);

        Task<GameProfile?> SaveAsync(GameProfile profile, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(GameProfile profile, CancellationToken cancellationToken);
    }
}
