using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal sealed record GameProfileJsonObject
    {
        public string Uuid { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public GameReference BaseGameReference { get; set; } = default;
        public ModReference[] ModReferences { get; set; } = [];
    }

    internal sealed class GameProfileJsonRepository : IGameProfileRepository
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly ILogger<GameProfileJsonRepository> logger;
        private readonly List<GameProfile> profiles = [];

        public GameProfileJsonRepository(ILogger<GameProfileJsonRepository> logger)
        {
            this.logger = logger;
        }

        public async Task<int> RefreshAsync(CancellationToken cancellationToken)
        {
            profiles.Clear();

            foreach (var manifestPath in EnumerateManifestPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await LoadManifestAsync(manifestPath, cancellationToken);
                if (item == null)
                {
                    continue;
                }

                profiles.Add(item);
            }

            logger.ZLogDebug($"Successfully loaded {profiles.Count} game profiles");
            return profiles.Count;
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
            if (string.IsNullOrWhiteSpace(reference.Uuid))
            {
                return false;
            }

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

        public async Task<GameProfile?> SaveAsync(GameProfile profile, CancellationToken cancellationToken)
        {
            string manifestPath = ResolveManifestPath(profile);
            var directory = Path.GetDirectoryName(manifestPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var manifest = new GameProfileJsonObject
            {
                Uuid = profile.Uuid,
                DisplayName = profile.DisplayName,
                VersionName = profile.VersionName,
                BaseGameReference = profile.BaseGameReference,
                ModReferences = profile.ModReferences?.ToArray() ?? []
            };

            await using (var stream = File.Create(manifestPath))
            {
                await JsonSerializer.SerializeAsync(stream, manifest, SerializerOptions, cancellationToken);
            }

            var snapshot = CreateProfileSnapshot(profile, manifestPath);
            ReplaceCachedProfile(snapshot);
            logger.ZLogInformation($"Saved game profile: {snapshot.DisplayName}");
            return snapshot;
        }

        public Task<bool> DeleteAsync(GameProfile profile, CancellationToken cancellationToken)
        {
            string manifestPath = ResolveManifestPath(profile);
            try
            {
                if (File.Exists(manifestPath))
                {
                    File.Delete(manifestPath);
                }

                RemoveCachedProfile(profile);
                logger.ZLogInformation($"Deleted game profile: {profile.DisplayName}");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Failed to delete game profile: {profile.DisplayName}");
                return Task.FromResult(false);
            }
        }

        private static string ResolveManifestPath(GameProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.ManifestFilePath))
            {
                return profile.ManifestFilePath;
            }

            var fileName = string.IsNullOrWhiteSpace(profile.Uuid)
                ? "profile.json"
                : $"{profile.Uuid}.json";
            return Path.Combine(AppPath.GameProfilesDirectoryName, fileName);
        }

        private static GameProfile CreateProfileSnapshot(GameProfile profile, string manifestPath)
        {
            return new GameProfile
            {
                ManifestFilePath = manifestPath,
                Uuid = profile.Uuid,
                DisplayName = profile.DisplayName,
                VersionName = profile.VersionName,
                BaseGameReference = profile.BaseGameReference,
                ModReferences = profile.ModReferences
            };
        }

        private IEnumerable<string> EnumerateManifestPaths()
        {
            string profileRootDirectoryName = AppPath.GameProfilesDirectoryName;

            if (!Directory.Exists(profileRootDirectoryName))
            {
                logger.ZLogWarning($"Directory: '{profileRootDirectoryName}' not found");
                yield break;
            }

            var profileFilePaths = Directory.GetFiles(profileRootDirectoryName, "*.json", SearchOption.AllDirectories);
            for (int i = 0; i < profileFilePaths.Length; i++)
            {
                yield return profileFilePaths[i];
            }
        }

        private async Task<GameProfile?> LoadManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }

            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var obj = await JsonSerializer.DeserializeAsync<GameProfileJsonObject>(
                    fileStream,
                    cancellationToken: cancellationToken);
                if (obj == null)
                {
                    logger.ZLogError($"Failed to deserialize manifest file with type: {typeof(GameProfileJsonObject)} from {manifestFilePath}");
                    return null;
                }

                var manifest = CreateManifest(obj, manifestFilePath);
                if (manifest == null)
                {
                    logger.ZLogError($"Failed to create manifest from deserialized object of type: {typeof(GameProfileJsonObject)} from {manifestFilePath}");
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

        private static GameProfile? CreateManifest(GameProfileJsonObject manifestObject, string manifestFilePath)
        {
            if (string.IsNullOrWhiteSpace(manifestObject.DisplayName)
                || string.IsNullOrWhiteSpace(manifestObject.VersionName))
            {
                return null;
            }

            var modReferences = manifestObject.ModReferences ?? [];
            return new GameProfile
            {
                ManifestFilePath = manifestFilePath,
                Uuid = manifestObject.Uuid,
                DisplayName = manifestObject.DisplayName,
                VersionName = manifestObject.VersionName,
                BaseGameReference = manifestObject.BaseGameReference,
                ModReferences = modReferences,
            };
        }

        private void ReplaceCachedProfile(GameProfile updatedProfile)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Uuid == updatedProfile.Uuid)
                {
                    profiles[i] = updatedProfile;
                    return;
                }
            }

            profiles.Add(updatedProfile);
        }

        private void RemoveCachedProfile(GameProfile targetProfile)
        {
            for (int i = profiles.Count - 1; i >= 0; i--)
            {
                if (profiles[i].Uuid == targetProfile.Uuid)
                {
                    profiles.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
