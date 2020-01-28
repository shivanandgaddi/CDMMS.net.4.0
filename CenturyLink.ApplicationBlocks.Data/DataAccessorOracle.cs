using System;
using System.Collections;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
//using CenturyLink.Network.Engineering.Common.Configuration;

namespace CenturyLink.ApplicationBlocks.Data
{
    /// <summary>
	/// Implementation class for Oracle, implemets IAccessor
	/// </summary>
    public class DataAccessorOracle : IAccessor
    {
#if DEBUG
        /// <summary>
        /// Keeps the current open connection
        /// </summary>
        internal static int current_count = 0;

        /// <summary>
        /// Keeps the total connection opened so far
        /// </summary>
        internal static int total_count = 0;

        /// <summary>
        /// Keeps the count of maximum connection opened at a time
        /// </summary>
        internal static int max_current_count = 0;

#endif

        #region Member Variables

        /// <summary>
        /// Oracle transaction variable
        /// </summary>
        private OracleTransaction _transaction = null;

        /// <summary>
        /// Oracle connection variable
        /// </summary>
        private OracleConnection _connection = null;

        /// <summary>
        /// Keeps the count of transaction. This helps for nested transactions.
        /// </summary>
        private int _transactionCount = 0;

        /// <summary>
        /// maximum tries allowed
        /// </summary>
        private int MAX_TRIES = 3;

        /// <summary>
        /// number of the trial
        /// </summary>
        private int iNoTries = 0;

        private int BLOB_BUFFER_SIZE = (1024 * 256);

        /// <summary>
        /// Connection string to oracle database
        /// </summary>
        private string sConnectionString = string.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Opens connection with the given connection string
        /// </summary>
        /// <param name="sConnection"></param>
        public DataAccessorOracle(string sConnection)
        {
            _connection = new OracleConnection(sConnection);

            this.sConnectionString = sConnection;
            //Connection would be opened in the prepare command method of the OracleHelper.
            //It's better to open connection late for better performance and efficient pooling.		
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets and Sets the _connection
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new OracleConnection(sConnectionString);
                }

                return _connection;
            }
        }

        /// <summary>
        /// Gets and Set the transaction
        /// </summary>
        public IDbTransaction Transaction
        {
            get
            {
                return _transaction;
            }
            //Client can set the value of transaction, or modify it.
            set
            {
                _transaction = (OracleTransaction)value;
            }
        }
        #endregion

        #region Methods

        /// <summary>		
        /// Opens the connection
        /// </summary>
        /// <param name="sConnection"></param>
        /// <returns></returns>
        private void OpenConnection()
        {
#if DEBUG
            current_count++;
            if (current_count > max_current_count)
                max_current_count = current_count;

            total_count++;
#endif

            Connection.Open();
        }

        /// <summary>
        /// Closes the connection, if not closed
        /// </summary>
        private void CloseConnection()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
#if DEBUG
                current_count--;
#endif
                Connection.Close();
                Connection.Dispose();
                _connection = null;

                if (_transaction != null)
                    _transaction.Dispose();
            }
        }

        /// <summary>
        /// Begins Transaction with default isolation level of ReadCommitted
        /// Does not support nested transaction
        /// </summary>
        public void BeginTransaction()
        {
            this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void SaveTransaction(string savePointName)
        {
            if (Transaction == null)
            {
                throw new Exception("Error: A Save Point cannot be defined without beginning a Transaction");
            }
            else
            {
                ((OracleTransaction)Transaction).Save(savePointName);
            }
        }

        /// <summary>
        /// Begins transaction with given isolation level
        /// Does not support nested transaction.
        /// </summary>
        /// <param name="iLevel"></param>
        public void BeginTransaction(IsolationLevel iLevel)
        {
            if (_transactionCount == 0)
            {
                _transactionCount++;

                if (Transaction == null)
                {
                    if (Connection.State != ConnectionState.Open)
                        this.OpenConnection();

                    Transaction = Connection.BeginTransaction(iLevel);
                }
            }
            else
            {
                throw new Exception("A Transaction has already been started.");
            }
        }

        /// <summary>
        /// This commits the transaction, and set the transaction object to null.
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                if (_transactionCount == 1)
                {
                    if (Transaction != null)
                    {
                        Transaction.Commit();
                        Transaction.Dispose();
                        Transaction = null;

                        //Only when transaction is committed, transaction count should be lowered
                        _transactionCount--;
                    }
                    else
                        throw new Exception("No transaction to commit.");
                }
                else
                {
                    throw new Exception("Nested transactions not supported.");
                }
            }
            finally
            {
                //Dispose the connection
                this.DisposeIfNoTransaction();
            }
        }

        /// <summary>
        /// This roll backs the transaction if transaction count is 1. Else throws exception
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                if (Transaction != null)
                {
                    // If nested transactions occurred in the same
                    // Accessor, the first catch block would RollbackTransaction
                    // and then throw the error. The next level catch would also
                    // try to RollbackTransaction throwing the error below in the process
                    if (_transactionCount == 1)
                    {
                        Transaction.Rollback();
                        Transaction.Dispose();
                        Transaction = null;
                        _transactionCount--;
                    }
                }
                else
                    throw new Exception("No transaction to rollback.");
            }
            finally
            {
                this.DisposeIfNoTransaction();
            }
        }

        /// <summary>
        /// This roll backs the transaction to a predefined Save Point if transaction count is 1. Else throws exception
        /// </summary>
        public void RollbackTransaction(string savePointName)
        {
            if (Transaction != null)
            {
                // If nested transactions occurred in the same
                // Accessor, the first catch block would RollbackTransaction
                // and then throw the error. The next level catch would also
                // try to RollbackTransaction throwing the error below in the process.
                ((OracleTransaction)Transaction).Rollback(savePointName);
            }
            else
            {
                throw new Exception("No transaction to rollback.");
            }
        }
        #endregion Methods


        #region Execute Methods	

        #region Scalar

        /// <summary>
        /// Uses Oracle Helper to perform Execute Scalar on the Connection object and Transaction.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType type, string commandText, IDbDataParameter[] parameters)
        {
            object returnObj = null;

            try
            {
                returnObj = OracleHelper.ExecuteScalar(type, commandText, this.Connection, parameters, this.Transaction);
            }
            catch (OracleException ex)
            {
                if (this.CheckODPError(ex, parameters))
                {
                    return ExecuteScalar(type, commandText, parameters);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                this.DisposeIfNoTransaction();
            }

            return returnObj;
        }

        /// <summary>
        /// Execute Scalar for parameterlist null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType type, string commandText)
        {
            return ExecuteScalar(type, commandText, null);
        }

        /// <summary>
        /// Execute Scalar for stored Procedure
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalarSP(string commandText, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(CommandType.StoredProcedure, commandText, parameters);
        }

        /// <summary>
        /// Execute Scalar for stored procedure
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public object ExecuteScalarSP(string commandText)
        {
            return ExecuteScalar(CommandType.StoredProcedure, commandText);
        }

        /// <summary>
        /// Execute Scalar for stored procedure given the Parameter Value object array
        /// </summary>
        /// <param name="procName">Stored Procedure Name</param>
        /// <param name="values">Parameter Value object array</param>
        /// <returns></returns>
        public object ExecuteScalarSP(string procName, object[] values)
        {
            IDbDataParameter[] oParams = OracleParameterCache.GetCachedParameterSet(this.Connection.ConnectionString, procName, this);
            IList outputParamsIndex = this.AssignParameterValues(oParams, values);
            object retObj = ExecuteScalar(CommandType.StoredProcedure, procName, oParams);

            AssignOutputParams(oParams, values, outputParamsIndex);

            return retObj;
        }
        #endregion Scalar

        #region NonQuery

        /// <summary>
        /// Performs Execute Non Query through OracleHelper class.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType type, string commandText, IDbDataParameter[] parameters)
        {
            int iReturn = 0;

            try
            {
                iReturn = OracleHelper.ExecuteNonQuery(type, commandText, Connection, parameters, Transaction);
            }
            catch (OracleException ex)
            {
                if (this.CheckODPError(ex, parameters))
                {
                    return ExecuteNonQuery(type, commandText, parameters);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                this.DisposeIfNoTransaction();
            }

            return iReturn;
        }

        /// <summary>
        /// ExecuteNonQuery for parameterlist null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType type, string commandText)
        {
            return ExecuteNonQuery(type, commandText, null);
        }

        /// <summary>
        /// Execute NonQuery for Stored Proc
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuerySP(string procName, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, procName, parameters);
        }

        /// <summary>
        /// ExecuteNonQuery for stored proc and parameterlist null
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public int ExecuteNonQuerySP(string procName)
        {
            return ExecuteNonQuery(CommandType.StoredProcedure, procName);
        }

        /// <summary>
        /// Performs Execute Non Query through OracleHelper class.
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="ParameterNames"></param>
        /// <param name="ParameterCollection"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, ArrayList ParameterNames, ArrayList ParameterCollection)
        {
            int iReturn = 0;

            //create a command and prepare it for execution
            try
            {
                iReturn = OracleHelper.ExecuteNonQuery(cmdType, cmdText, Connection, ParameterNames, ParameterCollection, Transaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.DisposeIfNoTransaction();
            }

            return iReturn;
        }
        /// <summary>
        /// Execute Non Query for stored procedure given the Parameter Value object array
        /// </summary>
        /// <param name="procName">Stored Procedure Name</param>
        /// <param name="values">Parameter Value object array</param>
        /// <returns></returns>
        public int ExecuteNonQuerySP(string procName, object[] values)
        {
            IDbDataParameter[] oParams = OracleParameterCache.GetCachedParameterSet(this.Connection.ConnectionString, procName, this);
            IList outputParamsIndex = this.AssignParameterValues(oParams, values);
            int retVal = ExecuteNonQuery(CommandType.StoredProcedure, procName, oParams);

            AssignOutputParams(oParams, values, outputParamsIndex);

            return retVal;
        }
        #endregion NonQuery

        #region DataSet

        /// <summary>
        /// Get Dataset for command type, text and parameter list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(CommandType type, string commandText, IDbDataParameter[] parameters)
        {
            return this.ExecuteDataSet(null, new DataSet(), type, commandText, parameters);
        }

        /// <summary>
        /// Get Dataset for parameter null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(CommandType type, string commandText)
        {
            return ExecuteDataSet(type, commandText, null);
        }

        /// <summary>
        /// Get Dataset for Stored Procedure
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSetSP(string procName, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(CommandType.StoredProcedure, procName, parameters);
        }

        /// <summary>
        /// Get Dataset for Stored Procedure and parameter null
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSetSP(string commandText)
        {
            return ExecuteDataSet(CommandType.StoredProcedure, commandText, null);
        }

        /// <summary>
        /// Get DataSet for stored procedure given the Parameter Value object array
        /// </summary>
        /// <param name="procName">Stored Procedure Name</param>
        /// <param name="values">Parameter Value object array</param>
        /// <returns></returns>
        public DataSet ExecuteDataSetSP(string procName, object[] values)
        {
            IDbDataParameter[] oParams = OracleParameterCache.GetCachedParameterSet(this.Connection.ConnectionString, procName, this);
            IList outputParamsIndex = this.AssignParameterValues(oParams, values);
            DataSet retDataSet = ExecuteDataSet(CommandType.StoredProcedure, procName, oParams);

            AssignOutputParams(oParams, values, outputParamsIndex);

            return retDataSet;
        }


        /// <summary>
        /// Returns a database for a given tablename and dataset reference
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string tableName, DataSet ds, CommandType type, string commandText, IDbDataParameter[] parameters)
        {
            try
            {
                OracleHelper.ExecuteDataset(tableName, ds, type, commandText, Connection, parameters, Transaction);
            }
            catch (OracleException ex)
            {
                if (this.CheckODPError(ex, parameters))
                {
                    return ExecuteDataSet(type, commandText, parameters);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                this.DisposeIfNoTransaction();
            }

            return ds;
        }

        /// <summary>
        /// Passes parameterList value as null
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string tableName, DataSet ds, CommandType type, string commandText)
        {
            return ExecuteDataSet(tableName, ds, type, commandText, null);
        }
        #endregion DataSet

        #region OracleDataReader
        /// <summary>
        ///  Get data reader for command type, text and parameter list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public IDataReader ExecuteDataReader(CommandType type, string commandText, IDbDataParameter[] parameters, out object cmd)
        {
            OracleDataReader dr = null;

            try
            {
                dr = OracleHelper.ExecuteDataReader(type, commandText, Connection, parameters, Transaction, out cmd);
            }
            catch (OracleException ex)
            {
                if (this.CheckODPError(ex, parameters))
                {
                    return ExecuteDataReader(type, commandText, parameters, out cmd);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                //There is no need to dispose here. It needs to be disposed by the client after it as finished reading.
            }

            return dr;
        }
        /// <summary>
        /// Get Dataset for command type, text and parameter list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteDataReader(CommandType type, string commandText, IDbDataParameter[] parameters)
        {
            OracleDataReader dr = null;

            try
            {
                dr = OracleHelper.ExecuteDataReader(type, commandText, Connection, parameters, Transaction);
            }
            catch (OracleException ex)
            {
                if (this.CheckODPError(ex, parameters))
                {
                    return ExecuteDataReader(type, commandText, parameters);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                //There is no need to dispose here. It needs to be disposed by the client after it as finished reading.
            }

            return dr;
        }

        /// <summary>
        /// Get Dataset for parameter null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public IDataReader ExecuteDataReader(CommandType type, string commandText)
        {
            return ExecuteDataReader(type, commandText, null);
        }

        /// <summary>
        /// Get Dataset for Stored Procedure
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteDataReaderSP(string procName, IDbDataParameter[] parameters)
        {
            return ExecuteDataReader(CommandType.StoredProcedure, procName, parameters);
        }

        /// <summary>
        /// Get Dataset for Stored Procedure and parameter null
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public IDataReader ExecuteDataReaderSP(string commandText)
        {
            return ExecuteDataReader(CommandType.StoredProcedure, commandText, null);
        }

        /// <summary>
        /// Get DataReader for stored procedure given the Parameter Value object array
        /// </summary>
        /// <param name="procName">Stored Procedure Name</param>
        /// <param name="values">Parameter Value object array</param>
        /// <returns></returns>
        public IDataReader ExecuteDataReaderSP(string procName, object[] values)
        {
            IDbDataParameter[] oParams = OracleParameterCache.GetCachedParameterSet(this.Connection.ConnectionString, procName, this);
            IList outputParamsIndex = this.AssignParameterValues(oParams, values);
            IDataReader retReader = ExecuteDataReader(CommandType.StoredProcedure, procName, oParams);

            AssignOutputParams(oParams, values, outputParamsIndex);

            return retReader;
        }

        #endregion OracleDataReader

        #endregion Execute Methods

        #region ODP Error Code

        /// <summary>
        /// Checks if ORA-03113 error, tries for MAX_TRIES times else throw exception
        /// </summary>
        /// <param name="ex"></param>
        private bool CheckODPError(OracleException ex, IDbDataParameter[] parameters)
        {
            bool isError = false;

            string sCheckODP = System.Configuration.ConfigurationManager.AppSettings["CheckODP.NET"];

            if (sCheckODP != null && sCheckODP.ToUpper().Equals("TRUE"))
            {
                //ORA-03113: end-of-file on communication channel error 
                if (ex.Number == 3113)
                {
                    //Try only MAX_TRIES times
                    if (iNoTries <= MAX_TRIES)
                    {
                        //increment the try count
                        iNoTries++;

                        //Close the stale connection
                        Connection.Close();

                        //instantiate a new Oracle connection
                        //If you get the Connection string from the Connection Variable, password would be null
                        _connection = new OracleConnection(this.sConnectionString);

                        //Same instance of parameters could not be assigned to different commands
                        //So obtain a new instance of parameter list						
                        if (parameters != null && parameters.Length != 0)
                        {
                            int iCount = 0;

                            foreach (IDbDataParameter aParam in parameters)
                            {
                                //Get the clone version of old Oracle Parameter
                                OracleParameter newParam = (OracleParameter)((OracleParameter)aParam).Clone();

                                //Replace the old instance of parameter with new instance
                                parameters[iCount] = newParam;

                                iCount++;
                            }
                        }

                        //Run the same command again
                        isError = true;
                    }
                }
            }

            return isError;
        }

        #endregion

        #region Get Parameter Methods

        /// <summary>
        /// Returns OracleParameter array of the required size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public IDbDataParameter[] GetParameterArray(int size)
        {
            return new OracleParameter[size];
        }

        /// <summary>
        /// Returns an Oracle Parameter
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter GetParameter()
        {
            return new OracleParameter();
        }

        /// <summary>
        /// Returns an Oracle Parameter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IDbDataParameter GetParameter(string paramName, DbType type, Object val, ParameterDirection direction)
        {
            OracleParameter aParam = new OracleParameter();

            aParam.ParameterName = paramName;
            aParam.DbType = type;
            aParam.Value = val;
            aParam.Direction = direction;

            return aParam;
        }

        /// <summary>
        /// Returns an Oracle Parameter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IDbDataParameter GetParameter(string paramName, DbType type, Object val, ParameterDirection direction, Int32 size)
        {
            OracleParameter aParam = new OracleParameter();

            aParam.ParameterName = paramName;
            aParam.DbType = type;
            aParam.Value = val;
            aParam.Direction = direction;
            aParam.Size = size;

            return aParam;
        }

        /// <summary>
        /// Returns an Oracle Parameter of Cursor type
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter GetParameterCursorType()
        {
            OracleParameter aParam = new OracleParameter();

            aParam.OracleDbType = OracleDbType.RefCursor;

            return aParam;
        }

        /// <summary>
        /// Returns an Oracle Parameter of Cursor type
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter GetParameterCursorType(string paramName, ParameterDirection direction)
        {
            OracleParameter aParam = new OracleParameter();

            aParam.OracleDbType = OracleDbType.RefCursor;
            aParam.ParameterName = paramName;
            aParam.Direction = direction;

            return aParam;
        }

        /// <summary>
        /// Get parameter of blob type
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter GetParameterBlobType()
        {
            OracleParameter aParam = new OracleParameter();

            aParam.OracleDbType = OracleDbType.Blob;

            return aParam;
        }

        public IDbDataParameter GetParameterClobType(string paramName, string clobValue, ParameterDirection direction)
        {
            if (_connection.State != ConnectionState.Open)
                OpenConnection();

            OracleParameter aParam = new OracleParameter();
            OracleClob clob = new OracleClob(_connection, false, false);
            byte[] bytes = new byte[clobValue.Length * sizeof(char)];

            aParam.OracleDbType = OracleDbType.Clob;
            aParam.Direction = direction;
            aParam.ParameterName = paramName;
            aParam.Value = clobValue;

            System.Buffer.BlockCopy(clobValue.ToCharArray(), 0, bytes, 0, bytes.Length);

            clob.BeginChunkWrite();
            clob.Write(bytes, 0, bytes.Length);
            clob.EndChunkWrite();

            return aParam;
        }

        /*public IDbDataParameter GetParameterBlobType(string blobValue)
        {
            
            OpenConnection();

            IDbDataParameter aParam = GetParameterBlobType();
            //OracleBlob blob = new OracleBlob(_connection, false);
            OracleClob blob = new OracleClob(_connection, false, false);

            ((OracleParameter)aParam).OracleDbType = OracleDbType.Clob;

            int bytesRead = 0;
            byte[] buffer = new byte[10];
            byte[] blobBuffer = new byte[10];
            int blobIdx = 0;
            long totalSize = 0;
            //System.IO.StringReader sr = new System.IO.StringReader(blobValue);
            System.IO.Stream msg = null; 
            
            blob.BeginChunkWrite();

            byte[] bytes = new byte[blobValue.Length * sizeof(char)];

            System.Buffer.BlockCopy(blobValue.ToCharArray(), 0, bytes, 0, bytes.Length);

            //blob.Write(bytes, 0, bytes.Length);

            do
            {
                bytesRead = msg.Read(blobBuffer, blobIdx, blobBuffer.Length - blobIdx);
                blobIdx += bytesRead;
                totalSize += bytesRead;
                if (blobIdx >= (10))
                {
                    blob.Write(blobBuffer, 0, blobIdx);
                    blobIdx = 0;
                }
                if (bytesRead == 0)
                {
                    if (blobIdx > 0)
                    {
                        blob.Write(blobBuffer, 0, blobIdx);
                    }
                    break;
                }
            } while (bytesRead > 0);

            //msg.DataStream.Close();

            blob.EndChunkWrite();

            return aParam;
        }*/

        /// <summary>
        /// used to convert blob datatype to byte
        /// </summary>
        /// <param name="blob">blob data</param>
        /// <returns>byte array</returns>
        public byte[] ConvertBlobTypeToByte(object blob)
        {
            return (byte[])((Oracle.ManagedDataAccess.Types.OracleBlob)(blob)).Value;
        }
        #endregion

        #region GetCachedParameters

        public IDbDataParameter[] GetParameterArrayForSP(string procName)
        {
            string[] splitPackageProcedureName = procName.Split(".".ToCharArray());
            //string databaseOwner = System.Configuration.ConfigurationManager.AppSettings["DatabaseOwner"];
            string databaseOwner = "CDMMS_OWNER"; //TODO JOE Get rid of hard coding without making a goofy circular reference
            string sqlQuery;
            DataSet dsParams = null;

            if (splitPackageProcedureName.Length < 2)
            {
                sqlQuery = @"SELECT a.argument_name, a.data_type, a.in_out, NVL(a.data_length,0) AS data_length, NVL(a.data_precision,0) AS data_precision, NVL(a.data_scale,0) AS data_scale 
							FROM ALL_ARGUMENTS a, all_objects o 
							WHERE o.object_id = (SELECT object_id FROM all_objects WHERE UPPER(object_name) = UPPER('" + procName + @"') 
                                                AND object_type='PROCEDURE' and owner ='" + databaseOwner + @"') 
							AND a.OBJECT_ID = o.OBJECT_ID 
							ORDER BY a.position 
							";
            }
            else
            {
                sqlQuery = @"SELECT a.argument_name, a.data_type, a.in_out, NVL(a.data_length,0) AS data_length, NVL(a.data_precision,0) AS data_precision, NVL(a.data_scale,0) AS data_scale 
							FROM ALL_ARGUMENTS a, all_objects o 
							WHERE o.object_id = (SELECT object_id FROM all_objects WHERE UPPER(object_name) = UPPER('" + splitPackageProcedureName[0] + @"') AND object_type='PACKAGE' 
                                                 and owner ='" + databaseOwner + @"') 
							AND UPPER(a.object_name) = UPPER('" + splitPackageProcedureName[1] + @"') 
							AND a.OBJECT_ID = o.OBJECT_ID 
							ORDER BY a.position asc 
							";
            }

            dsParams = this.ExecuteDataSet(CommandType.Text, sqlQuery);

            if (dsParams == null)
            {
                return null;
            }
            else
            {
                int totalParams = dsParams.Tables[0].Rows.Count;

                if (totalParams == 0)
                {
                    //SP has no parameters
                    return null;
                }
                else
                {
                    //create, initialise and return an IdbDataParameter array
                    IDbDataParameter[] oParams = this.GetParameterArray(totalParams);
                    DataTable dt = dsParams.Tables[0];

                    for (int i = 0; i < totalParams; i++)
                    {
                        if (dt.Rows[i]["DATA_TYPE"].ToString().ToUpper().Equals("REF CURSOR"))
                        {
                            oParams[i] = this.GetParameterCursorType();
                        }
                        else
                        {
                            oParams[i] = this.GetParameter();
                        }

                        oParams[i].ParameterName = dt.Rows[i]["ARGUMENT_NAME"].ToString();

                        if (dt.Rows[i]["IN_OUT"].ToString().ToUpper().Equals("IN"))
                        {
                            oParams[i].Direction = ParameterDirection.Input;
                        }
                        else if (dt.Rows[i]["IN_OUT"].ToString().ToUpper().Equals("OUT"))
                        {
                            oParams[i].Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            oParams[i].Direction = ParameterDirection.InputOutput;
                        }

                        //determine DbType
                        switch (dt.Rows[i]["DATA_TYPE"].ToString().ToUpper())
                        {
                            case "NUMBER":
                                oParams[i].DbType = DbType.Decimal;

                                if (Int32.Parse(dt.Rows[i]["DATA_PRECISION"].ToString()) != 0)
                                {
                                    oParams[i].Scale = Byte.Parse(dt.Rows[i]["DATA_PRECISION"].ToString());
                                }

                                if (Int32.Parse(dt.Rows[i]["DATA_SCALE"].ToString()) != 0)
                                {
                                    oParams[i].Scale = Byte.Parse(dt.Rows[i]["DATA_SCALE"].ToString());
                                }

                                break;
                            case "VARCHAR2":
                            //								oParams[i].DbType = DbType.String;
                            //								if (Int32.Parse(dt.Rows[i]["DATA_LENGTH"].ToString()) != 0) {
                            //									oParams[i].Size = Int32.Parse(dt.Rows[i]["DATA_LENGTH"].ToString());
                            //								}
                            //								else if(oParams[i].Direction == ParameterDirection.Output || oParams[i].Direction == ParameterDirection.InputOutput)
                            //								{
                            //									oParams[i].Size = 4000;
                            //								}
                            //								break;
                            case "NVARCHAR2":
                                oParams[i].DbType = DbType.String;

                                if (Int32.Parse(dt.Rows[i]["DATA_LENGTH"].ToString()) != 0)
                                {
                                    oParams[i].Size = Int32.Parse(dt.Rows[i]["DATA_LENGTH"].ToString());
                                }
                                else if (oParams[i].Direction == ParameterDirection.Output || oParams[i].Direction == ParameterDirection.InputOutput)
                                {
                                    //no size for the output varchar2 is specified in the oracle system tables - so the max possible value is assigned.
                                    oParams[i].Size = 4000;
                                }

                                break;
                            case "DATE":
                                oParams[i].DbType = DbType.Date;
                                break;
                            case "TIMESTAMP":
                                oParams[i].DbType = DbType.DateTime;
                                break;
                            case "RAW":
                                oParams[i].DbType = DbType.Binary;
                                break;
                            case "BLOB":
                                oParams[i].DbType = DbType.Binary;
                                break;
                            default:
                                break;
                        }
                    }
                    return oParams;
                }
            }
        }

        #endregion

        #region private functions

        /// <summary>
        /// If transaction has not started dispose the connection
        /// </summary>
        private void DisposeIfNoTransaction()
        {
            if (this._transactionCount == 0)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Assign values to the parameter. If value is null, assign it as DbNull.
        /// If parameter length not matches with value length throw Arguement Exception.
        /// </summary>
        /// <param name="oParams"></param>
        /// <param name="values"></param>
        private IList AssignParameterValues(IDbDataParameter[] oParams, object[] values)
        {
            ArrayList outputParamsIndex = new ArrayList();

            if (oParams != null && values != null)
            {
                if (oParams.Length != values.Length)
                {
                    throw new ArgumentException("Parameter count does not match Parameter Value count.");
                }

                for (int i = 0; i < oParams.Length; i++)
                {
                    if ((oParams[i].Direction == ParameterDirection.Output || oParams[i].Direction == ParameterDirection.InputOutput) && !(oParams[i] is OracleParameter && ((OracleParameter)oParams[i]).OracleDbType == OracleDbType.RefCursor))
                    {
                        outputParamsIndex.Add(i);
                    }

                    if (values[i] == null)
                    {
                        oParams[i].Value = DBNull.Value;
                    }
                    else
                    {
                        oParams[i].Value = values[i];
                    }
                }
            }

            return outputParamsIndex;
        }

        /// <summary>
        /// Assigns values of output parameters returned by the stored procedure to the value array. outputParamsIndex
        /// an ArrayList which contains the indexes of all output parameters in IDataParameter[].
        /// </summary>
        /// <param name="oParams"></param>
        /// <param name="values"></param>
        /// <param name="outputParamsIndex"></param>
        private void AssignOutputParams(IDataParameter[] oParams, object[] values, IList outputParamsIndex)
        {
            foreach (int index in outputParamsIndex)
            {
                values[index] = oParams[index].Value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Closes the connection. GC.SuppressFinalize() is called to prevent 
        /// the garbage collector from calling Object.Finalize on this object
        /// </summary>
        public virtual void Dispose()
        {
            CloseConnection();
            //GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Helper class to cache Parameter List keyed on Connection String and Stored Procedure Name
    /// </summary>
    public sealed class OracleParameterCache
    {
        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new OracleParameterCache()"
        private OracleParameterCache()
        {
        }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Static method to get the parameter array for a given stored procedure. First the cache is checked.
        /// If a miss is encountered, the parameter array is obtained from the database and inserted into the cache.
        /// </summary>
        /// <param name="connectStr"></param>
        /// <param name="procName"></param>
        /// <param name="oracleAccessor"></param>
        /// <returns></returns>
        public static IDbDataParameter[] GetCachedParameterSet(string connectStr, string procName, IAccessor oracleAccessor)
        {
            string hashKey = connectStr + ":" + procName;
            IDbDataParameter[] oParams;

            oParams = paramCache[hashKey] as IDbDataParameter[];

            if (oParams == null)
            {
                oParams = oracleAccessor.GetParameterArrayForSP(procName);
                paramCache[hashKey] = oParams;
            }

            if (oParams == null)
            {
                throw new Exception("No Parameters are present for the Package/Procedure. Please check whether the package/Procedure is installed properly or not. ");
            }

            return CloneParameters(oParams, oracleAccessor);
        }

        /// <summary>
        /// Returns a cloned parameter array 
        /// </summary>
        /// <param name="origParams"></param>
        /// <param name="oracleAccessor"></param>
        /// <returns></returns>
        public static IDbDataParameter[] CloneParameters(IDbDataParameter[] origParams, IAccessor oracleAccessor)
        {
            IDbDataParameter[] cloneParams = oracleAccessor.GetParameterArray(origParams.Length);

            for (int i = 0, j = origParams.Length; i < j; i++)
            {
                cloneParams[i] = (IDbDataParameter)((ICloneable)origParams[i]).Clone();
            }

            return cloneParams;
        }
    }
}
