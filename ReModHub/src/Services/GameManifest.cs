namespace ReModHub
{
    public class GameManifest
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// バージョン名
        /// </summary>
        public string VersionName { get; init; } = string.Empty;

        /// <summary>
        /// 実行ファイル名
        /// </summary>
        public string ExeFileName { get; init; } = string.Empty;

        public GameManifest(string id, string displayName, string versionName, string exeFileName)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            VersionName = versionName ?? throw new ArgumentNullException(nameof(versionName));
            ExeFileName = exeFileName ?? throw new ArgumentNullException(nameof(exeFileName));
        }

        /// <summary>
        /// 識別番号
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// GameReferenceを生成する
        /// </summary>
        public GameReference ToReference()
        {
            return new GameReference
            {
                Id = Id
            };
        }
    }

    public readonly struct GameReference : IEquatable<GameReference>
    {
        public GameReference()
        {
            Id = string.Empty;
        }

        // TODO: 識別番号をUUIDなどの一意な形式にする

        /// <summary>
        /// ゲームの識別番号
        /// </summary>
        public string Id { get; init; } = string.Empty;

        public bool Equals(GameReference other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is GameReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(GameReference left, GameReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameReference left, GameReference right)
        {
            return !(left == right);
        }
    }
}
