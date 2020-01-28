using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CenturyLink.ApplicationBlocks.Data;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface
{
    public class AuditDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private IAccessor dbAccessor = null;

        public AuditDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public AuditDbInterface(string dbConnectionString)
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

        public async Task<string> InsertAudit(long AuditColumnDefId, string AuditTablePKColumnName, string AuditTablePKColumnValue,
            string AuditParentTablePKColumnName, string AuditParentTablePKColumnValue, string ActionCode,
            string OldColumnValue, string NewColumnValue, string CUID, string CommentText)
        {
            IDbDataParameter[] parameters = null;
            string status = "";
            await Task.Run(() =>
            {
                try
                {
                    StartTransaction();
                    parameters = dbAccessor.GetParameterArray(10);
                    parameters[0] = dbAccessor.GetParameter("pAuditColDefId", DbType.Int64, AuditColumnDefId, ParameterDirection.Input);
                    parameters[1] = dbAccessor.GetParameter("pAuditTblPkColNm", DbType.String, AuditTablePKColumnName, ParameterDirection.Input);
                    parameters[2] = dbAccessor.GetParameter("pAuditTblPkColVal", DbType.String, AuditTablePKColumnValue, ParameterDirection.Input);
                    parameters[3] = dbAccessor.GetParameter("pAuditPrntTblPkColNm", DbType.String, AuditParentTablePKColumnName, ParameterDirection.Input);
                    parameters[4] = dbAccessor.GetParameter("pAuditPrntTblPkColVal", DbType.String, AuditParentTablePKColumnValue, ParameterDirection.Input);
                    parameters[5] = dbAccessor.GetParameter("pActnCd", DbType.String, ActionCode, ParameterDirection.Input);
                    parameters[6] = dbAccessor.GetParameter("pOldColVal", DbType.String, OldColumnValue, ParameterDirection.Input);
                    parameters[7] = dbAccessor.GetParameter("pNewColVal", DbType.String, NewColumnValue, ParameterDirection.Input);
                    parameters[8] = dbAccessor.GetParameter("pCreatActyId", DbType.String, CUID, ParameterDirection.Input);
                    parameters[9] = dbAccessor.GetParameter("pCmntTxt", DbType.String, CommentText, ParameterDirection.Input);
                    dbAccessor.ExecuteNonQuerySP("audit_pkg.insert_audit_da", parameters);
                    status = "success";
                    CommitTransaction();
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to add new audit record ({0}, {1}, {2})", AuditColumnDefId, OldColumnValue, NewColumnValue);
                    throw ex;
                }
                finally
                {
                }
            });
            return status;
        }

        public async Task<string> GetAuditColDefID(string tableName, string columnName)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string auditColDefId = "";
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = dbManager.GetParameterArray(3);
                    parameters[0] = dbManager.GetParameter("pTblNm", DbType.String, tableName, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pColNm", DbType.String, columnName, ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("audit_pkg.get_audit_col_def_id", parameters);

                    while (reader.Read())
                    {
                        auditColDefId = reader["audit_col_def_id"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    logger.Error(ex, "Unable to GetAuditColDefID ({0}, {1})", tableName, columnName);
                    throw ex;
                }
                finally
                {
                }
            });
            return auditColDefId;
        }
    }
}