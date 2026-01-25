using Microsoft.Extensions.Hosting;
using ReModHub.Appearance;
using ReModHub.Settings;
using ReModHub.Windows;

namespace ReModHub.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public sealed class ApplicationHostService : IHostedService
    {
        private readonly MainWindow _mainWindow;
        private readonly IThemeCoordinator _themeCoordinator;
        private readonly IAppSettingsStore _settingsStore;

        public ApplicationHostService(
            MainWindow mainWindow,
            IThemeCoordinator themeCoordinator,
            IAppSettingsStore settingsStore)
        {
            _mainWindow = mainWindow;
            _themeCoordinator = themeCoordinator;
            _settingsStore = settingsStore;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _themeCoordinator.Initialize(_mainWindow);
            var settings = await _settingsStore.LoadAsync();
            await _themeCoordinator.ApplyAsync(settings);

            _mainWindow.Show();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
