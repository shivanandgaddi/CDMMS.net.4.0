using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class Option
    {
        public Option()
        {
        }

        public Option(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public Option(string value, string text, string defaultValue)
        {
            Value = value;
            Text = text;
            DefaultValue = defaultValue;
        }

        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }

        [JsonProperty("text")]
        public string Text
        {
            get;
            set;
        }

        [JsonProperty("dflt")]
        public string DefaultValue
        {
            get;
            set;
        }

        public static implicit operator Option(List<Option> v)
        {
            throw new NotImplementedException();
        }
    }
}