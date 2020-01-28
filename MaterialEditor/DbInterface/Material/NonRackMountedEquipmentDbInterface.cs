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
    public class NonRackMountedEquipmentDbInterface : MaterialDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public NonRackMountedEquipmentDbInterface() : base()
        {
        }

        public NonRackMountedEquipmentDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override IMaterial GetMaterial(long materialItemId, long mtrlId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            IMaterial nonRmeMtrl = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItmId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("non_rme_mtrl_pkg.get_non_rme_mtrl", parameters);

                if (reader.Read())
                {
                    string cableIndicator = DataReaderHelper.GetNonNullValue(reader, "cabl_ind");
                    string recordOnly = DataReaderHelper.GetNonNullValue(reader, "rcrds_only_ind");
                    int featureTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "feat_typ_id", true));

                    nonRmeMtrl = new NonRackMountedEquipment(materialItemId, mtrlId, featureTypeId);

                    nonRmeMtrl.CatalogDescription = DataReaderHelper.GetNonNullValue(reader, "mtrl_dsc");
                    nonRmeMtrl.Manufacturer = DataReaderHelper.GetNonNullValue(reader, "mfr_cd");
                    nonRmeMtrl.ManufacturerId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "mfr_id", true));
                    nonRmeMtrl.LaborId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "lbr_id", true));
                    nonRmeMtrl.RootPartNumber = DataReaderHelper.GetNonNullValue(reader, "rt_part_no");
                    nonRmeMtrl.MaterialCode = DataReaderHelper.GetNonNullValue(reader, "mtrl_cd");
                    ((NonRackMountedEquipment)nonRmeMtrl).Depth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "dpth_no", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).CLEI = DataReaderHelper.GetNonNullValue(reader, "clei_cd");
                    ((NonRackMountedEquipment)nonRmeMtrl).OrderableMaterialStatusId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "ordbl_mtrl_stus_id", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).DimensionsUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dim_uom_id", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).SpecificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "specn_id", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).CableTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "cabl_typ_id", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).Height = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hgt_no", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).MaterialUnitOfMeasureId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "mtrl_uom_id", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).Length = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "lgth_no", true));
                    ((NonRackMountedEquipment)nonRmeMtrl).Width = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "wdth_no", true));                    

                    if ("Y".Equals(cableIndicator))
                        ((NonRackMountedEquipment)nonRmeMtrl).IsCable = true;

                    if ("Y".Equals(recordOnly))
                        nonRmeMtrl.IsRecordOnly = true;
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

            return nonRmeMtrl;
        }

        public void UpdateNonRmeMtrl(long mtrlId, string materialCode, int orderableId, string clei, int uomId, bool cableIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE update_non_rme_mtrl(pMtrlId IN NUMBER, pMtrlCd IN VARCHAR2, pCblInd IN VARCHAR2, pOrdblId IN NUMBER,
                //pClei IN VARCHAR2, pMtrlUomId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCblInd", DbType.String, cableIndicator ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, CheckNullValue(orderableId), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMtrlUomId", DbType.Int32, CheckNullValue(uomId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.update_non_rme_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update non_rme_mtrl ({0}, {1}, {2}, {3}, {4})", mtrlId, materialCode, orderableId, clei, uomId);

                throw ex;
            }
        }

        public void UpdateCableData(long mtrlId, int cableTypeId, long specId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE update_non_rme_cabl_mtrl(pMtrlId IN NUMBER, pCblTypId IN NUMBER, pSpecnId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCblTypId", DbType.Int32, cableTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, CheckNullValue(specId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.update_non_rme_cabl_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update non_rme_mtrl ({0}, {1}, {2})", mtrlId, cableTypeId, specId);

                throw ex;
            }
        }

        public void UpdateNonCableData(long mtrlId, decimal depth, decimal height, decimal width, decimal length, int uomId, int featureTypeId, long specId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE update_non_rme_non_cabl_mtrl(pMtrlId IN NUMBER, pFtrTypId IN NUMBER, pDpth IN NUMBER, pHght IN NUMBER,
                //pWdth IN NUMBER, pLgth IN NUMBER, pUomId IN NUMBER, pSpecnId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pFtrTypId", DbType.Int32, featureTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpth", DbType.Decimal, CheckNullValue(depth), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pHght", DbType.Decimal, CheckNullValue(height), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pWdth", DbType.Decimal, CheckNullValue(width), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pLgth", DbType.Decimal, CheckNullValue(length), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pUomId", DbType.Int32, CheckNullValue(uomId), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, CheckNullValue(specId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.update_non_rme_non_cabl_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update non_rme_mtrl ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", mtrlId, depth, height, width, length, uomId, featureTypeId, specId);

                throw ex;
            }
        }

        public void InsertNonRmeMtrl(long mtrlId, string materialCode, int orderableId, string clei, int uomId, bool cableIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE insert_non_rme_mtrl(pMtrlId IN NUMBER, pMtrlCd IN VARCHAR2, pCblInd IN VARCHAR2, pOrdblId IN NUMBER,
                //pClei IN VARCHAR2, pMtrlUomId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(6);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pCblInd", DbType.String, cableIndicator ? "Y" : "N", ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, CheckNullValue(orderableId), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pMtrlUomId", DbType.Int32, CheckNullValue(uomId), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.insert_non_rme_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert non_rme_mtrl ({0}, {1}, {2}, {3}, {4})", mtrlId, materialCode, orderableId, clei, uomId);

                throw ex;
            }
        }

        public void InsertCableData(long mtrlId, int cableTypeId, long specId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE insert_non_rme_cabl_mtrl(pMtrlId IN NUMBER, pCblTypId IN NUMBER, pSpecnId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pCblTypId", DbType.Int32, cableTypeId, ParameterDirection.Input);

                if(specId == 0)
                    parameters[2] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[2] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.insert_non_rme_cabl_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert non_rme_mtrl ({0}, {1}, {2})", mtrlId, cableTypeId, specId);

                throw ex;
            }
        }

        public void InsertNonCableData(long mtrlId, decimal depth, decimal height, decimal width, decimal length, int uomId, int featureTypeId, long specId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                //PROCEDURE insert_non_rme_non_cabl_mtrl(pMtrlId IN NUMBER, pFtrTypId IN NUMBER, pDpth IN NUMBER, pHght IN NUMBER,
                //pWdth IN NUMBER, pLgth IN NUMBER, pUomId IN NUMBER, pSpecnId IN NUMBER)
                parameters = dbAccessor.GetParameterArray(8);

                parameters[0] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, mtrlId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pFtrTypId", DbType.Int32, featureTypeId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pDpth", DbType.Decimal, CheckNullValue(depth), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pHght", DbType.Decimal, CheckNullValue(height), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pWdth", DbType.Decimal, CheckNullValue(width), ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pLgth", DbType.Decimal, CheckNullValue(length), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pUomId", DbType.Int32, CheckNullValue(uomId), ParameterDirection.Input);

                if(specId == 0)
                    parameters[7] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[7] = dbAccessor.GetParameter("pSpecnId", DbType.Int64, specId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.insert_non_rme_non_cabl_mtrl", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert non_rme_mtrl ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", mtrlId, depth, height, width, length, uomId, featureTypeId, specId);

                throw ex;
            }
        }

        public long CreateNonRackMountedEquipment(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
            string retiredInd, int laborId, int featureTypeId, string specificationInitInd, int uomId, long materialItemId, string clei, int orderableMaterialStatusId, string materialCode,
            bool isCable, long inMtrlId)
        {
            IDbDataParameter[] parameters = null;
            long outMaterialItemId = 0;

            try
            {
                //PROCEDURE create_non_rme_mtrl(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pMtrlCd IN VARCHAR2, 
                //pCblInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2, pMtrlUomId IN NUMBER, pMtlItmId IN NUMBER, oMtlItmId OUT NUMBER)
                parameters = dbAccessor.GetParameterArray(19);

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
                parameters[11] = dbAccessor.GetParameter("pMtrlCd", DbType.String, materialCode, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pCblInd", DbType.String, isCable ? "Y" : "N", ParameterDirection.Input);

                if (orderableMaterialStatusId == 0)
                    parameters[13] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, DBNull.Value, ParameterDirection.Input);
                else
                    parameters[13] = dbAccessor.GetParameter("pOrdblId", DbType.Int32, orderableMaterialStatusId, ParameterDirection.Input);

                parameters[14] = dbAccessor.GetParameter("pClei", DbType.String, CheckNullValue(clei), ParameterDirection.Input);

                if (uomId == 0)
                    parameters[15] = dbAccessor.GetParameter("pMtrlUomId", DbType.Int32, null, ParameterDirection.Input);
                else
                    parameters[15] = dbAccessor.GetParameter("pMtrlUomId", DbType.Int32, uomId, ParameterDirection.Input);

                parameters[16] = dbAccessor.GetParameter("pMtrlId", DbType.Int64, inMtrlId, ParameterDirection.Input);
                parameters[17] = dbAccessor.GetParameter("pMtlItmId", DbType.Int64, materialItemId, ParameterDirection.Input);
                parameters[18] = dbAccessor.GetParameter("oMtlItmId", DbType.Int64, outMaterialItemId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "non_rme_mtrl_pkg.create_non_rme_mtrl", parameters);

                outMaterialItemId = long.Parse(parameters[18].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create non_rme_mtrl ({0})", materialItemId);

                throw ex;
            }

            return outMaterialItemId;
        }
    }
}