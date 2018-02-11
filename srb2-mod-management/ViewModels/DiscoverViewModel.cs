using System;
using System.Collections.Generic;
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
            MessengerInstance.Register<ProfileModel>(this, HandleProfile);
            MessengerInstance.Register<Action>(this, HandleAction);
            MessengerInstance.Register<View>(this, HandleView);
            MessengerInstance.Register<ComponentView>(this, HandleComponentView);

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
                        MessengerInstance.Send(View.Home);
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
                MessengerInstance.Send(View.Home);
        }

        private void HandleAction(Action action)
        {
            if (action == Action.GoBack)
                BackCommand.Execute(null);
        }

        /// <summary>
        ///     Received the view driver model, depending on request change view and set appropriate model
        /// </summary>
        private async void HandleDiscoverModel(DiscoverModel discover)
        {
            // Whatever view im requesting, im on the logical view before that
            // when these transition states are attempted

            try
            {
                switch (discover.RequestedView)
                {
                    case ComponentView.Categories:
                        Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                        break;
                    case ComponentView.Releases:
                        Display = await SimpleIoc.Default.GetInstance<ReleasesViewModel>().SetModel(discover);
                        break;
                    case ComponentView.Release:
                        Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(discover);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _views.Push(discover.RequestedView);
            }

            catch (HttpRequestException)
            {
                switch (discover.RequestedView)
                {
                    case ComponentView.Categories:
                        break;
                    case ComponentView.Releases:
                        if (Display is CategoriesViewModel categories)
                            categories.Loading = false;
                        break;
                    case ComponentView.Release:
                        if (Display is ReleasesViewModel releases)
                            releases.LoadingPage = false;
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

        /// <summary>
        ///     Received a direct profile request, convert it to discover model and send to Release view
        /// </summary>
        private async void HandleProfile(ProfileModel profile)
        {
            var model = new DiscoverModel
            {
                RequestedView = ComponentView.Release,
                Category = profile.Category,
                ReleaseInfo = profile.ReleaseInfo,
                Refresh = profile.Refresh
            };
            
            Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(model);
        }

        /// <summary>
        ///     Received only a componentview, most likely just an Add
        /// </summary>
        private void HandleComponentView(ComponentView view)
        {
            switch (view)
            {
                case ComponentView.Categories:
                    Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                    break;
                case ComponentView.Releases:
                    Display = SimpleIoc.Default.GetInstance<ReleasesViewModel>();
                    break;
                case ComponentView.Release:
                    Display = SimpleIoc.Default.GetInstance<ReleaseViewModel>();
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

        /// <summary>
        ///     Received a direct view change, global view request change, set progressable states to default
        /// </summary>
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