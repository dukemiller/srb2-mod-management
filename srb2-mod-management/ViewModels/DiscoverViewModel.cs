using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.ViewModels.Components;
using Action = srb2_mod_management.Enums.Action;

namespace srb2_mod_management.ViewModels
{
    public class DiscoverViewModel : ViewModelBase
    {
        private ViewModelBase _display;

        private readonly Stack<ComponentView> _views = new Stack<ComponentView>();

        // 

        public DiscoverViewModel()
        {
            Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
            _views.Push(ComponentView.Categories);

            MessengerInstance.Register<DiscoverModel>(this, HandleDiscoverModel);
            MessengerInstance.Register<Action>(this, HandleAction);
            MessengerInstance.Register<ComponentView>(this, HandleComponentView);
            MessengerInstance.Register<(ReleaseInfo, Category)>(this, _ => HandleMod(_.Item1, _.Item2));
            MessengerInstance.Register<View>(this, HandleView);

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
                    case ComponentView.Categories:
                        MessengerInstance.Send(Enums.View.Home);
                        break;
                    case ComponentView.Release:
                        var releases = SimpleIoc.Default.GetInstance<ReleasesViewModel>();
                        releases.LoadingPage = false;
                        Display = releases;
                        break;
                    case ComponentView.Releases:
                        var categories = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                        categories.Loading = false;
                        Display = categories;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            else
                MessengerInstance.Send(Enums.View.Home);
        }

        private void HandleAction(Action action)
        {
            if (action == Action.GoBack)
                BackCommand.Execute(null);
        }

        private async void HandleDiscoverModel(DiscoverModel discoverModel)
        {
            // Whatever view im requesting, im on the logical view before that
            // when these transition states are attempted

            try
            {
                switch (discoverModel.RequestedView)
                {
                    case ComponentView.Releases:
                        Display = await SimpleIoc.Default.GetInstance<ReleasesViewModel>().SetModel(discoverModel);
                        break;
                    case ComponentView.Release:
                        Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(discoverModel);
                        break;
                    case ComponentView.Categories:
                        Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _views.Push(discoverModel.RequestedView);
            }

            catch (HttpRequestException)
            {
                switch (discoverModel.RequestedView)
                {
                    case ComponentView.Categories:
                        break;
                    case ComponentView.Release:
                        var releases = (ReleasesViewModel) Display;
                        releases.LoadingPage = false;
                        break;
                    case ComponentView.Releases:
                        var categories = (CategoriesViewModel) Display;
                        categories.Loading = false;
                        break;
                }

                // Display error
                var dialog = SimpleIoc.Default.GetInstance<IDialogCoordinator>();
                await dialog.ShowMessageAsync(this,
                    "Network Error",
                    "A network error occured while trying to complete this action. " +
                    "Ensure you're properly connected with no firewall blocking this application and try again. " +
                    "If the problem persists, it may be a server issue.");

                Back();
            }
        }

        private async void HandleMod(ReleaseInfo releaseInfo, Category category)
        {
            var model = new DiscoverModel
            {
                RequestedView = ComponentView.Release,
                Category = category,
                ReleaseInfo = releaseInfo
            };
            
            Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(model);

            // _views.Clear();
        }

        private void HandleComponentView(ComponentView view)
        {
            switch (view)
            {
                case ComponentView.Categories:
                    Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                    break;
                case ComponentView.Release:
                    Display = SimpleIoc.Default.GetInstance<ReleaseViewModel>();
                    break;
                case ComponentView.Releases:
                    Display = SimpleIoc.Default.GetInstance<ReleasesViewModel>();
                    break;
                case ComponentView.Add:
                    var vm = SimpleIoc.Default.GetInstance<AddViewModel>();
                    vm.SetDefaultValues();
                    Display = vm;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }

        private static void HandleView(View view)
        {
            // Remove any loading states
            switch (view)
            {
                case View.Home:
                    SimpleIoc.Default.GetInstance<CategoriesViewModel>().Loading = false;
                    SimpleIoc.Default.GetInstance<ReleasesViewModel>().LoadingPage = false;
                    break;
                case View.Discover:
                    break;
            }
        }
    }
}