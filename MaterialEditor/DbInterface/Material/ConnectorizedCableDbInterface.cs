using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class ConnectorizedCableDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ConnectorizedCableDbInterface() : base()
        {
        }

        public ConnectorizedCableDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial cable = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.GET_CONNECTORIZED_CABLE", parameters);

                if (reader.Read())
                {
                    cable = new ConnectorizedCable(materialItemId, mtrlId);

                    cable.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    cable.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    cable.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    cable.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    cable.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    cable.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((ConnectorizedCable)cable).BaseRevisionInd = DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind");
                    ((ConnectorizedCable)cable).CableTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "cabl_typ_id", true));
                    ((ConnectorizedCable)cable).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((ConnectorizedCable)cable).CurrentRevisionInd = DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind");
                    ((ConnectorizedCable)cable).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((ConnectorizedCable)cable).RetiredRevisionInd = DataReaderHelper.GetNonNullValue(reader, "ret_revsn_ind");
                    ((ConnectorizedCable)cable).RevisionNumber = DataReaderHelper.GetNonNullValue(reader, "revsn_no");
                    ((ConnectorizedCable)cable).SetLength = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "set_lgth_no", true));
                    ((ConnectorizedCable)cable).SetLengthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "set_lgth_uom_id", true));
                    ((ConnectorizedCable)cable).VariableLengthCableMtrlId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "var_lgth_cabl_mtrl_id", true));
                    ((ConnectorizedCable)cable).VariableLengthCableProductId = DataReaderHelper.GetNonNullValue(reader, "var_lgth_part_no");

                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");

                    if ("Y".Equals(recordOnly))
                        cable.IsRecordOnly = true;
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve material id: {0}";

                //hadException = true;

                logger.Error(oe, message, materialItemId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

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

            return cable;
        }

        public void UpdateRevisionData(long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId)
        {
            IDbDataParameter[] parameters = null;

            //if (string.IsNullOrEmpty(cuid))
            //    cuid = "cdmms";

            try
            {
                parameters = dbAccessor.GetParameterArray(7);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pBaseRvsnInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCurrRvsnInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pRetRvsnInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pOrdbleId", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_CNCTRZD_CABL_MTRL_PKG.UPDATE_REVISION_DATA", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_cnctrzd_cabl_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void UpdateCableData(long mtrlId, long variableCableId, decimal setLength, int uomId, int cableTypeId, long specificationId)
        {
            IDbDataParameter[] parameters = null;

            //if (string.IsNullOrEmpty(cuid))
            //    cuid = "cdmms";

            try
            {
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);

                if(variableCableId > 0)
                    parameters[1] = dbAccessor.GetParameter("pVarLgthId", DbType.Int64, variableCableId, ParameterDirection.Input);
                else
                    parameters[1] = dbAccessor.GetParameter("pVarLgthId", DbType.Int64, null, ParameterDirection.Input);

                parameters[2] = dbAccessor.GetParameter("pSetLgth", DbType.Decimal, setLength, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCblTypId", DbType.Int32, CheckNullValue(cableTypeId), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, CheckNullValue(specificationId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_CNCTRZD_CABL_MTRL_PKG.UPDATE_RME_CNCTRZD_CABL_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_cnctrzd_cabl_mtrl ({0}, {1}, {2}, {3})", mtrlId, variableCableId, setLength, uomId);

                throw ex;
            }
        }

        public async Task<List<MaterialItem>> GetAssociatedCable(long materialItemId)
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

                    parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.GET_ASSOCIATED_CABLE_INFO", parameters);

                    while (reader.Read())
                    {
                        if (items == null)
                            items = new List<MaterialItem>();

                        MaterialItem cable = new MaterialItem();

                        Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "cdmms_id", true));
                        Models.Attribute catalogDescription = new Models.Attribute("mat_desc", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                        Models.Attribute manufacturer = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute rootPartNumber = new Models.Attribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "part_no"));
                        Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cd"));
                        Models.Attribute spec = new Models.Attribute("spec_nm", DataReaderHelper.GetNonNullValue(reader, "spec"));

                        cable.Attributes.Add(id.Name, id);
                        cable.Attributes.Add(catalogDescription.Name, catalogDescription);
                        cable.Attributes.Add(manufacturer.Name, manufacturer);
                        cable.Attributes.Add(rootPartNumber.Name, rootPartNumber);
                        cable.Attributes.Add(materialCode.Name, materialCode);
                        cable.Attributes.Add(spec.Name, spec);

                        items.Add(cable);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to retrieve associated cable id: {0}";

                    //hadException = true;

                    logger.Error(oe, message, materialItemId);
                    //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //hadException = true;

                    logger.Error(ex, "Unable to retrieve associated cable id: {0}", materialItemId);
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

        public async Task<List<MaterialItem>> SearchForCablesToAssociateAsync(string productId, string partNumber, string description, string clmc, string cdmmsid, string source)
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

                    parameters = dbManager.GetParameterArray(7);

                    //PROCEDURE SEARCH(pSrc IN VARCHAR2, pMtrlCd IN VARCHAR2, pMfrCd IN VARCHAR2, pPrtNo IN VARCHAR2, pMtrlDsc IN VARCHAR2, pId IN VARCHAR2, retcsr OUT ref_cursor)
                    parameters[0] = dbManager.GetParameter("pSrc", DbType.String, source, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pMtrlCd", DbType.String, productId.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pMfrCd", DbType.String, clmc.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pPrtNo", DbType.String, partNumber.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pMtrlDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pId", DbType.String, cdmmsid.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("RME_CNCTRZD_CABL_MTRL_PKG.SEARCH", parameters);

                    while (reader.Read())
                    {
                        if (items == null)
                            items = new List<MaterialItem>();

                        MaterialItem cable = new MaterialItem();

                        Models.Attribute id = new Models.Attribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "cdmms_id", true));
                        Models.Attribute catalogDescription = new Models.Attribute("mat_desc", DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc"));
                        Models.Attribute manufacturer = new Models.Attribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd"));
                        Models.Attribute rootPartNumber = new Models.Attribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "part_no"));
                        Models.Attribute materialCode = new Models.Attribute("product_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cd"));
                        Models.Attribute spec = new Models.Attribute("spec_nm", DataReaderHelper.GetNonNullValue(reader, "spec"));

                        cable.Attributes.Add(id.Name, id);
                        cable.Attributes.Add(catalogDescription.Name, catalogDescription);
                        cable.Attributes.Add(manufacturer.Name, manufacturer);
                        cable.Attributes.Add(rootPartNumber.Name, rootPartNumber);
                        cable.Attributes.Add(materialCode.Name, materialCode);
                        cable.Attributes.Add(spec.Name, spec);

                        items.Add(cable);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to search for cables to associate.";

                    //hadException = true;

                    logger.Error(oe, message);
                    //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    //hadException = true;

                    logger.Error(ex, "Unable to search for cables to associate.");
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

        public async Task<string> UpdateCableAssociation(long materialItemId, long idToAssociate, string source)
        {
            IDbDataParameter[] parameters = null;
            string status = "Success";

            await Task.Run(() =>
            {
                try
                {
                    parameters = dbAccessor.GetParameterArray(3);

                    parameters[0] = dbAccessor.GetParameter("pSrc", DbType.String, source, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pIdToUpdate", DbType.Int64, materialItemId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pId", DbType.Int64, idToAssociate, ParameterDirection.Input);

                    dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_CNCTRZD_CABL_MTRL_PKG.UPDATE_ASSOCIATION", parameters);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to update rme_cnctrzd_cabl_mtrl ({0}, {1}, {2})", materialItemId, idToAssociate, source);

                    throw ex;
                }
            });

            return status;
        }

        public long[] CreateConnectorizedCable(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, long variableCableId, decimal setLength, int uomId, long materialItemId, int cableTypeId, 
            string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId, string materialCode, long inMtrlId)
        {
            long mtrlId = 0;
            long outMaterialItemId = 0;
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE CREATE_CNCTRZD_CABL(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pVarCblMtrlId IN NUMBER, pSetLgthNo IN NUMBER, 
                //pUomId IN NUMBER, pCblTypId IN NUMBER, pSpecId IN NUMBER, pMtlItmId IN NUMBER, 
                //pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRvsnRetInd IN VARCHAR2, 
                //pOrdblId IN NUMBER, pClei IN VARCHAR2, oMtrlId OUT NUMBER, oMtlItmId OUT NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(27);

                parameters[0] = dbAccessor.GetParameter("pMfrId", DbType.Int64, mfrId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCatId", DbType.Int32, materialCategoryId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRoInd", DbType.String, recordOnly ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDsc", DbType.String, description, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pRetInd", DbType.String, retiredInd, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pLbrId", DbType.Int32, laborId, ParameterDirection.Input);

                parameters[9] = dbAccessor.GetParameter("pFeatTyp", DbType.Int64, featureTypeId, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pSpecnInitInd", DbType.String, specificationInitInd, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pVarCblMtrlId", DbType.Int64, CheckNullValue(variableCableId), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pSetLgthNo", DbType.Decimal, setLength, ParameterDirection.Input);

                if (uomId == 0)
                    parameters[13] = dbAccessor.GetParameter("pUomId", DbType.Int32, null, ParameterDirection.Input);
                else
                    parameters[13] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);

                parameters[14] = dbAccessor.GetParameter("pCblTypId", DbType.Int64, CheckNullValue(cableTypeId), ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pSpecId", DbType.Int64, CheckNullValue(specId), ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pRevsnNo", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[19] = dbAccessor.GetParameter("pBaseInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[20] = dbAccessor.GetParameter("pCurrInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[21] = dbAccessor.GetParameter("pRvsnRetInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[22] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[22] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[23] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[24] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[25] = dbAccessor.GetParameter("oMtrlId", DbType.Int64, mtrlId, ParameterDirection.Output);
                parameters[26] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_CNCTRZD_CABL_MTRL_PKG.CREATE_CNCTRZD_CABL", parameters);

                mtrlId = long.Parse(parameters[25].Value.ToString());
                outMaterialItemId = long.Parse(parameters[26].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create rme_cnctrzd_cabl_mtrl ({0})", materialItemId);

                throw ex;
            }

            return new long[] { outMaterialItemId, mtrlId, 0 };
        }

        public void CreateRevision(long materialItemId, long mtrlId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, string materialCode)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE INSERT_REVISION(pMtlItmId IN NUMBER, pMtrlId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2)
                parameters = dbAccessor.GetParameterArray(9);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRevsnNo", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pBaseInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCurrInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pRvsnRetInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[7] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[7] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[8] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_CNCTRZD_CABL_MTRL_PKG.INSERT_REVISION", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert rme_cnctrzd_cabl_mtrl ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", materialItemId, mtrlId, revisionNumber, baseRevisionInd, currentRevisionInd,
                    retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }
    }
}