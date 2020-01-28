using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.TypeLibrary;
using NLog;

namespace CenturyLink.Network.Engineering.Common.DbInterface
{
    public class MaterialFlowThruDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;

        public MaterialFlowThruDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public FlowThruRecord GetFlowThruRecords()
        {
            FlowThruRecord flowThruRecord = new FlowThruRecord();

            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select originating_field_nm, field_description, flow_thru_ind, originating_system, " +
                "last_updtd_userid, last_updtd_tmstmp from material_flow_thru";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    flowThruRecord.OriginatingFieldName = reader["originating_field_name"].ToString();
                    flowThruRecord.FieldDescription = reader["field_description"].ToString();
                    flowThruRecord.FlowThruIndicator = reader["flow_thru_ind"].ToString();
                    flowThruRecord.OriginatingSystem = reader["originating_system"].ToString();
                    flowThruRecord.LastUpdatedUserID = reader["last_updtd_userid"].ToString();
                    flowThruRecord.LastUpdateDate = DateTime.Parse(reader["last_updtd_tmstmp"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while reading material_flow_thru - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialFlowThruDbInterface.GetFlowThruRecords({0})", sErrMsg);
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

            return flowThruRecord;
        }

        public Dictionary<string,string> GetFlowThruIndicators (string originatingSystem)
        {
            Dictionary<string, string> flowThruIndicators = new Dictionary<string, string>();

            IDataReader reader = null;
            IAccessor dbManager = null;

            string @sql = "select originating_field_nm, flow_thru_ind from material_flow_thru " +
                "where originating_system = '" + originatingSystem + "'";

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sql);

                while (reader.Read())
                {
                    flowThruIndicators.Add(reader["originating_field_nm"].ToString(), reader["flow_thru_ind"].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while reading material_flow_thru - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialFlowThruDbInterface.GetFlowThruIndicators({0})", sErrMsg);
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

            return flowThruIndicators;
        }

        public int UpdateFlowThruRecordIndicator(FlowThruRecord flowThruRecord)
        {
            int returnValue = -1;

            IAccessor dbManager = null;

            string sql = @"update material_flow_thru set flow_thru_ind = '" + flowThruRecord.FlowThruIndicator + "' " +
                "where originating_field_nm = '" + flowThruRecord.OriginatingFieldName + "' " +
                "and originating_system = '" + flowThruRecord.OriginatingSystem + "'";

            try
            {

                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                returnValue = dbManager.ExecuteNonQuery(CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while updating material_flow_thru - " + ex.Message;
                logger.Log(LogLevel.Info, ex, "MaterialFlowThruDbInterface.UpdateFlowThruRecordIndicator({0}, {1})", flowThruRecord.OriginatingFieldName, flowThruRecord.OriginatingSystem);
            }
            finally
            {
                dbManager.Dispose();
            }

            return returnValue;
        }
    }
}
