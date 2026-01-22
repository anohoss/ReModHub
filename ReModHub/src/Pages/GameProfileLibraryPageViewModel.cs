using CommunityToolkit.Mvvm.ComponentModel;
using ReModHub.Controls;
using System.Collections.ObjectModel;

namespace ReModHub.Pages
{
    public abstract class GameProfileLibraryPageViewModelBase
    {
        public abstract ObservableCollection<GameProfileListItem> GameProfileListItems { get; }

        internal virtual void Initialize() { }

        internal virtual void Deinitialize() { }
    }

    [INotifyPropertyChanged]
    internal partial class GameProfileLibraryPageViewModel : GameProfileLibraryPageViewModelBase
    {
        public override ObservableCollection<GameProfileListItem> GameProfileListItems { get; } = [];

        private GameManifestService GameManifestService { get; init; }

        public GameProfileLibraryPageViewModel(GameManifestService gameManifestService)
        {
            GameManifestService = gameManifestService;
        }

        // Note: GameProfileListItemsのデータ更新について
        // コンストラクタでは、GameManifestService が初期化されていない可能性があるため、データ取得に失敗する。
        // コンストラクタでデータ更新を行うべきではない。
        // ページが表示されるタイミングでもデータ更新を行うべきではない。表示・非表示のたびに比較的重い処理が発生してしまうため。
        // 確実にデータが変更されたタイミングでデータ更新を行うべきである。それ以外のケースではあくまでデータ取得に収めるべきである。

        internal override void Initialize()
        {
            RefleshGameProfileListItems();
        }

        internal override void Deinitialize()
        {
            GameProfileListItems.Clear();
        }

        private static readonly List<GameManifest> cachedGameManifests = [];

        private void RefleshGameProfileListItems()
        {
            static bool predicate(GameManifest manifest) => manifest is GameProfile;
            GameManifestService.PopulateGameManifests(cachedGameManifests, predicate);

            GameProfileListItems.Clear();
            foreach (var manifest in cachedGameManifests)
            {
                GameProfileListItems.Add(new GameProfileListItem { DisplayName = manifest.DisplayName });
            }

            cachedGameManifests.Clear();
        }
    }
}
