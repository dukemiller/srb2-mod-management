using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Enums;
using srb2_mod_management.Models;
using srb2_mod_management.Services.Interface;

namespace srb2_mod_management.ViewModels.Components
{
    public class ReleasesViewModel : ViewModelBase
    {
        private ObservableCollection<ReleaseInfo> _releases = new ObservableCollection<ReleaseInfo>();
        private readonly IModRetreiverService _modService;
        private Page _page;
        private ReleaseInfo _selectedRelease;
        private DiscoverModel _model;
        private int _pageNumber;
        private bool _loadingPage;
        private bool _lastPage;

        // 

        public ReleasesViewModel(IModRetreiverService modService)
        {
            _modService = modService;
            SelectCommand = new RelayCommand(Select, () => !LoadingPage);
            NextPageCommand = new RelayCommand(NextPage, () => !LoadingPage && !LastPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => !LoadingPage && PageNumber > 0);
        }

        // 

        public Page Page
        {
            get => _page;
            set => Set(() => Page, ref _page, value);
        }

        public ObservableCollection<ReleaseInfo> Releases
        {
            get => _releases;
            set => Set(() => Releases, ref _releases, value);
        }

        public ReleaseInfo SelectedRelease
        {
            get => _selectedRelease;
            set => Set(() => SelectedRelease, ref _selectedRelease, value);
        }

        public bool LoadingPage
        {
            get => _loadingPage;
            set
            {
                Set(() => LoadingPage, ref _loadingPage, value);
                NextPageCommand.RaiseCanExecuteChanged();
                PreviousPageCommand.RaiseCanExecuteChanged();
                SelectCommand.RaiseCanExecuteChanged();
            }
        }

        public bool LastPage
        {
            get => _lastPage;
            set
            {
                Set(() => LastPage, ref _lastPage, value);
                NextPageCommand.RaiseCanExecuteChanged();
                PreviousPageCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SelectCommand { get; set; }

        public RelayCommand NextPageCommand { get; set; }

        public RelayCommand PreviousPageCommand { get; set; }

        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                Set(() => PageNumber, ref _pageNumber, value);
                RaisePropertyChanged(nameof(PageNotification));
            }
        }

        public string PageNotification => $"Page {PageNumber + 1}";

        // 

        private void Select()
        {
            if (SelectedRelease != null)
            {
                _model.ReleaseInfo = SelectedRelease;
                _model.RequestedView = ComponentView.Release;
                MessengerInstance.Send(_model);
                LoadingPage = true;
            }
        }
        
        public async Task<ReleasesViewModel> SetModel(DiscoverModel model)
        {
            _model = model;
            LoadingPage = true;
            PageNumber = 0;
            Page = await _modService.RetrievePage(_model.Category, PageNumber);
            Releases = new ObservableCollection<ReleaseInfo>(Page.Releases);
            LastPage = Page.Releases.Count < 20;
            LoadingPage = false;
            return this;
        }

        private async void NextPage()
        {
            LoadingPage = true;
            PageNumber = Math.Min(PageNumber + 1, 9);
            Page = await _modService.RetrievePage(_model.Category, PageNumber);
            Releases = new ObservableCollection<ReleaseInfo>(Page.Releases);
            LastPage = Page.Releases.Count < 20;
            LoadingPage = false;
        }

        private async void PreviousPage()
        {
            LoadingPage = true;
            PageNumber = Math.Max(PageNumber - 1, 0);
            Page = await _modService.RetrievePage(_model.Category, PageNumber);
            LastPage = Page.Releases.Count < 20;
            Releases = new ObservableCollection<ReleaseInfo>(Page.Releases);
            LoadingPage = false;
        }
    }
}