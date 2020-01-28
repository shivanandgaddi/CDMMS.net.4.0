using System;
using CenturyLink.ApplicationBlocks.Data;
using NLog;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public abstract class SpecificationDbInterfaceImpl : ISpecificationDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        protected IAccessor dbAccessor = null;

        public SpecificationDbInterfaceImpl()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public SpecificationDbInterfaceImpl(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public string DbConnectionString
        {
            get
            {
                if (connectionString == null)
                    connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];

                return connectionString;
            }
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

        public object CheckNullValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return DBNull.Value;
            else
                return val.ToUpper();
        }

        public object CheckNullValue(string val, bool skipToUpper)
        {
            if (string.IsNullOrEmpty(val))
                return DBNull.Value;
            else
            {
                if (skipToUpper)
                    return val;
                else
                    return val.ToUpper();
            }
        }

        public object CheckNullValue(int val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }

        public object CheckNullValue(long val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }

        public object CheckNullValue(decimal val)
        {
            if (val <= 0)
                return DBNull.Value;
            else
                return val;
        }

        public abstract ISpecification GetSpecification(long specificationId);

        public abstract void AssociateMaterial(JObject jObject);
    }
}