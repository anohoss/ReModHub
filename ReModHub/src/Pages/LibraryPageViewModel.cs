using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ReModHub.Pages
{
    /// <summary>
    /// LibraryPage用のViewModelの抽象基底クラス
    /// </summary>
    public abstract class LibraryPageViewModelBase
    {
        /// <summary>
        /// 現在選択されているタブのインデックス（0=ゲーム、1=MOD）
        /// </summary>
        public abstract int SelectedTabIndex { get; set; }

        public abstract ObservableCollection<GameProfileDisplayInfo> GameProfiles { get; }

        public abstract ObservableCollection<ModManifest> ModManifests { get; }
    }

    /// <summary>
    /// LibraryPage用のViewModel実装クラス
    /// </summary>
    internal class LibraryPageViewModel : LibraryPageViewModelBase, INotifyPropertyChanged
    {
        private int selectedTabIndex = 0;

        private readonly GameProfileService gameProfileService;
        private readonly ModManifestService modManifestService;

        public override ObservableCollection<GameProfileDisplayInfo> GameProfiles { get; } = [];

        public override ObservableCollection<ModManifest> ModManifests { get; } = [];

        private CancellationTokenSource initializeCancellationTokenSource = new();

        public override int SelectedTabIndex
        {
            get => selectedTabIndex;
            set
            {
                if (selectedTabIndex != value)
                {
                    selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
        }

        public LibraryPageViewModel(GameProfileService gameProfileService, ModManifestService modManifestService)
        {
            this.gameProfileService = gameProfileService ?? throw new ArgumentNullException(nameof(gameProfileService));
            this.modManifestService = modManifestService ?? throw new ArgumentNullException(nameof(modManifestService));

            InitializeAsync(initializeCancellationTokenSource.Token).ConfigureAwait(false);
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await gameProfileService.RefreshGameProfilesAsync(cancellationToken);
            await modManifestService.RefreshModManifestsAsync(cancellationToken);

            RefreshGameProfiles();
            RefreshModManifests();
        }

        private void RefreshGameProfiles()
        {
            GameProfiles.Clear();
            var cachedProfiles = new List<GameProfileDisplayInfo>();
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
