using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using Action = srb2_mod_management.Enums.Action;

namespace srb2_mod_management.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _settingsIsOpen;

        private ViewModelBase _display;

        private bool _isHome = true;

        // 

        public MainWindowViewModel()
        {
            MessengerInstance.Register<Action>(this, ActionMessage);
            MessengerInstance.Register<View>(this, ViewMessage);
            Display = SimpleIoc.Default.GetInstance<HomeViewModel>();
            GoBackCommand = new RelayCommand(() => MessengerInstance.Send(Action.GoBack));
            GoHomeCommand = new RelayCommand(() => MessengerInstance.Send(View.Home));
        }

        // 

        public RelayCommand GoBackCommand { get; set; }

        public RelayCommand GoHomeCommand { get; set; }

        public bool SettingsIsOpen
        {
            get => _settingsIsOpen;
            set => Set(() => SettingsIsOpen, ref _settingsIsOpen, value);
        }

        public ViewModelBase Display
        {
            get => _display;
            set => Set(() => Display, ref _display, value);
        }

        public bool IsHome
        {
            get => _isHome;
            set => Set(() => IsHome, ref _isHome, value);
        }

        // 

        private void ActionMessage(Action action)
        {
            switch (action)
            {
                case Action.ToggleSettings:
                    SettingsIsOpen ^= true;
                    break;
                default:
                    break;
            }
        }

        private void ViewMessage(View view)
        {
            switch (view)
            {
                case View.Home:
                    // Remove selections
                    var vm = SimpleIoc.Default.GetInstance<HomeViewModel>();
                    vm.SelectedCharacters = new ObservableCollection<Mod>();
                    vm.SelectedMods = new ObservableCollection<Mod>();
                    vm.SelectedLevels = new ObservableCollection<Mod>();
                    vm.SelectedScripts = new ObservableCollection<Mod>();
                    Display = vm;
                    IsHome = true;
                    break;

                case View.Discover:
                    Display = SimpleIoc.Default.GetInstance<DiscoverViewModel>();
                    IsHome = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }

    }
}