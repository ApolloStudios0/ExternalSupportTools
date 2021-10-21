using Microsoft.Win32;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections;

namespace ExternalSupportTools.CommandFrames
{
    public partial class ChecklistWindfow : Window
    {
        // Core
        #region Constructors
        public ChecklistWindfow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        // Obtain Business Name & Begin Loading
        #region Start Logic
        private void BusinessName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Check Business Name
                if (!string.IsNullOrEmpty(BusinessName.Text)) { Properties.Settings.Default.BusinessName = BusinessName.Text; BeginLoadingProcedure(); } else { MessageBox.Show("Please enter a valid business name."); }
            }
        }

        private async void BeginLoadingProcedure()
        {
            // Hide All Other Boxes
            LabelText.Visibility = Visibility.Collapsed;
            LabelText_Copy.Visibility = Visibility.Collapsed;
            BusinessName.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Visible;


            // Check Pre-Required Assets
            LoadingText.Content = "Downloading Java 32/64";
            JavaDownload();

            // The Rest Thread From A Completed Java Download
        }
        #endregion

        // Download Java
        #region Java Logic
        public async void JavaDownload()
        {
            Thread thread = new(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string Java_DownloadURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/JavaDownload.msi";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadJavaComplete); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                        // Actual Download Execution
                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(Java_DownloadURL), "JavaDownload.msi");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to download drivers." + Environment.NewLine + ex.Message);
                }
            });
            thread.Start();
        }

        public async void DownloadJavaComplete(object sender, AsyncCompletedEventArgs e)
        {
            // Install MSI Package
            Process installerProcess = new Process();
            ProcessStartInfo processInfo = new ProcessStartInfo();

            // Run & Install
            processInfo.Arguments = @"/i JavaDownload.msi /q";
            processInfo.FileName = "msiexec";
            installerProcess.StartInfo = processInfo;

            // Start Installer
            installerProcess.Start();
            installerProcess.WaitForExit();

            // Hide Finished Progress Bar
            ProggyBar.Visibility = Visibility.Hidden;

            // Start Next Download
            Properties.Settings.Default.InstallJava6432 = true;
            DownloadNewWallpaper_Initial();
        }
        #endregion

        // Set Desktop Wallpaper
        #region Wallpaper Logic

        public async void DownloadNewWallpaper_Initial() // Initial
        {
            LoadingText.Content = "Setting Desktop Wallpaper";
            await Task.Delay(1500);
            WallpaperDownload_Start();
        }

        public void WallpaperDownload_Start() // Begin Download
        {
            Properties.Settings.Default.SetDesktopWallpaper = true;
            Thread thread = new(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string Wallpaper_URL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/WALLPAPER.jpg";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadWallpaper_Complete); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                        // Actual Download Execution
                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(Wallpaper_URL), "WALLPAPER.jpg");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to download drivers." + Environment.NewLine + ex.Message);
                }
            });
            thread.Start();
        }

        public async void DownloadWallpaper_Complete(object sender, AsyncCompletedEventArgs e)
        {
            // Move File To C:/SQLFixing Folder For Saving
            #region Move Logic
            try
            {
                // File Locations + Declarations
                string OurDirectory = $"WALLPAPER.jpg";
                string TheirDirectory = @"C:\SQLFixing";

                // Move Dat Sucker
                File.Move(OurDirectory, Path.Combine(TheirDirectory, Path.GetFileName(OurDirectory)));
            }
            catch { }
            #endregion

            // Use Downloaded Wallpaper
            string WallpaperDIR = @"C:\SQLFixing\WALLPAPER.jpg";
            DisplayPicture(WallpaperDIR);

            // Give 1 Second
            await Task.Delay(5000);

            // Start Region Settings
            SetRegionSettings();
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;

        private static void DisplayPicture(string file_name)
        {
            uint flags = 0;
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0, file_name, flags))
            {
                Console.WriteLine("Error");
            }
        }
        #endregion

        // Set Region Settings
        #region Region Logic
        public async void SetRegionSettings()
        {
            Properties.Settings.Default.SetDateTimeRegion = true;
            // Load Text
            LoadingText.Content = "Changing Region Settings";
            await Task.Delay(3000);
            // Build Powershell Command
            string[] lines =
            {
                @"Get-WinSystemLocale",
                @"Set-WinSystemLocale en --- English (United Kingdom)",
            };

            // Create Directory For File Saving
            string directory = @"C:\SQLFixing";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

            // Execute Reader
            await File.WriteAllLinesAsync(@"C:\SQLFixing" + @"\ChangeUKSettings.ps1", lines);

            // Run Powershell
            using (PowerShell PowerShellInst = PowerShell.Create())
            {

                PowerShell ps = PowerShell.Create();

                string param1 = "Get-WinSystemLocale";
                string param2 = "Set-WinSystemLocale en --- English (United Kingdom)";
                string scriptPath = @"C:\SQLFixing\ChangeUKSettings.ps1";

                ps.AddScript(File.ReadAllText(scriptPath));

                ps.AddArgument(param1);
                ps.AddArgument(param2);

                ps.Invoke();
            }

            // Clean Up & Handoff
            await Task.Delay(4000);
            First_Stage_Complete();
        }
        #endregion

        // Progress Bar Calculations
        #region Logic
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProggyBar.Visibility = Visibility.Visible;
            ProggyBar.Value = e.ProgressPercentage;
        }
        #endregion

        // <><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
        public async void First_Stage_Complete()
        {
            // Set Text [Time for threads to close&clean]
            LoadingText.Content = "Checking Files & Folders";
            await Task.Delay(2000);
            LoadingText.Content = "Loading Checklist";
            await Task.Delay(2000);

            // Check Files & Folders
            CommandFrames.Checklist_Complete checklist_Complete = new();
            checklist_Complete.Show();
            this.Close();
        }
    }
}
