using System;
using System.Data;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Utility;
using NLog;

namespace CenturyLink.Network.Engineering.Common.DbInterface
{
    internal class ConfigurationDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ConfigurationDbInterface()
        {

        }

        public static string GetValue(string category, string key, string connectionString)
        {
            string configValue = string.Empty;
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                parameters = dbManager.GetParameterArray(3);

                parameters[0] = dbManager.GetParameter("pCategory", DbType.String, category, ParameterDirection.Input, 50);
                parameters[1] = dbManager.GetParameter("pKey", DbType.String, key, ParameterDirection.Input, 50);
                parameters[2] = dbManager.GetParameter("oValue", DbType.String, configValue, ParameterDirection.Output, 4000);

                dbManager.ExecuteScalarSP("NEISL_PKG.GET_CONFIGURATION_VALUE", parameters);

                configValue = parameters[2].Value.ToString();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "category: {0}; key: {1}", category, key);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return configValue;
        }

        public static string GetValue(APPLICATION_NAME application, string key, string connectionString )
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string value = "";
            string sQuery = @"select config_value 
                                from system_configuration 
                                where config_key = :key 
                                and application_nm = :applicationName";

            if (application == APPLICATION_NAME.NEISL)
                return GetValue(Constants.ConfigurationCategory(CONFIGURATION_CATEGORY.POLLING), key, connectionString);            
            
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("key", DbType.String, key, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameter("applicationName", DbType.String, Constants.ApplicationName(application), ParameterDirection.Input);

                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery, parameters);

                while (reader.Read())
                {
                    value = reader[0].ToString();                    
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "application name: {0}; key: {1}", Constants.ApplicationName(application), key);
            }
            finally
            {
                if (dbManager != null)
                    dbManager.Dispose();
            }

            return value;
        }
    }
}