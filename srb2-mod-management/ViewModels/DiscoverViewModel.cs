﻿using System;
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
            MessengerInstance.Register<ComponentViews>(this, HandleComponentView);
            MessengerInstance.Register<(ReleaseInfo, Category)>(this, _ => HandleMod(_.Item1, _.Item2));

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
            // Whatever view im requesting, im on the logical view before that
            // when these transition states are attempted

            try
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

            catch (HttpRequestException)
            {
                switch (discoverModel.RequestedView)
                {
                    case ComponentViews.Categories:
                        break;
                    case ComponentViews.Release:
                        var releases = (ReleasesViewModel) Display;
                        releases.LoadingPage = false;
                        break;
                    case ComponentViews.Releases:
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
                RequestedView = ComponentViews.Release,
                Category = category,
                ReleaseInfo = releaseInfo
            };
            
            Display = await SimpleIoc.Default.GetInstance<ReleaseViewModel>().SetModel(model);

            // _views.Clear();
        }

        private void HandleComponentView(ComponentViews view)
        {
            switch (view)
            {
                case ComponentViews.Categories:
                    Display = SimpleIoc.Default.GetInstance<CategoriesViewModel>();
                    break;
                case ComponentViews.Release:
                    Display = SimpleIoc.Default.GetInstance<ReleaseViewModel>();
                    break;
                case ComponentViews.Releases:
                    Display = SimpleIoc.Default.GetInstance<ReleasesViewModel>();
                    break;
                case ComponentViews.Add:
                    var vm = SimpleIoc.Default.GetInstance<AddViewModel>();
                    vm.SetDefaultValues();
                    Display = vm;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }
    }
}