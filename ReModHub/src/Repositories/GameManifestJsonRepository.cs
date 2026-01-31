using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal sealed record GameManifestJsonObject
    {
        public string? Uuid { get; set; } = null;
        public string? DisplayName { get; set; } = null;
        public string? VersionName { get; set; } = null;
        public string? ExeFileName { get; set; } = null;
    }

    internal sealed class GameManifestJsonRepository : IGameManifestRepository
    {
        private readonly ILogger<GameManifestJsonRepository> logger;
        private readonly List<GameManifest> manifests = [];

        public GameManifestJsonRepository(ILogger<GameManifestJsonRepository> logger)
        {
            this.logger = logger;
        }

        public async Task<int> RefreshAsync(CancellationToken cancellationToken)
        {
            manifests.Clear();

            foreach (var manifestPath in EnumerateManifestPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await LoadManifestAsync(manifestPath, cancellationToken);
                if (item == null)
                {
                    continue;
                }

                manifests.Add(item);
            }

            logger.ZLogDebug($"Successfully loaded {manifests.Count} game manifests");
            return manifests.Count;
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
            if (string.IsNullOrWhiteSpace(reference.Uuid))
            {
                return false;
            }

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

        private IEnumerable<string> EnumerateManifestPaths()
        {
            string gamesRootDirectoryName = AppPath.GamesDirectoryName;

            if (!Directory.Exists(gamesRootDirectoryName))
            {
                logger.ZLogWarning($"Directory '{gamesRootDirectoryName}' not found");
                yield break;
            }

            var gameDirectoryNames = Directory.GetDirectories(gamesRootDirectoryName);

            for (int i = 0; i < gameDirectoryNames.Length; i++)
            {
                string? manifestFilePath = ResolveManifestFilePath(gameDirectoryNames[i]);
                if (manifestFilePath != null)
                {
                    yield return manifestFilePath;
                }
            }
        }

        private async Task<GameManifest?> LoadManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var obj = await JsonSerializer.DeserializeAsync<GameManifestJsonObject>(
                    fileStream,
                    cancellationToken: cancellationToken);
                if (obj == null)
                {
                    logger.ZLogError($"Failed to deserialize manifest file with type: {typeof(GameManifestJsonObject)} from {manifestFilePath}");
                    return null;
                }

                var manifest = CreateManifest(obj, manifestFilePath);
                if (manifest == null)
                {
                    logger.ZLogError($"Failed to create manifest from deserialized object of type: {typeof(GameManifestJsonObject)} from {manifestFilePath}");
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

        private static GameManifest? CreateManifest(GameManifestJsonObject manifestObject, string manifestFilePath)
        {
            if (string.IsNullOrWhiteSpace(manifestObject.Uuid)
                || string.IsNullOrWhiteSpace(manifestObject.DisplayName)
                || string.IsNullOrWhiteSpace(manifestObject.VersionName)
                || string.IsNullOrWhiteSpace(manifestObject.ExeFileName))
            {
                return null;
            }

            var manifestDirectory = Path.GetDirectoryName(manifestFilePath);
            if (string.IsNullOrWhiteSpace(manifestDirectory))
            {
                return null;
            }

            return new GameManifest
            {
                Uuid = manifestObject.Uuid,
                DisplayName = manifestObject.DisplayName,
                VersionName = manifestObject.VersionName,
                ExeFilePath = Path.Combine(manifestDirectory, manifestObject.ExeFileName)
            };
        }

        private static string? ResolveManifestFilePath(string gameDirPath)
        {
            var gameDirName = Path.GetFileName(gameDirPath);
            if (string.IsNullOrWhiteSpace(gameDirName))
            {
                return null;
            }

            int separatorIndex = gameDirName.IndexOf('-');
            if (separatorIndex <= 0)
            {
                return null;
            }

            string manifestBaseName = gameDirName[..separatorIndex];
            string manifestPath = Path.Combine(gameDirPath, $"{manifestBaseName}.json");

            return File.Exists(manifestPath) ? manifestPath : null;
        }
    }
}
