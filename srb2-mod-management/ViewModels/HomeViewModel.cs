﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories.Interface;

namespace srb2_mod_management.ViewModels
{
    public class HomeViewModel: ViewModelBase
    {
        private readonly ISettingsRepository _settings;

        private readonly IDownloadedModsRepository _downloadedMods;
        
        private string _gamePath;

        private ObservableCollection<Mod> _levels = new ObservableCollection<Mod>();

        private ObservableCollection<Mod> _characters = new ObservableCollection<Mod>();

        private ObservableCollection<Mod> _mods = new ObservableCollection<Mod>();

        private bool _starting;

        private int _index;

        // 

        public HomeViewModel(ISettingsRepository settings, IDownloadedModsRepository downloadedMods)
        {
            _settings = settings;
            _downloadedMods = downloadedMods;
            OpenSettingsCommand = new RelayCommand(() => MessengerInstance.Send(Actions.ToggleSettings));
            FindModsCommand = new RelayCommand(() => MessengerInstance.Send(Enums.Views.Discover));
            StartCommand = new RelayCommand(Start, () => _settings.PathValid() && !Starting);
            DeleteCommand = new RelayCommand(Delete);
            GamePath = _settings.GamePath;

            Levels = _downloadedMods.Levels;
            Characters = _downloadedMods.Characters;
            Mods = _downloadedMods.Mods;

        }

        // 

        public RelayCommand OpenSettingsCommand { get; set; }

        public RelayCommand FindModsCommand { get; set; }

        public RelayCommand StartCommand { get; set; }

        public RelayCommand DeleteCommand { get; set; }

        public int Index
        {
            get => _index;
            set => Set(() => Index, ref _index, value);
        }

        public bool Starting
        {
            get => _starting;
            set
            {
                Set(() => Starting, ref _starting, value);
                StartCommand.RaiseCanExecuteChanged();
            }
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

        public ObservableCollection<Mod> SelectedLevels { get; set; } = new ObservableCollection<Mod>();

        public ObservableCollection<Mod> SelectedCharacters { get; set; } = new ObservableCollection<Mod>();

        public ObservableCollection<Mod> SelectedMods { get; set; } = new ObservableCollection<Mod>();

        public int SelectedItems => SelectedLevels.Count + SelectedCharacters.Count + SelectedMods.Count;

        public int TotalItems => Levels.Count + Characters.Count + Mods.Count;

        public string GamePath
        {
            get => _gamePath;
            set
            {
                Set(() => GamePath, ref _gamePath, value);
                if (value != null && value.Length > 1 && Directory.Exists(GamePath))
                {
                    _settings.GamePath = value;
                    _settings.Save();
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // 
        
        private async void Start()
        {
            Starting = true;

            var mods = SelectedLevels.Concat(SelectedCharacters).Concat(SelectedMods);
            var command = string.Join(" ",
                mods.SelectMany(mod => mod.Files)
                    .Where(file => new[] {".wad", ".lua"}.Any(ext => file.ToLower().EndsWith(ext)))
                    .Distinct()
            );

            var info = new ProcessStartInfo
            {
                WorkingDirectory = _settings.GamePath,
                FileName = _settings.GameExe,
                Arguments = $"-file {command} -opengl",
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = info
            };

            process.Start();

            await Task.Delay(3000);

            Starting = false;
        }

        private async void Delete()
        {
            var collection = Index == 2 ? SelectedMods : Index == 1 ? SelectedCharacters : SelectedLevels;
            var category = Index == 2 ? Category.Mod : Index == 1 ? Category.Character : Category.Level;
            foreach (var mod in collection.ToList())
            {
                foreach(var file in mod.Files)
                    File.Delete(file);
                await _downloadedMods.Remove(category, mod);
                collection.Remove(mod);
            }
            RaisePropertyChanged(nameof(TotalItems));
            RaisePropertyChanged(nameof(SelectedItems));
        }
    }
}