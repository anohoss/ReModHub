using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using ReModHub.Windows;
using ReModHub.Pages;
using System.IO;

namespace ReModHub
{
    public class Program
    {
        [STAThread]
        private static void Main(string[]? args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging.
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Logging.AddZLoggerFile(Path.Combine(AppPath.LogsDirectoryName, "application.log"), options =>
            {
                options.UseJsonFormatter();
            });

            builder.Services.AddSingleton<GameManifestService>();

            // WPF-UI Services
            builder.Services.AddSingleton<Wpf.Ui.Abstractions.INavigationViewPageProvider, Wpf.Ui.DependencyInjection.DependencyInjectionNavigationViewPageProvider>();
            builder.Services.AddSingleton<Wpf.Ui.INavigationService, Wpf.Ui.NavigationService>();

            // ホーム
            builder.Services.AddSingleton<HomePage>();

            // ゲームプロファイルのライブラリ
            builder.Services.AddTransient<GameProfileLibraryPageViewModelBase, GameProfileLibraryPageViewModel>();
            builder.Services.AddSingleton<GameProfileLibraryPage>();

            // モッドのライブラリ
            builder.Services.AddSingleton<ModLibraryPage>();

            // ライブラリ（統合ページ）
            builder.Services.AddTransient<LibraryPageViewModelBase, LibraryPageViewModel>();
            builder.Services.AddSingleton<LibraryPage>();

            // メインウィンドウ
            builder.Services.AddTransient<MainWindowViewModel>();
            builder.Services.AddSingleton<MainWindow>();

            builder.Services.AddSingleton<App>();
            builder.Services.AddHostedService<AppLauncher>();

            var host = builder.Build();

            host.Run();
        }
    }
}
