using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ReModHub.Commands;
using System.Threading;
using System.Threading.Tasks;
using ReModHub.Windows;

namespace ReModHub.Pages
{
    /// <summary>
    /// LibraryPage用のViewModelの抽象基底クラス
    /// </summary>
    public abstract class LibraryPageViewModelBase
    {
        public abstract ObservableCollection<GameProfile> GameProfiles { get; }

        public abstract ObservableCollection<ModManifest> ModManifests { get; }

        public abstract ICommand LaunchGameProfileCommand { get; }

        public abstract ICommand EditGameProfileCommand { get; }

        public abstract ICommand CreateGameProfileCommand { get; }

        public abstract ICommand DeleteGameProfileCommand { get; }

        public abstract Task InitializeAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// LibraryPage用のViewModel実装クラス
    /// </summary>
    internal class LibraryPageViewModel : LibraryPageViewModelBase, INotifyPropertyChanged
    {
        private readonly GameProfileService gameProfileService;
        private readonly GameManifestService gameManifestService;
        private readonly GameLauncherService gameLauncherService;

        public override ObservableCollection<GameProfile> GameProfiles { get; } = [];

        private readonly ModManifestService modManifestService;

        public override ObservableCollection<ModManifest> ModManifests { get; } = [];

        public override ICommand LaunchGameProfileCommand { get; }

        public override ICommand EditGameProfileCommand { get; }

        public override ICommand CreateGameProfileCommand { get; }

        public override ICommand DeleteGameProfileCommand { get; }

        private bool isInitialized;

        public LibraryPageViewModel(
            GameProfileService gameProfileService,
            GameManifestService gameManifestService,
            ModManifestService modManifestService,
            GameLauncherService gameLauncherService)
        {
            this.gameProfileService = gameProfileService ?? throw new ArgumentNullException(nameof(gameProfileService));
            this.gameManifestService = gameManifestService ?? throw new ArgumentNullException(nameof(gameManifestService));
            this.modManifestService = modManifestService ?? throw new ArgumentNullException(nameof(modManifestService));
            this.gameLauncherService = gameLauncherService ?? throw new ArgumentNullException(nameof(gameLauncherService));

            LaunchGameProfileCommand = new RelayCommand<GameProfile>(LaunchGameProfileAsync);
            EditGameProfileCommand = new RelayCommand<GameProfile>(EditGameProfileAsync);
            CreateGameProfileCommand = new RelayCommand(CreateGameProfileAsync);
            DeleteGameProfileCommand = new RelayCommand<GameProfile>(DeleteGameProfileAsync);

        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInitialized)
            {
                return;
            }

            await gameManifestService.RefreshGameManifestsAsync(cancellationToken);
            await gameProfileService.RefreshGameProfilesAsync(cancellationToken);
            await modManifestService.RefreshModManifestsAsync(cancellationToken);

            RefreshGameProfiles();
            RefreshModManifests();

            isInitialized = true;
        }

        private void RefreshGameProfiles()
        {
            GameProfiles.Clear();

            var cachedProfiles = new List<GameProfile>();
            gameProfileService.PopulateGameProfiles(cachedProfiles);

            foreach (var profile in cachedProfiles)
            {
                GameProfiles.Add(profile);
            }

            OnPropertyChanged(nameof(GameProfiles));
        }

        private void RefreshModManifests()
        {
            ModManifests.Clear();

            var cachedManifests = new List<ModManifest>();
            modManifestService.PopulateModManifests(cachedManifests);

            foreach (var manifest in cachedManifests)
            {
                ModManifests.Add(manifest);
            }

            OnPropertyChanged(nameof(ModManifests));
        }

        private Task LaunchGameProfileAsync(GameProfile? profile)
        {
            if (profile == null)
            {
                return Task.CompletedTask;
            }

            gameLauncherService.StartGame(profile);
            return Task.CompletedTask;
        }

        private async Task EditGameProfileAsync(GameProfile? profile)
        {
            if (profile == null)
            {
                return;
            }

            string baseGameDisplay = ResolveBaseGameDisplay(profile);
            var window = new EditGameProfileWindow(profile, baseGameDisplay)
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = window.ShowDialog();
            if (result != true || window.UpdatedProfile == null)
            {
                return;
            }

            var savedProfile = await gameProfileService.SaveGameProfileAsync(window.UpdatedProfile, CancellationToken.None);
            if (savedProfile == null)
            {
                return;
            }

            ReplaceProfileInCollection(savedProfile);
        }

        private async Task CreateGameProfileAsync()
        {
            var manifests = new List<GameManifest>();
            gameManifestService.PopulateGameManifests(manifests, _ => true);

            var profiles = new List<GameProfile>();
            gameProfileService.PopulateGameProfiles(profiles);

            if (manifests.Count == 0 && profiles.Count == 0)
            {
                return;
            }

            var window = new NewGameProfileWindow(manifests, profiles)
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = window.ShowDialog();
            if (result != true || window.CreatedProfile == null)
            {
                return;
            }

            var savedProfile = await gameProfileService.SaveGameProfileAsync(window.CreatedProfile, CancellationToken.None);
            if (savedProfile == null)
            {
                return;
            }

            ReplaceProfileInCollection(savedProfile);
        }

        private async Task DeleteGameProfileAsync(GameProfile? profile)
        {
            if (profile == null)
            {
                return;
            }

            bool deleted = await gameProfileService.DeleteGameProfileAsync(profile, CancellationToken.None);
            if (!deleted)
            {
                return;
            }

            RemoveProfileFromCollection(profile);
        }

        private string ResolveBaseGameDisplay(GameProfile profile)
        {
            if (gameManifestService.FindGameManifest(profile.BaseGameReference, out var manifest) && manifest != null)
            {
                return $"{manifest.DisplayName} ({manifest.Uuid})";
            }

            return profile.BaseGameReference.Uuid;
        }

        private void ReplaceProfileInCollection(GameProfile profile)
        {
            for (int i = 0; i < GameProfiles.Count; i++)
            {
                if (GameProfiles[i].Uuid == profile.Uuid)
                {
                    GameProfiles[i] = profile;
                    return;
                }
            }

            GameProfiles.Add(profile);
        }

        private void RemoveProfileFromCollection(GameProfile profile)
        {
            for (int i = GameProfiles.Count - 1; i >= 0; i--)
            {
                if (GameProfiles[i].Uuid == profile.Uuid)
                {
                    GameProfiles.RemoveAt(i);
                    return;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
