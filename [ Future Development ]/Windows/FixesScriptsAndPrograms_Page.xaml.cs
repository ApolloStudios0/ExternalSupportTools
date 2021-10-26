using Notifications.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;
using Notifications.Wpf;
using Ookii.Dialogs.Wpf;

namespace ExternalSupportTools.__Future_Development__.Windows
{
    public partial class FixesScriptsAndPrograms_Page : Page
    {
        // Core
        #region Core
        public FixesScriptsAndPrograms_Page()
        {
            InitializeComponent();

            // Create Directory For File Saving
            string directory = @"C:\SQLBACKUP";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        #endregion

        // Download Hardware Viewer
        #region Download Speccy
        public void SpeccyDownloadComplete(object sender, AsyncCompletedEventArgs e) { Process.Start("Speccy.exe"); }

        private void SystemHardware_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists("Speccy.exe"))
                {
                    Thread thread = new(() =>
                    {
                        using (var client = new WebClient())
                        {
                            // Browser Request
                            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                            // Download URL
                            string SPACE_SNIFFER_DOWNLOADURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/Speccy64.exe";

                            client.DownloadFileCompleted += new AsyncCompletedEventHandler(SpeccyDownloadComplete); // Notify and install if specified

                            // Download To Home Directory
                            client.DownloadFileAsync(new Uri(SPACE_SNIFFER_DOWNLOADURL), "Speccy.exe");

                            // Notify [ No completion as its 125kb ]
                            var notificationManager = new NotificationManager();

                            notificationManager.Show(new NotificationContent
                            {
                                Title = "Starting Speccy Viewer",
                                Message = $"Please wait while Speccy Launches.",
                                Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                            });
                        }
                    });
                    thread.Start();
                }
                else { Process.Start("Speccy.exe"); }
            }
            catch { MessageBox.Show("Unable to download Speccy."); }
        }
        #endregion

        // Download Storage Viewer
        #region Download Storage Viewer
        public void SpaceSnifferFinished(object sender, AsyncCompletedEventArgs e) { Process.Start("SpaceSniffer.exe"); }
        private void StorageViewer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists("SpaceSniffer.exe"))
                {
                    Thread thread = new(() =>
                    {
                        using (var client = new WebClient())
                        {
                            // Browser Request
                            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                            // Download URL
                            string SPACE_SNIFFER_DOWNLOADURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/SpaceSniffer.exe";

                            client.DownloadFileCompleted += new AsyncCompletedEventHandler(SpaceSnifferFinished); // Notify and install if specified

                            // Download To Home Directory
                            client.DownloadFileAsync(new Uri(SPACE_SNIFFER_DOWNLOADURL), "SpaceSniffer.exe");

                            // Notify [ No completion as its 125kb ]
                            var notificationManager = new NotificationManager();

                            notificationManager.Show(new NotificationContent
                            {
                                Title = "Starting Space Viewer",
                                Message = $"Please wait while Space Sniffer Launches.",
                                Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                            });
                        }
                    });
                    thread.Start();
                }
                else { Process.Start("SpaceSniffer.exe"); }
            }
            catch { MessageBox.Show("Unable to download SpaceSniffer."); }
        }
        #endregion

        // Backup
        #region Backup Logic
        private async void Backup_Click(object sender, RoutedEventArgs e)
        {
            TheMainFrame.Content = new CommandFrames.CommandFrame(); // Show UI
        }
        #endregion
    }
}
