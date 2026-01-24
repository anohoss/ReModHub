using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ReModHub.Controls
{
    public partial class GameProfileLibraryControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(GameProfileLibraryControl));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public GameProfileLibraryControl()
        {
            InitializeComponent();
        }
    }
}
