using System;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace ReModHub.Windows
{
    public partial class EditGameProfileWindow : FluentWindow
    {
        public EditGameProfileWindow(GameProfile profile, string baseGameDisplay)
        {
            InitializeComponent();
            ViewModel = new EditGameProfileViewModel(profile, baseGameDisplay);
            DataContext = ViewModel;
        }

        public EditGameProfileViewModel ViewModel { get; }

        public GameProfile? UpdatedProfile { get; private set; }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            UpdatedProfile = ViewModel.BuildUpdatedProfile();
            DialogResult = true;
            Close();
        }
    }

    public sealed class EditGameProfileViewModel : INotifyPropertyChanged
    {
        private readonly GameProfile profile;
        private readonly string baseGameDisplay;
        private string displayName;
        private string versionName;

        public EditGameProfileViewModel(GameProfile profile, string baseGameDisplay)
        {
            this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
            this.baseGameDisplay = baseGameDisplay ?? string.Empty;
            displayName = profile.DisplayName;
            versionName = profile.VersionName;
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

        public string VersionName
        {
            get => versionName;
            set
            {
                if (versionName == value)
                {
                    return;
                }

                versionName = value;
                OnPropertyChanged(nameof(VersionName));
            }
        }

        public string BaseGameDisplay => baseGameDisplay;

        public GameProfile BuildUpdatedProfile()
        {
            return new GameProfile
            {
                ManifestFilePath = profile.ManifestFilePath,
                Uuid = profile.Uuid,
                DisplayName = DisplayName ?? string.Empty,
                VersionName = VersionName ?? string.Empty,
                BaseGameReference = profile.BaseGameReference,
                ModReferences = profile.ModReferences
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
