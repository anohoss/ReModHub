using System.IO;

namespace ReModHub
{
    internal static class AppPath
    {
        public static string RootDirectoryName => AppDomain.CurrentDomain.BaseDirectory;

        public static string TempDirectoryName => Path.Combine(RootDirectoryName, "Temp");

        public static string LogsDirectoryName => Path.Combine(RootDirectoryName, "Logs");

        public static string GameDirectoryName => Path.Combine(RootDirectoryName, "Games");

        public static string GameProfileDirectoryName => Path.Combine(RootDirectoryName, "GameProfiles");

        public static string ModDirectoryName => Path.Combine(RootDirectoryName, "Mods");
    }
}
