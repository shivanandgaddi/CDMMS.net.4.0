using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using CenturyLink.Network.Engineering.Material.Editor.Utility;


namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class SlotAssignment
    {
        [JsonProperty("defId")]
        public string DefId
        {
            get;
            set;
        }

        [JsonProperty("slotSpecID")]
        public string SlotSpecID
        {
            get;
            set;
        }

        [JsonProperty("slotSpecNm")]
        public string SlotSpecNm
        {
            get;
            set;
        }

        [JsonProperty("seqNo")]
        public string SeqNo
        {
            get;
            set;
        }

        [JsonProperty("seqQty")]
        public string SeqQty
        {
            get;
            set;
        }
        
        [JsonProperty("Dpth")]
        public float Depth
        {
            get;
            set;
        }
        
        [JsonProperty("Hght")]
        public float Height
        {
            get;
            set;
        }
        
        [JsonProperty("Wdth")]
        public float Width
        {
            get;
            set;
        }
        
        [JsonProperty("DimUom")]
        public int DimensionsUnitOfMeasure
        {
            get;
            set;
        }

    }
}