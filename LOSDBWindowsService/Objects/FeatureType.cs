using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Objects
{
    public class FeatureType
    {
        public long FeatureTypeID { get; set; }
        public string FeatType { get; set; }
        public string FeatureTypeDescription { get; set; }
        public string RMEIndicator { get; set; }
        public string CableIndicator { get; set; }
        public string RecordsOnlyAllowIndicator { get; set; }
        public string OrderableAllowIndicator { get; set; }
        public string SymbologyIndicator { get; set; }
        public string DimensionHeightIndicator { get; set; }
        public string DimensionWidthIndicator { get; set; }
        public string DimensionDepthIndicator { get; set; }
        public string DimensionLengthIndicator { get; set; }
        public string CntndInAllowIndicator { get; set; }
        public string CDMMSRTTableName { get; set; }
        public string CDMMSRevisionTableName { get; set; }
        public string CDMMSAliasValTableName { get; set; }
        public string AliasVal { get; set; }
    }
}
