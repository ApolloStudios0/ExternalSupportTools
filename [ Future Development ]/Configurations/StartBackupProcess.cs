using Microsoft.Win32.TaskScheduler;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ExternalSupportTools.__Future_Development__.Configurations
{
    class StartBackupProcess
    {
        public async void StartBatchExecution()
        {
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

            await File.WriteAllLinesAsync(@"C:\SQLBACKUP\BackupCore.bat", lines);

            // Save Full File Path For Scheduler
            Properties.Settings.Default.FullFilePathForSchedule = @"C:\SQLBACKUP\BackupCore.bat";
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            SetupScheduledTask();
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
    }
}
