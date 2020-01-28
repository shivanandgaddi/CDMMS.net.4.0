using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Utility;

namespace CenturyLink.Network.Engineering.Common.Polling
{
    public class PollingTimer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string pollingIntervalConfigurationKey = "pollingIntervalInMinutes";
        private int pollingIntervalConfigurationValue = 10;
        private DateTime lastPollStartTime;
        private DateTime lastPollEndTime;
        private TimeSpan timeToProcess;
        private TimeSpan pollingInterval;
        private APPLICATION_NAME application = APPLICATION_NAME.UNSET;

        public PollingTimer()
        {
            string applicationName = System.Configuration.ConfigurationManager.AppSettings["application"];

            Enum.TryParse<APPLICATION_NAME>(applicationName, out application);

            pollingIntervalConfigurationValue = GetPollingIntervalConfigurationValue();

            pollingInterval = new TimeSpan(0, pollingIntervalConfigurationValue, 0);

            logger.Info("Setting polling interval to " + pollingIntervalConfigurationValue + " minutes.");
            EventLogger.LogInfo("Setting polling interval to " + pollingIntervalConfigurationValue + " minutes.");
        }

        public void Start()
        {
            lastPollStartTime = DateTimeGenerator.Now;
        }

        public void WaitTillNextPoll()
        {
            lastPollEndTime = DateTimeGenerator.Now;
            timeToProcess = new TimeSpan(lastPollEndTime.Ticks - lastPollStartTime.Ticks);

            if (timeToProcess < pollingInterval)
                Thread.Sleep(new TimeSpan(pollingInterval.Ticks - timeToProcess.Ticks));

            CheckPollingInterval();
        }

        private void CheckPollingInterval()
        {
            int configurationValue = GetPollingIntervalConfigurationValue();

            if (configurationValue != pollingIntervalConfigurationValue)
            {
                pollingIntervalConfigurationValue = configurationValue;

                pollingInterval = new TimeSpan(0, pollingIntervalConfigurationValue, 0);

                logger.Info("Polling interval value has changed. Resetting polling interval to " + pollingIntervalConfigurationValue + " minutes.");
                EventLogger.LogInfo("Polling interval value has changed. Resetting polling interval to " + pollingIntervalConfigurationValue + " minutes.");
            }
        }

        private int GetPollingIntervalConfigurationValue()
        {
            int configurationValue = 0;

            try
            {
                configurationValue = int.Parse(ConfigurationManager.Value(application, pollingIntervalConfigurationKey));  // Get value from system_configuration table
            }
            catch (Exception ex)
            {
                configurationValue = 15;

                logger.Info(ex, "Unable to read polling interval configuration value from the database.");
            }

            return configurationValue;
        }
    }
}
