using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.TypeLibrary;
using NLog;

namespace CenturyLink.Network.Engineering.Common.DbInterface
{
    public class WorkDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public WorkDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public long InsertWorkToDo(ChangeSet changeSet, ChangeRecord changeRecord, string callingService)
        {
            long workToDoId = 0;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            object obj = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(7);
                parameters[0] = dbManager.GetParameter("pChngSetId", DbType.Int64, changeSet.ChangeSetId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pPrjctId", DbType.Int64, changeSet.ProjectId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameter("pPniId", DbType.Int64, long.Parse(changeRecord.PniId), ParameterDirection.Input);
                parameters[3] = dbManager.GetParameter("pTblNm", DbType.String, changeRecord.TableName, ParameterDirection.Input);
                parameters[4] = dbManager.GetParameter("pWrkTyp", DbType.String, callingService, ParameterDirection.Input);
                parameters[5] = dbManager.GetParameter("pStatus", DbType.String, Constants.ChangeSetStatus(CHANGE_SET_STATUS.READY), ParameterDirection.Input);
                parameters[6] = dbManager.GetParameter("outId", DbType.Int64, workToDoId, ParameterDirection.Output);

                obj = dbManager.ExecuteScalarSP("WORK_TO_DO_PKG.INSERT_WORK_TO_DO", parameters);

                workToDoId = long.Parse(parameters[6].Value.ToString());

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, ex, "WorkDbInterface.InsertWorkToDo({0})", workToDoId);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return workToDoId;
        }

        public ChangeRecord GetNextRecord(long changeSetId, long workToDoId)
        {
            ChangeRecord record = null;
            IAccessor dbManager = null;
            IDataReader reader = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pChngSetId", DbType.Int64, changeSetId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pWrkToDoId", DbType.Int64, workToDoId, ParameterDirection.Input);
                parameters[2] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP("WORK_TO_DO_PKG.GET_NEXT_CHANGE_RECORD", parameters);

                if (reader.Read())
                {
                    record = new ChangeRecord();

                    record.ChangeId = long.Parse(reader["changeid"].ToString());
                    record.OperationType = reader["operationtype"].ToString();
                    record.PniId = reader["pniid"].ToString();
                    record.TableName = reader["tablename"].ToString();
                }
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

            return record;
        }

        public void UpdateWorkToDo(long workToDoId, CHANGE_SET_STATUS status)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pWrkToDoId", DbType.Int64, workToDoId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("pStatus", DbType.String, Constants.ChangeSetStatus(status), ParameterDirection.Input);

                dbManager.ExecuteNonQuerySP("WORK_TO_DO_PKG.UPDATE_WORK_TO_DO", parameters);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, ex, "WorkDbInterface.UpdateWorkToDo({0}, {1})", workToDoId, Constants.ChangeSetStatus(status));
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }
        }

        public void InsertMasterPart(string productID)
        {
            IAccessor dbManager = null;

            string sql = @"insert into material_item " +
                "(record_only_ind, created_date, product_id) " +
                "values('N', sysdate, '" + productID + "')";

            try
            {

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                // nearly all errors that will fall into here are for records that have a product_id that already exist, which is ok
                string sErrMsg = "Error while inserting Master Part record - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "WorkDbInterface.InsertMasterPart({0})", productID);
            }
            finally
            {
                dbManager.Dispose();
            }
        }
    }
}
