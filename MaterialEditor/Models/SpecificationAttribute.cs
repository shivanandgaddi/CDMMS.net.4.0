using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class SpecificationAttribute
    {
        public SpecificationAttribute()
        {
        }

        public SpecificationAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public SpecificationAttribute(string name, bool value)
        {
            Name = name;
            BoolValue = value;
        }

        public SpecificationAttribute(bool isEditable, string name)
        {
            Name = name;
            IsEditable = isEditable;
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

        [JsonProperty("bool")]
        public bool BoolValue
        {
            get;
            set;
        }

        [JsonProperty("options")]
        public List<Option> Options
        {
            get;
            set;
        }

        [JsonProperty("enable")]
        public bool IsEditable
        {
            get;
            set;
        }

        [JsonProperty("list")]
        public List<Dictionary<string, SpecificationAttribute>> ObjectList
        {
            get;
            set;
        }
    }
}