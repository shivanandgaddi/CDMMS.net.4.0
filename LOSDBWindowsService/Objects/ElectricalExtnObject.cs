using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Objects
{
    public class ElectricalExtnObject
    {
        public string CleiCode { get; set; }
        public string ERInputVoltageFromUnit { get; set; }
        public string ERInputVoltageFromValue { get; set; }
        public string ERInputVoltageToUnit { get; set; }
        public string ERInputVoltageToValue { get; set; }
        public string ERInputVoltageFreqFromUnit { get; set; }
        public string ERInputVoltageFreqFromValue { get; set; }
        public string ERInputVoltageFreqToUnit { get; set; }
        public string ERInputVoltageFreqToValue { get; set; }
        public string ERInputCurrentFromUnit { get; set; }
        public string ERInputCurrentFromValue { get; set; }
        public string ERInputCurrentToUnit { get; set; }
        public string ERInputCurrentToValue { get; set; }
        public string ERFreeFormType { get; set; }
    }
}
