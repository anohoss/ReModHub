namespace ReModHub
{
    public abstract class GameManifest
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// バージョン名
        /// </summary>
        public abstract string VersionName { get; }

        /// <summary>
        /// 識別番号
        /// </summary>
        public string Id => $"{DisplayName.ToLower().Replace(" ", "")}-{VersionName}";

        /// <summary>
        /// GameManifestReferenceを生成する
        /// </summary>
        public GameManifestReference ToReference()
        {
            return new GameManifestReference
            {
                Id = Id
            };
        }
    }

    public readonly struct GameManifestReference : IEquatable<GameManifestReference>
    {
        public GameManifestReference()
        {
            Id = string.Empty;
        }

        // TODO: 識別番号をUUIDなどの一意な形式にする

        /// <summary>
        /// ゲームの識別番号
        /// </summary>
        public string Id { get; init; } = string.Empty;

        public bool Equals(GameManifestReference other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is GameManifestReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(GameManifestReference left, GameManifestReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameManifestReference left, GameManifestReference right)
        {
            return !(left == right);
        }
    }
}
