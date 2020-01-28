using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog; 
using System.Collections;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using System.Text;
using System.Diagnostics;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    public static class ExtensionMethods
    {
        public static Hashtable ToHashtable(this String str, char delim=',')
        {
            var o = new Hashtable();
            var list = str.Split(delim);
            foreach(var item in list)
            {
                o[item] = "";
            }
            return o;
        }
        public static OrderedDictionary ToOrderedDictionary(this String str, char delim = ',')
        {
            var o = new OrderedDictionary();
            var list = str.Split(delim);
            for( var i = 0; i < list.Length; i++ )
            {
                o.Insert(i, list[i], "");
            }
            return o;
        }
    }
    [RoutePrefix("api/comncnfg")]
    public class CommonConfigController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region PRIVATE METHODS
        private void Merge(Hashtable dst, Hashtable src)
        {
            foreach (DictionaryEntry e in src)
            {
                dst[e.Key] = e.Value;
            }
        }
        private bool HasChanged(Hashtable rec)
        {
            bool rv = false;
            if (rec.Contains("_hasChanged"))
            {
                Boolean.TryParse((rec["_hasChanged"] ?? "0").ToString(), out rv);
            }
            return rv;
        }
        private long UpdateComnCnfg(string cuid, Hashtable rec)
        {
            long comnCnfgId = 0L;
            long.TryParse((rec["COMN_CNFG_ID"] ?? "0").ToString(), out comnCnfgId);

            if (HasChanged(rec) == false)
                return comnCnfgId;

            var action = (comnCnfgId > 0 ? "C" : "A");
            if( (rec["DEL_IND"]??"N").ToString() == "Y" )
            {
                action = "D";
            }

            var deactivate = ((rec["RET_COMN_CNFG_IND"]??"N").ToString() == "Y");

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            dbi.UpdateAppLog("UpdateCommonConfigItems", cuid, rec);

            var args = ConvertToParams(rec);
            comnCnfgId = dbi.UpdateCommonConfig(args);

            if (action == "A" || action == "D") 
            {
                var errors = new List<Exception>();
                var log = new List<Hashtable>();
                var audit = new Hashtable();

                audit["comnCnfgId"] = comnCnfgId;
                audit["comnCnfgDefId"] = 0;
                audit["id"] = comnCnfgId;
                audit["dsc"] = (rec["COMN_CNFG_NM"] ?? "").ToString();
                audit["tbl"] = "comn_cnfg";
                audit["col"] = "comn_cnfg_nm"; // thought this should be comn_cnfg_id but that col isn't registered
                audit["oldVal"] = "";
                audit["newVal"] = "";
                audit["action"] = action;

                log.Add(audit);

                UpdateAuditLog(cuid, log, errors);
            }

            if( action == "D" || deactivate )
            {
                dbi.UPDATE_AUDIT_LOG_RELTD_CNFGS(cuid, comnCnfgId, (deactivate ? "DEACTIVATE" : "DELETE"));
            }
            return comnCnfgId;
        }

        private Hashtable ConvertToParams(Hashtable obj)
        {
            var rv = new Hashtable();
            foreach (string k in obj.Keys)
            {
                if (k.StartsWith("_"))
                    continue;

                var p = "p" + ConvertToCamelCase(k);
                rv[p] = (obj[k] ?? "").ToString();
            }
            return rv;
        }

        private string ConvertToCamelCase(string str)
        {
            var s = str.ToLower().Replace('_', ' ');
            var ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            var ti = ci.TextInfo;

            return ti.ToTitleCase(s).Replace(" ", string.Empty);
        }

        private async Task<bool> RemoveDeletedChildItems(string cuid, long comnCnfgId, JArray list, List<Exception> errors)
        {
            if (list.Count() == 0)
                return true;

            var log = new List<Hashtable>();

            var procs = new Dictionary<string, string>();
            procs["BAY"] = "DELETE_CNFG_CI_BAY_MTRL";
            procs["BAY EXTENDER"] = "DELETE_CNFG_CI_BAY_EXTND";
            procs["NODE"] = "DELETE_CNFG_CI_NODE_MTRL";
            procs["SHELF"] = "DELETE_CNFG_CI_SHELF_MTRL";
            procs["CARD"] = "DELETE_CNFG_CI_CARD_MTRL";
            procs["PLUG-IN"] = "DELETE_CNFG_CI_PLG_IN_MTRL";
            procs["COMMON CONFIG"] = "DELETE_CNFG_CI_COMN_CNFG";
            procs["HIGH LEVEL PART"] = "DELETE_CNFG_CI_HLP_MTRL";
            procs["MINOR MATERIAL"] = "DELETE_CNFG_CI_MNR_MTRL";
            procs["CONNECTORIZED/SET LENGTH"] = "DELETE_CNFG_CI_CNTC_CABL";
            procs["BULK"] = "DELETE_CNFG_CI_BULK_CABL";

            var deleteCiNonRmeMtrl = "DELETE_CNFG_CI_NON_RME_MTRL";

            // ugg....
            procs["COPPER TERMINAL"] = deleteCiNonRmeMtrl;
            procs["CONDUIT"] = deleteCiNonRmeMtrl;
            procs["FIBER TERMINAL"] = deleteCiNonRmeMtrl;
            procs["REPEATER"] = deleteCiNonRmeMtrl;
            procs["RISER"] = deleteCiNonRmeMtrl;
            procs["COPPER SPLICE"] = deleteCiNonRmeMtrl;
            procs["FIGURE EIGHT"] = deleteCiNonRmeMtrl;
            procs["CABLE"] = deleteCiNonRmeMtrl;
            procs["ANCHOR"] = deleteCiNonRmeMtrl;
            procs["LOAD COIL"] = deleteCiNonRmeMtrl;
            procs["SPLICE TRAY"] = deleteCiNonRmeMtrl;
            procs["TERMINAL ENCLOSURE"] = deleteCiNonRmeMtrl;
            procs["OPTICAL SPLITTER"] = deleteCiNonRmeMtrl;
            procs["UNDERGROUND UTILITY BOX"] = deleteCiNonRmeMtrl;
            procs["POLE"] = deleteCiNonRmeMtrl;
            procs["CAPACITOR"] = deleteCiNonRmeMtrl;
            procs["POWER TERMINAL"] = deleteCiNonRmeMtrl;
            procs["SHEATH SPLICE"] = deleteCiNonRmeMtrl;
            procs["NON-RME"] = deleteCiNonRmeMtrl;

            var doDelete = true;
            var comnCnfgDefId = 0L;
            var cntndInId = 0L;

            //
            // mwj: personally, I think the audit logging should occur in the delete proc
            // but we'd need to update all the procs to take a CUID so we can track who 
            // made the change, that's too big of a change for now
            //
            // so instead, I'm just going to collect a list of parent types that were deleted
            // then iterate through that list to removet the related-to relationships
            // and log that transaction...
            //
            var prntTyps = "BAY,NODE,SHELF,CARD,COMMON CONFIG,HIGH LEVEL PART".Split(',');
            var prnts = new List<Hashtable>();

            var toDelete = new List<Hashtable>();

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            foreach (JObject item in list)
            {
                var rec = item.ToObject<Hashtable>();

                long.TryParse((rec["COMN_CNFG_DEF_ID"] ?? "0").ToString(), out comnCnfgDefId);
                long.TryParse((rec["CNTND_IN_ID"] ?? rec["CDMMS_ID"] ?? "0").ToString(), out cntndInId);

                var featTypId = (rec["CNTND_IN_FEAT_TYP_ID"] ?? rec["FEAT_TYP_ID"] ?? "0").ToString().Trim().ToUpper();
                var mtrlCatId = (rec["CNTND_IN_MTRL_CAT_ID"] ?? rec["MTRL_CAT_ID"] ?? "0").ToString().Trim().ToUpper();
                var featTyp = (rec["FEAT_TYP"] ?? "").ToString().Trim().ToUpper();
                var mtrlCatTyp = (rec["MTRL_CAT_TYP"] ?? "").ToString().Trim().ToUpper();
                var mtrlTyp = (rec["MTRL_TYP"] ?? "").ToString().ToUpper();

                var chldTyp = (mtrlTyp == "MINOR MATERIAL" || mtrlTyp == "NON-RME" 
                              ? mtrlTyp
                              : (featTyp.Length == 0 || featTyp == "GENERIC" 
                                ? mtrlCatTyp
                                  : featTyp
                                )
                                  );

                var args = ConvertToParams(rec);
                args["pCntndInFeatTypId"] = featTypId;
                args["pCntndInMtrlCatId"] = mtrlCatId;

                try
                {
                    
                    if (chldTyp.Length > 0)
                    {
                        var proc = (procs[chldTyp] ?? null);
                        if (proc == null)
                        {
                            errors.Add(new Exception("RemoveDeletedChildItems: unknown childTyp - " + chldTyp));
                            continue;
                        }
                        if (prntTyps.Contains(chldTyp))
                        {
                            //prnts.Add(args);
                            await RemoveRelatedToParentReferences(cuid, args);
                        }

                        dbi.DeleteCntndInItem(proc, comnCnfgDefId, cntndInId);
                        if (chldTyp == "NODE" || chldTyp == "SHELF")
                        {
                            dbi.DeleteCntndInLocItem(chldTyp, comnCnfgDefId, cntndInId);
                        }
                        if (chldTyp == "HIGH LEVEL PART" || chldTyp == "COMMON CONFIG")
                        {
                            dbi.DeleteCntndInDefItem(chldTyp, comnCnfgDefId, cntndInId);
                        }
                    }
                    dbi.UpdateComnCnfgDefItem(args, doDelete);

                    var audit = new Hashtable();
                    audit["comnCnfgId"] = comnCnfgId;
                    audit["comnCnfgDefId"] = comnCnfgDefId;
                    audit["id"] = cntndInId;
                    audit["dsc"] = CreateFullDescr(rec);
                    audit["tbl"] = (rec["CDMMS_COMN_CNFG_CNTD_IN_TBL_NM"] ?? "").ToString();
                    audit["col"] = "comn_cnfg_def_id";
                    audit["oldVal"] = "";
                    audit["newVal"] = "";
                    audit["action"] = "D";

                    log.Add(audit);

                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            //foreach( Hashtable prnt in prnts )
            //{
            //    await RemoveRelatedToParentReferences(cuid, prnt);
            //}

            UpdateAuditLog(cuid, log, errors);

            return (errors.Count() == 0);
        }

        private async Task<bool> RemoveRelatedToParentReferences(string cuid, Hashtable prnt)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            // this will get the audit log lines...
            var log = await dbi.GET_RLTDTO_PRNT_REFS(cuid, prnt);
            foreach( Hashtable item in log )
            {
                System.Diagnostics.Trace.WriteLine(item["CMNT_TXT"].ToString());
                dbi.UPDATE_AUDIT_LOG_RAW(item);
            }

            // this will do the actuall removals...
            var finished = await dbi.REMOVE_RLTDTO_PRNT_REFS(cuid, prnt);

            return true;
        }

        private string CreateFullDescr(Hashtable rec)
        {
            var seqNo = (rec["CNTND_IN_SEQ_NO"] ?? "").ToString();
            var feat = (rec["FEAT_TYP"] ?? "").ToString();
            var mtrl = (rec["MTRL_CAT_TYP"] ?? "").ToString();
            var dsc = (rec["RT_PART_NO"] ?? rec["MTRL_DSC"] ?? rec["COMN_CNFG_NM"] ?? "").ToString();
            var typ = (feat.Length == 0 ? mtrl : feat);

            var descr = String.Format("item #{0}, {1} \"{2}\"", seqNo, typ, dsc);
            return descr;
        }
        private bool AddNewChildItems(string cuid, long comnCnfgId, JArray list, Hashtable mapComnCnfgDefIdToGuid, List<Exception> errors)
        {
            if (list.Count() == 0)
                return true;

            var log = new List<Hashtable>();

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            foreach (JObject item in list)
            {
                try
                {
                    var rec = item.ToObject<Hashtable>();

                    var guid = (rec["_guid"] ?? "").ToString().ToLower();

                    var featTypId = (rec["CNTND_IN_FEAT_TYP_ID"] ?? rec["FEAT_TYP_ID"] ?? "").ToString().ToUpper();
                    var mtrlCatId = (rec["CNTND_IN_MTRL_CAT_ID"] ?? rec["MTRL_CAT_ID"] ?? "").ToString().ToUpper();
                    var featTyp = (rec["FEAT_TYP"] ?? "").ToString().ToUpper();
                    var mtrlCatTyp = (rec["MTRL_CAT_TYP"] ?? "").ToString().ToUpper();
                    var mtrlTyp = (rec["MTRL_TYP"] ?? "").ToString().ToUpper();

                    var childTyp = ( mtrlTyp == "MINOR MATERIAL" || mtrlTyp == "NON-RME" ? mtrlTyp 
                                   : featTyp.Length == 0         || featTyp == "GENERIC" ? mtrlCatTyp 
                                   : featTyp
                                   );

                    rec["COMN_CNFG_ID"] = comnCnfgId;

                    var args = ConvertToParams(rec);
                    args["pCntndInFeatTypId"] = featTypId;
                    args["pCntndInMtrlCatId"] = mtrlCatId;

                    var comnCnfgDefId = dbi.UpdateComnCnfgDefItem(args);
                    if (comnCnfgDefId > 0)
                    {
                        args["pComnCnfgDefId"] = comnCnfgDefId;
                        mapComnCnfgDefIdToGuid[guid] = comnCnfgDefId;
                    }
                    switch (childTyp)
                    {
                        case "BAY"          : dbi.INSERT_CNFG_CI_BAY_MTRL(args);    break;
                        case "BAY EXTENDER" : dbi.INSERT_CNFG_CI_BAY_EXTND(args);   break;
                        case "NODE"         : dbi.INSERT_CNFG_CI_NODE_MTRL(args);   break;
                        case "SHELF"        : dbi.INSERT_CNFG_CI_SHELF_MTRL(args);  break;
                        case "CARD"         : dbi.INSERT_CNFG_CI_CARD_MTRL(args);   break;
                        case "PLUG-IN"      : dbi.INSERT_CNFG_CI_PLG_IN_MTRL(args); break;

                        case "COMMON CONFIG": dbi.InsertCntndInComnCnfg(args); break;
                        case "HIGH LEVEL PART": dbi.InsertCntndInHighLevelPart(args); break;
                        case "MINOR MATERIAL": dbi.InsertCntndInMnrMtrl(args); break;
                        case "CONNECTORIZED/SET LENGTH": dbi.InsertCntndInCnctCable(args); break;
                        case "BULK": dbi.InsertCntndInBulkCable(args); break;

                        // ugg....
                        case "COPPER TERMINAL": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "CONDUIT": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "FIBER TERMINAL": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "REPEATER": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "RISER": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "COPPER SPLICE": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "FIGURE EIGHT": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "CABLE": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "ANCHOR": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "LOAD COIL": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "SPLICE TRAY": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "TERMINAL ENCLOSURE": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "OPTICAL SPLITTER": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "UNDERGROUND UTILITY BOX": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "POLE": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "CAPACITOR": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "POWER TERMINAL": dbi.InsertCntndInNonRmeMtrl(args); break;
                        case "SHEATH SPLICE": dbi.InsertCntndInNonRmeMtrl(args); break;

                        case "NON-RME": dbi.InsertCntndInNonRmeMtrl(args); break;
                        default:
                            errors.Add(new Exception("AddNewChildItems: unknown childTyp - " + childTyp));
                            break;
                    }

                    SyncOtherConfigWithNewItem(comnCnfgDefId, childTyp);

                    var audit = new Hashtable();
                    audit["comnCnfgId"] = comnCnfgId;
                    audit["comnCnfgDefId"] = comnCnfgDefId;
                    audit["id"] = "0"; // don't know...
                    audit["dsc"] = (rec["RT_PART_NO"] ?? rec["MTRL_DSC"] ?? "").ToString();
                    audit["tbl"] = (rec["CDMMS_COMN_CNFG_CNTD_IN_TBL_NM"] ?? "").ToString();
                    audit["col"] = "comn_cnfg_def_id";
                    audit["oldVal"] = "";
                    audit["newVal"] = "";
                    audit["action"] = "A";

                    //{ "id":"0"
                    //, "action":"A"
                    //, "comnCnfgDefId":10738
                    //, "tbl":""
                    //, "dsc":"SI10A120PME"
                    //, "newVal":""
                    //, "comnCnfgId":9218
                    //, "col":"comn_cnfg_def_id"
                    //, "oldVal":""
                    //}
                    //COMN_CNFG_PKG.UPDATE_AUDIT_LOG: unable to perform update (db error)

                    // TODO: audit.tbl === "" so proc probably needs to find the mtrl/feat typ and determine
                    // what the table is based on that...

                    log.Add(audit);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            UpdateAuditLog(cuid, log, errors);

            return (errors.Count() == 0);
        }

        private async Task<bool> SyncOtherConfigWithNewItem(long comnCnfgDefId, string childTyp)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            var rv = await dbi.SYNC_OTHER_CNFGS_WITH_NEW_ITEM(comnCnfgDefId, childTyp);
            return rv;
        }

        private bool UpdateChildItems(string cuid, long comnCnfgId, JArray list, List<Exception> errors)
        {
            if (list.Count() == 0)
                return true;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            foreach (JObject item in list)
            {
                try
                {
                    var rec = item.ToObject<Hashtable>();

                    var featTypId = (rec["CNTND_IN_FEAT_TYP_ID"] ?? rec["FEAT_TYP_ID"] ?? "").ToString().Trim().ToUpper();
                    var mtrlCatId = (rec["CNTND_IN_MTRL_CAT_ID"] ?? rec["MTRL_CAT_ID"] ?? "").ToString().Trim().ToUpper();
                    var featTyp = (rec["CNTND_IN_FEAT_TYP"] ?? rec["FEAT_TYP"] ?? "").ToString().Trim().ToUpper();
                    var mtrlCatTyp = (rec["CNTND_IN_MTRL_CAT"] ?? rec["MTRL_CAT_TYP"] ?? "").ToString().Trim().ToUpper();
                    var mtrlTyp = (rec["MTRL_TYP"] ?? "").ToString().ToUpper();

                    var childTyp = (mtrlTyp == "MINOR MATERIAL" || mtrlTyp == "NON-RME" ? mtrlTyp
                                  : featTyp.Length == 0 || featTyp == "GENERIC" ? mtrlCatTyp
                                  : featTyp
                                  );

                    var args = ConvertToParams(rec);
                    args["pCntndInFeatTypId"] = featTypId;
                    args["pCntndInMtrlCatId"] = mtrlCatId;

                    var comnCnfgDefId = dbi.UpdateComnCnfgDefItem(args);
                    switch (childTyp)
                    {
                        case "BAY"                      : dbi.UPDATE_CNFG_CI_BAY_MTRL(args);    break;
                        case "BAY EXTENDER"             : dbi.UPDATE_CNFG_CI_BAY_EXTND(args);   break;
                        case "NODE"                     : dbi.UPDATE_CNFG_CI_NODE_MTRL(args);   break; // also calls UPDATE_CNFG_CI_NODE_LOC
                        case "SHELF"                    : dbi.UPDATE_CNFG_CI_SHELF_MTRL(args);  break; // also calls UPDATE_CNFG_CI_SHELF_LOC
                        case "CARD"                     : dbi.UPDATE_CNFG_CI_CARD_MTRL(args);   break;
                        case "PLUG-IN"                  : dbi.UPDATE_CNFG_CI_PLG_IN_MTRL(args); break;

                        case "COMMON CONFIG": dbi.UpdateCntndInComnCnfg(args); break;
                        case "HIGH LEVEL PART": dbi.UpdateCntndInHighLevelPart(args); break;
                        case "MINOR MATERIAL": dbi.UpdateCntndInMnrMtrl(args); break;
                        case "BULK"                     : dbi.UpdateCntndInBulkCable(args);     break;
                        case "CONNECTORIZED/SET LENGTH": dbi.UpdateCntndInCnctCable(args); break;
                        

                        // ugg....
                        case "COPPER TERMINAL": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "CONDUIT": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "FIBER TERMINAL": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "REPEATER": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "RISER": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "COPPER SPLICE": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "FIGURE EIGHT": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "CABLE": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "ANCHOR": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "LOAD COIL": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "SPLICE TRAY": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "TERMINAL ENCLOSURE": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "OPTICAL SPLITTER": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "UNDERGROUND UTILITY BOX": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "POLE": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "CAPACITOR": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "POWER TERMINAL": dbi.UpdateCntndInNonRmeMtrl(args); break;
                        case "SHEATH SPLICE": dbi.UpdateCntndInNonRmeMtrl(args); break;

                        case "NON-RME": dbi.UpdateCntndInNonRmeMtrl(args); break;

                        default: // NON_RME_MTRL
                            errors.Add(new Exception("UpdateChildItems: unknown childTyp - " + childTyp));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
            return (errors.Count() == 0);
        }
        private void UpdateAuditLog(string cuid, List<Hashtable> list, List<Exception> errors)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            var auditLog = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json);

            UpdateAuditLog(cuid, auditLog, errors);
        }
        private void UpdateAuditLog(string cuid, JArray list, List<Exception> errors)
        {
            if (list.Count() == 0)
                return;

            //long   AuditColumnDefId;
            //string AuditTablePKColumnName;
            //string AuditTablePKColumnValue;
            //string AuditParentTablePKColumnName;
            //string AuditParentTablePKColumnValue;
            //string ActionCode;
            //string OldColumnValue;
            //string NewColumnValue;
            //string CUID;
            //string CommentText;


            //var args = "pCUID,pAction,pComnCnfgId,pComnCnfgDefId,pId,pItemDsc,pTblNm,pColNm,pOldVal,pNewVal";
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();

            var entry = new Hashtable();
            entry["pCUID"] = cuid;


            foreach (JObject item in list)
            {
                var rec = item.ToObject<Hashtable>();

                //pCUID varchar2
                //    , pAction varchar2
                //    , pComnCnfgId number
                //    , pComnCnfgDefId number
                //    , pId number
                //    , pItemDsc varchar2
                //    , pTblNm varchar2
                //    , pColNm varchar2
                //    , pOldVal varchar2
                //    , pNewVal varchar2

                entry["pComnCnfgId"] = (rec["comnCnfgId"] ?? "0").ToString();
                entry["pComnCnfgDefId"] = (rec["comnCnfgDefId"] ?? "0").ToString();
                entry["pId"] = (rec["id"] ?? "").ToString();
                entry["pItemDsc"] = (rec["dsc"] ?? "0").ToString();
                entry["pTblNm"] = (rec["tbl"] ?? "").ToString().ToLower();
                entry["pColNm"] = (rec["col"] ?? "").ToString();
                entry["pOldVal"] = (rec["oldVal"] ?? "").ToString();
                entry["pNewVal"] = (rec["newVal"] ?? "").ToString();
                entry["pAction"] = (rec["action"] ?? "C").ToString();

                var defTyp = (rec["_cntndInDefTyp"] ?? "").ToString().ToUpper();
                entry["pCntndInDefTyp"] = (defTyp == "" ? "" : (defTyp.Contains("HIGH LEVEL PART") ? "HLP" : "CC"));
                entry["pCntndInDefId"]  = (rec["HLP_MTRL_REVSN_DEF_ID"] // set if newly added
                                            ?? rec["CNTND_IN_HLP_MTRL_REVSN_DEF_ID"] // set if existing
                                            ?? rec["CNTND_IN_COMN_CNFG_DEF_ID"] // set either way (new or existing)...
                                            ?? "-1")
                                            .ToString()
                                          ;

                
                try
                {
                    dbi.UpdateAuditLog(entry);
                }
                catch (Exception ex)
                {
                    var log = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                    errors.Add(new Exception(log+"\n"+ex.Message));
                }
            }
        }
        #endregion PRIVATE METHODS

        [HttpPost]
        [Route("clone")]
        public async Task<IHttpActionResult> Clone()
        {
            var json = "";
            var status = new Hashtable();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            long comnCnfgId = 0;

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to parse JSON for CommonConfig.Clone");

                return InternalServerError(ex);
            }

            try
            {
                var data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                var args = ConvertToParams(data.ToObject<Hashtable>());

                comnCnfgId = await dbi.CloneConfig(args);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to clone common config record #{0} from JSON: {1}", comnCnfgId, json);

                status["EC"] = -1;
                status["MSG"] = ex.Message;
                status["STACK_TRACE"] = ex.StackTrace;
                status["OV"] = comnCnfgId;

                return Ok(status);
            }

            status["EC"] = 0;
            status["MSG"] = "SUCCESS";
            status["OV"] = comnCnfgId;

            try
            {
                if( comnCnfgId > 0)
                {
                    var query = new Hashtable();
                    query["pComnCnfgId"] = comnCnfgId;

                    var results = new List<Hashtable>();
                    results = await dbi.SEARCH_COMN_CNFG(query);
                    if( results.Count > 0 )
                    {
                        status["REC"] = Newtonsoft.Json.JsonConvert.SerializeObject(results[0]);
                    }
                }
            }
            catch( Exception ex)
            {
                logger.Error(ex, "Failed to fetch common config record for Clone function: "+comnCnfgId.ToString());

                status["EC"] = -1;
                status["MSG"] = ex.Message;
                status["STACK_TRACE"] = ex.StackTrace;
                status["OV"] = comnCnfgId;

                return Ok(status);
            }
            
            return Ok(status);
        }
        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateCommonConfigItems()
        {
            var json = "";
            var status = new Hashtable();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            long comnCnfgId = 0;

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to parse JSON for CommonConfig.UpdateCommonConfigItems");

                return InternalServerError(ex);
            }

            var mapComnCnfgDefIdToGuid = new Hashtable();
            var warnings = new List<Exception>();

            var d = false;
            var a = false;
            var u = false;
            var c = false; // child items...
            try
            {
                var data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                
                var cuid = data.Value<string>("cuid");

                var comnCnfg = data["comnCnfg"].ToObject<Hashtable>();
                var items = (Newtonsoft.Json.Linq.JArray)data["items"];
                var added = (Newtonsoft.Json.Linq.JArray)data["added"];
                var changed = (Newtonsoft.Json.Linq.JArray)data["changed"];
                var deleted = (Newtonsoft.Json.Linq.JArray)data["deleted"];
                var cntndIn = (Newtonsoft.Json.Linq.JArray)data["cntndInChildItems"];
                var log = (Newtonsoft.Json.Linq.JArray)data["auditlog"];

                long.TryParse((comnCnfg.Contains("COMN_CNFG_ID") ? (comnCnfg["COMN_CNFG_ID"] ?? "0").ToString() : "0"), out comnCnfgId);

                comnCnfg["COMN_CNFG_ID"] = comnCnfgId;

                System.Diagnostics.Trace.WriteLine("cuid: "+cuid);
                System.Diagnostics.Trace.WriteLine("items: " + items.Count());
                System.Diagnostics.Trace.WriteLine("added: " + added.Count());
                System.Diagnostics.Trace.WriteLine("changed: " + changed.Count());
                System.Diagnostics.Trace.WriteLine("deleted: " +deleted.Count());
                System.Diagnostics.Trace.WriteLine("log: " + log.Count());

                comnCnfgId = UpdateComnCnfg(cuid, comnCnfg);

                d = await RemoveDeletedChildItems(cuid, comnCnfgId, deleted, warnings);
                a = AddNewChildItems(cuid, comnCnfgId, added, mapComnCnfgDefIdToGuid, warnings);
                u = UpdateChildItems(cuid, comnCnfgId, changed, warnings);
                c = UpdateCntndInChildItems(cuid, comnCnfgId, cntndIn, mapComnCnfgDefIdToGuid, warnings);

                await dbi.CLEAN_UP_COMN_CNFG_DEF_SEQS(comnCnfgId);

                UpdateAuditLog(cuid, log, warnings); // changes...
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CommonConfig.UpdateCommonConfigItems - Unable to persist common config record #{0} from JSON: {1}", comnCnfgId, json);

                status["EC"] = -1;
                status["MSG"] = ex.Message;
                status["STACK_TRACE"] = ex.StackTrace;
                status["OV"] = comnCnfgId;

                return Ok(status);
            }

            status["EC"] = 0;
            status["MSG"] = "SUCCESS";
            status["OV"] = comnCnfgId;

            var warns = new StringBuilder();
            foreach( var w in warnings)
            {
                warns.AppendFormat("{0}|", w.Message);
            }
            status["WARNINGS"] = warns.ToString();

            return Ok(status);
        }

        private bool UpdateCntndInChildItems(string cuid, long comnCnfgId, JArray list, Hashtable mapComnCnfgDefIdToGuid, List<Exception> errors)
        {
            if (list.Count() == 0)
                return true;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            foreach (JObject item in list)
            {
                try
                {
                    var rec = item.ToObject<Hashtable>();
                    var tbl = (rec["CDMMS_COMN_CNFG_CNTD_IN_TBL_NM"] ?? "").ToString().Trim().ToLower();
                    var guid = (rec["_parentGUID"] ?? "").ToString().ToLower();
                    var id = Int64.Parse((mapComnCnfgDefIdToGuid.Contains(guid) ? mapComnCnfgDefIdToGuid[guid] : "-1").ToString());
                    //var isNew = false;
                    var args = ConvertToParams(rec);
                    switch (tbl)
                    {
                        case "comn_cnfg_cntnd_hlp_mtrl_def":
                            var comnCnfgDefId = Int64.Parse((rec["COMN_CNFG_DEF_ID"] ?? "-1").ToString());
                            if (comnCnfgDefId <= 0 )
                            {
                                // this might not be set if we have added a new child item, so grab the id from the guid list...
                                args["pComnCnfgDefId"] = id;
                                //isNew = true;
                            }
                            dbi.UpdateCntndInHlpChildItem(args);
                            break;
                        default:
                            var cntndInCcDfId = Int64.Parse((rec["COMN_CNFG_CNTD_COMN_CNFG_DF_ID"] ?? "-1").ToString());
                            if (cntndInCcDfId <= 0)
                            {
                                // this might not be set if we have added a new child item, so grab the id from the guid list...
                                args["pComnCnfgDefId"] = id;
                                //isNew = true;
                            }
                            dbi.UpdateCntndInCcChildItem(args);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
            return (errors.Count() == 0);
        }
        [HttpPost]
        [Route("update/item")]
        public async Task<IHttpActionResult> UpdateCommonConfigItem()
        {
            var json = "";
            var status = new Hashtable();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            long comnCnfgDefId = 0;

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to parse JSON for Common Config record");

                return InternalServerError(ex);
            }

            try
            {
                //item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                //long.TryParse((string)item.SelectToken("pComnCnfgId"), out comnCnfgId);

                Hashtable rec = JsonConvert.DeserializeObject<Hashtable>(json);
                //status = await dbi.UpdateCommonConfig(rec);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CommonConfig.UpdateCommonConfigItem - Unable to persist common config record #{0} from JSON: {1}", comnCnfgDefId, json);

                status["EC"] = -1;
                status["MSG"] = ex.Message;
                status["OV"] = comnCnfgDefId;

                status["Status"] = "Error";
                status["Id"] = comnCnfgDefId;
                status["Error"] = ex.Message;

                return Ok(status);
            }

            status["EC"] = 0;
            status["MSG"] = "SUCCESS";
            status["OV"] = comnCnfgDefId;

            status["Status"] = "Success";
            status["Id"] = comnCnfgDefId;

            return Ok(status);
        }

        [HttpGet]
        [Route("search/exact")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchExact()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...

            var args = new Hashtable();
            if (query == null || query.Count() == 0)
            {
                logger.Info("CommonConfig.SearchAll - Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                dbi.UpdateAppLog("SearchAll()", "SYS", args);
                args["pExact"] = "Y";
                items = await dbi.SEARCH_COMN_CNFG(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.SearchAll");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("search/all")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchAll()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...

            var args = new Hashtable();
            if ( query == null || query.Count() == 0 ) {
                logger.Info("CommonConfig.SearchAll - Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }
            
            try
            {
                dbi.UpdateAppLog("SearchAll()", "SYS", args);
                args["pExact"] = "N";
                items = await dbi.SEARCH_COMN_CNFG(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.SearchAll");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("search/parts")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchForAssociatedParts()
        {
            ///
            // this methods searches for parts tied to a common config to 
            // return a list of common configs for the user to choose from...
            //
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...

            var args = new Hashtable();
            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                items = await dbi.SEARCH_COMN_CNFG_PARTS(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.SearchForAssociatedParts");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("search/specparts")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchForSpecParts()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...

            var args = new Hashtable();
            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                dbi.UpdateAppLog("SearchForSpecParts()", "SYS", args);
                items = await dbi.SEARCH_FOR_SPEC_PART(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.SearchForSpecParts");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("findReferencesInOtherConfigs/{comnCnfgDefId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> findRelatedToChildrenInOtherConfigs(long comnCnfgDefId)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            var list = new List<Hashtable>();

            try
            {
                list = await dbi.FIND_REFS_IN_OTHER_CNFGS(comnCnfgDefId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var msg = String.Format("Search failed for CommonConfig.findReferencesInOtherConfigs({0})", comnCnfgDefId);
                logger.Error(ex, msg);
                return InternalServerError();
            }
        }
        [HttpGet]
        [Route("findRelatedToChildrenInOtherConfigs/{prntComnCnfgId}/{prntComnCnfgDefId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> findRelatedToChildrenInOtherConfigs(long prntComnCnfgId, long prntComnCnfgDefId)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            var list = new List<Hashtable>();

            try
            {
                list = await dbi.FIND_RLTDTO_CHLDRN_IN_CNFGS(prntComnCnfgId, prntComnCnfgDefId);
                return Ok(list);
            }
            catch( Exception ex )
            {
                var msg = String.Format("Search failed for CommonConfig.findRelatedToChildrenInOtherConfigs({0},{1})", prntComnCnfgId, prntComnCnfgDefId);
                logger.Error(ex, msg);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("export/{comnCnfgId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> ExportComnCnfg(long comnCnfgId)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            var masterList = new List<Hashtable>();

            try
            {
                var collection = new Hashtable();
                var items = await dbi.GetCommonConfigDef(comnCnfgId);
                await TraverseAndCollectContainedInItems(comnCnfgId, items, collection, 0);
                
                BuildListForExport(masterList, items, collection, "", -1);

                if (masterList.Count() > 0)
                {
                    foreach( Hashtable item in masterList )
                    {
                        item["SPEC_NM"] = (item["SPEC_NM"] ?? "").ToString(); // make sure it's not undefined...

                        // pick up parents info...
                        item["PRNT_COMN_CNFG_DEF_ID"] = (item["CNTND_IN_PRNT_DEF_ID"] ?? item["PRNT_COMN_CNFG_DEF_ID"] ?? "0");
                        item["Y_COORD_NO"]      = (item["CNTND_IN_PRNT_Y_COORD_NO"] ?? item["Y_COORD_NO"]);
                        item["X_COORD_NO"]      = (item["CNTND_IN_PRNT_X_COORD_NO"] ?? item["X_COORD_NO"]);
                        item["FRNT_RER_IND"]    = (item["CNTND_IN_PRNT_FRNT_RER_IND"] ?? item["FRNT_RER_IND"]);
                        item["LABEL_NM"]        = (item["CNTND_IN_PRNT_LABEL_NM"] ?? item["LABEL_NM"]);
                        item["RACK_POS"]        = (item["CNTND_IN_PRNT_RACK_POS"] ?? item["RACK_POS"]);

                        item["CNTND_IN_REVSN_LVL_IND"] = (item["CNTND_IN_REVSN_LVL_IND"] ?? "");

                        var parentId = (item["PRNT_COMN_CNFG_DEF_ID"] ?? "0").ToString();
                        if ( parentId != "" && long.Parse(parentId) > 0)
                        {
                            var parent = (item["CNTND_IN_PRNT_DSC"]??"").ToString();
                            var pSeqNo = (item["CNTND_IN_PRNT_SEQ_NO"] ?? "").ToString();
                            if (parent.Length == 0)
                            {
                                var p = (from e in masterList where e["COMN_CNFG_DEF_ID"].ToString() == parentId select e).FirstOrDefault();
                                if (p == null)
                                {
                                    item["CNTND_IN_PRNT_DSC"] = parent;
                                    continue;
                                }

                                parent = String.Format("{0} {1}", p["CLMC"], p["RT_PART_NO"]);
                                pSeqNo = p["CNTND_IN_SEQ_NO"].ToString();
                            }
                            item["CNTND_IN_PRNT_DSC"] = pSeqNo + ": " + parent;
                        }
                    }

                    var cols = new Hashtable();
                    cols["COLS"] = "CNTND_IN_SEQ_NO,CNTND_IN_ID,MTRL_CD,CLMC,RT_PART_NO,SPEC_NM,MTRL_CAT_TYP,FEAT_TYP,CNTND_IN_PRNT_DSC,Y_COORD_NO,X_COORD_NO,FRNT_RER_IND,LABEL_NM,RACK_POS,CNTND_IN_REVSN_LVL_IND,CNTND_IN_MTRL_QTY,CNTND_IN_MTRL_SPR_QTY,CNTND_IN_MTRL_TOTAL_QTY";
                    cols["HDRS"] = "#,CDMMS ID/CCID,Material Code,CLMC,Part Number,Spec Name,Material Category,Feature Type,Related To,EQDES,HORZ DISP,Placement,Label,Rack Mount Pos,Revisions,QTY,Spare QTY,Total QTY";
                    masterList.Insert(0, cols);
                }
                return Ok(masterList);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.ExportComnCnfg");
                return InternalServerError();
            }
        }

        private object GetParentDsc(List<Hashtable> masterList, string v)
        {
            throw new NotImplementedException();
        }

        private void BuildListForExport(List<Hashtable> masterList, List<Hashtable> items, Hashtable collection, string seqPrefix, int level)
        {
            if( items == null )
            {
                return;
            }
            var lvl = level + 1;
            foreach( Hashtable item in items)
            {
                if( lvl > 0 )
                {
                    var s = seqPrefix + "-" + item["CNTND_IN_SEQ_NO"];
                    item["CNTND_IN_SEQ_NO"] = s;
                }
                // for debugging..
                //item["LVL"] = lvl;
                //item["SEQ_PREFIX"] = seqPrefix;
                masterList.Add(item);

                var id = (item["CNTND_IN_ID"] ?? "0").ToString();
                var mc = (item["MTRL_CAT_TYP"]??"").ToString().Replace(" ", "");
                var ft = (item["FEAT_TYP"] ?? "").ToString().Replace(" ", "");

                if ( mc == "HighLevelPart" || ft == "CommonConfig")
                {
                    var key = (mc == "HighLevelPart" ? mc : ft) + id;
                    var seq = seqPrefix + (seqPrefix.Length == 0 ? "" : "-")+item["CNTND_IN_SEQ_NO"].ToString();
                    BuildListForExport(masterList, collection[key] as List<Hashtable>, collection, seq, lvl);
                }
            }
        }

        private async Task<List<Hashtable>> TraverseAndCollectContainedInItems(long comnCnfgId, List<Hashtable> items, Hashtable cache, int lvl)
        {
            lvl++;

            var list = new List<Hashtable>();
            foreach (Hashtable item in items)
            {
                var comnCnfgDefId = long.Parse((item["COMN_CNFG_DEF_ID"] ?? "0").ToString());
                var cntndInId = long.Parse((item["CNTND_IN_ID"] ?? "0").ToString());

                var mc = (item["MTRL_CAT_TYP"] ?? "").ToString().Replace(" ", "");
                var ft = (item["FEAT_TYP"] ?? "").ToString().Replace(" ", "");
                var typ = (ft == "CommonConfig" ? ft : mc);

                var key = typ + cntndInId.ToString();
                switch (typ)
                {
                    case "HighLevelPart":
                        if (cache.ContainsKey(key) == false)
                        {
                            var hlp = await _getContainedInHighLevelPartItems(comnCnfgDefId, lvl, cntndInId, "");
                            cache[key] = hlp;
                            await TraverseAndCollectContainedInItems(comnCnfgDefId, hlp, cache, lvl);
                        }
                        break;
                    case "CommonConfig":
                        if (cache.ContainsKey(key) == false)
                        {
                            var cc = await _getContainedInCommonConfigItems(comnCnfgDefId, lvl, cntndInId);
                            cache[key] = cc;
                            await TraverseAndCollectContainedInItems(comnCnfgDefId, cc, cache, lvl);
                        }
                        break;
                    default:
                        break;
                }
            }
            return list;
        }

        [HttpGet]
        [Route("hlp/items/{id}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetHighLevelPartItems(long id)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> list = null;

            try
            {
                list = await dbi.GET_HLP_ITEMS(id);
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetHighLevelPartItems");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("cntndin-hlp/items/{comnCnfgDefId}/{lvl}/{seqNo}/{id}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetContainedInHighLevelPartItems(long comnCnfgDefId, int lvl, string seqNo, long id)
        {
            try
            {
                var list = await _getContainedInHighLevelPartItems(comnCnfgDefId, lvl, id, seqNo);
                return Ok(list);
            }
            catch( Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetContainedInHighLevelPartItems");
                return InternalServerError();
            }
        }

        //[HttpGet]
        //[Route("cntndin-hlp-by-seq/items/{comnCnfgDefId}/{lvl}/{hlpnId}/{seq}")]
        //[ResponseType(typeof(List<Hashtable>))]
        public async Task<List<Hashtable>> GetContainedInHighLevelPartItemsBySeqNo(long comnCnfgDefId, int lvl, long hlpnId, string seqNo)
        {
            
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                var list = await dbi.GET_HLP_ITEMS(hlpnId);
                var parts = (lvl > 1 ? seqNo.Split('-') : null);
                var seq = (parts != null ? Int32.Parse(parts[parts.Length - 1]) : Int32.Parse(seqNo));
                var cntxt = await dbi.GET_CNFG_CI_CI_HLP_ITEMS(comnCnfgDefId, lvl, hlpnId, seq);

                var example = cntxt[0];
                if ( list.Count > 0 && cntxt.Count > 0 )
                {
                    foreach (Hashtable item in list)
                    {
                        var defId = item["HLP_MTRL_REVSN_DEF_ID"].ToString();
                        var ctx = (from c in cntxt where c["CNTND_IN_HLP_MTRL_REVSN_DEF_ID"].ToString() == defId select c).FirstOrDefault();
                        if (ctx != null)
                        {
                            foreach (string key in example.Keys)
                            {
                                item[key] = ctx[key];
                            }
                        }
                    }
                }


                //return Ok(list);
                return list;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetContainedInHighLevelPartItems");
                //return InternalServerError();
                return (new List<Hashtable>());
            }
        }

        // ORIGINAL
        //private async Task<List<Hashtable>> _getContainedInHighLevelPartItems(long comnCnfgDefId, int lvl, long id)
        //{
        //    var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
        //    if (lvl > 1)
        //    {
        //        var list1 = await dbi.GetHighLevelPartItems(id);
        //        return list1;
        //    }
        //    var list2 = await dbi.GetContainedInHighLevelPartItems(comnCnfgDefId, lvl, id);
        //    return list2;
        //}

        private long GetRecValue(Hashtable rec, string col, long defVal)
        {
            long val = defVal;
            if (rec.ContainsKey(col) == false)
                return defVal;

            if( long.TryParse((rec[col]??"").ToString(), out val) )
                return val;

            return defVal;
        }
        private async Task<List<Hashtable>> _getContainedInHighLevelPartItems(long prntComnCnfgDefId, int lvl, long hlpnId, string seqNo)
        {
            List<Hashtable> list;
            Hashtable prnt = null;

            var timers = new StringBuilder();
            var mainTimer = new Stopwatch();
            var prntTimer = new Stopwatch();
            TimeSpan ts;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();

            if (lvl == 1 || lvl == 2)
            {
                mainTimer.Start();
                list = await dbi.GET_CNFG_CI_HLP_ITEMS(prntComnCnfgDefId, lvl, hlpnId);
                mainTimer.Stop();
                ts = mainTimer.Elapsed;
                timers.AppendFormat("GET_CNFG_CI_HLP_ITEMS: {0}:{1}.{2}\n", ts.Minutes, ts.Seconds, ts.Milliseconds);
                mainTimer.Reset();

                if( list.Count == 0 )
                {
                    //list = await dbi.GET_CNFG_CI_CI_HLP_ITEMS(prntComnCnfgDefId, lvl, hlpnId);
                    list = await GetContainedInHighLevelPartItemsBySeqNo(prntComnCnfgDefId, lvl, hlpnId, seqNo);
                }
                if( list.Count == 0 )
                {
                    list = await dbi.GET_HLP_ITEMS(hlpnId);
                }
            }
            else
            {
                list = await dbi.GET_HLP_ITEMS(hlpnId);
            }
            mainTimer.Start();
            foreach (Hashtable item in list)
            {
                item["IS_READONLY"] = (lvl > 2 ? 1 : 0);
                var prntId = GetRecValue(item, "PRNT_COMN_CNFG_DEF_ID", -1);
                var prntCiHlpMtrlRevsnDefId = GetRecValue(item, "PRNT_CI_HLP_MTRL_REVSN_DEF_ID", -1);
                var prntCiComnCnfgDefId = GetRecValue(item, "PRNT_CI_COMN_CNFG_DEF_ID", -1);

                var prntHlpDefId = GetRecValue(item, "PRNT_HLP_MTRL_REVSN_DEF_ID", -1);  // would be set on the HLP level only (not contained-in/contained-in)
                var prntCcDefId = GetRecValue(item, "PRNT_COMN_CNFG_DEF_ID", -1);  // would be set on the CC level only (not contained-in/contained-in)

                var prntRefKeyId = String.Format("{0}.{1}.{2}", prntId, prntCiHlpMtrlRevsnDefId, prntCiComnCnfgDefId);
                if (prntRefKeyId == "-1.-1.-1")
                {
                    if (prntHlpDefId <= 0 && prntCcDefId <= 0)
                    {
                        continue; // no parent info set
                    }

                    prntTimer.Start();
                    // this means nothing is customized for the CC so pull the parent info directly...
                    prnt = await dbi.GET_HLPN_OR_CC_PRNT_INFO(prntComnCnfgDefId, lvl, prntHlpDefId, prntCcDefId);
                    prntTimer.Stop();
                    ts = prntTimer.Elapsed;
                    timers.AppendFormat("GET_HLPN_OR_CC_PRNT_INFO({0}): {1}:{2}.{3}\n", prntHlpDefId, ts.Minutes, ts.Seconds, ts.Milliseconds);
                    prntTimer.Reset();
                }
                else
                {

                    item["CNTND_IN_PRNT_SEQ_NO"] = "";
                    item["CNTND_IN_PRNT_DSC"] = "??? (" + prntRefKeyId + ")";

                    prntTimer.Start();
                    prnt = await dbi.GET_PRNT_INFO(prntRefKeyId);
                    prntTimer.Stop();
                    ts = prntTimer.Elapsed;
                    timers.AppendFormat("GET_PRNT_INFO({0}): {1}:{2}.{3}\n", prntRefKeyId, ts.Minutes, ts.Seconds, ts.Milliseconds);
                    prntTimer.Reset();
                }

                if (prnt != null)
                {
                    item["CNTND_IN_PRNT_SEQ_NO"] = String.Format("{0}-{1}", (prnt["PRNT_SEQ_NO"] ?? ""), prnt["ITEM_SEQ_NO"]);
                    item["CNTND_IN_PRNT_DSC"] = (prnt["SPECN_NM"] ?? "???").ToString();

                    //
                    // this is my containing parent CC or HLP, my related to parent will either be somewhere in this heirarchy
                    // so my related-to parent seq # will start here
                    // and then we should append PRNT_SEQ_NO and ITEM_SEQ_NO to complete it 
                    // so my prntComnCnfgDefId will be something like #, #-#, or #-#-#, and then we'll append
                    // to rest: [#,#-#,#-#-#]-[PRNT_SEQ_NO-ITEM_SEQ_NO]
                    // if the PRNT is at the same level, then we can skip appending the PRNT_SEQ_NO
                    //
                    item["_rltd_to_comn_cnfg_def_id"] = prntComnCnfgDefId;


                    foreach (DictionaryEntry e in prnt)
                    {
                        item["_rltd_to_" + e.Key.ToString().ToLower()] = e.Value;
                    }
                }
            }
            mainTimer.Stop();
            ts = mainTimer.Elapsed;
            timers.AppendFormat("Parse list for Parents: {0}:{1}.{2}\n", ts.Minutes, ts.Seconds, ts.Milliseconds);
            mainTimer.Reset();

            System.Diagnostics.Trace.WriteLine(timers.ToString());
            return list;
        }
        /*
        private async Task<List<Hashtable>> _getContainedInHighLevelPartItemsTEST(long prntComnCnfgDefId, int lvl, long hlpnId)
        {
            List<Hashtable> list;
            Hashtable prnt = null;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            if (lvl >= 1)
            {
                list = await dbi.GetHighLevelPartItems(hlpnId);
                if( lvl >= 2)
                {
                    foreach (Hashtable item in list)
                    {
                        item["IS_READONLY"] = 1;
                        var prntId = GetRecValue(item, "PRNT_COMN_CNFG_DEF_ID", -1);
                        var prntCiHlpMtrlRevsnDefId = GetRecValue(item, "PRNT_CI_HLP_MTRL_REVSN_DEF_ID", -1);
                        var prntCiComnCnfgDefId = GetRecValue(item, "PRNT_CI_COMN_CNFG_DEF_ID", -1);

                        var prntHlpDefId = GetRecValue(item, "PRNT_HLP_MTRL_REVSN_DEF_ID", -1);  // would be set on the HLP level only (not contained-in/contained-in)
                        var prntCcDefId = GetRecValue(item, "PRNT_COMN_CNFG_DEF_ID", -1);  // would be set on the CC level only (not contained-in/contained-in)

                        var prntRefKeyId = String.Format("{0}.{1}.{2}", prntId, prntCiHlpMtrlRevsnDefId, prntCiComnCnfgDefId);
                        if (prntRefKeyId == "-1.-1.-1")
                        {
                            if (prntHlpDefId <= 0 && prntCcDefId <= 0)
                            {
                                continue; // no parent info set
                            }

                            // this means nothing is customized for the CC so pull the parent info directly...
                            prnt = await dbi.GET_HLPN_OR_CC_PRNT_INFO(prntComnCnfgDefId, lvl, prntHlpDefId, prntCcDefId);
                            //prnt["_rltd_to_prnt_comn_cnfg_def_id"] = prntComnCnfgDefId;
                        }
                        else
                        {

                            item["CNTND_IN_PRNT_SEQ_NO"] = "";
                            item["CNTND_IN_PRNT_DSC"] = "??? (" + prntRefKeyId + ")";

                            prnt = await dbi.GET_PRNT_INFO(prntRefKeyId);
                        }

                        if (prnt != null)
                        {
                            item["CNTND_IN_PRNT_SEQ_NO"] = String.Format("{0}-{1}", (prnt["PRNT_SEQ_NO"]??""), prnt["ITEM_SEQ_NO"]);
                            item["CNTND_IN_PRNT_DSC"] = (prnt["SPECN_NM"]??"???").ToString();

                            //
                            // this is my containing parent CC or HLP, my related to parent will either be somewhere in this heirarchy
                            // so my related-to parent seq # will start here
                            // and then we should append PRNT_SEQ_NO and ITEM_SEQ_NO to complete it 
                            // so my prntComnCnfgDefId will be something like #, #-#, or #-#-#, and then we'll append
                            // to rest: [#,#-#,#-#-#]-[PRNT_SEQ_NO-ITEM_SEQ_NO]
                            // if the PRNT is at the same level, then we can skip appending the PRNT_SEQ_NO
                            //
                            item["_rltd_to_comn_cnfg_def_id"] = prntComnCnfgDefId;

                            
                            foreach (DictionaryEntry e in prnt)
            {
                                item["_rltd_to_" + e.Key.ToString().ToLower()] = e.Value;
            }
        }

                    }
                }
                return list;
            }
            list = await dbi.GetContainedInHighLevelPartItems(prntComnCnfgDefId, lvl, hlpnId);
            return list;
        }
        */

        //[HttpGet]
        //[Route("search/mtrl/all")]
        //[ResponseType(typeof(List<Hashtable>))]
        //public async Task<IHttpActionResult> SearchForMaterial() // mwj: I do not believe this route is being used any more...
        //{
        //    IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
        //    var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
        //    List<Hashtable> items = null;

            
        //    if (query == null || query.Count() == 0)
        //    {
        //        logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
        //        return BadRequest("Invalid request sent to the server");
        //    }
        //    var args = new Hashtable();
        //    foreach (KeyValuePair<string, string> que in query)
        //    {
        //        args[que.Key] = (que.Value.EndsWith("Id")
        //                        ? (string.IsNullOrEmpty(que.Value) ? "0" : que.Value)
        //                        : (string.IsNullOrEmpty(que.Value) ? "" : que.Value) 
        //                        );
        //    }

        //    try
        //    {
        //        dbi.UpdateAppLog("SearchForSpecParts()", "SYS", args);
        //        items = await dbi.SearchForMaterial(args);
        //        return Ok(items);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Search failed for CommonConfigTemplate.SearchForMaterial");
        //        return InternalServerError();
        //    }
        //}
        [HttpGet]
        [Route("fetchRelatedToParentInfo/{prntRefKey}")]
        [ResponseType(typeof(Hashtable))]
        public async Task<IHttpActionResult> FetchRelatedToParentInfo(string prntRefKey)
        {
            Hashtable prnt;
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                // kludge...
                prntRefKey = prntRefKey.Replace('x', '.');
                prnt = await dbi.GET_PRNT_INFO(prntRefKey);
                if( prnt == null )
                {
                    prnt = await dbi.GET_CI_PRNT_INFO(prntRefKey);
                }
                return Ok(prnt);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.FetchRelatedToParentInfo");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("cc/parentCandidates/{comnCnfgId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetParentCandidates(long comnCnfgId)
        {
            List<Hashtable> list;
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                list = await dbi.GET_PRNT_CANDIDATES(comnCnfgId);
                if (list == null)
                {
                    return Ok(list); // no items...
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetParentCandidates");
                return InternalServerError();
            }
        }
        
        [HttpGet]
        [Route("cc/items/{comnCnfgId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetCommonConfigItems(long comnCnfgId)
        {
            List<Hashtable> list;
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                list = await dbi.GetCommonConfigDef(comnCnfgId);
                if (list == null)
                {
                    return Ok(list); // no items...
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetCommonConfigItems");
                return InternalServerError();
            }
        }
        [HttpGet]
        [Route("cntndin-cc/items/{prntComnCnfgDefId}/{lvl}/{comnCnfgId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetContainedInCommonConfigItems(long prntComnCnfgDefId, int lvl,long comnCnfgId)
        {
            List<Hashtable> list;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                list = await _getContainedInCommonConfigItems(prntComnCnfgDefId, lvl, comnCnfgId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetContainedInCommonConfigItems");
                return InternalServerError();
            }
        }
        public async Task<List<Hashtable>> _getContainedInCommonConfigItems(long prntComnCnfgDefId, int lvl, long comnCnfgId)
        {
            List<Hashtable> list;

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            if( lvl <= 2)
            {
                list = await dbi.GetCnfgCiCnfgAllItems(prntComnCnfgDefId, lvl, comnCnfgId);
                if (lvl >= 2)
                {
                    foreach (Hashtable item in list)
                    {
                        item["IS_READONLY"] = 1;
                        var prntId = long.Parse((item["PRNT_COMN_CNFG_DEF_ID"] ?? "0").ToString());
                        var prntCiHlpMtrlRevsnDefId = long.Parse((item["PRNT_CI_HLP_MTRL_REVSN_DEF_ID"] ?? "0").ToString());
                        var prntCiComnCnfgDefId = long.Parse((item["PRNT_CI_COMN_CNFG_DEF_ID"] ?? "0").ToString());
                        if (prntId > 0 || prntCiHlpMtrlRevsnDefId > 0 || prntCiComnCnfgDefId > 0)
                        {
                            var prntRefKeyId = String.Format("{0}.{1}.{2}", prntId, prntCiHlpMtrlRevsnDefId, prntCiComnCnfgDefId);
                            item["CNTND_IN_PRNT_SEQ_NO"] = "";
                            item["CNTND_IN_PRNT_DSC"] = "??? (" + prntRefKeyId + ")";

                            var prnt = await dbi.GET_PRNT_INFO(prntRefKeyId);
                            if (prnt != null)
                            {
                                item["CNTND_IN_PRNT_SEQ_NO"] = String.Format("{0}-{1}", prnt["PRNT_SEQ_NO"], prnt["ITEM_SEQ_NO"]);
                                item["CNTND_IN_PRNT_DSC"] = prnt["SPECN_NM"].ToString();

                                //
                                // this is my containing parent CC or HLP, my related to parent will either be somewhere in this heirarchy
                                // so my related-to parent seq # will start here
                                // and then we should append PRNT_SEQ_NO and ITEM_SEQ_NO to complete it 
                                // so my prntComnCnfgDefId will be something like #, #-#, or #-#-#, and then we'll append
                                // to rest: [#,#-#,#-#-#]-[PRNT_SEQ_NO-ITEM_SEQ_NO]
                                // if the PRNT is at the same level, then we can skip appending the PRNT_SEQ_NO
                                //
                                item["_rltd_to_comn_cnfg_def_id"] = prntComnCnfgDefId;

                                foreach( DictionaryEntry e in prnt )
                                {
                                    item["_rltd_to_" + e.Key.ToString().ToLower()] = e.Value;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                list = await dbi.GetCommonConfigDef(comnCnfgId);
            }

            return list;

        }

        [HttpGet]
        [Route("refs/{comnCnfgId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetCommonConfigReferences(long comnCnfgId)
        {
            List<Hashtable> list;
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            try
            {
                list = await dbi.GET_COMN_CNFG_REFS(comnCnfgId);
                if (list == null)
                {
                    return Ok(list); // no items...
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfig.GetCommonConfigReferences");
                return InternalServerError();
            }
        }
        [HttpGet]
        [Route("containmentrules")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetContainmentRules()
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> list = null;

            try
            {
                list = await dbi.GET_CONTAINMENT_RULES();
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfigTemplate.GetContainmentRules");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("changelog/{id}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetAuditLog(long id)
        {
            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List<Hashtable> list = null;

            try
            {
                list = await dbi.GetAuditLog(id);
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfigTemplate.GetAuditLog");
                return InternalServerError();
            }
        }
        [HttpGet]
        [Route("getOptionList")]
        [ResponseType(typeof(List<DbInterface.CommonConfig.Option>))]
        public async Task<IHttpActionResult> getOptionList()
        {
            var rv = new List<DbInterface.CommonConfig.Option>();
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            if( query.Count() == 0 )
            {
                return Ok(rv); // nothing requested...
            }

            var cat = (from q in query where q.Key == "CAT" select q.Value).FirstOrDefault();

            var dbi = new CenturyLink.Network.Engineering.Material.Editor.DbInterface.CommonConfig.DbInterface();
            List< DbInterface.CommonConfig.Option> list = null;

            try
            {
                list = await dbi.GetOptionList(cat);
                if( list == null )
                {
                    list = new List<DbInterface.CommonConfig.Option>();
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Search failed for CommonConfigTemplate.getOptionList");
                return InternalServerError();
            }
        }
    }
}
