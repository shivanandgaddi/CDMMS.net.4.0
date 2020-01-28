using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class shelf
    {

        public int specn_id { get; set; }
        public int node_Id { get; set; }
        public string specn_shlvsdef_id { get; set; }
        public string shelf_nm { get; set; }
        public string seq_num { get; set; }
        public string shelf_qty { get; set; }
        public string shelf_no_offset { get; set; }
        public string extra_shelves { get; set; }

        public shelf()
        {
        }

        public shelf(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public shelf(string name, bool value)
        {
            Name = name;
            BoolValue = value;
        }

        public shelf(bool isEditable, string name)
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
        public List<Dictionary<string, shelf>> ObjectList
        {
            get;
            set;
        }
    }

    public class shelfAssignment
    {
        public List<shelf> shelfAssign { get; set; }
    }
}