using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using srb2_mod_management.Classes;

namespace srb2_mod_management.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            if (AlreadyOpen)
            {
                NativeMethods.FocusOther();
                Close();
            }

            else
                InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();
        }

        /// <summary>
        ///     Returns the check if there is an already opened srb2_mod_management.
        /// </summary>
        private static bool AlreadyOpen
        {
            get
            {
                var name = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                return Process.GetProcessesByName(name).Length > 1;
            }
        }
    }
}
