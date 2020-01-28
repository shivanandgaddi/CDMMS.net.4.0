using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.Common.InformationBus
{
    public interface IBusObject
    {
        string BusServiceConfigurationKey
        {
            set;
        }

        string BusNetworkConfigurationKey
        {
            set;
        }

        string BusDaemonConfigurationKey
        {
            set;
        }

        string BusListenerSubjectConfigurationKey
        {
            set;
        }

        string BusTimeoutConfigurationKey
        {
            set;
        }

        string GetConfigurationValues();
    }
}
