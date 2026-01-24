using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReModHub.Appearance;
using ReModHub.Pages;
using ReModHub.Services;
using ReModHub.Settings;
using ReModHub.Windows;
using Wpf.Ui.DependencyInjection;
using ZLogger;

namespace ReModHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddZLoggerFile(Path.Combine(AppPath.LogsDirectoryName, "application.log"), options =>
                    {
                        options.UseJsonFormatter();
                    });
                });

                services.AddSingleton<IThemeCoordinator, ThemeCoordinator>();
                services.AddSingleton<IAppSettingsStore>(_ => new JsonAppSettingsStore("ReModHub"));

                // WPF-UI Services
                services.AddNavigationViewPageProvider();
                services.AddSingleton<Wpf.Ui.INavigationService, Wpf.Ui.NavigationService>();

                services.AddSingleton<ModManifestService>();
                services.AddSingleton<GameManifestService>();
                services.AddSingleton<GameProfileService>();

                // Pages
                services.AddSingleton<HomePage>();
                services.AddTransient<LibraryPageViewModelBase, LibraryPageViewModel>();
                services.AddSingleton<LibraryPage>();
                services.AddTransient<SettingsPageViewModel>();
                services.AddSingleton<SettingsPage>();

                // Main window
                services.AddSingleton<MainWindow>();

                // App hosting
                services.AddHostedService<ApplicationHostService>();
            })
            .Build();

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://learn.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception
        }
    }
}
