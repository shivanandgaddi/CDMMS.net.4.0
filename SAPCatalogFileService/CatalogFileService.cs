using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Catalog.File.Service.Business;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Polling;
using CenturyLink.Network.Engineering.Material.Management.Business;
using CenturyLink.Network.Engineering.Material.Management.Manager;

namespace CenturyLink.Network.Engineering.Catalog.File.Service
{
    public class CatalogFileService : ServiceBase
    {
        private System.ComponentModel.IContainer components = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string dbConnectionString = string.Empty;
        private static string nlogConnectionString = string.Empty;
        protected ThreadStart pollingDelegate = null;
        protected Thread pollingThread = null;
        protected Poller poller = null;

        public CatalogFileService()
        {
            InitializeComponent();
        }

        static void Main()
        {
            ServiceBase[] ServicesToRun;
            string runMode = System.Configuration.ConfigurationManager.AppSettings["runMode"];

            try
            {
                LogConnectionStrings();
                ValidateDatabaseConnectivity();
            }
            catch (Exception ex)
            {
                EventLogger.LogInfo("Exiting Main(), not starting the service.");
                string x = ex.Message;
                return;
            }
                if ("DEBUG".Equals(runMode))
                {
                    CatalogFileService service = new CatalogFileService();

                    service.StartPolling();
                }
                else
                {
                    ServicesToRun = new ServiceBase[]
                    {
                        new CatalogFileService()
                    };

                    Run(ServicesToRun);
                }
        }

        private static void LogConnectionStrings()
        {
            string maskedDbConnectionString = string.Empty;
            string maskedNlogConnectionString = string.Empty;

            dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
            nlogConnectionString = GetNLogConnectionString();

            if (!string.IsNullOrEmpty(nlogConnectionString))
            {
                maskedNlogConnectionString = GetMaskedConnectionString(nlogConnectionString);

                EventLogger.LogInfo("NLog connection string: " + maskedNlogConnectionString);
            }

            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                maskedDbConnectionString = GetMaskedConnectionString(dbConnectionString);

                EventLogger.LogInfo("Database connection string: " + maskedDbConnectionString);
            }
            else
            {
                Exception ex = new Exception("Database connection string not found in configuration file.");

                EventLogger.LogAlarm(ex, "Unable to set database connection string.", SentryIdentifier.EmailDev, SentrySeverity.Critical);

                throw ex;
            }
        }

        private static string GetMaskedConnectionString(string unmaskedString)
        {
            string maskedString = string.Empty;
            string mask = "xxx";
            string password = string.Empty;
            string oldString = string.Empty;
            string newString = string.Empty;
            int startIndex = 0;

            try
            {
                startIndex = unmaskedString.IndexOf("Password");

                oldString = unmaskedString.Substring(startIndex);
                oldString = oldString.Substring(0, oldString.IndexOf(";"));

                password = oldString.Split('=')[1];

                newString = oldString.Replace(password, mask);

                maskedString = unmaskedString.Replace(oldString, newString);
            }
            catch (Exception ex)
            {
                EventLogger.LogException(ex, "Exception caught attempting to mask string: " + unmaskedString);
            }

            return maskedString;
        }

        private static void ValidateDatabaseConnectivity()
        {
            string sql = "SELECT 'SUCCESS' AS success FROM dual";
            string success = string.Empty;
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, dbConnectionString);

                success = dbManager.ExecuteScalar(System.Data.CommandType.Text, sql).ToString();

                logger.Info("ValidateDatabaseConnectivity(): {0}", success);
                EventLogger.LogInfo(string.Format("ValidateDatabaseConnectivity(): {0}", success));
            }
            catch (Exception ex)
            {
                EventLogger.LogAlarm(ex, "ValidateDatabaseConnectivity() failed.", SentryIdentifier.EmailDev, SentrySeverity.Critical);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        private static string GetNLogConnectionString()
        {
            string connectionString = string.Empty;
            LoggingConfiguration nlog = LogManager.Configuration;
            DatabaseTarget target = null;

            try
            {
                if (nlog != null)
                {
                    target = (DatabaseTarget)nlog.FindTargetByName("DbLogger");

                    if (target != null)
                    {
                        connectionString = target.ConnectionString.ToString();
                    }
                }

                if (string.IsNullOrEmpty(connectionString))
                    EventLogger.LogInfo("Unable to configure NLog. May not be able to perform database logging.");
            }
            catch (Exception ex)
            {
                EventLogger.LogException(ex, "Unable to configure NLog. May not be able to perform database logging.");
            }

            return connectionString;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                pollingDelegate = new ThreadStart(StartPolling);
                pollingThread = new Thread(pollingDelegate);

                pollingThread.Name = ServiceName + "Poller";

                logger.Info(ServiceName + ": Starting polling thread.");
                EventLogger.LogInfo(ServiceName + ": Starting polling thread.");

                pollingThread.Start();

                logger.Info(ServiceName + ": Polling thread started.");
                logger.Info(ServiceName + " is started.");
                EventLogger.LogInfo(ServiceName + ": Polling thread started.");
                EventLogger.LogInfo(ServiceName + " is started.");

                //waitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Unable to start polling thread.");
                EventLogger.LogAlarm(ex, "Unable to start polling thread.", SentryIdentifier.EmailDev, SentrySeverity.Critical);
            }
        }

        protected override void OnStop()
        {
            logger.Info(ServiceName + " is stopping....");
            EventLogger.LogAlarm(new Exception(ServiceName + ".OnStop()"), ServiceName + " is stopping....", SentryIdentifier.EmailDev, SentrySeverity.Major);

            stop();

            logger.Info("IsAlive: {0}; ThreadState: {1}", pollingThread.IsAlive, pollingThread.ThreadState.ToString());

            if (pollingThread.IsAlive)
            {
                logger.Info("Abort!");

                if (pollingThread.Join(5000))
                {
                    logger.Info("Join() was successful");

                    pollingThread.Abort();

                    logger.Info("Abort() was successful");
                }
                else
                    logger.Info("Join() was unsuccessful");
            }

            logger.Info(ServiceName + " is stopped.");
            EventLogger.LogAlarm(new Exception(ServiceName + ".OnStop()"), ServiceName + " is stopped.", SentryIdentifier.EmailDev, SentrySeverity.Major);
        }

        protected void StartPolling()
        {
            try
            {
                poller = new FileManager();

                poller.Start();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "CatalogFileService.StartPolling() - Initiating service Stop()...");
                EventLogger.LogAlarm(ex, "CatalogFileService.StartPolling() - Initiating service Stop()...", SentryIdentifier.EmailDev, SentrySeverity.Major);

                Stop();
            }
        }

        private void stop()
        {
            try
            {
                if (poller != null)
                    poller.Stop();
            }
            catch (Exception ex)
            {
                logger.Info(ex, "{0}: Exception occurred in service stop method.", ServiceName);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.ServiceName = "SAPCatalogFileService";
        }

        #endregion
    }
}
