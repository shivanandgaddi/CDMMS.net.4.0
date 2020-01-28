using System;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class MinorMaterialDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MinorMaterialDbInterface() : base()
        {
        }

        public MinorMaterialDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial minorMaterial = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("mnr_mtrl_pkg.get_minor_material", parameters);

                //SELECT m.mtrl_id, m.mfr_id, m.rt_part_no, m.mtrl_cat_id, m.rcrds_only_ind, m.mtrl_dsc, m.lbr_id, 
                //mnr.mtrl_cd, mnr.ordbl_mtrl_stus_id, mnr.clei_cd, mnr.mtrl_uom_id

                if (reader.Read())
                {
                    minorMaterial = new MinorMaterial(materialItemId, mtrlId);

                    minorMaterial.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    minorMaterial.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    minorMaterial.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    minorMaterial.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    minorMaterial.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    minorMaterial.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((MinorMaterial)minorMaterial).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((MinorMaterial)minorMaterial).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((MinorMaterial)minorMaterial).MaterialUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_uom_id", true));

                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");

                    if ("Y".Equals(recordOnly))
                        minorMaterial.IsRecordOnly = true;
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

            return minorMaterial;
        }

        public void UpdateMinorMaterial(long mtrlId, int orderableId, string clei, int materialUomId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE update_mnr_mtrl(pMtrlId IN NUMBER, pOrdbl IN NUMBER, pClei IN VARCHAR2, pUomId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(4);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pOrdbl", DbType.Int64, CheckNullValue(orderableId), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pUomId", DbType.Int64, CheckNullValue(materialUomId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "mnr_mtrl_pkg.update_mnr_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update mnr_mtrl_pkg ({0}, {1}, {2}, {3})", mtrlId, orderableId, clei, materialUomId);

                throw ex;
            }
        }

        public long CreateMinorMaterial(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, int uomId, long materialItemId, string clei, int orderableMaterialStatusId, string materialCode, long inMtrlId)
        {
            IDbDataParameter[] parameters = null;
            long outMaterialItemId = 0;

            try
            {
                //PROCEDURE create_mnr_mtrl(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pMtrlCd IN VARCHAR2, pOrdbl IN NUMBER, 
                //pClei IN VARCHAR2, pUom IN NUMBER, pMtlItmId IN NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(18);

                parameters[0] = dbAccessor.GetParameter("pMfrId", DbType.Int64, mfrId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRtPrtNbr", DbType.String, rootPartNumber, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCatId", DbType.Int32, materialCategoryId, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pRoInd", DbType.String, recordOnly ? "Y" : "N", ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd, ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pRetInd", DbType.String, retiredInd, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pLbrId", DbType.Int32, laborId, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pFeatTyp", DbType.Int64, CheckNullValue(featureTypeId), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pSpecnInitInd", DbType.String, specificationInitInd, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[12] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[12] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[13] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);

                if (uomId == 0)
                    parameters[14] = dbAccessor.GetParameter("pUom", DbType.Int32, null, ParameterDirection.Input);
                else
                    parameters[14] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);

                parameters[15] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "mnr_mtrl_pkg.create_mnr_mtrl", parameters);

                outMaterialItemId = long.Parse(parameters[17].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create mnr_mtrl ({0})", materialItemId);

                throw ex;
            }

            return outMaterialItemId;
        }
    }
}