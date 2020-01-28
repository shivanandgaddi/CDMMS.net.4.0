using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.ApplicationBlocks.Data
{
    public interface IAccessor : IDisposable
    {
        #region Properties
        /// <summary>
        /// Get property of Connection
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Get and Set property for Transaction
        /// </summary>
        IDbTransaction Transaction { get; set; }
        #endregion

        #region Transaction methods
        /// <summary>
        /// Begin Transaction with some default isolation level
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commit Transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Defines a Save Point for the transaction with given name
        /// </summary>
        /// <param name="savePointName"></param>
        void SaveTransaction(string savePointName);

        /// <summary>
        /// Rollback Transaction
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Rolls back transaction to the specified SavePoint
        /// </summary>
        /// <param name="savePointName"></param>
        void RollbackTransaction(string savePointName);

        /// <summary>
        /// Begin Transaction with given isolation level
        /// </summary>
        /// <param name="iLevel"></param>
        void BeginTransaction(IsolationLevel iLevel);
        #endregion

        #region Execute Scalar methods

        /// <summary>
        /// Execute Scalar for given Command type, text and parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(CommandType type, string commandText, IDbDataParameter[] parameters);

        /// <summary>
        /// Execute scalar for no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        object ExecuteScalar(CommandType type, string commandText);

        /// <summary>
        /// Execute scalar for store procedure
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalarSP(string commandText, IDbDataParameter[] parameters);

        /// <summary>
        /// Execute scalar for store proc and no parameters
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        object ExecuteScalarSP(string commandText);

        /// <summary>
        /// Execute Scalar for stored proc and given parameter values 
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        /// <param name="values">ArrayList containing values for the IN parameters and NULL for OUT parameters</param>
        /// <returns></returns>
        object ExecuteScalarSP(string procName, object[] values);

        #endregion


        #region Execute Non Query methods

        /// <summary>
        /// Execute Non Query for command type, text and parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteNonQuery(CommandType type, string commandText, IDbDataParameter[] parameters);
        /// <summary>
        /// Execute Non Query for command type, text and parameter names and values
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterCollection"></param>
        /// <returns></returns>
        int ExecuteNonQuery(CommandType type, string commandText, ArrayList parameterName, ArrayList parameterCollection);
        /// <summary>
        /// Execute Non Query for no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        int ExecuteNonQuery(CommandType type, string commandText);

        /// <summary>
        /// Execute Non Query for store proc
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteNonQuerySP(string procName, IDbDataParameter[] parameters);

        /// <summary>
        /// Execute Non Query for store proc and no parameters
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        int ExecuteNonQuerySP(string procName);

        /// <summary>
        /// Execute Non Query for stored proc and given parameter values 
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        /// <param name="values">ArrayList containing values for the IN parameters and NULL for OUT parameters</param>
        /// <returns></returns>
        int ExecuteNonQuerySP(string procName, object[] values);

        #endregion

        #region Execute Dataset methods

        /// <summary>
        /// Returns dataset for command type, text and parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataSet ExecuteDataSet(CommandType type, string commandText, IDbDataParameter[] parameters);

        /// <summary>
        /// Returns dataset for no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        DataSet ExecuteDataSet(CommandType type, string commandText);

        /// <summary>
        /// Returns dataset for store proc
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataSet ExecuteDataSetSP(string procName, IDbDataParameter[] parameters);

        /// <summary>
        /// Returns DataSet for stored proc and given parameter values 
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        /// <param name="values">ArrayList containing values for the IN parameters and NULL for OUT parameters</param>
        /// <returns></returns>
        DataSet ExecuteDataSetSP(string procName, object[] values);

        /// <summary>
        /// Returns Dataset for store proc and no parameters
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        DataSet ExecuteDataSetSP(string commandText);
        #endregion

        #region Execute Datareader methods

        /// <summary>
        /// Returns data reader for command type, text and parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader ExecuteDataReader(CommandType type, string commandText, IDbDataParameter[] parameters);

        /// <summary>
        /// Returns data reader for no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        IDataReader ExecuteDataReader(CommandType type, string commandText);

        /// <summary>
        /// Returns data reader for command type, text , parameters and cmd 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        IDataReader ExecuteDataReader(CommandType type, string commandText, IDbDataParameter[] parameters, out object cmd);

        /// <summary>
        /// Returns data reader for store proc
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader ExecuteDataReaderSP(string procName, IDbDataParameter[] parameters);

        /// <summary>
        /// Returns data reader for store proc and no params
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        IDataReader ExecuteDataReaderSP(string procName);

        /// <summary>
        /// Execute DataReader for stored proc and given parameter values 
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        /// <param name="values">ArrayList containing values for the IN parameters and NULL for OUT parameters</param>
        /// <returns></returns>
        IDataReader ExecuteDataReaderSP(string procName, object[] values);
        #endregion

        #region Parameter methods

        /// <summary>
        /// Get Parameter array of required size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        IDbDataParameter[] GetParameterArray(int size);

        /// <summary>
        /// Get Parameter array for given Stored Procedure
        /// </summary>
        /// <param name="procName">Stored Procedure Name</param>
        /// <returns></returns>
        IDbDataParameter[] GetParameterArrayForSP(string procName);

        /// <summary>
        /// Get Parameter
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameter();

        /// <summary>
        /// Get Parameter
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameter(string paramName, DbType type, Object val, ParameterDirection direction);

        /// <summary>
        /// Get Parameter
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameter(string paramName, DbType type, Object val, ParameterDirection direction, Int32 size);

        /// <summary>
        /// Get parameter of cursor type
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameterCursorType();

        /// <summary>
        /// Get parameter of cursor type
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameterCursorType(string paramName, ParameterDirection direction);

        /// <summary>
        /// Get parameter of blob type
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameterBlobType();

        /// <summary>
        /// Get parameter of clob type
        /// </summary>
        /// <returns></returns>
        IDbDataParameter GetParameterClobType(string paramName, string clobValue, ParameterDirection direction);

        /// <summary>
        /// Converts blob type to byte
        /// </summary>
        /// <returns></returns>
        byte[] ConvertBlobTypeToByte(object blob);
        #endregion
    }
}
