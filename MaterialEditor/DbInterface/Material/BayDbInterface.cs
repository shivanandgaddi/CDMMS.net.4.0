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
    public class BayDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BayDbInterface() : base()
        {
        }

        public BayDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial bay = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("rme_bay_mtrl_pkg.get_bay", parameters);

                if (reader.Read())
                {
                    bay = new Bay(materialItemId, mtrlId);

                    bay.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    bay.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    bay.ManufacturerName = DataReaderHelper.GetNonNullValue(reader, "mfr_nm");
                    bay.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    bay.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    bay.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    bay.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    bay.SpecificationName = DataReaderHelper.GetNonNullValue(reader, "bay_specn_nm");
                    bay.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_specn_id", true));
                    bay.SpecificationRevisionName = DataReaderHelper.GetNonNullValue(reader, "bay_specn_revsn_nm");
                    bay.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_specn_revsn_alt_id", true));
                    ((Bay)bay).BaseRevisionInd = DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind");
                    ((Bay)bay).ExternalDepth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_dpth_no", true));
                    ((Bay)bay).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((Bay)bay).CurrentRevisionInd = DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind");
                    ((Bay)bay).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((Bay)bay).RetiredRevisionInd = DataReaderHelper.GetNonNullValue(reader, "ret_revsn_ind");
                    ((Bay)bay).RevisionNumber = DataReaderHelper.GetNonNullValue(reader, "revsn_no");
                    ((Bay)bay).ExternalDimensionsUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_dim_uom_id", true));
                    ((Bay)bay).HeatDissipation = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "het_dssptn_no", true));
                    ((Bay)bay).HeatDissipationUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "het_dssptn_uom_id", true));
                    ((Bay)bay).ExternalHeight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_hgt_no", true));
                    ((Bay)bay).MaxElectricalCurrentDrain = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_max_drn_no", true));
                    ((Bay)bay).MaxElectricalCurrentDrainUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_max_drn_uom_id", true));
                    ((Bay)bay).NormalElectricalCurrentDrain = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_norm_drn_no", true));
                    ((Bay)bay).NormalElectricalCurrentDrainUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_norm_drn_uom_id", true));
                    ((Bay)bay).Weight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_wt_no", true));
                    ((Bay)bay).WeightUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_wt_uom_id", true));
                    ((Bay)bay).ExternalWidth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_wdth_no", true));
                    ((Bay)bay).PlannedHeatGeneration = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "plnd_het_gntn_no", true));
                    ((Bay)bay).PlannedHeatGenerationUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "plnd_het_gntn_uom_id", true));
                    ((Bay)bay).InternalDepth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "intrl_dpth", true));
                    ((Bay)bay).InternalHeight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "intrl_hght", true));
                    ((Bay)bay).InternalWidth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "intrl_wdth", true));

                    string cabinetIndicator = DataReaderHelper.GetNonNullValue(reader, "cab_ind");
                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");
                    string specificationPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind");

                    if ("Y".Equals(specificationPropagated))
                        ((Bay)bay).SpecificationPropagated = true;

                    if ("Y".Equals(cabinetIndicator))
                        ((Bay)bay).CabinetIndicator = true;

                    if ("Y".Equals(recordOnly))
                        bay.IsRecordOnly = true;
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

            return bay;
        }

        public void UpdateRevisionData(long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd,
            string clei, int orderableMaterialStatusId, decimal plannedHeatGeneration, int plannedHeatGenerationUom, decimal normalDrain, int normalDrainUom, 
            decimal maxDrain, int maxDrainUom, decimal heatDissipation, int heatDissipationUom, decimal weight, int weightUom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE UPDATE_REVISION_DATA(pMtlItmId IN NUMBER, pRvsn IN rme_bay_mtrl_revsn.revsn_no%TYPE, pBaseRvsnInd IN rme_bay_mtrl_revsn.base_revsn_ind%TYPE,
                //pCurrRvsnInd IN rme_bay_mtrl_revsn.curr_revsn_ind % TYPE, pRetRvsnInd IN rme_bay_mtrl_revsn.ret_revsn_ind % TYPE,
                //pClei IN rme_bay_mtrl_revsn.clei_cd % TYPE, pOrdbleId IN rme_bay_mtrl_revsn.ordbl_mtrl_stus_id % TYPE,
                //pPlndHtGntn IN NUMBER, pPlndHtGntnUom IN NUMBER, pNrmDrn IN NUMBER, pNrmDrnUom IN NUMBER, pMaxDrn IN NUMBER,
                //pMaxDrnUom IN NUMBER, pWght IN NUMBER, pWghtUom IN NUMBER, pHetNo IN NUMBER, pHetUom IN NUMBER)
                parameters = dbAccessor.GetParameterArray(17);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pBaseRvsn", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCurrRvsn", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pRetRvsn", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pOrdbl", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pPlndHtGntn", DbType.Decimal, CheckNullValue(plannedHeatGeneration), ParameterDirection.Input);

                if (plannedHeatGenerationUom == 0)
                    parameters[8] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[8] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, plannedHeatGenerationUom, ParameterDirection.Input);

                parameters[9] = dbAccessor.GetParameter("pNrmDrn", DbType.Decimal, CheckNullValue(normalDrain), ParameterDirection.Input);

                if (normalDrainUom == 0)
                    parameters[10] = dbAccessor.GetParameter("pNrmDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[10] = dbAccessor.GetParameter("pNrmDrnUom", DbType.Int32, normalDrainUom, ParameterDirection.Input);

                parameters[11] = dbAccessor.GetParameter("pMaxDrn", DbType.Decimal, CheckNullValue(maxDrain), ParameterDirection.Input);

                if (maxDrainUom == 0)
                    parameters[12] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[12] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, maxDrainUom, ParameterDirection.Input);

                parameters[13] = dbAccessor.GetParameter("pWght", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUom == 0)
                    parameters[14] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[14] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, weightUom, ParameterDirection.Input);

                parameters[15] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUom == 0)
                    parameters[16] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[16] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_MTRL_PKG.UPDATE_REVISION_DATA", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_bay_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void CreateRevision(long materialItemId, long mtrlId, string materialCode, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd,
            string clei, int orderableMaterialStatusId, decimal plannedHeatGeneration, int plannedHeatGenerationUom, decimal normalDrain, int normalDrainUom,
            decimal maxDrain, int maxDrainUom, decimal heatDissipation, int heatDissipationUom, decimal weight, int weightUom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE INSERT_REVISION(pMtlItmId IN NUMBER, pMtrlId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2,
                //pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2,
                //pPlndHtGntn IN NUMBER, pPlndHtGntnUom IN NUMBER, pNrmDrn IN NUMBER, pNrmDrnUom IN NUMBER, pMaxDrn IN NUMBER,
                //pMaxDrnUom IN NUMBER, pWght IN NUMBER, pWghtUom IN NUMBER, pHetNo IN NUMBER, pHetUom IN NUMBER)
                parameters = dbAccessor.GetParameterArray(19);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pBaseRvsn", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCurrRvsn", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pRetRvsn", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pOrdbl", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pPlndHtGntn", DbType.Decimal, CheckNullValue(plannedHeatGeneration), ParameterDirection.Input);

                if (plannedHeatGenerationUom == 0)
                    parameters[10] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[10] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, plannedHeatGenerationUom, ParameterDirection.Input);

                parameters[11] = dbAccessor.GetParameter("pNrmDrn", DbType.Decimal, CheckNullValue(normalDrain), ParameterDirection.Input);

                if (normalDrainUom == 0)
                    parameters[12] = dbAccessor.GetParameter("pNrmDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[12] = dbAccessor.GetParameter("pNrmDrnUom", DbType.Int32, normalDrainUom, ParameterDirection.Input);

                parameters[13] = dbAccessor.GetParameter("pMaxDrn", DbType.Decimal, CheckNullValue(maxDrain), ParameterDirection.Input);

                if (maxDrainUom == 0)
                    parameters[14] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[14] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, maxDrainUom, ParameterDirection.Input);

                parameters[15] = dbAccessor.GetParameter("pWght", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUom == 0)
                    parameters[16] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[16] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, weightUom, ParameterDirection.Input);

                parameters[17] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUom == 0)
                    parameters[18] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[18] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_MTRL_PKG.INSERT_REVISION", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert rme_bay_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void UpdateBayData(long mtrlId, decimal depth, decimal height, decimal width, int uomId, bool cabinetIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCabInd", DbType.String, cabinetIndicator ? "Y" : "N", ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_MTRL_PKG.UPDATE_RME_BAY_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_bay_mtrl ({0}, {1}, {2}, {3}, {4})", mtrlId, depth, height, width, uomId);

                throw ex;
            }
        }

        public long[] CreateBay(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, decimal depth, decimal height, decimal width, int dimensionsUomId, string cabinetIndicator,
            long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId,
            decimal plannedHeatGeneration, int plannedHeatGenerationUomId, decimal normalDrain, int normalDrainUomId, decimal maxDrain, int maxDrainUomId, decimal weight,
            int weightUomId, decimal heatDissipation, int heatDissipationUomId, string materialCode, long inMtrlId)
        {            
            long mtrlId = 0;
            long outMaterialItemId = 0;
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE CREATE_BAY(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pDpth IN NUMBER, pHgt IN NUMBER, pWdth IN NUMBER, 
                //pDimUom IN NUMBER, pCabInd IN VARCHAR2, pMtlItmId NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2,
                //pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRvsnRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2,
                //pPlndHtGntn IN NUMBER, pPlndHtGntnUom IN NUMBER, pNrmDrn IN NUMBER, pNrmDrnUom IN NUMBER, pMaxDrn IN NUMBER,
                //pMaxDrnUom IN NUMBER, pWght IN NUMBER, pWghtUom IN NUMBER, pHetNo IN NUMBER, pHetUom IN NUMBER, pMtrlId IN NUMBER, oMtrlId OUT NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(37);

                parameters[0] = dbAccessor.GetParameter("pMfrId", DbType.Int64, mfrId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCatId", DbType.Int32, materialCategoryId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRoInd", DbType.String, recordOnly ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pRetInd", DbType.String, retiredInd, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pLbrId", DbType.Int32, laborId, ParameterDirection.Input);

                parameters[9] = dbAccessor.GetParameter("pFeatTyp", DbType.Int64, featureTypeId, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pSpecnInitInd", DbType.String, specificationInitInd, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);

                if (dimensionsUomId == 0)
                    parameters[14] = dbAccessor.GetParameter("pDimUom", DbType.Int32, null, ParameterDirection.Input);
                else
                    parameters[14] = dbAccessor.GetParameter("pDimUom", DbType.Int32, dimensionsUomId, ParameterDirection.Input);

                parameters[15] = dbAccessor.GetParameter("pCabInd", DbType.String, cabinetIndicator, ParameterDirection.Input);
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
                parameters[24] = dbAccessor.GetParameter("pPlndHtGntn", DbType.Decimal, CheckNullValue(plannedHeatGeneration), ParameterDirection.Input);

                if (plannedHeatGenerationUomId == 0)
                    parameters[25] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[25] = dbAccessor.GetParameter("pPlndHtGntnUom", DbType.Int32, plannedHeatGenerationUomId, ParameterDirection.Input);

                parameters[26] = dbAccessor.GetParameter("pNormDrn", DbType.Decimal, CheckNullValue(normalDrain), ParameterDirection.Input);

                if (normalDrainUomId == 0)
                    parameters[27] = dbAccessor.GetParameter("pNormDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[27] = dbAccessor.GetParameter("pNormDrnUom", DbType.Int32, normalDrainUomId, ParameterDirection.Input);

                parameters[28] = dbAccessor.GetParameter("pMaxDrn", DbType.Decimal, CheckNullValue(maxDrain), ParameterDirection.Input);

                if (maxDrainUomId == 0)
                    parameters[29] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[29] = dbAccessor.GetParameter("pMaxDrnUom", DbType.Int32, maxDrainUomId, ParameterDirection.Input);

                parameters[30] = dbAccessor.GetParameter("pWght", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUomId == 0)
                    parameters[31] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[31] = dbAccessor.GetParameter("pWghtUom", DbType.Int32, weightUomId, ParameterDirection.Input);

                parameters[32] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUomId == 0)
                    parameters[33] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[33] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUomId, ParameterDirection.Input);

                parameters[34] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);

                parameters[35] = dbAccessor.GetParameter("oMtrlId", DbType.Int64, mtrlId, ParameterDirection.Output);
                parameters[36] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, mtrlId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_MTRL_PKG.CREATE_BAY", parameters);

                mtrlId = long.Parse(parameters[35].Value.ToString());
                outMaterialItemId = long.Parse(parameters[36].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create rme_bay_mtrl ({0})", materialItemId);

                throw ex;
            }

            return new long[] { outMaterialItemId, mtrlId, 0, 0 };
        }

        public void UpdateBaySpecificationDescription(long specId, string description)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, description, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_MTRL_PKG.UPDATE_BAY_SPECN_DESC", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update specification description ({0}, {1})", specId, description);

                throw ex;
            }
        }

        public void ChangeSpecificationType(long specId, long specRevisionId, int oldFeatureType, int newFeatureType)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("pSpecId", DbType.Int64, specId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pSpecRvsnId", DbType.Int64, specRevisionId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pOldFeatTyp", DbType.Int64, oldFeatureType, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pNewFeatTyp", DbType.Int64, newFeatureType, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "SPECN_PKG.CHANGE_SPEC_TYPE", parameters);
                //PROCEDURE CHANGE_SPEC_TYPE(pSpecId IN NUMBER, pSpecRvsnId IN NUMBER, pOldFeatTyp IN NUMBER, pNewFeatTyp IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update specification type ({0}, {1}, {2}, {3})", specId, specRevisionId, oldFeatureType, newFeatureType);

                throw ex;
            }
        }

        public void AssociateMaterial(long specificationRevisionId, long materialItemId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnRvsnId", DbType.Int64, CheckNullValue(specificationRevisionId), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_specn_pkg.associate_material", parameters);

                //PROCEDURE associate_material(pSpecnRvsnId IN NUMBER, pMtlItmId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to associate material ({0}, {1})", specificationRevisionId, materialItemId);

                throw ex;
            }
        }
    }
}