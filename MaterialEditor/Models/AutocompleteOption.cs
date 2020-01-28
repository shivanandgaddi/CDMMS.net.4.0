using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class AutocompleteOption
    {
        public AutocompleteOption()
        {
        }

        public AutocompleteOption(string value, string text)
        {
            Value = value;
            Label = text;
        }

        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }

        [JsonProperty("label")]
        public string Label
        {
            get;
            set;
        }
    }
}