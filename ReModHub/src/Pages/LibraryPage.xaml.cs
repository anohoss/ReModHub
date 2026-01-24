using System.Windows.Controls;

namespace ReModHub.Pages
{
    /// <summary>
    /// ゲームライブラリとMODライブラリを切り替え可能なLibraryPage
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPageViewModelBase ViewModel => (LibraryPageViewModelBase)DataContext;

        public LibraryPage(LibraryPageViewModelBase viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
