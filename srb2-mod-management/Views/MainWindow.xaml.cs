using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using srb2_mod_management.ViewModels;

namespace srb2_mod_management.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel) DataContext).DialogCoordinator = DialogCoordinator.Instance;
        }
    }
}
