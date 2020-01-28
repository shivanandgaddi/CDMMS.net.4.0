using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class ModifiedPartType
    {
        public static string MP_MI_TABLE = "MATERIAL_ITEM";
        public static string MP_MIA_TABLE = "MATERIAL_ITEM_ATTRIBUTES";
        private Dictionary<string, DatabaseDefinition> modifiedparts = null;
        public Dictionary<string, DatabaseDefinition> ModifiedParts
        {
            get
            {
                if (modifiedparts == null)
                    GetModifiedPartsDefinition();

                return modifiedparts;
            }
        }
        private void GetModifiedPartsDefinition()
        {
            modifiedparts = new Dictionary<string, DatabaseDefinition>();
            modifiedparts.Add("material_item_id", new DatabaseDefinition(MP_MI_TABLE, "MATERIAL_ITEM_ID"));
            modifiedparts.Add("product_id", new DatabaseDefinition(MP_MI_TABLE, "PRODUCT_ID"));
            modifiedparts.Add("Specification_Name", new DatabaseDefinition(MP_MIA_TABLE, "SPECIFICATION_NAME"));
            modifiedparts.Add("Set_Length", new DatabaseDefinition(MP_MIA_TABLE, "SET_LENGTH"));
            modifiedparts.Add("Part_Number", new DatabaseDefinition(MP_MIA_TABLE, "PART_NUMBER"));

        }

    }
}