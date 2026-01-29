using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal class GameProfileService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly ILogger<GameProfileService> logger;
        private readonly GameProfileLoader loader;
        private readonly List<GameProfile> profiles = [];

        public GameProfileService(ILogger<GameProfileService> logger, GameProfileLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        public async Task RefreshGameProfilesAsync(CancellationToken cancellationToken)
        {
            profiles.Clear();

            var loaded = await loader.LoadAllManifestsAsync(cancellationToken);
            profiles.AddRange(loaded);

            logger.ZLogDebug($"Successfully loaded {profiles.Count} game profiles");
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

        public async Task<GameProfile?> SaveGameProfileAsync(GameProfile profile, CancellationToken cancellationToken)
        {
            if (profile == null)
            {
                return null;
            }

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

            var updatedProfile = new GameProfile
            {
                ManifestFilePath = manifestPath,
                Uuid = profile.Uuid,
                DisplayName = profile.DisplayName,
                VersionName = profile.VersionName,
                BaseGameReference = profile.BaseGameReference,
                ModReferences = profile.ModReferences
            };

            ReplaceCachedProfile(updatedProfile);
            logger.ZLogInformation($"Saved game profile: {updatedProfile.DisplayName}");
            return updatedProfile;
        }

        public Task<bool> DeleteGameProfileAsync(GameProfile profile, CancellationToken cancellationToken)
        {
            if (profile == null)
            {
                return Task.FromResult(false);
            }

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
