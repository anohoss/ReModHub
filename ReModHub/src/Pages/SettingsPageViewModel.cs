using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ReModHub.Appearance;
using ReModHub.Settings;
using Wpf.Ui.Appearance;
using System.Windows.Media;

namespace ReModHub.Pages
{
    public sealed class SettingsPageViewModel : ObservableObject
    {
        private readonly IThemeCoordinator _themeCoordinator;
        private readonly IAppSettingsStore _store;
        private bool _isInitialized;
        private bool _suppressApply;
        private AppSettings _settings = new();

        public SettingsPageViewModel(IThemeCoordinator themeCoordinator, IAppSettingsStore store)
        {
            _themeCoordinator = themeCoordinator;
            _store = store;
            ThemeOptions = new List<ThemeOption>
            {
                new("Light", ApplicationTheme.Light, ThemePreference.Light),
                new("Dark", ApplicationTheme.Dark, ThemePreference.Dark)
            };
            _selectedThemeOption = ThemeOptions[0];
        }

        public IReadOnlyList<ThemeOption> ThemeOptions { get; }

        private ThemeOption _selectedThemeOption;
        public ThemeOption SelectedThemeOption
        {
            get => _selectedThemeOption;
            set
            {
                if (Equals(_selectedThemeOption, value) || value is null)
                {
                    return;
                }

                _selectedThemeOption = value;
                OnPropertyChanged();

                if (_suppressApply)
                {
                    return;
                }

                _ = ApplySelectedThemeAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            _settings = await _store.LoadAsync();
            _suppressApply = true;
            SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Preference == _settings.ThemePreference)
                ?? ThemeOptions[0];
            _suppressApply = false;

            ApplicationThemeManager.Changed += OnThemeChanged;
            _isInitialized = true;
        }

        private async Task ApplySelectedThemeAsync()
        {
            _settings.ThemePreference = SelectedThemeOption.Preference;
            await _themeCoordinator.ApplyAsync(_settings);
            await _store.SaveAsync(_settings);
        }

        private void OnThemeChanged(ApplicationTheme theme, Color systemAccent)
        {
            var option = ThemeOptions.FirstOrDefault(item => item.Theme == theme);
            if (option is null || option == SelectedThemeOption)
            {
                return;
            }

            _suppressApply = true;
            SelectedThemeOption = option;
            _suppressApply = false;
        }
    }

    public sealed record ThemeOption(string DisplayName, ApplicationTheme Theme, ThemePreference Preference);
}
