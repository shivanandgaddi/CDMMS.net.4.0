using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class PortAssignment
    {
        [JsonProperty("defId")]
        public int DefId
        {
            get;
            set;
        }

        [JsonProperty("portSeqNo")]
        public int PortSeqNo
        {
            get;
            set;
        }

        [JsonProperty("portTypId")]
        public int PortTypId
        {
            get;
            set;
        }

        [JsonProperty("portTypNm")]
        public string PortTypNm
        {
            get;
            set;
        }

        [JsonProperty("portQty")]
        public int PortQty
        {
            get;
            set;
        }

        [JsonProperty("connectorTypId")]
        public int ConnectorTypId
        {
            get;
            set;
        }

        [JsonProperty("connectorTypNm")]
        public string ConnectorTypNm
        {
            get;
            set;
        }


        [JsonProperty("hasAssignedPort")]
        public string HasAssignedPort
        {
            get;
            set;
        }

        [JsonProperty("portName")]
        public string PortName
        {
            get;
            set;
        }

       
        [JsonProperty("portNoOffset")]
        public int PortNoOffset
        {
            get;
            set;
        }
    }
}