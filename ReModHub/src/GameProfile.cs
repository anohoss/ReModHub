namespace ReModHub
{
    /// <summary>
    /// ゲームのプロファイル
    /// </summary>
    public sealed class GameProfile(string displayName, string versionName, GameManifestReference baseGameReference) : GameManifest
    {
        public override string DisplayName { get; } = displayName ?? throw new ArgumentNullException(nameof(displayName));

        public override string VersionName { get; } = versionName ?? throw new ArgumentNullException(nameof(versionName));

        /// <summary>
        /// ベースとなるゲーム
        /// </summary>
        public GameManifestReference BaseGameReference { get; init; } = baseGameReference;

        /// <summary>
        /// インストールが必要なMODのリスト
        /// </summary>
        private List<ModManifestReference> ModReferences { get; } = [];

        public GameProfile(string displayName, string versionName, GameManifestReference baseGameReference, ReadOnlySpan<ModManifestReference> modReferences) 
            : this(displayName, versionName, baseGameReference)
        {
            for (int i = 0; i < modReferences.Length; i++)
            {
                RegisterModReference(modReferences[i]);
            }
        }

        public void GetModReferences(List<ModManifestReference> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            results.Clear();

            for (int i = 0; i < ModReferences.Count; i++)
            {
                results.Add(ModReferences[i]);
            }
        }

        public void RegisterModReference(ModManifestReference mod)
        {
            if (!ModReferences.Contains(mod))
            {
                ModReferences.Add(mod);
            }
        }

        public void UnregisterModReference(ModManifestReference mod)
        {
            ModReferences.Remove(mod);
        }
    }
}
