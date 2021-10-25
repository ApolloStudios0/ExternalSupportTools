using Notifications.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsFirewallHelper;

namespace ExternalSupportTools.__Future_Development__.Windows
{
    public partial class Till1_InstallPage : Page
    {
        // Core
        #region Core
        public Till1_InstallPage()
        {
            InitializeComponent();
        }

        public enum NotificationType
        {
            Information,
            Success,
            Warning,
            Error,
            Notification
        }
        #endregion

        // Install EPOS + Crystal Reports + SQL_CLI
        #region InstallEPOS Logic
        private async void SoftwareAndDrivers_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            SoftwareAndDrivers.Opacity = 0.5;

            // Install Blank Users
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

            // Install Basic Drivers [ SQL_CLI + Crystal Reports ]
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
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(EPOS_ProgressChanged);

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
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(EPOS_ProgressChanged);

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
                EPOS_ProggyBar.Visibility = Visibility.Collapsed;

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

            await Task.Delay(15000);

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
        private void EPOS_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Progress Bar Calculations
            SQL_ProggyBar.Visibility = Visibility.Visible;
            SQL_ProggyBar.Value = e.ProgressPercentage;
            #endregion
        }
        #endregion

        // Install Optional Addons
        #region OptionalAddons Logic
        private async void OptionalAddons_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            OptionalAddons.Opacity = 0.5;

            // Download Ninite
            #region Download it, If its doesn't already exist
            try
            {
                if (!File.Exists("OptionalDownloads.exe"))
                {
                    Thread thread = new(() =>
                    {
                        using (var client = new WebClient())
                        {
                            string NiniteDownloadURL = "https://github.com/NebulaFX/PremierEPOS_ExtenalTools/raw/master/Drivers/OptionalDownloads.exe";

                            // Browser Request
                            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                            // Download To Home Directory
                            client.DownloadFileAsync(new Uri(NiniteDownloadURL), "OptionalDownloads.exe");

                            // Notify [ No completion as its 125kb ]
                            var notificationManager = new NotificationManager();

                            notificationManager.Show(new NotificationContent
                            {
                                Title = "Downloading Additional Files",
                                Message = $"Downloading Ninite Files...",
                                Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                            });
                        }
                    });
                    thread.Start();
                }
            }
            catch
            {
                MessageBox.Show("Failed to download Ninite. Please manually add this to the home directory.", "Downloading Issue", MessageBoxButton.OK);
            }
            #endregion

            // Run Ninite
            #region Running Logic
            try { System.Diagnostics.Process.Start("OptionalDownloads.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
            #endregion

            // Wait 10 Seconds, Then Download AnyDesk & Silently Install It
            await Task.Delay(10000);
            #region Download AnyDesk & Run Silently
            string DownloadURL = "https://download.anydesk.com/AnyDesk.msi";

            using (var client = new WebClient())
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        client.DownloadFileAsync(new Uri(DownloadURL), @"AnyDeskInstaller.msi");
                    });

                    // Give 15 Seconds
                    await Task.Delay(15000);

                    // Install MSI Package
                    Process installerProcess = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // Run & Install
                    processInfo.Arguments = @"/i AnyDeskInstaller.msi /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();

                    TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "--- ADDITONAL OPERATIONS SUCCESSFUL ---";
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            #endregion
        }
        #endregion

        // Install SQL Files
        #region Install SQL Logic

        // Download SQL Express + Management Studio
        #region Download Logic [SQL Express, Management Studio]
        public void DownloadFullSetupDrivers()
        {
            Thread thread1 = new(() =>
            {
                // DOWNLOAD FILES
                #region SQL Express Download
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string EXPRESS_DOWNLOAD_URL = "https://go.microsoft.com/fwlink/?linkid=866658";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(SQL_EXPRESS_FINISHED_DOWNLOAD); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(SQL_ProgressChanged);

                        // Actual Download Execution

                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(EXPRESS_DOWNLOAD_URL), "SQL_Express.exe");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to download full drivers." + Environment.NewLine + ex.Message);
                }
            });
            thread1.Start();
            #endregion

            Thread thread2 = new(() =>
            {
                // DOWNLOAD FILES
                #region Management Studio Download
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string MANAGEMENT_STUDIO_DOWNLOAD_URL = "https://aka.ms/ssmsfullsetup";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(MANAGEMENT_STUDIO_FINISHED_DOWNLOAD); // Notify and install if specified

                        // Progress Bar
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(SQL_ProgressChanged);

                        // Actual Download Execution

                        this.Dispatcher.Invoke(() =>
                        {
                            client.DownloadFileAsync(new Uri(MANAGEMENT_STUDIO_DOWNLOAD_URL), "Management_Studio.exe");
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to download full drivers." + Environment.NewLine + ex.Message);
                }
            });
            thread2.Start();
            #endregion
        }

        private void SQL_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Progress Bar Calculations
            SQL_ProggyBar.Visibility = Visibility.Visible;
            SQL_ProggyBar.Value = e.ProgressPercentage;
            #endregion
        }

        public void SQL_EXPRESS_FINISHED_DOWNLOAD(object sender, AsyncCompletedEventArgs e)
        {
            #region Express Finished Download
            try { Process.Start(@"SQL_Express.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
            #endregion
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "SQL Express Downloaded Successfully";
        }

        public async void MANAGEMENT_STUDIO_FINISHED_DOWNLOAD(object sender, AsyncCompletedEventArgs e)
        {
            #region Management Studio Finished Download
            try { Process.Start(@"Management_Studio.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
            SQL_ProggyBar.Visibility = Visibility.Hidden;

            // Enable SA Configuration Button [ I chose this file as its the largest, and takes the longest to download]
            Properties.Settings.Default.CheckFullInstall = true;
            #endregion

            #region Notify Progrss On Completion
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Management Studio Downloaded Successfully ";
            await Task.Delay(1500);
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "--- SQL FILE DOWNLOAD OPERATIONS COMPLETE ---";
            #endregion
        }
        #endregion

        private void SQLFiles_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            SQLFiles.Opacity = 0.5;

            // Driver Download
            DownloadFullSetupDrivers();
        }
        #endregion

        // Run Global Fixes
        #region Global Fixes Logic

        private async Task FixSQLServerAsync()
        {
            // Advisory Warning [ This Process stops SQL Server ]
            #region Notify on Start
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = "Starting SQL Update",
                Message = $"Please wait while SQL server is updated",
                Type = (Notifications.Wpf.NotificationType)NotificationType.Notification
            });
            #endregion

            // Build Batch File
            #region Build Command + Create Directory [ Cant save direct to C Drive so it makes a folder 'SQLFIXING' ]

            string[] lines =
            {
                @"cd program files\microsoft SQL server\mssql15.sqlexpress\mssql\binn",
                @"sqlservr -m -T4022 -T3659 -s ""SQLEXPRESS"" -q ""Latin1_General_CI_AS""",
                };

            // Create Directory For File Saving
            string directory = @"C:\SQLFixing";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Execute Reader [ Fill Bat File With Commands from Lines Array ]
            await File.WriteAllLinesAsync(@"C:\SQLFixing" + @"\FixSQLServer.bat", lines);

            #endregion Build Command + Create Directory [ Cant save direct to C Drive so it makes a folder 'SQLFIXING' ]

            // Stop SQL Sercvice [ Does Not Error If SQL does not exist ]
            #region Stop SQL Service
            try
            {
                // Stopping SQL Service [ Preparation For Execution ]
                try
                {
                    new System.ServiceProcess.ServiceController("MSSQL$SQLEXPRESS").Stop();
                }
                catch (Exception exc) // Catching those pesky errors [ for rob ;) ]
                {
                    MessageBox.Show($"This service is already stopped, attempting to start.. {exc.Message}", "Failed To Stop SQL"); // I'm not Dan..
                }

                // Execute Built Batch File
                System.Diagnostics.Process.Start(@"C:\SQLFIXING\FixSQLServer.bat");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Start the previously stopped SQL service
                new ServiceController("MSSQL$SQLEXPRESS").WaitForStatus(ServiceControllerStatus.Stopped);
                try { new ServiceController("MSSQL$SQLEXPRESS").Start(); } catch { }
                TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "SQL Server Patched To The Latest Version";
            }
            #endregion
        }
        private async void RunGlobalFixes_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            #region Mark Completion
            RunGlobalFixes.Opacity = 0.5;
            #endregion

            // Open SQL Ports
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
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Opened SQL Ports [ 1317 & 1434 ]";
            #endregion

            // Build & Run Regedit Fix [ TLS FIX ]
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

            await Task.Delay(2000);

            // Notify Progression Window
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Fixed TLS & Patched To Version 1.2";
            #endregion
            #endregion

            // Fix SQL Server
            #region Fix SQL Server
            try { await FixSQLServerAsync(); } catch { MessageBox.Show("SQL NOT INSTALLED"); }
            #endregion

            // Set Region Settings
            #region Region Logic
            // Build Powershell Command
            string[] PowerShellLines =
            {
                @"Get-WinSystemLocale",
                @"Set-WinSystemLocale en --- English (United Kingdom)",
                };

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

            // Execute Reader
            await File.WriteAllLinesAsync(@"C:\SQLFixing" + @"\ChangeUKSettings.ps1", PowerShellLines);

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

            // Notify Completion Progress
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "Region Settings Changed To United Kingdom";
            #endregion

            // Notify Global Done
            TheProgressTextBox.Text = TheProgressTextBox.Text + Environment.NewLine + "--- GLOBAL SETTINGS OPERATION COMPLETE ---";
        }
        #endregion
    }
}
