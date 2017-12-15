using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories.Interface;
using Action = srb2_mod_management.Enums.Action;

namespace srb2_mod_management.ViewModels.Components
{
    public class AddViewModel: ViewModelBase
    {
        private readonly IDownloadedModsRepository _modsRepository;

        private Category _selectedCategory;

        private Mod _mod;

        private ModFile _selectedModFile;

        private string _changedThings;

        // 

        public AddViewModel(IDownloadedModsRepository modsRepository)
        {
            _modsRepository = modsRepository;
            OpenFileBrowserCommand = new RelayCommand(OpenFileBrowser);
            AddCommand = new RelayCommand(Add, CanAdd);
            DeleteCommand = new RelayCommand(Delete);
            CancelCommand = new RelayCommand(() => MessengerInstance.Send(Action.GoBack));
        }

        private void ModOnPropertyChanged(object sender, PropertyChangedEventArgs args) => AddCommand.RaiseCanExecuteChanged();

        private void FilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) => AddCommand.RaiseCanExecuteChanged();

        public async void SetDefaultValues()
        {
            var id = 100_000;

            // Find an unused id

            await Task.Run(() =>
            {
                while (_modsRepository.Find(id) != null)
                    id++;
            });

            // Set default values
            SelectedCategory = Category.Level;
            ChangedThings = "";
            Mod = new Mod {Id = id, IsUserAdded = true};

            // Set event listeners for notifying change
            Mod.PropertyChanged -= ModOnPropertyChanged;
            Mod.PropertyChanged += ModOnPropertyChanged;
            Mod.Files.CollectionChanged -= FilesOnCollectionChanged;
            Mod.Files.CollectionChanged += FilesOnCollectionChanged;
        }
        
        // 

        public RelayCommand OpenFileBrowserCommand { get; set; }

        public RelayCommand AddCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }

        public RelayCommand DeleteCommand { get; set; }

        public string ChangedThings
        {
            get => _changedThings;
            set => Set(() => ChangedThings, ref _changedThings, value);
        }

        public ModFile SelectedModFile
        {
            get => _selectedModFile;
            set => Set(() => SelectedModFile, ref _selectedModFile, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => Set(() => SelectedCategory, ref _selectedCategory, value);
        }

        public Mod Mod
        {
            get => _mod;
            set => Set(() => Mod, ref _mod, value);
        }

        public IEnumerable<Category> Categories => Enum.GetValues(typeof(Category)).Cast<Category>();

        // 

        private void Delete() => Mod.Files.Remove(SelectedModFile);

        private void Add()
        {
            Mod.ChangedThings = ChangedThings
                .Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToList();

            _modsRepository.Add(SelectedCategory, Mod);
            MessengerInstance.Send(Action.GoBack);
        }

        private bool CanAdd() => Mod != null 
            && Mod.Files.Count > 0 
            && Mod.Name?.Length > 0 
            && ChangedThings?.Length > 0;

        private void OpenFileBrowser()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = true, CheckFileExists = true };
            var result = dlg.ShowDialog();
            if (result == true)
                foreach(var file in dlg.FileNames)
                    Mod.Files.Add(new ModFile { Path = file });
        }
    }
}