using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using srb2_mod_management.Repositories;
using srb2_mod_management.Repositories.Interface;
using srb2_mod_management.Services;
using srb2_mod_management.Services.Interface;
using srb2_mod_management.ViewModels;
using srb2_mod_management.ViewModels.Components;

namespace srb2_mod_management
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Repositories
            SimpleIoc.Default.Register<ISettingsRepository>(SettingsRepository.Load);
            SimpleIoc.Default.Register<IDownloadedModsRepository>(DownloadedModsRepository.Load);

            // Services
            SimpleIoc.Default.Register<IModRetreiverService, Srb2ForumService>();

            // Viewmodels
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<DiscoverViewModel>();

            // Components
            SimpleIoc.Default.Register<CategoriesViewModel>();
            SimpleIoc.Default.Register<ReleasesViewModel>();
            SimpleIoc.Default.Register<ReleaseViewModel>();
            SimpleIoc.Default.Register<AddViewModel>();
        }

        public static MainWindowViewModel Main => ServiceLocator.Current.GetInstance<MainWindowViewModel>();
        public static HomeViewModel Home => ServiceLocator.Current.GetInstance<HomeViewModel>();
        public static DiscoverViewModel Discover => ServiceLocator.Current.GetInstance<DiscoverViewModel>();

    }
}