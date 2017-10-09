using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using srb2_mod_management.Models;
using srb2_mod_management.ViewModels;

namespace srb2_mod_management.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        public Home()
        {
            InitializeComponent();
        }

        // https://stackoverflow.com/questions/31176949/binding-selecteditems-of-listview-to-viewmodel
        private void LevelsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewmodel = (HomeViewModel) DataContext;
            viewmodel.SelectedLevels = new ObservableCollection<Mod>(LevelsListBox.SelectedItems
                .Cast<Mod>());
            viewmodel.RaisePropertyChanged("SelectedItems");
        }

        private void CharactersListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewmodel = (HomeViewModel)DataContext;
            viewmodel.SelectedCharacters = new ObservableCollection<Mod>(CharactersListBox.SelectedItems
                .Cast<Mod>());
            viewmodel.RaisePropertyChanged("SelectedItems");
        }

        private void ModsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewmodel = (HomeViewModel)DataContext;
            viewmodel.SelectedMods = new ObservableCollection<Mod>(ModsListBox.SelectedItems
                .Cast<Mod>());
            viewmodel.RaisePropertyChanged("SelectedItems");
        }
    }
}
