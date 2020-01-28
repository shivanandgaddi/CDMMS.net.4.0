using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Objects
{
    public class LOSDBElectricalExtn
    {
        public string CleiCode { get; set; }
        public string ERInputVoltageFromUnit { get; set; }
        public float ERInputVoltageFromValue { get; set; }
        public string ERInputVoltageToUnit { get; set; }
        public float ERInputVoltageToValue { get; set; }
        public string ERInputVoltageFreqFromUnit { get; set; }
        public float ERInputVoltageFreqFromValue { get; set; }
        public string ERInputVoltageFreqToUnit { get; set; }
        public float ERInputVoltageFreqToValue { get; set; }
        public string ERInputCurrentFromUnit  { get; set; }
        public float ERInputCurrentFromValue { get; set; }
        public string ERInputCurrentToUnit { get; set; }
        public float ERInputCurrentToValue { get; set; }
        public string ERFreeFormType { get; set; }
        public DateTime DateInserted { get; set; }
    }
}
