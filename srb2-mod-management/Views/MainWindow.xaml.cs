using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using srb2_mod_management.ViewModels;

namespace srb2_mod_management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // SimpleIoc.Default.GetInstance<MainWindowViewModel>().DialogCoordinator = DialogCoordinator.Instance;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel) DataContext).DialogCoordinator = DialogCoordinator.Instance;
        }
    }
}
