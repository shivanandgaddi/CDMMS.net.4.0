using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class SearchResult
    {
        [JsonProperty("itemValue")]
        public string ItemValue
        {
            get;
            set;
        }

        [JsonProperty("displayValue")]
        public string DisplayValue
        {
            get;
            set;
        }
    }
}