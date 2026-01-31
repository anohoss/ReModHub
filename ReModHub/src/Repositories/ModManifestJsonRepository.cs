using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal sealed record ModManifestJsonObject
    {
        public string Uuid { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string PakFileName { get; init; } = string.Empty;

        public ModReference[] DependentMods { get; } = [];
    }

    internal sealed class ModManifestJsonRepository : IModManifestRepository
    {
        private readonly ILogger<ModManifestJsonRepository> logger;
        private readonly Dictionary<string, ModManifest> idToManifest = [];

        public ModManifestJsonRepository(ILogger<ModManifestJsonRepository> logger)
        {
            this.logger = logger;
        }

        public bool FindModManifest(ModReference reference, out ModManifest? manifest)
        {
            return idToManifest.TryGetValue(reference.Uuid, out manifest);
        }

        public async Task<int> RefreshAsync(CancellationToken cancellationToken)
        {
            idToManifest.Clear();

            foreach (var manifestPath in EnumerateManifestPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await LoadManifestAsync(manifestPath, cancellationToken);
                if (item == null)
                {
                    continue;
                }

                idToManifest[item.Uuid] = item;
            }

            logger.ZLogDebug($"Successfully loaded {idToManifest.Count} mod manifests");
            return idToManifest.Count;
        }

        public void PopulateModManifests(List<ModManifest> results)
        {
            foreach (var manifest in idToManifest.Values)
            {
                results.Add(manifest);
            }
        }

        private IEnumerable<string> EnumerateManifestPaths()
        {
            string rootDirName = AppPath.ModsDirectoryName;

            if (!Directory.Exists(rootDirName))
            {
                logger.ZLogWarning($"Directory: '{rootDirName}' not found");
                yield break;
            }

            var modDirNames = Directory.GetDirectories(rootDirName);

            for (int i = 0; i < modDirNames.Length; i++)
            {
                var manifestFilePath = MakeManifestFilePath(modDirNames[i]);
                if (manifestFilePath != null)
                {
                    yield return manifestFilePath;
                }
            }
        }

        private async Task<ModManifest?> LoadManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var obj = await JsonSerializer.DeserializeAsync<ModManifestJsonObject>(
                    fileStream,
                    cancellationToken: cancellationToken);
                if (obj == null)
                {
                    logger.ZLogError($"Failed to deserialize manifest file with type: {typeof(ModManifestJsonObject)} from {manifestFilePath}");
                    return null;
                }

                var manifest = CreateManifest(obj, manifestFilePath);
                if (manifest == null)
                {
                    logger.ZLogError($"Failed to create manifest from deserialized object of type: {typeof(ModManifestJsonObject)} from {manifestFilePath}");
                    return null;
                }

                return manifest;
            }
            catch (Exception ex)
            {
                logger.ZLogError($"Failed to load manifest: {ex.Message}");
                return null;
            }
        }

        private static ModManifest? CreateManifest(ModManifestJsonObject manifest, string manifestFilePath)
        {
            if (string.IsNullOrEmpty(manifest.Uuid)
                || string.IsNullOrEmpty(manifest.DisplayName)
                || string.IsNullOrEmpty(manifest.VersionName)
                || string.IsNullOrEmpty(manifest.PakFileName))
            {
                return null;
            }

            var manifestDirectory = Path.GetDirectoryName(manifestFilePath) ?? string.Empty;
            return new ModManifest
            {
                Uuid = manifest.Uuid,
                DisplayName = manifest.DisplayName,
                VersionName = manifest.VersionName,
                PakFilePath = Path.Combine(manifestDirectory, manifest.PakFileName),
                DependentMods = manifest.DependentMods,
            };
        }

        private string? MakeManifestFilePath(string modDirPath)
        {
            const string manifestExtension = "json";
            string modDirName = Path.GetFileName(modDirPath) ?? string.Empty;

            if (string.IsNullOrEmpty(modDirName))
            {
                logger.ZLogWarning($"Mod directory path is invalid: '{modDirPath}'");
                return null;
            }

            int separatorIndex = modDirName.IndexOf('-');
            if (separatorIndex <= 0)
            {
                logger.ZLogWarning($"Mod directory name does not contain version separator: '{modDirName}'");
                return null;
            }

            string manifestFileNameWithoutExtension = modDirName[..separatorIndex];
            string manifestFileName = $"{manifestFileNameWithoutExtension}.{manifestExtension}";
            return Path.Combine(modDirPath, manifestFileName);
        }
    }
}
