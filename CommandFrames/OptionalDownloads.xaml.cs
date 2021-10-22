using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ExternalSupportTools.CommandFrames
{
    public partial class OptionalDownloads : Page
    {
        #region Core
        public OptionalDownloads()
        {
            InitializeComponent();
            ProggyBar.Visibility = Visibility.Hidden;

            string directory = @"Fixes";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Load Barcodes, If Visible
            BarcodeFONTMAC.Content = Properties.Settings.Default.CustomersMAC;
            CustomersMACAddressLabel.Content = Properties.Settings.Default.CustomersMAC;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Progress Bar Calculations
            ProggyBar.Visibility = Visibility.Visible;
            ProggyBar.Value = e.ProgressPercentage;
            #endregion
        }
        #endregion

        public async void InstallOpenOffice(){
            #region Install Open Office
            string DownloadURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/openoffice4111.msi";

            using (var client = new WebClient())
            {
                try
                {
                    // Progress Bar
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                    this.Dispatcher.Invoke(() =>
                    {
                        client.DownloadFileAsync(new Uri(DownloadURL), @"Fixes\OpenOfficeInstaller.msi");
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i Fixes\OpenOfficeInstaller.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            #endregion
        }

        public async void InstallAnyDesk() {
            #region Install AnyDesk
            string DownloadURL = "https://download.anydesk.com/AnyDesk.msi";

            using (var client = new WebClient())
            {
                try
                {
                    // Progress Bar
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                    this.Dispatcher.Invoke(() =>
                    {
                        client.DownloadFileAsync(new Uri(DownloadURL), @"Fixes\AnyDeskInstaller.msi");
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i Fixes\AnyDeskInstaller.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            #endregion
        }

        public async void InstallChrome(){
            #region Install Chrome
            string DownloadURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/GoogleChromeStandaloneEnterprise64.msi";

            using (var client = new WebClient())
            {
                try
                {
                    // Progress Bar
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                    this.Dispatcher.Invoke(() =>
                    {
                        client.DownloadFileAsync(new Uri(DownloadURL), @"Fixes\ChromeInstaller.msi");
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i Fixes\ChromeInstaller.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            #endregion
        }

        public async void InstallClassicShell() {
            #region Install Classic Shell
            string DownloadURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/ClassicShellSetup64_4_3_1.msi";

            using (var client = new WebClient())
            {
                try
                {
                    // Progress Bar
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                    this.Dispatcher.Invoke(() =>
                    {
                        client.DownloadFileAsync(new Uri(DownloadURL), @"ClassicShell.msi");
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i ClassicShell.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            #endregion
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)OpenOffice.IsChecked) { InstallOpenOffice(); Properties.Settings.Default.InstallOpenOffice = true; } // Open Office

                if ((bool)AnyDesk.IsChecked) { InstallAnyDesk(); Properties.Settings.Default.InstallAnyDesk = true; } // AnyDesk

                if ((bool)Chrome.IsChecked) { InstallChrome(); Properties.Settings.Default.InstallChrome = true; } // Chrome

                if ((bool)ClassicShell.IsChecked) { InstallClassicShell(); Properties.Settings.Default.InstallClassicShell = true; } // Classic Shell
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
