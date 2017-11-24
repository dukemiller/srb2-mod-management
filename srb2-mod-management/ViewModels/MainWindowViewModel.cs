using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;

namespace srb2_mod_management.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _settingsIsOpen;

        private ViewModelBase _display;

        // 

        public MainWindowViewModel()
        {
            MessengerInstance.Register<Actions>(this, ActionMessage);
            MessengerInstance.Register<Enums.Views>(this, ViewMessage);
            Display = SimpleIoc.Default.GetInstance<HomeViewModel>();
            GoBackCommand = new RelayCommand(() => MessengerInstance.Send(Actions.GoBack));
        }

        // 

        public RelayCommand GoBackCommand { get; set; }

        public IDialogCoordinator DialogCoordinator { get; set; }

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

        // 

        private void ActionMessage(Actions action)
        {
            switch (action)
            {
                case Actions.ToggleSettings:
                    SettingsIsOpen ^= true;
                    break;
                default:
                    break;
            }
        }

        private void ViewMessage(Enums.Views view)
        {
            switch (view)
            {
                case Enums.Views.Home:
                    // Remove selections
                    var vm = SimpleIoc.Default.GetInstance<HomeViewModel>();
                    vm.SelectedCharacters = new ObservableCollection<Mod>();
                    vm.SelectedMods = new ObservableCollection<Mod>();
                    vm.SelectedLevels = new ObservableCollection<Mod>();
                    vm.SelectedScripts = new ObservableCollection<Mod>();
                    Display = vm;
                    break;

                case Enums.Views.Discover:
                    Display = SimpleIoc.Default.GetInstance<DiscoverViewModel>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }

    }
}