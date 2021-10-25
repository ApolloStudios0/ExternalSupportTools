using Notifications.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsFirewallHelper;

namespace ExternalSupportTools.__Future_Development__.Windows
{
    public partial class Back_Office_Installation : Page
    {
        // Core
        #region Core Logic
        public Back_Office_Installation()
        {
            InitializeComponent();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Progress Bar Calculations
            ProggyBar.Visibility = Visibility.Visible;
            ProggyBar.Value = e.ProgressPercentage;
            #endregion
        }
        #endregion

        // Full Back-Office Installation [ Ports, Drivers, TLS, SQL Fix, EPOS Install, Etc ]
        #region Basic Install Logic
        private async void OpenPorts()
        {
            #region Open Ports
            // Instance & Port Assignment [1317]
            var FIRSTrule = FirewallManager.Instance.CreatePortRule(
                @"PremierEPOS_1317",
                FirewallAction.Allow,
                1317,
                FirewallProtocol.TCP);

            // Instance & Port Assignment [1434]
            var SECONDrule = FirewallManager.Instance.CreatePortRule(
                @"PremierEPOS_1434",
                FirewallAction.Allow,
                1434,
                FirewallProtocol.UDP);

            // Add Those Suckers To Firewall List
            FirewallManager.Instance.Rules.Add(FIRSTrule);
            FirewallManager.Instance.Rules.Add(SECONDrule);

            // Notify Progression Window
            await Task.Delay(1500);
            TheProgressTextBox.Text = "Opened SQL Ports [ 1317 & 1434 ]";
            #endregion
        }

        private void InstallSoftware()
        {
            // Move Logic [ Directory Locked To C: [Marks Suggestion]
            #region Move EPOS to C:Premier EPOS [Core]
            try
            {
                // Locked to C:/PremierEPOS [Core] ( Marks Suggestion )
                string LockedDirectory = @"C:\PremierEPOS [Core]";

                // What The Downloaded Updater Is Called
                string OurDirectory = $"Premier EPOS Update Software.exe";

                // Move Dat Sucker
                File.Move(OurDirectory, System.IO.Path.Combine(LockedDirectory, System.IO.Path.GetFileName(OurDirectory)));
            }
            catch (Exception exc) { MessageBox.Show($"It seems an updater already exists here." + Environment.NewLine + exc.Message); } // I'm not Dan, I catch my errors ;)
            #endregion

            // Run Updater Once Its Moved
            #region Running Logic
            try { System.Diagnostics.Process.Start(@"C:\PremierEPOS [Core]\Premier EPOS Update Software.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }

            // Notify Progression
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Downloaded EPOS Software Successfully";
            #endregion
        }

        private void InstallRequiredDrivers()
        {
            // SQL_CLI & Crystal Reports Downloading & Installation
            #region Download & Run Logic
            Thread thread = new(() =>
            {
                // DOWNLOAD SQL_CLI
                #region Try Downloading SQL_CLI
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string SQLCLI_DownloadURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/sqlncli_x64.msi";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(SQlcli_DownloadComplete); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                        // Actual Download Execution

                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(SQLCLI_DownloadURL), "SqlCLI_Download.msi");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to download drivers." + Environment.NewLine + ex.Message);
                }
                #endregion
            });

            Thread thread2 = new(() =>
            {
                // DOWNLOAD CRYSTAL REPORTS
                #region Try Downloading Crystal Reports
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string CRRuntime_DownloadURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/CRRuntime_32bit_13_0.msi";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(CrystalReports_DownloadComplete); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                        // Actual Download Execution

                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(CRRuntime_DownloadURL), "CRRuntime_Download.msi");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to download drivers." + Environment.NewLine + ex.Message);
                }
                #endregion
            });

            async void CrystalReports_DownloadComplete(object sender, AsyncCompletedEventArgs e)
            {
                // NOTIFY CRYSTAL REPORTS DOWNLOAD COMPLETE
                #region Notify On Crystal Reports Download Completion
                try
                {
                    var notificationManager = new NotificationManager();

                    notificationManager.Show(new NotificationContent
                    {
                        Title = "Cleaning Up",
                        Message = $"Drivers installed. Cleaning up files.",
                        Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i CRRuntime_Download.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();

                    // Notify Progress Window
                    TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Installed Crystal Report Drivers Successfully";
                }
                catch { }

                // Hide Progress Bar [ Bigger Download, Finishes Last ]
                ProggyBar.Visibility = Visibility.Collapsed;

                #endregion
            }

            async void SQlcli_DownloadComplete(object sender, AsyncCompletedEventArgs e)
            {
                // NOTIFY SQL_CLI DOWNLOAD COMPLETE
                #region Notify On SQL_CLI Download Completion
                try
                {
                    var notificationManager = new NotificationManager();

                    notificationManager.Show(new NotificationContent
                    {
                        Title = "Drivers Downloaded",
                        Message = $"Drivers downloaded. Please wait while they are installed.",
                        Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                    });

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i SqlCLI_Download.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();

                    // Notify Progress Window
                    TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Installed SQL_CLI Drivers Successfully";
                }
                catch { }
                #endregion
            }
            #endregion
        }

        private async Task FixTLSAsync()
        {
            // Build & Run Regedit Fix
            #region TLS Fix Logic
            string[] lines =
            {
                @"Windows Registry Editor Version 5.00",
                "",
                @"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319]",
                "\"SchUseStrongCrypto\"=dword:00000001",
                "",
                @"[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319]",
                "\"SchUseStrongCrypto\"=dword:00000001",
                "",
                @"[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2]",
                "",
                @"[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client]",
                "\"DisabledByDefault\"=dword:00000000",
                "\"Enabled\"=dword:00000001",
                "",
                @"[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server]",
                "\"DisabledByDefault\"=dword:00000000",
                "\"Enabled\" = dword:00000001",
            };

            // Create Directory For File Saving
            string directory = @"C:\SQLFixing";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Execute Reader [ Fill Bat File With Commands from Lines Array ]
            await File.WriteAllLinesAsync(@"C:\SQLFixing" + @"\TLSFIX.reg", lines);

            // Run Regedit Fix
            Process regeditProcess = Process.Start("regedit.exe", @"/s C:\SQLFixing\TLSFIX.reg");

            // Notify On Completion
            #region Notify
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = "Forced TLS 1.2",
                Message = $"TLS 1.2 has been configured sucessfully.",
                Type = (Notifications.Wpf.NotificationType)NotificationType.Success
            });

            await Task.Delay(1500);

            // Notify Progression Window
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Fixed TLS & Patched To Version 1.2";
            #endregion
            #endregion
        }

        private async void DownloadBlankUsers()
        {
            // Download & Move Blank Users.mdb
            #region Blank Users Logic
            Thread thread1 = new(async () =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string DOWNLOAD_URL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/Users.mdb";

                        // Actual Download Execution
                        await this.Dispatcher.Invoke(async () =>
                        {
                            client.DownloadFileAsync(new Uri(DOWNLOAD_URL), "Users.mdb");

                            // File Locations + Declarations
                            string OurDirectory = $"Users.mdb";

                            await Task.Delay(10000);

                            // Move Dat Sucker
                            File.Move(OurDirectory, System.IO.Path.Combine(@"C:\PremierEPOS [Core]", System.IO.Path.GetFileName(OurDirectory)));

                            // Notify Progression Box
                            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Downloaded & Moved Blank Users.mdb File";
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to download users file." + Environment.NewLine + ex.Message);
                }
            });
            thread1.Start();
            #endregion
        }

        private async void FullInstall_Click(object sender, RoutedEventArgs e)
        {
            // Make Button Half Opacity [Marking Completion]
            FullInstall.Opacity = 0.5;

            // Clear Progress
            TheProgressTextBox.Text = "";

            // Open Ports [1317, 1434]
            OpenPorts();

            // Install SQL_CLI + Crystal Reports
            InstallRequiredDrivers();

            // TLS Fix [Web Security?]
            FixTLSAsync();

            // Download & Move Blank Users File [ For EPOS Launching ]
            DownloadBlankUsers();

            // Install EPOS [ Final Step ]
            await Task.Delay(10000); // Gives Program 10 Seconds Leeway
            InstallSoftware();
        }
        #endregion
    }
}
