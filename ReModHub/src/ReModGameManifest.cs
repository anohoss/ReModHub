namespace ReModHub
{
    internal class ReModGameManifest : GameManifest
    {
        public override string DisplayName { get; } = string.Empty;

        public override string VersionName { get; } = string.Empty;

        /// <summary>
        /// 実行ファイル名
        /// </summary>
        public string ExeFileName { get; init; } = string.Empty;

        public ReModGameManifest(string displayName, string versionName, string exeFileName)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            VersionName = versionName ?? throw new ArgumentNullException(nameof(versionName));
            ExeFileName = exeFileName ?? throw new ArgumentNullException(nameof(exeFileName));
        }
    }
}
