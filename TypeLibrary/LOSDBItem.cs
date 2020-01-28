using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class LOSDBItem
    {
        public string OrderingCode { get; set; }
        public string LsOrSrs { get; set; }
        public string PartNumber { get; set; }
        public string Vendorcode { get; set; }
        public string ProdID { get; set; }
        public string Drawing { get; set; }
        public string DrawingISS { get; set; }
        public string EquipmentCatalogItemID { get; set; }
    }
}
