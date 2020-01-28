using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.ShelfModel
{
    public class AssignmentDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        protected IAccessor dbAccessor = null;
        shelfAssignment shelf = new shelfAssignment();
        bool updateStatus = true;
        public AssignmentDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public AssignmentDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public string DbConnectionString
        {
            get
            {
                if (connectionString == null)
                    connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];

                return connectionString;
            }
        }

        public void StartTransaction()
        {
            if (dbAccessor == null)
                dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            dbAccessor.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.RollbackTransaction();
        }

        public void Dispose()
        {
            if (dbAccessor != null)
                dbAccessor.Dispose();
        }

        public object CheckNullValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return DBNull.Value;
            else
                return val.ToUpper();
        }

        public object CheckNullValue(string val, bool skipToUpper)
        {
            if (string.IsNullOrEmpty(val))
                return DBNull.Value;
            else
            {
                if (skipToUpper)
                    return val;
                else
                    return val.ToUpper();
            }
        }

        public object CheckNullValue(int val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }

        public object CheckNullValue(long val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }

        public object CheckNullValue(decimal val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }


        public async Task<List<Dictionary<string, string>>> SearchSpecificationsAsync(string nodeId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, string>> shelfList = null;
            bool hadException = false;
            SpecificationType specType = new SpecificationType();

            //await Task.Run(() =>
            //{
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int32, nodeId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("specn_assign_pkg.get_shelfAssignment", parameters);

                while (reader.Read())
                {
                    if (shelfList == null)
                        shelfList = new List<Dictionary<string, string>>();

                    Dictionary<string, string> spec = new Dictionary<string, string>();

                    spec.Add("specn_shlvsdef_id", reader["NODE_SPECN_WITH_SHLVS_DEF_ID"].ToString());
                    spec.Add("node_Id", reader["NODE_SPECN_ID"].ToString());
                    spec.Add("specn_id", reader["SHELF_SPECN_ID"].ToString());
                    spec.Add("shelf_nm", DataReaderHelper.GetNonNullValue(reader, "SHELF_SPECN_NM"));
                    spec.Add("seq_num", DataReaderHelper.GetNonNullValue(reader, "SHELF_SEQ_NO"));
                    spec.Add("shelf_qty", reader["SHELF_QTY"].ToString());
                    spec.Add("shelf_no_offset", reader["SHELF_NO_OFST_VAL"].ToString());
                    spec.Add("extra_shelves", reader["EXTR_SHELF_ALLOW_IND"].ToString());

                    shelfList.Add(spec);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to perform search: {0}";

                hadException = true;

                logger.Error(oe, message, nodeId);
                EventLogger.LogAlarm(oe, string.Format(message, nodeId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to perform search");
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
            //});

            if (hadException)
                throw new Exception();

            return shelfList;
        }

        public async Task<string> InsertAssignment(string actionCode, string shelfStnumber, List<shelf> list)
        {
            //string jsonData = "";
            String errormsg = "";
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);


                    dbManager.BeginTransaction();
                    foreach (var shelflist in list)
                    {
                        if (actionCode == "update" && updateStatus)
                        {
                            updateStartNumber(shelfStnumber, shelflist.node_Id.ToString());
                            updateStatus = false;
                        }

                        parameters = dbManager.GetParameterArray(8);
                        parameters[0] = dbManager.GetParameter("actionCode", DbType.String, actionCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("shlvsdefId", DbType.Int32, shelflist.specn_shlvsdef_id.Contains("Item") ? "0" : shelflist.specn_shlvsdef_id, ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("nodeId", DbType.Int32, shelflist.node_Id, ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("specId", DbType.Int32, shelflist.specn_id, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("seqNum", DbType.Int32, shelflist.seq_num, ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("shelfQty", DbType.Int32, shelflist.shelf_qty, ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("shelfOffset", DbType.Int32, shelflist.shelf_no_offset, ParameterDirection.Input);
                        if (shelflist.extra_shelves.Length > 1)
                        {
                            parameters[7] = dbManager.GetParameter("extraShlvs", DbType.String, shelflist.extra_shelves.ToLower() == "true" ? "Y" : "N", ParameterDirection.Input);
                        }
                        else
                        {
                            parameters[7] = dbManager.GetParameter("extraShlvs", DbType.String, shelflist.extra_shelves == "Y" ? "Y" : "N", ParameterDirection.Input);
                        }
                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "specn_assign_pkg.insertupdate_shelfAssignment", parameters);
                    }

                    errormsg = (actionCode == "insert") ? "Insert Successfull" : (actionCode == "delete") ? "Delete Successfull" : (actionCode == "update") ? "Update Successfull" : "";
                    dbManager.CommitTransaction();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}";
                    errormsg = Convert.ToString(oe);
                    logger.Error(oe, message);
                    dbManager.RollbackTransaction();
                    //EventLogger.LogAlarm(oe, string.Format(message), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //  hadException = true;
                    errormsg = Convert.ToString(ex);
                    logger.Error(ex, "Unable to perform search");
                    dbManager.RollbackTransaction();
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return errormsg;

        }

        private bool updateStartNumber(string StartNumber, string NodeID)
        {

            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("startNum", DbType.Int32, StartNumber, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("nodeId", DbType.Int32, NodeID, ParameterDirection.Input);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "specn_assign_pkg.update_shelfstartNum", parameters);

                return true;
            }
            catch (OracleException oe)
            {
                string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}";


                logger.Error(oe, message);
                //EventLogger.LogAlarm(oe, string.Format(message), SentryIdentifier.EmailDev, SentrySeverity.Major);
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to perform search");
                return false;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

        }

        public async Task<string> GetnodeNamefromdb(int nodeId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string nodeName = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("nodeId", DbType.String, nodeId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("nodeName", DbType.String, nodeName, ParameterDirection.Output, 20);

                    reader = dbManager.ExecuteDataReaderSP("specn_assign_pkg.get_nodeName", parameters);

                    nodeName = parameters[1].Value.ToString();

                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Card-Plugin Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, nodeId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Card-Plugin Assignment details for Card ID: {0}", nodeId);
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
            });

            if (hadException)
                throw new Exception();

            return nodeName;
        }

        public async Task<string> GetcardNamefromdb(int cardId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            string cardName = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("cardId", DbType.String, cardId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("cardName", DbType.String, cardName, ParameterDirection.Output, 250);

                    reader = dbManager.ExecuteDataReaderSP("specn_assign_pkg.get_cardName", parameters);

                    cardName = parameters[1].Value.ToString();

                }
                catch (OracleException oe)
                {
                    string message = "Unable to get Card-Plugin Assignment details for Card ID : {0}";

                    hadException = true;

                    logger.Error(oe, message, cardId);
                    //EventLogger.LogAlarm(oe, string.Format(message, Id, specificationType), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get Card-Plugin Assignment details for Card ID: {0}", cardId);
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
            });

            if (hadException)
                throw new Exception();

            return cardName;
        }

    }
}