using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;
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

        private void SoftwareAndDrivers_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            SoftwareAndDrivers.Opacity = 0.5;
        }

        private void OptionalAddons_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            OptionalAddons.Opacity = 0.5;
        }

        private void SQLFiles_Click(object sender, RoutedEventArgs e)
        {
            // Mark Completion
            SQLFiles.Opacity = 0.5;
        }

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
            TheProgressTextBox.Text = "Opened SQL Ports [ 1317 & 1434 ]";
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
