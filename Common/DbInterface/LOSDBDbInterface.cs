using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using NLog;

namespace CenturyLink.Network.Engineering.Common.DbInterface
{
    public class LOSDBDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public LOSDBDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public int UpdateVendor(string tableName, string columnName, string newValue, string vendorCode)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = "update " + tableName + " set " + columnName + " = '" + newValue + "' where mfr_cd = '" + vendorCode + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateVendor - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateVendor({0}, {1})", tableName, columnName);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateEquipment(string tableName, string columnName, string newValue, string prodID)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = "update " + tableName + " set " + columnName + " = '" + newValue + "' where mtrl_id = " +
                            "(select mtrl_id from mtrl_alias_val where alias_val = '" + prodID + "')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateEquipment - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateEquipment({0}, {1})", tableName, columnName);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateEquipmentRevision(string tableName, string columnName, string newValue, string prodID)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = "";
            // MIKE TODO Question about updating revsn_no with drwg_iss which is the only possibility in this category

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateEquipmentRevision - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateEquipmentRevision({0}, {1})", tableName, columnName);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateInventory(string tableName, string columnName, string newValue, string prodID)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = "";
            // so far, there isn't anything to update is a CDMMS table

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateInventory - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateInventory({0}, {1})", tableName, columnName);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }

        public int UpdateInventoryRevision(string tableName, string columnName, string newValue, string equipmentCatalogItemID)
        {
            int returnValue = -1;
            List<string> revisionTableNames = new List<string>();
            IAccessor dbManager = null;

            if (columnName.ToUpper() == "CLEI_CD")
            {
                // get list of revision tables that have CLEI_CD as a column
                revisionTableNames = GetRevisionTableNames(columnName.ToUpper(), "%_MTRL_REVSN");

                foreach (string revisionTableName in revisionTableNames)
                {
                    // MIKE TODO Question about updating revsn_no
                    string sql = "update " + revisionTableName + " set " + columnName + "  = '" + newValue + "' where mtrl_id = " +
                                    "(select mtrl_id from mtrl_alias_val where alias_val = " +
                                    "(select prod_id from ies_invntry where eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "'))";

                    try
                    {
                        dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                        returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
                    }
                    catch (Exception ex)
                    {
                        string sErrMsg = "Error while updating UpdateInventoryRevision - " + ex.Message;
                        logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateInventoryRevision({0}, {1})", tableName, columnName);
                    }
                    finally
                    {
                        dbManager.Dispose();
                    }
                }
            }
            else if(columnName.ToUpper() == "LS_OR_SRS")
            {

            }

            return returnValue;
        }

        public int UpdateParentRevision(string tableName, string columnName, string newValue, string equipmentCatalogItemID, string mask)
        {
            // Note: this also works for non-revision tables as well.  so, both *_mtrl_revsn and *_mtrl

            int returnValue = -1;
            List<string> revisionTableNames = new List<string>();
            IAccessor dbManager = null;

            revisionTableNames = GetRevisionTableNames(columnName.ToUpper(), mask);

            foreach (string revisionTableName in revisionTableNames)
            {
                // MIKE TODO Question about updating revsn_no
                string sql = "update " + revisionTableName + " set " + columnName + "  = '" + newValue + "' where mtrl_id = " +
                                "(select mtrl_id from mtrl_alias_val where alias_val = " +
                                "(select prod_id from ies_invntry where eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "'))";

                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
                }
                catch (Exception ex)
                {
                    string sErrMsg = "Error while updating UpdateInventoryRevision - " + ex.Message;
                    logger.Log(LogLevel.Info, ex, "LOSDBDbInterface.UpdateInventoryRevision({0}, {1})", tableName, columnName);
                }
                finally
                {
                    dbManager.Dispose();
                }
            }

            return returnValue;
        }

        public List<string>GetRevisionTableNames (string columnName, string mask)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> revisionTableNames = new List<string>();

            string @sql = "select table_name from all_tab_columns where column_name = '" + columnName + "' and table_name like '" + mask + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    string tableName = reader["table_name"].ToString();
                    revisionTableNames.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetRevisionTableNames - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetRevisionTableNames({0}, {1})", "");
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (dbManager != null)
                    dbManager.Dispose();
            }

            return revisionTableNames;
        }
    }
}
