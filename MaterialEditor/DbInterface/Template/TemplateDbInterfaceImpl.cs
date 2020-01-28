using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using NLog;
using Oracle.ManagedDataAccess.Client;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public abstract class TemplateDbInterfaceImpl : ITemplateDbInterface
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        protected IAccessor dbAccessor = null;

        public TemplateDbInterfaceImpl()
        {
            connectionString = System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"];
        }

        public TemplateDbInterfaceImpl(string dbConnectionString)
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

        // mwj: need to work on better generalizing these CreateParamsXxxxx methods...
        protected IDbDataParameter[] CreateParamsWithRefCursor(IAccessor dbi, string parms, Hashtable fields)
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
                    //if (val == null || val.Length == 0 || val == "%")
                    if (val == null || val.Length == 0)
                    {
                        val = null;
                        p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                    }
                    else if(val == "%")
                    {
                        p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                    }
                   else
                    {
                        p[i] = dbi.GetParameter(parm, DbType.Int64, val, ParameterDirection.Input);
                    }
                }
                else
                {
                    p[i] = dbi.GetParameter(parm, DbType.String, val, ParameterDirection.Input);
                }
            }

            p[len] = dbi.GetParameterCursorType("retCsr", ParameterDirection.Output);

            return p;
        }

        protected IDbDataParameter[] CreateStringParamsWithRefCursor(IAccessor dbi, string parms, Hashtable fields)
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

        protected IDbDataParameter[] CreateParams(IAccessor dbi, string parms, Hashtable fields)
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

        protected IDbDataParameter[] CreateParamsWithReturnedId(IAccessor dbi, string parms, Hashtable fields, string retParamName)
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

        protected IDbDataParameter[] CreateParamsWithRefCursor(IAccessor dbi, Hashtable args)
        {
            var keys = new List<string>();

            foreach (var e in args.Keys)
            {
                keys.Add(e.ToString());
            }

            var parms = String.Join(",", keys);

            return CreateStringParamsWithRefCursor(dbi, parms, args);
        }

        protected Option Copy(Option item, IDataReader reader)
        {
            item.text = DataReaderHelper.GetNonNullValue(reader, "TEXT");
            item.value = DataReaderHelper.GetNonNullValue(reader, "VALUE");
            item.description = DataReaderHelper.GetNonNullValue(reader, "DESCRIPTION");

            return item;
        }

        public void UPDATE_APP_LOG(string pkg, string callSite, string cuid, string msg, string logLevel = "INFO")
        {
            var parms = "pCallSite,pMsg,pCUID,pLogLevel";
            var args = new Hashtable();

            args["pCallSite"] = callSite;
            args["pCUID"] = cuid;
            args["pMsg"] = (msg.Length > 4000 ? msg.Substring(0, 3999) : msg);
            args["pLogLevel"] = logLevel;

            ExecNonQueryStoredProc(pkg, "UPDATE_APP_LOG", parms, args);
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

        public List<Hashtable> ExecQueryStoredProc(string pkg, string proc, Hashtable query)
        {
            var keys = new List<string>();

            foreach (var e in query.Keys)
            {
                keys.Add(e.ToString());
            }

            var parms = String.Join(",", keys);

            return ExecQueryStoredProc(proc, parms, query);
        }

        public void ExecNonQueryStoredProc(string pkg, string proc, string args, Hashtable item)
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

        public List<Hashtable> ExecQueryStoredProc(string pkg, string proc, string args, Hashtable query)
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

        protected long CreateTemplateInTmpltTbl(string name, string description, string templateType, bool isBaseTemplate)
        {
            IDbDataParameter[] tmpltParameters = null;
            long id = 0;

            try
            {
                tmpltParameters = dbAccessor.GetParameterArray(12);

                tmpltParameters[0] = dbAccessor.GetParameter("pTmpltNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                tmpltParameters[1] = dbAccessor.GetParameter("pTmpltTyp", DbType.String, templateType, ParameterDirection.Input);
                tmpltParameters[2] = dbAccessor.GetParameter("pTmpltDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                tmpltParameters[3] = dbAccessor.GetParameter("pBaseTmpltInd", DbType.String, isBaseTemplate ? "Y" : "N", ParameterDirection.Input);
                if (templateType.ToUpper().IndexOf("HIGH") > -1)
                {
                    tmpltParameters[4] = dbAccessor.GetParameter("pHlpTmpltInd", DbType.String, "Y", ParameterDirection.Input);
                }
                else {
                    tmpltParameters[4] = dbAccessor.GetParameter("pHlpTmpltInd", DbType.String, "N", ParameterDirection.Input);
                }
                if (templateType.ToUpper().IndexOf("COMMN") > -1)
                {
                    tmpltParameters[5] = dbAccessor.GetParameter("pComnCnfgTmpltInd", DbType.String, "Y", ParameterDirection.Input);
                }
                else
                {
                    tmpltParameters[5] = dbAccessor.GetParameter("pComnCnfgTmpltInd", DbType.String, "N", ParameterDirection.Input);
                }
                tmpltParameters[6] = dbAccessor.GetParameter("pCmpltInd", DbType.String, "N", ParameterDirection.Input);
                tmpltParameters[7] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, "N", ParameterDirection.Input);
                tmpltParameters[8] = dbAccessor.GetParameter("pUpdtInPrgsInd", DbType.String, "N", ParameterDirection.Input);
                tmpltParameters[9] = dbAccessor.GetParameter("pRetTmpltInd", DbType.String, "N", ParameterDirection.Input);
                tmpltParameters[10] = dbAccessor.GetParameter("pDelInd", DbType.String, "N", ParameterDirection.Input);
                tmpltParameters[11] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Output);
                //PROCEDURE insert_tmplt(pTmpltNm IN tmplt.tmplt_nm% TYPE, pTmpltTyp IN tmplt.tmplt_typ_id % TYPE, pTmpltDsc IN tmplt.tmplt_dsc % TYPE,
                //pBaseTmpltInd IN tmplt.base_tmplt_ind % TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind % TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind % TYPE,
                //pCmpltInd IN tmplt.cmplt_ind % TYPE, pPrpgtInd IN tmplt.prpgt_ind % TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind % TYPE,
                //pRetTmpltInd IN tmplt.ret_tmplt_ind % TYPE, pDelInd tmplt.del_ind % TYPE, pTmpltId OUT tmplt.tmplt_id % TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "tmplt_pkg.insert_tmplt", tmpltParameters);

                id = long.Parse(tmpltParameters[11].Value.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("Unable to create base template ({0}, {1})", name, description);
                id = 0;
                throw ex;
            }

            return id;
        }

        public void UpdateTemplate(string name, string description, bool isBaseTemplate, bool isHlpTmplt, bool isComnConfigTmplt, bool isCompleted, bool isPropagated,
            bool updateInProgress, bool isRetired, bool isDeleted, long id)
        {
            IDbDataParameter[] tmpltParameters = null;

            try
            {
                tmpltParameters = dbAccessor.GetParameterArray(11);

                tmpltParameters[0] = dbAccessor.GetParameter("pTmpltNm", DbType.String, name.ToUpper(), ParameterDirection.Input);
                tmpltParameters[1] = dbAccessor.GetParameter("pTmpltDsc", DbType.String, CheckNullValue(description), ParameterDirection.Input);
                tmpltParameters[2] = dbAccessor.GetParameter("pBaseTmpltInd", DbType.String, isBaseTemplate ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[3] = dbAccessor.GetParameter("pHlpTmpltInd", DbType.String, isHlpTmplt ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[4] = dbAccessor.GetParameter("pComnCnfgTmpltInd", DbType.String, isComnConfigTmplt ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[5] = dbAccessor.GetParameter("pCmpltInd", DbType.String, isCompleted ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[6] = dbAccessor.GetParameter("pPrpgtInd", DbType.String, isPropagated ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[7] = dbAccessor.GetParameter("pUpdtInPrgsInd", DbType.String, updateInProgress ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[8] = dbAccessor.GetParameter("pRetTmpltInd", DbType.String, isRetired ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[9] = dbAccessor.GetParameter("pDelInd", DbType.String, isDeleted ? "Y" : "N", ParameterDirection.Input);
                tmpltParameters[10] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                //PROCEDURE update_tmplt (pTmpltNm IN tmplt.tmplt_nm%TYPE, pTmpltDsc IN tmplt.tmplt_dsc%TYPE,
                //pBaseTmpltInd IN tmplt.base_tmplt_ind % TYPE, pHlpTmpltInd IN tmplt.hlp_tmplt_ind % TYPE, pComnCnfgTmpltInd IN tmplt.comn_cnfg_tmplt_ind % TYPE,
                //pCmpltInd IN tmplt.cmplt_ind % TYPE, pPrpgtInd IN tmplt.prpgt_ind % TYPE, pUpdtInPrgsInd IN tmplt.updt_in_prgs_ind % TYPE,
                //pRetTmpltInd IN tmplt.ret_tmplt_ind % TYPE, pDelInd tmplt.del_ind % TYPE, pTmpltId IN tmplt.tmplt_id % TYPE)

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "tmplt_pkg.update_tmplt", tmpltParameters);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to update template ({0}, {1}, {2})", id, name, description);

                throw ex;
            }
        }

        public void CreateOverAllTemplate(long id,long baseTmpltId, string cuid,int HlpRevId)
        {
            IDbDataParameter[] tmpltParameters = null;

            try
            {
                tmpltParameters = dbAccessor.GetParameterArray(18);

                tmpltParameters[0] = dbAccessor.GetParameter("pTmpltId", DbType.Int64, id, ParameterDirection.Input);
                tmpltParameters[1] = dbAccessor.GetParameter("pBaseTmpltId", DbType.Int64, baseTmpltId, ParameterDirection.Input);
                tmpltParameters[2] = dbAccessor.GetParameter("pMtrlCatId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[3] = dbAccessor.GetParameter("pFeatTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[4] = dbAccessor.GetParameter("pBayExtndrSpecnRevsnAltId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[5] = dbAccessor.GetParameter("pCardSpecnWithPrtsPrtsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[6] = dbAccessor.GetParameter("pCardSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[7] = dbAccessor.GetParameter("pShelfSpecnWithSltsSltsId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[8] = dbAccessor.GetParameter("pAsnblNdSpcnWthPrtsAsmtId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[9] = dbAccessor.GetParameter("pHlpMtrlRevsnId", DbType.Int64, HlpRevId, ParameterDirection.Input);
                tmpltParameters[10] = dbAccessor.GetParameter("pComnCnfgId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[11] = dbAccessor.GetParameter("pLabelNm", DbType.String, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[12] = dbAccessor.GetParameter("pRottnAnglId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[13] = dbAccessor.GetParameter("pFrntRerInd", DbType.String, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[14] = dbAccessor.GetParameter("pPortTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[15] = dbAccessor.GetParameter("pCnctrTypId", DbType.Int64, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[16] = dbAccessor.GetParameter("pUserComment", DbType.String, DBNull.Value, ParameterDirection.Input);
                tmpltParameters[17] = dbAccessor.GetParameter("pCuid", DbType.String, cuid, ParameterDirection.Input);

                dbAccessor.ExecuteNonQuery(CommandType.StoredProcedure, "COMPLEX_TMPLT_PKG.insert_complex_tmplt", tmpltParameters);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to update template ({0}, {1}, {2})", id, baseTmpltId, cuid);

                throw ex;
            }
        }
        public abstract ITemplate GetTemplate(long templateId, bool isBaseTemplate);
    }
}