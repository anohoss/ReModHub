using Microsoft.Extensions.Logging;
using System.IO;
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

    internal sealed class GameManifestLoader : ManifestLoaderBase<GameManifest, GameManifestJsonObject>
    {

        public GameManifestLoader(ILogger<GameManifestLoader> logger)
            : base(logger, CreateManifest)
        {
        }

        protected override IEnumerable<string> EnumerateManifestPaths()
        {
            string gamesRootDirectoryName = AppPath.GamesDirectoryName;

            if (!Directory.Exists(gamesRootDirectoryName))
            {
                Logger.ZLogWarning($"Directory '{gamesRootDirectoryName}' not found");
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

        private static GameManifest? CreateManifest(GameManifestJsonObject manifest, string manifestFilePath)
        {
            if (string.IsNullOrEmpty(manifest.Uuid)
                || string.IsNullOrEmpty(manifest.DisplayName)
                || string.IsNullOrEmpty(manifest.VersionName)
                || string.IsNullOrEmpty(manifest.ExeFileName))
            {
                return null;
            }

            return new GameManifest
            {
                Uuid = manifest.Uuid,
                DisplayName = manifest.DisplayName,
                VersionName = manifest.VersionName,
                ExeFilePath = $"{Path.GetDirectoryName(manifestFilePath)}/{manifest.ExeFileName}"
            };
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
