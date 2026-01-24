using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class ModManifestService
    {
        private ILogger<ModManifestService> Logger { get; init; }

        private readonly Dictionary<string, ModManifest> idToManifest = [];

        public ModManifestService(ILogger<ModManifestService> logger)
        {
            Logger = logger;
        }

        public bool FindModManifest(ModManifestReference reference, out ModManifest? manifest)
        {
            return idToManifest.TryGetValue(reference.Id, out manifest);
        }

        public async Task RefreshModManifestsAsync(CancellationToken cancellationToken)
        {
            idToManifest.Clear();
            await LoadManifestsAsync(cancellationToken);
        }

        public void PopulateModManifests(List<ModManifest> results)
        {
            foreach (var manifest in idToManifest.Values)
            {
                results.Add(manifest);
            }
        }

        private async Task LoadManifestsAsync(CancellationToken cancellationToken)
        {
            string RootDirName = AppPath.ModsDirectoryName;

            if (!Directory.Exists(RootDirName))
            {
                Logger.ZLogWarning($"Directory: '{RootDirName}' not found");
                return;
            }

            var modDirNames = Directory.GetDirectories(RootDirName);

            for (int i = 0; i < modDirNames.Length; i++)
            {
                var modDirName = modDirNames[i];
                var manifestFilePath = MakeManifestFilePath(modDirName);

                var manifest = await LoadManifestAsync(manifestFilePath, cancellationToken);
                if (manifest != null)
                {
                    string modId = Path.GetFileName(modDirName) ?? string.Empty;
                    idToManifest[modId] = manifest;
                }
            }

            Logger.ZLogDebug($"Successfully loaded {idToManifest.Count} mod manifests");
        }

        private string MakeManifestFilePath(string modDirPath)
        {
            const string ManifestExtension = "json";
            string modDirName = Path.GetFileName(modDirPath) ?? string.Empty;

            if (string.IsNullOrEmpty(modDirName))
            {
                throw new ArgumentException("Mod directory path is invalid", nameof(modDirPath));
            }

            string manifestFileNameWithoutExtension = modDirName[0..modDirName.IndexOf('-')];
            string manifestFileName = $"{manifestFileNameWithoutExtension}.{ManifestExtension}";
            return Path.Combine(modDirPath, manifestFileName);
        }

        private async Task<ModManifest?> LoadManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                Logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var manifest = await JsonSerializer.DeserializeAsync<ModManifest>(fileStream, cancellationToken: cancellationToken);

                if (manifest == null)
                {
                    Logger.ZLogError($"Failed to deserialize manifest from {manifestFilePath}");
                    return null;
                }

                return manifest;
            }
            catch (Exception ex)
            {
                Logger.ZLogError($"Failed to load mod manifest: {ex.Message}");
                return null;
            }
        }
    }
} 
