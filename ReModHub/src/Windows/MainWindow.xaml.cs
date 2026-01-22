using System.Windows;
using System.Windows.Controls;
using ReModHub.Pages;
using Wpf.Ui.Controls;

namespace ReModHub.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow, Wpf.Ui.INavigationWindow
    {
        private bool _isInitialized;

        public MainWindow(
            MainWindowViewModel viewModel,
            Wpf.Ui.Abstractions.INavigationViewPageProvider pageService,
            Wpf.Ui.INavigationService navigationService)
        {
            DataContext = viewModel;
            InitializeComponent();
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(this);

            RootNavigation.SetPageProviderService(pageService);
            navigationService.SetNavigationControl(RootNavigation);

            Loaded += (_, _) => InitializeNavigation();
        }

        public Wpf.Ui.Controls.INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(Wpf.Ui.Abstractions.INavigationViewPageProvider pageService)
        {
            RootNavigation.SetPageProviderService(pageService);
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            RootNavigation.SetServiceProvider(serviceProvider);
        }

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        private void InitializeNavigation()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            RootNavigation.Navigate(typeof(HomePage));
        }
    }
}
