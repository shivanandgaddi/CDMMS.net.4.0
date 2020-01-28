using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class SpecificationAlias
    {

        [JsonProperty("aliasName")]
        public string AliasName
        {
                get;
                set;
        }

        [JsonProperty("aliasValue")]
        public string AliasValue
        {
                get;
                set;
         }

        [JsonProperty("nteVal")]
        public string NTEValue
        {
            get;
            set;
        }

        [JsonProperty("appName")]
        public string AppName
        {
            get;
            set;
        }

    }
}