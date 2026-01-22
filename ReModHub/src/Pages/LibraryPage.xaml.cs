using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace ReModHub.Pages
{
    /// <summary>
    /// ゲームライブラリとMODライブラリを切り替え可能なLibraryPage
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPageViewModelBase ViewModel => (LibraryPageViewModelBase)DataContext;

        public GameProfileLibraryPage GameProfileLibraryPage { get; init; }

        public ModLibraryPage ModLibraryPage { get; init; }

        public LibraryPage(
            LibraryPageViewModelBase viewModel,
            GameProfileLibraryPage gameProfileLibraryPage,
            ModLibraryPage modLibraryPage)
        {
            DataContext = viewModel;

            GameProfileLibraryPage = gameProfileLibraryPage;
            ModLibraryPage = modLibraryPage;

            InitializeComponent();

            // 初期タブをナビゲート
            NavigateToTab(viewModel.SelectedTabIndex);
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == LibraryTabView)
            {
                NavigateToTab(LibraryTabView.SelectedIndex);
            }
        }

        private void NavigateToTab(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    if (GameLibraryFrame.Content != GameProfileLibraryPage)
                    {
                        GameLibraryFrame.Navigate(GameProfileLibraryPage);
                    }
                    break;
                case 1:
                    if (ModLibraryFrame.Content != ModLibraryPage)
                    {
                        ModLibraryFrame.Navigate(ModLibraryPage);
                    }
                    break;
            }
        }
    }
}
