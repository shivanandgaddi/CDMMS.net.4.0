using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface
{
    public class AssemblyUnitDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private IAccessor dbAccessor = null;

        public AssemblyUnitDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }
        public AssemblyUnitDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public void StartTransaction()
        {
            if (dbAccessor == null)
                dbAccessor = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

            dbAccessor.BeginTransaction();
        }
        public void CommitTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            if (dbAccessor != null)
                dbAccessor.RollbackTransaction();
        }
        public void Dispose()
        {
            if (dbAccessor != null)
                dbAccessor.Dispose();
        }
        public async Task<List<Option>> GetlbrIdsList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_all_labor_ids", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }
                        string optionValue = reader["lbr_id"].ToString().ToUpper();
                        string optionText = reader["title"].ToString().ToUpper();
                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Labor Id drop down list for Assembly unit page");
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
        public async Task<AssemblyUnit> GetlbrIdAttributes(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            AssemblyUnit LaborIddesc = new AssemblyUnit();

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_labor_id", parameters);

                    if (reader.Read())
                    {
                        LaborIddesc.LaborTitle = reader["lbr_title_nm"].ToString().ToUpper();
                        LaborIddesc.LaborDesc = reader["lbr_dsc"].ToString().ToUpper();
                    }
                    else { LaborIddesc = null; }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Labor Id title and desc for id : {0}", id);
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

            return LaborIddesc;
        }
        public async Task<List<Dictionary<string, Models.Attribute>>> GetlbrIdMtrlClassification(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, Models.Attribute>> LaborIdMclst = null;
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            Dictionary<string, Models.Attribute> attr = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_mtrl_clsfctn_for_lbr_id", parameters);

                    while (reader.Read())
                    {
                        if (LaborIdMclst == null)
                            LaborIdMclst = new List<Dictionary<string, Models.Attribute>>();

                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.LaborIdMtrlclsfctn)
                        {
                            attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_MTRL_CLSFCTN_FOR_LBR_ID));
                        }

                        LaborIdMclst.Add(attr);

                        attr = null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Material classification table list for labor id {0}", id);
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

            return LaborIdMclst;
        }
        public async Task<List<Dictionary<string, Models.Attribute>>> GetlbrIdAssemblyUnits(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, Models.Attribute>> LaborIdAsmblyUnitlst = null;
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            Dictionary<string, Models.Attribute> attr = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_assembly_units_for_lbr_id", parameters);

                    while (reader.Read())
                    {
                        if (LaborIdAsmblyUnitlst == null)
                            LaborIdAsmblyUnitlst = new List<Dictionary<string, Models.Attribute>>();

                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.LaborIdAUnit)
                        {
                            if (keyValue.Key == "isselected")
                            {
                                attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, "Y", AssemblyUnitType.GET_ASSEMBLY_UNITS_FOR_LBR_ID));

                            }
                            else
                            {
                                attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_ASSEMBLY_UNITS_FOR_LBR_ID));
                            }
                        }
                        LaborIdAsmblyUnitlst.Add(attr);
                        attr = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get associated Assembly unit for labor id {0}", id);
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

            return LaborIdAsmblyUnitlst;
        }
        public async Task<List<Option>> GetCalcList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_all_au_calc", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionValue = reader["au_calc_id"].ToString().ToUpper();
                        string optionText = reader["calc_nm"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Calculation drop down list for Assembly unit page");
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
        public async Task<List<Option>> GetUomList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);
                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_uom", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }

                        string optionValue = reader["au_uom_id"].ToString().ToUpper();
                        string optionText = reader["uom_nm"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get UOM drop down list for Assembly unit page Search fireworks popup.");
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
        public async Task<List<Option>> GetOperatorList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_operators", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();
                        }

                        string optionValue = reader["au_op_opr_id"].ToString().ToUpper();
                        string optionText = reader["opr_nm"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get operator list for Assembly unit page");
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
        public async Task<List<Option>> GetOperatorTermList()
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_operator_terms", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();
                        }

                        string optionValue = reader["au_op_term_id"].ToString().ToUpper();
                        string optionText = reader["term_nm"].ToString().ToUpper();

                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get operator term list for Assembly unit page");
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
        public async Task<List<Dictionary<string, Models.Attribute>>> GetAssemblyUnits(long id, string auNm, long calcID, string retInd)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, Models.Attribute>> AsmblyUnitlst = null;
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            Dictionary<string, Models.Attribute> attr = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(5);

                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pAuNm", DbType.String, auNm.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pCalcId", DbType.Int64, calcID, ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pRetInd", DbType.String, retInd.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.search_assembly_units", parameters);

                    while (reader.Read())
                    {
                        if (AsmblyUnitlst == null)
                            AsmblyUnitlst = new List<Dictionary<string, Models.Attribute>>();

                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.AUnits)
                        {
                            if (keyValue.Key == "default")
                            { attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, "N", AssemblyUnitType.SEARCH_ASSEMBLY_UNITS)); }
                            else if (keyValue.Key == "alttoauid")
                            { attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, "0", AssemblyUnitType.SEARCH_ASSEMBLY_UNITS)); }
                            else if (keyValue.Key == "lbridauid")
                            { attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, "0", AssemblyUnitType.SEARCH_ASSEMBLY_UNITS)); }
                            
                            else
                            {
                                attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.SEARCH_ASSEMBLY_UNITS));

                            }
                        }

                        AsmblyUnitlst.Add(attr);

                        attr = null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get assembly unit for search input {0},{1},{2},{3}", id, auNm, calcID, retInd);
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

            return AsmblyUnitlst;
        }
        public async Task<List<Dictionary<string, Models.Attribute>>> GetAssemblyUnitsforLbrIdPU(long lbrid)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, Models.Attribute>> LbrIdAsmblyUnitlst = null;
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            Dictionary<string, Models.Attribute> attr = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_au_for_lbr_id_popup", parameters);

                    while (reader.Read())
                    {
                        if (LbrIdAsmblyUnitlst == null)
                            LbrIdAsmblyUnitlst = new List<Dictionary<string, Models.Attribute>>();

                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.AUnitforLaborIDPU)
                        {
                            attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_AU_FOR_LBR_ID_POPUP));
                        }

                        LbrIdAsmblyUnitlst.Add(attr);
                        attr = null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Assembly unit for labor id - {0} in pop up", lbrid);
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

            return LbrIdAsmblyUnitlst;
        }
        public async Task<List<Dictionary<string, Models.Attribute>>> GetCalculationOperations(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            AssemblyUnitType auType = new AssemblyUnitType();
            List<Dictionary<string, Models.Attribute>> operations = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pAuCalcId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_operations_for_calculation", parameters);

                    while (reader.Read())
                    {
                        if (operations == null)
                            operations = new List<Dictionary<string, Models.Attribute>>();

                        Dictionary<string, Models.Attribute> operation = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in auType.CalculationOperations)
                        {
                            if (keyValue.Key == "ordrOfOprtn")
                            {
                                Models.Attribute attr = new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_OPERATIONS_FOR_CALCULATION);

                                attr.Options = GetOrderOfOpertionsOptionsList();

                                operation.Add(keyValue.Key, attr);
                            }
                            else
                                operation.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_OPERATIONS_FOR_CALCULATION));
                        }

                        operations.Add(operation);

                        operation = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get operations for calculation id {0}", id);
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

            return operations;
        }

        public async Task<bool> GetCalculationCanBeDeleted(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            AssemblyUnitType auType = new AssemblyUnitType();
            bool returnValue = false;
            string oDeleted = String.Empty;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pAuCalcId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("oDeleted", DbType.String, oDeleted, ParameterDirection.Output, 4000);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_calculation_can_be_deleted", parameters);

                    if (parameters[1].Value.ToString() == "FALSE")
                    {
                        returnValue = false;
                    }
                    else returnValue = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get operations for calculation id {0}", id);
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

            return returnValue;
        }

        public async Task<string> DeleteCalculation(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            AssemblyUnitType auType = new AssemblyUnitType();
            string returnValue = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(1);

                    parameters[0] = dbManager.GetParameter("pAuCalcId", DbType.Int64, id, ParameterDirection.Input);

                    dbManager.ExecuteNonQuerySP("assembly_unit_pkg.delete_calculation", parameters);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get operations for calculation id {0}", id);
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
            return returnValue;
        }

        private List<Option> GetOrderOfOpertionsOptionsList()
        {
            List<Option> options = new List<Option>();

            for (int i = 0; i < 10; i++)
            {
                options.Add(new Option(i.ToString(), i.ToString()));
            }

            return options;
        }
        public async Task<string> InsertlbrIdAttributes(long lbrid,string title, string desc)
        {
            string status = "";
            IDbDataParameter[] parameters = null;
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(3);
                    parameters[0] = dbAccessor.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pTitle", DbType.String, title.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, desc.ToUpper(), ParameterDirection.Input);
                    dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_lbr_id", parameters);
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(CDMMS_OWNER.LBR_PK) violated")){
                        status = "Labor Id already exists please enter an unique id";
                    }
                    RollbackTransaction();                   
                    logger.Error(ex, "Unable to insert labor id ({0}, {1}", title, desc);
                }
                finally
                {
                }
            });
            return status;
        }
        public async Task<string> UpdatelbrIdAttributes(long lbrid, string title, string desc)
        {
            string status = "";
            IDbDataParameter[] parameters = null;
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(3);
                    parameters[0] = dbAccessor.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pTitle", DbType.String, title.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pDsc", DbType.String, desc.ToUpper(), ParameterDirection.Input);
                    dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.update_lbr_id", parameters);
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to update labor id ({0}, {1},{2})", lbrid, title, desc);
                }
                finally
                {
                }
            });
            return status;
        }
        public async Task<string> SaveLbrclassification(long lbrid, JArray mtrlClsIdLst)
        {
            string status = "";
            long mtrlClsId;
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    DeleteLbrclassification(lbrid);
                    for (int i = 0; i < mtrlClsIdLst.Count; i++)
                    {
                        string selected = mtrlClsIdLst[i].Value<JObject>("selected").Value<string>("value").ToUpper();
                        if (string.IsNullOrEmpty(mtrlClsIdLst[i].Value<JObject>("mtrlclsfctnid").Value<string>("value")))
                            mtrlClsId = 0;
                        else
                            mtrlClsId = long.Parse(mtrlClsIdLst[i].Value<JObject>("mtrlclsfctnid").Value<string>("value"));
                        if (selected == "Y")
                        {
                            InsertLbrclassification(lbrid, mtrlClsId);
                        }
                    }
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to update labor Mtrl Classification ({0})", lbrid);
                }
                finally
                {
                }
            });
            return status;
        }
        public void DeleteLbrclassification(long lbrid)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);
                parameters[0] = dbAccessor.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.delete_lbr_clsfctn", parameters);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                logger.Error(ex, "Unable to delete labor id ({0})", lbrid);
                throw ex;
            }
            finally
            {
            }
        }
        public void InsertLbrclassification(long lbrid, long mtrlClsId)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(2);
                parameters[0] = dbAccessor.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pMtrlClsId", DbType.Int64, mtrlClsId, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_lbr_clsfctn", parameters);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                logger.Error(ex, "Unable to insert material classfication for labor id -{0} mtrl class id - {1}", lbrid, mtrlClsId);
                throw ex;
            }
            finally
            {
            }
        }

        public async Task<string> UpdateLbrIdAU(long lbrid, JArray LbrIdexAULst, JArray LbrIdNewAULst)
        {
            string status = "";
            long LbrIdAuId;
            long LbrIdAu, LbrIdAuAltId, MtplrNo, AuId, AltAuId, mtplrno;

            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    for (int i = 0; i < LbrIdexAULst.Count; i++)
                    {
                        if (string.IsNullOrEmpty(LbrIdexAULst[i].Value<JObject>("lbridauid").Value<string>("value")))
                            LbrIdAu = 0;
                        else
                            LbrIdAu = long.Parse(LbrIdexAULst[i].Value<JObject>("lbridauid").Value<string>("value"));

                        if (string.IsNullOrEmpty(LbrIdexAULst[i].Value<JObject>("lbridaualtid").Value<string>("value")))
                            LbrIdAuAltId = 0;
                        else
                            LbrIdAuAltId = long.Parse(LbrIdexAULst[i].Value<JObject>("lbridaualtid").Value<string>("value"));
                        if (string.IsNullOrEmpty(LbrIdexAULst[i].Value<JObject>("mtplrno").Value<string>("value")))
                            MtplrNo = 0;
                        else
                            MtplrNo = long.Parse(LbrIdexAULst[i].Value<JObject>("mtplrno").Value<string>("value"));
                        string IsSelected = LbrIdexAULst[i].Value<JObject>("isselected").Value<string>("value").ToUpper();
                        UpdateLbrIdAu(LbrIdAu, LbrIdAuAltId, MtplrNo, IsSelected);
                    }
                    for (int i = 0; i < LbrIdNewAULst.Count; i++)
                    {
                        if (string.IsNullOrEmpty(LbrIdNewAULst[i].Value<JObject>("mtplrno").Value<string>("value")))
                            mtplrno = 0;
                        else
                            mtplrno = long.Parse(LbrIdNewAULst[i].Value<JObject>("mtplrno").Value<string>("value"));
                        string defaultAu = LbrIdNewAULst[i].Value<JObject>("default").Value<string>("value").ToUpper();
                        string alternativeAu = LbrIdNewAULst[i].Value<JObject>("alternative").Value<string>("value").ToUpper();
                        if (defaultAu == "Y")
                        {
                            if (string.IsNullOrEmpty(LbrIdNewAULst[i].Value<JObject>("auid").Value<string>("value")))
                                AuId = 0;
                            else
                                AuId = long.Parse(LbrIdNewAULst[i].Value<JObject>("auid").Value<string>("value"));
                            InsertLbrIdAu(lbrid, AuId, mtplrno);
                        }
                        if (alternativeAu == "Y")
                        {
                            if (string.IsNullOrEmpty(LbrIdNewAULst[i].Value<JObject>("auid").Value<string>("value")))
                                AltAuId = 0;
                            else
                                AltAuId = long.Parse(LbrIdNewAULst[i].Value<JObject>("auid").Value<string>("value"));
                            if (string.IsNullOrEmpty(LbrIdNewAULst[i].Value<JObject>("lbridauid").Value<string>("value")))
                                LbrIdAuId = 0;
                            else
                                LbrIdAuId = long.Parse(LbrIdNewAULst[i].Value<JObject>("lbridauid").Value<string>("value"));
                            InsertLbrIdAuAlt(LbrIdAuId, AltAuId, mtplrno);
                        }
                    }
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to update labor id existing assembly unit association ({0})", lbrid);
                }
                finally
                {
                }
            });

            return status;
        }

        //public async Task<string> SaveLbrIdAuAltUnit(long lbrid, JArray LbrIdAULst)
        //{
        //    long LbrIdAuId = lbrid;
        //    string status = "";
        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            StartTransaction();
        //            for (int i = 0; i < LbrIdAULst.Count; i++)
        //            {
        //                if (string.IsNullOrEmpty(LbrIdexAULst[i].Value<JObject>("lbridauid").Value<string>("value")))
        //                    LbrIdAu = 0;
        //                else
        //                    LbrIdAu = long.Parse(LbrIdexAULst[i].Value<JObject>("lbridauid").Value<string>("value"));
        //                 AuId = long.Parse(LbrIdAULst[i].Value<JObject>("auid").Value<string>("value"));
        //                AltAuId = long.Parse(LbrIdAULst[i].Value<JObject>("alttoauid").Value<string>("value"));
        //                mtplrno = long.Parse(LbrIdAULst[i].Value<JObject>("mtplrno").Value<string>("value"));

        //                string alternativeAu = LbrIdAULst[i].Value<JObject>("alternative").Value<string>("value").ToUpper();
        //                InsertLbrIdAu(lbrid, AuId, mtplrno);
        //                if (alternativeAu == "Y") { InsertLbrIdAuAlt(LbrIdAuId, AltAuId, mtplrno); }
        //            }
        //            CommitTransaction();
        //            status = "success";
        //        }
        //        catch (Exception ex)
        //        {
        //            RollbackTransaction();
        //            logger.Error(ex, "Unable to save  labor id new assembly units association ({0})", lbrid);
        //        }
        //        finally
        //        {
        //        }
        //    });
        //    return status;
        //}
        public void InsertLbrIdAu(long lbrid, long AuId, long MtplrNo)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(3);
                parameters[0] = dbAccessor.GetParameter("pLbrId", DbType.Int64, lbrid, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pAuId", DbType.Int64, AuId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMtplrNo", DbType.Int64, MtplrNo, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_lbr_id_au", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to add assembly unit to labor id ({0}, {1}, {2})", lbrid, AuId, MtplrNo);
                throw ex;
            }
            finally
            {
            }
        }
        public void InsertLbrIdAuAlt(long LbrIdAuId, long AltAuId, long MtplrNo)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(3);
                parameters[0] = dbAccessor.GetParameter("pLbrIdAuId", DbType.Int64, LbrIdAuId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pAltAuId", DbType.Int64, AltAuId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMtplrNo", DbType.Int64, MtplrNo, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_lbr_id_au_alt", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to add alternative assembly unit to labor id ({0}, {1}, {2})", LbrIdAuId, AltAuId, MtplrNo);
                throw ex;
            }
            finally
            {
            }

        }
        public void UpdateLbrIdAu(long LbrIdAuId, long LbrIdAuAltId, long MtplrNo, string IsSelected)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(4);
                parameters[0] = dbAccessor.GetParameter("pLbrIdAuId", DbType.Int64, LbrIdAuId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pLbrIdAuAltId", DbType.Int64, LbrIdAuAltId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pMtplrNo", DbType.Int64, MtplrNo, ParameterDirection.Input);
                parameters[3] = dbAccessor.GetParameter("pIsSelected", DbType.String, IsSelected.ToUpper(), ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.update_lbr_id_au", parameters);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update labor id existing assembly units ({0}, {1}, {2}, {3})", LbrIdAuId, LbrIdAuAltId, MtplrNo, IsSelected);
                throw ex;
            }
            finally
            {
            }

        }
        public async Task<List<Dictionary<string, Models.Attribute>>> GetFireworks(string name, string notRetired, string section)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Dictionary<string, Models.Attribute>> FireworksLst = null;
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            Dictionary<string, Models.Attribute> attr = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(4);

                    parameters[0] = dbManager.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pNotRetFilterOn", DbType.String, notRetired.ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pSectionFilterOn", DbType.String, section.ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.search_fireworks", parameters);

                    while (reader.Read())
                    {
                        if (FireworksLst == null)
                            FireworksLst = new List<Dictionary<string, Models.Attribute>>();

                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.FireworksName)
                        {
                            attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.SEARCH_FIREWORKS));
                        }

                        FireworksLst.Add(attr);

                        attr = null;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Fireworks list {0}", name);
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

            return FireworksLst;
        }
        public async Task<string> InsertFireworksAu(string name, long calcId, long uomId, string AuSK, string cuid)
        {
            string status = "";
            IDbDataParameter[] parameters = null;
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(5);
                    parameters[0] = dbAccessor.GetParameter("pNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pCalcId", DbType.Int64, calcId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pUomId", DbType.Int64, uomId, ParameterDirection.Input);
                    parameters[3] = dbAccessor.GetParameter("pAuSk", DbType.String, AuSK.ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbAccessor.GetParameter("pCuid", DbType.String, cuid.ToUpper(), ParameterDirection.Input);
                    dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_au", parameters);
                    CommitTransaction();
                    status = "success";
                }

                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to insert fireworks assembly unit page ({0}, {1}, {2},{3}, {4})", name, calcId, uomId, AuSK, cuid);
                    throw ex;
                }
                finally
                {
                }
            });
            return status;
        }
        public void DeleteOprForCalculations(long AuCalcId)
        {
            IDbDataParameter[] parameters = null;

            try
            {
                parameters = dbAccessor.GetParameterArray(1);
                parameters[0] = dbAccessor.GetParameter("pAuCalcId", DbType.Int64, AuCalcId, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.delete_oprtns_for_clcltn", parameters);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                logger.Error(ex, "Unable to delete operations for calculation id ({0})", AuCalcId);
                throw ex;
            }
            finally
            {
            }
        }
        public void InsertOprForCalculations(long AuCalcId, long AuOpId, long OrdrOfOpNo)
        {
            IDbDataParameter[] parameters = null;
            try
            {
                parameters = dbAccessor.GetParameterArray(3);
                parameters[0] = dbAccessor.GetParameter("pAuCalcId", DbType.Int64, AuCalcId, ParameterDirection.Input);
                parameters[1] = dbAccessor.GetParameter("pAuOpId", DbType.Int64, AuOpId, ParameterDirection.Input);
                parameters[2] = dbAccessor.GetParameter("pOrdrOfOpNo", DbType.Int64, OrdrOfOpNo, ParameterDirection.Input);
                dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_oprtns_for_clcltn", parameters);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                logger.Error(ex, "Unable to insert operations for calculation id -{0} operations id - {1}", AuCalcId, AuOpId);
                throw ex;
            }
            finally
            {
            }
        }
        public async Task<string> InsertOperations(string OpNm, long AuOpOprId, long DataTermId, long ConstantNo, long VarTermId)
        {
            IDbDataParameter[] parameters = null;
            string status = "";
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(5);
                    parameters[0] = dbAccessor.GetParameter("pOpNm", DbType.String, OpNm.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pAuOpOprId", DbType.Int64, AuOpOprId, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pDataTermId", DbType.Int64, DataTermId, ParameterDirection.Input);
                    parameters[3] = dbAccessor.GetParameter("pConstantNo", DbType.Int64, ConstantNo, ParameterDirection.Input);
                    parameters[4] = dbAccessor.GetParameter("pVarTermId", DbType.Int64, VarTermId, ParameterDirection.Input);
                    dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_operation", parameters);
                    status = "success";
                    CommitTransaction();
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to add New operations ({0}, {1}, {2})", OpNm, AuOpOprId, DataTermId);
                    throw ex;
                }
                finally
                {
                }
            });
            return status;
        }

        public long InsertAuCalculation(string calcNm)
        {
            IDbDataParameter[] parameters = null;
            long auCalcId=0;
           
                try
                {
                   
                    parameters = dbAccessor.GetParameterArray(2);
                    parameters[0] = dbAccessor.GetParameter("pCalcNm", DbType.String, calcNm.ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("oAuCalcId", DbType.Int64, auCalcId, ParameterDirection.Output);                    
                    dbAccessor.ExecuteNonQuerySP("assembly_unit_pkg.insert_au_calc", parameters);
                    auCalcId = long.Parse(parameters[1].Value.ToString());
                   
                }
                catch (Exception ex)
                {
                  
                    logger.Error(ex, "Unable to add New operations ({0})", calcNm);
                    throw ex;
                }
                finally
                {
                }
         
            return auCalcId;
        }
        public async Task<string> SaveOprforCalculation(string calcNm,JArray oprCalcLst)
        {
            string status = "";
            long AuCalcId, AuOpId, OrdrOfOpNo;
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    AuCalcId=InsertAuCalculation(calcNm);
                    for (int i = 0; i < oprCalcLst.Count; i++)
                    {
                        if (string.IsNullOrEmpty(oprCalcLst[i].Value<JObject>("auOpId").Value<string>("value")))
                            AuOpId = 0;
                        else
                            AuOpId = long.Parse(oprCalcLst[i].Value<JObject>("auOpId").Value<string>("value"));
                        if (string.IsNullOrEmpty(oprCalcLst[i].Value<JObject>("ordrOfOprtn").Value<string>("value")))
                            OrdrOfOpNo = 0;
                        else
                            OrdrOfOpNo = long.Parse(oprCalcLst[i].Value<JObject>("ordrOfOprtn").Value<string>("value"));
                        InsertOprForCalculations(AuCalcId, AuOpId, OrdrOfOpNo);
                    }
                    CommitTransaction();
                    status = "success";
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to Save Operations for calculation");
                }
                finally
                {
                }
            });
            return status;
        }

        public async Task<List<Option>> filterLaborIds(long pMtrlCatId,long pFeatTypId,long pCablTypId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> options = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(4);
                    if (pMtrlCatId == 0)
                    {
                        parameters[0] = dbManager.GetParameter("pMtrlCatId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                    }else
                    {
                        parameters[0] = dbManager.GetParameter("pMtrlCatId", DbType.Int64, pMtrlCatId, ParameterDirection.Input);
                    }
                    if (pFeatTypId == 0)
                    {
                        parameters[1] = dbManager.GetParameter("pFeatTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                    }
                    else
                    {
                        parameters[1] = dbManager.GetParameter("pFeatTypId", DbType.Int64, pFeatTypId, ParameterDirection.Input);
                    }
                    if (pCablTypId == 0)
                    {
                        parameters[2] = dbManager.GetParameter("pCablTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                    }
                    else                    {
                        parameters[2] = dbManager.GetParameter("pCablTypId", DbType.Int64, pCablTypId, ParameterDirection.Input);
                    }
                    parameters[3] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("assembly_unit_pkg.get_lbr_id_by_mtrl_clsfctn", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }
                        string optionValue = reader["option_value"].ToString().ToUpper();
                        string optionText = reader["option_text"].ToString().ToUpper();
                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Labor Id drop down list for Assembly unit page");
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