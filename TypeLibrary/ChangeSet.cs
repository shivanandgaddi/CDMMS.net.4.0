using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class ChangeSet
    {
        private long changeSetId = 0;
        private long projectId = 0;
        private IList<ChangeRecord> changeRecords = null;

        public ChangeSet()
        {

        }

        public long ChangeSetId
        {
            get
            {
                return changeSetId;
            }
            set
            {
                changeSetId = value;
            }
        }

        public long ProjectId
        {
            get
            {
                return projectId;
            }
            set
            {
                projectId = value;
            }
        }

        public IList<ChangeRecord> ChangeRecords
        {
            get
            {
                return changeRecords;
            }
            set
            {
                changeRecords = value;
            }
        }
    }
}
