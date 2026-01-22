using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ReModHub.Controls
{
    public class GameProfileListItem
    {
        public required string DisplayName { get; init; } = string.Empty;
    }

    /// <summary>
    /// Interaction logic for GameProfileListControl.xaml
    /// </summary>
    public partial class GameProfileListControl : UserControl
    {
        // Dependency properties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(ObservableCollection<GameProfileListItem>),
            typeof(GameProfileListControl));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ObservableCollection<GameProfileListItem> Items
        {
            get => (ObservableCollection<GameProfileListItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public GameProfileListControl()
        {
            Items = [];

            InitializeComponent();
        }
    }
}
