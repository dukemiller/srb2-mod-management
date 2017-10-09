using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using Category = srb2_mod_management.Enums.Category;

namespace srb2_mod_management.ViewModels.Components
{
    public class CategoriesViewModel : ViewModelBase
    {
        private bool _loading;

        // 

        public CategoriesViewModel()
        {
            ChooseCommand = new RelayCommand<string>(
                _ =>
                {
                    Loading = true;
                    MessengerInstance.Send(new DiscoverModel
                    {
                        Category = ToCategory(_),
                        RequestedView = ComponentViews.Releases
                    });
                },
                _ => !Loading);
        }

        // 

        public RelayCommand<string> ChooseCommand { get; set; }

        public bool Loading
        {
            get => _loading;
            set
            {
                Set(() => Loading, ref _loading, value);
                ChooseCommand.RaiseCanExecuteChanged();
            }
        }

        // 

        private static Category ToCategory(string value)
        {
            switch (value)
            {
                case "Mods":
                    return Category.Mod;
                case "Characters":
                    return Category.Character;
                case "Levels":
                default:
                    return Category.Level;
            }
        }
    }
}