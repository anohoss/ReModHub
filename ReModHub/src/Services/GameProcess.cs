using System;
using System.Diagnostics;

namespace ReModHub
{
    public sealed class GameProcess : IDisposable
    {
        public string ProfileId { get; }

        public Process Process { get; }

        public GameProcess(string profileId, Process process)
        {
            ProfileId = profileId ?? throw new ArgumentNullException(nameof(profileId));
            Process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public void Stop()
        {
            if (Process.HasExited)
            {
                return;
            }

            try
            {
                if (Process.CloseMainWindow())
                {
                    if (Process.WaitForExit(2000))
                    {
                        return;
                    }
                }
            }
            catch
            {
                // Ignore and fall through to Kill.
            }

            try
            {
                Process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore stop failures for now.
            }
        }

        public void Dispose()
        {
            try
            {
                if (!Process.HasExited)
                {
                    Stop();
                }
            }
            finally
            {
                Process.Dispose();
            }
        }
    }
}
