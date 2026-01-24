using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class GameProfileService
    {
        private record class GameProfileJsonObject
        {
            public string? DisplayName { get; set; } = string.Empty;
            public string? VersionName { get; set; } = string.Empty;
            public GameManifestReference? BaseGameReference { get; set; } = null;
            public ModManifestReference[]? ModReferences { get; set; } = null;
        }

        private readonly ILogger<GameProfileService> logger;
        private readonly List<GameProfileDisplayInfo> profiles = [];

        public GameProfileService(ILogger<GameProfileService> logger)
        {
            this.logger = logger;
        }

        public async Task RefreshGameProfilesAsync(CancellationToken cancellationToken)
        {
            profiles.Clear();
            await LoadGameProfilesAsync(cancellationToken);
        }

        public void PopulateGameProfiles(List<GameProfileDisplayInfo> results)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                results.Add(profiles[i]);
            }
        }

        private async Task LoadGameProfilesAsync(CancellationToken cancellationToken)
        {
            string profileRootDirectoryName = ResolveProfileRootDirectoryName();

            if (!Directory.Exists(profileRootDirectoryName))
            {
                logger.ZLogWarning($"Directory: '{profileRootDirectoryName}' not found");
                return;
            }

            var profileFilePaths = Directory.GetFiles(profileRootDirectoryName, "*.json", SearchOption.AllDirectories);

            for (int i = 0; i < profileFilePaths.Length; i++)
            {
                var profile = await LoadGameProfileAsync(profileFilePaths[i], cancellationToken);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }

            logger.ZLogDebug($"Successfully loaded {profiles.Count} game profiles");
        }

        private static string ResolveProfileRootDirectoryName()
        {
            string? currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string candidate = Path.Combine(currentDirectory, "GameProfiles");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return AppPath.GameProfilesDirectoryName;
        }

        private async Task<GameProfileDisplayInfo?> LoadGameProfileAsync(string profileFilePath, CancellationToken cancellationToken)
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

            return new GameProfileDisplayInfo
            {
                DisplayName = profile.DisplayName,
                VersionName = profile.VersionName,
                BaseGameId = profile.BaseGameReference?.Id ?? string.Empty,
                ModCount = profile.ModReferences?.Length ?? 0
            };
        }
    }
}
