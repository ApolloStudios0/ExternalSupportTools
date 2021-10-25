using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExternalSupportTools.__Future_Development__._Main_Menu
{
    public partial class Main_Menu : Window
    {
        // Core
        #region Core Resources
        public Main_Menu()
        {
           InitializeComponent();

           // Show Tab Control
           FIXES_TAB.Opacity = 0.5;

           // Show Computer Frame
           MainFrame.Content = new Windows.Till_BackOffice_Installs();

            // Download Installer [ For EPOS Updates ]
            #region Download EPOS

            #region Create EPOS Directory
            // Create Directory For File Saving
            string directory = @"C:\PremierEPOS [Core]";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            #endregion

            try
            {
                if (!File.Exists("Premier EPOS Update Software.exe"))
                {
                    Thread thread = new(() =>
                    {
                        using (var client = new WebClient())
                        {
                            // Browser Request
                            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                            // Download URL
                            string EPOS_UPDATER_DOWNLOAD_URL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/Premier%20EPOS%20Update%20Software.exe";

                            // Download To Home Directory
                            client.DownloadFileAsync(new Uri(EPOS_UPDATER_DOWNLOAD_URL), "Premier EPOS Update Software.exe");

                            // Notify [ No completion as its 125kb ]
                            var notificationManager = new NotificationManager();

                            notificationManager.Show(new NotificationContent
                            {
                                Title = "Downloading EPOS Update",
                                Message = $"Downloading new EPOS updater...",
                                Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                            });
                        }
                    });
                    thread.Start();
                }
            }
            catch
            {
                MessageBox.Show("Failed to download new EPOS updater. Please manually add this to the home directory.", "Downloading Issue", MessageBoxButton.OK);
            }
            #endregion
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            // Drag Window
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        // Load Frames For Content
        #region Content Logic
        private void LoadFixFrame(object sender, RoutedEventArgs e)
        {
            // Visible Bar
            FIXES_TAB.Opacity = 1;
            TILL_TAB.Opacity = 0.5;

            MainFrame.Content = null;
        }

        private void LoadTillFrame(object sender, RoutedEventArgs e)
        {
            // Visible Bar
            TILL_TAB.Opacity = 1;
            FIXES_TAB.Opacity = 0.5;

            // Show Computer Frame
            MainFrame.Content = new Windows.Till_BackOffice_Installs();
        }
        #endregion
    }
}
