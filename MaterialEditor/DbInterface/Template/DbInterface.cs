using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;
using System.Text;
using NLog;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Utility;
using System.Collections;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Controllers;
using Newtonsoft.Json.Linq;
//using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
//using CenturyLink.Network.Engineering.Material.Editor.Utility;


namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public class Option
    {
        public Option()
        {
        }
        public string text { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }

    public class GenericRec
    {
        public Hashtable Data = new Hashtable();
        public GenericRec()
        {
        }
        public GenericRec(IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var col = reader.GetName(i).ToUpper();
                var val = DataReaderHelper.GetNonNullValue(reader, col);
                Data[col] = val;
            }
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Data);
        }
    }
    public class DbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private const string pkg = "TMPLT_PKG";

        private IAccessor dbAccessor = null;

        public DbInterface()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public DbInterface(string dbConnectionString)
        {
            connectionString = dbConnectionString;
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
        // mwj: need to work on better generalizing these CreateParamsXxxxx methods...
        private IDbDataParameter[] CreateParamsWithRefCursor(IAccessor dbi, string parms, Hashtable fields)
        {
            var list = parms.Split(',');
            var len = list.Length;
            var p = dbi.GetParameterArray(len + 1);
            for (var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();
                if (parm.EndsWith("Id"))
                {
                    if (val == null || val.Length == 0 || val == "%")
                    {
                        val = null;
                    }
                    p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                }
                else
                {
                    p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                }
            }
            p[len] = dbi.GetParameterCursorType("retCsr", ParameterDirection.Output);

            return p;
        }

        private IDbDataParameter[] CreateStringParamsWithRefCursor(IAccessor dbi, string parms, Hashtable fields)
        {
            var list = parms.Split(',');
            var len = list.Length;
            var p = dbi.GetParameterArray(len + 1);
            for (var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();
                if (parm.EndsWith("Id") && ((val == null || val.Length == 0 || val == "%")))
                {
                    val = "-1";
                }
                p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
            }
            p[len] = dbi.GetParameterCursorType("retCsr", ParameterDirection.Output);

            return p;
        }
        private IDbDataParameter[] CreateParams(IAccessor dbi, string parms, Hashtable fields)
        {
            var list = parms.Split(',');
            var len = list.Length;
            var p = dbi.GetParameterArray(len);

            for (var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();
                if (parm.EndsWith("Id") || parm.EndsWith("Qty"))
                {
                    if (val == null || val.Length == 0)
                    {
                        val = null;
                    }
                    p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                }
                else
                {
                    p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                }
            }

            return p;
        }
        private IDbDataParameter[] CreateParamsWithReturnedId(IAccessor dbi, string parms, Hashtable fields, string retParamName)
        {
            var list = parms.Split(',');
            var len = list.Length;
            var p = dbi.GetParameterArray(len + 1);

            for (var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();
                if (parm.EndsWith("Id") || parm.EndsWith("Qty"))
                {
                    if (val == null || val.Length == 0)
                    {
                        val = null;
                    }
                    p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                }
                else
                {
                    p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                }
            }

            if (retParamName.EndsWith("Id"))
            {
                p[len] = dbi.GetParameter(retParamName, DbType.Int64, 0, ParameterDirection.Output);
            }
            else
            {
                p[len] = dbi.GetParameter(retParamName, DbType.String, "", ParameterDirection.Output);
            }

            return p;
        }
        private IDbDataParameter[] CreateParamsWithRefCursor(IAccessor dbi, Hashtable args)
        {
            var keys = new List<string>();
            foreach (var e in args.Keys)
            {
                keys.Add(e.ToString());
            }
            var parms = String.Join(",", keys);
            return CreateStringParamsWithRefCursor(dbi, parms, args);
        }

        private Option Copy(Option item, IDataReader reader)
        {
            item.text = DataReaderHelper.GetNonNullValue(reader, "TEXT");
            item.value = DataReaderHelper.GetNonNullValue(reader, "VALUE");
            item.description = DataReaderHelper.GetNonNullValue(reader, "DESCRIPTION");
            return item;
        }

        public void UPDATE_APP_LOG(string callSite, string cuid, string msg, string logLevel = "INFO")
        {
            var parms = "pCallSite,pMsg,pCUID,pLogLevel";
            var args = new Hashtable();
            args["pCallSite"] = callSite;
            args["pCUID"] = cuid;
            args["pMsg"] = (msg.Length > 4000 ? msg.Substring(0, 3999) : msg);
            args["pLogLevel"] = logLevel;

            ExecNonQueryStoredProc("UPDATE_APP_LOG", parms, args);

        }
        public void UPDATE_APP_LOG(string callSite, string cuid, Hashtable msg, string logLevel = "INFO")
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
            UPDATE_APP_LOG(callSite, cuid, json, logLevel);
        }

        public void UPDATE_AUDIT_LOG(Hashtable item)
        {
            throw new NotImplementedException("UpdateAuditLog - NOT IMPLEMENTED YET");
            //var args = "pCUID,pAction,pTmpltId,pTmpltDefId,pId,pItemDsc,pTblNm,pColNm,pOldVal,pNewVal";
            //var proc = "UPDATE_AUDIT_LOG";
            //var cntndInDefTyp = (item["pCntndInDefTyp"] ?? "").ToString().Trim().ToUpper();
            //ExecNonQueryStoredProc(proc, args, item);
        }
        public long UpdateTemplate(Hashtable rec)
        {
            //IAccessor dbm = null;
            //IDbDataParameter[] parameters = null;
            //IDataReader reader = null;

            var tmpltId = 0L;

            if (tmpltId == 0)
            {
                throw new NotImplementedException("UpdateTemplate - NOT IMPLEMENTED YET");
            }

            return tmpltId;
        }

        public async Task<long> CLONE_TMPLT(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;

            var err = "";

            var proc = pkg + ".CLONE_TMPLT";
            var parms = "pCuid,pSrcTmpltId,pTmpltNm,pTmpltDsc";
            var comnCnfgId = 0L;
            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = CreateParamsWithReturnedId(dbm, parms, data, "oTmpltId");

                    dbm.BeginTransaction();
                    dbm.ExecuteNonQuerySP(proc, parameters);
                    dbm.CommitTransaction();

                    comnCnfgId = long.Parse(parameters[parameters.Length - 1].Value.ToString());
                }
                catch (OracleException oe)
                {
                    err = "Unable to perform operation: " + proc;
                    logger.Error(oe, err);

                    // mwj: this blows up on my machine 
                    //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);

                    err = oe.Message.Contains("unique constraint")
                          ? "Could not update database due to unique contraint;\nplease check that the name of the entry is unique."
                          : oe.Message
                    ;

                    dbm.RollbackTransaction();
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    logger.Error(ex, "Unable to perform operation: " + proc);
                }
                finally
                {

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (err != "")
            {
                throw new Exception(err);
            }
            return comnCnfgId;
        }

        public List<Hashtable> ExecQueryStoredProc(string proc, string args, Hashtable query)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            Exception error = null;

            var list = new List<Hashtable>();

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            try
            {
                dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parms = CreateParamsWithRefCursor(dbm, args, query);

                reader = dbm.ExecuteDataReaderSP(proc, parms);
                while (reader.Read())
                {
                    var item = new GenericRec(reader);
                    list.Add(item.Data);
                }

            }
            catch (OracleException oe)
            {
                error = oe;
                logger.Error(oe, proc + ": oracle error");

                // mwj: this doesn't work on my machine...
                //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                error = ex;
                logger.Error(ex, proc + ": general error");
            }
            finally
            {
                if (dbm != null)
                    dbm.Dispose();
            }

            if (error != null)
            {
                throw error;
            }

            return list;
        }

        public List<Hashtable> ExecQueryStoredProc(string proc, Hashtable query)
        {
            var keys = new List<string>();
            foreach (var e in query.Keys)
            {
                keys.Add(e.ToString());
            }
            var parms = String.Join(",", keys);
            return ExecQueryStoredProc(proc, parms, query);
        }


        public void ExecNonQueryStoredProc(string proc, string args, Hashtable item)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            var err = "";

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            try
            {
                dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                parms = CreateParams(dbm, args, item);

                dbm.BeginTransaction();
                dbm.ExecuteNonQuerySP(proc, parms);
                dbm.CommitTransaction();
            }
            catch (OracleException oe)
            {
                err = proc + ": unable to perform update (db error)";

                logger.Error(oe, err);

                // mwj: this doesn't work on my machine...
                EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                err = proc + ": upable to perform update (general error)";
                logger.Error(ex, err);
            }
            finally
            {
                if (dbm != null)
                    dbm.Dispose();
            }

            if (err.Length > 0)
            {
                throw new Exception(err);
            }
        }


        public async Task<List<Hashtable>> GET_TMPLT_REFS(long tmpltId)
        {
            var parms = new Hashtable();
            parms["pTmpltId"] = tmpltId;

            var proc = "GET_TMPLT_REFS";

            var list = new List<Hashtable>();
            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(proc, parms);
            });
            return list;
        }

        public async Task<List<Hashtable>> GET_AUDIT_LOG(long tmpltId)
        {
            var parms = new Hashtable();
            parms["pTmpltId"] = tmpltId;

            var proc = "GET_AUDIT_LOG";

            var list = new List<Hashtable>();
            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(proc, parms);
            });
            return list;
        }

        public async Task<List<Hashtable>> GET_CONTAINMENT_RULES()
        {
            IAccessor dbi = null;
            IDbDataParameter[] args = null;
            IDataReader reader = null;
            List<Hashtable> list = new List<Hashtable>();
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbi = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    args = dbi.GetParameterArray(1);
                    args[0] = dbi.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbi.ExecuteDataReaderSP(pkg + ".GET_CONTAINMENT_RULES", args);
                    while (reader.Read())
                    {
                        var item = new GenericRec(reader);
                        list.Add(item.Data);
                    }
                }
                catch (OracleException oe)
                {
                    error = oe;

                    string message = "Unable to perform search";
                    logger.Error(oe, message);
                    EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    error = ex;

                    logger.Error(ex, "Unable to perform search");
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbi != null)
                        dbi.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }

        public async Task<List<Hashtable>> SEARCH_TMPLT(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;
            var proc = pkg + ".search_tmplt";
            var parms = "pTmpltNm,pTmpltTypId,pTmpltDsc,pBaseTmpltInd,pHlpTmpltInd,pComnCnfgTmpltInd,pCmpltInd,pPrpgtInd,pUpdtInPrgsInd,pRetTmpltInd,pDelInd,pTmpltId";

            var list = new List<Hashtable>();

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            await Task.Run(() =>
            {
                try
                {
                 
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = CreateParamsWithRefCursor(dbm, parms, data);
                    dbm.BeginTransaction();
                    reader = dbm.ExecuteDataReaderSP(proc, parameters);
                    //dbm.CommitTransaction();                   
                    
                    while (reader.Read())
                    {
                        var item = new GenericRec(reader);
                        list.Add(item.Data);
                    }

                }
                catch (OracleException oe)
                {
                    error = oe;
                    logger.Error(oe, proc + ": oracle error");

                    // mwj: this doesn't work on my machine...
                    //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    error = ex;
                    logger.Error(ex, proc + ": general error");
                }
                finally
                {
                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
            {
                throw error;
            }

            return list;
        }

        public async Task<long> INSERT_TMPLT(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;           
            Exception error = null;
            var templtId = 0L;
            var proc = pkg + ".insert_tmplt";
            var parms = "pTmpltNm,pTmpltTypId,pTmpltDsc,pBaseTmpltInd,pHlpTmpltInd,pComnCnfgTmpltInd,pCmpltInd,pPrpgtInd,pUpdtInPrgsInd,pRetTmpltInd,pDelInd";

            var list = new List<Hashtable>();

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            await Task.Run(() =>
            {
                try
                {

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = CreateParamsWithReturnedId(dbm, parms, data, "pTmpltId");
                    dbm.BeginTransaction();
                    dbm.ExecuteNonQuerySP(proc, parameters);
                    dbm.CommitTransaction();

                    templtId = long.Parse(parameters[parameters.Length - 1].Value.ToString());

                }
                catch (OracleException oe)
                {
                    error = oe;
                    logger.Error(oe, proc + ": oracle error");

                    // mwj: this doesn't work on my machine...
                    //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    error = ex;
                    logger.Error(ex, proc + ": general error");
                }
                finally
                {
                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
            {
                throw error;
            }

            return templtId;
        }

        public async Task<List<Option>> GET_OPTION_LIST(string optionType)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Option> list = new List<Option>(); // either it'll be empty or it won't be
            Exception error = null;
            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    // pID in varchar2, pNAME in varchar2, pDSC in varchar2, pSTATUS in varchar2, pTMPLT_TYPE in varchar2

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbm.GetParameterArray(2);
                    parameters[0] = dbm.GetParameter("pOptionType", DbType.String, optionType, ParameterDirection.Input);
                    parameters[1] = dbm.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbm.ExecuteDataReaderSP(pkg + ".GET_OPTION_LIST", parameters);

                    while (reader.Read())
                    {
                        var item = new Option();
                        list.Add(Copy(item, reader));
                    }
                }
                catch (OracleException oe)
                {
                    error = oe;

                    message = "Unable to perform search";
                    logger.Error(oe, message);
                    EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    error = ex;

                    message = "Unable to perform search";
                    logger.Error(ex, message);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }
    }
}