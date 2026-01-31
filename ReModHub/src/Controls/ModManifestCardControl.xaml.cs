using System.Windows;
using System.Windows.Controls;

namespace ReModHub.Controls
{
    public partial class ModManifestCardControl : UserControl
    {
        public static readonly DependencyProperty ManifestProperty = DependencyProperty.Register(
            nameof(Manifest),
            typeof(ModManifest),
            typeof(ModManifestCardControl));

        public ModManifest? Manifest
        {
            get => (ModManifest?)GetValue(ManifestProperty);
            set => SetValue(ManifestProperty, value);
        }

        public ModManifestCardControl()
        {
            InitializeComponent();
        }
    }
}
