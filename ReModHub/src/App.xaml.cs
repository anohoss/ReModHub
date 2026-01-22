using System.Windows;
using ReModHub.Windows;

namespace ReModHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
        (MainWindow mainWindow): Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            mainWindow.Show();
        }
    }
}
