using Microsoft.Extensions.Logging;
using ZLogger;

namespace ReModHub
{
    internal class GameProfileService
    {
        private readonly ILogger<GameProfileService> logger;
        private readonly GameProfileLoader loader;
        private readonly List<GameProfile> profiles = [];

        public GameProfileService(ILogger<GameProfileService> logger, GameProfileLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        public async Task RefreshGameProfilesAsync(CancellationToken cancellationToken)
        {
            profiles.Clear();

            var loaded = await loader.LoadAllManifestsAsync(cancellationToken);
            profiles.AddRange(loaded);

            logger.ZLogDebug($"Successfully loaded {profiles.Count} game profiles");
        }

        public void PopulateGameProfiles(List<GameProfile> results)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                results.Add(profiles[i]);
            }
        }

        public bool FindGameProfile(GameReference reference, out GameProfile? profile)
        {
            profile = null;

            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Uuid == reference.Uuid)
                {
                    profile = profiles[i];
                    return true;
                }
            }

            return false;
        }
    }
}
