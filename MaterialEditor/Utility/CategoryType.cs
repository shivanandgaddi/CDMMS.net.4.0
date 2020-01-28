using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class CategoryType
    {
        private static string SAP_TABLE = "CATEGORY";
         

        private Dictionary<string, DatabaseDefinition> sapCategory = null;
       
        public Dictionary<string, DatabaseDefinition> SAPCategory
        {
            get
            {
                if (sapCategory == null)
                    GetSAPCategoryDefinition();

                return sapCategory;
            }
        }

        private void GetSAPCategoryDefinition()
        {
           
                sapCategory = new Dictionary<string, DatabaseDefinition>();

                sapCategory.Add("Appnm", new DatabaseDefinition(SAP_TABLE, "APP_NM", true));
                sapCategory.Add("mtlgrpid", new DatabaseDefinition(SAP_TABLE, "MTL_GRP_ID"));
                sapCategory.Add("lastupdtid", new DatabaseDefinition(SAP_TABLE, "LAST_UPDTD_USR_ID"));
                sapCategory.Add("update", new DatabaseDefinition(SAP_TABLE, "LAST_UPDTD_TMSTMP"));
                sapCategory.Add("effsrtdt", new DatabaseDefinition(SAP_TABLE, "EFFECTIVE_START_DATE"));
                sapCategory.Add("effenddt", new DatabaseDefinition(SAP_TABLE, "EFFECTIVE_END_DATE"));
                sapCategory.Add("approvedstatus", new DatabaseDefinition(SAP_TABLE, "approved_status"));
            
           
        }

      
    }

    public class DatabaseDefinition1
    {
        private DatabaseDefinition1()
        {
        }

        public DatabaseDefinition1(string table, string column)
        {
            Table = table;
            Column = column;
            IsNumber = false;
        }

        public DatabaseDefinition1(string table, string column, bool isNumber)
        {
            Table = table;
            Column = column;
            IsNumber = isNumber;
        }

        public string Table
        {
            get;
            set;
        }

        public string Column
        {
            get;
            set;
        }

        public bool IsNumber
        {
            get;
            set;
        }
    }
}