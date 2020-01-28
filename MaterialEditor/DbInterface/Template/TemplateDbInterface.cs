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
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;

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

    public class GenericResponse : GenericRec
    {
        public GenericResponse()
            : base()
        {
            EC = 0;
            MSG = "SUCCESS";
            OV = "";

            Data["EC"] = EC;
            Data["MSG"] = MSG;
            Data["OV"] = OV;
        }
        public GenericResponse(int ec, string msg = "", string ov = "")
            : base()
        {
            EC = ec;
            MSG = msg;
            OV = ov;

            Data["EC"] = EC;
            Data["MSG"] = MSG;
            Data["OV"] = OV;
        }

        public GenericResponse(IDataReader reader)
            : base(reader)
        {
            var ec = 0;
            if( Int32.TryParse((Data["EC"] ?? "0").ToString(), out ec) )
            {
                EC = ec;
            }
            MSG = (Data["MSG"] ?? "").ToString();
            OV = (Data["OV"] ?? "").ToString();
        }
        public int EC { get; set;  }
        public string MSG { get; set;  }
        public string OV { get; set;  }
    }

    public class TemplateDbInterface : TemplateDbInterfaceImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string pkg = "TMPLT_PKG";

        public TemplateDbInterface() : base()
        {
        }

        public TemplateDbInterface(string dbConnectionString) : base(dbConnectionString)
        {
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
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
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
                          : oe.Message;

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

        public async Task<List<Hashtable>> GET_TMPLT_REFS(long tmpltId)
        {
            var parms = new Hashtable();            
            var proc = "GET_TMPLT_REFS";
            var list = new List<Hashtable>();

            parms["pTmpltId"] = tmpltId;

            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(pkg, proc, parms);
            });

            return list;
        }

        public async Task<List<Hashtable>> GET_AUDIT_LOG(long tmpltId)
        {
            var parms = new Hashtable();            
            var proc = "GET_AUDIT_LOG";
            var list = new List<Hashtable>();

            parms["pTmpltId"] = tmpltId;

            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(pkg, proc, parms);
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
                    dbi = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
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
                 
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = CreateParamsWithRefCursor(dbm, parms, data);
                    
                    reader = dbm.ExecuteDataReaderSP(proc, parameters);                  
                    
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

        public async Task<List<Hashtable>> SEARCH_HLP_MNR_MTL(Hashtable data)
        {            
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            //IDbDataParameter[] Parameter = null;
            IDataReader reader = null;
            Exception error = null;
            var proc = "MATERIAL_PKG.SEARCH_MATERIAL_ALL_DEAD";
           // var parms = "pTmpltNm,pTmpltTypId,pTmpltDsc,pBaseTmpltInd,pHlpTmpltInd,pComnCnfgTmpltInd,pCmpltInd,pPrpgtInd,pUpdtInPrgsInd,pRetTmpltInd,pDelInd,pTmpltId";
           
            //var parms = "pPrdId,pPrtNo,pMtlDesc,pMFG,pcdmmsid,pItmSts,pStatus,pFtrTyp,pSpecNm,pMtlCtgry,pCblTyp,pLstDt,pLstCuid,pHeciClei,pHasHeciClei,pStandaloneClei";

            var list = new List<Hashtable>();

            //if (proc.StartsWith(pkg) == false)
            //{
            //    proc = pkg + "." + proc;
            //}

            await Task.Run(() =>
            {
                try
                {

                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    //Parameter = CreateParamsWithRefCursor(dbm, parms, data);

                    parameters = dbManager.GetParameterArray(17);

                    parameters[0] = dbManager.GetParameter("pPrdId", DbType.String, data["pPrdId"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameter("pPrtNo", DbType.String, data["pPrtNo"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[2] = dbManager.GetParameter("pMtlDesc", DbType.String, data["pMtlDesc"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[3] = dbManager.GetParameter("pMFG", DbType.String, data["pMFG"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[4] = dbManager.GetParameter("pcdmmsid", DbType.String, "-99".ToUpper(), ParameterDirection.Input);
                    parameters[5] = dbManager.GetParameter("pItmSts", DbType.String, data["pItmSts"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[6] = dbManager.GetParameter("pStatus", DbType.String, data["pStatus"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[7] = dbManager.GetParameter("pFtrTyp", DbType.String, data["pFtrTyp"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[8] = dbManager.GetParameter("pSpecNm", DbType.String, data["pSpecNm"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[9] = dbManager.GetParameter("pMtlCtgry", DbType.String, data["pMtlCtgry"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[10] = dbManager.GetParameter("pCblTyp", DbType.String, data["pCblTyp"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[11] = dbManager.GetParameter("pLstDt", DbType.String, data["pLstDt"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[12] = dbManager.GetParameter("pLstCuid", DbType.String, data["pLstCuid"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[13] = dbManager.GetParameter("pHeciClei", DbType.String, data["pHasHeciClei"].ToString().ToUpper(), ParameterDirection.Input);
                    parameters[14] = dbManager.GetParameter("pHasHeciClei", DbType.String, "N".ToUpper(), ParameterDirection.Input);
                    parameters[15] = dbManager.GetParameter("pStandaloneClei", DbType.String, "N".ToUpper(), ParameterDirection.Input);
                    parameters[16] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP(proc, parameters);

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
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (dbManager != null)
                        dbManager.Dispose();
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

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
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

        public async Task<List<Hashtable>> searchTmpltSpec(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;
            var list = new List<Hashtable>();
            var proc = "specn_pkg.search_specifications";
            var parms = "pTyp,pClss,pId,pNm,pDsc,pStts,pmodelNm,pMtlCd";

            await Task.Run(() =>
            {
                try
                {

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = CreateParamsWithRefCursor(dbm, parms, data);
                    
                    reader = dbm.ExecuteDataReaderSP(proc, parameters);                  

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

        public async Task<long> SearchBaseTmplt(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            Exception error = null;
            var baseTempltId = 0L;
            var proc = pkg + ".search_base_tmplt";
            var parms = "pSpecId,pSpecTyp";
            var list = new List<Hashtable>();

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = CreateParamsWithReturnedId(dbm, parms, data, "pBaseTmpId");
                    
                    dbm.ExecuteNonQuerySP(proc, parameters);                    

                    baseTempltId = long.Parse(parameters[parameters.Length - 1].Value.ToString());
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

            return baseTempltId;
        }

        public async Task<string> FindTmpltByName(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            Exception error = null;
            var returnRes = 0L;
            var resultVal = string.Empty;
            var proc = pkg + ".search_tmplt_name";
            var parms = "pTmpNm";
            var list = new List<Hashtable>();

            if (proc.StartsWith(pkg) == false)
            {
                proc = pkg + "." + proc;
            }

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = CreateParamsWithReturnedId(dbm, parms, data, "resId");

                    dbm.ExecuteNonQuerySP(proc, parameters);

                    returnRes = long.Parse(parameters[parameters.Length - 1].Value.ToString());
                    if (returnRes == 1)
                    {
                        resultVal = "This template name " + parameters[0].Value.ToString() + " already exists; please choose a different name.";
                    }
                    else
                    {
                        resultVal = "This template name " + parameters[0].Value.ToString() + " does not exist";
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

            return resultVal;
        }

        public async Task<List<Hashtable>> GET_TMPLT(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            Exception error = null;
            var list = new List<Hashtable>();
            var proc = pkg + ".get_tmplt";
            var parms = "pTmpltId";

            await Task.Run(() =>
            {
                try
                {

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = CreateParamsWithRefCursor(dbm, parms, data);

                    reader = dbm.ExecuteDataReaderSP(proc, parameters);

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

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);
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

        public override ITemplate GetTemplate(long templateId, bool isBaseTemplate)
        {
            throw new NotImplementedException();
        }

        public async Task<Hashtable> GET_SHELF_TMPLT(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "overl_shelf_pkg.get_shelf_tmplt";
            Hashtable item = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                    parameters = dbManager.GetParameterArray(2);

                    parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    
                    reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                    while (reader.Read())
                    {
                        var rec = new GenericRec(reader);
                        item = rec.Data;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "TemplateDbInterface.GET_SHELF_TMPLT({0}): Unable to retrieve template record - {1}", pTmpltId, proc);
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
            });
            return item;
        }

        public async Task<List<Hashtable>> SEARCH_FOR_EQUIP(string tmpltId, string cat,string context, string match)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            string proc = "tmplt_pkg.search_for_equip";
            
            var list = new List<Hashtable>();
            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parms = dbManager.GetParameterArray(5);

                parms[0] = dbManager.GetParameter("pREF_TMPLT_ID", DbType.String, tmpltId, ParameterDirection.Input);
                parms[1] = dbManager.GetParameter("pCAT", DbType.String, cat, ParameterDirection.Input);
                parms[2] = dbManager.GetParameter("pCONTEXT", DbType.String, context, ParameterDirection.Input);
                parms[3] = dbManager.GetParameter("pMATCH", DbType.String, match, ParameterDirection.Input);
                parms[4] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parms);
                while (reader.Read())
                {
                    list.Add((new GenericRec(reader)).Data);
                }
            }
            catch (Exception ex)
            {
                var msg = string.Format("ERROR: TemplateDbInterface.SEARCH_FOR_EQUIP({0},{1},{2})", cat, context, match);
                logger.Error(ex, msg);
                list.Add((new GenericResponse(-1, msg, ex.Message)).Data);
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
            return list;
        }

        public ComplexTemplate GET_COMPLEX_TMPLT(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "complex_tmplt_pkg.get_complex_tmplt";
            ComplexTemplate complextTemplate = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    if (complextTemplate == null)
                    {
                        complextTemplate = new ComplexTemplate();
                    }
                    complextTemplate.TemplateID = int.Parse(reader["tmplt_id"].ToString());
                    complextTemplate.BaseTemplateID = int.Parse(reader["base_tmplt_id"].ToString());
                    complextTemplate.MaterialCatID = reader["mtrl_cat_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["mtrl_cat_id"].ToString());
                    complextTemplate.FeatureTypeID = reader["feat_typ_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["feat_typ_id"].ToString());
                    complextTemplate.BayExtenderSpecnRevisionAltID = reader["bay_extndr_specn_revsn_alt_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["bay_extndr_specn_revsn_alt_id"].ToString());
                    complextTemplate.CardSpecnWithPartsPartsID = reader["card_specn_with_prts_prts_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["card_specn_with_prts_prts_id"].ToString());
                    complextTemplate.CardSpecnWithSlotsSlotsID = reader["card_specn_with_slts_slts_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["card_specn_with_slts_slts_id"].ToString());
                    complextTemplate.ShelfSpecnWithSlotsSlotsID = reader["shelf_specn_with_slts_slts_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["shelf_specn_with_slts_slts_id"].ToString());
                    complextTemplate.AssignableNDSpecnWithPartsAsmtID = reader["asnbl_nd_spcn_wth_prts_asmt_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["asnbl_nd_spcn_wth_prts_asmt_id"].ToString());
                    complextTemplate.HlpMaterialRevsnID = reader["hlp_mtrl_revsn_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["hlp_mtrl_revsn_id"].ToString());
                    complextTemplate.CommonConfigID = reader["comn_cnfg_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["comn_cnfg_id"].ToString());
                    complextTemplate.LabelName = reader["label_nm"].ToString();
                    complextTemplate.RotationAngleID = reader["rottn_angl_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["rottn_angl_id"].ToString());
                    complextTemplate.FrontRearInd = reader["frnt_rer_ind"].ToString();
                    complextTemplate.PortTypeID = reader["port_typ_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["port_typ_id"].ToString());
                    complextTemplate.ConnectorTypeID = reader["cnctr_typ_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["cnctr_typ_id"].ToString());
                    complextTemplate.UserComment = reader["user_comment"].ToString();
                    complextTemplate.CUID = reader["cuid"].ToString();
                    complextTemplate.LastUpdatedDate = reader["last_updated_date"].ToString() == String.Empty ? default(DateTime) : DateTime.Parse(reader["last_updated_date"].ToString());
                    complextTemplate.MaterialCatType = reader["mtrl_cat_typ"].ToString();
                    complextTemplate.FeatureType = reader["feat_typ"].ToString();
                    complextTemplate.BayExtenderSpecnRevisionName = reader["bay_extndr_specn_revsn_nm"].ToString();
                    complextTemplate.RootPartNumber = reader["rt_part_no"].ToString();
                    complextTemplate.MaterialCode = reader["mtrl_cd"].ToString();
                    complextTemplate.CommonConfigName = reader["comn_cnfg_nm"].ToString();
                    complextTemplate.RotationAngleDegreeNumber = reader["rottn_angl_dgr_no"].ToString();
                    complextTemplate.PortType = reader["port_typ"].ToString();
                    complextTemplate.ConnectorTypeCode = reader["cnctr_typ_cd"].ToString();

                    List<long> templateDefIDs = GetTemplateDefIDs(pTmpltId);
                    foreach (long templateDefID in templateDefIDs)
                    {
                        if (complextTemplate.ComplextTemplateDefList == null)
                        {
                            complextTemplate.ComplextTemplateDefList = new List<ComplexTemplate.ComplextTemplateDefObject>();
                        }
                        complextTemplate.ComplextTemplateDefList.Add(GetTemplateDef(templateDefID));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GetComplextTemplate({0}): Unable to retrieve GetComplextTemplate record - {1}", pTmpltId, proc);
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
            return complextTemplate;
        }
        
        public List<long> GetTemplateDefIDs(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "complex_tmplt_pkg.get_template_defs";
            List<long> templateDefIDs = new List<long>();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    templateDefIDs.Add(long.Parse(reader["tmplt_def_id"].ToString()));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GetTemplateDefIDs({0}): Unable to retrieve GetTemplateDefIDs - {1}", pTmpltId, proc);
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
            return templateDefIDs;
        }

        public ComplexTemplate.ComplextTemplateDefObject GetTemplateDef(long pTmpltDefId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "complex_tmplt_pkg.get_complex_tmplt_def";
            ComplexTemplate.ComplextTemplateDefObject complexTemplateDefObject = new ComplexTemplate.ComplextTemplateDefObject();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltDefId", DbType.Int64, pTmpltDefId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    complexTemplateDefObject.TemplateDefID = int.Parse(reader["tmplt_def_id"].ToString());
                    complexTemplateDefObject.AssociatedTemplateID = int.Parse(reader["assoc_tmplt_id"].ToString());
                    complexTemplateDefObject.ParentTemplateID = int.Parse(reader["prnt_tmplt_id"].ToString());
                    complexTemplateDefObject.XCoordNo = reader["x_coord_no"].ToString() == String.Empty ? default(int) : int.Parse(reader["x_coord_no"].ToString());
                    complexTemplateDefObject.YCoordNo = reader["y_coord_no"].ToString() == String.Empty ? default(int) : int.Parse(reader["y_coord_no"].ToString());
                    complexTemplateDefObject.FrontRearInd = reader["frnt_rer_ind"].ToString();
                    complexTemplateDefObject.LabelName = reader["label_nm"].ToString();
                    complexTemplateDefObject.RotationAngleID = reader["rottn_angl_id"].ToString() == String.Empty ? default(int) : int.Parse(reader["rottn_angl_id"].ToString());
                    complexTemplateDefObject.AssociatedTemplateName = reader["assoc_tmplt_nm"].ToString();
                    complexTemplateDefObject.ParentTemplateName = reader["prnt_tmplt_nm"].ToString();
                    complexTemplateDefObject.RotationAngleDegreeNumber = reader["rottn_angl_dgr_no"].ToString();
                    complexTemplateDefObject.AssociatedTemplate = new ComplexTemplate.TemplateObject();
                    complexTemplateDefObject.AssociatedTemplate = GET_TMPLT(complexTemplateDefObject.AssociatedTemplateID);
                    complexTemplateDefObject.ParentTemplate = new ComplexTemplate.TemplateObject();
                    complexTemplateDefObject.ParentTemplate = GET_TMPLT(complexTemplateDefObject.ParentTemplateID);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GetTemplateDef({0}): Unable to retrieve GetTemplateDef record - {1}", pTmpltDefId, proc);
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
            return complexTemplateDefObject;
        }
        /*
        public Hashtable GET_COMPLEX_TMPLT(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "tmplt_pkg.get_tmplt";

            Hashtable rec = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    var item = new GenericRec(reader);
                    rec = item.Data;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GET_TMPLT({0}): Unable to retrieve template record - {1}", pTmpltId, proc);
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
            return rec;
        }
        */
        /*
        public Hashtable GET_TMPLT(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "tmplt_pkg.get_tmplt";

            Hashtable rec = null;

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    var item = new GenericRec(reader);
                    rec = item.Data;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GET_TMPLT({0}): Unable to retrieve template record - {1}", pTmpltId, proc);
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
            return rec;
        }
        */
        public ComplexTemplate.TemplateObject GET_TMPLT(long pTmpltId)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            string proc = "tmplt_pkg.get_tmplt";
            ComplexTemplate.TemplateObject templateObject = new ComplexTemplate.TemplateObject();

            try
            {
                dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, DbConnectionString);

                parameters = dbManager.GetParameterArray(2);

                parameters[0] = dbManager.GetParameter("pTmpltId", DbType.Int64, pTmpltId, ParameterDirection.Input);
                parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);


                reader = dbManager.ExecuteDataReaderSP(proc, parameters);
                while (reader.Read())
                {
                    templateObject.TemplateID = int.Parse(reader["tmplt_id"].ToString());
                    templateObject.TemplateName = reader["tmplt_nm"].ToString();
                    templateObject.TemplateTypeID = int.Parse(reader["tmplt_typ_id"].ToString());
                    templateObject.TemplateType = reader["tmplt_typ"].ToString();
                    templateObject.TemplateDescription = reader["tmplt_dsc"].ToString();
                    templateObject.BaseTemplateInd = reader["base_tmplt_ind"].ToString();
                    templateObject.HlpTmpltInd = reader["hlp_tmplt_ind"].ToString();
                    templateObject.CommonConfigTemplateInd = reader["comn_cnfg_tmplt_ind"].ToString();
                    templateObject.CompletedInd = reader["cmplt_ind"].ToString();
                    templateObject.PropagatedInd = reader["prpgt_ind"].ToString();
                    templateObject.UpdateInProgressInd = reader["updt_in_prgs_ind"].ToString();
                    templateObject.RetiredTemplateInd = reader["ret_tmplt_ind"].ToString();
                    templateObject.DeletedInd = reader["del_ind"].ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateDbInterface.GetTemplateObject({0}): Unable to retrieve GetTemplateObject record - {1}", pTmpltId, proc);
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
            return templateObject;
        }

        public async Task<string> GetTmpltTyp(long tmpltId)
        {
            var typ = "";
            await Task.Run(() =>
            {
               try
               {
                   var tmplt = this.GET_TMPLT(tmpltId);
                   typ = tmplt.TemplateType; // mixed case bad ("Bay" vs "BAY")
               }
               catch (Exception ex)
               {
                   logger.Error(ex, "TemplateDbInterface.GetTmpltTyp({0}): Unable to retrieve template type", tmpltId);
               }
               finally
               {

               }
           });
           return typ;
        }
    }
}