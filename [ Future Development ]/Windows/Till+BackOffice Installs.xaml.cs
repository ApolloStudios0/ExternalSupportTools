using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExternalSupportTools.__Future_Development__.Windows
{
    public partial class Till_BackOffice_Installs : Page
    {
        public Till_BackOffice_Installs()
        {
            InitializeComponent();
        }

        private void BackOffice_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Windows.Back_Office_Installation();
        }

        private void Till1Setup_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Windows.Till1_InstallPage();
        }

        private void PrinterInstall_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
