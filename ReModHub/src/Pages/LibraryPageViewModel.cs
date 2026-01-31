using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ReModHub.Commands;
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
        private readonly IGameProfileRepository gameProfileRepository;
        private readonly IGameManifestRepository gameManifestRepository;
        private readonly IModManifestRepository modManifestRepository;
        private readonly IGameLauncherService gameLauncherService;

        public override ObservableCollection<GameProfile> GameProfiles { get; } = [];

        public override ObservableCollection<ModManifest> ModManifests { get; } = [];

        public override ICommand LaunchGameProfileCommand { get; }

        public override ICommand EditGameProfileCommand { get; }

        public override ICommand CreateGameProfileCommand { get; }

        public override ICommand DeleteGameProfileCommand { get; }

        private bool isInitialized;

        public LibraryPageViewModel(
            IGameProfileRepository gameProfileRepository,
            IGameManifestRepository gameManifestRepository,
            IModManifestRepository modManifestRepository,
            IGameLauncherService gameLauncherService)
        {
            this.gameProfileRepository = gameProfileRepository ?? throw new ArgumentNullException(nameof(gameProfileRepository));
            this.gameManifestRepository = gameManifestRepository ?? throw new ArgumentNullException(nameof(gameManifestRepository));
            this.modManifestRepository = modManifestRepository ?? throw new ArgumentNullException(nameof(modManifestRepository));
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

            await gameManifestRepository.RefreshAsync(cancellationToken);
            await gameProfileRepository.RefreshAsync(cancellationToken);
            await modManifestRepository.RefreshAsync(cancellationToken);

            RefreshGameProfiles();
            RefreshModManifests();

            isInitialized = true;
        }

        private void RefreshGameProfiles()
        {
            GameProfiles.Clear();

            var cachedProfiles = new List<GameProfile>();
            gameProfileRepository.PopulateGameProfiles(cachedProfiles);

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
            modManifestRepository.PopulateModManifests(cachedManifests);

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

            var savedProfile = await gameProfileRepository.SaveAsync(window.UpdatedProfile, CancellationToken.None);
            if (savedProfile == null)
            {
                return;
            }

            ReplaceProfileInCollection(savedProfile);
        }

        private async Task CreateGameProfileAsync()
        {
            var manifests = new List<GameManifest>();
            gameManifestRepository.PopulateGameManifests(manifests, _ => true);

            var profiles = new List<GameProfile>();
            gameProfileRepository.PopulateGameProfiles(profiles);

            var modManifests = new List<ModManifest>();
            modManifestRepository.PopulateModManifests(modManifests);

            if (manifests.Count == 0 && profiles.Count == 0)
            {
                return;
            }

            var window = new NewGameProfileWindow(manifests, profiles, modManifests)
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = window.ShowDialog();
            if (result != true || window.CreatedProfile == null)
            {
                return;
            }

            var savedProfile = await gameProfileRepository.SaveAsync(window.CreatedProfile, CancellationToken.None);
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

            bool deleted = await gameProfileRepository.DeleteAsync(profile, CancellationToken.None);
            if (!deleted)
            {
                return;
            }

            RemoveProfileFromCollection(profile);
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

        private string ResolveBaseGameDisplay(GameProfile profile)
        {
            var reference = profile.BaseGameReference;
            if (gameProfileRepository.FindGameProfile(reference, out var baseProfile)
                && baseProfile != null)
            {
                var versionLabel = string.IsNullOrWhiteSpace(baseProfile.VersionName)
                    ? baseProfile.Uuid
                    : baseProfile.VersionName;
                return $"{baseProfile.DisplayName} (v{versionLabel})";
            }

            if (gameManifestRepository.FindGameManifest(reference, out var manifest)
                && manifest != null)
            {
                var versionLabel = string.IsNullOrWhiteSpace(manifest.VersionName)
                    ? manifest.Uuid
                    : manifest.VersionName;
                return $"{manifest.DisplayName} (v{versionLabel})";
            }

            return reference.Uuid;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
