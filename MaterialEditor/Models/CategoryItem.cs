using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class CategoryItem
    {

       
        public CategoryItem()
        {
            Attributes = new Dictionary<string, Attribute>();
        }

        public Dictionary<string, Attribute> Attributes
        {
            get;
            set;
        }
    }
}