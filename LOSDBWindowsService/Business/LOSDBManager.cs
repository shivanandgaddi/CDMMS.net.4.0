using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Polling;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;
using CenturyLink.Network.Engineering.LOSDB.Service.DBInterface;
using CenturyLink.Network.Engineering.TypeLibrary;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Business
{
    class LOSDBManager : Poller
    {
        public override void ProcessPendingTransactions()
        {
            //throw new NotImplementedException();
            while (true)
            {
                ProcessLOSDB();
                System.Threading.Thread.Sleep(30000);
                //  MIKE TODO need to agree on something in the database to look for when an LOSDB run is completed
            }
        }

        private void ProcessLOSDB()
        {
            try
            {
                HandleFlowThru handleFlowThru = new HandleFlowThru();
                handleFlowThru.HandleFlowThruToNDS();

                // Send a notification to ??.  Include the old and new values.
            }
            catch (Exception ex)
            {
                EventLogger.LogException(ex, "Error in ProcessLOSDB");
            }
        }
    }
}
