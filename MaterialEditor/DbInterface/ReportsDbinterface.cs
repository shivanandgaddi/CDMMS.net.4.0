using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


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
    public class ReportsDbinterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private IAccessor dbAccessor = null;
        public ReportsDbinterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }
        public ReportsDbinterface(string dbConnectionString)
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
        public async Task<List<Option>> GetAvailableReportsList()
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
                    reader = dbManager.ExecuteDataReaderSP("user_reports_pkg.get_available_reports", parameters);

                    while (reader.Read())
                    {
                        if (options == null)
                        {
                            options = new List<Option>();

                            options.Add(new Option("", ""));
                        }
                        string optionValue = reader["user_reports_id"].ToString().ToUpper();
                        string optionText = reader["name"].ToString().ToUpper();
                        options.Add(new Option(optionValue, optionText));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get available reports drop down list for reports page");
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
        public async Task<Reports> GetlbrIdreports(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Reports reportsList = new Reports();
            AssemblyUnitType AsmblyuntType = new AssemblyUnitType();
            ReportsType ReportsMtrlItmType = new ReportsType();
            Dictionary<string, Models.Attribute> attr = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(3);

                    parameters[0] = dbManager.GetParameter("pLbrId", DbType.Int64, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("oLbrIdCsr", ParameterDirection.Output);
                    parameters[2] = dbManager.GetParameterCursorType("oMtrlCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP("user_reports_pkg.get_labor_id_report", parameters);

                    while (reader.Read())
                    {
                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in AsmblyuntType.LaborIdAUnit)
                        {
                            attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), AssemblyUnitType.GET_ASSEMBLY_UNITS_FOR_LBR_ID));
                        }
                        reportsList.LbrIdAUList.Add(attr);
                        attr = null;
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        attr = new Dictionary<string, Models.Attribute>();

                        foreach (KeyValuePair<string, DatabaseDefinition> keyValue in ReportsMtrlItmType.LbrIdMtrldict)
                        {
                            attr.Add(keyValue.Key, new Models.Attribute(keyValue.Key, DataReaderHelper.GetNonNullValue(reader, keyValue.Value.Column, keyValue.Value.IsNumber).ToUpper(), ReportsType.GET_LABOR_ID_REPORT));
                        }
                        reportsList.LbrIdMtrlList.Add(attr);
                        attr = null;

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Labor Id associated Assembly units and Material Item for labor id {0}", id);
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

            return reportsList;
        }

    }
}