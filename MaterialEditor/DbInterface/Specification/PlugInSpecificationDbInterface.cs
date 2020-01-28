using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class PlugInSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PlugInSpecificationDbInterface() : base()
        {
        }

        public PlugInSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification plugIn = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("plg_in_specn_pkg.get_plg_in_specn", parameters);

                if (reader.Read())
                {
                    //string genericIndicator = DataReaderHelper.GetNonNullValue(reader, "gnrc_ind");
                    int plugInRoleTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_id", true));

                    plugIn = new PlugInSpecification(specificationId);
                    SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                    ((PlugInSpecification)plugIn).PluginUseTypId = dbInterface.GetUseType("Plug-In", plugInRoleTypeId);

                    plugIn.Name = DataReaderHelper.GetNonNullValue(reader, "plg_in_specn_nm");
                    //plugIn.Description = DataReaderHelper.GetNonNullValue(reader, "bay_specn_dsc");
                    //plugIn.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    //plugIn.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    //plugIn.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;
                    List <wavelengthLst> lstStr= getWavelength(specificationId);

                    if (lstStr != null)
                    {
                        foreach (var acct in lstStr)
                        {
                            if(acct.value =="")
                            {
                                ((PlugInSpecification)plugIn).WavelengthId = int.Parse(acct.Id);
                                
                            }
                            else if(acct.value == "T")
                            {
                                ((PlugInSpecification)plugIn).TransmitWavelengthId = int.Parse(acct.Id);
                            }
                            else if (acct.value == "R")
                            {
                                ((PlugInSpecification)plugIn).RecieveWavelengthId = int.Parse(acct.Id);
                            }
                        }
                    }


                    ((PlugInSpecification)plugIn).MaxLightTransmissionDistance = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "max_lite_xmsn_dist_no", true));
                    ((PlugInSpecification)plugIn).MaxLightTransmissionDistanceUom = int.Parse(DataReaderHelper.GetNonNullValue(reader, "dist_uom_id", true));
                    ((PlugInSpecification)plugIn).BiDirectionalIndicator = DataReaderHelper.GetNonNullValue(reader, "bi_drctnl_ind");
                    ((PlugInSpecification)plugIn).VariableWavelengthIndicator = DataReaderHelper.GetNonNullValue(reader, "var_wvlgth_ind");
                    ((PlugInSpecification)plugIn).FormFactorCode = DataReaderHelper.GetNonNullValue(reader, "form_fctr_cd");
                    ((PlugInSpecification)plugIn).FunctionCodeDescription = DataReaderHelper.GetNonNullValue(reader, "fnctn_cd_dsc");
                    ((PlugInSpecification)plugIn).LowTemperature = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "lo_tmprtr_cls_no", true));
                    ((PlugInSpecification)plugIn).HighTemperature = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "hi_tmprtr_cls_no", true));
                    ((PlugInSpecification)plugIn).TransmissionMediaId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "xmsn_med_id", true));
                    ((PlugInSpecification)plugIn).ConnectorTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "cnctr_typ_id", true));
                    ((PlugInSpecification)plugIn).PlugInRoleTypeId = plugInRoleTypeId;
                    ((PlugInSpecification)plugIn).ChannelNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "chnl_no", true));
                    ((PlugInSpecification)plugIn).MultipleFixedWavelengthIndicator = DataReaderHelper.GetNonNullValue(reader, "mult_fx_wvlgth_ind");                   
                    ((PlugInSpecification)plugIn).RevisionId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "RME_PLG_IN_MTRL_REVSN_ID", true));
                    ((PlugInSpecification)plugIn).TransmissionRateLst = GetTransmissionRates(specificationId);
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve specification id: {0}";

                //hadException = true;

                logger.Error(oe, message, specificationId);
                //EventLogger.LogAlarm(oe, string.Format(message, materialItemId), SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                //hadException = true;

                logger.Error(ex, "Unable to retrieve specification id: {0}", specificationId);
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

            return plugIn;
        }

        public class wavelengthLst
        {
            public string Id { get; set; }
            public string value { get; set; }
        }
        public List<wavelengthLst> getWavelength(long pluginId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;           
            wavelengthLst wavlngth;
            List<wavelengthLst> wavelst = null;
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, pluginId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("plg_in_specn_pkg.get_wavelength_dtls", parameters);

                while (reader.Read())
                {
                    wavlngth = new wavelengthLst();
                    wavlngth.Id = DataReaderHelper.GetNonNullValue(reader, "WVLGTH_ID");
                    wavlngth.value = DataReaderHelper.GetNonNullValue(reader, "XMT_RECV_IND");

                    if (wavelst == null)
                        wavelst = new List<wavelengthLst>();
                    wavelst.Add(wavlngth);
                }

            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Plug-In Role Types");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Plug-In Role Types");
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

            return wavelst;
        }

        public string GetTransmissionRates(long pluginId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string transRate = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pId", DbType.Int64, pluginId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("plg_in_specn_pkg.get_transmission_rates", parameters);

                if (reader.Read())
                {
                    transRate = DataReaderHelper.GetNonNullValue(reader, "xmsn_rt_lst");
                }

            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Plug-In Role Types");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Plug-In Role Types");
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

            return transRate;
        }


        public Dictionary<long, PlugInRoleTypeSpecification> GetPlugInRoleTypes()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Dictionary<long, PlugInRoleTypeSpecification> roleTypes = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(1);

                parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("plg_in_specn_pkg.get_all_plg_in_role_typ", parameters);

                while (reader.Read())
                {
                    long specificationId = long.Parse(DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_id", true));
                    PlugInRoleTypeSpecification plugInRoleType = new PlugInRoleTypeSpecification(specificationId);

                    ((PlugInRoleTypeSpecification)plugInRoleType).PlugInRoleType = DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ");
                    ((PlugInRoleTypeSpecification)plugInRoleType).BiDirectionalAllowIndicator = DataReaderHelper.GetNonNullValue(reader, "bi_drctnl_allow_ind");
                    ((PlugInRoleTypeSpecification)plugInRoleType).ExternalHeight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_hgt_no"));
                    ((PlugInRoleTypeSpecification)plugInRoleType).ExternalWidth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_wdth_no", true));
                    ((PlugInRoleTypeSpecification)plugInRoleType).ExternalDepth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "xtnl_dpth_no", true));
                    ((PlugInRoleTypeSpecification)plugInRoleType).ExternalDimensionsUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "dim_uom_cd");
                    ((PlugInRoleTypeSpecification)plugInRoleType).TypeDescription = DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_dsc");
                    ((PlugInRoleTypeSpecification)plugInRoleType).TypeName = DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_nm", true);
                    ((PlugInRoleTypeSpecification)plugInRoleType).ConnectorHeight = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "cntnr_hgt_no", true));
                    ((PlugInRoleTypeSpecification)plugInRoleType).ConnectorWidth = decimal.Parse(DataReaderHelper.GetNonNullValue(reader, "cntnr_wdth_no", true));
                    ((PlugInRoleTypeSpecification)plugInRoleType).ConnectorDimensionsUnitOfMeasure = DataReaderHelper.GetNonNullValue(reader, "cd_dim_uom_cd");

                    if (roleTypes == null)
                        roleTypes = new Dictionary<long, PlugInRoleTypeSpecification>();

                    roleTypes.Add(plugInRoleType.SpecificationId, plugInRoleType);
                }
            }
            catch (OracleException oe)
            {
                logger.Error(oe, "Unable to retrieve Plug-In Role Types");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Plug-In Role Types");
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

            return roleTypes;
        }

        public class RoleTypeList
        {
            public string roleId { get; set; }
            public string rolename { get; set; }
        }

        public async Task<List<RoleTypeList>> GetPlugInRoleTypesList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            RoleTypeList roleType;
            List<RoleTypeList> roleTypes = null;
            // var roleTypes = new List<string>();
            string roleId = string.Empty;
            string rolename = string.Empty;
            bool hadException = false;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("plg_in_specn_pkg.get_all_plg_in_role_typ", parameters);

                    while (reader.Read())
                    {
                        roleType = new RoleTypeList();
                        roleType.roleId = DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ_ID");
                        roleType.rolename = DataReaderHelper.GetNonNullValue(reader, "plg_in_role_typ");

                        if (roleTypes == null)
                            roleTypes = new List<RoleTypeList>();

                        roleTypes.Add(roleType);
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to retrieve Plug-In Role Types");
                    hadException = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to retrieve Plug-In Role Types");
                    hadException = true;
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

            return roleTypes;
        }
        public void UpdatePlugInSpecification(long specificationId, string name, decimal maxLightDistance, int maxDistanceUom, string biDirectionalIndicator, string variableWaveLengthIndicator, string formCode, string functionDescription,
            decimal lowTemp, decimal hiTemp, int transmissionId, int connectorId, int pluginRoleTypeId, int channelNumber, string multipleWaveLengthIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(15);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMxDistNo", DbType.Decimal, maxLightDistance, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pMxDistUom", DbType.Int32, maxDistanceUom, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pBiDrctnlInd", DbType.String, biDirectionalIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pVarWvlgthInd", DbType.String, variableWaveLengthIndicator, ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pFormCd", DbType.String, CheckNullValue(formCode), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pFnctnDsc", DbType.String, CheckNullValue(functionDescription), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pLoTmp", DbType.Decimal, lowTemp, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pHiTmp", DbType.Decimal, hiTemp, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pXmsnId", DbType.Int64, CheckNullValue(transmissionId), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pCnctrId", DbType.Int32, connectorId, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pPlgInRlTypId", DbType.Int32, pluginRoleTypeId, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pChnlNo", DbType.Int32, channelNumber, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("pMultFxInd", DbType.String, multipleWaveLengthIndicator, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.update_plg_in_specn", parameters);

                //PROCEDURE update_plg_in_specn(pId IN NUMBER, pNm IN VARCHAR2, pMxDistNo IN NUMBER, pMxDistUom IN NUMBER,
                //pBiDrctnlInd IN VARCHAR2, pVarWvlgthInd IN VARCHAR2, pFormCd IN VARCHAR2, pFnctnDsc IN VARCHAR2,
                //pLoTmp IN NUMBER, pHiTmp IN NUMBER, pXmsnId IN NUMBER, pCnctrId IN NUMBER, pPlgInRlTypId IN NUMBER,
                //pChnlNo IN NUMBER, pMultFxInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update plg_in_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14})", specificationId, name, maxLightDistance, maxDistanceUom, biDirectionalIndicator, variableWaveLengthIndicator,
                    formCode, functionDescription, lowTemp, hiTemp, transmissionId, connectorId, pluginRoleTypeId, channelNumber, multipleWaveLengthIndicator);

                throw ex;
            }
        }

        public void UpdatePlugInWaveLength(long specificationId, long waveLengthId, string transmitReceiveIndicator)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pWvlgthId", DbType.Int64, waveLengthId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pXmtRecvInd", DbType.String, transmitReceiveIndicator, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.upsert_plg_in_specn_wvlgth", parameters);

                //PROCEDURE upsert_plg_in_specn_wvlgth(pId IN NUMBER, pWvlgthId IN NUMBER, pXmtRecvInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update plg_in_specn_wvlgth ({0}, {1}, {2})", specificationId, waveLengthId, transmitReceiveIndicator);

                throw ex;
            }
        }



        public void UpdatePlugInRecieverWavelength(long specificationId, long RecwaveLengthId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(3);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pRecWvlgthId", DbType.Int64, RecwaveLengthId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pXmtRecvInd", DbType.String, 'R', ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.upsert_plg_in_specn_Recwvlgth", parameters);

                //PROCEDURE upsert_plg_in_specn_wvlgth(pId IN NUMBER, pWvlgthId IN NUMBER, pXmtRecvInd IN VARCHAR2)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update plg_in_specn_wvlgth ({0}, {1}, {2})", specificationId, RecwaveLengthId);

                throw ex;
            }
        }
        public void UpdatePlugInTransmissionRate(long specificationId, long transmissionRateId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pXmsnRtId", DbType.Int64, transmissionRateId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.upsert_plg_in_specn_xmsn_rt", parameters);

                //PROCEDURE upsert_plg_in_specn_xmsn_rt(pId IN NUMBER, pXmsnRtId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update plg_in_specn_xmsn_rt ({0}, {1})", specificationId, transmissionRateId);

                throw ex;
            }
        }

        public void UpdatePlugInTransmissionRate(long specificationId, string[] transmissionRateLst)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(2);

                foreach (var strid in transmissionRateLst)
                {
                    parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pXmsnRtId", DbType.String, strid, ParameterDirection.Input);

                    dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.upsert_plg_in_specn_xmsn_rt", parameters);
                }
                //PROCEDURE upsert_plg_in_specn_xmsn_rt(pId IN NUMBER, pXmsnRtId IN NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update plg_in_specn_xmsn_rt ({0}, {1})", specificationId, transmissionRateLst);

                throw ex;
            }
        }

        public long CreatePlugInSpecification(string name, decimal maxLightDistance, int maxDistanceUom, string biDirectionalIndicator, string variableWaveLengthIndicator, string formCode, string functionDescription,
            decimal lowTemp, decimal hiTemp, int transmissionId, int connectorId, int pluginRoleTypeId, int channelNumber, string multipleWaveLengthIndicator)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(15);


                parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMxDistNo", DbType.Decimal, maxLightDistance, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMxDistUom", DbType.Int32, maxDistanceUom, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pBiDrctnlInd", DbType.String, biDirectionalIndicator, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pVarWvlgthInd", DbType.String, variableWaveLengthIndicator, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pFormCd", DbType.String, CheckNullValue(formCode), ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pFnctnDsc", DbType.String, CheckNullValue(functionDescription), ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pLoTmp", DbType.Decimal, lowTemp, ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pHiTmp", DbType.Decimal, hiTemp, ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pXmsnId", DbType.Int64, CheckNullValue(transmissionId), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pCnctrId", DbType.Int32, connectorId, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pPlgInRlTypId", DbType.Int32, pluginRoleTypeId, ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pChnlNo", DbType.Int32, channelNumber, ParameterDirection.Input);
                parameters[13] = dbAccessor.GetParameter("pMultFxInd", DbType.String, multipleWaveLengthIndicator, ParameterDirection.Input);
                parameters[14] = dbAccessor.GetParameter("oSpecnId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "plg_in_specn_pkg.insert_plg_in_specn", parameters);

                specificationId = long.Parse(parameters[14].Value.ToString());

                //PROCEDURE insert_plg_in_specn(pNm IN VARCHAR2, pMxDistNo IN NUMBER, pMxDistUom IN NUMBER,
                //pBiDrctnlInd IN VARCHAR2, pVarWvlgthInd IN VARCHAR2, pFormCd IN VARCHAR2, pFnctnDsc IN VARCHAR2,
                //pLoTmp IN NUMBER, pHiTmp IN NUMBER, pXmsnId IN NUMBER, pCnctrId IN NUMBER, pPlgInRlTypId IN NUMBER,
                //pChnlNo IN NUMBER, pMultFxInd IN VARCHAR2, oSpecnId OUT NUMBER)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert plg_in_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})", name, maxLightDistance, maxDistanceUom, biDirectionalIndicator, variableWaveLengthIndicator,
                    formCode, functionDescription, lowTemp, hiTemp, transmissionId, connectorId, pluginRoleTypeId, channelNumber, multipleWaveLengthIndicator);

                throw ex;
            }

            return specificationId;
        }

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }
    }
}