using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class TemplateAttribute
    {
        private TemplateAttribute()
        {

        }

        public TemplateAttribute(string name)
        {
            Name = name;
        }

        public TemplateAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [JsonIgnore]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }

        [JsonProperty("optionVals")]
        public List<Option> Options
        {
            get;
            set;
        }
    }
}