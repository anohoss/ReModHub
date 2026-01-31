using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReModHub.Controls
{
    public partial class GameProfileCardControl : UserControl
    {
        public static readonly DependencyProperty ProfileProperty = DependencyProperty.Register(
            nameof(Profile),
            typeof(GameProfile),
            typeof(GameProfileCardControl));

        public static readonly DependencyProperty LaunchCommandProperty = DependencyProperty.Register(
            nameof(LaunchCommand),
            typeof(ICommand),
            typeof(GameProfileCardControl));

        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
            nameof(EditCommand),
            typeof(ICommand),
            typeof(GameProfileCardControl));

        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register(
            nameof(DeleteCommand),
            typeof(ICommand),
            typeof(GameProfileCardControl));

        public GameProfile? Profile
        {
            get => (GameProfile?)GetValue(ProfileProperty);
            set => SetValue(ProfileProperty, value);
        }

        public ICommand? LaunchCommand
        {
            get => (ICommand?)GetValue(LaunchCommandProperty);
            set => SetValue(LaunchCommandProperty, value);
        }

        public ICommand? EditCommand
        {
            get => (ICommand?)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public GameProfileCardControl()
        {
            InitializeComponent();
        }

        private void OnMenuClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
