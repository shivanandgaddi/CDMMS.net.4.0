using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class Attribute
    {
        public Attribute()
        {
        }
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public Attribute(string name, List<string> liststrings)
        {
            Name = name;
            ListStrings = liststrings;
        }
        public Attribute(string name, Hashtable htparts)
        {
            Name = name;
            HashTableParts = htparts;
        }
        public Attribute(string name, bool value)
        {
            Name = name;
            BoolValue = value;
        }
        public Attribute(string name, string value, string source, bool bvalue)
        {
            Name = name;
            Value = value;
            Source = source;
            BoolValue = bvalue;
        }
        public Attribute(string name, string value, string source)
        {
            Name = name;
            Value = value;
            Source = source;
        }

        public Attribute(string name, bool value, string source)
        {
            Name = name;
            BoolValue = value;
            Source = source;
        }

        public Attribute(bool isEditable, string name)
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

        [JsonProperty("enable")]
        public bool IsEditable
        {
            get;
            set;
        }

        [JsonIgnore]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("iOro")]
        public string IspOrOsp
        {
            get;
            set;
        }

        [JsonProperty("maxLgth")]
        public string MaxLength
        {
            get;
            set;
        }

        [JsonProperty("src")]
        public string Source
        {
            get;
            set;
        }

        [JsonProperty("ovrrdId")]
        public string MaterialItemAttributesDefId
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

        [JsonProperty("liststrings")]
        public List<string> ListStrings
        {
            get;
            set;
        }

        [JsonProperty("htparts")]
        public Hashtable HashTableParts
        {
            get;
            set;
        }
    }
}