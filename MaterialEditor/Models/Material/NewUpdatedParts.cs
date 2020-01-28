using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public class NewUpdatedParts
    {
        public NewUpdatedParts()
        {
            Attributes = new Dictionary<string, Attribute>();
        }
        public NewUpdatedParts(string source)
        {
            Models.Attribute attr = new Models.Attribute("source", source, "unset");
            Attributes = new Dictionary<string, Attribute>();

            Attributes.Add(attr.Name, attr);
        }

        public NewUpdatedParts(int materialItemId)
        {
            Models.Attribute attr = new Models.Attribute("id", materialItemId.ToString(), "unset");
            Attributes = new Dictionary<string, Attribute>();

            Attributes.Add(attr.Name, attr);
        }
        public Dictionary<string, Attribute> Attributes
        {
            get;
            set;
        }
    }
}