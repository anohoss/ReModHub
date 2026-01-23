using System.Threading.Tasks;
using System.Windows;
using ReModHub.Settings;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ReModHub.Appearance
{
    public sealed class ThemeCoordinator : IThemeCoordinator
    {
        private Window? _mainWindow;
        private bool _isWatching;

        public ThemeCoordinator()
        {
        }

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public Task ApplyAsync(AppSettings settings)
        {
            if (settings.ThemePreference == ThemePreference.System)
            {
                if (_mainWindow is not null)
                {
                    SystemThemeWatcher.Watch(_mainWindow, settings.Backdrop, settings.UpdateAccents);
                    _isWatching = true;
                }
                else
                {
                    ApplySystemTheme(settings);
                }

                UpdateWindowBackdrop(settings.Backdrop);
                return Task.CompletedTask;
            }

            if (_isWatching && _mainWindow is not null)
            {
                SystemThemeWatcher.UnWatch(_mainWindow);
                _isWatching = false;
            }

            var theme = settings.ThemePreference == ThemePreference.Dark
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;

            ApplicationThemeManager.Apply(theme, settings.Backdrop, settings.UpdateAccents);
            UpdateWindowBackdrop(settings.Backdrop);

            return Task.CompletedTask;
        }

        private static void ApplySystemTheme(AppSettings settings)
        {
            var systemTheme = ApplicationThemeManager.GetSystemTheme();
            var theme = systemTheme == SystemTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
            ApplicationThemeManager.Apply(theme, settings.Backdrop, settings.UpdateAccents);
        }

        private void UpdateWindowBackdrop(WindowBackdropType backdrop)
        {
            if (_mainWindow is FluentWindow fluentWindow)
            {
                fluentWindow.WindowBackdropType = backdrop;
            }
        }

    }
}
