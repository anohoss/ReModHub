using Microsoft.Extensions.Logging;
using ZLogger;

namespace ReModHub
{
    internal class ModManifestService
    {
        private readonly ILogger<ModManifestService> logger;

        private readonly ModManifestLoader loader;

        private readonly Dictionary<string, ModManifest> idToManifest = [];

        public ModManifestService(ILogger<ModManifestService> logger, ModManifestLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        public bool FindModManifest(ModReference reference, out ModManifest? manifest)
        {
            return idToManifest.TryGetValue(reference.Uuid, out manifest);
        }

        public async Task RefreshModManifestsAsync(CancellationToken cancellationToken)
        {
            idToManifest.Clear();

            var loaded = await loader.LoadAllManifestsAsync(cancellationToken);
            for (int i = 0; i < loaded.Count; i++)
            {
                idToManifest[loaded[i].Uuid] = loaded[i];
            }

            logger.ZLogDebug($"Successfully loaded {idToManifest.Count} mod manifests");
        }

        public void PopulateModManifests(List<ModManifest> results)
        {
            foreach (var manifest in idToManifest.Values)
            {
                results.Add(manifest);
            }
        }
    }
}
