using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public class AssemblyUnit
    {        
        public List<Dictionary<string, Attribute>> AttrLbrIdAssemblyUnitLst
        {
            get;
            set;
        }
        public AssemblyUnit()
        {
            
        }
        public List<Dictionary<string, Attribute>> AttributesMtrlClsfnLst
        {
            get;
            set;
        }
        public string LaborTitle
        { get; set; }
        public string LaborDesc
        { get; set; }        
       
    }
}