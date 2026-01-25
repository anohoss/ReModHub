using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using ZLogger;

namespace ReModHub
{
    internal class GameLauncherService
    {
        private readonly ILogger<GameLauncherService> logger;
        private readonly GameManifestService gameManifestService;
        private readonly ModManifestService modManifestService;
        private readonly Dictionary<string, GameProcess> runningProcesses = [];

        public GameLauncherService(
            ILogger<GameLauncherService> logger,
            GameManifestService gameManifestService,
            ModManifestService modManifestService)
        {
            this.logger = logger;
            this.gameManifestService = gameManifestService;
            this.modManifestService = modManifestService;
        }

        public GameProcess? TryGetRunningProcess(string profileId)
        {
            if (runningProcesses.TryGetValue(profileId, out var process))
            {
                if (process.Process.HasExited)
                {
                    runningProcesses.Remove(profileId);
                    return null;
                }

                return process;
            }

            return null;
        }

        public GameProcess? StopGame(string profileId)
        {
            if (!runningProcesses.TryGetValue(profileId, out var process))
            {
                return null;
            }

            process.Stop();
            runningProcesses.Remove(profileId);
            return process;
        }

        public GameProcess? StartGame(GameProfile profile)
        {
            if (string.IsNullOrEmpty(profile.Id))
            {
                logger.ZLogWarning($"Profile id is empty. Launch is skipped.");
                return null;
            }

            var existing = TryGetRunningProcess(profile.Id);
            if (existing != null)
            {
                return existing;
            }

            if (!gameManifestService.FindGameManifest(new GameReference { Id = profile.BaseGameId }, out var manifest) || manifest == null)
            {
                logger.ZLogWarning($"Base game manifest not found for '{profile.BaseGameId}'.");
                return null;
            }

            string exePath = Path.Combine(AppPath.GamesDirectoryName, profile.BaseGameId, manifest.ExeFileName);
            if (!File.Exists(exePath))
            {
                logger.ZLogWarning($"Executable not found at '{exePath}'.");
                return null;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? AppPath.GamesDirectoryName,
                UseShellExecute = true
            };

            string? modArgument = BuildModArgument(profile);
            if (!string.IsNullOrEmpty(modArgument))
            {
                startInfo.Arguments = modArgument;
            }

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) =>
            {
                runningProcesses.Remove(profile.Id);
                process.Dispose();
            };

            if (!process.Start())
            {
                logger.ZLogWarning($"Failed to start process for '{profile.Id}'.");
                process.Dispose();
                return null;
            }

            var gameProcess = new GameProcess(profile.Id, process);
            runningProcesses[profile.Id] = gameProcess;
            return gameProcess;
        }

        private string? BuildModArgument(GameProfile profile)
        {
            if (profile.ModReferences.Count == 0)
            {
                return null;
            }

            var modPaths = new List<string>();

            for (int i = 0; i < profile.ModReferences.Count; i++)
            {
                var reference = profile.ModReferences[i];
                if (!modManifestService.FindModManifest(reference, out var manifest) || manifest == null)
                {
                    logger.ZLogWarning($"Mod manifest not found for '{reference.Id}'.");
                    continue;
                }

                string modPath = Path.Combine(AppPath.ModsDirectoryName, reference.Id, manifest.PakFilename);
                if (!modPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                {
                    logger.ZLogWarning($"Mod file is not a .pak: '{modPath}'.");
                    continue;
                }

                if (!File.Exists(modPath))
                {
                    logger.ZLogWarning($"Mod file not found at '{modPath}'.");
                    continue;
                }

                modPaths.Add(modPath);
            }

            if (modPaths.Count == 0)
            {
                return null;
            }

            string joined = string.Join("|", modPaths);
            return $"-Mods=\"{joined}\"";
        }
    }
}
