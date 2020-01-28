using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Objects
{
    class AuditObject
    {
        public long AuditDaID { get; set; }
        public long AuditIesColDefID { get; set; }
        public long AuditIesAliasDefID { get; set; }
        public string AuditTablePkColumnName { get; set; }
        public string AuditTablePkColumnValue { get; set; }
        public string AuditParentTablePKColumnName { get; set; }
        public string AuditParentTablePkColumnValue { get; set; }
        public string ActionCode { get; set; }
        public string OldColumnValue { get; set; }
        public string NewColumnValue { get; set; }
        public string AuditTableDefID { get; set; }
        public string AuditColumnName { get; set; }
        public string FlowThruIndicator { get; set; }
        public string CDMMSTableName { get; set; }
        public string CDMMSColumnName { get; set; }
        public string NDSRequiredIndicator { get; set; }
        public string AuditTableName { get; set; }
        public string AllRecordsIndicator { get; set; }
        public string AliasTableName { get; set; }
        public string AliasValueTableName { get; set; }
        public string SAPMaterialCode { set; get; }
        public string LOSDBProdID { set; get; }
    }
}
