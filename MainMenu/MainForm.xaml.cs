using Microsoft.Win32.TaskScheduler;
using Notifications.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WindowsFirewallHelper;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalSupportTools.MainMenu
{
    public partial class MainForm : Window
    {
        // Core Files
        #region Core

        public MainForm()
        {
            InitializeComponent();

            // Download Installer If It Doesn't Exist
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
                            string EPOS_UPDATER_DOWNLOAD_URL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/Premier%20EPOS%20Update%20Software.exe";

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
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        } // Dragabble Window

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeShutdown();
        }

        public enum NotificationType
        {
            Information,
            Success,
            Warning,
            Error,
            Notification
        }

        #endregion Core

        // Backup
        #region Backup File

        private async void SetupBackupButton_Click(object sender, RoutedEventArgs e)
        {
            #region Grab File Directory

            // Instance
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please Select A Directory For Backup Files...";
            dialog.UseDescriptionForTitle = true;

            // Check if outdated windows [ those damn 1030's ]
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Using old file explorer.", "Pre Windows Vista?");

            // Save Dialog Path [ For BAT usage ]
            if ((bool)dialog.ShowDialog(this))
                Properties.Settings.Default.BackupPath = dialog.SelectedPath;

            #endregion Grab File Directory

            #region Configuration To Specific Customer

            MainFrame.Content = new CommandFrames.CommandFrame(); // Show UI

            #endregion Configuration To Specific Customer
        }

        public async void BeginBatchExecution()
        {
            #region Build Batch File [ Pre-Execution ]

            // Grabbed From Initial Setup
            string DatabaseName = Properties.Settings.Default.CustomersDBName;
            string ConnectionString = Properties.Settings.Default.CustomersSQLName;

            string[] lines =
                {
                    "@echo off",
                    "cls",
                    "title Premier EPOS",
                    "color A",
                    "echo Backing Up Your Data. Please Wait.",
                    $"Set dt=%date:~0,2%-%date:~3,2%-%date:~6,4%_%time:~0,2%_%time:~3,2%_%time:~6,2%", // Set Datetime For Backup File
                    $@"Set ""FileName=backup%dt%""",
                    $@"sqlcmd -S {ConnectionString} -E -Q ""BACKUP DATABASE {DatabaseName} TO Disk='{Properties.Settings.Default.BackupPath}\%FileName%.bak'"""
                };

            await File.WriteAllLinesAsync(Properties.Settings.Default.BackupPath + @"\BackupCore.bat", lines);

            // Save Full File Path For Scheduler
            Properties.Settings.Default.FullFilePathForSchedule = Properties.Settings.Default.BackupPath + @"\BackupCore.bat";
            SetupScheduledTask();

            #endregion Build Batch File [ Pre-Execution ]
        }

        private void SetupScheduledTask()
        {
            #region Default = Runs Every Other Day

            using (TaskService ts = new TaskService())
            {
                // Instance
                TaskDefinition td = ts.NewTask();

                // Description
                td.RegistrationInfo.Description = "This is a scheduled backup program for Premier EPOS.";

                // Runs Every Day
                td.Triggers.Add(new DailyTrigger { DaysInterval = 2 });

                // File To Execute
                td.Actions.Add(new ExecAction($"{Properties.Settings.Default.FullFilePathForSchedule}", @"c:\BackupLog.log", null));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition("EPOS Backup", td);
            }
            #endregion Default = Runs Every Other Day
        }

        #endregion Backup File

        // Fix SQL Server
        #region SQL Fix

        public async void FixSQLMethod()
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

            #region Stop SQL Service
            try
            {
                // Stopping SQL Service [ Preparation For Execution ]
                try
                {
                    new ServiceController("MSSQL$SQLEXPRESS").Stop();
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
            }
            #endregion

            // Notify On Completion
            #region Toast Notification [Completed Download]
            notificationManager.Show(new NotificationContent
            {
                Title = "SQL Update Succesful",
                Message = $"Your SQL server has been updated.",
                Type = (Notifications.Wpf.NotificationType)NotificationType.Success
            });
            #endregion
        }

        private async void FixSqlButton_Click(object sender, RoutedEventArgs e)
        {
            FixSQLMethod();
        }
        #endregion SQL Fix

        // Basic Install
        #region Installation Logic

        public async void OpenPortsMethod()
        {
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
        }

        public async void InstallFullSoftware()
        {
            // Open Ports
            #region Add Firewall Rules
            try
            {
                OpenPortsMethod(); // Open Ports
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion Add Firewall Rules If Specified

            // Selecting Directory For Installation [ Saved to settings 'SelectedPath' ]
            #region Grab File Directory

            // Instance
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please Select A Directory For EPOS installation";
            dialog.UseDescriptionForTitle = true;

            // Check if outdated windows [ those damn sp1030's ]
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Using old file explorer.", "Pre Windows Vista?");

            // Save Dialog Path [ For Installation ]
            if ((bool)dialog.ShowDialog(this))
                Properties.Settings.Default.InstallPath = dialog.SelectedPath;

            #endregion Grab File Directory

            // Move Updater Software There [ From Our Local Files ]
            #region Move Logic

            try
            {
                // File Locations + Declarations
                string OurDirectory = $"Premier EPOS Update Software.exe";
                string TheirDirectory = Properties.Settings.Default.InstallPath;

                // Move Dat Sucker
                File.Move(OurDirectory, Path.Combine(TheirDirectory, Path.GetFileName(OurDirectory)));
            }
            catch (Exception exc)
            {
                // You do realize I only catch these errors for Rob, right?
                // Might not in the future, just for entertainment purposes. Lol.
                MessageBox.Show($"It seems an updater already exists here." + Environment.NewLine + exc.Message);
            }

            #endregion Move Logic

            // Run Update Once Its Moved
            #region Running Logic
            try
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.InstallPath + @"\Premier EPOS Update Software.exe");
            }
            catch (Exception ex)
            {
                // Have to admit, Tom was the first to crash my program. Gurd durmit. God damn try catches Joel, try catches. grr....
                MessageBox.Show(ex.Message);
            }
            #endregion Running Logic
        }

        public async void InstallBasicSoftware()
        {
            // Open Ports
            #region Add Firewall Rules
            try
            {
                OpenPortsMethod(); // Open Ports
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion Add Firewall Rules If Specified

            // Selecting Directory For Installation [ Saved to settings 'SelectedPath' ]
            #region Grab File Directory

            // Instance
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please Select A Directory For Installation";
            dialog.UseDescriptionForTitle = true;

            // Check if outdated windows [ those damn sp1030's ]
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Using old file explorer.", "Pre Windows Vista?");

            // Save Dialog Path [ For Installation ]
            if ((bool)dialog.ShowDialog(this))
                Properties.Settings.Default.InstallPath = dialog.SelectedPath;

            #endregion Grab File Directory

            // Move Updater Software There [ From Our Local Files ]
            #region Move Logic

            try
            {
                // File Locations + Declarations
                string OurDirectory = $"Premier EPOS Update Software.exe";
                string TheirDirectory = Properties.Settings.Default.InstallPath;

                // Move Dat Sucker
                File.Move(OurDirectory, Path.Combine(TheirDirectory, Path.GetFileName(OurDirectory)));
            }
            catch (Exception exc)
            {
                // You do realize I only catch these errors for Rob, right?
                // Might not in the future, just for entertainment purposes. Lol.
                MessageBox.Show($"It seems an updater already exists here." + Environment.NewLine + exc.Message);
            }

            #endregion Move Logic

            // Install Basic Drivers
            #region Installation & Downloading of Drivers Logic

            // Confirm Install
            MessageBoxResult MessageResult = MessageBox.Show("Install SQL_CLI & Crystal Reports?", "Confirm Driver Installation", MessageBoxButton.YesNo);

            switch (MessageResult)
            {
                case MessageBoxResult.Yes:

                    // Download SQLcli & Crystal Reports
                    DownloadBasicDrivers();

                    break;

                case MessageBoxResult.No:
                    break;
            }
            #endregion

            // Run Update Once Its Moved
            #region Running Logic
            try
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.InstallPath + @"\Premier EPOS Update Software.exe");
            }
            catch (Exception ex)
            {
                // Have to admit, Tom was the first to crash my program. Gurd durmit. God damn try catches Joel, try catches. grr....
                MessageBox.Show(ex.Message);
            }
            #endregion Running Logic
        }

        private async void InstallSoftware_Click(object sender, RoutedEventArgs e)
        {
            InstallBasicSoftware();
        }

        public void DownloadSQLCompleted(object sender, AsyncCompletedEventArgs e)
        {
            #region Download SQL Completed [Install Package]
            try
            {
                var notificationManager = new NotificationManager();

                notificationManager.Show(new NotificationContent
                {
                    Title = "Drivers Downloaded",
                    Message = $"Drivers downloaded. Please wait while they are installed.",
                    Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                });

                // Hide Progress Bar
                ProggyBar.Visibility = Visibility.Hidden;

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

                // Start Progress Bar For Second Installation
                ProggyBar.Visibility = Visibility.Visible;
            }
            catch { }
        }
        #endregion

        public void DownloadCRTCompleted(object sender, AsyncCompletedEventArgs e)
        {
            #region Download CR Runtime Complete [Install Package]
            try
            {
                var notificationManager = new NotificationManager();

                notificationManager.Show(new NotificationContent
                {
                    Title = "Cleaning Up",
                    Message = $"Drivers installed. Cleaning up files.",
                    Type = (Notifications.Wpf.NotificationType)NotificationType.Information
                });

                // Hide Progress Bar
                ProggyBar.Visibility = Visibility.Hidden;

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
            }
            catch { }
        }
        #endregion

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Progress Bar Calculations
            ProggyBar.Visibility = Visibility.Visible;
            ProggyBar.Value = e.ProgressPercentage;
            #endregion
        }

        #endregion Installation Logic

        // Full Install
        #region Till 1 Setup Logic

        // Install SQL Express **DOWNLOAD COMPLETED**
        public void SQL_EXPRESS_FINISHED_DOWNLOAD(object sender, AsyncCompletedEventArgs e)
        {
            #region Express Finished Download
            try { Process.Start(@"SQL_Express.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
            #endregion
        }

        // Install SQL Management Studio **DOWNLOAD COMPLETED**
        public void MANAGEMENT_STUDIO_FINISHED_DOWNLOAD(object sender, AsyncCompletedEventArgs e)
        {
            #region Management Studio Finished Download
            try { Process.Start(@"Management_Studio.exe"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
            ProggyBar.Visibility = Visibility.Hidden;

            // Enable SA Configuration Button [ I chose this file as its the largest, and takes the longest to download]
            Properties.Settings.Default.CheckFullInstall = true;
            #endregion
        }

        // Install Crystal Reports + SQL_Cli
        public void DownloadBasicDrivers()
        {
            #region Basic Driver Download [SQL + Crystal Report]
            // Threading as to not freeze UI
            Thread thread = new(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string SQLCLI_DownloadURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/sqlncli_x64.msi";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadSQLCompleted); // Notify and install if specified

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
            });

            Thread thread2 = new(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        // Fake browser request [stops crashing?]
                        client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                        // Download URL's
                        string CRRuntime_DownloadURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/CRRuntime_32bit_13_0.msi";

                        // Notify Completion
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCRTCompleted); // Notify and install if specified

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
            });

            // Start Driver Download
            thread.Start();
            thread2.Start();
            #endregion
        }

        // Download SQL Express + Management Studio
        public void DownloadFullSetupDrivers()
        {
            #region Download Logic [SQL Express, Management Studio]
            Thread thread1 = new(() =>
            {
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
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

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
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

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
            #endregion
        }

        // Force TLS 1.2 [Web Security?]
        public async void TLS_Fix()
        {
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
            #endregion
            #endregion
        }

        public void CreateEPOSDirectory()
        {
            #region Create EPOS Directory
            // Create Directory For File Saving
            string directory = @"C:\PremierEPOS [Core]";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            #endregion
        }

        public void InstallBlankUsers()
        {
            #region Blank Users Logic
            Thread thread1 = new(() =>
            {
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
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

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
        }

        private async void Till1Setup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --------------------
                // Core Configurations
                // --------------------
                CreateEPOSDirectory(); // Create PremierEPOS folder in the C Drive [For Till Setups Touchscreen]
                await System.Threading.Tasks.Task.Delay(5000);
                DownloadBasicDrivers(); // Download SQLcli & Crystal Reports
                InstallFullSoftware(); // Download / Install EPOS [Also Opens Firewall Ports]
                InstallBlankUsers(); // Download & Move Blank Users File

                // Save Installs For Checklist
                Properties.Settings.Default.InstallPremierEPOSSoftware = true;
                Properties.Settings.Default.InstallCrystalReports = true;
                Properties.Settings.Default.OpenSQLPorts = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message + "Failed to download core drivers."); };

            try
            {
                // --------------------
                // Final Configurations
                // --------------------
                TLS_Fix(); // Force TLS 2
            }
            catch (Exception exc) { MessageBox.Show(exc.Message + "Failed to configure system"); }
        }

        #endregion Add Port Rules Logic

        // SA Configuration [Must Have Run Full Installer]
        #region SA Configuration Logic
        private void SAConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.CheckFullInstall)
            {
                //MessageBox.Show("Please run a full installation before configuring SA");
                MessageBox.Show("This feature is not implemented yet.");
            }
            else
            {
                // Configure SQL Server Later Here
                FixSQLMethod(); // Fix SQL Server
            }
        }
        #endregion

        // Install Optional Addons [Chrome, AnyDesk, Classic Shell, Open Office]
        #region Optional Downloads Logic
        private void OptionalAddons_Click(object sender, RoutedEventArgs e)
        {
            CommandFrames.OptionalDownloads OptionalDownloads = new();
            MainFrame.Content = OptionalDownloads;
        }
        #endregion

        // Install SQL Server Files [Segments From Full Install]
        #region SQL Server Express + Studio Download
        private void InstallSQL_Click(object sender, RoutedEventArgs e)
        {
            #region Notify
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = "Drivers Downloading",
                Message = $"Drivers downloading. Please wait while they are downloaded.",
                Type = (Notifications.Wpf.NotificationType)NotificationType.Information
            });
            #endregion

            DownloadFullSetupDrivers();

            Properties.Settings.Default.InstallSQLFiles = true;
        }
        #endregion

        // Administration Menu [ Windows Version, Hardware ]
        #region Administration Logic
        private void Administration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature is not implemented yet.");
        }
        #endregion

        // Force TLS 1+2 Button
        #region Force TLS Logic
        private void ForceTLS_Click(object sender, RoutedEventArgs e)
        {
            TLS_Fix();
        }
        #endregion

        // View Storage Use
        #region View Storage Logic
        public void SpaceSnifferFinished(object sender, AsyncCompletedEventArgs e)
        {
            Process.Start("SpaceSniffer.exe");
        }

        private void StorageUse_Click(object sender, RoutedEventArgs e)
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
                            string SPACE_SNIFFER_DOWNLOADURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/SpaceSniffer.exe";

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

        // View Hardware Specs
        #region View Hardware Logic

        public void SpeccyDownloadComplete(object sender, AsyncCompletedEventArgs e) { Process.Start("Speccy.exe"); }

        private void Hardware_Click(object sender, RoutedEventArgs e)
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
                            string SPACE_SNIFFER_DOWNLOADURL = "https://github.com/NebulaFX/ExternalSupport/raw/master/Drivers/Speccy64.exe";

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

        // Attatch Database [Zip & UnZip]
        #region Attatch Database Logic
        private void AttatchDB_Click(object sender, RoutedEventArgs e)
        {
            // Create Directory For File Zip & Unzip
            string directory = @"C:\SQLFixing";

            // Create If It Doesn't Exist Already
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        #endregion





        // <><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
        // THE BELOW SECTION IS UNDER HEAVY DEVELOPMENT
        // <><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
        private void BeginChecklist_Click(object sender, RoutedEventArgs e)
        {
            var CautionMsg = MessageBox.Show("This feature is still under development. Please confirm you would like to continue", "Proceed?", MessageBoxButton.YesNo);

            switch (CautionMsg)
            {
                case MessageBoxResult.Yes:
                    BeginChecklist();
                    break;

                case MessageBoxResult.No:
                    break;
            }
        }

        // This is the main function. Aims to take a till from step 1 > finish.
        // I've saved the results as settings, so they can be referenced when printing the finalized PDF
        private void BeginChecklist()
        {
            CommandFrames.ChecklistWindfow checklist = new();
            checklist.Show();
        }

        private void TestButtonSideBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
#endregion