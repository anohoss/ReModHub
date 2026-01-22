using Microsoft.Extensions.Hosting;

namespace ReModHub
{
    class AppLauncher
        (App app) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            app.Dispatcher.Invoke(() =>
            {
                app.InitializeComponent();
                app.Run();
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            app.Dispatcher.Invoke(() =>
            {
                app.Shutdown();
            });

            return Task.CompletedTask;
        }
    }
}
