using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class ModifiedPartDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private IAccessor dbAccessor = null;

        public ModifiedPartDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public ModifiedPartDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
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

        public async Task<IDictionary<string, string>> GetMaterialItemDef(bool name)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            IDictionary<string, string> IDvalue = new Dictionary<string, string>();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_MATERIAL_ITEM_DEF", parameters);

                    while (reader.Read())
                    {
                        if (name)
                        {
                            IDvalue.Add(reader["LONG_NAME"].ToString().Replace(" ", ""), reader["MTL_ITEM_ATTRIBUTES_DEF_ID"].ToString());
                        }
                        else
                        {
                            IDvalue.Add(reader["MTL_ITEM_ATTRIBUTES_DEF_ID"].ToString(), reader["LONG_NAME"].ToString().Replace(" ", ""));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception while perform GetMaterialItemDef. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
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

            return IDvalue;
        }

        public async Task<string[]> SaveMaterialItem(Dictionary<string, ModifiedPart> dicMP, string productID, string cuid, string publish)
        {
            IAccessor dbManager = null;
            int savestatus = 0;
            IDbDataParameter[] parameters = null;
            long materialId = 0;
            string exmsg = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("ppubl", DbType.String, publish, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("oMtlItmId", DbType.Int64, materialId, ParameterDirection.Output);

                    dbManager.ExecuteScalarSP("MATERIAL_PKG.INSERT_MATERIAL_ITEM", parameters);

                    materialId = long.Parse(parameters[1].Value.ToString());

                    savestatus = 0;
                    parameters = dbManager.GetParameterArray(5);

                    foreach (KeyValuePair<string, ModifiedPart> kMP in dicMP)
                    {
                        //parameters[0] = dbManager.GetParameter("pmate", DbType.Int32, materialID, ParameterDirection.Input);
                        parameters[0] = dbManager.GetParameter("pmate", DbType.Int64, materialId, ParameterDirection.Input);
                        parameters[1] = dbManager.GetParameter("pmiad", DbType.Int64, long.Parse(kMP.Value.ID), ParameterDirection.Input);
                        parameters[2] = dbManager.GetParameter("pvalu", DbType.String, kMP.Value.SValue.ToUpper(), ParameterDirection.Input);
                        parameters[3] = dbManager.GetParameter("pcuid", DbType.String, cuid, ParameterDirection.Input);
                        parameters[4] = dbManager.GetParameter("retnum", DbType.Int32, savestatus, ParameterDirection.Output);

                        dbManager.ExecuteNonQuerySP("MATERIAL_PKG.INSERT_MATERIAL_ITEM_ATTRIBUTE", parameters);

                        savestatus = int.Parse(parameters[4].Value.ToString());

                        if (savestatus == 0)
                        {
                            throw new System.Exception(kMP.Value.Name + " : " + kMP.Value.SValue + " was not inserted correctly. Please check"); ;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("not inserted"))
                        exmsg = ex.Message;

                    if (ex.Message.Contains("unique constraint"))
                    {
                    }
                    else
                    {
                        parameters = dbManager.GetParameterArray(1);

                        parameters[0] = dbManager.GetParameter("pmate", DbType.Int64, materialId, ParameterDirection.Input);

                        dbManager.ExecuteNonQuerySP("MATERIAL_PKG.DELETE_FAILED_MATERIAL_ITEM", parameters);
                    }

                    savestatus = 0;
                    logger.Error(ex, "Exception while perform SaveMaterialItem. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (exmsg != "")
            {
                return new string[] { "fail", exmsg };
            }
            else if (savestatus > 0)
            {
                return new string[] { "success", materialId.ToString(), "0" };
            }
            else
            {
                return new string[] { "fail", "constraint" };
            }
        }

        public async Task<List<SearchResult>> SearchRecordOnlyPartAsync(string searchValue, string recordType, string searchBy)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<SearchResult> results = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(4);

                    parameters[0] = dbManager.GetParameter("pprod", DbType.String, searchValue.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("preco", DbType.String, recordType.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pSearchBy", DbType.String, searchBy, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_RECORD_ONLY_PART", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SearchResult>();

                        SearchResult result = new SearchResult();

                        result.ItemValue = reader["MATERIAL_ITEM_ID"].ToString();
                        result.DisplayValue = reader["PRODUCT_ID"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}";

                    hadException = true;

                    logger.Error(oe, message, searchValue);
                    EventLogger.LogAlarm(oe, string.Format(message, searchValue), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search: {0}", searchValue);
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

            return results;
        }

        public  async Task<List<Option>> GetDropDownAttributes(string short_name)
        {
            string selsql = @"select sql from MATERIAL_ITEM_ATTRIBUTES_DEF where sql is not null and short_name=:shortname";
            List<Option> Optionsdd=null;
            IDbDataParameter[] parameters = null;
            ReferenceDbInterface refDbInterface = new ReferenceDbInterface();
            object sqldd;

            await Task.Run(async () =>
            {
                try
                {
                    StartTransaction();

                    parameters = dbAccessor.GetParameterArray(1);
                    parameters[0] = dbAccessor.GetParameter("shortname", DbType.String, short_name, ParameterDirection.Input);

                    sqldd = dbAccessor.ExecuteScalar(CommandType.Text, selsql, parameters);

                    if (sqldd != null)
                    {
                        Optionsdd = await refDbInterface.GetListOptions(sqldd.ToString());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get drop down list options for ({0})", short_name);
                    
                    throw ex;
                }
            });                   
            
            return Optionsdd;
        }
        
        public async Task<ModifiedPart> SearchAllModifiedParts(string materialId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ModifiedPartType mpType = new ModifiedPartType();
            ModifiedPart mPart = null;
            IDictionary<string, string> IDvalue = null;
            string name = "", idkey = "", value = "";
            IDvalue = await GetMaterialItemDef(false);

            await Task.Run(()=>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pmate", DbType.Int32, int.Parse(materialId), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("ptabl", DbType.String, "ITEM", ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCHALL_RECORDONLYPART", parameters);

                    while (reader.Read())
                    {
                        if (mPart == null)
                        {
                            mPart = new ModifiedPart();
                            mPart.Attributes.Add("MaterialID", materialId);
                        }

                        mPart.Attributes.Add("ProductID", reader["PRODUCT_ID"].ToString());

                        if (reader["RECORD_ONLY_IS_PUBLISHED_IND"].ToString() == "Y")
                        {
                            mPart.Attributes.Add("Publish", "true");
                        }
                        else
                        {
                            mPart.Attributes.Add("Publish", "");
                        }
                    }

                    reader = null;
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pmate", DbType.Int32, int.Parse(materialId), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("ptabl", DbType.String, "ATTRIBUTE", ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCHALL_RECORDONLYPART", parameters);

                    while (reader.Read())
                    {
                        idkey = reader["MTL_ITEM_ATTRIBUTES_DEF_ID"].ToString();
                        value = reader["VALUE"].ToString();

                        if (IDvalue.ContainsKey(idkey))
                        {
                            IDvalue.TryGetValue(idkey, out name);

                            if (mPart == null)
                                mPart = new ModifiedPart();

                            mPart.Attributes.Add(name, value);
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

            return mPart;
        }

        public void InsertMaterialRO(string materialID, string Def_ID, string val, string cuid)
        {
            string sql = @"INSERT INTO MATERIAL_ITEM_ATTRIBUTES (MATERIAL_ITEM_ID, MTL_ITEM_ATTRIBUTES_DEF_ID, VALUE, LAST_UPDATED_CUID, LAST_UPDATED_DATE)
                            VALUES(:material_item_id, :def_id, :val, :cuid, SYSDATE)";
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialID, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("def_id", DbType.String, Def_ID, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("val", DbType.String, val.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("cuid", DbType.String, cuid.ToUpper(), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert material_item_attributes ({0}, {1}", materialID, val);

                throw ex;
            }
        }

        public void UpdateMaterialRO(long materialID, string Def_ID, string val, string cuid)
        {
            string sql = @"UPDATE material_item_attributes 
                            SET value = :value, last_updated_cuid = :cuid, last_updated_date = SYSDATE
                            WHERE material_item_id = :material_item_id
                            AND mtl_item_attributes_def_id = :mtl_item_attributes_def_id";
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("value", DbType.String, val.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("cuid", DbType.String, cuid.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialID, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, Def_ID, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update material_item_attributes({0}, {1}, {2}", materialID, Def_ID, val);

                throw ex;
            }
        }

        public void DeleteMaterialRO(long materialID, string Def_ID)
        {
            string sql = @"DELETE 
                            FROM material_item_attributes a
                            WHERE a.material_item_id = :materialItemId
                            AND a.mtl_item_attributes_def_id = :materialItemDefId";
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialID, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, Def_ID, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete from material_item_attributes_def ({0}, {1})", materialID, Def_ID);

                throw ex;
            }
        }

        public int UpdatePublish(long materialID, string publish)
        {
            string selsql = @"select RECORD_ONLY_IS_PUBLISHED_IND from MATERIAL_ITEM WHERE material_item_id = :materialItemId";
            string updsql = @"UPDATE MATERIAL_ITEM SET RECORD_ONLY_IS_PUBLISHED_IND ='" + publish + "' WHERE material_item_id = :materialItemId";
            int savestatus = 0;
            IDbDataParameter[] parameters = null;
            object isPublished;
            try
            {

                parameters = dbAccessor.GetParameterArray(1);
                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialID, ParameterDirection.Input);
                isPublished = dbAccessor.ExecuteScalar(CommandType.Text, selsql, parameters);
                if (publish != isPublished.ToString())
                {
                    dbAccessor.ExecuteNonQuery(CommandType.Text, updsql, parameters);
                    savestatus = 1;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete from material_item_attributes_def ({0})", materialID);
                savestatus = 0;
                throw ex;
            }

            return savestatus;
        }

        public async Task<string[]> Update(Dictionary<string, ModifiedPart> updatedMP, Dictionary<string, ModifiedPart> existingMP, string materialID, string cuid, string publish)
        {
            int savestatus = 0;
            string exmsg = "";
            Dictionary<string, ModifiedPart> insertMP = new Dictionary<string, ModifiedPart>();
            Dictionary<string, ModifiedPart> changeMP = new Dictionary<string, ModifiedPart>();
            MaterialDbInterface MDI = new MaterialDbInterface();

            await Task.Run(() =>
            {
                try
                {
                    foreach (KeyValuePair<string, ModifiedPart> uMP in updatedMP)
                    {
                        bool isChange = false, isNtChange = false;

                        foreach (KeyValuePair<string, ModifiedPart> eMP in existingMP)
                        {
                            if (uMP.Value.Name == eMP.Value.Name)
                            {
                                if (uMP.Value.SValue.ToUpper() == eMP.Value.SValue.ToUpper())
                                {
                                    isNtChange = true;
                                }
                                else
                                {
                                    changeMP.Add(uMP.Key, uMP.Value); isChange = true;
                                }
                            }
                        }

                        if (!isChange && !isNtChange)
                        {
                            insertMP.Add(uMP.Key, uMP.Value);
                        }
                    }

                    foreach (KeyValuePair<string, ModifiedPart> kMP in updatedMP)
                    {
                        existingMP.Remove(kMP.Key);
                    }
                }
                catch (Exception ex)
                {
                    savestatus = 0;
                    exmsg = ex.Message;
                    logger.Error(ex, "Exception while checking updated values with existing value. Material Item. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }

                try
                {
                    StartTransaction();

                    savestatus = UpdatePublish(long.Parse(materialID), publish);

                    if (insertMP.Count > 0 | changeMP.Count > 0 | existingMP.Count > 0)
                    {
                        foreach (KeyValuePair<string, ModifiedPart> iMP in insertMP)
                        {
                            InsertMaterialRO(materialID, iMP.Value.ID, iMP.Value.SValue, cuid);
                        }

                        foreach (KeyValuePair<string, ModifiedPart> uMP in changeMP)
                        {
                            UpdateMaterialRO(long.Parse(materialID), uMP.Value.ID, uMP.Value.SValue, cuid);
                        }

                        foreach (KeyValuePair<string, ModifiedPart> dMP in existingMP)
                        {
                            DeleteMaterialRO(long.Parse(materialID), dMP.Value.ID);
                        }

                        savestatus = 1;
                    }

                    if (savestatus == 1)
                    {
                        //UpdateTime(long.Parse(materialID),cuid);
                        CommitTransaction();
                    }
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    savestatus = 0;
                    exmsg = ex.Message;
                    logger.Error(ex, "Exception while updating Material Item. Message: {0} \\r\\n StackTrace :{1}", ex.Message, ex.StackTrace);
                }
                finally
                {
                    Dispose();
                }
            });

            if (exmsg != "")
            {
                return new string[] { "fail", exmsg };
            }
            else if (savestatus > 0)
            {
                return new string[] { "success", materialID, "0" };
            }
            else
            {
                return new string[] { "No changes made." };
            }
        }
        
        public int UpdateTime(long materialID, string cuid)
        {
            string selsql = @"select VALUE from MATERIAL_ITEM_ATTRIBUTES where MTL_ITEM_ATTRIBUTES_DEF_ID = '163' and material_item_id = :materialItemId";
            int savestatus = 0;
            IDbDataParameter[] parameters = null;
            object isPublished;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);
                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialID, ParameterDirection.Input);
                isPublished = dbAccessor.ExecuteScalar(CommandType.Text, selsql, parameters);

                if (isPublished == null)
                {
                    InsertMaterialRO(materialID.ToString(), "163", DateTime.Now.ToString(), cuid);                    
                    savestatus = 1;
                }
                else
                {
                    UpdateMaterialRO(materialID, "163", DateTime.Now.ToString(), cuid);                   
                    savestatus = 1;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete from material_item_attributes_def ({0})", materialID);
                savestatus = 0;
                throw ex;
            }

            return savestatus;
        }
    }
}