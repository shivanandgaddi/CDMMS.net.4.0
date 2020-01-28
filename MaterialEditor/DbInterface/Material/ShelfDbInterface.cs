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
    public class ShelfDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ShelfDbInterface() : base()
        {
        }

        public ShelfDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial shelf = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("rme_shelf_mtrl_pkg.get_shelf", parameters);

                if (reader.Read())
                {
                    shelf = new Shelf(materialItemId, mtrlId);

                    shelf.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    shelf.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    shelf.ManufacturerName = DataReaderHelper.GetNonNullValue(reader, "mfr_nm");
                    shelf.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    shelf.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    shelf.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    shelf.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    shelf.SpecificationName = DataReaderHelper.GetNonNullValue(reader, "shelf_specn_nm");
                    shelf.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_specn_id", true));
                    shelf.SpecificationRevisionName = DataReaderHelper.GetNonNullValue(reader, "shelf_specn_revsn_nm");
                    shelf.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_specn_revsn_alt_id", true));
                    ((Shelf)shelf).BaseRevisionInd = DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind");
                    ((Shelf)shelf).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    ((Shelf)shelf).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((Shelf)shelf).CurrentRevisionInd = DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind");
                    ((Shelf)shelf).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((Shelf)shelf).RetiredRevisionInd = DataReaderHelper.GetNonNullValue(reader, "ret_revsn_ind");
                    ((Shelf)shelf).RevisionNumber = DataReaderHelper.GetNonNullValue(reader, "revsn_no");
                    ((Shelf)shelf).DimensionsUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    ((Shelf)shelf).HeatDissipation = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "het_dssptn_no", true));
                    ((Shelf)shelf).HeatDissipationUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "het_dssptn_uom_id", true));
                    ((Shelf)shelf).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    ((Shelf)shelf).Weight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_wt_no", true));
                    ((Shelf)shelf).WeightUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_wt_uom_id", true));
                    ((Shelf)shelf).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    ((Shelf)shelf).MaxElectricalCurrentDrain = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_max_drn_no", true));
                    ((Shelf)shelf).MaxElectricalCurrentDrainUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_max_drn_uom_id", true));
                    ((Shelf)shelf).NormalElectricalCurrentDrain = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_norm_drn_no", true));
                    ((Shelf)shelf).NormalElectricalCurrentDrainUnitOfMeasure = int.Parse(DataReaderHelper.GetNonNullValue(reader, "elc_curr_norm_drn_uom_id", true));

                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");
                    string specificationPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind");

                    if ("Y".Equals(specificationPropagated))
                        ((Shelf)shelf).SpecificationPropagated = true;

                    if ("Y".Equals(recordOnly))
                        shelf.IsRecordOnly = true;
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

            return shelf;
        }

        public void UpdateRevisionData(long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd,
            string clei, int orderableMaterialStatusId, long specId, decimal heatDissipation, int heatDissipationUom, decimal weight, int weightUom)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE update_revision_data(pMtlItmId IN NUMBER, pRvsn IN rme_shelf_mtrl_revsn.revsn_no % TYPE,
                //pBaseRvsn IN rme_shelf_mtrl_revsn.base_revsn_ind % TYPE, pCurrRvsn IN rme_shelf_mtrl_revsn.curr_revsn_ind % TYPE,
                //pRetRvsn IN rme_shelf_mtrl_revsn.ret_revsn_ind % TYPE, pOrdbl IN rme_shelf_mtrl_revsn.ordbl_mtrl_stus_id % TYPE,
                //pSpecId IN rme_shelf_mtrl_revsn.shelf_specn_revsn_alt_id % TYPE, pClei IN rme_shelf_mtrl_revsn.clei_cd % TYPE,
                //pHetNo IN NUMBER, pHetUom IN NUMBER, pShelfWt IN NUMBER, pShelfWtUom IN NUMBER)
                parameters = dbAccessor.GetParameterArray(12);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pBaseRvsn", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pCurrRvsn", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pRetRvsn", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pOrdbl", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pSpecId", DbType.Int64, CheckNullValue(specId), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUom == 0)
                    parameters[9] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[9] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUom, ParameterDirection.Input);

                parameters[10] = dbAccessor.GetParameter("pCardWt", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUom == 0)
                    parameters[11] = dbAccessor.GetParameter("pCardWtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[11] = dbAccessor.GetParameter("pCardWtUom", DbType.Int32, weightUom, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_SHELF_MTRL_PKG.UPDATE_REVISION_DATA", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_shelf_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void UpdateShelfData(long mtrlId, decimal depth, decimal height, decimal width, int uomId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(5);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_SHELF_MTRL_PKG.UPDATE_SHELF_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_shelf_mtrl ({0}, {1}, {2}, {3}, {4})", mtrlId, depth, height, width, uomId);

                throw ex;
            }
        }

        public long[] CreateShelf(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, decimal depth, decimal height, decimal width, int dimensionsUomId,
            long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId,
            decimal weight, int weightUomId, decimal heatDissipation, int heatDissipationUomId, string materialCode, long inMtrlId)
        {
            long mtrlId = 0;
            long outMaterialItemId = 0;
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE create_shelf(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pDpth IN NUMBER, pHgt IN NUMBER, pWdth IN NUMBER, 
                //pDimUom IN NUMBER, pMtlItmId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2,
                //pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRvsnRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2,
                //pHetNo IN NUMBER, pHetUom IN NUMBER, pShelfWt IN NUMBER, pShelfWtUom IN NUMBER, oMtrlId OUT NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(30);

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
                parameters[11] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depth, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pHgt", DbType.Decimal, height, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pWdth", DbType.Decimal, width, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pDimUom", DbType.Int32, dimensionsUomId, ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pRevsnNo", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("pBaseInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[19] = dbAccessor.GetParameter("pCurrInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[20] = dbAccessor.GetParameter("pRvsnRetInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[21] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[21] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[22] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[23] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUomId == 0)
                    parameters[24] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[24] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUomId, ParameterDirection.Input);

                parameters[25] = dbAccessor.GetParameter("pShelfWt", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUomId == 0)
                    parameters[26] = dbAccessor.GetParameter("pShelfWtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[26] = dbAccessor.GetParameter("pShelfWtUom", DbType.Int32, weightUomId, ParameterDirection.Input);

                parameters[27] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[28] = dbAccessor.GetParameter("oMtrlId", DbType.Int64, mtrlId, ParameterDirection.Output);
                parameters[29] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "rme_shelf_mtrl_pkg.create_shelf", parameters);

                mtrlId = long.Parse(parameters[28].Value.ToString());
                outMaterialItemId = long.Parse(parameters[29].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create rme_shelf_mtrl ({0})", materialItemId);

                throw ex;
            }

            return new long[] { outMaterialItemId, mtrlId, 0, 0 };
        }

        public void CreateRevision(long materialItemId, long mtrlId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId,
            decimal weight, int weightUomId, decimal heatDissipation, int heatDissipationUomId, string materialCode)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE insert_revision(pMtlItmId IN NUMBER, pMtrlId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2,
                //pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2,
                //pHetNo IN NUMBER, pHetUom IN NUMBER, pShelfWt IN NUMBER, pShelfWtUom IN NUMBER)
                parameters = dbAccessor.GetParameterArray(13);

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
                parameters[9] = dbAccessor.GetParameter("pHetNo", DbType.Decimal, CheckNullValue(heatDissipation), ParameterDirection.Input);

                if (heatDissipationUomId == 0)
                    parameters[10] = dbAccessor.GetParameter("pHetUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[10] = dbAccessor.GetParameter("pHetUom", DbType.Int32, heatDissipationUomId, ParameterDirection.Input);

                parameters[11] = dbAccessor.GetParameter("pShelfWt", DbType.Decimal, CheckNullValue(weight), ParameterDirection.Input);

                if (weightUomId == 0)
                    parameters[12] = dbAccessor.GetParameter("pShelfWtUom", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[12] = dbAccessor.GetParameter("pShelfWtUom", DbType.Int32, weightUomId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "rme_shelf_mtrl_pkg.insert_revision", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create rme_shelf_mtrl_revsn ({0}, {1})", materialItemId, mtrlId);

                throw ex;
            }
        }

        public void UpdateShelfSpecificationDescription(long specId, string description)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, description, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "rme_shelf_mtrl_pkg.update_shelf_specn_desc", parameters);
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "shelf_specn_pkg.associate_material", parameters);

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