using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public class SpecificationDbInterface : SpecificationDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SpecificationDbInterface() : base()
        {

        }

        public SpecificationDbInterface(string dbConnectionString) : base(dbConnectionString)
        {

        }

        public override ISpecification GetSpecification(long specificationId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Dictionary<string, SpecificationAttribute>>> SearchSpecByIdAsync(string id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, SpecificationAttribute>> specificationList = null;
            bool hadException = false;
            SpecificationType specType = new SpecificationType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pId", DbType.String, id.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("specn_pkg.search_spec_exact", parameters);

                    while (reader.Read())
                    {
                        if (specificationList == null)
                            specificationList = new List<Dictionary<string, SpecificationAttribute>>();

                        Dictionary<string, SpecificationAttribute> spec = new Dictionary<string, SpecificationAttribute>();

                        spec.Add("specn_id", new SpecificationAttribute("specn_id", reader["specn_id"].ToString()));
                        spec.Add("specn_nm", new SpecificationAttribute("specn_nm", DataReaderHelper.GetNonNullValue(reader, "specn_nm")));
                        spec.Add("specn_model_nm", new SpecificationAttribute("specn_model_nm", DataReaderHelper.GetNonNullValue(reader, "specn_model_nm")));
                        spec.Add("specn_dsc", new SpecificationAttribute("specn_dsc", DataReaderHelper.GetNonNullValue(reader, "specn_dsc")));
                        spec.Add("cmplt_ind", new SpecificationAttribute("cmplt_ind", reader["cmplt_ind"].ToString()));
                        spec.Add("prpgt_ind", new SpecificationAttribute("prpgt_ind", reader["prpgt_ind"].ToString()));
                        spec.Add("del_ind", new SpecificationAttribute("del_ind", reader["del_ind"].ToString()));
                        spec.Add("specTyp", new SpecificationAttribute("specTyp", DataReaderHelper.GetNonNullValue(reader, "specTyp")));
                        spec.Add("product_id", new SpecificationAttribute("product_id", DataReaderHelper.GetNonNullValue(reader, "product_id")));
                        spec.Add("specClss", new SpecificationAttribute("specClss", DataReaderHelper.GetNonNullValue(reader, "specClss")));
                        spec.Add("enumSpecTyp", new SpecificationAttribute("enumSpecTyp", DataReaderHelper.GetNonNullValue(reader, "enumSpecTyp")));

                        specificationList.Add(spec);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}";

                    hadException = true;

                    logger.Error(oe, message, id);
                    EventLogger.LogAlarm(oe, string.Format(message, id), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search");
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

            return specificationList;
        }
        public async Task<List<Dictionary<string, SpecificationAttribute>>> SearchSpecificationsAsync(string specificationType, string specificationClass, string id, string name, string description, string status, string modelName, string materialCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, SpecificationAttribute>> specificationList = null;
            bool hadException = false;
            SpecificationType specType = new SpecificationType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(9);

                    parameters[0] = dbManager.GetParameter("pTyp", DbType.String, specificationType, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pClss", DbType.String, specificationClass, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pId", DbType.String, id.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pStts", DbType.String, status, ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("pmodelNm", DbType.String, modelName, ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pMtlCd", DbType.String, materialCode.ToUpper(), ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("specn_pkg.search_specifications", parameters);

                    while (reader.Read())
                    {
                        if (specificationList == null)
                            specificationList = new List<Dictionary<string, SpecificationAttribute>>();

                        Dictionary<string, SpecificationAttribute> spec = new Dictionary<string, SpecificationAttribute>();

                        spec.Add("specn_id", new SpecificationAttribute("specn_id", reader["specn_id"].ToString()));
                        spec.Add("specn_nm", new SpecificationAttribute("specn_nm", DataReaderHelper.GetNonNullValue(reader, "specn_nm")));
                        spec.Add("specn_model_nm", new SpecificationAttribute("specn_model_nm", DataReaderHelper.GetNonNullValue(reader, "specn_model_nm")));
                        spec.Add("specn_dsc", new SpecificationAttribute("specn_dsc", DataReaderHelper.GetNonNullValue(reader, "specn_dsc")));
                        spec.Add("cmplt_ind", new SpecificationAttribute("cmplt_ind", reader["cmplt_ind"].ToString()));
                        spec.Add("prpgt_ind", new SpecificationAttribute("prpgt_ind", reader["prpgt_ind"].ToString()));
                        spec.Add("del_ind", new SpecificationAttribute("del_ind", reader["del_ind"].ToString()));
                        spec.Add("specTyp", new SpecificationAttribute("specTyp", DataReaderHelper.GetNonNullValue(reader, "specTyp")));
                        spec.Add("product_id", new SpecificationAttribute("product_id", DataReaderHelper.GetNonNullValue(reader, "product_id")));
                        spec.Add("specClss", new SpecificationAttribute("specClss", DataReaderHelper.GetNonNullValue(reader, "specClss")));
                        spec.Add("enumSpecTyp", new SpecificationAttribute("enumSpecTyp", DataReaderHelper.GetNonNullValue(reader, "enumSpecTyp")));

                        specificationList.Add(spec);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}";

                    hadException = true;

                    logger.Error(oe, message, specificationType, specificationClass, id, name, description, status);
                    EventLogger.LogAlarm(oe, string.Format(message, specificationType, specificationClass, id, name, description, status), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search");
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

            return specificationList;
        }

        public async Task<List<Dictionary<string, SpecificationAttribute>>> SearchPartsToAssociateAsync(string specificationType, string materialCode, string id, string clmc, string partNumber, string description, string recordOnly, string roletype, string cleiValue)
        {
            IAccessor dbManager = null;
            IDataReader reader = null;
            List<Dictionary<string, SpecificationAttribute>> specificationList = null;
            IDbDataParameter[] parameters = null;
            bool hadException = false;
            SpecificationType specType = new SpecificationType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(10);

                    parameters[0] = dbManager.GetParameter("pTyp", DbType.String, specificationType.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pMtlCd", DbType.String, materialCode.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pId", DbType.String, id, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pPrtNo", DbType.String, partNumber.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pDsc", DbType.String, description.ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pClmc", DbType.String, clmc.ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("pRo", DbType.String, recordOnly.ToUpper(), ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pRoleType", DbType.String, roletype, ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameter("pCleiId", DbType.String, cleiValue, ParameterDirection.Input);
                    parameters[9] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("specn_pkg.search_material", parameters);

                    //PROCEDURE search_material(pTyp IN VARCHAR2, pMtlCd IN VARCHAR2, pId IN VARCHAR2, pPrtNo IN VARCHAR2, 
                    //pDsc IN VARCHAR2, pClmc IN VARCHAR2, pRo IN VARCHAR2, retcsr OUT ref_cursor)

                    while (reader.Read())
                    {
                        if (specificationList == null)
                            specificationList = new List<Dictionary<string, SpecificationAttribute>>();

                        Dictionary<string, SpecificationAttribute> spec = new Dictionary<string, SpecificationAttribute>();

                        spec.Add("product_id", new SpecificationAttribute("product_id", DataReaderHelper.GetNonNullValue(reader, "mtrl_cd")));
                        spec.Add("mfg_part_no", new SpecificationAttribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "part_no")));
                        spec.Add("mfg_id", new SpecificationAttribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd")));
                        spec.Add("material_item_id", new SpecificationAttribute("material_item_id", DataReaderHelper.GetNonNullValue(reader, "id")));
                        spec.Add("item_desc", new SpecificationAttribute("item_desc", DataReaderHelper.GetNonNullValue(reader, "item_desc")));

                        specificationList.Add(spec);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}";

                    hadException = true;

                    logger.Error(oe, message, specificationType, materialCode, id, clmc, partNumber, description);
                    EventLogger.LogAlarm(oe, string.Format(specificationType, materialCode, id, clmc, partNumber, description), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search");
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

            return specificationList;
        }

        public async Task<List<Dictionary<string, SpecificationAttribute>>> SearchAssociatedPartsAsync(string tableName, string columnName, string RvsId, 
                                                              string altColumnName, string recordOnly, string specID, string specificationType)
        {
            IAccessor dbManager = null;
            string sql = "";
            IDataReader reader = null;
            List<Dictionary<string, SpecificationAttribute>> specificationList = null;
            bool hadException = false;
            SpecificationType specType = new SpecificationType();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    if (specificationType != "PLUG_IN")
                    {
                        sql = "select rvsn." + columnName + " as REASN_NO, rvsn." + altColumnName + ", rvsn.mtrl_cd, m.rt_part_no, mf.mfr_cd, m.mtrl_dsc " +
                                    " from " + tableName + " rvsn, mtrl m, mfr mf " +
                                    " where rvsn.mtrl_id = m.mtrl_id " +
                                    " and m.mfr_id = mf.mfr_id " +
                                    " and rvsn." + altColumnName + " = " + RvsId +
                                    " and m.rcrds_only_ind = '" + recordOnly + "'";
                    }
                    else
                    {
                        sql = "select rm.MTRL_ID, rvsn." + altColumnName + " as REASN_NO, rvsn." + altColumnName + ", rvsn.mtrl_cd, m.rt_part_no, mf.mfr_cd, m.mtrl_dsc " +
                                   " from " + tableName + " rvsn, mtrl m, mfr mf,RME_PLG_IN_MTRL rm " +
                                   " where rvsn.mtrl_id = m.mtrl_id " +
                                   " and rm.PLG_IN_SPECN_ID = " + specID +  
                                   " and m.mfr_id = mf.mfr_id and rm.mtrl_id = rvsn.mtrl_id " +
                                   // " and rvsn." + altColumnName + " = " + RvsId +
                                   " and m.rcrds_only_ind = '" + recordOnly + "'";
                    }


                    reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                    while (reader.Read())
                    {
                        if (specificationList == null)
                            specificationList = new List<Dictionary<string, SpecificationAttribute>>();

                        Dictionary<string, SpecificationAttribute> spec = new Dictionary<string, SpecificationAttribute>();

                        spec.Add("product_id", new SpecificationAttribute("product_id", reader["mtrl_cd"].ToString()));
                        spec.Add("mfg_part_no", new SpecificationAttribute("mfg_part_no", DataReaderHelper.GetNonNullValue(reader, "rt_part_no")));
                        spec.Add("mfg_id", new SpecificationAttribute("mfg_id", DataReaderHelper.GetNonNullValue(reader, "mfr_cd")));
                        spec.Add("material_item_id", new SpecificationAttribute("material_item_id", reader["REASN_NO"].ToString()));
                        spec.Add("item_desc", new SpecificationAttribute("item_desc", reader["mtrl_dsc"].ToString()));
                        spec.Add("alt_spec_id", new SpecificationAttribute("alt_spec_id", reader[altColumnName].ToString()));

                        specificationList.Add(spec);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}";

                    hadException = true;

                    logger.Error(oe, message, tableName, RvsId, columnName);
                    EventLogger.LogAlarm(oe, string.Format(tableName, RvsId, columnName), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform search");
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

            return specificationList;
        }

        public async Task AssociatePartToSpecPartAsync(string tableName, string specIDColumnName,
            string materialRevIDColumnName, string specID, string materialID, string RevID,string specificationType)
        {
            IAccessor dbManager = null;
            string sql = "";
            bool hadException = false;
            SpecificationType specType = new SpecificationType();
            IDataReader reader = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    if (specificationType != "PLUG_IN")
                    {
                        sql = "update " + tableName + " set " + specIDColumnName + " = " + specID + " where " + materialRevIDColumnName + " = " + materialID;
                    }
                    else
                    {
                        /// Initially update the current value as NULL
                        sql = "select rvsn.mtrl_id " +
                                  " from " + tableName + " rvsn " +
                                  " where rvsn." + materialRevIDColumnName + " = '" + RevID + "'";

                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                        string update_materialID = "";

                        while (reader.Read())
                        {
                            update_materialID = DataReaderHelper.GetNonNullValue(reader, "mtrl_id");
                        }

                        sql = "update RME_PLG_IN_MTRL  set PLG_IN_SPECN_ID = Null where MTRL_ID = " + update_materialID;
                      
                        int retrnCode = dbManager.ExecuteNonQuery(CommandType.Text, sql);
                     

                        /// Get the material id of the current slected material and update in to RME_PLG_IN_MTRL table.
                        sql = "select rvsn.mtrl_id " +
                                  " from " + tableName + " rvsn " +
                                  " where rvsn." + materialRevIDColumnName + " = '" + materialID + "'";

                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                        while (reader.Read())
                        {
                            materialID = DataReaderHelper.GetNonNullValue(reader, "mtrl_id");
                        }

                        sql = "update RME_PLG_IN_MTRL  set PLG_IN_SPECN_ID = " + specID + " where MTRL_ID = " + materialID;
                    }
                    int returnCode = dbManager.ExecuteNonQuery(CommandType.Text, sql);

                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}, {4}";

                    hadException = true;

                    logger.Error(oe, message, tableName, specIDColumnName, materialRevIDColumnName, specID, materialID);
                    EventLogger.LogAlarm(oe, string.Format(tableName, specIDColumnName, materialRevIDColumnName, specID, materialID), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform update");
                }
                finally
                {

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (hadException)
                throw new Exception();
        }

        public async Task DisassociatePartToSpecPartAsync(string tableName, string specIDColumnName,
           string materialRevIDColumnName, string materialID,string specificationType)
        {
            IAccessor dbManager = null;
            string sql = "";
            bool hadException = false;
            SpecificationType specType = new SpecificationType();
            IDataReader reader = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                    if (specificationType != "PLUG_IN")
                    {
                        sql = "update " + tableName + " set " + specIDColumnName + " = NULL where " + materialRevIDColumnName + " = " + materialID;
                    }
                    else
                    {
                        sql = "select rvsn.mtrl_id " +
                                  " from RME_PLG_IN_MTRL_REVSN rvsn " +
                                  " where rvsn.MTRL_CD = '" + materialID + "'";

                        reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                        while (reader.Read())
                        {
                            materialID = DataReaderHelper.GetNonNullValue(reader, "mtrl_id");
                        }

                        sql = "update RME_PLG_IN_MTRL  set PLG_IN_SPECN_ID = NULL where MTRL_ID = " + materialID;
                    }
                    int returnCode = dbManager.ExecuteNonQuery(CommandType.Text, sql);

                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search: {0}, {1}, {2}, {3}";

                    hadException = true;

                    logger.Error(oe, message, tableName, specIDColumnName, materialRevIDColumnName, materialID);
                    EventLogger.LogAlarm(oe, string.Format(tableName, specIDColumnName, materialRevIDColumnName, materialID), SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    hadException = true;

                    logger.Error(ex, "Unable to perform update");
                }
                finally
                {

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if (hadException)
                throw new Exception();
        }

        public override void AssociateMaterial(JObject jObject)
        {
            throw new NotImplementedException();
        }
        public string GetGenericUseType(string sql)
        {
            string useTyp = "";
            IAccessor dbManager = null;
            IDataReader reader = null;


            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                if (reader.Read())
                {
                    useTyp = DataReaderHelper.GetNonNullValue(reader, "alias_val");

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetGenericUseType");
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

            return useTyp;
        }

        public string GetUseType(string specTyp, int optRoleTypId = 0)
        {
            string sql = "";
            string useTyp = "";
            string gnrcSql = @"select val.alias_val from specn_typ_alias_val val,specn_typ_alias ali,specn_typ typ where ali.specn_typ_alias_id=val.specn_typ_alias_id
and typ.specn_typ_id = val.specn_typ_id and typ.specn_typ='" + specTyp + "' and ali.alias_nm='RME Record Use Type'";
            IAccessor dbManager = null;
            IDataReader reader = null;
            if (specTyp == "Bay")
            {
                sql = @"select val.alias_val from bay_role_typ_alias_val val,bay_role_typ_alias ali 
                        where val.bay_role_typ_alias_id = ali.bay_role_typ_alias_id 
                        and ali.alias_nm='RME Record Use Type' 
                        and val.bay_role_typ_id =" + optRoleTypId;
            }

            else if (specTyp == "Node")
            {
                sql = @"select val.alias_val from node_role_typ_alias_val val,node_role_typ_alias ali 
                        where val.node_role_typ_alias_id = ali.node_role_typ_alias_id 
                        and ali.alias_nm='RME Record Use Type' 
                        and val.node_role_typ_id =" + optRoleTypId;
            }
            else if (specTyp == "Shelf")
            {
                sql = @"select val.alias_val from shelf_role_typ_alias_val val, shelf_role_typ_alias ali
                        where val.shelf_role_typ_alias_id = ali.shelf_role_typ_alias_id
                        and ali.alias_nm = 'RME Record Use Type' 
                        and val.shelf_role_typ_id =" + optRoleTypId;
            }
            else
            { sql = gnrcSql; }
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                if (reader.Read())
                {
                    useTyp = DataReaderHelper.GetNonNullValue(reader, "alias_val");

                }
                if (useTyp == "")
                {
                    useTyp = GetGenericUseType(gnrcSql);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetGenericUseType");
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

            return useTyp;
        }

        public async Task<string> GetSpecNameDuplicate(string specName, int id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string duplicateSpecNameInfo = "NODUP";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pId", DbType.Int32, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pSpecName", DbType.String, specName, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("specn_pkg.get_spec_name_duplicate", parameters);

                    while (reader.Read())
                    {
                        string specID = reader["id"].ToString();
                        specName = reader["specn_nm"].ToString();
                        string specType = reader["spec_type"].ToString();
                        duplicateSpecNameInfo = specID + "~" + specName + "~" + specType;
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to GetSpecNameDuplicate");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to GetSpecNameDuplicate");
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

            return duplicateSpecNameInfo;
        }

        public async Task<string> GetWeightConversion(string uomCode)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string conversion = "1";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);
                    
                    parameters[0] = dbManager.GetParameter("pUomCode", DbType.String, uomCode, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("specn_pkg.get_weight_conversion", parameters);

                    while (reader.Read())
                    {
                        conversion = reader["multiplier"].ToString();
                    }
                }
                catch (OracleException oe)
                {
                    logger.Error(oe, "Unable to GetWeightConversion");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to GetWeightConversion");
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

            return conversion;
        }

        public void UpdateAssociatedParts(long oldCDMMSId, long newCDMMSId, long specnID, string cuid, string specName)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(5);

                parameters[0] = dbManager.GetParameter("pOldCDMMSId", DbType.Int64, oldCDMMSId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pNewCDMMSId", DbType.Int64, newCDMMSId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pSpecnId", DbType.Int64, specnID, ParameterDirection.Input);
                parameters[3] = dbManager.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);
                parameters[4] = dbManager.GetParameter("pNewSpecNm", DbType.String, specName, ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("SPECN_PKG.UPDATE_ASSOCIATED_PART", parameters);
            }
            catch (OracleException oex)
            {
                string msg = oex.Message;

                throw oex;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update UpdateAssociatedParts ({0}, {1}, {2})", oldCDMMSId, newCDMMSId, specnID);

                throw ex;
            }
        }

        public void UnassociatePart(long oldCDMMSId, long specnID, string cuid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pOldCDMMSId", DbType.Int64, oldCDMMSId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pSpecnId", DbType.Int64, specnID, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("SPECN_PKG.UNASSOCIATE_PART", parameters);
            }
            catch (OracleException oex)
            {
                string msg = oex.Message;

                throw oex;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to execute UnassociatePart ({0}, {1})", oldCDMMSId, specnID);

                throw ex;
            }
        }

        public StringCollection GetSpecDimensions(long mtrlId, int featureTypeId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            StringCollection dimensions = new StringCollection();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pMtlId", DbType.Int32, mtrlId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pFtrTypId", DbType.Int32, featureTypeId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("specn_pkg.get_dimensions", parameters);

                if (reader.Read())
                {
                    dimensions.Add(reader["dpth"].ToString());
                    dimensions.Add(reader["wdth"].ToString());
                    dimensions.Add(reader["hgt"].ToString());
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to retrieve dimensions for material id: {0}";

                logger.Error(oe, message, mtrlId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get spec dimensions from material id: {0}", mtrlId);
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

            return dimensions;
        }

        public string GetDeletedIndicator(long specId, int featTypeId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string deletedIndicator = String.Empty;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pSpecId", DbType.Int32, specId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pFtrTypId", DbType.Int32, featTypeId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("specn_pkg.get_deleted_indicator", parameters);

                if (reader.Read())
                {
                    deletedIndicator = reader["del_ind"].ToString();
                }
            }
            catch (OracleException oe)
            {
                string message = "Unable to get deleted indicator for material id: {0}";

                logger.Error(oe, message, specId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get deleted indicator from spec id: {0}", specId);
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

            return deletedIndicator;
        }

        public void insertSpecAttributeName(long materialItemId, string specName)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pMtlItemId", DbType.Int32, materialItemId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pSpecNm", DbType.String, specName, ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("specn_pkg.insert_name_attribute", parameters);
            }
            catch (OracleException oex)
            {
                string msg = oex.Message;

                throw oex;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to insert or update insertSpecAttributeName ({0}, {1})", materialItemId, specName);

                throw ex;
            }
        }

    }
}