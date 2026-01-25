namespace ReModHub
{
    public sealed class ModManifest
    {
        public string Uuid { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string PakFilePath { get; init; } = string.Empty;

        /// <summary>
        /// 依存しているMODのリスト
        /// </summary>
        public IReadOnlyList<ModReference> DependentMods { get; init; } = [];
    }

    public readonly struct ModReference : IEquatable<ModReference>
    {
        public ModReference()
        {
            Uuid = string.Empty;
        }

        public bool Equals(ModReference other)
        {
            return Uuid == other.Uuid;
        }

        public override bool Equals(object? obj)
        {
            return obj is ModReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Uuid.GetHashCode();
        }
        // TODO: 識別番号をUUIDなどの一意な形式にする

        /// <summary>
        /// MODの識別番号
        /// </summary>
        public required string Uuid { get; init; }
    }
}
