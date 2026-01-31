namespace ReModHub
{
    public sealed class GameProfile
    {
        public string ManifestFilePath { get; init; } = string.Empty;

        public string Uuid { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public GameReference BaseGameReference { get; init; } = new GameReference();

        public IReadOnlyList<ModReference> ModReferences { get; init; } = [];

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
}


