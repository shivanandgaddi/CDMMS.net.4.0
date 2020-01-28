using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class CadAttributes
    {
        [JsonProperty("CAD_ATTR_ID")]
        public string CAD_ATTR_ID { get; set; }

        [JsonProperty("CAD_ATTR_TYP")]
        public string CAD_ATTR_TYP { get; set; }

        [JsonProperty("TMPLT_ID")]
        public string TMPLT_ID { get; set; }

        [JsonProperty("TMPLT_DEF_ID")]
        public string TMPLT_DEF_ID { get; set; }

        [JsonProperty("JSON_DATA")]
        public string JSON_DATA { get; set; }

    }
}