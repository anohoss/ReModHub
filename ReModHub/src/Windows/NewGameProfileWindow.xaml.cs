using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ReModHub.Commands;
using Wpf.Ui.Controls;

namespace ReModHub.Windows
{
    public partial class NewGameProfileWindow : FluentWindow
    {
        public NewGameProfileWindow(
            IReadOnlyList<GameManifest> manifests,
            IReadOnlyList<GameProfile> profiles,
            IReadOnlyList<ModManifest> modManifests)
        {
            InitializeComponent();
            ViewModel = new NewGameProfileViewModel(manifests, profiles, modManifests);
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
        private string modSearchText = string.Empty;
        private ModOption? selectedAvailableMod;
        private ModOption? selectedSelectedMod;
        private readonly Dictionary<string, GameProfile> profilesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ModOption> modOptionsById = new(StringComparer.OrdinalIgnoreCase);

        public NewGameProfileViewModel(
            IReadOnlyList<GameManifest> manifests,
            IReadOnlyList<GameProfile> profiles,
            IReadOnlyList<ModManifest> modManifests)
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
                    if (!string.IsNullOrWhiteSpace(profile.Uuid))
                    {
                        profilesById[profile.Uuid] = profile;
                    }

                    BaseGameOptions.Add(new BaseGameOption(
                        $"{profile.DisplayName} ({profile.VersionName})",
                        profile.ToReference()));
                }
            }

            if (modManifests != null)
            {
                for (int i = 0; i < modManifests.Count; i++)
                {
                    var manifest = modManifests[i];
                    var option = new ModOption(
                        $"{manifest.DisplayName} ({manifest.VersionName})",
                        new ModReference { Uuid = manifest.Uuid });
                    ModOptions.Add(option);
                    if (!string.IsNullOrWhiteSpace(manifest.Uuid))
                    {
                        modOptionsById[manifest.Uuid] = option;
                    }
                }
            }

            FilteredModOptions = CollectionViewSource.GetDefaultView(ModOptions);
            FilteredModOptions.Filter = FilterModOption;
            FilteredModOptions.SortDescriptions.Add(
                new SortDescription(nameof(ModOption.DisplayText), ListSortDirection.Ascending));

            AddSelectedModCommand = new RelayCommand(AddSelectedModAsync, () => SelectedAvailableMod != null);
            RemoveSelectedModCommand = new RelayCommand(RemoveSelectedModAsync, () => SelectedSelectedMod != null && !SelectedSelectedMod.IsLocked);

            SelectedBaseGameOption = BaseGameOptions.FirstOrDefault();
        }

        public ObservableCollection<BaseGameOption> BaseGameOptions { get; } = [];

        public ObservableCollection<ModOption> ModOptions { get; } = [];

        public ObservableCollection<ModOption> SelectedMods { get; } = [];

        public ICollectionView FilteredModOptions { get; }

        public ICommand AddSelectedModCommand { get; }

        public ICommand RemoveSelectedModCommand { get; }

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
                UpdateLockedModsFromBaseProfile();
            }
        }

        public string ModSearchText
        {
            get => modSearchText;
            set
            {
                if (modSearchText == value)
                {
                    return;
                }

                modSearchText = value;
                OnPropertyChanged(nameof(ModSearchText));
                FilteredModOptions.Refresh();
            }
        }

        public ModOption? SelectedAvailableMod
        {
            get => selectedAvailableMod;
            set
            {
                if (selectedAvailableMod == value)
                {
                    return;
                }

                selectedAvailableMod = value;
                OnPropertyChanged(nameof(SelectedAvailableMod));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ModOption? SelectedSelectedMod
        {
            get => selectedSelectedMod;
            set
            {
                if (selectedSelectedMod == value)
                {
                    return;
                }

                selectedSelectedMod = value;
                OnPropertyChanged(nameof(SelectedSelectedMod));
                CommandManager.InvalidateRequerySuggested();
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

            var selectedMods = new List<ModReference>();
            for (int i = 0; i < SelectedMods.Count; i++)
            {
                selectedMods.Add(SelectedMods[i].Reference);
            }

            return new GameProfile
            {
                Uuid = Guid.NewGuid().ToString("D"),
                DisplayName = trimmedName,
                VersionName = "1",
                BaseGameReference = SelectedBaseGameOption.BaseGameReference,
                ModReferences = selectedMods
            };
        }

        private bool FilterModOption(object obj)
        {
            if (obj is not ModOption option)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(ModSearchText))
            {
                return true;
            }

            return option.DisplayText.Contains(ModSearchText, StringComparison.OrdinalIgnoreCase)
                || option.Reference.Uuid.Contains(ModSearchText, StringComparison.OrdinalIgnoreCase);
        }

        private Task AddSelectedModAsync()
        {
            if (SelectedAvailableMod == null)
            {
                return Task.CompletedTask;
            }

            if (!SelectedMods.Any(mod => mod.Reference.Uuid == SelectedAvailableMod.Reference.Uuid))
            {
                SelectedMods.Add(SelectedAvailableMod);
            }

            return Task.CompletedTask;
        }

        private Task RemoveSelectedModAsync()
        {
            if (SelectedSelectedMod == null)
            {
                return Task.CompletedTask;
            }

            if (SelectedSelectedMod.IsLocked)
            {
                return Task.CompletedTask;
            }

            SelectedMods.Remove(SelectedSelectedMod);
            return Task.CompletedTask;
        }

        private void UpdateLockedModsFromBaseProfile()
        {
            for (int i = SelectedMods.Count - 1; i >= 0; i--)
            {
                if (SelectedMods[i].IsLocked)
                {
                    SelectedMods[i].IsLocked = false;
                    SelectedMods.RemoveAt(i);
                }
            }

            if (SelectedBaseGameOption == null)
            {
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            var reference = SelectedBaseGameOption.BaseGameReference;
            if (!profilesById.TryGetValue(reference.Uuid, out var baseProfile))
            {
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            var modReferences = baseProfile.ModReferences ?? [];
            for (int i = 0; i < modReferences.Count; i++)
            {
                string modId = modReferences[i].Uuid;
                if (string.IsNullOrWhiteSpace(modId))
                {
                    continue;
                }

                var option = GetOrCreateModOption(modId);
                option.IsLocked = true;

                if (!SelectedMods.Any(mod => mod.Reference.Uuid == modId))
                {
                    SelectedMods.Add(option);
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private ModOption GetOrCreateModOption(string modId)
        {
            if (modOptionsById.TryGetValue(modId, out var existing))
            {
                return existing;
            }

            var option = new ModOption(
                $"未登録MOD ({modId})",
                new ModReference { Uuid = modId });
            modOptionsById[modId] = option;
            return option;
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

    public sealed class ModOption : INotifyPropertyChanged
    {
        private bool isLocked;

        public ModOption(string displayText, ModReference reference)
        {
            DisplayText = displayText ?? string.Empty;
            Reference = reference;
        }

        public string DisplayText { get; }

        public string DisplayTextWithLock => IsLocked ? $"{DisplayText} (固定)" : DisplayText;

        public string LockTooltip => IsLocked ? "ベースプロファイル由来のため解除不可" : string.Empty;

        public ModReference Reference { get; }

        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (isLocked == value)
                {
                    return;
                }

                isLocked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLocked)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTextWithLock)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LockTooltip)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
