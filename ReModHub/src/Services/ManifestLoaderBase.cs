using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ZLogger;

namespace ReModHub
{
    internal abstract class ManifestLoaderBase<TManifest, TManifestJsonObject>
        where TManifest : class
        where TManifestJsonObject : class
    {
        private Func<TManifestJsonObject, string, TManifest?> CreateManifestFunc { get; init; }

        protected ManifestLoaderBase(
            ILogger logger,
            Func<TManifestJsonObject, string, TManifest?> createManifestFunc)
        {
            Logger = logger;
            CreateManifestFunc = createManifestFunc
                ?? throw new ArgumentNullException(nameof(createManifestFunc));
        }

        protected ILogger Logger { get; }

        public async Task<IReadOnlyList<TManifest>> LoadAllManifestsAsync(CancellationToken cancellationToken)
        {
            var results = new List<TManifest>();

            foreach (var manifestPath in EnumerateManifestPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await LoadManifestAsync(manifestPath, cancellationToken);
                if (item == null)
                {
                    continue;
                }

                if (!Validate(item, manifestPath))
                {
                    continue;
                }

                results.Add(item);
            }

            return results;
        }

        protected abstract IEnumerable<string> EnumerateManifestPaths();

        protected async Task<TManifest?> LoadManifestAsync(string manifestFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(manifestFilePath))
            {
                Logger.ZLogWarning($"Manifest file not found at {manifestFilePath}");
                return null;
            }
            try
            {
                using var fileStream = File.OpenRead(manifestFilePath);
                var obj = await JsonSerializer.DeserializeAsync<TManifestJsonObject>(fileStream, cancellationToken: cancellationToken);
                if (obj == null)
                {
                    Logger.ZLogError($"Failed to deserialize manifest file with type: {typeof(TManifestJsonObject)} from {manifestFilePath}");
                    return null;
                }

                var manifest = CreateManifestFunc(obj, manifestFilePath);
                if (manifest == null)
                {
                    Logger.ZLogError($"Failed to create manifest from deserialized object of type: {typeof(TManifestJsonObject)} from {manifestFilePath}");
                    return null;
                }

                return manifest;
            }
            catch (Exception ex)
            {
                Logger.ZLogError($"Failed to load manifest: {ex.Message}");
                return null;
            }
        }

        protected virtual bool Validate(TManifest item, string manifestFilePath)
        {
            return true;
        }
    }
}
