using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Objects
{
    public class EquipmentObject
    {
        public string VendorCode { get; set; }
        public string Drawing { get; set; }
        public string DrawingISS { get; set; }
        public string Description { get; set; }
        public string ProdID { get; set; }
        public string EquipmentCatalogItemID { get; set; }
        public string PartNumber { get; set; }
        public string OrderingCode { get; set; }
        public string LsOrSrs { get; set; }
        public string Stenciling { get; set; }
        public string CompatibleEquipmentCLEI7 { get; set; }
        public string AlternateOrderingCode { get; set; }
        public bool PassedStep3a { get; set; }
        public bool PassedStep3b { get; set; }
        public bool PassedStep3c { get; set; }
        public string MaterialID { get; set; }
        public string Passed { get; set; }
        public string CLEICode { get; set; }
        public string MaterialItemID { get; set; }
    }
}
