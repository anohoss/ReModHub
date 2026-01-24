using ReModHub.Pages;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace ReModHub.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow, INavigationWindow
    {
        private bool _isInitialized;

        public MainWindow(
            INavigationViewPageProvider pageService,
            INavigationService navigationService)
        {
            InitializeComponent();

            RootNavigation.SetPageProviderService(pageService);
            navigationService.SetNavigationControl(RootNavigation);

            Loaded += (_, _) =>
            {
                InitializeNavigation();
            };
        }

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider pageService)
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
