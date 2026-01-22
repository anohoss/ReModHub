using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class GameManifestService
    {
        private record class ReModGameManifestJsonObject
        {
            public string? DisplayName { get; set; } = null;

            public string? VersionName { get; set; } = null;

            public string? ExeFileName { get; set; } = null;
        }

        private record class GameProfileJsonObject
        {
            public string? DisplayName { get; set; } = string.Empty;

            public string? VersionName { get; set; } = string.Empty;

            public GameManifestReference? BaseGame { get; set; } = null;

            public ModManifestReference[]? Mods { get; set; } = null;
        }

        private const string ProfileFileName = "profile.json";
        private const string ManifestFileName = "manifest.json";

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

        private async Task LoadGameManifestsAsync(CancellationToken cancellationToken)
        {
            string gamesRootDirectoryName = AppPath.GameDirectoryName;

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

                string manifestFilePath = Path.Combine(gameDirName, ManifestFileName);
                string profileFilePath = Path.Combine(gameDirName, ProfileFileName);

                if (File.Exists(manifestFilePath))
                {
                    // マニフェストファイルの読み込み
                    manifest = await LoadReModGameManifestAsync(manifestFilePath, cancellationToken);
                }
                else if (File.Exists(profileFilePath))
                {
                    // プロファイルファイルの読み込み
                    manifest = await LoadGameProfileAsync(profileFilePath, cancellationToken);
                }

                if (manifest is not null)
                {
                    manifests.Add(manifest);
                }
            }

            logger.ZLogDebug($"Successfully loaded {manifests.Count} game manifests");
        }

        private async Task<GameProfile?> LoadGameProfileAsync(string profileFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(profileFilePath))
            {
                logger.ZLogWarning($"Profile: '{profileFilePath}' not found.");
                return null;
            }

            GameProfileJsonObject? profile;
            try
            {
                using var fileStream = File.OpenRead(profileFilePath);
                profile = await JsonSerializer.DeserializeAsync<GameProfileJsonObject>(fileStream, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.ZLogError($"Failed to load game profile: {ex.Message}");
                return null;
            }

            if (profile == null)
            {
                logger.ZLogError($"Failed to deserialize profile from {profileFilePath}");
                return null;
            }

            if (string.IsNullOrEmpty(profile.DisplayName) || string.IsNullOrEmpty(profile.VersionName))
            {
                logger.ZLogError($"Required fields DisplayName or VersionName are missing in profile at {profileFilePath}");
                return null;
            }

            return new GameProfile(
                profile.DisplayName,
                profile.VersionName,
                profile.BaseGame ?? new GameManifestReference(),
                profile.Mods ?? []);
        }

        private async Task<GameManifest?> LoadReModGameManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            ReModGameManifestJsonObject? manifest;
            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                manifest = await JsonSerializer.DeserializeAsync<ReModGameManifestJsonObject>(fileStream, cancellationToken: cancellationToken);
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

            return new ReModGameManifest(
                manifest.DisplayName,
                manifest.VersionName,
                manifest.ExeFileName ?? string.Empty);
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

        public bool FindGameManifest(GameManifestReference reference, out GameManifest? manifest)
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
    }
}
