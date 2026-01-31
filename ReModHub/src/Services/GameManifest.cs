namespace ReModHub
{
    public sealed class GameManifest
    {
        /// <summary>
        /// 識別番号
        /// </summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// バージョン名
        /// </summary>
        public string VersionName { get; init; } = string.Empty;

        /// <summary>
        /// 実行ファイルのパス
        /// </summary>
        public string ExeFilePath { get; init; } = string.Empty;

        /// <summary>
        /// GameReferenceを生成する
        /// </summary>
        public GameReference ToReference()
        {
            return new GameReference
            {
                Uuid = Uuid,
            };
        }
    }

    public readonly struct GameReference : IEquatable<GameReference>
    {
        public GameReference()
        {
            Uuid = string.Empty;
        }

        // TODO: 識別番号をUUIDなどの一意な形式にする

        /// <summary>
        /// ゲームの識別番号
        /// </summary>
        public string Uuid { get; init; } = string.Empty;

        public bool Equals(GameReference other)
        {
            return Uuid == other.Uuid;
        }

        public override bool Equals(object? obj)
        {
            return obj is GameReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Uuid);
        }

        public override string ToString()
        {
            return Uuid;
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
