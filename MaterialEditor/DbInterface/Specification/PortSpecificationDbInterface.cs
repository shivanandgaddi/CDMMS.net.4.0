using System;
using System.Collections.Generic;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;
// added by rxjohn
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.Models;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class PortSpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PortSpecificationDbInterface() : base()
        {
        }

        public PortSpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
        }

        public override ISpecification GetSpecification(long specificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            ISpecification port = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pPortId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("PORT_SPECN_PKG.get_port_specn", parameters);

                if (reader.Read())
                {
                    port = new PortSpecification(specificationId);

                    port.Name = DataReaderHelper.GetNonNullValue(reader, "port_model_nm");
                    port.Description = DataReaderHelper.GetNonNullValue(reader, "port_dsc");
                    port.IsCompleted = DataReaderHelper.GetNonNullValue(reader, "cmplt_ind") == "Y" ? true : false;
                    port.IsPropagated = DataReaderHelper.GetNonNullValue(reader, "prpgt_ind") == "Y" ? true : false;
                    port.IsDeleted = DataReaderHelper.GetNonNullValue(reader, "del_ind") == "Y" ? true : false;

                    ((PortSpecification)port).IsGeneric = true;
                    ((PortSpecification)port).PortUseTypId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "port_use_typ_id", true));
                    ((PortSpecification)port).SpecNm = DataReaderHelper.GetNonNullValue(reader, "port_nm");
                    ((PortSpecification)port).PortUseTyp = DataReaderHelper.GetNonNullValue(reader, "use_typ");
                    ((PortSpecification)port).PortTyp = DataReaderHelper.GetNonNullValue(reader, "port_typ");
                    ((PortSpecification)port).PortSrvLvl = DataReaderHelper.GetNonNullValue(reader, "port_srvclvl");
                    ((PortSpecification)port).PhysStts = DataReaderHelper.GetNonNullValue(reader, "port_phys_stts");
                    ((PortSpecification)port).PortDept = DataReaderHelper.GetNonNullValue(reader, "port_dept");
                    ((PortSpecification)port).ConnectorTypeId = int.Parse(DataReaderHelper.GetNonNullValue(reader, "port_cnctr_id", true));
                    //SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                    //((PortSpecification)port).SlotUseTypId = dbInterface.GetUseType("Slot");
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

            return port;
        }

        public void UpdatePortSpecification(long specificationId, string ModelName, string SpecName, string description, bool completionInd, bool propagationInd, bool deleteInd, int useType,
                                            string PortTyp, string PortSrvLvl, int PortCnctrId, string PortPhysStts, string PortDept)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);

                parameters[0] = dbAccessor.GetParameter("pPortId", DbType.Int64, specificationId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pPortNm", DbType.String, SpecName.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPortModelNm", DbType.String, ModelName.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPortDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pPortUseTypId", DbType.Int64, useType, ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd ? "Y" : "N", ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pDelInd", DbType.String, deleteInd ? "Y" : "N", ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pPortTyp", DbType.String, PortTyp.ToUpper(), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pPortSrvLvl", DbType.String, PortSrvLvl.ToUpper(), ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pPortCnctrId", DbType.Int64, PortCnctrId, ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pPortPhysStts", DbType.String, PortPhysStts.ToUpper(), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("pPortDept", DbType.String, PortDept.ToUpper(), ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "PORT_SPECN_PKG.update_port_specn", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update update_port_specn ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                    specificationId, SpecName, ModelName, description, useType, completionInd, propagationInd, deleteInd);

                throw ex;
            }
        }

        public void DeletePortSpecificationRole(long specificationId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);

                parameters[0] = dbAccessor.GetParameter("pId", DbType.Int64, specificationId, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "PORT_SPECN_PKG.delete_slot_specn_role", parameters);
                //PROCEDURE delete_slot_specn_role(pId IN NUMBER);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete delete_port_specn_role ({0})", specificationId);
                throw ex;
            }
        }

        public long CreatePortSpecification(string revisionName, string Modelname, string description, int PortUseTypId, bool completionInd, bool propagationInd, bool deleteInd,
                                            string PortTyp, string PortSrvLvl, int PortCnctrId, string PortPhysStts, string PortDept)
        {
            IDbDataParameter[] parameters = null;
            long specificationId = 0;

            try
            {
                parameters = dbAccessor.GetParameterArray(13);

                parameters[0] = dbAccessor.GetParameter("pPortNm", DbType.String, revisionName.ToUpper(), ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pPortModelNm", DbType.String, Modelname.ToUpper(), ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pPortDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pPortUseTypId", DbType.Int64, PortUseTypId, ParameterDirection.Input);
                parameters[4] = dbAccessor.GetParameter("pCmpltInd", DbType.String, completionInd ? "Y" : "N", ParameterDirection.Input);
                parameters[5] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, propagationInd ? "Y" : "N", ParameterDirection.Input);
                parameters[6] = dbAccessor.GetParameter("pDelInd", DbType.String, deleteInd ? "Y" : "N", ParameterDirection.Input);
                parameters[7] = dbAccessor.GetParameter("pPortTyp", DbType.String, PortTyp.ToUpper(), ParameterDirection.Input);
                parameters[8] = dbAccessor.GetParameter("pPortSrvLvl", DbType.String, PortSrvLvl.ToUpper(), ParameterDirection.Input);
                parameters[9] = dbAccessor.GetParameter("pPortCnctrId", DbType.Int64, PortCnctrId, ParameterDirection.Input);
                parameters[10] = dbAccessor.GetParameter("pPortPhysStts", DbType.String, PortPhysStts.ToUpper(), ParameterDirection.Input);
                parameters[11] = dbAccessor.GetParameter("pPortDept", DbType.String, PortDept.ToUpper(), ParameterDirection.Input);
                parameters[12] = dbAccessor.GetParameter("oPortId", DbType.Int64, specificationId, ParameterDirection.Output);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "PORT_SPECN_PKG.insert_port_specn", parameters);

                specificationId = long.Parse(parameters[12].Value.ToString());

                //PROCEDURE insert_slot(pNm IN VARCHAR2, pDsc IN VARCHAR2, pSltCnsmptnId IN NUMBER, pSbSltInd IN VARCHAR2,
                //pDpth IN NUMBER, pHght IN NUMBER, pWdth IN NUMBER, pUom IN NUMBER, pCmplt IN VARCHAR2, pPrpgt IN VARCHAR2,
                //pDel IN VARCHAR2, pStrghtThruInd IN VARCHAR2, pRvsnNm IN VARCHAR2, oSpecnId OUT NUMBER);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create insert_slot ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} )",
                    revisionName, Modelname, description, PortUseTypId, completionInd, propagationInd, deleteInd);

                throw ex;
            }

            return specificationId;
        }

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }


        public async Task<List<Option>> GetSpecUseTypes(int SpecificationId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pSpecTypId", DbType.Int32, SpecificationId, ParameterDirection.Input); // This is the port spec type id in specn_record_use_typ table
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("PORT_SPECN_PKG.get_port_useTyp", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionValue = reader["ID"].ToString().ToUpper();
                        string optionText = reader["TYP"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get PortTypes for Port Sequence screen.");
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

            return options;
        }


    }
}