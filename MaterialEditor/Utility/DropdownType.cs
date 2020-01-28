using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class DropdownType
    {
        //public static string NUP_MCS_TABLE = "material_catalog_staging";
        //public static string NUP_MCSH_TABLE = "material_catalog_staging_hist";
        //public static string NUP_MIS_TABLE = "mtl_item_sap";
        public static string DD_MCS_TABLE = "material_type_cd";
        public static string DD_MCSH_TABLE = "material_flow_thru";



        //private Dictionary<string, DatabaseDefinition> newupdatedparts = null;
        //private Dictionary<string, DatabaseDefinition> stagingParts = null;
        //private Dictionary<string, DatabaseDefinition> updatedStagingParts = null;

        private Dictionary<string, DatabaseDefinition> typeCD = null;
        private Dictionary<string, DatabaseDefinition> flowThru = null;

        public Dictionary<string, DatabaseDefinition> TypeCD
        {
            get
            {
                if (typeCD == null)
                    GettypeCDsDefinition();

                return typeCD;
            }
        }
        public Dictionary<string, DatabaseDefinition> FlowThru
        {
            get
            {
                if (flowThru == null)
                    GetflowThruDefinition();

                return flowThru;
            }
        }

        private void GetflowThruDefinition()
        {
            flowThru = new Dictionary<string, DatabaseDefinition>();
            flowThru.Add("flowthruid", new DatabaseDefinition(DD_MCSH_TABLE, "FLOW_THRU_ID"));
            flowThru.Add("originatingfieldname", new DatabaseDefinition(DD_MCSH_TABLE, "ORIGINATING_FIELD_NM"));
            flowThru.Add("fielddescription", new DatabaseDefinition(DD_MCSH_TABLE, "FIELD_DESCRIPTION"));
            flowThru.Add("flowthruind", new DatabaseDefinition(DD_MCSH_TABLE, "FLOW_THRU_IND"));
            flowThru.Add("originatingsystem", new DatabaseDefinition(DD_MCSH_TABLE, "ORIGINATING_SYSTEM"));
            flowThru.Add("lastupdateduserid", new DatabaseDefinition(DD_MCSH_TABLE, "LAST_UPDTD_USERID"));
            flowThru.Add("lastupdated", new DatabaseDefinition(DD_MCSH_TABLE, "LAST_UPDTD_TMSTMP"));


        }
        private void GettypeCDsDefinition()
        {
            typeCD = new Dictionary<string, DatabaseDefinition>();
            typeCD.Add("materialid", new DatabaseDefinition(DD_MCS_TABLE, "MTL_TYPE_CD"));
            typeCD.Add("appname", new DatabaseDefinition(DD_MCS_TABLE, "APP_NAME"));
            typeCD.Add("effectivedate", new DatabaseDefinition(DD_MCS_TABLE, "EFF_DT"));
            typeCD.Add("enddate", new DatabaseDefinition(DD_MCS_TABLE, "END_DT"));
            typeCD.Add("lastupdateduserid", new DatabaseDefinition(DD_MCS_TABLE, "LAST_UPDT_USERID"));
            typeCD.Add("lastupdated", new DatabaseDefinition(DD_MCS_TABLE, "LAST_UPDT"));
            typeCD.Add("approvedstatus", new DatabaseDefinition(DD_MCS_TABLE, "approved_status"));

        }
    }

}