using System;
using System.IO;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Polling;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Management.Manager;

namespace CenturyLink.Network.Engineering.Catalog.File.Service.Business
{
    public class FileManager : Poller
    {
        private const string remoteFilePatternKey = "remoteFilePattern";
        private const string localFilePathKey = "localFilePath";
        private const string processedFilesPathKey = "processedFilesPath";
        private string remoteFilePattern = string.Empty;
        private string localFilePath = string.Empty;
        private string processedFilesPath = string.Empty;
        private int sessionTimeoutInSeconds = 0;

        public FileManager()
        {

        }

        public override void ProcessPendingTransactions()
        {
            remoteFilePattern = ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, remoteFilePatternKey);
            localFilePath = ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, localFilePathKey);
            processedFilesPath = ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, processedFilesPathKey);

            ProcessFiles();
        }

        private void ProcessFiles()
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(localFilePath);

                if (folder.Exists)
                {
                    // Retrieve filenames
                    FileInfo[] files = folder.GetFiles(remoteFilePattern);

                    if (files != null && files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            // Before backup, read the XML and process it.
                            string readText = "";
                            readText = System.IO.File.ReadAllText(localFilePath + "\\" + files[i].Name);

                            MaterialManager materialManager = new MaterialManager();

                            // Call the existing ProcessMessage method
                            materialManager.ProcessMessage(readText);

                            // Backup files - delete files in the original directory by moving them here
                            files[i].MoveTo(processedFilesPath + "\\" + files[i].Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogException(ex, "Error processing incoming files");
            }
        }
    }
}
