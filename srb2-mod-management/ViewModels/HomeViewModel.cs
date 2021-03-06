﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories.Interface;
using srb2_mod_management.Services.Interface;
using Action = srb2_mod_management.Enums.Action;

namespace srb2_mod_management.ViewModels
{
    public class HomeViewModel: ViewModelBase
    {
        private readonly ISettingsRepository _settings;

        private readonly IDownloadedModsRepository _downloadedMods;

        private readonly IModRetreiverService _modService;
        
        private ObservableCollection<Mod> _levels = new ObservableCollection<Mod>();

        private ObservableCollection<Mod> _characters = new ObservableCollection<Mod>();

        private ObservableCollection<Mod> _mods = new ObservableCollection<Mod>();
        
        private ObservableCollection<Mod> _scripts;

        private bool _starting;

        private int _index;

        private GameOptions _options;

        private bool _pathValid;

        // 

        public HomeViewModel(ISettingsRepository settings, IDownloadedModsRepository downloadedMods, IModRetreiverService modService)
        {
            _settings = settings;
            _downloadedMods = downloadedMods;
            _modService = modService;

            // Commands
            OpenSettingsCommand = new RelayCommand(() => MessengerInstance.Send(Action.ToggleSettings));
            FindModsCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(View.Discover);
                MessengerInstance.Send(ComponentView.Categories);
            }, () => PathValid);
            AddModsCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(View.Discover);
                MessengerInstance.Send(ComponentView.Add);
            });
            StartCommand = new RelayCommand(Start, () => PathValid && !Starting);
            DeleteCommand = new RelayCommand(Delete);
            HighlightCommand = new RelayCommand(Highlight);
            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenFileBrowserCommand = new RelayCommand(OpenFileBrowser);
            DeselectCommand = new RelayCommand(Deselect);

            PathValid = _settings.PathValid();
            Options = _settings.Options;
            Options.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "GameExe")
                {
                    PathValid = _settings.PathValid();
                    StartCommand.RaiseCanExecuteChanged();
                    FindModsCommand.RaiseCanExecuteChanged();
                }
                ;
                _settings.Save();
            };
            
            // Lists
            LoadCollections();

            // Messages
            MessengerInstance.Register<Mod>(this, StartSingle);
        }

        // 

        public RelayCommand OpenSettingsCommand { get; set; }

        public RelayCommand OpenFileBrowserCommand { get; set; }

        public RelayCommand AddModsCommand { get; set; }

        public RelayCommand FindModsCommand { get; set; }

        public RelayCommand StartCommand { get; set; }

        public RelayCommand DeleteCommand { get; set; }

        public RelayCommand HighlightCommand { get; set; }

        public RelayCommand OpenProfileCommand { get; set; }

        public RelayCommand DeselectCommand { get; set; }

        public System.Action DeselectRequest { get; set; }

        /// <summary>
        ///     The tab panes index for retreiving a collection.
        /// </summary>
        public int Index
        {
            get => _index;
            set => Set(() => Index, ref _index, value);
        }

        /// <summary>
        ///     Indication of the exe being started
        /// </summary>
        public bool Starting
        {
            get => _starting;
            set
            {
                Set(() => Starting, ref _starting, value);
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        public bool PathValid
        {
            get => _pathValid;
            set => Set(() => PathValid, ref _pathValid, value);
        }

        public ObservableCollection<Mod> Levels
        {
            get => _levels;
            set => Set(() => Levels, ref _levels, value);
        }

        public ObservableCollection<Mod> Characters
        {
            get => _characters;
            set => Set(() => Characters, ref _characters, value);
        }

        public ObservableCollection<Mod> Mods
        {
            get => _mods;
            set => Set(() => Mods, ref _mods, value);
        }

        public ObservableCollection<Mod> Scripts
        {
            get => _scripts;
            set => Set(() => Scripts, ref _scripts, value);
        }

        public ObservableCollection<Mod> SelectedLevels { get; set; } = new ObservableCollection<Mod>();

        public ObservableCollection<Mod> SelectedCharacters { get; set; } = new ObservableCollection<Mod>();

        public ObservableCollection<Mod> SelectedMods { get; set; } = new ObservableCollection<Mod>();

        public ObservableCollection<Mod> SelectedScripts { get; set; } = new ObservableCollection<Mod>();

        public GameOptions Options
        {
            get => _options;
            set => Set(() => Options, ref _options, value);
        }

        public int SelectedItems => SelectedLevels.Count + SelectedCharacters.Count + SelectedMods.Count + SelectedScripts.Count;

        public int TotalItems => Levels.Count + Characters.Count + Mods.Count + Scripts.Count;

        public string Version => "version " + string.Concat(Assembly.GetExecutingAssembly().GetName().Version.ToString().Reverse().Skip(2).Reverse());

        // 

        private Process CreateSrb2Process(IEnumerable<Mod> mods)
        {
            var info = new ProcessStartInfo
            {
                WorkingDirectory = Options.GamePath,
                FileName = Options.GameExe,
                Arguments = Options.BuildArguments(mods),
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = info
            };

            return process;
        }

        private async void StartSingle(Mod mod)
        {
            if (!PathValid)
                return;

            Starting = true;
            await Task.Run(() => CreateSrb2Process(new[] {mod}).Start());
            await Task.Delay(3000);
            Starting = false;
        }
        
        private async void Start()
        {
            Starting = true;
            await Task.Run(() =>
            {
                var mods = SelectedLevels.Concat(SelectedCharacters).Concat(SelectedMods).Concat(SelectedScripts);
                CreateSrb2Process(mods).Start();
            });
            await Task.Delay(3000);
            Starting = false;
        }

        private async void Delete()
        {
            var collection = Index == 3 ? SelectedMods : Index == 2 ? SelectedScripts : Index == 1 ? SelectedCharacters : SelectedLevels;
            var category = Index == 3 ? Category.Mod : Index == 2 ? Category.Script : Index == 1 ? Category.Character : Category.Level;
            foreach (var mod in collection.ToList())
            {
                if (!mod.IsUserAdded)
                    foreach(var file in mod.Files)
                        File.Delete(file.Path);
                await _downloadedMods.Remove(category, mod);
                collection.Remove(mod);
            }
            RaisePropertyChanged(nameof(TotalItems));
            RaisePropertyChanged(nameof(SelectedItems));
        }

        private async void Highlight()
        {
            var collection = Index == 3 ? SelectedMods : Index == 2 ? SelectedScripts : Index == 1 ? SelectedCharacters : SelectedLevels;
            await Task.Delay(100);
            foreach (var mod in collection)
                mod.Highlighted ^= true;
            await _downloadedMods.Save();
        }

        private void OpenProfile()
        {
            var collection = Index == 3 ? SelectedMods : Index == 2 ? SelectedScripts : Index == 1 ? SelectedCharacters : SelectedLevels;
            var category = Index == 3 ? Category.Mod : Index == 2 ? Category.Script : Index == 1 ? Category.Character : Category.Level;

            var mod = collection.FirstOrDefault();
            if (mod == null)
                return;

            var modinfo = _modService.GetReleaseInfo(mod, category);
            if (modinfo == null)
                return;

            var model = new ProfileModel
            {
                Category = category,
                ReleaseInfo = modinfo,
                Refresh = false
            };
            
            MessengerInstance.Send(View.Discover);
            MessengerInstance.Send(model);
        }

        private void OpenFileBrowser()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = false, CheckFileExists = true, CheckPathExists = true, Filter=".exe files (*.exe)|*.exe"};
            var result = dlg.ShowDialog();
            if (result == true)
                _settings.Options.GameExe = dlg.FileName;
        }

        private void Deselect()
        {
            DeselectRequest?.Invoke();
            SelectedCharacters = new ObservableCollection<Mod>();
            SelectedMods = new ObservableCollection<Mod>();
            SelectedLevels = new ObservableCollection<Mod>();
            SelectedScripts = new ObservableCollection<Mod>();
            RaisePropertyChanged(nameof(SelectedItems));
        }

        private async void LoadCollections()
        {
            await _downloadedMods.Validate();
            Levels = _downloadedMods.Levels;
            Characters = _downloadedMods.Characters;
            Mods = _downloadedMods.Mods;
            Scripts = _downloadedMods.Scripts;
        }
    }
}