using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalSupportTools.__Future_Development__.Configurations
{
    class Utility
    {
        /// <summary>
        /// Stores Default Connection String In The Format 'Machine Name\SQLExpress' 
        /// </summary>
        public static string DefaultConnectionString { get; set; } = Environment.MachineName + @"\SQLEXPRESS";
        public static string SQLString { get; set; } = $"Server={DefaultConnectionString};";

        /// <summary>
        /// Asynchronously Executes A Query & Returns The Result In A Datatable (string QueryToExecute)
        /// </summary>
        public static Task<DataTable> GetSQLData(string QueryToExecute)
        {
            return Task.Run(() => {

                // Instances
                DataTable dt = new DataTable();
                SqlConnection sqlConnection = new SqlConnection();

                // Run Query & Return As DataTable
                try { sqlConnection.ConnectionString = DefaultConnectionString; } catch { }

                using (SqlDataAdapter da = new SqlDataAdapter(QueryToExecute, sqlConnection.ConnectionString))
                {
                    try { da.Fill(dt); } catch { }
                }

                return dt;
            });
        }

        public static Task InstallMSIFile(string LocalFileName)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Install MSI Package
                    Process installerProcess = new();
                    ProcessStartInfo processInfo = new();

                    // Run & Install
                    processInfo.Arguments = @$"/i {LocalFileName} /q";
                    processInfo.FileName = "msiexec";
                    installerProcess.StartInfo = processInfo;

                    // Start Installer
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
                catch { }
            });
        }

        public static Task ShowNotification(string Title, string Body)
        {
            return Task.Run(() =>
            {
                try
                {
                    var notificationManager = new NotificationManager();

                    notificationManager.Show(new NotificationContent
                    {
                        Title = Title,
                        Message = Body,
                        Type = (NotificationType)NotificationType.Information
                    });
                }
                catch { }
            });
        }

        public static Task DownloadThisFile(string FileDownloadSource, string LocalFileName)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(LocalFileName))
                    {
                        Thread thread = new(() =>
                        {
                            using (var client = new WebClient())
                            {
                                string NiniteDownloadURL = FileDownloadSource;

                                // Browser Request
                                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");

                                // Download To Home Directory
                                client.DownloadFileAsync(new Uri(FileDownloadSource), LocalFileName);

                                // Notify Completion
                                Utility.ShowNotification("File Downloading", "Downloading " + LocalFileName);
                            }
                        });
                        thread.Start();
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Asynchronously Executes A NON-Query & Returns No Result (string QueryToExecute)
        /// </summary>
        public static Task ExecuteThisQuery(string QueryToExecute)
        {
            return Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(SQLString))
                {
                    SqlCommand command = new SqlCommand(QueryToExecute, connection);
                    command.Connection.Open(); command.ExecuteNonQuery(); command.Connection.Close();
                }
            });
        }

        /// <summary>
        /// Asynchronously Executes A Query & Returns The Result As Scalar Obj (string QueryToExecute)
        /// </summary>
        public static string ExecuteSQLScalar(string QueryToExecute)
        {
            using (SqlConnection connection = new SqlConnection(SQLString))
            {
                SqlCommand tempcommand = new SqlCommand(QueryToExecute, connection);
                string result = "";
                try
                {
                    if (tempcommand.Connection.State != ConnectionState.Open)
                    {
                        tempcommand.Connection.Open();
                    }
                    tempcommand.CommandTimeout = 0;
                    try
                    {
                        result = Convert.ToString(tempcommand.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                    }
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                    tempcommand.Dispose();
                }
                catch (Exception ex2)
                {
                    result = "";
                }
                return result;
            }
        }
    }
}
