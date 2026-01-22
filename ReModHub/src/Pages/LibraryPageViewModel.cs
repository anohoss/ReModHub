using System.ComponentModel;

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
    }

    /// <summary>
    /// LibraryPage用のViewModel実装クラス
    /// </summary>
    internal class LibraryPageViewModel : LibraryPageViewModelBase, INotifyPropertyChanged
    {
        private int selectedTabIndex = 0;

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

        public LibraryPageViewModel()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
