using System;
using System.Collections;
using System.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace CenturyLink.ApplicationBlocks.Data
{
    /// <summary>
	/// This class is used by DataAccessorOracle class to call different ADO.NET execute methods.
	/// This also created command and adaptor objects.
	/// </summary>
    public class OracleHelper
    {
        #region ExecuteNonQuery	

        /// <summary>
        /// Execute a OracleCommand (that returns no resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        {
            if (tran != null)
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

                //finally, execute the command.
                int retval = cmd.ExecuteNonQuery();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
            //create a command and prepare it for execution
            using (OracleCommand cmd = new OracleCommand())
            {
                PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

                //finally, execute the command.
                int retval = cmd.ExecuteNonQuery();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
        }
        /// <summary>
        /// Execute a OracleCommand (that returns no resultset) against the specified OracleConnection 
        /// using the provided parameter names and values
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramNames"></param>
        /// <param name="paramValues"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, IDbConnection con, ArrayList paramNames, ArrayList paramValues, IDbTransaction tran)
        {
            if (tran != null)
            {
                OracleCommand cmd = new OracleCommand();
                try
                {
                    PrepareCommand(cmd, cmdType, cmdText, con, paramNames, paramValues, tran);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //finally, execute the command.
                int retval = cmd.ExecuteNonQuery();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
            //create a command and prepare it for execution
            using (OracleCommand cmd = new OracleCommand())
            {
                try
                {
                    PrepareCommand(cmd, cmdType, cmdText, con, paramNames, paramValues, tran);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //finally, execute the command.
                int retval = cmd.ExecuteNonQuery();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
        }
        #endregion ExecuteNonQuery

        #region ExecuteDataSet

        /// <summary>
        /// Execute a OracleCommand (that returns a resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        //		public static DataSet ExecuteDataset(CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        //		{
        //			//create a command and prepare it for execution
        //			OracleCommand cmd = new OracleCommand();
        //			PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);
        //			
        //			//create the DataAdapter & DataSet
        //			OracleDataAdapter da = new OracleDataAdapter(cmd);
        //			DataSet ds = new DataSet();
        //
        //			//fill the DataSet using default values for DataTable names, etc.
        //			da.Fill(ds);
        //			
        //			// detach the OracleParameters from the command object, so they can be used again.			
        //			cmd.Parameters.Clear();
        //			
        //			//return the DataSet
        //			return ds;
        //		}

        /// <summary>
        /// Able to fill a typed DataSet passed in with table to be filled
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static void ExecuteDataset(string tableName, DataSet ds, CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        {
            //create a command and prepare it for execution
            using (OracleCommand cmd = new OracleCommand())
            {
                PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

                //create the DataAdapter 
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {

                    //If table name is null
                    if (tableName == null || tableName.Length == 0)
                    {
                        da.Fill(ds);
                    }
                    else
                    {
                        //fill the DataSet using default values for DataTable names, etc.
                        da.Fill(ds, tableName);
                    }
                }
                // detach the OracleParameters from the command object, so they can be used again.			
                cmd.Parameters.Clear();
            }
        }
        #endregion ExecuteDataSet

        #region ExecuteScalar

        /// <summary>
        /// Execute a stored procedure via a OracleCommand (that returns a 1x1 resultset) against the specified
        /// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        ///  </remarks>
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="param"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static object ExecuteScalar(CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        {
            if (tran != null)
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

                //execute the command & return the results
                object retval = cmd.ExecuteScalar();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
            //create a command and prepare it for execution
            using (OracleCommand cmd = new OracleCommand())
            {
                PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

                //execute the command & return the results
                object retval = cmd.ExecuteScalar();

                // detach the OracleParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
                return retval;
            }
        }
        #endregion ExecuteScalar

        #region ExecuteDataReader	

        /// <summary>
        /// Execute a OracleCommand (that returns no resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static OracleDataReader ExecuteDataReader(CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

            //finally, execute the command.
            OracleDataReader retval = cmd.ExecuteReader();

            // detach the OracleParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a OracleCommand (that returns no resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static OracleDataReader ExecuteDataReader(CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran, out object command)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, cmdType, cmdText, con, paramList, tran);

            //finally, execute the command.
            OracleDataReader retval = cmd.ExecuteReader();

            // detach the OracleParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            command = cmd;
            return retval;
        }
        #endregion ExecuteDataReader

        #region PrepareCommand
        /// <summary>
        /// Prepares command object with command type, command text, connectin and transaction.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="paramList"></param>
        /// <param name="tran"></param>
        public static void PrepareCommand(OracleCommand cmd, CommandType cmdType, string cmdText, IDbConnection con, IDbDataParameter[] paramList, IDbTransaction tran)
        {
            //if the provided connection is not open, we will open it
            //command.CommandTimeout = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("command_timeout"));
            if (con.State != ConnectionState.Open)
            {
                con.Open();
#if DEBUG
                DataAccessorOracle.current_count++;
                if (DataAccessorOracle.current_count > DataAccessorOracle.max_current_count)
                    DataAccessorOracle.max_current_count = DataAccessorOracle.current_count;
                DataAccessorOracle.total_count++;
#endif
            }

            //associate the connection with the command
            cmd.Connection = (OracleConnection)con;

            //set the command text (stored procedure name or Oracle statement)
            cmd.CommandText = cmdText;

            //if we were provided a transaction, assign it.
            //Note:In OPD.Net we don't need to associate transaction to a command object 
            //			if (tran != null)
            //			{
            //				cmd.Transaction = (OracleTransaction)tran;
            //			}

            //set the command type
            cmd.CommandType = cmdType;

            //attach the command parameters if they are provided
            if (paramList != null && paramList.Length != 0)
            {
                foreach (IDbDataParameter aParam in paramList)
                {
                    cmd.Parameters.Add((OracleParameter)aParam);
                }
            }
        }

        /// <summary>
        /// Prepares command object with command type, command text, connectin and transaction.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="con"></param>
        /// <param name="parameterNames"></param>
        /// <param name="parameterValues"></param>
        /// <param name="tran"></param>
        public static void PrepareCommand(OracleCommand cmd, CommandType cmdType, string cmdText, IDbConnection con, ArrayList parameterNames, ArrayList parameterValues, IDbTransaction tran)
        {
            //if the provided connection is not open, we will open it
            //command.CommandTimeout = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("command_timeout"));
            if (con.State != ConnectionState.Open)
            {
                con.Open();
#if DEBUG
                DataAccessorOracle.current_count++;
                if (DataAccessorOracle.current_count > DataAccessorOracle.max_current_count)
                    DataAccessorOracle.max_current_count = DataAccessorOracle.current_count;
                DataAccessorOracle.total_count++;
#endif
            }
            if (parameterValues[0] as string[] != null)
            {
                cmd.ArrayBindCount = (parameterValues[0] as string[]).Length;
            }
            OracleParameterCollection pc = cmd.Parameters;
            OracleParameter p;

            //associate the connection with the command
            cmd.Connection = (OracleConnection)con;

            //set the command text (stored procedure name or Oracle statement)
            cmd.CommandText = cmdText;

            //if we were provided a transaction, assign it.
            //Note:In OPD.Net we don't need to associate transaction to a command object 
            //			if (tran != null)
            //			{
            //				cmd.Transaction = (OracleTransaction)tran;
            //			}

            //set the command type
            cmd.CommandType = cmdType;
            //attach the command parameters if they are provided
            if (parameterNames != null && parameterNames.Count != 0
                && parameterValues != null && parameterValues.Count != 0)
            {
                int i = 0;
                foreach (string paramName in parameterNames)
                {
                    p = pc.Add(":" + paramName, OracleDbType.Varchar2, ParameterDirection.Input);
                    p.Value = parameterValues[i++];
                }
            }
        }
        #endregion PrepareCommand

        #region Update Data Set

        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet)
        {
            UpdateDataSet(DetailsAdapter, dataSet, new DataRowState[] { DataRowState.Added, DataRowState.Modified, DataRowState.Deleted });
        }

        //Changed on 06/29/2005
        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet, OracleTransaction transaction)
        {
            UpdateDataSet(DetailsAdapter, dataSet, new DataRowState[] { DataRowState.Added, DataRowState.Modified, DataRowState.Deleted }, dataSet.Tables[0].TableName, transaction);
        }

        //Method that uses default order of save and a defined tables
        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet, string srcTable)
        {
            UpdateDataSet(DetailsAdapter, dataSet, new DataRowState[] { DataRowState.Added, DataRowState.Modified, DataRowState.Deleted }, srcTable);
        }

        //Method that uses default table to update with order of save
        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet, DataRowState[] orderOfSave)
        {
            UpdateDataSet(DetailsAdapter, dataSet, orderOfSave, dataSet.Tables[0].TableName);
        }

        //Original UpdateDataSet Method 
        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet, DataRowState[] orderOfSave, string srcTable)
        {
            OracleConnection originalConnection = null;

            if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                originalConnection = DetailsAdapter.SelectCommand.Connection;
            else if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                originalConnection = DetailsAdapter.InsertCommand.Connection;
            else if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                originalConnection = DetailsAdapter.UpdateCommand.Connection;
            else if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                originalConnection = DetailsAdapter.DeleteCommand.Connection;



            OracleConnection connection = null;

            try
            {
                connection = GetOpenConnection(originalConnection.ConnectionString);

                for (int i = 0; i < orderOfSave.Length; i++)
                {
                    switch (orderOfSave[i])
                    {
                        case DataRowState.Added:
                            if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                                DetailsAdapter.SelectCommand.Connection = connection;
                            if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                            {
                                DetailsAdapter.InsertCommand.Connection = connection;
                                DataSet addedRows = dataSet.GetChanges(System.Data.DataRowState.Added);
                                if (addedRows != null)
                                    DetailsAdapter.Update(addedRows, srcTable);

                            }
                            break;
                        case DataRowState.Modified:
                            if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                            {
                                DetailsAdapter.UpdateCommand.Connection = connection;
                                DataSet changedRows = dataSet.GetChanges(System.Data.DataRowState.Modified);
                                if (changedRows != null)
                                    DetailsAdapter.Update(changedRows, srcTable);
                            }
                            break;
                        case DataRowState.Deleted:
                            if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                            {
                                DetailsAdapter.DeleteCommand.Connection = connection;
                                DataSet deletedRows = dataSet.GetChanges(System.Data.DataRowState.Deleted);
                                if (deletedRows != null)
                                    DetailsAdapter.Update(deletedRows, srcTable);
                            }
                            break;
                    }
                }
            }
            finally
            {
                CloseConnection(connection);

                if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                    DetailsAdapter.SelectCommand.Connection = originalConnection;
                if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                    DetailsAdapter.InsertCommand.Connection = originalConnection;
                if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                    DetailsAdapter.UpdateCommand.Connection = originalConnection;
                if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                    DetailsAdapter.DeleteCommand.Connection = originalConnection;
            }
        }


        //Changed on 06/29/2005
        //Original UpdateDataSet Method 
        public static void UpdateDataSet(OracleDataAdapter DetailsAdapter, DataSet dataSet, DataRowState[] orderOfSave, string srcTable, OracleTransaction transaction)
        {
            OracleConnection originalConnection = null;

            if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                originalConnection = DetailsAdapter.SelectCommand.Connection;
            else if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                originalConnection = DetailsAdapter.InsertCommand.Connection;
            else if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                originalConnection = DetailsAdapter.UpdateCommand.Connection;
            else if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                originalConnection = DetailsAdapter.DeleteCommand.Connection;



            OracleConnection connection = null;

            try
            {
                //				DetailsAdapter.SelectCommand.Connection=null;
                //				DetailsAdapter.InsertCommand.Connection=null;
                //				DetailsAdapter.UpdateCommand.Connection=null;
                //				DetailsAdapter.DeleteCommand.Connection=null;

                if (transaction != null)
                    connection = transaction.Connection;
                else
                    connection = GetOpenConnection(originalConnection.ConnectionString);

                for (int i = 0; i < orderOfSave.Length; i++)
                {
                    switch (orderOfSave[i])
                    {
                        case DataRowState.Added:
                            if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                                DetailsAdapter.SelectCommand.Connection = connection;
                            //							DetailsAdapter.SelectCommand.Transaction =transaction;
                            if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                            {
                                DetailsAdapter.InsertCommand.Connection = connection;
                                //								DetailsAdapter.InsertCommand.Transaction =transaction;
                                DataSet addedRows = dataSet.GetChanges(System.Data.DataRowState.Added);
                                if (addedRows != null)

                                    DetailsAdapter.Update(addedRows, srcTable);



                            }
                            break;
                        case DataRowState.Modified:
                            if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                            {
                                DetailsAdapter.UpdateCommand.Connection = connection;
                                //								DetailsAdapter.UpdateCommand.Transaction= transaction;
                                DataSet changedRows = dataSet.GetChanges(System.Data.DataRowState.Modified);
                                if (changedRows != null)
                                    DetailsAdapter.Update(changedRows, srcTable);


                            }
                            break;
                        case DataRowState.Deleted:
                            if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                            {
                                DetailsAdapter.DeleteCommand.Connection = connection;
                                //								DetailsAdapter.DeleteCommand.Transaction  = transaction;
                                DataSet deletedRows = dataSet.GetChanges(System.Data.DataRowState.Deleted);
                                if (deletedRows != null)

                                    DetailsAdapter.Update(deletedRows, srcTable);


                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //		CloseConnection(connection);

                if (null != DetailsAdapter.SelectCommand && null != DetailsAdapter.SelectCommand.Connection)
                    DetailsAdapter.SelectCommand.Connection = originalConnection;
                if (null != DetailsAdapter.InsertCommand && null != DetailsAdapter.InsertCommand.Connection)
                    DetailsAdapter.InsertCommand.Connection = originalConnection;
                if (null != DetailsAdapter.UpdateCommand && null != DetailsAdapter.UpdateCommand.Connection)
                    DetailsAdapter.UpdateCommand.Connection = originalConnection;
                if (null != DetailsAdapter.DeleteCommand && null != DetailsAdapter.DeleteCommand.Connection)
                    DetailsAdapter.DeleteCommand.Connection = originalConnection;
            }
        }

        #endregion

        public static OracleConnection GetOpenConnection(string connectionString)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void CloseConnection(OracleConnection connection)
        {
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
