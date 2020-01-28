using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.ApplicationBlocks.Data
{
    /// <summary>
	/// This class is Factory for all the Database access providers, which would implement IAccessor interface.
	/// </summary>
    public class DataAccessorFactory
    {
        /// <summary>
		/// This enum defines all the database this Factory can give access to
		/// </summary>
		public enum DATABASE
        {
            Oracle
            //,MsSql
        }

        /// <summary>
        /// Static method. It returns DataAccessor class for a given Database and Connection String.
        /// </summary>
        /// <param name="eDb">Database, Oracle, MSSql etc</param>
        /// <param name="sConnection">Connection string</param>
        /// <returns></returns>
        public static IAccessor GetDataAccessor(DATABASE eDb, string sConnection)
        {
            IAccessor iaAccessor = null;

            //Connection string should not be passed as null.
            if (sConnection == null && sConnection.Trim().Length == 0)
            {
                throw new Exception("Connection string cannot be null.");
            }
            else
            {
                switch (eDb)
                {
                    case DATABASE.Oracle:
                        iaAccessor = new DataAccessorOracle(sConnection);
                        break;
                    /*case DATABASE.MsSql:
                        iaAccessor = new DataAccessorSql(sConnection);
                        break;*/
                    default:
                        throw new Exception("No implementation for the specified Database.");
                }
            }
            return iaAccessor;
        }

        /// <summary>
        /// Use this method if database is specified in config file.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns></returns>
        public static IAccessor GetDataAccessor(string connectionString)
        {
            string database = System.Configuration.ConfigurationManager.AppSettings["Database"];

            if (connectionString == null || connectionString.Length == 0)
            {
                throw new Exception("Connection string cannot be null.");
            }
            else if (database == null || database.Length == 0)
            {
                throw new Exception("Property 'Database' should be set in config file.");
            }

            return DataAccessorFactory.GetDataAccessor((DATABASE)Enum.Parse(typeof(DATABASE), database, true), connectionString);
        }
    }
}
