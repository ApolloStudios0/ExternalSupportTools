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
using System.Windows.Shapes;

namespace ExternalSupportTools.CommandFrames
{
    public partial class Checklist_Complete : Window
    {
        public Checklist_Complete()
        {
            InitializeComponent();
            IsEnabled = true;
            StartTheChecklistChecker();
            BusinessName.Content = Properties.Settings.Default.BusinessName;
        }

        public async void StartTheChecklistChecker() 
        {
            // Start Checklist [ Done In Iffs To Save Space ;) ]
            if (Properties.Settings.Default.InstallPremierEPOSSoftware == true) { INSTALL_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.InstallSQLFiles == true) { MANAGEMENTSTUDIO_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.LicenseKey == true) { LICENSEKEY_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.InstallAnyDesk == true) { INSTALLADDITIONALOPTIONS_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.InstallJava6432 == true) { JAVA3264_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.OpenSQLPorts == true) { OPENSQLPORTS_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.WindowsUpdates == true) { WINDOWSUPDATES_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.OCDCashDrawer == true) { TESTCASHDRAWER_CHECKBOX.IsChecked = true; }
            await Task.Delay(500);
            if (Properties.Settings.Default.SetDateTimeRegion == true) { DATETIMEREGION_CHECKBOX.IsChecked = true; }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        } // Drag Window

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prompt Print
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true) { printDialog.PrintVisual(CheckList, "InstallChecklist"); }
            }
            catch
            {
                MessageBox.Show("Failed to print");
            }
        } // Print Button
    }
}
