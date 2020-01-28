using System;
using NLog;

namespace CenturyLink.Network.Engineering.Common.Polling
{
    public abstract class Poller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private PollingTimer timer = null;
        private bool keepRunning = true;

        public Poller()
        {
            timer = new PollingTimer();
        }

        public void Start()
        {
            logger.Info("Polling started.");

            while (keepRunning)
            {
                try
                {
                    timer.Start();

                    ProcessPendingTransactions();

                    timer.WaitTillNextPoll();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Poller.Start(): " + ex.Message);

                    throw ex;
                }
            }
        }

        public void Stop()
        {
            keepRunning = false;
        }

        public abstract void ProcessPendingTransactions();
    }
}
