using System.Windows;
using srb2_mod_management.ViewModels.Components;

namespace srb2_mod_management.Views.Components
{
    /// <summary>
    /// Interaction logic for Release.xaml
    /// </summary>
    public partial class Release
    {
        public Release()
        {
            InitializeComponent();
        }

        private async void Screenshots_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = (ReleaseViewModel) DataContext;
            await vm.VisibleChanged((bool) e.NewValue);
        }
    }
}
