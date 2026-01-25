using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ReModHub.Commands;
using System.Threading;
using System.Threading.Tasks;

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

        private CancellationTokenSource initializeCancellationTokenSource = new();

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

            InitializeAsync(initializeCancellationTokenSource.Token).ConfigureAwait(false);
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await gameManifestService.RefreshGameManifestsAsync(cancellationToken);
            await gameProfileService.RefreshGameProfilesAsync(cancellationToken);
            await modManifestService.RefreshModManifestsAsync(cancellationToken);

            RefreshGameProfiles();
            RefreshModManifests();
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
