namespace ReModHub
{
    public sealed class GameProfileDisplayInfo
    {
        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string BaseGameId { get; init; } = string.Empty;

        public int ModCount { get; init; }
    }
}
