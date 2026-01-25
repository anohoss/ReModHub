using Microsoft.Extensions.Logging;
using ZLogger;

namespace ReModHub
{
    internal class GameManifestService
    {
        private readonly ILogger<GameManifestService> logger;
        private readonly GameManifestLoader loader;
        private readonly List<GameManifest> manifests = [];

        public GameManifestService(ILogger<GameManifestService> logger, GameManifestLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        public async Task RefreshGameManifestsAsync(CancellationToken cancellationToken)
        {
            manifests.Clear();

            var loaded = await loader.LoadAllManifestsAsync(cancellationToken);
            manifests.AddRange(loaded);

            logger.ZLogDebug($"Successfully loaded {manifests.Count} game manifests");
        }

        public void PopulateGameManifests(List<GameManifest> results, Predicate<GameManifest> predicate)
        {
            for (int i = 0; i < manifests.Count; i++)
            {
                if (!predicate(manifests[i]))
                {
                    continue;
                }

                results.Add(manifests[i]);
            }
        }

        public bool FindGameManifest(GameReference reference, out GameManifest? manifest)
        {
            manifest = null;

            for (int i = 0; i < manifests.Count; i++)
            {
                if (manifests[i].Uuid == reference.Uuid)
                {
                    manifest = manifests[i];
                    return true;
                }
            }

            return false;
        }
    }
}
