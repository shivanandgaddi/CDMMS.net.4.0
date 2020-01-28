using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface
{
    public class MockDataDbInterface
    {
        private string connectionString = string.Empty;

        public MockDataDbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public MockDataDbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public string InsertData(string data, string description, string controller, string route)
        {
            OracleConnection connection = null;
            OracleCommand command = null;
            OracleParameter[] parameters = null;
            string message = "Success.";
            string errorMessage = "";
            int returnCode = 0;
            int errorCode = 0;

            try
            {
                connection = new OracleConnection(connectionString);
                command = connection.CreateCommand();
                parameters = new OracleParameter[7];

                command.CommandText = "mock_data_pkg.insert_data";
                command.CommandType = CommandType.StoredProcedure;

                parameters[0] = new OracleParameter("pData", OracleDbType.Clob, data, ParameterDirection.Input);
                parameters[1] = new OracleParameter("pDesc", OracleDbType.Varchar2, description, ParameterDirection.Input);
                parameters[2] = new OracleParameter("pCntrlr", OracleDbType.Varchar2, controller, ParameterDirection.Input);
                parameters[3] = new OracleParameter("pRte", OracleDbType.Varchar2, route, ParameterDirection.Input);
                parameters[4] = new OracleParameter("retCD", OracleDbType.Int32, returnCode, ParameterDirection.Output);
                parameters[5] = new OracleParameter("error_cd", OracleDbType.Int32, errorCode, ParameterDirection.Output);
                parameters[6] = new OracleParameter("err_msg", OracleDbType.Varchar2, errorMessage, ParameterDirection.Output);

                command.Parameters.AddRange(parameters);

                connection.Open();

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (command != null)
                    command.Dispose();

                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return message;
        }

        public async Task<string> GetData(long id)
        {
            OracleConnection connection = null;
            OracleCommand command = null;
            OracleParameter[] parameters = null;
            OracleDataReader reader = null;
            string data = "{}";
            string errorMessage = "";
            int returnCode = 0;
            int errorCode = 0;

            await Task.Run(() =>
            {
                try
                {
                    connection = new OracleConnection(connectionString);
                    command = connection.CreateCommand();
                    parameters = new OracleParameter[5];

                    command.CommandText = "mock_data_pkg.get_data";
                    command.CommandType = CommandType.StoredProcedure;

                    parameters[0] = new OracleParameter("pId", OracleDbType.Int32, id, ParameterDirection.Input);
                    parameters[1] = new OracleParameter("retcsr", OracleDbType.RefCursor, null, ParameterDirection.Output);
                    parameters[2] = new OracleParameter("retCD", OracleDbType.Int32, returnCode, ParameterDirection.Output);
                    parameters[3] = new OracleParameter("error_cd", OracleDbType.Int32, errorCode, ParameterDirection.Output);
                    parameters[4] = new OracleParameter("err_msg", OracleDbType.Varchar2, errorMessage, ParameterDirection.Output);

                    command.Parameters.AddRange(parameters);

                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        data = reader.GetOracleClob(reader.GetOrdinal("data")).Value;
                    }
                }
                catch (Exception ex)
                {
                    data = ex.Message;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (command != null)
                        command.Dispose();

                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            });

            return data;
        }
    }
}