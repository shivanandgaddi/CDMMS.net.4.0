using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class ReportsType
    {
        private Dictionary<string, DatabaseDefinition> lbrIdMtrl = null;
        public static string GET_LABOR_ID_REPORT = "get_labor_id_report";
        public Dictionary<string, DatabaseDefinition> LbrIdMtrldict
        {
            get
            {
                if (lbrIdMtrl == null)
                    GetLaborIdMtrlList();

                return lbrIdMtrl;
            }
        }

        private void GetLaborIdMtrlList()
        {
            lbrIdMtrl = new Dictionary<string, DatabaseDefinition>();
            lbrIdMtrl.Add("materialitemid", new DatabaseDefinition(GET_LABOR_ID_REPORT, "material_item_id"));
            lbrIdMtrl.Add("productid", new DatabaseDefinition(GET_LABOR_ID_REPORT, "product_id"));
            lbrIdMtrl.Add("mfgpartno", new DatabaseDefinition(GET_LABOR_ID_REPORT, "mfg_part_no"));
            lbrIdMtrl.Add("mfgid", new DatabaseDefinition(GET_LABOR_ID_REPORT, "mfg_id"));
            lbrIdMtrl.Add("ctlgdsc", new DatabaseDefinition(GET_LABOR_ID_REPORT, "ctlgDsc"));
        }
    }
}