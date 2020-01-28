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
    public class BayExtenderDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BayExtenderDbInterface() : base()
        {
        }

        public BayExtenderDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial bayExtender = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("RME_BAY_EXTNDR_MTRL_PKG.GET_BAY_EXTENDER", parameters);

                if (reader.Read())
                {
                    bayExtender = new BayExtender(materialItemId, mtrlId);

                    bayExtender.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    bayExtender.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    bayExtender.ManufacturerName = DataReaderHelper.GetNonNullValue(reader, "mfr_nm");
                    bayExtender.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    bayExtender.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    bayExtender.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    bayExtender.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    bayExtender.SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_id", true));
                    bayExtender.SpecificationName = DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_nm");
                    bayExtender.SpecificationRevisionId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_alt_id", true));
                    bayExtender.SpecificationRevisionName = DataReaderHelper.GetNonNullValue(reader, "bay_extndr_specn_revsn_nm");
                    ((BayExtender)bayExtender).BaseRevisionInd = DataReaderHelper.GetNonNullValue(reader, "base_revsn_ind");
                    ((BayExtender)bayExtender).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((BayExtender)bayExtender).CurrentRevisionInd = DataReaderHelper.GetNonNullValue(reader, "curr_revsn_ind");
                    ((BayExtender)bayExtender).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((BayExtender)bayExtender).RetiredRevisionInd = DataReaderHelper.GetNonNullValue(reader, "ret_revsn_ind");
                    ((BayExtender)bayExtender).RevisionNumber = DataReaderHelper.GetNonNullValue(reader, "revsn_no");
                    ((BayExtender)bayExtender).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    ((BayExtender)bayExtender).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));
                    ((BayExtender)bayExtender).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    ((BayExtender)bayExtender).HeightId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_hgt_id", true));
                    ((BayExtender)bayExtender).WidthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_wdth_id", true));
                    ((BayExtender)bayExtender).DepthId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "bay_extndr_itnl_dpth_id", true));
                    //((BayExtender)bayExtender).UnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));

                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");
                    string specificationPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind");

                    if ("Y".Equals(specificationPropagated))
                        ((BayExtender)bayExtender).SpecificationPropagated = true;

                    if ("Y".Equals(recordOnly))
                        bayExtender.IsRecordOnly = true;
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

            return bayExtender;
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_EXTNDR_MTRL_PKG.UPDATE_REVISION_DATA", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_bay_extndr_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6})", materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void CreateRevision(long materialItemId, long mtrlId, string materialCode, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId)
        {
            IDbDataParameter[] parameters = null;

            //PROCEDURE INSERT_REVISION(pMtlItmId IN NUMBER, pMtrlId IN NUMBER, pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRetInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2)

            try
            {
                parameters = dbAccessor.GetParameterArray(9);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pRvsn", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pBaseRvsnInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCurrRvsnInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pRetRvsnInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pOrdbleId", DbType.Int32, CheckNullValue(orderableMaterialStatusId), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);                

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_EXTNDR_MTRL_PKG.INSERT_REVISION", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert rme_bay_extndr_mtrl_revsn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", materialItemId, mtrlId, materialCode, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId);

                throw ex;
            }
        }

        public void UpdateBayExtender(long mtrlId, long depthId, long heightId, long widthId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDpth", DbType.Int64, depthId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pHgt", DbType.Int64, heightId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pWdth", DbType.Int64, widthId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_EXTNDR_MTRL_PKG.UPDATE_RME_BAY_EXTNDR_MTRL", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_bay_extndr_mtrl ({0}, {1}, {2}, {3})", mtrlId, depthId, heightId, widthId);

                throw ex;
            }
        }

        public long[] CreateBayExtender(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, long depthId, long heightId, long widthId, long materialItemId, 
            string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId, string materialCode, long inMtrlId)
        {
            long mtrlId = 0;
            long outMaterialItemId = 0;
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE CREATE_BAY_EXTNDR(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE,  pDpth IN NUMBER, 
                //pHgt IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, pMtlItmId IN NUMBER, pRevsnNo IN VARCHAR2, 
                //pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRvsnRetInd IN VARCHAR2, pOrdblId IN NUMBER, 
                //pClei IN VARCHAR2, oMtrlId OUT NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(25);

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
                parameters[11] = dbAccessor.GetParameter("pDpth", DbType.Decimal, depthId, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pHgt", DbType.Decimal, heightId, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pWdth", DbType.Decimal, widthId, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pRevsnNo", DbType.String, CheckNullValue(revisionNumber), ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pBaseInd", DbType.String, baseRevisionInd, ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("pCurrInd", DbType.String, currentRevisionInd, ParameterDirection.Input);
                parameters[19] = dbAccessor.GetParameter("pRvsnRetInd", DbType.String, retiredRevisionInd, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[20] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[20] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[21] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[22] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[23] = dbAccessor.GetParameter("oMtrlId", DbType.Int64, mtrlId, ParameterDirection.Output);
                parameters[24] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_EXTNDR_MTRL_PKG.CREATE_BAY_EXTNDR", parameters);

                mtrlId = long.Parse(parameters[23].Value.ToString());
                outMaterialItemId = long.Parse(parameters[24].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create rme_bay_extndr_mtrl ({0})", materialItemId);

                throw ex;
            }

            return new long[] { outMaterialItemId, mtrlId, 0, 0 };
        }

        public void UpdateBayExtenderSpecificationDescription(long specId, string description)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pDsc", DbType.String, description, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_BAY_EXTNDR_MTRL_PKG.UPDATE_BAY_EXTNDR_SPECN_DESC", parameters);
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

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "bay_extndr_specn_pkg.associate_material", parameters);

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