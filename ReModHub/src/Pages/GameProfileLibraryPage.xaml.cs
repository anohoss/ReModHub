using System.Windows.Controls;

namespace ReModHub.Pages
{
    /// <summary>
    /// Interaction logic for GameProfileLibraryPage.xaml
    /// </summary>
    public partial class GameProfileLibraryPage : Page
    {
        public GameProfileLibraryPageViewModelBase ViewModel => (GameProfileLibraryPageViewModelBase)DataContext;

        public GameProfileLibraryPage(GameProfileLibraryPageViewModelBase viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
