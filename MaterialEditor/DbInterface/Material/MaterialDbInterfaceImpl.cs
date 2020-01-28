using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public abstract class MaterialDbInterfaceImpl : IMaterialDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        protected IAccessor dbAccessor = null;

        public MaterialDbInterfaceImpl()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public MaterialDbInterfaceImpl(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public string DbConnectionString
        {
            get
            {
                if(connectionString == null)
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

        public void DeleteMaterialItemAttributes(long materialItemId, long materialItemDefId)
        {
            string sql = @"DELETE 
                            FROM material_item_attributes a
                            WHERE a.material_item_id = :materialItemId
                            AND a.mtl_item_attributes_def_id = :materialItemDefId";
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, materialItemDefId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete from material_item_attributes_def ({0}, {1})", materialItemId, materialItemDefId);

                throw ex;
            }
        }

        public void InsertMaterialItemAttributes(string attributeValue, long materialItemId, string attributeName, string cuid)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("value", DbType.String, attributeValue.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("short_name", DbType.String, attributeName, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("cuid", DbType.String, cuid, ParameterDirection.Input);
                
                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.INSERT_MTL_ITM_ATTRIBUTES", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert material_item_attributes_def ({0}, {1}, {2})", attributeValue, materialItemId, attributeName);

                throw ex;
            }
        }

        public void UpdateMaterialItemAttributes(string attributeValue, long materialItemId, long materialItemDefId, string cuid)
        {
            IDbDataParameter[] parameters = null;

            if (string.IsNullOrEmpty(cuid))
                cuid = "cdmms";

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("value", DbType.String, attributeValue.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("mtl_item_attributes_def_id", DbType.Int64, materialItemDefId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("cuid", DbType.String, cuid, ParameterDirection.Input);                

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.UPDATE_MTL_ITM_ATTRIBUTES", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update material_item_attributes_def ({0}, {1}, {2})", attributeValue, materialItemId, materialItemDefId);

                throw ex;
            }
        }

        public void InsertUpdateMaterialItemAttributes(string attributeValue, long materialItemId, string attributeName, string cuid)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("value", DbType.String, CheckNullValue(attributeValue), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("short_name", DbType.String, attributeName, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("cuid", DbType.String, cuid, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MATERIAL_PKG.UPSERT_MTL_ITM_ATTRIBUTES", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert material_item_attributes_def ({0}, {1}, {2})", attributeValue, materialItemId, attributeName);

                throw ex;
            }
        }

        public long InsertWorkToDo(long materialItemId, string workType, string tableName)
        {
            IDbDataParameter[] parameters = null;
            bool dispose = false;
            long workToDoId = 0;

            try
            {
                if (dbAccessor == null)
                {
                    dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    dispose = true;
                }

                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("material_item_id", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("work_type", DbType.String, workType, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pTblNm", DbType.String, CheckNullValue(tableName), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("work_to_do_id", DbType.Int64, workToDoId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuerySP("MATERIAL_PKG.INSERT_WORK_TO_DO", parameters);

                workToDoId = long.Parse(parameters[3].Value.ToString());

                return workToDoId;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update work_to_do ({0}, {1}, {2})", materialItemId, workType, tableName);

                throw ex;
            }
            finally
            {
                if (dispose && dbAccessor != null)
                    dbAccessor.Dispose();
            }
        }

        public long InsertWorkToDo(long id, long parentId, string workType, string tableName, string status)
        {
            IDbDataParameter[] parameters = null;
            bool dispose = false;
            long workToDoId = 0;

            try
            {
                if (dbAccessor == null)
                {
                    dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    dispose = true;
                }

                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pDefId", DbType.Int64, id, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pPrntId", DbType.Int64, CheckNullValue(parentId), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pWrkTyp", DbType.String, workType, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pTblNm", DbType.String, CheckNullValue(tableName), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pStatus", DbType.String, CheckNullValue(status), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("oWrkId", DbType.Int64, workToDoId, ParameterDirection.Output);

                //PROCEDURE INSERT_WORK_TO_DO(pDefId IN NUMBER, pPrntId IN NUMBER, pWrkTyp IN VARCHAR2, pTblNm IN VARCHAR2, pStatus IN VARCHAR2, oWrkId OUT work_to_do.work_to_do_id%TYPE)
                dbAccessor.ExecuteNonQuerySP("hlp_mtrl_pkg.insert_work_to_do", parameters);

                workToDoId = long.Parse(parameters[5].Value.ToString());

                return workToDoId;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update work_to_do ({0}, {1}, {2}, {3}, {4})", id, parentId, workType, tableName, status);

                throw ex;
            }
            finally
            {
                if (dispose && dbAccessor != null)
                    dbAccessor.Dispose();
            }
        }

        public long GetWorkToDoPniId(long workToDoId)
        {
            IDbDataParameter[] parameters = null;
            bool dispose = false;
            long pniId = 0;

            try
            {
                if (dbAccessor == null)
                {
                    dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    dispose = true;
                }

                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pWkToDoId", DbType.Int64, workToDoId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("oPniId", DbType.Int64, pniId, ParameterDirection.Output);

                //PROCEDURE GET_WORK_TO_DO_PNI_ID(pWkToDoId IN NUMBER, oPniId OUT NUMBER)
                dbAccessor.ExecuteNonQuerySP("MATERIAL_PKG.GET_WORK_TO_DO_PNI_ID", parameters);

                pniId = long.Parse(parameters[1].Value.ToString());

                return pniId;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get work_to_do.pni_id ({0})", workToDoId);

                throw ex;
            }
            finally
            {
                if (dispose && dbAccessor != null)
                    dbAccessor.Dispose();
            }
        }

        public long InsertMaterialItem(string productId, long iesProdId, string recordOnlyInd, string sourceOfRecord, string recordOnlyPublishedInd)
        {
            IDbDataParameter[] parameters = null;
            bool dispose = false;
            long materialItemId = 0;

            try
            {
                if (dbAccessor == null)
                {
                    dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    dispose = true;
                }

                //PROCEDURE CREATE_MATERIAL_ITEM(pPrdctId IN material_item.product_id%TYPE, pPrdId IN material_item.ies_eqpt_prod_id%TYPE,
                //pRoInd IN material_item.record_only_ind % TYPE, pSrc IN material_item.source_of_record % TYPE,
                //pRoPubInd IN material_item.record_only_is_published_ind % TYPE, oMtlItmId OUT material_item.material_item_id % TYPE)
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pPrdctId", DbType.String, productId, ParameterDirection.Input);

                if(iesProdId == 0)
                    parameters[1] = dbAccessor.GetParameter("pPrdId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[1] = dbAccessor.GetParameter("pPrdId", DbType.Int64, iesProdId, ParameterDirection.Input);

                parameters[2] = dbAccessor.GetParameter("pRoInd", DbType.String, recordOnlyInd, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pSrc", DbType.String, sourceOfRecord, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pRoPubInd", DbType.String, recordOnlyPublishedInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuerySP("MATERIAL_PKG.CREATE_MATERIAL_ITEM", parameters);

                materialItemId = long.Parse(parameters[5].Value.ToString());

                return materialItemId;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create material_item ({0}, {1}, {2}, {3}, {4})", productId, iesProdId, recordOnlyInd, sourceOfRecord, recordOnlyPublishedInd);

                throw ex;
            }
            finally
            {
                if (dispose && dbAccessor != null)
                    dbAccessor.Dispose();
            }
        }

        public void UpdateBaseMaterial(long materialItemId, long mtrlId, string rootPartNumber, long manufacturerId, string catalogDescription, int laborId)
        {
            IDbDataParameter[] parameters = null;

            //if (string.IsNullOrEmpty(cuid))
            //    cuid = "cdmms";

            try
            {
                parameters = dbAccessor.GetParameterArray(5);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMfrId", DbType.Int64, manufacturerId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCtlgDsc", DbType.String, catalogDescription, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pLbrId", DbType.Int32, laborId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MTRL_PKG.UPDATE_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update mtrl ({0}, {1}, {2}, {3}, {4}, {5})", materialItemId, mtrlId, rootPartNumber, manufacturerId, catalogDescription, laborId);

                throw ex;
            }
        }

        public void ChangeMaterialItemType(long oldMtrlId, long newMtrlId, long materialItemId, int oldMaterialCategoryId, int oldFeatureTypeId, int newMaterialCategoryId, int newFeatureTypeId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pOldMtrlId", DbType.Int64, oldMtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNewMtrlId", DbType.Int64, newMtrlId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRvsnId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pOldMtlCtgry", DbType.Int32, oldMaterialCategoryId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pOldFtrTyp", DbType.Int32, CheckNullValue(oldFeatureTypeId), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pNewMtlCtgry", DbType.Int32, newMaterialCategoryId, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pNewFtrTyp", DbType.Int32, CheckNullValue(newFeatureTypeId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MTRL_PKG.CHANGE_MTRL_ITEM_TYPE", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to change mtrl type ({0}, {1}, {2}, {3}, {4}, {5}, {6})", oldMtrlId, newMtrlId, materialItemId, oldMaterialCategoryId, oldFeatureTypeId, newMaterialCategoryId, newFeatureTypeId);

                throw ex;
            }
        }

        public void UpdateRootPartNumber(long mtrlId, string rootPartNumber)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "MTRL_PKG.UPDATE_ROOT_PART_NUMBER", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to change mtrl root part number ({0}, {1})", mtrlId, rootPartNumber);

                throw ex;
            }
        }

        public abstract IMaterial GetMaterial(long materialItemId, long mtrlId);

        public List<MaterialItem> GetRevisions(long materialId, string tableName)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;
            List<MaterialItem> revisions = null;
            string aliasTableName = String.Empty;
            string revisionIDColumnName = String.Empty;

            switch (tableName.ToUpper())
            {
                case "RME_CARD_MTRL_REVSN":
                    {
                        aliasTableName = "RME_CARD_MTRL_REVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_CARD_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_BAY_EXTNDR_MTRL_REVSN":
                    {
                        aliasTableName = "RME_BAY_EXTNDR_MTRL_RV_ALS_VAL";
                        revisionIDColumnName = "RME_BAY_EXTNDR_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_BAY_MTRL_REVSN":
                    {
                        aliasTableName = "RME_BAY_MTRL_REVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_BAY_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_BULK_CABL_MTRL_REVSN":
                    {
                        aliasTableName = "RME_BULK_CABL_MTRL_RVS_ALS_VAL";
                        revisionIDColumnName = "RME_BULK_CABL_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_CNCTRZD_CABL_MTRL_REVSN":
                    {
                        aliasTableName = "RME_CNCTRZD_CABL_REVSN_ALS_VAL";
                        revisionIDColumnName = "RME_CNCTRZD_CABL_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_DF_BLK_MTRL_REVSN":
                    {
                        aliasTableName = "RME_DF_BLK_MTRL_RVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_DF_BLK_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_NODE_MTRL_REVSN":
                    {
                        aliasTableName = "RME_NODE_MTRL_REVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_NODE_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_PLG_IN_MTRL_REVSN":
                    {
                        aliasTableName = "RME_PLG_IN_MTRL_RVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_PLG_IN_MTRL_REVSN_ID";
                        break;
                    }
                case "RME_SHELF_MTRL_REVSN":
                    {
                        aliasTableName = "RME_SHELF_MTRL_REVSN_ALIAS_VAL";
                        revisionIDColumnName = "RME_SHELF_MTRL_REVSN_ID";
                        break;
                    }
            }
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(5);

                parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int64, materialId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pTblNm", DbType.String, tableName, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pAliasTblNm", DbType.String, aliasTableName, ParameterDirection.Input);
                parameters[3] = dbManager.GetParameter("pRevColNm", DbType.String, revisionIDColumnName, ParameterDirection.Input);
                parameters[4] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MATERIAL_REVISIONS_PKG.GET_MATERIAL_REVISIONS", parameters);

                while (reader.Read())
                {
                    MaterialItem revision = new MaterialItem();

                    Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                    Models.Attribute currentMtrlId = new Models.Attribute("curr_mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                    Models.Attribute updatedMtrlId = new Models.Attribute("updtd_mtrl_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_id", true));
                    Models.Attribute catalogDescription = new Models.Attribute("item_desc", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                    Models.Attribute manufacturer = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                    Models.Attribute partNumber = new Models.Attribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "mfg_part_no"));
                    Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cd"));
                    Models.Attribute spec = new Models.Attribute("spec_name", DataReaderHelper.GetNonNullValue(reader, "specNm"));
                    Models.Attribute apcl = new Models.Attribute("apcl", DataReaderHelper.GetNonNullValue(reader, "apcl_cd"));
                    Models.Attribute orderable = new Models.Attribute("orderable", DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_cd"));
                    Models.Attribute rvsn = new Models.Attribute("rvsn", DataReaderHelper.GetNonNullValue(reader, "revsn_no"));
                    Models.Attribute baseRvsn = new Models.Attribute("baseRvsn", DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind"));
                    Models.Attribute currRvsn = new Models.Attribute("currRvsn", DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind"));
                    Models.Attribute cleiCode = new Models.Attribute("cleiCode", DataReaderHelper.GetNonNullValue(reader, "clei_cd"));
                    Models.Attribute proposedSpec = new Models.Attribute("proposedSpec", "");

                    revision.Attributes.Add(id.Name, id);
                    revision.Attributes.Add(currentMtrlId.Name, currentMtrlId);
                    revision.Attributes.Add(updatedMtrlId.Name, updatedMtrlId);
                    revision.Attributes.Add(catalogDescription.Name, catalogDescription);
                    revision.Attributes.Add(manufacturer.Name, manufacturer);
                    revision.Attributes.Add(partNumber.Name, partNumber);
                    revision.Attributes.Add(materialCode.Name, materialCode);
                    revision.Attributes.Add(spec.Name, spec);
                    revision.Attributes.Add(apcl.Name, apcl);
                    revision.Attributes.Add(orderable.Name, orderable);
                    revision.Attributes.Add(rvsn.Name, rvsn);
                    revision.Attributes.Add(baseRvsn.Name, baseRvsn);
                    revision.Attributes.Add(currRvsn.Name, currRvsn);
                    revision.Attributes.Add(cleiCode.Name, cleiCode);
                    revision.Attributes.Add(proposedSpec.Name, proposedSpec);

                    if (revisions == null)
                        revisions = new List<MaterialItem>();

                    revisions.Add(revision);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get material revisions ({0}, {1})", materialId, tableName);

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
            
            return revisions;
        }

        public void GetReplacementMaterialLabel(IMaterial material)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, material.MaterialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MTRL_PKG.GET_REPLACEMENT_MTL_LABEL", parameters);

                while (reader.Read())
                {
                    if ("Y".Equals(DataReaderHelper.GetNonNullValue(reader, "is_replacement")))
                    {
                        //material.ReplacesMaterialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                        material.ReplacesMaterialPartNumber += DataReaderHelper.GetNonNullValue(reader, "part_no") + ", ";
                    }
                    else
                    {
                        material.ReplacedByMaterialItemId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                        material.ReplacedByMaterialPartNumber = DataReaderHelper.GetNonNullValue(reader, "part_no");
                    }
                }

                if (!string.IsNullOrEmpty(material.ReplacesMaterialPartNumber))
                {
                    material.ReplacesMaterialPartNumber = material.ReplacesMaterialPartNumber.Trim();
                    material.ReplacesMaterialPartNumber = material.ReplacesMaterialPartNumber.TrimEnd(new char[] { ',' });
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve replacement material id: {0}";

                //hadException = true;

                logger.Error(oe, message, material.MaterialItemId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to retrieve replacement material id: {0}", material.MaterialItemId);
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

        public async Task<List<MaterialItem>> GetReplacementMaterialInfoAsync(long materialItemId)
        {
            List<MaterialItem> material = null;

            await Task.Run(() =>
            {
                material = GetReplacementMaterialInfo(materialItemId);
            });

            return material;
        }

        public List<MaterialItem> GetReplacementMaterialInfo(long materialItemId)
        {
            List<MaterialItem> materials = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MTRL_PKG.GET_REPLACEMENT_MTL_INFO", parameters);

                while (reader.Read())
                {
                    MaterialItem material = new MaterialItem();

                    if (materials == null)
                        materials = new List<MaterialItem>();

                    //SELECT v2.part_no, v2.material_item_id, v2.product_id, mfr.mfr_cd, nvl(m.mtrl_dsc, v2.item_desc) AS mtl_desc, a.cmnt_txt, 
                    //'N' AS is_replacement, 'Y' AS is_replaced
                    Models.Attribute manufacturer = new Models.Attribute(MaterialType.JSON.Mfg, DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                    Models.Attribute catalogDescription = new Models.Attribute(MaterialType.JSON.CtlgDesc, DataReaderHelper.GetNonNullValue(reader, "mtl_desc"));
                    Models.Attribute id = new Models.Attribute(MaterialType.JSON.MaterialItemId, DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                    Models.Attribute partNumber = new Models.Attribute(MaterialType.JSON.PrtNo, DataReaderHelper.GetNonNullValue(reader, "part_no"));
                    Models.Attribute productId = new Models.Attribute(MaterialType.JSON.PrdctId, DataReaderHelper.GetNonNullValue(reader, "product_id"));
                    Models.Attribute comment = new Models.Attribute("cmntTxt", DataReaderHelper.GetNonNullValue(reader, "cmnt_txt"));
                    Models.Attribute apcl = new Models.Attribute(MaterialType.JSON.Apcl, DataReaderHelper.GetNonNullValue(reader, "value"));

                    material.Attributes.Add(productId.Name, productId);
                    material.Attributes.Add(comment.Name, comment);
                    material.Attributes.Add(manufacturer.Name, manufacturer);
                    material.Attributes.Add(catalogDescription.Name, catalogDescription);
                    material.Attributes.Add(id.Name, id);
                    material.Attributes.Add(partNumber.Name, partNumber);
                    material.Attributes.Add(apcl.Name, apcl);

                    materials.Add(material);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get material replacement information [{0}].";

                //hadException = true;

                logger.Error(oe, message, materialItemId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to get material replacement information [{0}].", materialItemId);
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

            return materials;
        }

        public async Task<List<MaterialItem>> GetChainingMaterialInfoAsync(long materialItemId)
        {
            List<MaterialItem> material = null;

            await Task.Run(() =>
            {
                material = GetChainingMaterialInfo(materialItemId);
            });

            return material;
        }

        public List<MaterialItem> GetChainingMaterialInfo(long materialItemId)
        {
            List<MaterialItem> materials = null;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("MTRL_PKG.GET_CHAINING_MTL_INFO", parameters);

                while (reader.Read())
                {
                    MaterialItem material = new MaterialItem();

                    if (materials == null)
                        materials = new List<MaterialItem>();

                    //SELECT v2.part_no, v2.material_item_id, v2.product_id, mfr.mfr_cd, nvl(m.mtrl_dsc, v2.item_desc) AS mtl_desc, a.cmnt_txt, 
                    //'N' AS is_child, 'Y' AS is_parent
                    Models.Attribute manufacturer = new Models.Attribute(MaterialType.JSON.Mfg, DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                    Models.Attribute catalogDescription = new Models.Attribute(MaterialType.JSON.CtlgDesc, DataReaderHelper.GetNonNullValue(reader, "mtl_desc"));
                    Models.Attribute id = new Models.Attribute(MaterialType.JSON.MaterialItemId, DataReaderHelper.GetNonNullValue(reader, "material_item_id", true));
                    Models.Attribute partNumber = new Models.Attribute(MaterialType.JSON.PrtNo, DataReaderHelper.GetNonNullValue(reader, "part_no"));
                    Models.Attribute productId = new Models.Attribute(MaterialType.JSON.PrdctId, DataReaderHelper.GetNonNullValue(reader, "product_id"));
                    Models.Attribute comment = new Models.Attribute("cmntTxt", DataReaderHelper.GetNonNullValue(reader, "cmnt_txt"));
                    Models.Attribute isChild = new Models.Attribute("is_child", DataReaderHelper.GetNonNullValue(reader, "is_child"));

                    material.Attributes.Add(productId.Name, productId);
                    material.Attributes.Add(comment.Name, comment);
                    material.Attributes.Add(manufacturer.Name, manufacturer);
                    material.Attributes.Add(catalogDescription.Name, catalogDescription);
                    material.Attributes.Add(id.Name, id);
                    material.Attributes.Add(partNumber.Name, partNumber);
                    material.Attributes.Add(isChild.Name, isChild);

                    materials.Add(material);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get material replacement information [{0}].";

                //hadException = true;

                logger.Error(oe, message, materialItemId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to get material replacement information [{0}].", materialItemId);
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

            return materials;
        }

        public void HasRevisions(IMaterial material)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string output = "";

            if (material != null && material is MaterialRevision)
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pMtrlId", DbType.Int64, material.MaterialId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pTbl", DbType.String, ((MaterialRevision)material).RevisionTableName, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("oRvsns", DbType.String, output, ParameterDirection.Output, 10);

                    dbManager.ExecuteScalarSP("MATERIAL_REVISIONS_PKG.MATERIAL_HAS_REVISIONS", parameters);

                    if ("Y".Equals(parameters[2].Value.ToString()))
                        material.HasRevisions = true;
                    else
                        material.HasRevisions = false;
                }
                catch (OracleException oe)
                {
                    string message = "Unable to get material revision information [{0}].";

                    //hadException = true;

                    logger.Error(oe, message, material.MaterialItemId);
                    //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //hadException = true;

                    logger.Error(ex, "Unable to get material revision information [{0}].", material.MaterialItemId);
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
            else
                material.HasRevisions = false;
        }

        public object CheckNullValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return DBNull.Value;
            else
                return val.ToUpper();
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
    }
}