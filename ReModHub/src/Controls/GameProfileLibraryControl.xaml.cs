using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReModHub.Controls
{
    public partial class GameProfileLibraryControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(GameProfileLibraryControl));

        public static readonly DependencyProperty LaunchCommandProperty = DependencyProperty.Register(
            nameof(LaunchCommand),
            typeof(ICommand),
            typeof(GameProfileLibraryControl));

        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
            nameof(EditCommand),
            typeof(ICommand),
            typeof(GameProfileLibraryControl));

        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register(
            nameof(DeleteCommand),
            typeof(ICommand),
            typeof(GameProfileLibraryControl));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
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

        public GameProfileLibraryControl()
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
