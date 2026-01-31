using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using ZLogger;

namespace ReModHub
{
    internal sealed class GameLauncherService : IGameLauncherService
    {
        private readonly ILogger<GameLauncherService> logger;
        private readonly IGameManifestRepository gameManifestRepository;
        private readonly IGameProfileRepository gameProfileRepository;
        private readonly IModManifestRepository modManifestRepository;
        private readonly Dictionary<string, GameProcess> runningProcesses = [];

        public GameLauncherService(
            ILogger<GameLauncherService> logger,
            IGameManifestRepository gameManifestRepository,
            IGameProfileRepository gameProfileRepository,
            IModManifestRepository modManifestRepository)
        {
            this.logger = logger;
            this.gameManifestRepository = gameManifestRepository;
            this.gameProfileRepository = gameProfileRepository;
            this.modManifestRepository = modManifestRepository;
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
            if (string.IsNullOrEmpty(profile.Uuid))
            {
                logger.ZLogWarning($"Profile id is empty. Launch is skipped.");
                return null;
            }

            var existing = TryGetRunningProcess(profile.Uuid);
            if (existing != null)
            {
                return existing;
            }

            if (!TryResolveBaseManifest(profile.BaseGameReference, out var manifest) || manifest == null)
            {
                logger.ZLogWarning($"Base game manifest not found for '{profile.BaseGameReference}'.");
                return null;
            }

            string exePath = manifest.ExeFilePath;
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
                runningProcesses.Remove(profile.Uuid);
                process.Dispose();
            };

            if (!process.Start())
            {
                logger.ZLogWarning($"Failed to start process for '{profile.Uuid}'.");
                process.Dispose();
                return null;
            }

            var gameProcess = new GameProcess(profile.Uuid, process);
            runningProcesses[profile.Uuid] = gameProcess;
            return gameProcess;
        }

        private bool TryResolveBaseManifest(GameReference reference, out GameManifest? manifest)
        {
            var visited = new HashSet<GameReference>();
            return TryResolveBaseManifest(reference, visited, out manifest);
        }

        private bool TryResolveBaseManifest(
            GameReference reference,
            HashSet<GameReference> visited,
            out GameManifest? manifest)
        {
            manifest = null;
            if (string.IsNullOrWhiteSpace(reference.Uuid))
            {
                return false;
            }

            if (!visited.Add(reference))
            {
                logger.ZLogWarning($"Base game reference loop detected at '{reference}'.");
                return false;
            }

            if (gameProfileRepository.FindGameProfile(reference, out var profile) && profile != null)
            {
                return TryResolveBaseManifest(profile.BaseGameReference, visited, out manifest);
            }

            return gameManifestRepository.FindGameManifest(reference, out manifest) && manifest != null;
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
                if (!modManifestRepository.FindModManifest(reference, out var manifest) || manifest == null)
                {
                    logger.ZLogWarning($"Mod manifest not found for '{reference.Uuid}'.");
                    continue;
                }

                string modPath = manifest.PakFilePath;
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
