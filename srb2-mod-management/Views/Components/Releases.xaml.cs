using System.ComponentModel;
using System.Windows.Controls;

namespace srb2_mod_management.Views.Components
{
    /// <summary>
    /// Interaction logic for Releases.xaml
    /// </summary>
    public partial class Releases 
    {
        public Releases()
        {
            InitializeComponent();
        }

        private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortDirection == null)
                e.Column.SortDirection = ListSortDirection.Ascending;
            e.Handled = false;
        }
    }
}
