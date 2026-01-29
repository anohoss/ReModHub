using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Wpf.Ui.Controls;

namespace ReModHub.Windows
{
    public partial class NewGameProfileWindow : FluentWindow
    {
        public NewGameProfileWindow(IReadOnlyList<GameManifest> manifests, IReadOnlyList<GameProfile> profiles)
        {
            InitializeComponent();
            ViewModel = new NewGameProfileViewModel(manifests, profiles);
            DataContext = ViewModel;
        }

        public NewGameProfileViewModel ViewModel { get; }

        public GameProfile? CreatedProfile { get; private set; }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            var profile = ViewModel.BuildNewProfile();
            if (profile == null)
            {
                return;
            }

            CreatedProfile = profile;
            DialogResult = true;
            Close();
        }
    }

    public sealed class NewGameProfileViewModel : INotifyPropertyChanged
    {
        private BaseGameOption? selectedBaseGameOption;
        private string displayName = string.Empty;

        public NewGameProfileViewModel(IReadOnlyList<GameManifest> manifests, IReadOnlyList<GameProfile> profiles)
        {
            if (manifests != null)
            {
                for (int i = 0; i < manifests.Count; i++)
                {
                    var manifest = manifests[i];
                    BaseGameOptions.Add(new BaseGameOption(
                        $"Game: {manifest.DisplayName} ({manifest.VersionName})",
                        manifest.ToReference()));
                }
            }

            if (profiles != null)
            {
                for (int i = 0; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    BaseGameOptions.Add(new BaseGameOption(
                        $"Profile: {profile.DisplayName} ({profile.VersionName})",
                        profile.ToReference()));
                }
            }

            SelectedBaseGameOption = BaseGameOptions.FirstOrDefault();
        }

        public ObservableCollection<BaseGameOption> BaseGameOptions { get; } = [];

        public BaseGameOption? SelectedBaseGameOption
        {
            get => selectedBaseGameOption;
            set
            {
                if (selectedBaseGameOption == value)
                {
                    return;
                }

                selectedBaseGameOption = value;
                OnPropertyChanged(nameof(SelectedBaseGameOption));
            }
        }

        public string DisplayName
        {
            get => displayName;
            set
            {
                if (displayName == value)
                {
                    return;
                }

                displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public GameProfile? BuildNewProfile()
        {
            if (SelectedBaseGameOption == null)
            {
                return null;
            }

            string trimmedName = DisplayName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                return null;
            }

            return new GameProfile
            {
                Uuid = Guid.NewGuid().ToString("D"),
                DisplayName = trimmedName,
                VersionName = "1",
                BaseGameReference = SelectedBaseGameOption.BaseGameReference,
                ModReferences = []
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class BaseGameOption
    {
        public BaseGameOption(string displayText, GameReference baseGameReference)
        {
            DisplayText = displayText ?? string.Empty;
            BaseGameReference = baseGameReference;
        }

        public string DisplayText { get; }

        public GameReference BaseGameReference { get; }
    }
}
