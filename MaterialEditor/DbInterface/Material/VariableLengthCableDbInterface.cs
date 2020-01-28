using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material
{
    public class VariableLengthCableDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public VariableLengthCableDbInterface() : base()
        {
        }

        public VariableLengthCableDbInterface(string dbConnectionString) : base(dbConnectionString)
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

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("RME_VAR_LGTH_CABL_MTRL_PKG.GET_VARIABLE_LENGTH_CABLE", parameters);

                if (reader.Read())
                {
                    cable = new VariableLengthCable(materialItemId, mtrlId);

                    cable.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    cable.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    cable.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    cable.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    cable.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    cable.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    cable.IsRecordOnly = true;
                    ((VariableLengthCable)cable).ProductId = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((VariableLengthCable)cable).CableTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "cabl_typ_id", true));
                    ((VariableLengthCable)cable).SetLengthUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "set_lgth_uom_id", true));
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

        //public override void HasRevisions(IMaterial material)
        //{
            
        //}

        public void UpdateCableData(long mtrlId, int cableTypeId, int uomId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCblTypId", DbType.Int32, cableTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pUomId", DbType.Int32, uomId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_VAR_LGTH_CABL_MTRL_PKG.UPDATE_VARIABLE_LENGTH_CABLE", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update rme_cnctrzd_cabl_mtrl ({0}, {1}, {2})", mtrlId, cableTypeId, uomId);

                throw ex;
            }
        }

        public long CreateVariableLengthCable(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, int uomId, long materialItemId, int cableTypeId, long specId, string materialCode, long inMtrlId)
        {
            IDbDataParameter[] parameters = null;
            long outMaterialItemId = 0;

            try
            {
                //PROCEDURE CREATE_VARIABLE_LENGTH_CABLE(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, 
                //pCatId IN mtrl.mtrl_cat_id % TYPE, pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, 
                //pCmpltInd IN mtrl.cmplt_ind % TYPE, pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, 
                //pLbrId IN mtrl.lbr_id % TYPE, pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, 
                //pMtlItmId IN NUMBER, pCblTypId IN NUMBER, pUomId IN NUMBER, pSpecnId IN NUMBER, pMtrlCd IN VARCHAR2, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(18);

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
                parameters[11] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pCblTypId", DbType.Int32, cableTypeId, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pUom", DbType.Int32, uomId, ParameterDirection.Input);                
                parameters[14] = dbAccessor.GetParameter("pSpecnId", DbType.Int32, CheckNullValue(specId), ParameterDirection.Input);
                parameters[15] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[16] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "RME_VAR_LGTH_CABL_MTRL_PKG.CREATE_VARIABLE_LENGTH_CABLE", parameters);

                outMaterialItemId = long.Parse(parameters[17].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create var_lgth_cabl_mtrl ({0})", materialItemId);

                throw ex;
            }

            return outMaterialItemId;
        }
    }
}