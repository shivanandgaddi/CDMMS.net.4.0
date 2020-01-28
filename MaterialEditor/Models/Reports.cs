using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class Reports
    {
        public Reports()
        {
            LbrIdAUList = new List<Dictionary<string, Attribute>>();
            LbrIdMtrlList = new List<Dictionary<string, Attribute>>();
        }
        public List<Dictionary<string, Attribute>> LbrIdAUList
        {
            get;
            set;
        }
        public List<Dictionary<string, Attribute>> LbrIdMtrlList
        {
            get;
            set;
        }
    }
}