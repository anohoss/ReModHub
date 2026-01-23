using ReModHub.Appearance;
using Wpf.Ui.Controls;

namespace ReModHub.Settings
{
    public sealed class AppSettings
    {
        public ThemePreference ThemePreference { get; set; } = ThemePreference.System;

        public WindowBackdropType Backdrop { get; set; } = WindowBackdropType.Auto;

        public bool UpdateAccents { get; set; } = true;
    }
}
