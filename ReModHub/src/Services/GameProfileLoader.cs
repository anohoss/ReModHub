using Microsoft.Extensions.Logging;
using System.IO;
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

    internal sealed class GameProfileLoader : ManifestLoaderBase<GameProfile, GameProfileJsonObject>
    {
        public GameProfileLoader(ILogger<GameProfileLoader> logger)
            : base(logger, CreateManifest)
        {
        }

        protected override IEnumerable<string> EnumerateManifestPaths()
        {
            string profileRootDirectoryName = ResolveProfileRootDirectoryName();

            if (!Directory.Exists(profileRootDirectoryName))
            {
                Logger.ZLogWarning($"Directory: '{profileRootDirectoryName}' not found");
                yield break;
            }

            var profileFilePaths = Directory.GetFiles(profileRootDirectoryName, "*.json", SearchOption.AllDirectories);
            for (int i = 0; i < profileFilePaths.Length; i++)
            {
                yield return profileFilePaths[i];
            }
        }

        private static GameProfile? CreateManifest(GameProfileJsonObject manifestObject, string manifestFilePath)
        {
            if (string.IsNullOrEmpty(manifestObject.DisplayName) || string.IsNullOrEmpty(manifestObject.VersionName))
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

        private static string ResolveProfileRootDirectoryName()
        {
            return AppPath.GameProfilesDirectoryName;
        }
    }
}
