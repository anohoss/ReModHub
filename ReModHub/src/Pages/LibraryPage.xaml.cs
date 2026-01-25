using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ReModHub.Pages
{
    /// <summary>
    /// ゲームライブラリとMODライブラリを切り替え可能なLibraryPage
    /// </summary>
    public partial class LibraryPage : Page
    {
        private CancellationTokenSource? _loadCancellationTokenSource;

        public LibraryPageViewModelBase ViewModel => (LibraryPageViewModelBase)DataContext;

        public LibraryPage(LibraryPageViewModelBase viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await ViewModel.InitializeAsync(_loadCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation triggered by navigation/unload.
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loadCancellationTokenSource?.Cancel();
        }
    }
}
