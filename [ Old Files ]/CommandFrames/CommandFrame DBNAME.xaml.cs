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

namespace ExternalSupportTools.CommandFrames
{
    public partial class CommandFrame : Page
    {
        public CommandFrame()
        {
            InitializeComponent();

            // Pre-Prompt Text
            SQLText.Text = Environment.MachineName + @"\SQLEXPRESS";

            // Focus Textbox
            CustomersDB.Focus();

            // Hide Elements
            SQLText.Visibility = Visibility.Hidden;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void CustomersDBName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {

                // Save Customers DB Name
                Properties.Settings.Default.CustomersDBName = CustomersDB.Text;

                // Hide Elements
                CustomersDB.Visibility = Visibility.Hidden;
                LabelText.Content = "Please Enter Connection String";
                LabelText_Copy.Content = @"EG: DesktopCM0\SQLEXPRESS";

                // Show Elements
                SQLText.Visibility = Visibility.Visible;

                // Focus Onto Next Textbox
                SQLText.Focus();
            }
        }

        private void SQLMouseClick_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                // Save Customers DB Name
                Properties.Settings.Default.CustomersSQLName = SQLText.Text;

                // Save
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                // Final Handoff
                NavigationService.Navigate(null);
                __Future_Development__.Configurations.StartBackupProcess startBackupProcess = new();
                startBackupProcess.StartBatchExecution();
            }
        }
    }
}
