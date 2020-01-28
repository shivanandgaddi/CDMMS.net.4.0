using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;
using System.Text;
using NLog;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class MaterialDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        //private IAccessor dbAccessor = null;
        
        public MaterialDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public MaterialDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        //public void StartTransaction()
        //{
        //    if (dbAccessor == null)
        //        dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

        //    dbAccessor.BeginTransaction();
        //}

        //public void CommitTransaction()
        //{
        //    if (dbAccessor != null)
        //        dbAccessor.CommitTransaction();
        //}

        //public void RollbackTransaction()
        //{
        //    if (dbAccessor != null)
        //        dbAccessor.RollbackTransaction();
        //}

        //public void Dispose()
        //{
        //    if (dbAccessor != null)
        //        dbAccessor.Dispose();
        //}

        //public void DeleteMaterialItemAttributes(long materialItemId, long materialItemDefId)
        //{
        //    string sql = @"DELETE 
        //                    FROM material_item_attributes a
        //                    WHERE a.material_item_id = :materialItemId
        //                    AND a.mtl_item_attributes_def_id = :materialItemDefId";
        //    IDbDataParameter[] parameters = null;

        //    try
        //    {
        //        parameters = dbAccessor.GetParameterArray(2);

        //        parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
        //        parameters[1] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, materialItemDefId, ParameterDirection.Input);

        //        dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to delete from material_item_attributes_def ({0}, {1})", materialItemId, materialItemDefId);

        //        throw ex;
        //    }
        //}

        //public void InsertMaterialItemAttributes(string attributeValue, long materialItemId, string attributeName, string cuid)
        //{
        //    string sql = @"INSERT INTO material_item_attributes (material_item_id, mtl_item_attributes_def_id, value, last_updated_cuid, last_updated_date)
        //                    (SELECT :material_item_id, b.mtl_item_attributes_def_id, :value, :cuid, SYSDATE
        //                    FROM material_item_attributes_def b
        //                    WHERE b.short_name = :short_name)";
        //    IDbDataParameter[] parameters = null;

        //    try
        //    {
        //        parameters = dbAccessor.GetParameterArray(4);

        //        parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
        //        parameters[1] = dbAccessor.GetParameter("value", DbType.String, attributeValue.ToUpper(), ParameterDirection.Input);
        //        parameters[2] = dbAccessor.GetParameter("cuid", DbType.String, cuid, ParameterDirection.Input);
        //        parameters[3] = dbAccessor.GetParameter("short_name", DbType.String, attributeName, ParameterDirection.Input);

        //        dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to insert material_item_attributes_def ({0}, {1}, {2})", attributeValue, materialItemId, attributeName);

        //        throw ex;
        //    }
        //}

        //public void UpdateMaterialItemAttributes(string attributeValue, long materialItemId, long materialItemDefId, string cuid)
        //{
        //    string sql = @"UPDATE material_item_attributes 
        //                    SET value = :value, last_updated_cuid = :cuid, last_updated_date = SYSDATE
        //                    WHERE material_item_id = :material_item_id
        //                    AND mtl_item_attributes_def_id = :mtl_item_attributes_def_id";
        //    IDbDataParameter[] parameters = null;

        //    if (string.IsNullOrEmpty(cuid))
        //        cuid = "cdmms";

        //    try
        //    {
        //        parameters = dbAccessor.GetParameterArray(4);

        //        parameters[0] = dbAccessor.GetParameter("value", DbType.String, attributeValue.ToUpper(), ParameterDirection.Input);
        //        parameters[1] = dbAccessor.GetParameter("cuid", DbType.String, cuid, ParameterDirection.Input);
        //        parameters[2] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
        //        parameters[3] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, materialItemDefId, ParameterDirection.Input);

        //        dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to update material_item_attributes_def ({0}, {1}, {2})", attributeValue, materialItemId, materialItemDefId);

        //        throw ex;
        //    }
        //}

        public async Task<List<MaterialItem>> SearchMaterialItemAllAsync(string PrdctId, string PrtNo, string MtlDesc, string clmc, string cdmmsid, string status, string cableType, string featureType, string itemStatus, 
            string specificationName, string materialCategory, string userid, string heciclei, string standaloneCleiSearch, string exactMatch, string startdt, string enddt)
        {       
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<MaterialItem> mtlItems = null;
            bool hadException = false;
            MaterialType mtType = new MaterialType();
            string hasHeciClei = String.Empty;

            if (string.IsNullOrEmpty(heciclei) || heciclei == "%")
            {
                hasHeciClei = "N";
            }
            else
            {
                hasHeciClei = "Y";
            }

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(19);
                    
                    parameters[0] = dbManager.GetParameter("pPrdId", DbType.String, PrdctId.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPrtNo", DbType.String, PrtNo.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pMtlDesc", DbType.String, MtlDesc.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pMFG", DbType.String, clmc.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pcdmmsid", DbType.String, cdmmsid.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pItmSts", DbType.String, itemStatus.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("pStatus", DbType.String, status.ToUpper(), ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pFtrTyp", DbType.String, featureType.ToUpper(), ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameter("pSpecNm", DbType.String, specificationName.ToUpper(), ParameterDirection.Input);
                    parameters[9] = dbManager.GetParameter("pMtlCtgry", DbType.String, materialCategory.ToUpper(), ParameterDirection.Input);
                    parameters[10] = dbManager.GetParameter("pCblTyp", DbType.String, cableType.ToUpper(), ParameterDirection.Input);                  
                    parameters[11] = dbManager.GetParameter("pLstCuid", DbType.String, userid.ToUpper(), ParameterDirection.Input);
                    parameters[12] = dbManager.GetParameter("pHeciClei", DbType.String, heciclei.ToUpper(), ParameterDirection.Input);
                    parameters[13] = dbManager.GetParameter("pHasHeciClei", DbType.String, hasHeciClei, ParameterDirection.Input);
                    parameters[14] = dbManager.GetParameter("pStandaloneClei", DbType.String, standaloneCleiSearch, ParameterDirection.Input);
                    parameters[15] = dbManager.GetParameter("pExactMatch", DbType.String, exactMatch, ParameterDirection.Input);
                    parameters[16] = dbManager.GetParameter("pStartDt", DbType.String, startdt, ParameterDirection.Input);
                    parameters[17] = dbManager.GetParameter("pEndDt", DbType.String, enddt, ParameterDirection.Input);
                    parameters[18] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_MATERIAL_ALL_NEW", parameters);

                    while (reader.Read())
                    {
                        if (mtlItems == null)
                            mtlItems = new List<MaterialItem>();

                        MaterialItem mtlItem = new MaterialItem();

                        mtlItem.Attributes.Add("material_item_id", new Models.Attribute("MATERIAL_ITEM_ID", reader["MATERIAL_ITEM_ID"].ToString()));
                        // mford - the next line is odd but necessary as a placeholder
                        mtlItem.Attributes.Add("cin_cdmms_Id", new Models.Attribute("CIN_CDMMS_ID", reader["MATERIAL_ITEM_ID"].ToString()));
                        mtlItem.Attributes.Add("mtrl_id", new Models.Attribute("MTRL_ID", DataReaderHelper.GetNonNullValue(reader, "MTRL_ID", true)));
                        mtlItem.Attributes.Add("product_id", new Models.Attribute("PRODUCT_ID", reader["PRODUCT_ID"].ToString()));
                        mtlItem.Attributes.Add("mfg_part_no", new Models.Attribute("MFG_PART_NO", reader["MFG_PART_NO"].ToString()));
                        mtlItem.Attributes.Add("item_desc", new Models.Attribute("ITEM_DESC", reader["ITEM_DESC"].ToString()));
                        mtlItem.Attributes.Add("mfg_id", new Models.Attribute("MFG_ID", reader["MFG_ID"].ToString()));
                        mtlItem.Attributes.Add("item_current_status", new Models.Attribute("ITEM_CURRENT_STATUS", reader["ITEM_CURRENT_STATUS"].ToString()));
                        mtlItem.Attributes.Add("ftr_typ", new Models.Attribute("FTR_TYP", DataReaderHelper.GetNonNullValue(reader, "FTR_TYP")));
                        mtlItem.Attributes.Add("cbl_typ", new Models.Attribute("CABL_TYP", DataReaderHelper.GetNonNullValue(reader, "CABL_TYP")));
                        mtlItem.Attributes.Add("spec_nm", new Models.Attribute("SPEC_NM", DataReaderHelper.GetNonNullValue(reader, "SPEC_NM")));
                        mtlItem.Attributes.Add("stts", new Models.Attribute("STTS", DataReaderHelper.GetNonNullValue(reader, "STTS")));
                        mtlItem.Attributes.Add("mtl_ctgry", new Models.Attribute("MTL_CTGRY", DataReaderHelper.GetNonNullValue(reader, "MTL_CTGRY")));
                        mtlItem.Attributes.Add("lastupdt", new Models.Attribute("lastupdt", DataReaderHelper.GetNonNullValue(reader, "LSTDT")));
                        mtlItem.Attributes.Add("userid", new Models.Attribute("userid", DataReaderHelper.GetNonNullValue(reader, "LSTCUID")));

                        mtlItems.Add(mtlItem);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search";

                    hadException = true;

                    logger.Error(oe, message);
                    EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
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
            });

            if (hadException)
                throw new Exception();

            return mtlItems;
        }

        public async Task<List<MaterialItem>> SearchMaterialItemAllRONewAsync(string PrdctId, string PrtNo, string MtlDesc,
            string clmc, string cdmmsid, string status, string cableType, string featureType,
            string specificationName, string materialCategory, string lastupdt, string userid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<MaterialItem> mtlItems = null;
            bool hadException = false;
            MaterialType mtType = new MaterialType();
            string hasHeciClei = String.Empty;
            StringCollection dups = new StringCollection();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(13);

                    parameters[0] = dbManager.GetParameter("pProdId", DbType.String, PrdctId.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPartNo", DbType.String, PrtNo.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pItemDesc", DbType.String, MtlDesc.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pClmc", DbType.String, clmc.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pCDMMSID", DbType.String, cdmmsid.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pStatus", DbType.String, status, ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("pCatType", DbType.String, materialCategory, ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pFeatType", DbType.String, featureType, ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameter("pCableType", DbType.String, cableType, ParameterDirection.Input);
                    parameters[9] = dbManager.GetParameter("pSpecName", DbType.String, specificationName.ToUpper(), ParameterDirection.Input);
                    parameters[10] = dbManager.GetParameter("pLastCuid", DbType.String, userid.ToUpper(), ParameterDirection.Input);
                    parameters[11] = dbManager.GetParameter("pLastUpdateDate", DbType.String, lastupdt, ParameterDirection.Input);
                    parameters[12] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_MATERIAL_RO_NEW", parameters);

                    while (reader.Read())
                    {
                        if (mtlItems == null)
                            mtlItems = new List<MaterialItem>();

                        MaterialItem mtlItem = new MaterialItem();

                        mtlItem.Attributes.Add("material_item_id", new Models.Attribute("MATERIAL_ITEM_ID", reader["MATERIAL_ITEM_ID"].ToString()));
                        // mford - the next line is odd but necessary as a placeholder
                        mtlItem.Attributes.Add("cin_cdmms_Id", new Models.Attribute("CIN_CDMMS_ID", reader["MATERIAL_ITEM_ID"].ToString()));
                        mtlItem.Attributes.Add("mtrl_id", new Models.Attribute("MTRL_ID", DataReaderHelper.GetNonNullValue(reader, "MTRL_ID", true)));
                        mtlItem.Attributes.Add("product_id", new Models.Attribute("PRODUCT_ID", reader["PRODUCT_ID"].ToString()));
                        mtlItem.Attributes.Add("mfg_part_no", new Models.Attribute("MFG_PART_NO", reader["MFG_PART_NO"].ToString()));
                        mtlItem.Attributes.Add("item_desc", new Models.Attribute("ITEM_DESC", reader["ITEM_DESC"].ToString()));
                        mtlItem.Attributes.Add("mfr_cd", new Models.Attribute("MFR_CD", reader["MFR_CD"].ToString()));
                        mtlItem.Attributes.Add("ftr_typ", new Models.Attribute("FTR_TYP", DataReaderHelper.GetNonNullValue(reader, "FTR_TYP")));
                        mtlItem.Attributes.Add("cbl_typ", new Models.Attribute("CABL_TYP", DataReaderHelper.GetNonNullValue(reader, "CBL_TYP")));
                        mtlItem.Attributes.Add("spec_nm", new Models.Attribute("SPEC_NM", DataReaderHelper.GetNonNullValue(reader, "SPEC_NM")));
                        mtlItem.Attributes.Add("stts", new Models.Attribute("STTS", DataReaderHelper.GetNonNullValue(reader, "STTS")));
                        mtlItem.Attributes.Add("mtl_ctgry", new Models.Attribute("MTL_CTGRY", DataReaderHelper.GetNonNullValue(reader, "MTL_CTGRY")));
                        mtlItem.Attributes.Add("lastupdt", new Models.Attribute("lastupdt", DataReaderHelper.GetNonNullValue(reader, "LSTDT")));
                        mtlItem.Attributes.Add("userid", new Models.Attribute("userid", DataReaderHelper.GetNonNullValue(reader, "LSTCUID")));

                        if (!dups.Contains(reader["MATERIAL_ITEM_ID"].ToString()))
                        {
                            mtlItems.Add(mtlItem);
                            dups.Add(reader["MATERIAL_ITEM_ID"].ToString());
                        }
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search";

                    hadException = true;

                    logger.Error(oe, message);
                    EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
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
            });

            if (hadException)
                throw new Exception();

            return mtlItems;
        }

        public async Task<List<SearchResult>> SearchMaterialItemAsync(string attributeToSearch, string searchValue)
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

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pAttr", DbType.String, attributeToSearch, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pValue", DbType.String, searchValue.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_MATERIAL", parameters);

                    while (reader.Read())
                    {
                        if (results == null)
                            results = new List<SearchResult>();

                        SearchResult result = new SearchResult();

                        result.ItemValue = reader["material_item_id"].ToString();
                        result.DisplayValue = reader["item_desc"].ToString();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}";

                    hadException = true;

                    logger.Error(oe, message, attributeToSearch, searchValue);
                    EventLogger.LogAlarm(oe, string.Format(message, attributeToSearch, searchValue), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search: {0}, {1}", attributeToSearch, searchValue);
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

        public async Task<MaterialItem> GetMaterialItemSAPAsync(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialItem material = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_SAP_MATERIAL", parameters);

                    if (reader.Read())
                    {
                        material = new MaterialItem(materialItemId);

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.SAPMaterial)
                        {
                            if (MaterialType.JSON.HzrdInd.Equals(keyValue.Key))
                            {
                                string val = DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber);

                                if ("X".Equals(val))
                                    material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, true, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                                else
                                    material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, false, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                            }
                            else
                                material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve SAP material: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve SAP material: {0}", materialItemId);
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

            return material;
        }

        public async Task<MaterialItem> GetMaterialItemRecordOnlyAsync(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialItem material = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_RECORD_ONLY_ATTRIBUTES", parameters);

                    while (reader.Read())
                    {
                        string name = DataReaderHelper.GetNonNullValue(reader, "short_name");

                        if(material == null)
                            material = new MaterialItem(materialItemId);

                        if (MaterialType.JSON.HzrdInd.Equals(name))
                        {
                            material.Attributes.Add(name, new Models.Attribute(name, DataReaderHelper.GetNonNullValue(reader, "value") == "Y" ? true : false));
                        }
                        else
                            material.Attributes.Add(name, new Models.Attribute(name, DataReaderHelper.GetNonNullValue(reader, "value")));
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve RO material: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve RO material: {0}", materialItemId);
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

            return material;
        }

        public async Task<MaterialItem> GetMaterialItemSAPAsync(string productId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialItem material = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pPrdctId", DbType.String, productId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_SAP_MATERIAL_BY_PRDCT_ID", parameters);

                    if (reader.Read())
                    {
                        material = new MaterialItem();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.SAPMaterial)
                        {
                            if (MaterialType.JSON.HzrdInd.Equals(keyValue.Key))
                            {
                                string val = DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber);

                                if ("X".Equals(val))
                                    material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, true, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                                else
                                    material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, false, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                            }
                            else
                                material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.SAP)));
                        }
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve SAP material: {0}";

                    hadException = true;

                    logger.Error(oe, message, productId);
                    EventLogger.LogAlarm(oe, string.Format(message, productId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve SAP material: {0}", productId);
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

            return material;
        }

        public async Task<MaterialItem> GetMaterialItemLOSDBAsync(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialItem material = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_LOSDB_MATERIAL", parameters);

                    if (reader.Read())
                    {
                        material = new MaterialItem(materialItemId);

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.LOSDBMaterial)
                        {
                            material.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                        }
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve LOSDB material: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve LOSDB material: {0}", materialItemId);
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

            return material;
        }

        public List<MaterialItem> GetExistingRootParts(string partNumber)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            List<MaterialItem> rootParts = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pPrtNmbr", DbType.String, partNumber, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MTRL_PKG.GET_EXISTING_ROOT_PART_NMBRS", parameters);

                while (reader.Read())
                {
                    MaterialItem rootPart = new MaterialItem();

                    Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                    Models.Attribute mtrlId = new Models.Attribute("mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                    Models.Attribute catalogDescription = new Models.Attribute("item_desc", DataReaderHelper.GetNonNullValue(reader, "item_desc"));
                    Models.Attribute manufacturer = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                    Models.Attribute prtNbr = new Models.Attribute("part_no", DataReaderHelper.GetNonNullValue(reader, "part_no"));
                    Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "product_id"));
                    Models.Attribute rvsn = new Models.Attribute("rvsn", DataReaderHelper.GetNonNullValue(reader, "revsn_no"));

                    rootPart.Attributes.Add(id.Name, id);
                    rootPart.Attributes.Add(mtrlId.Name, mtrlId);
                    rootPart.Attributes.Add(catalogDescription.Name, catalogDescription);
                    rootPart.Attributes.Add(manufacturer.Name, manufacturer);
                    rootPart.Attributes.Add(prtNbr.Name, prtNbr);
                    rootPart.Attributes.Add(materialCode.Name, materialCode);
                    rootPart.Attributes.Add(rvsn.Name, rvsn);

                    if (rootParts == null)
                        rootParts = new List<MaterialItem>();

                    rootParts.Add(rootPart);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get existing root part ({0})", partNumber);

                throw ex;
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

            return rootParts;
        }

        public async Task<string> GetExistingRootPartsRO(string partNumber, string clmc)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            string returnString = String.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pPrtNmbr", DbType.String, partNumber, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pMfrCd", DbType.String, clmc, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MTRL_PKG.GET_EXISTING_ROOT_PART_NMBRSRO", parameters);

                    if (reader.Read())
                    {
                        returnString = reader["id"].ToString() + "~" + reader["record_only"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get existing root part ({0})", partNumber);

                    throw ex;
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

            return returnString;
        }

        public string[] GetProductId(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string productId = "";
            string publishedIndicator = "N";
            string[] results = null;

            try
            {
                //PROCEDURE GET_PRODUCT_ID(PMTLITMID IN MATERIAL_ITEM.MATERIAL_ITEM_ID%TYPE, oPrdctId OUT VARCHAR2)
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("PMTLITMID", DbType.Int64, materialItemId, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("oPrdctId", DbType.String, productId, ParameterDirection.Output, 4000);
                parameters[2] = dbManager.GetParameter("oRoPblshd", DbType.String, productId, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("MATERIAL_PKG.GET_PRODUCT_ID", parameters);

                productId = parameters[1].Value.ToString();
                publishedIndicator = parameters[2].Value.ToString();

                results = new string[2];

                results[0] = productId;
                results[1] = publishedIndicator;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GetProductId: {0}; ", materialItemId);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return results;
        }

        public string GetMaterialSourceOfRecord(long materialItemId)
        {
            string aliasVal = string.Empty;
            string returnValue = string.Empty;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_SOURCE_OF_RECORD", parameters);

                if (reader.Read())
                {
                    aliasVal = DataReaderHelper.GetNonNullValue(reader, "alias_val");
                    if (aliasVal != String.Empty)
                    {
                        returnValue = "LOSDB";
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get material source of record: {0}";

                hadException = true;

                logger.Error(oe, message, materialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to get material source of record: {0}", materialItemId);
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

            if (hadException)
                throw new Exception();

            return returnValue;
        }

        public async Task<List<Models.Attribute>> GetAdditionalAttributesAsync(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Models.Attribute> attributes = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;
            ReferenceDbInterface refDbInterface = new ReferenceDbInterface();

            await Task.Run(async () =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_ADDITIONAL_ATTRIBUTES", parameters);

                    while (reader.Read())
                    {
                        Models.Attribute attr = null;
                        string name = DataReaderHelper.GetNonNullValue(reader, "short_name");
                        string value = DataReaderHelper.GetNonNullValue(reader, "value").Trim();
                        string source = DataReaderHelper.GetNonNullValue(reader, "application");
                        string sql = DataReaderHelper.GetNonNullValue(reader, "sql");

                        if (attributes == null)
                            attributes = new List<Models.Attribute>();
                      
                        attr = new Models.Attribute(name, value, source);

                        attr.IspOrOsp = DataReaderHelper.GetNonNullValue(reader, "isp_or_osp_ind");
                        attr.MaterialItemAttributesDefId = DataReaderHelper.GetNonNullValue(reader, "mtl_item_attributes_def_id");

                        if (!string.IsNullOrEmpty(sql))
                        {
                            IDbDataParameter[] optionParameters = null;

                            if (name.Equals(MaterialType.JSON.LbrId))
                            {
                                optionParameters = dbManager.GetParameterArray(2);

                                optionParameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                                optionParameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                                attr.Options = await refDbInterface.GetListOptionsSP(sql, optionParameters);
                            }
                            else
                              attr.Options = await refDbInterface.GetListOptions(sql);
                        }
                        
                        attributes.Add(attr);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get additional attributes: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get additional attributes: {0}", materialItemId);
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

            return attributes;
        }

        public async Task<List<Models.Attribute>> GetAttributeOverridesAsync(long materialItemId, string applicationName)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Models.Attribute> attributes = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pAppl", DbType.String, applicationName, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_ATTRIBUTE_OVERRIDES", parameters);

                    while (reader.Read())
                    {
                        Models.Attribute attr = null;
                        string name = DataReaderHelper.GetNonNullValue(reader, "short_name");
                        string value = DataReaderHelper.GetNonNullValue(reader, "value");
                        string defId = DataReaderHelper.GetNonNullValue(reader, "mtl_item_attributes_def_id");

                        if (attributes == null)
                            attributes = new List<Models.Attribute>();

                        attr = new Models.Attribute(name, value, MaterialType.JSON.OVERRIDE);

                        attr.MaterialItemAttributesDefId = defId;

                        if (MaterialType.JSON.HzrdInd.Equals(name) && "X".Equals(value))
                            attr.BoolValue = true;

                        attributes.Add(attr);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get override attributes: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to get override attributes: {0}", materialItemId);
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

            return attributes;
        }

        public List<string> GetAdditionalAttributeNames()
        {
            List<string> names = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_ADDITIONAL_ATTRIBUTE_NAMES", parameters);

                while (reader.Read())
                {
                    string name = DataReaderHelper.GetNonNullValue(reader, "short_name");

                    if (names == null)
                        names = new List<string>();

                    names.Add(name);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get attribute names";

                logger.Error(oe, message);
                EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get attribute names");
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

            return names;
        }

        public Dictionary<string, Models.Attribute> GetAdditionalAttributes()
        {
            Dictionary<string, Models.Attribute> attributes = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_ADDITIONAL_ATTRIBUTE_NAMES", parameters);

                while (reader.Read())
                {
                    string name = DataReaderHelper.GetNonNullValue(reader, "short_name");

                    if (attributes == null)
                        attributes = new Dictionary<string, Models.Attribute>();

                    attributes.Add(name, new Models.Attribute(true, name));
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get attribute names";

                logger.Error(oe, message);
                EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get attribute names");
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

            return attributes;
        }

        //public long InsertWorkToDo(long materialItemId)
        //{
        //    IDbDataParameter[] parameters = null;
        //    bool dispose = false;
        //    long workToDoId = 0;

        //    try
        //    {
        //        if (dbAccessor == null)
        //        {
        //            dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

        //            dispose = true;
        //        }

        //        parameters = dbAccessor.GetParameterArray(2);

        //        parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
        //        parameters[1] = dbAccessor.GetParameter("work_to_do_id", DbType.Int64, workToDoId, ParameterDirection.Output);

        //        dbAccessor.ExecuteNonQuerySP("MATERIAL_PKG.INSERT_WORK_TO_DO", parameters);

        //        workToDoId = long.Parse(parameters[1].Value.ToString());

        //        return workToDoId;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to update work_to_do ({0})", materialItemId);

        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (dispose && dbAccessor != null)
        //            dbAccessor.Dispose();
        //    }
        //}

        public async Task<NameValueCollection> GetSendToNdsStatus(long workToDoId, string workType)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            NameValueCollection status = new NameValueCollection();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("wrkId", DbType.Int32, workToDoId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("wrktyp", DbType.String, workType, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("NEISL_PKG.GET_CHANGE_SET_STATUS", parameters);

                    if (reader.Read())
                    {
                        string stts = DataReaderHelper.GetNonNullValue(reader, "status");
                        string notes = DataReaderHelper.GetNonNullValue(reader, "notes");

                        status.Add("status", stts);
                        status.Add("notes", notes);
                    }
                    else
                        status.Add("status", Constants.ChangeSetStatus(CHANGE_SET_STATUS.PROCESSING));
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve NDS status: {0}";

                    status.Add("status", "EXCEPTION");

                    logger.Error(oe, message, workToDoId);
                    EventLogger.LogAlarm(oe, string.Format(message, workToDoId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    status.Add("status", "EXCEPTION");

                    logger.Error(ex, "Unable to retrieve NDS status: {0}", workToDoId);
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

            return status;
        }

        public async Task<string> GetHighLevelPartSendToNdsStatus(long workToDoId, string workType)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            NameValueCollection statusCollection = new NameValueCollection();
            StringBuilder sb = new StringBuilder();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("wrkId", DbType.Int32, workToDoId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("wrktyp", DbType.String, workType, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("NEISL_PKG.GET_HLP_CHANGE_SET_STATUS", parameters);

                    while (reader.Read())
                    {
                        string notes = DataReaderHelper.GetNonNullValue(reader, "notes");
                        string status = DataReaderHelper.GetNonNullValue(reader, "status");
                        string changeSetId = DataReaderHelper.GetNonNullValue(reader, "change_set_id");

                        sb.Append("Request ID " + changeSetId + ": " + status + " - " + notes);
                        sb.Append("; ");
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve NDS status: {0}";

                    statusCollection.Add("status", "EXCEPTION");

                    logger.Error(oe, message, workToDoId);
                    EventLogger.LogAlarm(oe, string.Format(message, workToDoId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    statusCollection.Add("status", "EXCEPTION");

                    logger.Error(ex, "Unable to retrieve NDS status: {0}", workToDoId);
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

            return sb.ToString();
        }

        public async Task<NameValueCollection> GetMaterialIdCategoryAndFeatureTypesAsync(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            NameValueCollection values = null;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItemId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.GET_MTRL_CAT_AND_FEAT_TYPES", parameters);

                    if (reader.Read())
                    {
                        values = new NameValueCollection();

                        values.Add("mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        values.Add("mtrl_cat_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cat_id", true));
                        values.Add("feat_typ_id", DataReaderHelper.GetNonNullValue(reader, "feat_typ_id", true));
                        values.Add("rcrds_only_ind", DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind"));
                        values.Add("record_only_ind", DataReaderHelper.GetNonNullValue(reader, "record_only_ind"));
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve material id: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialItemId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve material id: {0}", materialItemId);
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

            return values;
        }

        public async Task<string> UpdateReplacementAssociation(long parentId, long childId, string comments)
        {
            IDbDataParameter[] parameters = null;
            string status = "Success";

            await Task.Run(() =>
            {
                try
                {
                    parameters = dbAccessor.GetParameterArray(3);

                    parameters[0] = dbAccessor.GetParameter("pParentId", DbType.String, parentId, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pChildId", DbType.Int64, childId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pCmnt", DbType.String, comments, ParameterDirection.Input);

                    dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MTRL_PKG.UPDATE_REPLACEMENT_ALT_MTRL", parameters);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to update alt_mtrl ({0}, {1}, {2})", parentId, childId, comments);

                    throw ex;
                }
            });

            return status;
        }

        public async Task<string> UpdateChainingAssociation(long parentId, long childId, string comments)
        {
            IDbDataParameter[] parameters = null;
            string status = "Success";

            await Task.Run(() =>
            {
                try
                {
                    parameters = dbAccessor.GetParameterArray(3);

                    parameters[0] = dbAccessor.GetParameter("pParentId", DbType.String, parentId, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pChildId", DbType.Int64, childId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pCmnt", DbType.String, comments, ParameterDirection.Input);

                    dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MTRL_PKG.UPDATE_CHAINING_ALT_MTRL", parameters);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to update alt_mtrl ({0}, {1}, {2})", parentId, childId, comments);

                    throw ex;
                }
            });

            return status;
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            return null;
        }

        public async Task<List<MaterialItem>> SearchLosdb(string partno, string cleicd, string itemdesc, string clei7, string mfrcd)
        {
            List<MaterialItem> items = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(6);

                    parameters[0] = dbManager.GetParameter("pPartno", DbType.String, partno.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pCleicd", DbType.String, cleicd.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pItemdesc", DbType.String, itemdesc.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pClei7", DbType.String, clei7.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pMfrcd", DbType.String, mfrcd.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameterCursorType("RETCSR", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_PKG.SEARCH_LOSDB", parameters);

                    while (reader.Read())
                    {
                        if (items == null)
                            items = new List<MaterialItem>();

                        MaterialItem losdbItem = new MaterialItem();

                        Models.Attribute part = new Models.Attribute("part_no_losdb", DataReaderHelper.GetNonNullValue(reader, "prt_no"));
                        Models.Attribute clmc = new Models.Attribute("clmc_losdb", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute cleicdd = new Models.Attribute("Clei_losdb", DataReaderHelper.GetNonNullValue(reader, "clei_cd"));
                        Models.Attribute compatibleclei = new Models.Attribute("compatible_losdb", DataReaderHelper.GetNonNullValue(reader, "clei7"));
                        Models.Attribute itemdescd = new Models.Attribute("desc_losdb", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                        Models.Attribute matid = new Models.Attribute("prod_id", DataReaderHelper.GetNonNullValue(reader, "prod_id"));
                        Models.Attribute eqptId = new Models.Attribute("eqpt_id", DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id"));

                        losdbItem.Attributes.Add(part.Name, part);
                        losdbItem.Attributes.Add(clmc.Name, clmc);
                        losdbItem.Attributes.Add(cleicdd.Name, cleicdd);
                        losdbItem.Attributes.Add(compatibleclei.Name, compatibleclei);
                        losdbItem.Attributes.Add(itemdescd.Name, itemdescd);
                        losdbItem.Attributes.Add(matid.Name, matid);
                        losdbItem.Attributes.Add(eqptId.Name, eqptId);

                        items.Add(losdbItem);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to search for LOSDB material to associate.";

                    //hadException = true;

                    logger.Error(oe, message);
                    //EventLogger.LogAlarm(oe, string.Format(message,materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //hadException = true;

                    logger.Error(ex, "Unable to search for LOSDB material to associate.");
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

            return items;
        }

        public async Task<List<MaterialItem>> GetAssociatedLOSDBMaterial(long materialItemId)
        {
            List<MaterialItem> items = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("RETCSR", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_LOSDB_ASSOCIATED_MATERIAL", parameters);

                    while (reader.Read())
                    {
                        if (items == null)
                            items = new List<MaterialItem>();

                        MaterialItem losdbItem = new MaterialItem();

                        Models.Attribute part = new Models.Attribute("part_no_losdb", DataReaderHelper.GetNonNullValue(reader, "prt_no"));
                        Models.Attribute clmc = new Models.Attribute("clmc_losdb", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute cleicdd = new Models.Attribute("Clei_losdb", DataReaderHelper.GetNonNullValue(reader, "clei_cd"));
                        Models.Attribute compatibleclei = new Models.Attribute("compatible_losdb", DataReaderHelper.GetNonNullValue(reader, "clei7"));
                        Models.Attribute itemdescd = new Models.Attribute("desc_losdb", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                        Models.Attribute matid = new Models.Attribute("prod_id", DataReaderHelper.GetNonNullValue(reader, "prod_id"));
                        Models.Attribute eqptId = new Models.Attribute("eqpt_id", DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id"));

                        losdbItem.Attributes.Add(part.Name, part);
                        losdbItem.Attributes.Add(clmc.Name, clmc);
                        losdbItem.Attributes.Add(cleicdd.Name, cleicdd);
                        losdbItem.Attributes.Add(compatibleclei.Name, compatibleclei);
                        losdbItem.Attributes.Add(itemdescd.Name, itemdescd);
                        losdbItem.Attributes.Add(matid.Name, matid);
                        losdbItem.Attributes.Add(eqptId.Name, eqptId);

                        items.Add(losdbItem);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve associated LOSDB material.";

                    //hadException = true;

                    logger.Error(oe, message);
                    //EventLogger.LogAlarm(oe, string.Format(message,materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //hadException = true;

                    logger.Error(ex, "Unable to retrieve associated LOSDB material.");
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

            return items;
        }

        public async Task<List<MaterialItem>> SearchMaterialRevisions(long materialId, string materialItemId, string productId, string clmc, string partNumber, string description)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<MaterialItem> results = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(7);

                    //PROCEDURE SEARCH(pMtrlId IN NUMBER, pMtrlCd IN VARCHAR2, pMfrCd IN VARCHAR2, pPrtNo IN VARCHAR2, pMtrlDsc IN VARCHAR2, pId IN VARCHAR2, retcsr OUT ref_cursor)
                    parameters[0] = dbManager.GetParameter("pMtrlId", DbType.String, materialId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pMtrlCd", DbType.String, productId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pMfrCd", DbType.String, clmc.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pPrtNo", DbType.String, partNumber.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pMtrlDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pId", DbType.String, materialItemId, ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("MATERIAL_REVISIONS_PKG.SEARCH", parameters);

                    while (reader.Read())
                    {
                        MaterialItem result = new MaterialItem();

                        //SELECT mi.material_item_id, m.mtrl_id, mfr.mfr_cd, s.product_id, m.rt_part_no, s.mfg_part_no, nvl(m.mtrl_dsc, s.item_desc) mtrl_dsc
                        Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                        Models.Attribute mtrlId = new Models.Attribute("mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                        Models.Attribute catalogDescription = new Models.Attribute("item_desc", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                        Models.Attribute manufacturer = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute mfgPartNumber = new Models.Attribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "mfg_part_no"));
                        Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "product_id"));
                        Models.Attribute rootPartNumber = new Models.Attribute("rt_part_no", DataReaderHelper.GetNonNullValue(reader, "rt_part_no"));
                        Models.Attribute revisionNumber = new Models.Attribute("rvsn", DataReaderHelper.GetNonNullValue(reader, "revsn_no"));

                        result.Attributes.Add(id.Name, id);
                        result.Attributes.Add(mtrlId.Name, mtrlId);
                        result.Attributes.Add(catalogDescription.Name, catalogDescription);
                        result.Attributes.Add(manufacturer.Name, manufacturer);
                        result.Attributes.Add(mfgPartNumber.Name, mfgPartNumber);
                        result.Attributes.Add(materialCode.Name, materialCode);
                        result.Attributes.Add(rootPartNumber.Name, rootPartNumber);
                        result.Attributes.Add(revisionNumber.Name, revisionNumber);

                        if (results == null)
                            results = new List<MaterialItem>();

                        results.Add(result);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve revision search results: {0}";

                    hadException = true;

                    logger.Error(oe, message, materialId);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to retrieve revision search results: {0}", materialId);
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

        public async Task<string> MaterialRevisionExists(long materialId, string revisionNumber)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string exists = "N";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(3);

                    //PROCEDURE REVISION_NUMBER_EXISTS(pMtrlId IN NUMBER, pRvsnNbr IN VARCHAR2, oExists OUT VARCHAR2)
                    parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int64, materialId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pRvsnNbr", DbType.String, revisionNumber, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("oExists", DbType.String, exists, ParameterDirection.Output, 1);

                    dbManager.ExecuteScalarSP("MATERIAL_REVISIONS_PKG.REVISION_NUMBER_EXISTS", parameters);

                    exists = parameters[2].Value.ToString();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to verify revision exists: {0}, {1}";

                    logger.Error(oe, message, materialId, revisionNumber);
                    EventLogger.LogAlarm(oe, string.Format(message, materialId, revisionNumber), SentryIdentifier.EmailDev, SentrySeverity.Major);

                    throw oe;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to verify revision exists: {0}, {1}", materialId, revisionNumber);

                    throw ex;
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return exists;
        }

        public async Task<string> RootPartNumberExists(string partNumber, long manufacturerId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            string exists = "N";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(3);

                    //PROCEDURE ROOT_PART_NUMBER_EXISTS(pPrtNbr IN VARCHAR2, pMfrId IN NUMBER, oExists OUT VARCHAR2)
                    parameters[0] = dbManager.GetParameter("pPrtNbr", DbType.String, partNumber, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pMfrId", DbType.Int64, manufacturerId, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("oExists", DbType.String, exists, ParameterDirection.Output, 1);

                    dbManager.ExecuteScalarSP("MTRL_PKG.ROOT_PART_NUMBER_EXISTS", parameters);

                    exists = parameters[2].Value.ToString();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to verify root part number exists: {0}, {1}";

                    logger.Error(oe, message, partNumber, manufacturerId);
                    EventLogger.LogAlarm(oe, string.Format(message, partNumber, manufacturerId), SentryIdentifier.EmailDev, SentrySeverity.Major);

                    throw oe;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to verify root part number exists: {0}, {1}", partNumber, manufacturerId);

                    throw ex;
                }
                finally
                {
                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            return exists;
        }

        public async Task<string> DeleteLOSDBAssociations(long materialItemId, string mtrlId, string revisionTableName, string aliasTableName)
        {
            string status = "SUCCESS";
            string sql = "";
            string rmeSql = "";

            await Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(revisionTableName))
                    {
                        rmeSql = @"DELETE 
                                    FROM " + aliasTableName + @" 
                                    WHERE " + revisionTableName + "_id = " + materialItemId + @"
                                    AND rme_mtrl_revsn_alias_id IN (SELECT a.rme_mtrl_revsn_alias_id
                                    FROM rme_mtrl_revsn_alias a, appl ap
                                    WHERE a.appl_id = ap.appl_id
                                    AND ap.appl_nm = 'LOSDB')";

                        dbAccessor.ExecuteNonQuery(CommandType.Text, rmeSql);
                    }

                    sql = @"DELETE 
                            FROM mtrl_alias_val
                            WHERE mtrl_id = " + mtrlId + @"
                            AND mtrl_alias_id IN (SELECT ma.mtrl_alias_id
                            FROM mtrl_alias ma, appl ap
                            WHERE ma.appl_id = ap.appl_id
                            AND ap.appl_nm = 'LOSDB')";

                    dbAccessor.ExecuteNonQuery(CommandType.Text, sql);
                }
                catch (OracleException oe)
                {
                    string message = "Unable to delete LOSDB material association: {0}, {1}, {2}, {3}";

                    status = "ERROR";

                    logger.Error(oe, message, materialItemId, mtrlId, revisionTableName, aliasTableName);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId, mtrlId, revisionTableName, aliasTableName), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    status = "ERROR";

                    logger.Error(ex, "Unable to delete LOSDB material association: {0}, {1}, {2}, {3}", materialItemId, mtrlId, revisionTableName, aliasTableName);
                }
            });

            return status;
        }

        public async Task<string> UpdateMaterialRevisions(long materialItemId, long currentMtrlId, long updatedMtrlId, string revisionNumber, 
            string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd)
        {
            string status = "SUCCESS";
            IDbDataParameter[] parameters = null;

            await Task.Run(() =>
            {
                try
                {
                    parameters = dbAccessor.GetParameterArray(8);

                    //PROCEDURE UPDATE_REVISION_DATA(pMtlItmId IN NUMBER, pOldMtrlId IN NUMBER, pNewMtrlId IN NUMBER, pRvsnNo IN VARCHAR2, 
                    //pBaseRvsnInd IN VARCHAR2, pCurrRvsnInd IN VARCHAR2, pRetRvsnInd IN VARCHAR2, oStatus OUT VARCHAR2)
                    parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pOldMtrlId", DbType.Int64, currentMtrlId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pNewMtrlId", DbType.Int64, updatedMtrlId, ParameterDirection.Input);
                    parameters[3] = dbAccessor.GetParameter("pRvsnNo", DbType.String, revisionNumber, ParameterDirection.Input);
                    parameters[4] = dbAccessor.GetParameter("pBaseRvsnInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                    parameters[5] = dbAccessor.GetParameter("pCurrRvsnInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                    parameters[6] = dbAccessor.GetParameter("pRetRvsnInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                    parameters[7] = dbAccessor.GetParameter("oStatus", DbType.String, status, ParameterDirection.Output, 10);

                    dbAccessor.ExecuteNonQuerySP("MATERIAL_REVISIONS_PKG.UPDATE_REVISION_DATA", parameters);

                    status = parameters[7].Value.ToString();
                }
                catch (OracleException oe)
                {
                    string message = "Unable to update revision data: {0}, {1}, {2}, {3}, {4}, {5}, {6}";

                    status = "ERROR";

                    logger.Error(oe, message, materialItemId, currentMtrlId, updatedMtrlId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd);
                    EventLogger.LogAlarm(oe, string.Format(message, materialItemId, currentMtrlId, updatedMtrlId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    status = "ERROR";
                    
                    logger.Error(ex, "Unable to update revision data: {0}, {1}, {2}, {3}, {4}, {5}, {6}", materialItemId, currentMtrlId, updatedMtrlId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd);
                }
            });

            return status;
        }

        public LOSDBMaterialManager.KeyCollection GetLOSDBMaterialKeys(long materialItemId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            bool hadException = false;
            LOSDBMaterialManager.KeyCollection keys = null;
            string cleiKey = "";
            string electricalKey = "";
            string prodId = "";
            long parsedValue = 0;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_LOSDB_KEYS", parameters);

                if (reader.Read())
                {
                    keys = new LOSDBMaterialManager.KeyCollection();

                    keys.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    keys.CLEICode = DataReaderHelper.GetNonNullValue(reader, "cleicode");
                    cleiKey = DataReaderHelper.GetNonNullValue(reader, "comp_clei_key");
                    electricalKey = DataReaderHelper.GetNonNullValue(reader, "electrical_key");
                    prodId = DataReaderHelper.GetNonNullValue(reader, "prod_id");

                    if (!string.IsNullOrEmpty(cleiKey))
                    {
                        if (long.TryParse(cleiKey, out parsedValue))
                            keys.CLEIKey = parsedValue;
                        else
                            logger.Error("Value for comp_clei_key is not a numeric value [{0}]. Material item id: {1}", cleiKey, materialItemId);
                    }

                    parsedValue = 0;

                    if (!string.IsNullOrEmpty(electricalKey))
                    {
                        if (long.TryParse(electricalKey, out parsedValue))
                            keys.ElectricalKey = parsedValue;
                        else
                            logger.Error("Value for electrical_key is not a numeric value [{0}]. Material item id: {1}", electricalKey, materialItemId);
                    }

                    parsedValue = 0;

                    if (!string.IsNullOrEmpty(prodId))
                    {
                        if (long.TryParse(prodId, out parsedValue))
                            keys.ProdId = parsedValue;
                        else
                            logger.Error("Value for prod_id is not a numeric value [{0}]. Material item id: {1}", prodId, materialItemId);
                    }
                }

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_LOSDB_KEYS", parameters);

                if (keys == null)
                {
                    keys = new LOSDBMaterialManager.KeyCollection();
                }
                keys.ElectricalKeyList = new List<long>();
                keys.CLEIKeyList = new List<long>();
                while (reader.Read())
                {
                    if (reader["electrical_key"].ToString() != String.Empty)
                    {
                        try
                        {
                            if (!keys.ElectricalKeyList.Contains(long.Parse(reader["electrical_key"].ToString())))
                            {
                                keys.ElectricalKeyList.Add(long.Parse(reader["electrical_key"].ToString()));
                            }
                        }
                        catch { }
                    }
                    if ( reader["comp_clei_key"].ToString() != String.Empty)
                    {
                        try
                        {
                            if (!keys.CLEIKeyList.Contains(long.Parse(reader["comp_clei_key"].ToString())))
                            {
                                keys.CLEIKeyList.Add(long.Parse(reader["comp_clei_key"].ToString()));
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get LOSDB material keys: {0}";

                hadException = true;

                logger.Error(oe, message, materialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to get LOSDB material keys: {0}", materialItemId);
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

            if (hadException)
                throw new Exception();

            return keys;
        }

        public void GetLOSDBIesEqpt(MaterialItem materialItem, long prodId, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pProdId", DbType.Int64, prodId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_IES_EQUIPMENT_VALUES", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESEquipment)
                    {
                        materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Equipment: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Equipment: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public void GetLOSDBIesInventory(MaterialItem materialItem, string eqptCatalogId, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pEqptId", DbType.String, eqptCatalogId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_IES_INVENTORY_VALUES", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESInventory)
                    {
                        if (!materialItem.Attributes.ContainsKey(keyValue.Key))
                            materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Inventory: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Inventory: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public void GetLOSDBIesInventoryAndEquipment(MaterialItem materialItem, string eqptCatalogId, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pEqptId", DbType.String, eqptCatalogId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_IES_INVNTRY_AND_EQPT_VALS", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESInventory)
                    {
                        if (!materialItem.Attributes.ContainsKey(keyValue.Key))
                            materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESEquipment)
                    {
                        if (!materialItem.Attributes.ContainsKey(keyValue.Key))
                            materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Inventory and IES Equipment: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Inventory and IES Equipment: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public void GetLOSDBIesEquipmentAndInventory(MaterialItem materialItem, long prodId, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pPrdId", DbType.Int64, prodId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_IES_EQPT_AND_INVNTRY_VALS", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESInventory)
                    {
                        if (!materialItem.Attributes.ContainsKey(keyValue.Key))
                            materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESEquipment)
                    {
                        if (!materialItem.Attributes.ContainsKey(keyValue.Key))
                            materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Inventory and IES Equipment: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Inventory and IES Equipment: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public void GetLOSDBElectrical(MaterialItem materialItem, long key, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pKey", DbType.Int64, key, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXTENDED_ATTRIBUTES_ELECRL", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESElectrical)
                    {
                        materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Electrical: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Electrical: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public List<MaterialItem.ElectricalItemObject> GetLOSDBElectrical(MaterialItem materialItem, LOSDBMaterialManager.KeyCollection keys)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;
            List<MaterialItem.ElectricalItemObject> electricalItems = new List<MaterialItem.ElectricalItemObject>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                foreach (long key in keys.ElectricalKeyList)
                {
                    parameters[0] = dbManager.GetParameter("pKey", DbType.Int64, key, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXTENDED_ATTRIBUTES_ELECRL", parameters);

                    while (reader.Read())
                    {
                        MaterialItem.ElectricalItemObject electricalItem = new MaterialItem.ElectricalItemObject();
                        electricalItem.InputvoltageMinimum = reader["er_inputvoltagefrom_value"].ToString();
                        if (reader["er_inputvoltageto_unit"].ToString() != "NA" && reader["er_inputvoltageto_unit"].ToString() != "NS")
                        {
                            electricalItem.InputvoltageMinimum += " " + reader["er_inputvoltageto_unit"].ToString();
                        }
                        electricalItem.InputVoltageMaximum = reader["er_inputvoltageto_value"].ToString();
                        if (reader["er_inputvoltageto_unit"].ToString() != "NA" && reader["er_inputvoltageto_unit"].ToString() != "NS")
                        {
                            electricalItem.InputVoltageMaximum += " " + reader["er_inputvoltageto_unit"].ToString();
                        }
                        electricalItem.NormalElectricalCurrent = reader["er_inputcurrentfrom_value"].ToString();
                        if (reader["er_inputcurrentto_unit"].ToString() != "NA" && reader["er_inputcurrentto_unit"].ToString() != "NS")
                        {
                            electricalItem.NormalElectricalCurrent += " " + reader["er_inputcurrentto_unit"].ToString();
                        }
                        electricalItem.MaximumElectricalCurrent = reader["er_inputcurrentto_value"].ToString();
                        if (reader["er_inputcurrentto_unit"].ToString() != "NA" && reader["er_inputcurrentto_unit"].ToString() != "NS")
                        {
                            electricalItem.MaximumElectricalCurrent += " " + reader["er_inputcurrentto_unit"].ToString();
                        }
                        electricalItems.Add(electricalItem);
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Electrical: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Electrical: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();

            return electricalItems;
        }

        public MaterialItem.CompatibleSevenItemObject GetLOSDBCompClei7(MaterialItem materialItem, LOSDBMaterialManager.KeyCollection keys)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;
            MaterialItem.CompatibleSevenItemObject compatible7CleiObject = new MaterialItem.CompatibleSevenItemObject();
            compatible7CleiObject.CompatibleClei7 = new List<string>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                foreach (long key in keys.CLEIKeyList)
                {
                    parameters[0] = dbManager.GetParameter("pKey", DbType.Int64, key, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXTENDED_ATTRIBUTES_CLEI", parameters);

                    while (reader.Read())
                    {
                        if (!compatible7CleiObject.CompatibleClei7.Contains(reader["compatibleequipmentclei7"].ToString()))  // make sure no duplicates
                        {
                        compatible7CleiObject.CompatibleClei7.Add(reader["compatibleequipmentclei7"].ToString());
                    }
                }
            }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Comp Clei: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Electrical: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();

            return compatible7CleiObject;
        }

        public void GetLOSDBCompatibleCLEI(MaterialItem materialItem, long key, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pKey", DbType.Int64, key, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXTENDED_ATTRIBUTES_CLEI", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESCompatibleCLEI)
                    {
                        materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Compatible CLEI: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Compatible CLEI: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public List<LOSDBMaterialManager.Mtrl> GetExistingAssociatedLOSDBMaterial(long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<LOSDBMaterialManager.Mtrl> materialList = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXISTING_ASSOCIATED_MTRL", parameters);

                while (reader.Read())
                {
                    LOSDBMaterialManager.Mtrl mtrl = new LOSDBMaterialManager.Mtrl();

                    mtrl.MtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                    mtrl.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    mtrl.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    mtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_alias_prod_id", true));
                    mtrl.AlternateIndicator = DataReaderHelper.GetNonNullValue(reader, "altInd");

                    if (mtrl.ProdId == 0)
                        mtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "prod_id", true));

                    if (materialList == null)
                        materialList = new List<LOSDBMaterialManager.Mtrl>();

                    materialList.Add(mtrl);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve existing associated LOSDB material: {0}";

                logger.Error(oe, message, mtrlId);
                EventLogger.LogException(oe, string.Format(message, mtrlId));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve existing associated LOSDB material: {0}", mtrlId);
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

            return materialList;
        }

        public List<LOSDBMaterialManager.LosdbMtrl> GetChainedInLOSDBMaterialByClei(LOSDBMaterialManager.Mtrl mtrl)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<LOSDBMaterialManager.LosdbMtrl> materialList = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                //parameters = dbManager.GetParameterArray(3);

                //parameters[0] = dbManager.GetParameter("pProdId", DbType.Int64, mtrl.ProdId, ParameterDirection.Input);
                //parameters[1] = dbManager.GetParameter("pEqptId", DbType.String, mtrl.EquipmentCatalogItemId, ParameterDirection.Input);
                //parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlId", DbType.Int64, mtrl.MtrlId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                //reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_CI_MTRL_BY_CLEI", parameters);
                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_CI_MTRL_BY_CLEI_NEW", parameters);

                while (reader.Read())
                {
                    LOSDBMaterialManager.LosdbMtrl losdbMtrl = new LOSDBMaterialManager.LosdbMtrl();

                    losdbMtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "prod_id", true));
                    losdbMtrl.CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    losdbMtrl.Drawing = DataReaderHelper.GetNonNullValue(reader, "drwg");
                    losdbMtrl.DrawingIssue = DataReaderHelper.GetNonNullValue(reader, "drwg_iss");
                    losdbMtrl.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    losdbMtrl.LsOrSrs = DataReaderHelper.GetNonNullValue(reader, "ls_or_srs");
                    losdbMtrl.OrderingCode = DataReaderHelper.GetNonNullValue(reader, "ordg_cd");
                    losdbMtrl.PartNumber = DataReaderHelper.GetNonNullValue(reader, "part_no");
                    losdbMtrl.Vendor = DataReaderHelper.GetNonNullValue(reader, "vndr_cd");
                    losdbMtrl.CleiCompatible7 = DataReaderHelper.GetNonNullValue(reader, "compatibleequipmentclei7");
                    losdbMtrl.PCNChange = DataReaderHelper.GetNonNullValue(reader, "pcnchange");
                    losdbMtrl.AlternateIndicator = mtrl.AlternateIndicator;

                    if (materialList == null)
                        materialList = new List<LOSDBMaterialManager.LosdbMtrl>();

                    materialList.Add(losdbMtrl);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve existing associated LOSDB material: {0}, {1}";

                logger.Error(oe, message, mtrl.ProdId, mtrl.EquipmentCatalogItemId);
                EventLogger.LogException(oe, string.Format(message, mtrl.ProdId, mtrl.EquipmentCatalogItemId));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve existing associated LOSDB material: {0}, {1}", mtrl.ProdId, mtrl.EquipmentCatalogItemId);
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

            return materialList;
        }

        public List<LOSDBMaterialManager.LosdbMtrl> GetChainedInLOSDBMaterialByOrderingCode(LOSDBMaterialManager.Mtrl mtrl)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<LOSDBMaterialManager.LosdbMtrl> materialList = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pProdId", DbType.Int64, mtrl.ProdId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pEqptId", DbType.String, mtrl.EquipmentCatalogItemId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_CI_MTRL_BY_ORDG_CD", parameters);

                while (reader.Read())
                {
                    LOSDBMaterialManager.LosdbMtrl losdbMtrl = new LOSDBMaterialManager.LosdbMtrl();

                    losdbMtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "prod_id", true));
                    losdbMtrl.CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    losdbMtrl.Drawing = DataReaderHelper.GetNonNullValue(reader, "drwg");
                    losdbMtrl.DrawingIssue = DataReaderHelper.GetNonNullValue(reader, "drwg_iss");
                    losdbMtrl.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    losdbMtrl.LsOrSrs = DataReaderHelper.GetNonNullValue(reader, "ls_or_srs");
                    losdbMtrl.OrderingCode = DataReaderHelper.GetNonNullValue(reader, "ordg_cd");
                    losdbMtrl.PartNumber = DataReaderHelper.GetNonNullValue(reader, "part_no");
                    losdbMtrl.Vendor = DataReaderHelper.GetNonNullValue(reader, "vndr_cd");
                    losdbMtrl.CleiCompatible7 = DataReaderHelper.GetNonNullValue(reader, "compatibleequipmentclei7");
                    losdbMtrl.PCNChange = DataReaderHelper.GetNonNullValue(reader, "pcnchange");
                    losdbMtrl.AlternateIndicator = mtrl.AlternateIndicator;

                    if (materialList == null)
                        materialList = new List<LOSDBMaterialManager.LosdbMtrl>();

                    materialList.Add(losdbMtrl);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve existing associated LOSDB material: {0}, {1}";

                logger.Error(oe, message, mtrl.ProdId, mtrl.EquipmentCatalogItemId);
                EventLogger.LogException(oe, string.Format(message, mtrl.ProdId, mtrl.EquipmentCatalogItemId));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve existing associated LOSDB material: {0}, {1}", mtrl.ProdId, mtrl.EquipmentCatalogItemId);
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

            return materialList;
        }

        public List<LOSDBMaterialManager.LosdbMtrl> GetChainedInLOSDBMaterialByPartNumber(LOSDBMaterialManager.Mtrl mtrl)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<LOSDBMaterialManager.LosdbMtrl> materialList = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pProdId", DbType.Int64, mtrl.ProdId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pEqptId", DbType.String, mtrl.EquipmentCatalogItemId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_CI_MTRL_BY_PART_NO", parameters);

                while (reader.Read())
                {
                    LOSDBMaterialManager.LosdbMtrl losdbMtrl = new LOSDBMaterialManager.LosdbMtrl();

                    losdbMtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "prod_id", true));
                    losdbMtrl.CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    losdbMtrl.Drawing = DataReaderHelper.GetNonNullValue(reader, "drwg");
                    losdbMtrl.DrawingIssue = DataReaderHelper.GetNonNullValue(reader, "drwg_iss");
                    losdbMtrl.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    losdbMtrl.LsOrSrs = DataReaderHelper.GetNonNullValue(reader, "ls_or_srs");
                    losdbMtrl.OrderingCode = DataReaderHelper.GetNonNullValue(reader, "ordg_cd");
                    losdbMtrl.PartNumber = DataReaderHelper.GetNonNullValue(reader, "part_no");
                    losdbMtrl.Vendor = DataReaderHelper.GetNonNullValue(reader, "vndr_cd");
                    losdbMtrl.CleiCompatible7 = DataReaderHelper.GetNonNullValue(reader, "compatibleequipmentclei7");
                    losdbMtrl.PCNChange = DataReaderHelper.GetNonNullValue(reader, "pcnchange");
                    losdbMtrl.AlternateIndicator = mtrl.AlternateIndicator;

                    if (materialList == null)
                        materialList = new List<LOSDBMaterialManager.LosdbMtrl>();

                    materialList.Add(losdbMtrl);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve existing associated LOSDB material: {0}, {1}";

                logger.Error(oe, message, mtrl.ProdId, mtrl.EquipmentCatalogItemId);
                EventLogger.LogException(oe, string.Format(message, mtrl.ProdId, mtrl.EquipmentCatalogItemId));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve existing associated LOSDB material: {0}, {1}", mtrl.ProdId, mtrl.EquipmentCatalogItemId);
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

            return materialList;
        }

        public List<LOSDBMaterialManager.LosdbMtrl> GetChainedInLOSDBMaterialByDrawing(LOSDBMaterialManager.Mtrl mtrl)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<LOSDBMaterialManager.LosdbMtrl> materialList = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pProdId", DbType.Int64, mtrl.ProdId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pEqptId", DbType.String, mtrl.EquipmentCatalogItemId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_CI_MTRL_BY_DRWG", parameters);

                while (reader.Read())
                {
                    LOSDBMaterialManager.LosdbMtrl losdbMtrl = new LOSDBMaterialManager.LosdbMtrl();

                    losdbMtrl.ProdId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "prod_id", true));
                    losdbMtrl.CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    losdbMtrl.Drawing = DataReaderHelper.GetNonNullValue(reader, "drwg");
                    losdbMtrl.DrawingIssue = DataReaderHelper.GetNonNullValue(reader, "drwg_iss");
                    losdbMtrl.EquipmentCatalogItemId = DataReaderHelper.GetNonNullValue(reader, "eqpt_ctlg_item_id");
                    losdbMtrl.LsOrSrs = DataReaderHelper.GetNonNullValue(reader, "ls_or_srs");
                    losdbMtrl.OrderingCode = DataReaderHelper.GetNonNullValue(reader, "ordg_cd");
                    losdbMtrl.PartNumber = DataReaderHelper.GetNonNullValue(reader, "part_no");
                    losdbMtrl.Vendor = DataReaderHelper.GetNonNullValue(reader, "vndr_cd");
                    losdbMtrl.CleiCompatible7 = DataReaderHelper.GetNonNullValue(reader, "compatibleequipmentclei7");
                    losdbMtrl.PCNChange = DataReaderHelper.GetNonNullValue(reader, "pcnchange");
                    losdbMtrl.AlternateIndicator = mtrl.AlternateIndicator;

                    if (materialList == null)
                        materialList = new List<LOSDBMaterialManager.LosdbMtrl>();

                    materialList.Add(losdbMtrl);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve existing associated LOSDB material: {0}, {1}";

                logger.Error(oe, message, mtrl.ProdId, mtrl.EquipmentCatalogItemId);
                EventLogger.LogException(oe, string.Format(message, mtrl.ProdId, mtrl.EquipmentCatalogItemId));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve existing associated LOSDB material: {0}, {1}", mtrl.ProdId, mtrl.EquipmentCatalogItemId);
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

            return materialList;
        }

        public void GetLOSDBMainExtension(MaterialItem materialItem, string clei, ref bool valuesWereFound)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            MaterialType mtlType = new MaterialType();
            bool hadException = false;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pClei", DbType.String, clei, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("LOSDB_PKG.GET_EXTENDED_ATTRIBUTES_MAIN", parameters);

                if (reader.Read())
                {
                    valuesWereFound = true;

                    foreach (KeyValuePair<string, DatabaseDefinition> keyValue in mtlType.IESMainExtension)
                    {
                        if (MaterialType.JSON.HzrdInd.Equals(keyValue.Key))
                        {
                            string hzrdInd = DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber);

                            if (string.IsNullOrEmpty(hzrdInd) || "N".Equals(hzrdInd) || "0".Equals(hzrdInd))
                                materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, "N", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB), false));
                            else
                                materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, true, MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                        }
                        else
                        materialItem.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber), MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                    }
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve LOSDB IES Main Extension: {0}";

                hadException = true;

                logger.Error(oe, message, materialItem.MaterialItemId);
                EventLogger.LogAlarm(oe, string.Format(message, materialItem.MaterialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                hadException = true;

                logger.Error(ex, "Unable to retrieve LOSDB IES Main Extension: {0}", materialItem.MaterialItemId);
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

            if (hadException)
                throw new Exception();
        }

        public void UpdateMaterialRevisionCleiCode(string revisionTableName, string revisionID, string cleiCode, string revisionIDColumnName)
        {
            IAccessor dbManager = null;
            dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            string sql = @"update " + revisionTableName + " set clei_cd = '" + cleiCode + "' where " + revisionIDColumnName + " = " + revisionID;

            try
            {
                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating UpdateMaterialRevisionCleiCode - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialDbInterface.UpdateMaterialRevisionCleiCode");
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public void UpdateHighLevelPartMADefinitionId(long materialItemId, long maDefinitionId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pMaDefId", DbType.Int64, maDefinitionId, ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("hlp_mtrl_pkg.upsert_ma_definition_id", parameters);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "MaterialDbInterface.UpdateHighLevelPartMADefinitionId({0}, {1}): " + ex.Message, materialItemId, maDefinitionId);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public void UpdateHighLevelPartWorkToDo(long workToDoId, string action, string status)
        {
            IDbDataParameter[] parameters = null;
            bool dispose = false;

            try
            {
                if (dbAccessor == null)
                {
                    dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    dispose = true;
                }

                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pWtdId", DbType.Int64, workToDoId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pAction", DbType.String, CheckNullValue(action), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pStatus", DbType.String, status, ParameterDirection.Input);

                //PROCEDURE update_hlpn_work_to_do(pWtdId IN NUMBER, pAction IN VARCHAR2, pStatus IN VARCHAR2)
                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.update_hlpn_work_to_do", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update work_to_do for high level part ({0}, {1}, {2})", workToDoId, action, status);

                throw ex;
            }
            finally
            {
                if (dispose && dbAccessor != null)
                    dbAccessor.Dispose();
            }
        }

        public CenturyLink.Network.Engineering.LOSDB.Service.Objects.FeatureType GetFeatureByMtrlID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            CenturyLink.Network.Engineering.LOSDB.Service.Objects.FeatureType feature = null;


            string @sql = "select feat_typ_id, cdmms_rt_tbl_nm, cdmms_revsn_tbl_nm, cdmms_alias_val_tbl_nm from feat_typ " +
                            "where feat_typ_id = " +
                            "(select feat_typ_id from mtrl where mtrl_id = " + materialID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    if (feature == null)
                    {
                        feature = new CenturyLink.Network.Engineering.LOSDB.Service.Objects.FeatureType();
                    }
                    feature.FeatureTypeID = long.Parse(reader["feat_typ_id"].ToString());
                    feature.CDMMSRTTableName = reader["cdmms_rt_tbl_nm"].ToString();
                    feature.CDMMSRevisionTableName = reader["cdmms_revsn_tbl_nm"].ToString();
                    feature.CDMMSAliasValTableName = reader["cdmms_alias_val_tbl_nm"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetFeatureByMtrlID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetFeatureByMtrlID({0})", materialID);
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

            return feature;
        }

        public int GetMaterialCatalogID(string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialCatalogID = String.Empty;

            string @sql = "select mtrl_cat_id from mtrl where mtrl_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialCatalogID = reader["mtrl_cat_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetMaterialCatalogID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetMaterialCatalogID({0})", materialID);
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

            if (materialCatalogID != String.Empty)
                return int.Parse(materialCatalogID);
            else return 0;
        }

        public CenturyLink.Network.Engineering.LOSDB.Service.Objects.EquipmentObject GetEquipmentObject(string prodID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            EquipmentObject equipmentObject = new EquipmentObject();

            CenturyLink.Network.Engineering.LOSDB.Service.Objects.FeatureType feature = new CenturyLink.Network.Engineering.LOSDB.Service.Objects.FeatureType();

            string @sql = @"select b.prod_id, b.clei_cd, b.eqpt_ctlg_item_id, a.vndr_cd, a.drwg, a.drwg_iss, a.part_no, b.ordg_cd, b.ls_or_srs, c.stenciling, c.orderingcode
                            from ies_eqpt a, ies_invntry b, ies_ea_main_extn c
                            where a.prod_id = b.prod_id 
                            and b.clei_cd = c.cleicode(+) 
                            and b.prod_id = " + prodID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    equipmentObject.ProdID = reader["prod_id"].ToString();
                    equipmentObject.EquipmentCatalogItemID = reader["eqpt_ctlg_item_id"].ToString();
                    equipmentObject.VendorCode = reader["vndr_cd"].ToString();
                    equipmentObject.Drawing = reader["drwg"].ToString();
                    equipmentObject.DrawingISS = reader["drwg_iss"].ToString();
                    equipmentObject.PartNumber = reader["part_no"].ToString();
                    equipmentObject.OrderingCode = reader["orderingcode"].ToString();
                    equipmentObject.LsOrSrs = reader["ls_or_srs"].ToString();
                    equipmentObject.Stenciling = reader["stenciling"].ToString();
                    equipmentObject.CLEICode = reader["clei_cd"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetEquipmentObject - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetEquipmentObject({0})", "");
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

            return equipmentObject;
        }

        public void InsertEquipmentIDAliasVal(string aliasTableName, string revisionIDColumnName, string aliasIDColumnName,
            long revisionID, string revisionAliasID, string aliasVal)
        {
            IAccessor dbManager = null;
            dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            string sql = @"insert into " + aliasTableName + " (" + revisionIDColumnName + "," + aliasIDColumnName + ",alias_val) " +
                            "values (" + revisionID + "," + revisionAliasID + ",'" + aliasVal + "')";

            try
            {
                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating InsertEquipmentIDAliasVal - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "InsertEquipmentIDAliasVal");
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        public string GetRevisionID(string revisionIDColumnName, string materialID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string revisionID = "";

            string revisionTableName = revisionIDColumnName.Substring(0, revisionIDColumnName.Length - 3);

            string @sql = "select distinct " + revisionIDColumnName + " from " + revisionTableName + " where mtrl_id = " + materialID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    revisionID = reader[revisionIDColumnName].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetRevisionID - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetRevisionID({0}, {1})", "");
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

            return revisionID;
        }

        public List<string> GetElectricalKeys(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> electricalKeys = new List<string>();

            string @sql = "select electrical_key from ies_ea_electrical_extn where cleicode in " +
                            "(select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    electricalKeys.Add(reader["electrical_key"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetElectricalKeys - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetElectricalKeys({0}, {1})", "");
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

            return electricalKeys;
        }

        public List<string> GetCompCleiKeys(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> compCleiKeys = new List<string>();

            string @sql = "select comp_clei_key from ies_ea_comp_clei_extn where cleicode in " +
                            "(select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    compCleiKeys.Add(reader["comp_clei_key"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetCompCleiKeys - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetCompCleiKeys({0}, {1})", "");
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

            return compCleiKeys;
        }

        public List<string> GetCleiCodes(string prodID, string equipmentCatalogItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            List<string> cleiCodes = new List<string>();

            string @sql = "select cleicode from ies_ea_main_extn where cleicode in ( " +
                            "select clei_cd as cleicode from ies_invntry " +
                            "where prod_id = " + prodID + " and eqpt_ctlg_item_id = '" + equipmentCatalogItemID + "')";
      

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    cleiCodes.Add(reader["cleicode"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetCleiCodes - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "DbInterface.GetCleiCodes({0}, {1})", "");
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

            return cleiCodes;
        }

        public string GetMaterialCode(long materialItemID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string materialCode = "";

            string @sql = "select product_id from material_id_mapping_vw where material_item_id = '" + materialItemID.ToString() + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    materialCode = reader["product_id"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetMaterialCode - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetMaterialCode({0}, {1})", "");
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

            return materialCode;
        }

        public long GetDimUomId(string dimUomCd)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            long dimUomId = 0L;

            string @sql = "select dim_uom_id from dim_uom where dim_uom_cd = '" + dimUomCd + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    dimUomId = long.Parse(reader["dim_uom_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetDimUomId - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetDimUomId({0}, {1})", dimUomCd, sql);
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

            return dimUomId;
        }

        public bool RmeRecordExist(long mtlID, string rmeTable)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            bool exists = false;

            string @sql = "select mtrl_id from " + rmeTable + " where mtrl_id = " + mtlID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    exists = true;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in RmeRecordExist - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "RmeRecordExist({0}, {1})", mtlID.ToString(), sql);
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

            return exists;
        }

        public string RmeSpecnAltIDExist(long mtlID, string rmeTable, string rmeColumn)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string specnRevsnAltID = String.Empty;

            string @sql = "select " + rmeColumn + " from " + rmeTable + " where mtrl_id = " + mtlID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    specnRevsnAltID = reader[rmeColumn].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in RmeSpecnAltIDExist - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "RmeSpecnAltIDExist({0}, {1})", mtlID.ToString(), sql);
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

            return specnRevsnAltID;
        }

        public string CheckPropAndComp(string specnRevsnID, string specnTable, string specnColumn)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            string check = "N";

            string @sql = "select cmplt_ind, prpgt_ind from " + specnTable + " where " + specnColumn + " = " + specnRevsnID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    string completed = reader["cmplt_ind"].ToString();
                    string propagated = reader["prpgt_ind"].ToString();
                    if (completed == "Y" && propagated == "Y")
                    {
                        check = "Y";
                    }
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in CheckPropAndComp - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "CheckPropAndComp({0}, {1})", specnRevsnID, sql);
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

            return check;
        }

        public void InsertRme(long mtlID, string rmeTable, string depth, string width, string height, long dimUomID, string depthColumn,
            string heightColumn, string widthColumn, string dimUomColumn)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "insert into " + rmeTable + " (mtrl_id, " + depthColumn + ", " + heightColumn + ", " + widthColumn + ", " + dimUomColumn + ") " +
                " values (" + mtlID.ToString() + "," + depth + "," + height + "," + width + "," + dimUomID.ToString() + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in InsertRme - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "InsertRme({0}, {1})", mtlID.ToString(), sql);
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
        }

        public void InsertRmeBay(long mtlID, string rmeTable, string depth, string width, string height, long dimUomID, string depthColumn,
            string heightColumn, string widthColumn, string dimUomColumn)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "insert into " + rmeTable + " (mtrl_id, " + depthColumn + ", " + heightColumn + ", " + widthColumn + ", " + dimUomColumn + ",cab_ind) " +
                " values (" + mtlID.ToString() + "," + depth + "," + height + "," + width + "," + dimUomID.ToString() + ",'Y')";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in InsertRmeBay - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "InsertRmeBay({0}, {1})", mtlID.ToString(), sql);
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
        }

        public void UpdateRme(long mtlID, string rmeTable, string depth, string width, string height, long dimUomID, string depthColumn,
            string heightColumn, string widthColumn, string dimUomColumn)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "update " + rmeTable + " set " + depthColumn + " = " + depth + ", " + heightColumn + " = " + height +
                ", " + widthColumn + " = " + width + ", " + dimUomColumn + " = " + dimUomID.ToString() + " where mtrl_id = " + mtlID.ToString();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in UpdateRme - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "UpdateRme({0}, {1})", mtlID.ToString(), sql);
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
        }

        public long GetBayExtenderItnlDepth(string depth, ref long dimUomID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            long depthID = 0L;

            string @sql = "select bay_extndr_itnl_dpth_id, itnl_dim_uom_id from bay_extndr_intl_dpth where itnl_dpth_no = " + depth;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    depthID = long.Parse(reader["bay_extndr_itnl_dpth_id"].ToString());
                    dimUomID = long.Parse(reader["itnl_dim_uom_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetBayExtenderItnlDepth - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetBayExtenderItnlDepth({0}, {1})", depth, sql);
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

            return depthID;
        }

        public long GetBayExtenderItnlWidth(string width, ref long dimUomID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            long widthID = 0L;

            string @sql = "select bay_extndr_itnl_wdth_id, itnl_dim_uom_id from bay_extndr_intl_wdth where itnl_wdth_no = " + width;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    widthID = long.Parse(reader["bay_extndr_itnl_wdth_id"].ToString());
                    dimUomID = long.Parse(reader["itnl_dim_uom_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetBayExtenderItnlWidth - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetBayExtenderItnlWidth({0}, {1})", width, sql);
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

            return widthID;
        }

        public long GetBayExtenderItnlHeight(string height, ref long dimUomID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            long heightID = 0L;

            string @sql = "select bay_extndr_itnl_hgt_id, itnl_dim_uom_id from bay_extndr_intl_hgt where itnl_wdth_no = " + height;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    heightID = long.Parse(reader["bay_extndr_itnl_hgt_id"].ToString());
                    dimUomID = long.Parse(reader["itnl_dim_uom_id"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetBayExtenderItnlHeight - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetBayExtenderItnlHeight({0}, {1})", height, sql);
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

            return heightID;
        }

        public void InsertRmeBayExtender(long mtlID, long depthID, long heightID, long widthID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "insert into rme_bay_extndr_mtrl (mtrl_id, itnl_dpth_id, itnl_hgt_id, itnl_wdth_id) " +
                " values (" + mtlID + "," + depthID + "," + heightID + "," + widthID + ")";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in InsertRmeBayExtender - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "InsertRmeBayExtender({0}, {1})", mtlID.ToString(), sql);
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
        }

        public void UpdateRmeBayExtender(long mtlID, long depthID, long heightID, long widthID)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "update rme_bay_extndr_mtrl set itnl_dpth_id = " + depthID + ", itnl_hgt__id = " + heightID +
                ", itnl_wdth_id = " + widthID + " where mtrl_id = " + mtlID;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in UpdateRmeBayExtender - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "UpdateRmeBayExtender({0}, {1})", mtlID.ToString(), sql);
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
        }

        public string GetManufacturerName(int id)
        {
            IDataReader reader = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            string name = "";
            
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);
                parameters[0] = dbManager.GetParameter("pId", DbType.Int32, id, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("material_pkg.get_mfr_nm",parameters);

                while (reader.Read())
                {
                    name = reader["mfr_nm"].ToString();
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while in GetManufacturerName - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "GetManufacturerName({0})", id);
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

            return name;
        }
    }
}