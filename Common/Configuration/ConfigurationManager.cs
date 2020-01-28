using System;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.Common.Utility;

namespace CenturyLink.Network.Engineering.Common.Configuration
{
    public class ConfigurationManager
    {
        private static string connectionString = string.Empty;

        private ConfigurationManager()
        {
        }

        public static string Value(string category, string key)
        {
            if (String.IsNullOrEmpty(connectionString))
                connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];

            return ConfigurationDbInterface.GetValue(category, key, connectionString);
        }

        //public static string Value(string key)
        //{
        //    if (String.IsNullOrEmpty(connectionString))
        //        connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];

        //    return ConfigurationDbInterface.GetValue(key, connectionString);
        //}

        public static string Value(APPLICATION_NAME application, string key)
        {
            if (String.IsNullOrEmpty(connectionString))
                connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];

            if (application == APPLICATION_NAME.NEISL)
                return Value(Constants.ConfigurationCategory(CONFIGURATION_CATEGORY.POLLING), key);
            else
                return ConfigurationDbInterface.GetValue(application, key, connectionString);
        }

        public static string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }
    }
}
