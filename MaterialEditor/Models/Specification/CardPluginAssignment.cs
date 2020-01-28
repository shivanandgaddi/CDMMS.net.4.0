using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    // this models is added for Card - Plugin Association screen. 
    public class CardPluginAssignment
    {
        [JsonProperty("defId")]
        public int DefId
        {
            get;
            set;
        }

        [JsonProperty("crdSpecId")]
        public int CardSpecId
        {
            get;
            set;
        }

        [JsonProperty("pluginSpecId")]
        public int PluginSpecId
        {
            get;
            set;
        }

        [JsonProperty("pluginSpecNm")]
        public string PluginSpecNm
        {
            get;
            set;
        }

        [JsonProperty("pluginRoleTy")]
        public string PluginRoleType
        {
            get;
            set;
        }

        [JsonProperty("pluginCntrTy")]
        public string PluginCntrType
        {
            get;
            set;
        }

        [JsonProperty("partNo")]
        public string PartNo
        {
            get;
            set;
        }

        [JsonProperty("cleiCd")]
        public string CleiCd
        {
            get;
            set;
        }


    }
}