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

 
namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig
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
        private const string pkg = "COMN_CNFG_PKG";

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
            var p = dbi.GetParameterArray(len+1);
            for( var i = 0; i < len; i++)
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

        public void UPDATE_AUDIT_LOG_RELTD_CNFGS(string cuid, long comnCnfgId, string action)
        {
            //var proc = "UPDATE_AUDIT_LOG_RELTD_CNFGS";
            //var parms = "pCUID,pAction,pComnCnfgId";
            //var data = new Hashtable();
            //data["pCUID"] = cuid;
            //data["pComnCnfgId"] = comnCnfgId;
            //data["pAction"] = action;
            ////ExecNonQueryStoredProc(proc, parms, data);
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
                if (parm.EndsWith("Id") && ((val == null || val.Length == 0 || val == "%")) )
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
                    if( val == null || val.Length == 0 )
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

            for( var i = 0; i < len; i++)
            {
                var key = list[i];
                var val = (fields.Contains(key) ? fields[key] : "").ToString().Trim();
                var parm = (key.ToString().StartsWith("p") ? key : "p" + key).ToString();
                if ( parm.EndsWith("Id") || parm.EndsWith("Qty") )
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

            if( retParamName.EndsWith("Id"))
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
            foreach( var e in args.Keys )
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

        public void UpdateAppLog(string callSite, string cuid, string msg, string logLevel = "INFO")
        {
            var parms = "pCallSite,pMsg,pCUID,pLogLevel";
            var args = new Hashtable();
            args["pCallSite"] = callSite;
            args["pCUID"] = cuid;
            args["pMsg"] = (msg.Length > 4000 ? msg.Substring(0,3999) : msg);
            args["pLogLevel"] = logLevel;

            ExecNonQueryStoredProc("UPDATE_APP_LOG", parms, args);

        }
        public void UpdateAppLog(string callSite, string cuid, Hashtable msg, string logLevel = "INFO")
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
            UpdateAppLog(callSite, cuid, json, logLevel);
        }

        public void UpdateAuditLog(Hashtable item)
        {
            var args = "pCUID,pAction,pComnCnfgId,pComnCnfgDefId,pId,pItemDsc,pTblNm,pColNm,pOldVal,pNewVal";
            var proc = "UPDATE_AUDIT_LOG";
            var cntndInDefTyp = (item["pCntndInDefTyp"] ?? "").ToString().Trim().ToUpper();
            
            if( cntndInDefTyp == "HLP" || cntndInDefTyp == "CC" )
            {
                args = "pCUID,pAction,pComnCnfgId,pComnCnfgDefId,pCntndInDefId,pCntndInDefTyp,pId,pItemDsc,pTblNm,pColNm,pOldVal,pNewVal";
                proc = "UPDATE_AUDIT_LOG_CNTND_IN_ITEM";
            }
            ExecNonQueryStoredProc(proc, args, item);
        }

        public bool UPDATE_AUDIT_LOG_RAW(Hashtable rec)
        {
            var args = "pCREAT_ACTY_ID,pAUDIT_COL_DEF_ID,pAUDIT_TBL_PK_COL_NM,pAUDIT_TBL_PK_COL_VAL,pAUDIT_PRNT_TBL_PK_COL_NM,pAUDIT_PRNT_TBL_PK_COL_VAL,pACTN_CD,pOLD_COL_VAL,pNEW_COL_VAL,pCMNT_TXT";
            var proc = "UPDATE_AUDIT_LOG_RAW";

            var log = new Hashtable();
            foreach( var e in rec.Keys)
            {
                log["p" + e.ToString()] = rec[e.ToString()];
            }
            ExecNonQueryStoredProc(proc, args, log);
            return true;
        }
        public long UpdateCommonConfig(Hashtable rec)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            var err = "";

            long comnCnfgId = (rec.Contains("pComnCnfgId") ? long.Parse((rec["pComnCnfgId"] ?? "0").ToString()) : 0);

            var isUpdating = (comnCnfgId > 0);
            var proc = pkg + (isUpdating ? ".UPDATE_COMN_CNFG" : ".INSERT_COMN_CNFG");

            //await Task.Run(() =>
            //{
                try
                {

                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    var parms = (isUpdating
                                    ? "pComnCnfgId,pComnCnfgNm,pComnCnfgDsc,pDefCmpltInd,pDefPrpgtInd,pUpdtInPrgsInd,pRetComnCnfgInd,pTmpltTypId,pSpecnRevsnAltId,pTmpltNm,pDelInd"
                                    : "pComnCnfgNm,pComnCnfgDsc,pDefCmpltInd,pDefPrpgtInd,pUpdtInPrgsInd,pRetComnCnfgInd,pDelInd,pTmpltTypId,pSpecnRevsnAltId,pTmpltNm"
                                );

                    parameters = (isUpdating
                                    ? CreateParams(dbm, parms, rec)
                                    : CreateParamsWithReturnedId(dbm, parms, rec, "oComnCnfgId")
                                );

                    dbm.ExecuteNonQuerySP(proc, parameters);

                    if (isUpdating == false)
                    {
                        comnCnfgId = long.Parse(parameters[parameters.Length - 1].Value.ToString());
                    }
                }
                catch (OracleException oe)
                {
                    err = "Unable to perform operation: " + proc;
                    logger.Error(oe, err);

                    // mwj: this blows up on my machine 
                    //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);

                    err = oe.Message.Contains("unique constraint")
                          ? proc+": Could not update database due to unique contraint;\nplease check that the name of the entry is unique."
                          : oe.Message
                    ;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    logger.Error(ex, "Unable to perform operation: " + proc);
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
            //});

            if (err != "")
            {
                throw new Exception(err);
            }

            return comnCnfgId;
        }

        
        public async Task<List<Hashtable>> GET_RLTDTO_PRNT_REFS(string cuid, Hashtable prnt)
        {
            var args = "pCUID,pComnCnfgId,pComnCnfgDefId,pMtrlCatTyp,pFeatTyp,pSpecNm";
            prnt["pCUID"] = cuid;

            List<Hashtable> list = null;
            var proc = "GET_RLTDTO_PRNT_REFS";
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, args, prnt);
            });
            return list;
        }

        public async Task<bool> REMOVE_RLTDTO_PRNT_REFS(string cuid, Hashtable prnt)
        {
            var args = "pComnCnfgId,pComnCnfgDefId,pMtrlCatTyp,pFeatTyp,pSpecNm";
            var proc = "REMOVE_RLTDTO_PRNT_REFS";
            await Task.Run(() => {
                ExecNonQueryStoredProc(proc, args, prnt);
            });
            return true;
        }

        public async Task<bool> SYNC_OTHER_CNFGS_WITH_NEW_ITEM(long comnCnfgDefId, string chldTyp)
        {
            var args = "pComnCnfgDefId,pChildTyp";
            var proc = "SYNC_OTHER_CNFGS_WITH_NEW_ITEM";
            var query = new Hashtable();
            query["pComnCnfgDefId"] = comnCnfgDefId;
            query["pChldTyp"] = chldTyp;
            await Task.Run(() => {
                ExecNonQueryStoredProc(proc, args, query);
            });
            return true;
        }

        public async Task<List<Hashtable>> GET_HLP_ITEMS(long hlpId)
        {
            var parms = new Hashtable();
            parms["pHlpId"] = hlpId;

            var proc = "GET_HLP_ITEMS";

            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms);
            });
            return list;
        }
        public async Task<List<Hashtable>> GET_CNFG_CI_HLP_ITEMS(long comnCnfgDefId, int lvl, long hlpId)
        {
            List<Hashtable> list = null;

            var proc = "GET_CNFG_CI_HLP_ITEMS";

            var parms = "pComnCnfgDefId,pHlpId";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;
            args["pHlpId"] = hlpId;

            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);
            });

            return list;
        }

        public async Task<List<Hashtable>> GET_CNFG_CI_CI_HLP_ITEMS(long comnCnfgDefId, int lvl, long hlpId, int seq)
        {
            List<Hashtable> list = null;

            var proc = "GET_CNFG_CI_CI_HLP_ITEMS";

            var parms = "pComnCnfgDefId,pLvl,pHlpId,pSeq";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;
            args["pLvl"] = lvl;
            args["pHlpId"] = hlpId;
            args["pSeq"] = seq;

            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);
            });

            return list;
        }

        public bool DeleteCntndInItem(string proc, long comnCnfgDefId, long cntndInId)
        {
            var parms = "pComnCnfgDefId,pCntndInId";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;
            args["pCntndInId"] = cntndInId;
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
            return true;
        }

        public bool DeleteCntndInLocItem(string cat, long comnCnfgDefId, long cntndInId)
        {
            var proc = "DELETE_CNFG_CI_" + cat + "_LOC";
            var parms = "pComnCnfgDefId,pCntndInId";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;
            args["pCntndInId"] = cntndInId;
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
            return true;
        }

        public bool DeleteCntndInDefItem(string cat, long comnCnfgDefId, long cntndInId)
        {
            cat = (cat == "HIGH LEVEL PART" ? "HLP_MTRL"
                  : cat == "COMMON CONFIG" ? "COMN_CNFG"
                  : "");

            if ( cat.Length == 0 )
            {
                return true;
            }

            var proc = "DELETE_CNFG_CI_" + cat+"_DEF";
            var parms = "pComnCnfgDefId,pCntndInId";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;
            args["pCntndInId"] = cntndInId;
            //await Task.Run(() => {
            ExecNonQueryStoredProc(proc, parms, args);
            //});
            return true;
        }

        public long UpdateComnCnfgDefItem(Hashtable item, bool doDelete = false)
        {
            var del = ".DELETE_COMN_CNFG_DEF";
            var d_parms = "pComnCnfgDefId";

            var insert = ".INSERT_COMN_CNFG_DEF";
            var i_parms = "pComnCnfgId,pCntndInSeqNo,pCntndInMtrlCatId,pCntndInFeatTypId";

            var update = ".UPDATE_COMN_CNFG_DEF";
            var u_parms = "pComnCnfgDefId,pComnCnfgId,pCntndInSeqNo,pCntndInMtrlCatId,pCntndInFeatTypId";

            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;

            var err = "";

            long comnCnfgDefId = (item.Contains("pComnCnfgDefId") ? long.Parse((item["pComnCnfgDefId"] ?? "0").ToString()) : 0);

            var isUpdating = (comnCnfgDefId > 0);
            var proc = pkg + (doDelete ? del : (isUpdating ? update : insert));
            var parms = (doDelete ? d_parms : (isUpdating ? u_parms : i_parms));

            //await Task.Run(() =>
            //{
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parameters = (isUpdating
                                    ? CreateParams(dbm, parms, item)
                                    : CreateParamsWithReturnedId(dbm, parms, item, "oComnCnfgDefId")
                                );

                    dbm.BeginTransaction();
                        dbm.ExecuteNonQuerySP(proc, parameters);
                    dbm.CommitTransaction();

                    if (isUpdating == false)
                    {
                        comnCnfgDefId = long.Parse(parameters[parameters.Length - 1].Value.ToString());
                    }
                }
                catch (OracleException oe)
                {
                    err = "Unable to perform operation: " + proc;
                    logger.Error(oe, err);

                    // mwj: this blows up on my machine 
                    //EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);

                    err = oe.Message.Contains("unique constraint")
                          ? proc+": Could not update database due to unique contraint;\nplease check that the name of the entry is unique."
                          : oe.Message
                    ;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    logger.Error(ex, "Unable to perform operation: " + proc);
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
            //});

            if (err != "")
            {
                throw new Exception(err);
            }

            return comnCnfgDefId;
        }

        
        public async Task<long> CloneConfig(Hashtable data)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            
            var err = "";

            var proc =pkg+".CLONE_COMN_CNFG";
            var parms = "pCuid,pSrcComnCnfgId,pComnCnfgNm,pComnCnfgDsc,pTmpltNm";
            var comnCnfgId = 0L;
            await Task.Run(()=>{
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = CreateParamsWithReturnedId(dbm, parms, data, "oComnCnfgId");
                    
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
                          ? proc+": Could not update database due to unique contraint;\nplease check that the name of the entry is unique."
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

            if( err != "" )
            {
                throw new Exception(err);
            }
            return comnCnfgId;
        }

        public async void UPDATE_CNFG_CI_BAY_MTRL(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_BAY_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pFrntRerInd,pTmpltId";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public async void UPDATE_CNFG_CI_BAY_EXTND(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_BAY_EXTND";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UPDATE_CNFG_CI_NODE_MTRL(Hashtable args)
        {
            // CHECKHERE: for parent id's....
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            //await Task.Run(() => {
                ExecNonQueryStoredProc("UPDATE_CNFG_CI_NODE_MTRL", parms, args);
                UpdateCntndInLocInfo("UPDATE_CNFG_CI_NODE_LOC", args);
            //});
        }

        public void UPDATE_CNFG_CI_SHELF_MTRL(Hashtable args)
        {
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            
            //await Task.Run(() => {
                ExecNonQueryStoredProc("UPDATE_CNFG_CI_SHELF_MTRL", parms, args);
                UpdateCntndInLocInfo("UPDATE_CNFG_CI_SHELF_LOC", args);
            //});
        }

        public void UPDATE_CNFG_CI_CARD_MTRL(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_CARD_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UPDATE_CNFG_CI_PLG_IN_MTRL(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_PLG_IN_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInComnCnfg(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_COMN_CNFG";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";

            //
            // NOTE: do we need to update "def" version the UPDATE_CNFG_CI_COMN_CNFG_DEF?
            //       the only thing changing here is qty....
            //
            //var defParms = "pComnCnfgCntndComnCnfgDefId,pComnCnfgDefId,pCntndInComnCnfgDefId,pPrntComnCnfgDefId";
            // 

            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInCcChildItem(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_CC_CHILD_ITEM";
            var parms = "pComnCnfgDefId,pComnCnfgCntdComnCnfgDfId,pCntndInComnCnfgDefId,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pXCoordNo,pYCoordNo,pFrntRerInd,pLabelNm,pRackPos";
            //await Task.Run(() => {
            ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInHlpChildItem(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_HLP_CHILD_ITEM";
            var parms = "pComnCnfgDefId,pComnCnfgCntndHlpMtrlDfId,pHlpMtrlRevsnDefId,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pXCoordNo,pYCoordNo,pFrntRerInd,pLabelNm,pRackPos";

            //await Task.Run(() => {
            ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInMnrMtrl(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_MNR_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInCnctCable(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_CNCT_CABL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInBulkCable(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_BULK_CABL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, args);
            //});
        }

        public void UpdateCntndInNonRmeMtrl(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_NON_RME_MTRL"; // doesnt exist yet...
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, args);
        }

        public void UpdateCntndInHighLevelPart(Hashtable args)
        {
            var proc = "UPDATE_CNFG_CI_HLP_MTRL_DEF"; 
            var parms = "pComnCnfgCntndHlpMtrlDefId,pComnCnfgDefId,pCntndInHlpMtrlRevsnDefId,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId";
            ExecNonQueryStoredProc(proc, parms, args);

            proc = "UPDATE_CNFG_CI_HLP_MTRL";
            parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            
            ExecNonQueryStoredProc(proc, parms, args);

            //
            // NOTE: do we need to update "def" version the UPDATE_CNFG_CI_HLP_MTRL_DEF?
            //       the only thing changing here is qty....
            //
            //var defParms = "pComnCnfgCntndHlpMtrlDefId,pComnCnfgDefId,pCntndInHlpMtrlRevsnDefId,pPrntComnCnfgDefId";
            // 

            //await Task.Run(() => { ExecNonQueryStoredProc(proc, parms, args); });
        }

        public void UpdateCntndInItem(string proc, string args, Hashtable item)
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
                dbm.ExecuteNonQuerySP(proc, parms);
            }
            catch (OracleException oe)
            {
                err = proc+": unable to perform insert (db error)";
                logger.Error(oe, err);
                EventLogger.LogAlarm(oe, err, SentryIdentifier.EmailDev, SentrySeverity.Major);
            }
            catch (Exception ex)
            {
                err = proc + ": upable to perform insert (general error)";
                logger.Error(ex, err);
            }
            finally
            {
                if (dbm != null)
                    dbm.Dispose();
            }

            if( err.Length > 0 )
                throw new Exception(err);
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
        public void INSERT_CNFG_CI_BAY_MTRL(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_BAY_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pFrntRerInd,pTmpltId";
            //await Task.Run(() => {
                UpdateCntndInItem(proc, parms, rec);
            //});
        }
        public void INSERT_CNFG_CI_BAY_EXTND(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_BAY_EXTND";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId";
            ExecNonQueryStoredProc(proc, parms, rec);
            //await Task.Run(() => { });
        }
        public void INSERT_CNFG_CI_NODE_MTRL(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_NODE_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";

            //await Task.Run(() => {
                UpdateCntndInItem(proc, parms, rec);
                UpdateCntndInLocInfo("INSERT_CNFG_CI_NODE_LOC", rec);
            //});
        }

        public void INSERT_CNFG_CI_SHELF_MTRL(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_SHELF_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            
            //await Task.Run(() => {
                UpdateCntndInItem(proc, parms, rec);
                UpdateCntndInLocInfo("INSERT_CNFG_CI_SHELF_LOC", rec);
            //});
        }
        public void UpdateCntndInLocInfo(string proc, Hashtable rec)
        {
            var parms = "pComnCnfgDefId,pCntndInId,pXCoordNo,pYCoordNo,pFrntRerInd,pLabelNm,pRackPos,pRottnAnglId";

            var x = (rec["pXCoordNo"] ?? "").ToString().Trim();
            var y = (rec["pYCoordNo"] ?? "").ToString().Trim();
            var fr = (rec["pFrntRerInd"] ?? "").ToString().Trim();

            rec["pXCoordNo"] = (x.Length == 0 ? "0" : x); // currently COMN_CNFG_XXX_LOC tables required a value; not null...
            rec["pYCoordNo"] = (y.Length == 0 ? "0" : y); // currently COMN_CNFG_XXX_LOC tables required a value; not null...
            rec["pFrntRerInd"] = (fr.Length == 0 ? "" : fr);
            rec["pRottnAnglId"] = 1; // default...

            //await Task.Run(() => {
                ExecNonQueryStoredProc(proc, parms, rec);
            //});
        }

      

        public void INSERT_CNFG_CI_CARD_MTRL(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_CARD_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            ExecNonQueryStoredProc(proc, parms, rec);
            //await Task.Run(() => { });
        }
        public void INSERT_CNFG_CI_PLG_IN_MTRL(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_PLG_IN_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId,pTmpltId";
            ExecNonQueryStoredProc(proc, parms, rec);

            //await Task.Run(() => { });
        }
        public void InsertCntndInHighLevelPart(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_HLP_MTRL_DEF";
            var parms = "pComnCnfgCntndHlpMtrlDefId,pComnCnfgDefId,pCntndInHlpMtrlRevsnDefId,pPrntComnCnfgDefId,pPrntCiHlpMtrlRevsnDefId,pPrntCiComnCnfgDefId";
            rec["pComnCnfgCntndHlpMtrlDefId"] = (rec["pCntndInId"]??"-1").ToString();
            ExecNonQueryStoredProc(proc, parms, rec);

            proc = "INSERT_CNFG_CI_HLP_MTRL";
            parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);

            // await Task.Run(() => { });
        }
        public void InsertCntndInComnCnfg(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_COMN_CNFG";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);

            proc = "INSERT_CNFG_CI_COMN_CNFG_DEF";
            parms = "pComnCnfgCntndComnCnfgDefId,pComnCnfgDefId,pCntndInComnCnfgDefId,pPrntComnCnfgDefId";
            rec["pCntndInComnCnfgDefId"] = (rec["pCntndInId"] ?? "-1").ToString();
            ExecNonQueryStoredProc(proc, parms, rec);
            //await Task.Run(() => { });
        }

        public async Task<List<Hashtable>> GET_COMN_CNFG_REFS(long comnCnfgId)
        {
            var parms = new Hashtable();
            parms["pComnCnfgId"] = comnCnfgId;

            var proc = "GET_COMN_CNFG_REFS";

            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms);
            });
            return list;
        }

       
        public void InsertCntndInCnctCable(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_CNCT_CABL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);

            //await Task.Run(() => { });

        }

        public async Task<List<Hashtable>> GetAuditLog(long id)
        {
            var parms = new Hashtable();
            parms["pComnCnfgId"] = id;

            var proc = "GET_AUDIT_LOG";

            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms);
            });
            return list;
        }

        public void InsertCntndInBulkCable(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_BULK_CABL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInRevsnLvlInd,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);

            //await Task.Run(() => { });
        }
        public void InsertCntndInMnrMtrl(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_MNR_MTRL";
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);

            //await Task.Run(() => { });
        }
        public void InsertCntndInNonRmeMtrl(Hashtable rec)
        {
            var proc = "INSERT_CNFG_CI_NON_RME_MTRL"; // doesn't exists
            var parms = "pComnCnfgDefId,pCntndInId,pCntndInMtrlQty,pCntndInMtrlSprQty";
            ExecNonQueryStoredProc(proc, parms, rec);
            //await Task.Run(() => { });
        }

        public async Task<bool> CLEAN_UP_COMN_CNFG_DEF_SEQS(long pComnCnfgId)
        {
            var proc = "CLEAN_UP_COMN_CNFG_DEF_SEQS"; // doesn't exists
            var query = new Hashtable();
            query["pComnCnfgId"] = pComnCnfgId;

            await Task.Run(() => { 
                ExecNonQueryStoredProc(proc, "pComnCnfgId", query);
            });

            return true;
        }
        public async Task<List<Hashtable>> SEARCH_FOR_PART_CANDIDATES(Hashtable query)
        {
            IAccessor dbm = null;
            IDbDataParameter[] args = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    var parms = "pPartNo,pRcrdOnly,pMtrlCd,pClmc,pMtrlCatId,pFeatTypId,pDsc,pStatus,pCdmmsId,pSpecNm";
                    args = CreateParamsWithRefCursor(dbm, parms, query);
                    reader = dbm.ExecuteDataReaderSP(pkg + ".SEARCH_FOR_PART_CANDIDATES", args);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

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

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }

        public async Task<List<Hashtable>> FIND_RLTDTO_CHLDRN_IN_CNFGS(long prntComnCnfgId, long prntComnCnfgDefId)
        {
            var parms = "pPrntComnCnfgId,pPrntComnCnfgDefId";
            var args = new Hashtable();
            args["pPrntComnCnfgId"] = prntComnCnfgId;
            args["pPrntComnCnfgDefId"] = prntComnCnfgDefId;

            var proc = pkg + ".FIND_RLTDTO_CHLDRN_IN_CNFGS";

            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);

            });
            return list;
        }

        public async Task<List<Hashtable>> FIND_REFS_IN_OTHER_CNFGS(long comnCnfgDefId)
        {
            var parms = "pComnCnfgDefId";
            var args = new Hashtable();
            args["pComnCnfgDefId"] = comnCnfgDefId;

            var proc = pkg + ".FIND_REFS_IN_OTHER_CNFGS";

            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);

            });
            return list;
        }

        public async Task<Hashtable> GetContainedInMinorMaterial(long comnCnfgDefId)
        {
            Hashtable rec = null;
            await Task.Run(() => { rec = this.GetContainedInRecords("GET_CNFG_CI_MNR_MTRL", comnCnfgDefId); });
            return rec;
        }
        public async Task<Hashtable> GetContainedInSpecPart(long comnCnfgDefId)
        {
            Hashtable rec = null;
            await Task.Run(() => { rec = this.GetContainedInRecords("GET_CNFG_CI_SPEC_PART", comnCnfgDefId); });
            return rec;
        }

        
        public async Task<Hashtable> GetContainedInHighLevelPart(long comnCnfgDefId)
        {
            Hashtable rec = null;
            await Task.Run(() => { rec = this.GetContainedInRecords("GET_CNFG_CI_HLP", comnCnfgDefId); });
            return rec;
        }
        public async Task<Hashtable> GetContainedInCommonConfig(long comnCnfgDefId)
        {
            Hashtable rec = null;
            await Task.Run(() => { rec = this.GetContainedInRecords("GET_CNFG_CI_COMN_CNFG", comnCnfgDefId); });
            return rec;
        }
        public Hashtable GetContainedInRecords(string proc, long comnCnfgDefId)
        { 
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            Hashtable rec = null;
            Exception error = null;
            var message = "";

            if( proc.StartsWith(pkg) == false )
            {
                proc = pkg + "." + proc;
            }
            //await Task.Run(() =>
            //{
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parms = dbm.GetParameterArray(2);
                    parms[0] = dbm.GetParameter("pComnCnfgDefId", DbType.Int64, comnCnfgDefId, ParameterDirection.Input);
                    parms[1] = dbm.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbm.ExecuteDataReaderSP(proc, parms);

                    while (reader.Read())
                    {
                        var item = new GenericRec(reader);
                        rec = item.Data;
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
            //});

            if (error != null)
                throw error;

            return rec;
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

        public async Task<List<Hashtable>> GetLocatableItems(long comnCnfgId)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            List<Hashtable> list = new List<Hashtable>();
            Exception error = null;
            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parms = dbm.GetParameterArray(2);
                    parms[0] = dbm.GetParameter("pComnCnfgId", DbType.Int64, comnCnfgId, ParameterDirection.Input);
                    parms[1] = dbm.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbm.ExecuteDataReaderSP(pkg + ".GET_CNFG_CI_LOC_COORDS", parms);

                    while (reader.Read())
                    {
                        var item = new GenericRec(reader);
                        list.Add(item.Data);
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


        public async Task<List<Hashtable>> SEARCH_COMN_CNFG_PARTS(Hashtable query)
        {
            IAccessor dbm = null;
            IDbDataParameter[] args = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    var parms = "pDsc,pSpecNm,pMtlCd,pClmc,pPartNo,pMatCatId,pCdmmsId,pFeatTypId,pStatus,pRcrdOnly";
                    args = CreateStringParamsWithRefCursor(dbm, parms, query);
                    reader = dbm.ExecuteDataReaderSP(pkg + ".SEARCH_COMN_CONFIG_PARTS", args);
                    
                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

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

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }
        public async Task<List<Hashtable>> GetCnfgCiCnfgAllItems(long prntComnCnfgDefId, int lvl, long comnCnfgId)
        {
            List<Hashtable> list = null;

            var proc = "GET_CNFG_CI_CNFG_ALL_ITEMS_V2";

            var parms = "pPrntComnCnfgDefId,pComnCnfgId";
            var args = new Hashtable();
            args["pPrntComnCnfgDefId"] = prntComnCnfgDefId;
            args["pComnCnfgId"] = comnCnfgId;

            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);
            });
            
            return list;
        }
        public async Task<List<Hashtable>> GetCommonConfigDef(long comnCnfgId)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;
            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parms = dbm.GetParameterArray(2);
                    parms[0] = dbm.GetParameter("pComnCnfgId", DbType.Int64, comnCnfgId, ParameterDirection.Input);
                    parms[1] = dbm.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbm.ExecuteDataReaderSP(pkg + ".GET_COMN_CNFG_ALL_ITEMS", parms);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);
                        list.Add(item.Data);
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

   

        public  async Task<List<Hashtable>> GET_PRNT_CANDIDATES(long comnCnfgId)
        {
            var parms = "pComnCnfgId";
            var args = new Hashtable();
            args["pComnCnfgId"] = comnCnfgId;

            var proc = pkg + ".GET_PRNT_CANDIDATES";
            
            var list = new List<Hashtable>();
            await Task.Run(() => {
                list = ExecQueryStoredProc(proc, parms, args);

            });
            return list;
        }

        public async Task<List<Hashtable>> GetCommonConfigDefOld(long comnCnfgId)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parms = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);

                    parms = dbm.GetParameterArray(2);
                    parms[0] = dbm.GetParameter("pComnCnfgId", DbType.Int64, comnCnfgId, ParameterDirection.Input);
                    parms[1] = dbm.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbm.ExecuteDataReaderSP(pkg + ".GET_COMN_CNFG_DEF", parms);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);
                        list.Add(item.Data);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search";

                    error = oe;

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

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }

        public async Task<Hashtable> GET_PRNT_INFO(string prntRefKeyId)
        {
            var proc = "GET_PRNT_INFO";
            var query = new Hashtable();
            query["pPrntRefKey"] = prntRefKeyId;
            var list = new List<Hashtable>();
            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(proc, query);
            });

            if (list.Count == 0)
                return null;

            return list[0];
        }
        public async Task<Hashtable> GET_CI_PRNT_INFO(string prntRefKeyId)
        {
            var query = new Hashtable();
            var parts = prntRefKeyId.Split('.');
            if( parts.Length != 3 )
            {
                return null;
            }

            query["pComnCnfgDefId"] = parts[0];
            query["pComnCiHlpMtrlRevsnDefId"] = parts[1];
            query["pComnCiCnfgDefId"] = parts[2];

            var proc = "GET_CI_PRNT_INFO";
            var parms = "pComnCnfgDefId,pComnCiHlpMtrlRevsnDefId,pComnCiCnfgDefId";
            var list = new List<Hashtable>();
            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(proc, parms, query);
            });

            if (list.Count == 0)
                return null;

            return list[0];
        }
        public async Task<Hashtable> GET_HLPN_OR_CC_PRNT_INFO(long comnCnfgDefId, int lvl, long prntHlpMtrlRevsnDefId, long prntComnCnfgDefId)
        {
            var proc = "GET_HLPN_OR_CC_PRNT_INFO";
            var args = "pComnCnfgDefId,pLvl,pPrntHlpMtrlRevsnDefId,pPrntComnCnfgDefId";

            var query = new Hashtable();
            query["pComnCnfgDefId"] = comnCnfgDefId;
            query["pLvl"] = lvl;
            query["pPrntHlpMtrlRevsnDefId"] = prntHlpMtrlRevsnDefId;
            query["pPrntComnCnfgDefId"] = prntComnCnfgDefId;

            var list = new List<Hashtable>();
            await Task.Run(() =>
            {
                list = ExecQueryStoredProc(proc, args, query);
            });

            if (list.Count == 0)
                return null;

            return list[0];
        }
        public async Task<List<Hashtable>> SEARCH_COMN_CNFG(Hashtable query)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    var parms = "pComnCnfgId,pComnCnfgNm,pComnCnfgDsc,pTmpltTypId,pStatus,pTmpltNm,pExact"; // cos oracle requires named parameters to be in order....
                    parameters = CreateParamsWithRefCursor(dbm, parms, query);
                    reader = dbm.ExecuteDataReaderSP(pkg+".SEARCH_COMN_CNFG", parameters);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);
                        list.Add(item.Data);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search";

                    error = oe;

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

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }
        public async Task<List<Hashtable>> SearchForMaterial(Hashtable query)
        {
            IAccessor dbm = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            //PROCEDURE SEARCH_ALL_PARTS (pComnCnfgDefId IN NUMBER, pCdmmsId IN VARCHAR2, pMtlCd IN VARCHAR2, pPartNo IN VARCHAR2, pClmc in VARCHAR2,
            // pDsc IN VARCHAR2, pCnfgNm IN VARCHAR2, pRoInd IN VARCHAR2, retCsr OUT ref_cursor) AS

            await Task.Run(() =>
            {
                try
                {
                    dbm = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    var parms = "pComnCnfgId,pCdmmsId,pMtrlCd,pPartNo,pClmc,pDsc,pCnfgNm,pRoInd"; // ugg - oracle requires named parameters to be in order...
                    parameters = CreateParamsWithRefCursor(dbm, parms, query);

                    reader = dbm.ExecuteDataReaderSP(pkg+ ".SEARCH_ALL_PARTS", parameters);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);
                        list.Add(item.Data);
                    }
                }
                catch (OracleException oe)
                {
                    string message = "Unable to perform search";

                    error = oe;

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

                    if (dbm != null)
                        dbm.Dispose();
                }
            });

            if (error != null)
                throw error;

            return list;
        }
        public async Task<List<Hashtable>> SEARCH_FOR_SPEC_PART(Hashtable query)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;

            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    var args = "pTmpltTypId,pPartNo,pRcrdOnly,pMtrlCd,pClmc,pMtrlCatId,pFeatTypId,pDsc,pStatus,pCdmmsId,pSpecNm";
                    parameters = CreateStringParamsWithRefCursor(dbManager, args, query);
                    
                    reader = dbManager.ExecuteDataReaderSP(pkg + ".SEARCH_FOR_SPEC_PART", parameters);
                    
                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);

                        list.Add(item.Data);
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

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if ( error != null ) 
                throw new Exception(message+"\n"+error.Message);

            return list;
        }

        public async Task<List<Hashtable>> SearchForCardTemplate(Hashtable query)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;
            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    // pCdmmsId in varchar2, pMtrlCode in varchar2, pPartNo in varchar2, pClmc in varchar2, pCatDescr in varchar2

                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    var args = "CdmmsId,MtrlCode,PartNo,Clmc,CatDescr";
                    parameters = CreateParamsWithRefCursor(dbManager, args, query);

                    reader = dbManager.ExecuteDataReaderSP(pkg+".SEARCH_CARD_TEMPLATES", parameters);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);

                        list.Add(item.Data);
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

                    if (dbManager != null)
                        dbManager.Dispose();
                }
            });

            if( error != null )
                throw new Exception(message+"\n"+error.Message);

            return list;
        }
        public async Task<List<Hashtable>> GetCardTemplate(long id)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<Hashtable> list = null;
            Exception error = null;
            var message = "";

            await Task.Run(() =>
            {
                try
                {
                    // pCdmmsId in varchar2, pMtrlCode in varchar2, pPartNo in varchar2, pClmc in varchar2, pCatDescr in varchar2

                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pID", DbType.Int32, id, ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retCsr", ParameterDirection.Output);

                    reader = dbManager.ExecuteDataReaderSP(pkg + ".GET_CARD_TEMPLATE", parameters);

                    while (reader.Read())
                    {
                        if (list == null)
                            list = new List<Hashtable>();

                        var item = new GenericRec(reader);

                        list.Add(item.Data);
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
                    logger.Error(ex,message);
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
                throw new Exception(message+"\n"+error.Message);

            return list;
        }
        public async Task<List<Option>> GetOptionList(string optionType)
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

                    reader = dbm.ExecuteDataReaderSP(pkg+".GET_OPTION_LIST", parameters);

                    while (reader.Read())
                    {
                        var item = new Option();
                        list.Add(Copy(item, reader));
                    }
                }
                catch (OracleException oe)
                {
                    error = oe;

                    message = "Unable to perform search: GET_OPTION_LIST for "+ optionType;
                    logger.Error(oe, message);
                    EventLogger.LogAlarm(oe, message, SentryIdentifier.EmailDev, SentrySeverity.Major);
                }
                catch (Exception ex)
                {
                    error = ex;

                    message = "Unable to perform search: GET_OPTION_LIST for " + optionType;
                    logger.Error(ex,message);
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

        public async Task<List<string>> GetDefIDs(string materialItemID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<string> defIDs = new List<string>();
            Exception error = null;

            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMaterialItemId", DbType.Int64, long.Parse(materialItemID), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("comn_cnfg_pkg.get_ci_def_id_list", parameters);
                    while (reader.Read())
                    {
                        defIDs.Add(reader["comn_cnfg_def_id"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get def IDs ({0})", materialItemID);
                    error = ex;
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
                if( error != null )
                {
                    throw error;
                }
            });
            return defIDs;
        }
        public async Task<List<string>> GetConfigNames(string materialItemID)
        {
            IAccessor dbManager = null;
            IDbDataParameter[] parameters = null;
            IDataReader reader = null;
            List<string> configNames = new List<string>();
            Exception error = null;
            await Task.Run(() =>
            {
                try
                {
                    dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, connectionString);
                    parameters = dbManager.GetParameterArray(2);
                    parameters[0] = dbManager.GetParameter("pMaterialItemId", DbType.Int64, long.Parse(materialItemID), ParameterDirection.Input);
                    parameters[1] = dbManager.GetParameterCursorType("retcsr", ParameterDirection.Output);
                    reader = dbManager.ExecuteDataReaderSP("comn_cnfg_pkg.get_ci_def_id_list", parameters);
                    while (reader.Read())
                    {
                        configNames.Add(reader["comn_cnfg_nm"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get Config Names ({0})", materialItemID);
                    error = ex;
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
                if( error != null )
                {
                    throw error;
                }
            });
            return configNames;
        }
    }
}