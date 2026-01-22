using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class ModManifestService
    {
        private const string ManifestFileName = "manifest.json";

        private readonly ILogger<ModManifestService> logger;
        private readonly Dictionary<string, ModManifest> idToManifest = [];

        public ModManifestService(ILogger<ModManifestService> logger)
        {
            this.logger = logger;
        }

        public bool FindModManifest(ModManifestReference reference, out ModManifest? manifest)
        {
            return idToManifest.TryGetValue(reference.Id, out manifest);
        }

        public async Task RefreshModManifestsAsync(CancellationToken cancellationToken)
        {
            idToManifest.Clear();
            await LoadModManifestsAsync(cancellationToken);
        }

        private async Task LoadModManifestsAsync(CancellationToken cancellationToken)
        {
            string modRootDirectoryName = AppPath.ModDirectoryName;

            if (!Directory.Exists(modRootDirectoryName))
            {
                logger.ZLogWarning($"Directory: '{modRootDirectoryName}' not found");
                return;
            }

            var modDirectoryNames = Directory.GetDirectories(modRootDirectoryName);

            for (int i = 0; i < modDirectoryNames.Length; i++)
            {
                var modDirName = modDirectoryNames[i];
                string manifestFilePath = Path.Combine(modDirName, ManifestFileName);

                var manifest = await LoadModManifestAsync(manifestFilePath, cancellationToken);
                if (manifest != null)
                {
                    string modId = Path.GetFileName(modDirName) ?? string.Empty;
                    idToManifest[modId] = manifest;
                }
            }

            logger.ZLogDebug($"Successfully loaded {idToManifest.Count} mod manifests");
        }

        private async Task<ModManifest?> LoadModManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var manifest = await JsonSerializer.DeserializeAsync<ModManifest>(fileStream, cancellationToken: cancellationToken);

                if (manifest == null)
                {
                    logger.ZLogError($"Failed to deserialize manifest from {manifestFilePath}");
                    return null;
                }

                return manifest;
            }
            catch (Exception ex)
            {
                logger.ZLogError($"Failed to load mod manifest: {ex.Message}");
                return null;
            }
        }
    }
} 