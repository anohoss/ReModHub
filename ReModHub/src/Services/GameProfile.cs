using System;
using System.Collections.Generic;

namespace ReModHub
{
    public sealed class GameProfile
    {
        public string Id { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string VersionName { get; init; } = string.Empty;

        public string BaseGameId { get; init; } = string.Empty;

        public IReadOnlyList<ModManifestReference> ModReferences { get; init; } = [];

        public int ModCount { get; init; }
    }
}
