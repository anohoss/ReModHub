using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class GameManifestService
    {
        private record class GameManifestJsonObject
        {
            public string? DisplayName { get; set; } = null;

            public string? VersionName { get; set; } = null;

            public string? ExeFileName { get; set; } = null;

            public string? ExeFilename { get; set; } = null;
        }

        // Injected fields
        private readonly ILogger<GameManifestService> logger;

        private readonly List<GameManifest> manifests = [];

        public GameManifestService(ILogger<GameManifestService> logger)
        {
            this.logger = logger;
        }

        public async Task RefreshGameManifestsAsync(CancellationToken cancellationToken)
        {
            manifests.Clear();

            await LoadGameManifestsAsync(cancellationToken);
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
                if (manifests[i].Id == reference.Id)
                {
                    manifest = manifests[i];
                    return true;
                }
            }

            return false;
        }

        private async Task LoadGameManifestsAsync(CancellationToken cancellationToken)
        {
            string gamesRootDirectoryName = AppPath.GamesDirectoryName;

            if (!Directory.Exists(gamesRootDirectoryName))
            {
                logger.ZLogWarning($"Directory '{gamesRootDirectoryName}' not found");
                return;
            }

            var gameDirectoryNames = Directory.GetDirectories(gamesRootDirectoryName);

            for (int i = 0; i < gameDirectoryNames.Length; i++)
            {
                GameManifest? manifest = null;
                var gameDirName = gameDirectoryNames[i];

                string? manifestFilePath = ResolveManifestFilePath(gameDirName);
                if (manifestFilePath != null)
                {
                    // マニフェストファイルの読み込み
                    manifest = await LoadGameManifestAsync(manifestFilePath, cancellationToken);
                }

                if (manifest is not null)
                {
                    manifests.Add(manifest);
                }
            }

            logger.ZLogDebug($"Successfully loaded {manifests.Count} game manifests");
        }

        private async Task<GameManifest?> LoadGameManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            GameManifestJsonObject? manifest;
            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                manifest = await JsonSerializer.DeserializeAsync<GameManifestJsonObject>(fileStream, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.ZLogError($"Failed to load game manifest: {ex.Message}");
                return null;
            }

            if (manifest == null)
            {
                logger.ZLogError($"Failed to deserialize manifest from {manifestFilePath}");
                return null;
            }

            if (string.IsNullOrEmpty(manifest.DisplayName) || string.IsNullOrEmpty(manifest.VersionName))
            {
                logger.ZLogError($"Required fields DisplayName or VersionName are missing in manifest at {manifestFilePath}");
                return null;
            }

            var exeFileName = string.IsNullOrEmpty(manifest.ExeFileName) ? manifest.ExeFilename : manifest.ExeFileName;
            var versionName = manifest.VersionName;
            var id = $"{Path.GetFileNameWithoutExtension(manifestFilePath)}-{versionName}";

            return new GameManifest(
                id,
                manifest.DisplayName,
                manifest.VersionName,
                exeFileName ?? string.Empty);
        }

        private static string? ResolveManifestFilePath(string gameDirPath)
        {
            string gameDirName = Path.GetFileName(gameDirPath) ?? string.Empty;
            int separatorIndex = gameDirName.IndexOf('-');
            if (separatorIndex > 0)
            {
                string manifestBaseName = gameDirName[..separatorIndex];
                string manifestPath = Path.Combine(gameDirPath, $"{manifestBaseName}.json");

                if (File.Exists(manifestPath))
                {
                    return manifestPath;
                }
            }

            return null;
        }
    }
}
