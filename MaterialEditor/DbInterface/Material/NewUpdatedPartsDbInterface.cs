using System;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using NLog;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.TypeLibrary;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{

    public class NewUpdatedPartsDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public NewUpdatedPartsDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public NewUpdatedPartsDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public string GetMTLID_PRODID(string aliasTableName, string aliasValTableName, string aliasPKColumnName, string auditTablePKColumnValue)
        {
            string returnString = "";
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select distinct a.alias_val, a.mtrl_id from mtrl_alias_val a, mtrl b " +
                " where a.mtrl_id = (select distinct c.mtrl_id from " + aliasTableName + " c, " + aliasValTableName + " d " +
                " where c." + aliasPKColumnName + " = d." + aliasPKColumnName + " and d.alias_val = '" + auditTablePKColumnValue + "')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnString = reader["alias_val"].ToString() + "," + reader["mtrl_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return returnString;
        }
        public async Task<List<NewUpdatedParts>> SearchNewUpdatedPartsAsync(string materialCode, string partno, string heci,
            string descr, string isnew, string recordtype, int minsearch, int maxsearch, string frmdate = "", string todate = "", 
            string viewrejected = "", string rejectedcuid = "")
        {
            /*DBcall : to search the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IDataReader reader1 = null;
            List<NewUpdatedParts> newUpdatedParts = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            int i = 1;

            if (todate == "")
                todate = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");

            if (frmdate == "")
            {
                if (materialCode == "" && partno == "" && descr == "")
                    frmdate = DateTime.Now.AddMonths(-6).ToString("MM/dd/yyyy");
                else
                    frmdate = DateTime.Now.AddYears(-7).ToString("MM/dd/yyyy");
            }

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(11);
                    parameters[0] = dbManager.GetParameter("pmate", DbType.String, materialCode.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("ppart", DbType.String, partno.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pheci", DbType.String, heci.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pdesc", DbType.String, descr.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pfrmd", DbType.String, frmdate.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("ptoda", DbType.String, todate.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("preco", DbType.String, recordtype.ToUpper(), ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pisne", DbType.String, isnew.ToUpper(), ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameter("pvrej", DbType.String, viewrejected.ToUpper(), ParameterDirection.Input);
                    parameters[9] = dbManager.GetParameter("pcuid", DbType.String, rejectedcuid.ToUpper(), ParameterDirection.Input);
                    parameters[10] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_NEWUPDATEDPARTS", parameters);

                    while (reader.Read())
                    {
                        if (i >= minsearch && i <= maxsearch)
                        {
                            if (newUpdatedParts == null)
                                newUpdatedParts = new List<NewUpdatedParts>();

                            NewUpdatedParts newUpdatedPart = new NewUpdatedParts();
                            foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.NewUpdatedPartslosdb)
                            {
                                newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_AUDIT_IES_TABLE));
                            }

                            foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.NewUpdatedParts)
                            {
                                newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE));
                            }

                            int matchCount = GetPossibleMatchCount(reader["mtl_cd"].ToString());
                            if (matchCount > 0)
                            {
                                newUpdatedPart.Attributes.Remove("has_losdb");
                                newUpdatedPart.Attributes.Add("has_losdb", new Models.Attribute("has_losdb", "Y", NewUpdatedPartsType.NUP_MCS_TABLE));
                            }

                            matchCount = GetSpecInitCount(reader["mfg_part_no"].ToString().ToUpper(), reader["manufacturer"].ToString());
                            if (matchCount > 0)
                            {
                                newUpdatedPart.Attributes.Remove("has_specn_init");
                                newUpdatedPart.Attributes.Add("has_specn_init", new Models.Attribute("has_specn_init", "Y", NewUpdatedPartsType.NUP_MCS_TABLE));
                            }

                            newUpdatedPart.Attributes.Remove("check_me");
                            newUpdatedPart.Attributes.Add("check_me", new Models.Attribute("check_me", "N", NewUpdatedPartsType.NUP_MCS_TABLE));

                            newUpdatedParts.Add(newUpdatedPart);
                        }
                        i++;
                    }
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    reader = null;
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_NEWUPDATEDPARTS_LOSDB", parameters);

                    while (reader.Read())
                    {
                        string alias_tbl_nm = null;
                        string audit_tblpk_val = null;
                        string alias_pkconm = null;
                        string alias_valtblnm = null;
                        string tableval = null;
                        if ((reader["ALIAS_TBL_NM"] != DBNull.Value) && (reader["ALIAS_VAL_TBL_NM"] != DBNull.Value))
                        {
                            alias_tbl_nm = (string)reader["ALIAS_TBL_NM"];
                            alias_valtblnm = (string)reader["ALIAS_VAL_TBL_NM"];

                            if (reader["AUDIT_TBL_PK_COL_VAL"] != DBNull.Value)
                            {
                                audit_tblpk_val = (string)reader["AUDIT_TBL_PK_COL_VAL"];
                            }
                            if (reader["ALIAS_PK_COL_NM"] != DBNull.Value)
                            {
                                alias_pkconm = (string)reader["ALIAS_PK_COL_NM"];
                            }
                            tableval = GetMTLID_PRODID(alias_tbl_nm, alias_valtblnm, alias_pkconm, audit_tblpk_val);
                        }
                        if (tableval != "" && tableval != null)
                        {
                            var res = tableval.Split(',');
                            string mtlid = res[1];
                            string losdbprodid = res[0];
                            parameters = dbManager.GetParameterArray(2);
                            parameters[0] = dbManager.GetParameter("pmate", DbType.String, mtlid.ToUpper(), ParameterDirection.Input);
                            parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                            reader1 = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_NEWUPDATEDPARTS_LOSDB", parameters);

                            while (reader1.Read())
                            {
                                if (newUpdatedParts == null)
                                    newUpdatedParts = new List<NewUpdatedParts>();

                                NewUpdatedParts newUpdatedPart = new NewUpdatedParts();

                                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.NewUpdatedParts)
                                {
                                    newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader1, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE));

                                }
                                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.NewUpdatedPartslosdb)
                                {
                                    newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_AUDIT_IES_TABLE));

                                }
                                newUpdatedPart.Attributes.Add("losdbprodid", new Models.Attribute("losdbprodid", losdbprodid, NewUpdatedPartsType.NUP_AUDIT_IES_TABLE));
                                newUpdatedParts.Add(newUpdatedPart);
                            }
                        }
                    }

                }


                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform search. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return newUpdatedParts;
        }

        public async Task<List<NewUpdatedParts>> GetDetailInformationStagingLosdb(int materialCode)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            NewUpdatedParts newUpdatedPartsap = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMTLID", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_LOSDB_DETAIL", parameters);
                    while (reader.Read())
                    {
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.UpdatedStagingPartsLosdb)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.IES_EA_MAIN_EXTN));
                        }
                        nupStaging.Add(newUpdatedPartsap);
                    }

                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform show detail. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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
            return nupStaging;

        }

        public async Task<List<NewUpdatedParts>> GetPossibleMatchSap(int materialCode)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            NewUpdatedParts newUpdatedPartsap = null;

            await Task.Run(() =>
            {
                try
                {
                    string prodt_id = null;
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMTLCD", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_POSSIBLE_LOSDB_ASSOCIATIONS", parameters);
                    while (reader.Read())
                    {

                        prodt_id = (string)reader["product_id"];
                        parameters = dbManager.GetParameterArray(2);
                        parameters[0] = dbManager.GetParameter("pPRODID", DbType.String, prodt_id.ToUpper(), ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                        reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_LOSDB_ASSOCIATION_DETAIL", parameters);
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.PossibleMatch)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.IES_EQPT));
                        }
                        newUpdatedPartsap.Attributes.Add("prodt_id", new Models.Attribute("prodt_id", prodt_id, NewUpdatedPartsType.IES_EQPT));
                        nupStaging.Add(newUpdatedPartsap);

                    }

                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform show detail. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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
            return nupStaging;

        }
        public async Task<List<NewUpdatedParts>> GetPossibleMatchLosdb(int materialCode)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IDataReader reader1 = null;
            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            NewUpdatedParts newUpdatedPartsap = null;

            await Task.Run(() =>
            {
                try
                {
                    string prodt_id = String.Empty;
                    string ies_eqpt_ctlg_item_id = String.Empty;
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMTLCD", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_POSSIBLE_LOSDB_ASTS", parameters);
                    while (reader.Read())
                    {
                        newUpdatedPartsap = new NewUpdatedParts();
                        prodt_id = reader["ies_eqpt_prod_id"].ToString();
                        ies_eqpt_ctlg_item_id = reader["ies_eqpt_ctlg_item_id"].ToString();
                        parameters = dbManager.GetParameterArray(2);
                        parameters[0] = dbManager.GetParameter("pPRODID", DbType.Int32, Int32.Parse(prodt_id), ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                        reader1 = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_LOSDB_ASSOCIATION_DETAIL", parameters);
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        reader1.Read();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.PossibleMatch)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader1, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.IES_EQPT));
                        }
                        newUpdatedPartsap.Attributes.Remove("prodt_id");
                        newUpdatedPartsap.Attributes.Add("prodt_id", new Models.Attribute("prodt_id", prodt_id, NewUpdatedPartsType.IES_EQPT));
                        newUpdatedPartsap.Attributes.Remove("ies_eqpt_ctlg_item_id");
                        newUpdatedPartsap.Attributes.Add("ies_eqpt_ctlg_item_id", new Models.Attribute("ies_eqpt_ctlg_item_id", ies_eqpt_ctlg_item_id, NewUpdatedPartsType.IES_EQPT));
                        nupStaging.Add(newUpdatedPartsap);
                    }
                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform GetPossibleMatchLosdb. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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
                    if (reader1 != null)
                    {
                        reader1.Close();
                        reader1.Dispose();
                    }
                }
            });
            return nupStaging;

        }
        public async Task<List<NewUpdatedParts>> GetDetailInformationStaging(int materialCode, string recordtype)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IAccessor dbManagerUpdate1 = null;
            IAccessor dbManagerUpdate2 = null;

            IDbDataParameter[] parameters = null;
            IDbDataParameter[] parametersUpdate1 = null;
            IDbDataParameter[] parametersUpdate2 = null;

            IDataReader reader = null;
            IDataReader readerUpdate1 = null;
            IDataReader readerUpdate2 = null;

            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            NewUpdatedParts newUpdatedPartsap = null;
            int updatecnt = 0;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pmate", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_NEWUPDATEDPARTS_SAP", parameters);
                    while (reader.Read())
                    {
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        newUpdatedPartsap = new NewUpdatedParts("Existing");
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.UpdatedStagingPartsSap)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MIS_TABLE));
                        }
                        nupStaging.Add(newUpdatedPartsap);
                    }
                    if (dbManager != null)
                        dbManager.Dispose();
                    dbManagerUpdate1 = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    readerUpdate1 = null;
                    parametersUpdate1 = dbManager.GetParameterArray(3);
                    parametersUpdate1[0] = dbManager.GetParameter("pmate", DbType.Int32, materialCode, ParameterDirection.Input);
                    parametersUpdate1[1] = dbManager.GetParameter("preco", DbType.String, recordtype, ParameterDirection.Input);
                    parametersUpdate1[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    readerUpdate1 = dbManagerUpdate1.ExecuteDataReaderSP("MATERIAL_PKG.GET_NEWUPDATEDPARTS_MCSH", parametersUpdate1);
                    while (readerUpdate1.Read())
                    {
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        Models.Attribute svalue;
                        NewUpdatedParts newUpdatedPart = new NewUpdatedParts("Update:" + (++updatecnt).ToString());
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.StagingParts)
                        {
                            if (newUpdatedPartsap != null)
                            {
                                if (newUpdatedPartsap.Attributes.TryGetValue(keyValue.Key, out svalue))
                                {
                                    if (svalue.Value == DataReaderHelper.GetNonNullValue(readerUpdate1, keyValue.Value.Column, keyValue.Value.IsNumber))
                                    {
                                        newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate1, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, false));
                                    }
                                    else
                                    {
                                        newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate1, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, true));
                                    }
                                }
                            }
                            else
                            {
                                newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate1, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, false));
                            }

                        }
                        nupStaging.Add(newUpdatedPart);
                    }
                    if (dbManagerUpdate1 != null)
                        dbManagerUpdate1.Dispose();

                    readerUpdate2 = null;
                    dbManagerUpdate2 = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parametersUpdate2 = dbManagerUpdate2.GetParameterArray(3);
                    parametersUpdate2[0] = dbManagerUpdate2.GetParameter("pmate", DbType.Int32, materialCode, ParameterDirection.Input);
                    parametersUpdate2[1] = dbManagerUpdate2.GetParameter("preco", DbType.String, recordtype, ParameterDirection.Input);
                    parametersUpdate2[2] = dbManagerUpdate2.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    readerUpdate2 = dbManagerUpdate2.ExecuteDataReaderSP("MATERIAL_PKG.GET_NEWUPDATEDPARTS_MCS", parametersUpdate2);
                    while (readerUpdate2.Read())
                    {
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        NewUpdatedParts newUpdatedPart = new NewUpdatedParts("Update:" + (++updatecnt).ToString());
                        Models.Attribute svalue;
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.StagingParts)
                        {
                            if (newUpdatedPartsap != null)
                            {
                                if (newUpdatedPartsap.Attributes.TryGetValue(keyValue.Key, out svalue))
                                {
                                    if (svalue.Value == DataReaderHelper.GetNonNullValue(readerUpdate2, keyValue.Value.Column, keyValue.Value.IsNumber))
                                    {
                                        newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate2, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, false));
                                    }
                                    else
                                    {
                                        newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate2, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, true));
                                    }
                                }
                            }
                            else
                            {
                                newUpdatedPart.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(readerUpdate2, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.NUP_MCS_TABLE, false));
                            }
                        }
                        nupStaging.Add(newUpdatedPart);
                    }
                    if (dbManagerUpdate2 != null)
                        dbManagerUpdate2.Dispose();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve 'showdetails' Staging material: {0}";

                    logger.Error(oe, message, materialCode);
                    EventLogger.LogAlarm(oe, string.Format(message, materialCode), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }

                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform show detail. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    if (readerUpdate1 != null)
                    {
                        readerUpdate1.Close();
                        readerUpdate1.Dispose();
                    }
                    if (readerUpdate2 != null)
                    {
                        readerUpdate2.Close();
                        readerUpdate2.Dispose();
                    }

                    if (dbManager != null)
                        dbManager.Dispose();
                    if (dbManagerUpdate1 != null)
                        dbManagerUpdate1.Dispose();
                    if (dbManagerUpdate2 != null)
                        dbManagerUpdate2.Dispose();

                }
            });
            return nupStaging;

        }

        public async Task<string> GetInsertRevisionData(int materialCode)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();

            NewUpdatedParts newUpdatedPartsapbay = null;
            NewUpdatedParts newUpdatedPartsapcard = null;
            NewUpdatedParts newUpdatedPartsapbulk = null;
            NewUpdatedParts newUpdatedPartsapvar = null;
            NewUpdatedParts newUpdatedPartsapbaymtl = null;
            NewUpdatedParts newUpdatedPartsapnode = null;
            NewUpdatedParts newUpdatedPartsapplg = null;
            NewUpdatedParts newUpdatedPartsapshelf = null;
            NewUpdatedParts newUpdatedPartsapcntcard = null;

            await Task.Run(() =>
            {
                try
                {
                    //bayextender                 
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_BAY_EXTNDR_MTRL_PKG.GET_BAY_EXTENDER", parameters);
                    while (reader.Read())
                    {
                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmebayextn)
                        {
                            newUpdatedPartsapbay.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_bay_extn));

                        }
                        //nupStaging.Add(newUpdatedPartsapbay);
                        parameters = dbManager.GetParameterArray(9);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapbay.Attributes["mtrlidbayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapbay.Attributes["revnobayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapbay.Attributes["mfrcdbayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapbay.Attributes["baservnobayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapbay.Attributes["currevnobayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapbay.Attributes["retrvnobayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapbay.Attributes["ordblbayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapbay.Attributes["cleicdbayextn"].Value.ToString(), ParameterDirection.Input);
                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("RME_BAY_EXTNDR_MTRL_PKG.INSERT_REVISION", parameters);
                    }
                    //baymtrl
                    reader = null;

                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_BAY_MTRL_PKG.GET_BAY", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmebaymtrl)
                        {
                            newUpdatedPartsapbaymtl.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_bay_mtrl));

                        }
                        parameters = dbManager.GetParameterArray(19);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapbaymtl.Attributes["mtrlidbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapbaymtl.Attributes["revnobayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapbaymtl.Attributes["mtrlcdbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapbaymtl.Attributes["baservbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapbaymtl.Attributes["currrevbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapbaymtl.Attributes["retrvnbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapbaymtl.Attributes["ordblbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapbaymtl.Attributes["cleicdbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("pPlndHtGntn", DbType.String, newUpdatedPartsapbaymtl.Attributes["plnhetnobayextn"].Value.ToString(), ParameterDirection.Input);
                        parameters[10] = dbManager.GetParameter("pPlndHtGntnUom", DbType.String, newUpdatedPartsapbaymtl.Attributes["plndhetuombayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[11] = dbManager.GetParameter("pNrmDrn", DbType.String, newUpdatedPartsapbaymtl.Attributes["elenormbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[12] = dbManager.GetParameter("pNrmDrnUom", DbType.String, newUpdatedPartsapbaymtl.Attributes["elecurrnormuombayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[13] = dbManager.GetParameter("pMaxDrn", DbType.String, newUpdatedPartsapbaymtl.Attributes["elecurrdrnbayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[14] = dbManager.GetParameter("pMaxDrnUom", DbType.String, newUpdatedPartsapbaymtl.Attributes["elemaxuombayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[15] = dbManager.GetParameter("pWght", DbType.String, newUpdatedPartsapbaymtl.Attributes["baywtnobayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[16] = dbManager.GetParameter("pWghtUom", DbType.String, newUpdatedPartsapbaymtl.Attributes["baywtuombayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[17] = dbManager.GetParameter("pHetNo", DbType.String, newUpdatedPartsapbaymtl.Attributes["hetdspnobayextnm"].Value.ToString(), ParameterDirection.Input);
                        parameters[18] = dbManager.GetParameter("pHetUom", DbType.String, newUpdatedPartsapbaymtl.Attributes["hetdsspuombayextnm"].Value.ToString(), ParameterDirection.Input);
                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("RME_BAY_MTRL_PKG.INSERT_REVISION", parameters);
                    }
                    //bulkcable
                    reader = null;
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_bulk_cabl_mtrl_pkg.get_bulk_cable", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmebulk)
                        {
                            newUpdatedPartsapbulk.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_bulk));

                        }

                        parameters = dbManager.GetParameterArray(10);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapbulk.Attributes["mtrlidbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapbulk.Attributes["rvnobulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapbulk.Attributes["mtrlcdbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapbulk.Attributes["baservbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapbulk.Attributes["currvbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapbulk.Attributes["retevbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapbulk.Attributes["orblbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapbulk.Attributes["cleicdbulk"].Value.ToString(), ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("pSpecnId", DbType.String, newUpdatedPartsapbulk.Attributes["specidbulk"].Value.ToString(), ParameterDirection.Input);
                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("rme_bulk_cabl_mtrl_pkg.INSERT_REVISION", parameters);
                    }
                    //card mtrl
                    reader = null;
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_card_mtrl_pkg.get_card", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmecard)
                        {
                            newUpdatedPartsapcard.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_card));

                        }

                        parameters = dbManager.GetParameterArray(17);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapcard.Attributes["mtrlidcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapcard.Attributes["revnocard"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapcard.Attributes["mtrlcdcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapcard.Attributes["baservcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapcard.Attributes["currvcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapcard.Attributes["retrvcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapcard.Attributes["ordblcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapcard.Attributes["cleicdcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("pNormDrn", DbType.String, newUpdatedPartsapcard.Attributes["elecurnomnocard"].Value.ToString(), ParameterDirection.Input);
                        parameters[10] = dbManager.GetParameter("pNormDrnUom", DbType.String, newUpdatedPartsapcard.Attributes["elecnormcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[11] = dbManager.GetParameter("pMaxDrn", DbType.String, newUpdatedPartsapcard.Attributes["elecucard"].Value.ToString(), ParameterDirection.Input);
                        parameters[12] = dbManager.GetParameter("pMaxDrnUom", DbType.String, newUpdatedPartsapcard.Attributes["elecuruomcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[13] = dbManager.GetParameter("pHetNo", DbType.String, newUpdatedPartsapcard.Attributes["hetdespcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[14] = dbManager.GetParameter("pHetUom", DbType.String, newUpdatedPartsapcard.Attributes["hetuomcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[15] = dbManager.GetParameter("pCardWt", DbType.String, newUpdatedPartsapcard.Attributes["crdwtcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[16] = dbManager.GetParameter("pCardWtUom", DbType.String, newUpdatedPartsapcard.Attributes["crduomcard"].Value.ToString(), ParameterDirection.Input);
                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("rme_card_mtrl_pkg.INSERT_REVISION", parameters);
                    }
                    //cntrlz cbl mtrl
                    reader = null;

                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.GET_CONNECTORIZED_CABLE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmecntcard)
                        {
                            newUpdatedPartsapcntcard.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_cnt_card));

                        }

                        parameters = dbManager.GetParameterArray(9);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapcntcard.Attributes["mtrlidcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapcntcard.Attributes["revnocnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapcntcard.Attributes["mtrlcdcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapcntcard.Attributes["baservcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapcntcard.Attributes["currvcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapcntcard.Attributes["retrvcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapcntcard.Attributes["orblcnzcard"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapcntcard.Attributes["cleicdcnzcard"].Value.ToString(), ParameterDirection.Input);

                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.INSERT_REVISION", parameters);
                    }
                    //node
                    reader = null;

                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_NODE_MTRL_PKG.GET_NODE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmenode)
                        {
                            newUpdatedPartsapnode.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_node));

                        }
                        parameters = dbManager.GetParameterArray(15);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapnode.Attributes["mtrlidnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapnode.Attributes["rervnonode"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapnode.Attributes["mtrlcdnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapnode.Attributes["baservnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapnode.Attributes["currvnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapnode.Attributes["retvnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapnode.Attributes["ordblnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapnode.Attributes["cleicdnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("pPlndHtGntn", DbType.String, newUpdatedPartsapnode.Attributes["plndnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[10] = dbManager.GetParameter("pPlndHtGntnUom", DbType.String, newUpdatedPartsapnode.Attributes["pnduomnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[11] = dbManager.GetParameter("pWght", DbType.String, newUpdatedPartsapnode.Attributes["nodewtnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[12] = dbManager.GetParameter("pWghtUom", DbType.String, newUpdatedPartsapnode.Attributes["nodewtuomnode"].Value.ToString(), ParameterDirection.Input);
                        parameters[13] = dbManager.GetParameter("pHetNo", DbType.String, newUpdatedPartsapnode.Attributes["hetnonode"].Value.ToString(), ParameterDirection.Input);
                        parameters[14] = dbManager.GetParameter("pHetUom", DbType.String, newUpdatedPartsapnode.Attributes["hetuomnode"].Value.ToString(), ParameterDirection.Input);

                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("RME_NODE_MTRL_PKG.INSERT_REVISION", parameters);
                    }
                    //plugin
                    reader = null;

                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_plg_in_mtrl_pkg.get_plug_in", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmeplg)
                        {
                            newUpdatedPartsapplg.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_plg));

                        }

                        parameters = dbManager.GetParameterArray(9);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapplg.Attributes["mtrlidplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapplg.Attributes["revnoplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapplg.Attributes["mtlcdplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapplg.Attributes["baservplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapcard.Attributes["currrplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapplg.Attributes["retrvplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapplg.Attributes["orblplg"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapplg.Attributes["cleicdplg"].Value.ToString(), ParameterDirection.Input);

                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("rme_plg_in_mtrl_pkg.INSERT_REVISION", parameters);
                    }
                    //shelf
                    reader = null;
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_shelf_mtrl_pkg.get_shelf", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmeshelf)
                        {
                            newUpdatedPartsapshelf.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_shelf));

                        }
                        parameters = dbManager.GetParameterArray(13);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pMtrlId", DbType.String, newUpdatedPartsapshelf.Attributes["mtrlids"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pRevsnNo", DbType.String, newUpdatedPartsapshelf.Attributes["revsnos"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapshelf.Attributes["mtrlcds"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pBaseInd", DbType.String, newUpdatedPartsapshelf.Attributes["baserevs"].Value.ToString(), ParameterDirection.Input);
                        parameters[5] = dbManager.GetParameter("pCurrInd", DbType.String, newUpdatedPartsapshelf.Attributes["currevsids"].Value.ToString(), ParameterDirection.Input);
                        parameters[6] = dbManager.GetParameter("pRetInd", DbType.String, newUpdatedPartsapshelf.Attributes["retrvsns"].Value.ToString(), ParameterDirection.Input);
                        parameters[7] = dbManager.GetParameter("pOrdblId", DbType.String, newUpdatedPartsapshelf.Attributes["ordblmtrls"].Value.ToString(), ParameterDirection.Input);
                        parameters[8] = dbManager.GetParameter("pClei", DbType.String, newUpdatedPartsapshelf.Attributes["cleicds"].Value.ToString(), ParameterDirection.Input);
                        parameters[9] = dbManager.GetParameter("pHetNo", DbType.String, newUpdatedPartsapshelf.Attributes["hetnos"].Value.ToString(), ParameterDirection.Input);
                        parameters[10] = dbManager.GetParameter("pHetUom", DbType.String, newUpdatedPartsapshelf.Attributes["hetuoms"].Value.ToString(), ParameterDirection.Input);
                        parameters[11] = dbManager.GetParameter("pShelfWt", DbType.String, newUpdatedPartsapshelf.Attributes["shelfwts"].Value.ToString(), ParameterDirection.Input);
                        parameters[12] = dbManager.GetParameter("pShelfWtUom", DbType.String, newUpdatedPartsapshelf.Attributes["shelfuoms"].Value.ToString(), ParameterDirection.Input);
                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("rme_shelf_mtrl_pkg.INSERT_REVISION", parameters);
                    }
                    //varlength
                    reader = null;

                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_VAR_LGTH_CABL_MTRL_PKG.GET_VARIABLE_LENGTH_CABLE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.Rmevar)
                        {
                            newUpdatedPartsapvar.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.rme_var));

                        }

                        parameters = dbManager.GetParameterArray(5);
                        parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pCblTypId", DbType.String, newUpdatedPartsapvar.Attributes["cbltypvar"].Value.ToString(), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pUomId", DbType.String, newUpdatedPartsapvar.Attributes["setlngtvar"].Value.ToString(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pSpecnId", DbType.String, newUpdatedPartsapvar.Attributes["setlngtvar"].Value.ToString(), ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("pMtrlCd", DbType.String, newUpdatedPartsapvar.Attributes["mtrlcdvar"].Value.ToString(), ParameterDirection.Input);

                        reader = null;
                        reader = dbManager.ExecuteDataReaderSP("RME_VAR_LGTH_CABL_MTRL_PKG.INSERT_VARIABLE_LENGTH_CABLE", parameters);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform show detail. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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
            return "SUCCESS";
        }
        public async Task<List<NewUpdatedParts>> GetRevisionData(int materialCode)
        {
            /*DBcall : to get detail updates from catalog staging of the material ids pending for review.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<NewUpdatedParts> nupStaging = null;
            NewUpdatedPartsType nupType = new NewUpdatedPartsType();
            NewUpdatedParts newUpdatedPartsap = null;
            await Task.Run(() =>
            {
                try
                {
                    //bayextender                 
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    string Bayextender = "Bayextender";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_BAY_EXTNDR_MTRL_PKG.GET_BAY_EXTENDER", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();
                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", Bayextender, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //baymtrl
                    reader = null;
                    string Baymtrl = "Baymtrl";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_BAY_MTRL_PKG.GET_BAY", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", Baymtrl, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //bulkcable
                    reader = null;
                    string Bulkcable = "Bulkcable";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_bulk_cabl_mtrl_pkg.get_bulk_cable", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", Bulkcable, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //card mtrl
                    reader = null;
                    string cardmtrl = "cardmtrl";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_card_mtrl_pkg.get_card", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", cardmtrl, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //cntrlz cbl mtrl
                    reader = null;
                    string cntzcblmtrl = "cntzcblmtrl";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.GET_CONNECTORIZED_CABLE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", cntzcblmtrl, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //node
                    reader = null;
                    string node = "node";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_NODE_MTRL_PKG.GET_NODE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", node, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //plugin
                    reader = null;
                    string plugin = "plugin";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_plg_in_mtrl_pkg.get_plug_in", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", plugin, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //shelf
                    reader = null;
                    string shelf = "shelf";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("rme_shelf_mtrl_pkg.get_shelf", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", shelf, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                    //varlength
                    reader = null;
                    string varlength = "varlength";
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = null;
                    reader = dbManager.ExecuteDataReaderSP("RME_VAR_LGTH_CABL_MTRL_PKG.GET_VARIABLE_LENGTH_CABLE", parameters);
                    while (reader.Read())
                    {

                        if (nupStaging == null)
                            nupStaging = new List<NewUpdatedParts>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in nupType.RevisionPart)
                        {
                            newUpdatedPartsap.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), NewUpdatedPartsType.commonrevison));
                        }
                        newUpdatedPartsap.Attributes.Add("typer", new Models.Attribute("typer", varlength, NewUpdatedPartsType.commonrevison));

                        nupStaging.Add(newUpdatedPartsap);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform show detail. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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
            return nupStaging;
        }


        public async Task<string> AcceptUpdatedPartsDB(string mId, string cuid, string recordtype, string isnew, string losdbprodid, string losdbEqptId, string clmc, string desc)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();
            int status = 0;
            long materialItemId = 0;
            string returnString = String.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(8);

                    parameters[0] = dbManager.GetParameter("pmate", DbType.String, mId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pcuid", DbType.String, cuid, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("preco", DbType.String, recordtype.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pisne", DbType.String, isnew.ToUpper(), ParameterDirection.Input);
                    if (losdbprodid == "")
                    {
                        parameters[4] = dbManager.GetParameter("plosdbprodid", DbType.Int32, null, ParameterDirection.Input);
                    }
                    else
                    {
                        parameters[4] = dbManager.GetParameter("plosdbprodid", DbType.Int32, Int32.Parse(losdbprodid), ParameterDirection.Input);
                    }
                    if (losdbEqptId == "")
                    {
                        parameters[5] = dbManager.GetParameter("plosdbeqptid", DbType.String, null, ParameterDirection.Input);
                    }
                    else
                    {
                        parameters[5] = dbManager.GetParameter("plosdbeqptid", DbType.String, losdbEqptId, ParameterDirection.Input);
                    }
                    parameters[6] = dbManager.GetParameter("retval", DbType.Int32, status, ParameterDirection.Output);
                    parameters[7] = dbManager.GetParameter("oMtlItemId", DbType.Int64, materialItemId, ParameterDirection.Output);

                    dbManager.ExecuteNonQuerySP("MATERIAL_PKG.ACCEPT_NEWUPDATEDPARTS", parameters);

                    status = int.Parse(parameters[6].Value.ToString());
                    if (status > 0) returnString = "success";

                    materialItemId = long.Parse(parameters[7].Value.ToString());

                    mtlDbInterface.InsertWorkToDo(materialItemId, "CATALOG_UI", null);

                    // check to see if this material has a record in mtrl and check if the mfr_id or description is changing
                    string materialID = GetMyMaterialID(materialItemId.ToString());
                    if (materialID.ToString() != String.Empty)
                    {
                        MaterialItem materialItem = new MaterialItem();
                        materialItem = GetMaterial(materialID);
                        string mfrID = GetMFRID(clmc);
                        if (mfrID != String.Empty)  // The clmc exists in the mfr table
                        {
                            if (mfrID != materialItem.MfrID.ToString() || desc != materialItem.Desc)  // if either one of these is different, update mtrl table
                            {
                                UpdateMtrl(materialID, mfrID, desc);

                                // this material has a spec so we have to deal with a changing clmc or description downstream in NDS
                                if (materialItem.SpecName != String.Empty)
                                {
                                    // Get Spec ID, spec table name and alias_val from spec name
                                    string specID = String.Empty;
                                    string specTableName = String.Empty;
                                    string NDSID = String.Empty;
                                    string specType = String.Empty;
                                    string propagatedInd = String.Empty;
                                    string returnInfoString = String.Empty;

                                    GetSpecInfo(materialItem.SpecName, ref specID, ref specTableName, ref NDSID, ref specType, ref propagatedInd, ref returnInfoString);

                                    // Need to update spec name to start with new clmc
                                    if (materialItem.SpecName.Length >= 4)
                                    {
                                        if (materialItem.SpecName.ToUpper().Substring(0, 4) == materialItem.CLMC.ToUpper())
                                        {
                                            materialItem.SpecName = clmc.ToUpper() + materialItem.SpecName.ToUpper().Substring(4, materialItem.SpecName.Length - 4);
                                            materialItem.Desc = desc;
                                            // update spec
                                            UpdateSpec(specID, specTableName, materialItem.SpecName, materialItem.Desc, materialItemId.ToString());

                                            // Need to update spec downstream to NDS so create a work_to_do
                                            if (specID != String.Empty) // this material has a spec
                                            {
                                                WorkDbInterface workDbInterface = new WorkDbInterface();
                                                ChangeSet changeSet = new ChangeSet();
                                                ChangeRecord changeRecord = new ChangeRecord();
                                                changeSet.ChangeSetId = long.Parse(specID);
                                                changeSet.ProjectId = 0L;
                                                changeRecord.PniId = NDSID;
                                                long workToDoID = 0L;
                                                if (NDSID == String.Empty)
                                                {
                                                    // this would be a case where there is a spec id but no nds id, which would be odd if
                                                    // the spec was never sent to nds in the first place, so send this as an insert
                                                    changeRecord.TableName = "INSERT";
                                                    changeRecord.PniId = "0";
                                                    workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                                                }
                                                else if (NDSID == "0")
                                                {
                                                    // this is a bay or bay_extndr and has no bay_specn_alias_val or bay_extndr_specn_alias_val table??
                                                }
                                                else
                                                {
                                                    // this has an nds id so send an udpate
                                                    changeRecord.TableName = "UPDATE";
                                                    workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SPEC");
                                                }
                                                if (workToDoID > 0)
                                                {
                                                    returnString = specID + "/" + workToDoID + "/" + specType;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // This may be a good place to deal with new parts.
                    //else if (isnew == "Y")
                    //{
                    //}
                }
                catch (Exception ex)
                {
                    status = -1;
                    logger.Error(ex, "Exception while perform accept. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                    returnString = "";
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return returnString;
        }

        private void UpdateMtrl(string materialID, string mfrID, string desc)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                // Per Jesus Hernandez, remove the update of the mtrl_dsc
                //string sql = "update mtrl set mfr_id = " + mfrID + ", mtrl_dsc = '" + desc + "' where mtrl_id = " + materialID;
                string sql = "update mtrl set mfr_id = " + mfrID + " where mtrl_id = " + materialID;

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform UpdateMtrl. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public void UpdateSpec(string specID, string tableName, string specName, string specDesc, string materialItemID)
        {
            IAccessor dbManager = null;
            string descriptionColumn = String.Empty;
            bool revisionAlt = false;
            string sql = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                dbManager.BeginTransaction();
                
                if (tableName != String.Empty && tableName.Substring(0, 4) == "card")
                {
                    descriptionColumn = "card_dsc";
                    revisionAlt = true;
                }
                else if (tableName != String.Empty && tableName.Substring(0, 3) == "plg")
                {
                    descriptionColumn = tableName + "plg_in_dsc";
                    revisionAlt = false;
                }
                else if (tableName != String.Empty && tableName.Substring(0,4) == "slot")
                {
                    descriptionColumn = "slot_dsc";
                    revisionAlt = false;
                }
                else
                {
                    descriptionColumn = tableName + "_dsc";
                    revisionAlt = true;
                }
                
                if (specID != String.Empty)
                {
                    if (specDesc == String.Empty)
                    {
                        sql = "update " + tableName + " set " + tableName + "_nm " + "= '" + specName + "' where " + tableName + "_id = " + specID;
                    }
                    else
                    {
                        sql = "update " + tableName + " set " + tableName + "_nm " + "= '" + specName + "', " + descriptionColumn + " = '" + specDesc + "' where " + tableName + "_id = " + specID;
                    }
                    dbManager.ExecuteNonQuery(CommandType.Text, sql);
                }

                // update material_item_attributes
                sql = "update material_item_attributes set value = '" + specName + "' where material_item_id = " + materialItemID + " and mtl_item_attributes_def_id = 121";
                dbManager.ExecuteNonQuery(CommandType.Text, sql);

                if (specDesc != String.Empty)
                {
                    sql = "update material_item_attributes set value = '" + specDesc + "' where material_item_id = " + materialItemID + " and mtl_item_attributes_def_id = 123";
                    dbManager.ExecuteNonQuery(CommandType.Text, sql);
                }

                // update the ***_specn_revsn_alt table
                if (revisionAlt && specID != String.Empty)
                {
                    sql = "update " + tableName + "_revsn_alt set " + tableName + "_revsn_nm = '" + specName + "' where " + tableName + "_id = " + specID;
                    dbManager.ExecuteNonQuery(CommandType.Text, sql);
                }
                dbManager.CommitTransaction();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform UpdateSpec. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public void InsertSpec(string specName, string materialItemID)
        {
            IAccessor dbManager = null;
            string descriptionColumn = String.Empty;
            string sql = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                dbManager.BeginTransaction();

                // insert material_item_attributes
                sql = "insert into material_item_attributes (material_item_id, mtl_item_attributes_def_id, value, last_updated_cuid, last_updated_date) " +
                      "values(" + materialItemID + ", 121, '" + specName + "', 'CDMMS', sysdate)";
                dbManager.ExecuteNonQuery(CommandType.Text, sql);

                dbManager.CommitTransaction();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform UpdateSpec. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        private string GetMyMaterialID(string materialItemID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDataReader reader = null;
            string materialID = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                string sql = "select mtrl_id from material_id_mapping_vw where material_item_id = " + materialItemID;

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialID = reader["mtrl_id"].ToString();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetMyMaterialID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return materialID;
        }

        private string GetMFRID(string clmc)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDataReader reader = null;
            string mfrID = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                string sql = "select mfr_id from mfr where mfr_cd = '" + clmc + "'";

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    mfrID = reader["mfr_id"].ToString();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetMFRID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return mfrID;
        }

        private MaterialItem GetMaterial(string materialID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDataReader reader = null;
            MaterialItem materialItem = new MaterialItem();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                string sql = "select a.mfr_id, a.mtrl_dsc, b.spec_nm, c.mfr_cd " +
                            "from mtrl a, material_id_mapping_vw b, mfr c " +
                            "where a.mtrl_id = '" + materialID + "' " +
                            "and a.mtrl_id = b.mtrl_id " +
                            "and a.mfr_id = c.mfr_id";

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);
                    
                while(reader.Read())
                {
                    string mfrid = reader["mfr_id"].ToString();
                    if (mfrid != String.Empty)
                    {
                        materialItem.MfrID = long.Parse(mfrid);
                    }
                    materialItem.Desc = reader["mtrl_dsc"].ToString();
                    materialItem.CLMC = reader["mfr_cd"].ToString();
                    materialItem.SpecName = reader["spec_nm"].ToString();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetMaterial. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return materialItem;
        }

        public void GetSpecInfo(string specName, ref string specID, ref string tableName, ref string aliasVal, ref string specType, ref string propagatedInd, ref string returnString)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDataReader reader = null;
            returnString = "SUCCESS";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                string sql = "select a.bay_specn_id as specid, 'bay_specn' as tablename, '0' as alias_val, 'BAY' as spectype, b.prpgt_ind " +
                            "from bay_specn a, bay_specn_revsn_alt b " +
                            "where a.bay_specn_id = b.bay_specn_id(+) and a.bay_specn_nm = '" + specName + "' " +
                            "union " +
                            "select a.bay_extndr_specn_id as specid, 'bay_extndr_specn' as tablename, '0' as alias_val, 'BAY_EXTENDER' as spectype, b.prpgt_ind " +
                            "from bay_extndr_specn a, bay_extndr_specn_revsn_alt b where a.bay_extndr_specn_nm = '" + specName + "' " +
                            "union " +
                            "select a.card_specn_id as specid, 'card_specn' as tablename, b.alias_val, 'CARD' as spectype, c.prpgt_ind " +
                            "from card_specn a, card_specn_alias_val b, card_specn_revsn_alt c where a.card_specn_nm = '" + specName + "' " +
                            "and a.card_specn_id = b.card_specn_id(+) " +
                            "and a.card_specn_id = c.card_specn_id(+) " +
                            "union " +
                            "select a.node_specn_id as specid, 'node_specn' as tablename, b.alias_val, 'NODE' as spectype, c.prpgt_ind " +
                            "from node_specn a, node_specn_alias_val b, node_specn_revsn_alt c where a.node_specn_nm = '" + specName + "' " +
                            "and a.node_specn_id = b.node_specn_id(+) " +
                            "and a.node_specn_id = c.node_specn_id(+) " +
                            "union " +
                            "select a.plg_in_specn_id as specid, 'plg_in_specn' as tablename, b.alias_val, 'PLUG_IN' as spectype, 'N' as prpgt_ind " +
                            "from plg_in_specn a, plg_in_specn_alias_val b where a.plg_in_specn_nm = '" + specName + "' " +
                            "and a.plg_in_specn_id = b.plg_in_specn_id(+) " +
                            "union " +
                            "select a.shelf_specn_id as specid, 'shelf_specn' as tablename, b.alias_val, 'SHELF' as spectype, c.prpgt_ind " +
                            "from shelf_specn a, shelf_specn_alias_val b, shelf_specn_revsn_alt c where a.shelf_specn_nm = '" + specName + "' " +
                            "and a.shelf_specn_id = b.shelf_specn_id(+) " +
                            "and a.shelf_specn_id = c.shelf_specn_id(+) " +
                            "union " +
                            "select a.slot_specn_id as specid, 'slot_specn' as tablename, b.alias_val, 'SLOT' as spectype, 'N' as prpgt_ind " +
                            "from slot_specn a, slot_specn_alias_val b where a.slot_specn_nm = '" + specName + "' " +
                            "and a.slot_specn_id = b.slot_specn_id(+) ";

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    specID = reader["specid"].ToString();
                    tableName = reader["tablename"].ToString();
                    aliasVal = reader["alias_val"].ToString();
                    specType = reader["spectype"].ToString();
                    propagatedInd = reader["prpgt_ind"].ToString();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetSpecInfo. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                returnString = "ERROR";
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public async Task<string> AcceptUpdatedPartsDBlosdb(string auditId, string usraprvText, string usrTmstp, string apprvId,
            string AuditTablePkColumnName, string CDMMSColumnName, string NewColumnValue, string CDMMSTableName, string LOSDBProdID,
            string AuditTablePkColumnValue, string AuditParentTablePKColumnName, string AuditParentTablePkColumnValue)
        {

            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Common.DbInterface.LOSDBDbInterface commonLOSDBDbInterface = new Common.DbInterface.LOSDBDbInterface();
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(5);

                    parameters[0] = dbManager.GetParameter("pAPPRVIND", DbType.String, apprvId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pUSERID", DbType.String, apprvId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pAPPROVEDT", DbType.String, usrTmstp, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pAPPRVTEXT", DbType.String, usraprvText, ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pAUDITDAID", DbType.Int32, auditId, ParameterDirection.Input);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.ACCEPT_LOSDB_PART", parameters);


                }
                catch (Exception ex)
                {

                    logger.Error(ex, "Exception while perform accept. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }

                if (AuditTablePkColumnName == "PROD_ID")
                {
                    // only care if this has CDMMS ramifications
                    if (CDMMSColumnName != null && CDMMSColumnName != "")
                    {
                        if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("REVSN"))
                        {
                            commonLOSDBDbInterface.UpdateEquipmentRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, LOSDBProdID);
                        }
                        else if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("MTRL"))
                        {
                            // there are none of these listed so far
                        }
                        else
                        {
                            commonLOSDBDbInterface.UpdateEquipment(CDMMSTableName, CDMMSColumnName, NewColumnValue, LOSDBProdID);
                        }
                    }
                }
                else if (AuditTablePkColumnName == "EQPT_CTLG_ITEM_ID")
                {
                    if (CDMMSColumnName != null && CDMMSColumnName != "")
                    {
                        if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("REVSN"))
                        {
                            commonLOSDBDbInterface.UpdateInventoryRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditTablePkColumnValue);
                        }
                        else if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("MTRL"))
                        {
                            // there are none of these listed so far
                        }
                        else
                        {
                            commonLOSDBDbInterface.UpdateInventory(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditTablePkColumnValue);
                        }
                    }
                }
                else if (AuditTablePkColumnName == "COMP_CLEI_KEY" && AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
                {
                    if (CDMMSColumnName != null && CDMMSColumnName != "")
                    {
                        if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("REVSN"))
                        {
                            commonLOSDBDbInterface.UpdateParentRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditParentTablePkColumnValue, "*_MTRL_REVSN");
                        }
                        else if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("MTRL"))
                        {
                            commonLOSDBDbInterface.UpdateParentRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditParentTablePkColumnValue, "*_MTRL");
                        }
                    }
                }
                else if (AuditTablePkColumnName == "ELECTRICAL_KEY" && AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
                {
                    if (CDMMSColumnName != null && CDMMSColumnName != "")
                    {
                        if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("REVSN"))
                        {
                            commonLOSDBDbInterface.UpdateParentRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditParentTablePkColumnValue, "*_MTRL_REVSN");
                        }
                        else if (CDMMSTableName.StartsWith("*") && CDMMSTableName.ToUpper().EndsWith("MTRL"))
                        {
                            commonLOSDBDbInterface.UpdateParentRevision(CDMMSTableName, CDMMSColumnName, NewColumnValue, AuditParentTablePkColumnValue, "*_MTRL");
                        }
                    }
                }
                else if (AuditTablePkColumnName == "CLEICODE" && AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
                {
                    // no CDMMS tables to update for this scenario
                }
            });

            return "success";
        }

        public async Task<string> AssociateLosdbtoSap(string materialItemId, string iesProdId, string equipmentCatalogItemID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            string status = "SUCCESS";

            await Task.Run(() =>
            {
                try
                {
                    status = NonAsyncAssociateLosdbtoSap(materialItemId, iesProdId, equipmentCatalogItemID);
                }
                catch (Exception)
                {
                    status = "ERROR";
                }
            });

            return status;
        }

        public string NonAsyncAssociateLosdbtoSap(string materialItemId, string iesProdId, string equipmentCatalogItemID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, long.Parse(materialItemId), ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pProdId", DbType.Int64, long.Parse(iesProdId), ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pEqptCtlgItemId", DbType.String, equipmentCatalogItemID, ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("MATERIAL_PKG.ASSOCIATE_LOSDB_TO_SAP", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while attempting to associate LOSDB material ({0}, {1}). Message: {2}", materialItemId, iesProdId, ex.Message);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return "SUCCESS";
        }

        public async Task<string> AssociateLosdbtoSapMaterialCode(string materialCode, string prodID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();
            IDataReader reader = null;


            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pMtrlCode", DbType.String, materialCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pProdID", DbType.Int32, Int32.Parse(prodID), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("RETCSR", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.ASSOCIATE_LOSDB_TO_SAP_MTL_CD", parameters);
                }
                catch (Exception ex)
                {

                    logger.Error(ex, "Exception while perform AssociateLosdbtoSapMaterialCode. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return "success";
        }

        public string NonAsyncAssociateLosdbtoSapMaterialCode(string materialCode, string prodID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();
            IDataReader reader = null;


            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pMtrlCode", DbType.String, materialCode, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pProdID", DbType.Int32, Int32.Parse(prodID), ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("RETCSR", ParameterDirection.Output);
                reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.ASSOCIATE_LOSDB_TO_SAP_MTL_CD", parameters);
            }
            catch (Exception ex)
            {

                logger.Error(ex, "Exception while perform AssociateLosdbtoSapMaterialCode. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return "success";
        }

        public async Task<string> AssociateSaptoLosdb(string mID, string pID)
        {
            /*DBcall : to accept the updates of the materials from catalog staging. Updates will be moved to item SAP. pending.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();
            IDataReader reader = null;


            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pPRODUCTID", DbType.String, mID, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPRODID", DbType.String, pID, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("RETCSR", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.ASSOCIATE_SAP_TO_LOSDB", parameters);


                }
                catch (Exception ex)
                {

                    logger.Error(ex, "Exception while perform accept. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return "success";
        }

        public async Task<string> RejectUpdatedPartsDB(string mId, string cuid, string recordtype)
        {
            /*DBcall : Rejects the updates of material catalog staging.*/
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            int status = 0;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(4);

                    parameters[0] = dbManager.GetParameter("pmate", DbType.String, mId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pcuid", DbType.String, cuid.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("preco", DbType.String, recordtype.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("retval", DbType.Int32, status, ParameterDirection.Output);

                    dbManager.ExecuteNonQuerySP("MATERIAL_PKG.REJECT_NEWUPDATEDPARTS", parameters);

                    status = int.Parse(parameters[3].Value.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform reject. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                    status = -1;
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (status >= 0)
            {
                return "success";
            }
            else
                return "";
        }

        public async Task<string> GetMaterialID(string materialCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string returnString = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pPrdctId", DbType.String, materialCode, ParameterDirection.Input, 4000);
                    parameters[1] = dbManager.GetParameter("oMtrlId", DbType.String, returnString, ParameterDirection.Output, 4000);
                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "GET_MATERIAL_ID", parameters);
                    returnString = parameters[1].Value.ToString();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform GetMaterialID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                    {
                        dbManager.Dispose();
                    }
                }
            });
            return returnString;
        }

        public string CheckExistingAssocation(string materialID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string returnString = "";


            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.String, materialID, ParameterDirection.Input, 4000);
                parameters[1] = dbManager.GetParameter("oProductId", DbType.String, returnString, ParameterDirection.Output, 4000);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "LOSDB_PKG.CHECK_EXISTING_ASSOCIATED_MTRL", parameters);
                returnString = parameters[1].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform CheckExistingAssocation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                {
                    dbManager.Dispose();
                }
            }
            return returnString;
        }

        public void DeleteExistingAssocation(string materialID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(1);
                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.String, materialID, ParameterDirection.Input, 4000);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "LOSDB_PKG.DEL_EXISTING_ASSOCIATED_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform DeleteExistingAssocation. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                {
                    dbManager.Dispose();
                }
            }
        }

        public async Task<string> GetMaterialIDFromMaterialItemID(long materialItemID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string returnString = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMtrlItemID", DbType.Int32, materialItemID, ParameterDirection.Input, 4000);
                    parameters[1] = dbManager.GetParameter("oMtrlId", DbType.String, returnString, ParameterDirection.Output, 4000);
                    dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.GET_MATERIAL_ID_FROM_MITEM_ID", parameters);
                    returnString = parameters[1].Value.ToString();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform GetMaterialIDFromMaterialItemID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                    {
                        dbManager.Dispose();
                    }
                }
            });
            return returnString;
        }

        public string NonAsyncGetMaterialIDFromMaterialItemID(long materialItemID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string returnString = "";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pMtrlItemID", DbType.Int32, materialItemID, ParameterDirection.Input, 4000);
                parameters[1] = dbManager.GetParameter("oMtrlId", DbType.String, returnString, ParameterDirection.Output, 4000);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.GET_MATERIAL_ID_FROM_MITEM_ID", parameters);
                returnString = parameters[1].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetMaterialIDFromMaterialItemID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                {
                    dbManager.Dispose();
                }
            }
            return returnString;
        }

        //public async Task<string> GetEquipmentProdID(long materialItemID)
        //{
        //    IAccessor dbManager = null;
        //    IDbDataParameter[] parameters = null;
        //    string returnString = "";

        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
        //            parameters = dbManager.GetParameterArray(2);
        //            parameters[0] = dbManager.GetParameter("pMtrlItemID", DbType.Int32, materialItemID, ParameterDirection.Input, 4000);
        //            parameters[1] = dbManager.GetParameter("oEqptProdID", DbType.String, returnString, ParameterDirection.Output, 4000);
        //            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.GET_EQUPIMENT_PROD_ID", parameters);
        //            returnString = parameters[1].Value.ToString();
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error(ex, "Exception while perform GetEquipmentProdID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
        //        }
        //        finally
        //        {
        //            if (dbManager != null)
        //            {
        //                dbManager.Dispose();
        //            }
        //        }
        //    });
        //    return returnString;
        //}

        public string[] NonAsyncGetEquipmentProdID(long materialItemID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string[] returnString = new string[2];

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pMtrlItemID", DbType.Int32, materialItemID, ParameterDirection.Input, 4000);
                parameters[1] = dbManager.GetParameter("oEqptProdID", DbType.String, returnString, ParameterDirection.Output, 4000);
                parameters[2] = dbManager.GetParameter("oEqptCtlgItmId", DbType.String, returnString, ParameterDirection.Output, 4000);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.GET_EQUIPMENT_PROD_ID", parameters);

                returnString[0] = parameters[1].Value.ToString();
                returnString[1] = parameters[2].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while perform GetEquipmentProdID. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (dbManager != null)
                {
                    dbManager.Dispose();
                }
            }

            return returnString;
        }

        public string GetMaterialItemID(string materialCode)
        {
            string returnString = "";
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select material_item_id from material_id_mapping_vw where product_id = '" + materialCode + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnString = reader["material_item_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMTLID_PRODID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMTLID_PRODID({0}, {1})");
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

            return returnString;
        }

        public List<string> GetMaterialItemIDFromMaterialID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<String> materialItemIDList = new List<string>();

            string @sql = "select material_item_id from material_id_mapping_vw where mtrl_id = '" + materialID + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialItemIDList.Add(reader["material_item_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while GetMaterialItemIDFromMaterialID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialItemDBInterface.GetMaterialItemIDFromMaterialID({0}, {1})");
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

            return materialItemIDList;
        }

        public List<string> GetRevisionTableNames(string rootPartNumber, string clmc)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> tableNames = new List<string>();

            string @sql = "select distinct b.cdmms_revsn_tbl_nm " +
                            "from mtrl a, feat_typ b, mfr c " +
                            "where a.feat_typ_id = b.feat_typ_id " +
                            "and a.mfr_id = c.mfr_id " +
                            "and a.rt_part_no like '" + rootPartNumber + "%' " +
                            "and c.mfr_cd = '" + clmc + "' " +
                            "and b.cdmms_revsn_tbl_nm is not null ";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    string tableName = reader["cdmms_revsn_tbl_nm"].ToString();
                    tableNames.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetRevisionTableNames - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetRevisionTableNames({0}, {1})", rootPartNumber, sql);
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

            return tableNames;
        }

        public List<PartialMatch> GetPartialMatches(string rootPartNumber, string clmc, string revisionTableName)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<PartialMatch> partialMatches = new List<PartialMatch>();

            string @sql = "select a.mtrl_id, i.material_item_id, a.rt_part_no, b.mfr_cd, c.cdmms_revsn_tbl_nm, h.heci, k.clei_cd, " +
                            "d.mtrl_cat_typ, i.product_id, f.mfg_part_no, nvl(a.mtrl_dsc, h.item_desc) as mtrl_dsc, c.feat_typ, g.revsn_no " +
                            "from mtrl a, mfr b, feat_typ c, mtrl_cat d, mtl_item_sap f, " + revisionTableName + " g, " +
                            "mtl_item_sap h, material_item i, mtrl_alias_val j, ies_invntry k " +
                            "where a.rt_part_no like '" + rootPartNumber + "' " +
                            "and b.mfr_cd = '" + clmc + "' " +
                            "and a.mfr_id = b.mfr_id " +
                            "and a.feat_typ_id = c.feat_typ_id(+) " +
                            "and a.mtrl_cat_id = d.mtrl_cat_id " +
                            "and i.product_id = f.product_id " +
                            "and a.mtrl_id = g.mtrl_id " +
                            "and c.cdmms_revsn_tbl_nm is not null " +
                            "and h.product_id = i.product_id " +
                            "and g." + revisionTableName + "_id = i.material_item_id " +
                            "and a.mtrl_id = j.mtrl_id(+) " +
                            "and j.mtrl_alias_id(+) = 1 " +
                            "and to_number(j.alias_val) = k.prod_id(+) ";
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    PartialMatch partialMatch = new PartialMatch();
                    partialMatch.CheckedRow = false;
                    partialMatch.MaterialID = long.Parse(reader["mtrl_id"].ToString());
                    partialMatch.MaterialItemID = long.Parse(reader["material_item_id"].ToString());
                    partialMatch.RootPartNumber = reader["rt_part_no"].ToString();
                    partialMatch.MfgPartNumber = reader["mfg_part_no"].ToString();
                    partialMatch.CLMC = reader["mfr_cd"].ToString();
                    partialMatch.CDMMSRevisionTableName = reader["cdmms_revsn_tbl_nm"].ToString();
                    partialMatch.MaterialCategory = reader["mtrl_cat_typ"].ToString();
                    partialMatch.ProductID = reader["product_id"].ToString();
                    partialMatch.Description = reader["mtrl_dsc"].ToString();
                    partialMatch.FeatureType = reader["feat_typ"].ToString();
                    partialMatch.RevisionNumber = reader["revsn_no"].ToString();
                    partialMatch.HECI = reader["heci"].ToString();
                    partialMatch.CLEI = reader["clei_cd"].ToString();
                    partialMatches.Add(partialMatch);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPartialMatches - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetPartialMatches({0}, {1})", rootPartNumber, sql);
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

            return partialMatches;
        }

        public string GetNewMaterialItemID(string productID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialItemID = String.Empty;

            string @sql = "select material_item_id from material_item where product_id = '" + productID + "'";
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialItemID = reader["material_item_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetMaterialItemID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetMaterialItemID({0}, {1})", productID, sql);
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

            return materialItemID;
        }

        public string InsertGenericRevision(string revisionTableName, string materialItemID, string materialID, string revisionNumber, 
            string productID, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd)
        {
            IAccessor dbManager = null;
            string sql = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.BeginTransaction();

                if (currentRevisionInd == "Y")
                {
                    sql = "update " + revisionTableName + " set curr_revsn_ind = 'N' where mtrl_id = " + materialID;
                }

                dbManager.ExecuteNonQuery(CommandType.Text, sql);

                sql = "insert into " + revisionTableName + " (" + revisionTableName + "_id, mtrl_id, revsn_no, mtrl_cd, " +
                    "base_revsn_ind, curr_revsn_ind, ret_revsn_ind) values (" + materialItemID + ", " + materialID + ", '" +
                    revisionNumber + "', " + productID + ", '" + baseRevisionInd + "', '" + currentRevisionInd + "', '" + retiredRevisionInd + "')";

                dbManager.ExecuteNonQuery(CommandType.Text, sql);

                dbManager.CommitTransaction();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while attempting InsertGenericRevision ({0}, {1}). Message: {2}", materialItemID, materialID, ex.Message);

                throw ex;
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return "SUCCESS";
        }

        public int GetPossibleMatchCount(string materialCode)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            int returnCount = 0;

            string @sql = "select count(*) as result from possible_part_associations where product_id = '" + materialCode + "'";
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnCount = int.Parse(reader["result"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetPossibleMatchCount - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetPossibleMatchCount({0}, {1})", materialCode, sql);
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

            return returnCount;
        }

        public int GetSpecInitCount(string partNumber, string manufacturer)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            int returnCount = 0;

            string @sql = "select count(*) as result from mtrl a, mfr b where a.mfr_id = b.mfr_id and a.specn_init_ind = 'Y' and upper(a.rt_part_no) = '" + partNumber + "' and b.mfr_cd = '" + manufacturer + "'";
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    returnCount = int.Parse(reader["result"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetSpecInitCount - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetSpecInitCount({0}, {1})", partNumber, sql);
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

            return returnCount;
        }
    }
}