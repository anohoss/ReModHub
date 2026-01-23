using System.Windows.Controls;

namespace ReModHub.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel => (SettingsPageViewModel)DataContext;

        public SettingsPage(SettingsPageViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            Loaded += async (_, _) => await viewModel.LoadAsync();
        }
    }
}
