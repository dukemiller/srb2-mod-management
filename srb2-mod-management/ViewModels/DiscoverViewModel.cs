using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.ViewModels.Components;

namespace srb2_mod_management.ViewModels
{
    public class DiscoverViewModel : ViewModelBase
    {
        private ViewModelBase _display;

        private readonly Stack<ComponentViews> _views = new Stack<ComponentViews>(); 

        // 

        public DiscoverViewModel()
        {
            Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
            _views.Push(ComponentViews.Categories);

            MessengerInstance.Register<DiscoverModel>(this, HandleDiscoverModel);
            MessengerInstance.Register<Actions>(this, HandleAction);
            
            BackCommand = new RelayCommand(Back);
        }

        // 

        public ViewModelBase Display
        {
            get => _display;
            set => Set(() => Display, ref _display, value);
        }

        public RelayCommand BackCommand { get; set; }

        // 

        private void Back()
        {
            if (_views.Count > 0)
            {
                // Go to the view prior to the requested view
                switch (_views.Pop())
                {
                    case ComponentViews.Categories:
                        MessengerInstance.Send(Enums.Views.Home);
                        break;
                    case ComponentViews.Release:
                        var releases = SimpleIoc.Default.GetInstance<ReleasesViewModel>();
                        releases.LoadingPage = false;
                        Display = releases;
                        break;
                    case ComponentViews.Releases:
                        var categories = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                        categories.Loading = false;
                        Display = categories;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            else
                MessengerInstance.Send(Enums.Views.Home);
        }
        
        private void HandleAction(Actions action)
        {
            if (action == Actions.GoBack)
                BackCommand.Execute(null);
        }

        private async void HandleDiscoverModel(DiscoverModel discoverModel)
        {
            switch (discoverModel.RequestedView)
            {
                case ComponentViews.Releases:
                    Display = await SimpleIoc.Default.GetInstance<ReleasesViewModel>().SetModel(discoverModel);
                    break;
                case ComponentViews.Release:
                    Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(discoverModel);
                    break;
                case ComponentViews.Categories:
                    Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _views.Push(discoverModel.RequestedView);
        }

    }
}