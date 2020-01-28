using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public class ModifiedPart
    {
        public ModifiedPart()
        {
           
            Attributes = new Dictionary<string, string>();
        
    }
        public ModifiedPart(string long_name, string id,string svalue)
        {
            Name = long_name;
            SValue = svalue;
            ID= id;
        }
        public ModifiedPart(string long_name, string svalue)
        {
            Name = long_name;
            SValue = svalue;
        }
        public string Name
        {
            get;
            set;
        }
        public string SValue
        {
            get;
            set;
        }
        public string ID
        {
            get;
            set;
        }
        public Dictionary<string, string> Attributes
        {
            get;
            set;
        }
        public List<Option> Options
        {
            get;
            set;
        }
    }
}