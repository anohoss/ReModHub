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

        public GameProfileLibraryControl()
        {
            InitializeComponent();
        }
    }
}
