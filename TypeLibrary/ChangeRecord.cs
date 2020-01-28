using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class ChangeRecord
    {
        private string tableName = string.Empty;
        private string pniId = string.Empty;
        private string operationType = string.Empty;
        private long changeId = 0L;

        public ChangeRecord()
        {

        }

        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                tableName = value;
            }
        }

        public string PniId
        {
            get
            {
                return pniId;
            }
            set
            {
                pniId = value;
            }
        }

        public string OperationType
        {
            get
            {
                return operationType;
            }
            set
            {
                operationType = value;
            }
        }

        public long ChangeId
        {
            get
            {
                return changeId;
            }
            set
            {
                changeId = value;
            }
        }
    }
}
