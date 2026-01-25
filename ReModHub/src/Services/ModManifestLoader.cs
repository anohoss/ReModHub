using Microsoft.Extensions.Logging;
using System.IO;
using ZLogger;

namespace ReModHub
{
    internal sealed record ModManifestJsonObject
    {
        public string Uuid { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string PakFileName { get; init; } = string.Empty;

        /// <summary>
        /// ˆË‘¶‚µ‚Ä‚¢‚éMOD‚ÌƒŠƒXƒg
        /// </summary>
        public ModReference[] DependentMods { get; } = [];
    }

    internal sealed class ModManifestLoader : ManifestLoaderBase<ModManifest, ModManifestJsonObject>
    {
        public ModManifestLoader(ILogger<ModManifestLoader> logger)
            : base(logger, CreateManifest)
        {
        }

        protected override IEnumerable<string> EnumerateManifestPaths()
        {
            string rootDirName = AppPath.ModsDirectoryName;

            if (!Directory.Exists(rootDirName))
            {
                Logger.ZLogWarning($"Directory: '{rootDirName}' not found");
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

        private static ModManifest? CreateManifest(ModManifestJsonObject manifest, string manifestFilePath)
        {
            if (string.IsNullOrEmpty(manifest.Uuid)
                || string.IsNullOrEmpty(manifest.DisplayName)
                || string.IsNullOrEmpty(manifest.VersionName)
                || string.IsNullOrEmpty(manifest.PakFileName))
            {
                return null;
            }

            return new ModManifest
            {
                Uuid = manifest.Uuid,
                DisplayName = manifest.DisplayName,
                VersionName = manifest.VersionName,
                PakFilePath = $"{Path.GetDirectoryName(manifestFilePath)}/{manifest.PakFileName}",
                DependentMods = manifest.DependentMods,
            };
        }

        private string? MakeManifestFilePath(string modDirPath)
        {
            const string manifestExtension = "json";
            string modDirName = Path.GetFileName(modDirPath) ?? string.Empty;

            if (string.IsNullOrEmpty(modDirName))
            {
                Logger.ZLogWarning($"Mod directory path is invalid: '{modDirPath}'");
                return null;
            }

            int separatorIndex = modDirName.IndexOf('-');
            if (separatorIndex <= 0)
            {
                Logger.ZLogWarning($"Mod directory name does not contain version separator: '{modDirName}'");
                return null;
            }

            string manifestFileNameWithoutExtension = modDirName[..separatorIndex];
            string manifestFileName = $"{manifestFileNameWithoutExtension}.{manifestExtension}";
            return Path.Combine(modDirPath, manifestFileName);
        }
    }
}
