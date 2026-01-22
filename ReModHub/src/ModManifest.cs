namespace ReModHub
{
    public sealed class ModManifest
    {
        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string PakFilename { get; init; } = string.Empty;

        /// <summary>
        /// 依存しているMODのリスト
        /// </summary>
        public List<ModManifestReference> DependentMods { get; } = [];
    }

    public struct ModManifestReference: IEquatable<ModManifestReference>
    {
        public ModManifestReference()
        {
            Id = string.Empty;
        }

        public bool Equals(ModManifestReference other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is ModManifestReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        // TODO: 識別番号をUUIDなどの一意な形式にする

        /// <summary>
        /// MODの識別番号
        /// </summary>
        public required string Id { get; init; }
    }
}
