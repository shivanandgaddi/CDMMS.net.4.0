using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class FlowThruRecord
    {
        private string originatingFieldName = String.Empty;
        private string fieldDescription = String.Empty;
        private string flowThruIndicator = String.Empty;
        private string originatingSystem = String.Empty;
        private string lastUpdatedUserID = String.Empty;
        private DateTime lastUpdateDate;

        public string OriginatingFieldName
        {
            get
            {
                return originatingFieldName;
            }
            set
            {
                originatingFieldName = value;
            }
        }

        public string FieldDescription
        {
            get
            {
                return fieldDescription;
            }
            set
            {
                fieldDescription = value;
            }
        }

        public string FlowThruIndicator
        {
            get
            {
                return flowThruIndicator;
            }
            set
            {
                flowThruIndicator = value;
            }
        }

        public string OriginatingSystem
        {
            get
            {
                return originatingSystem;
            }
            set
            {
                originatingSystem = value;
            }
        }

        public string LastUpdatedUserID
        {
            get
            {
                return LastUpdatedUserID;
            }
            set
            {
                lastUpdatedUserID = value;
            }
        }

        public DateTime LastUpdateDate
        {
            get
            {
                return lastUpdateDate;
            }
            set
            {
                lastUpdateDate = value;
            }
        }
    }
}
