using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using System.Text;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/tmplt")]
    public class TemplateController : ApiController
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

        private long UpdateTemplate(string cuid, Hashtable rec)
        {
            var tmpltId = 0L;

            if( tmpltId == 0 )
            {
                throw new NotImplementedException("TemplateController.UpdateTemmplate NOT IMPLEMENTED YET");
            }

            return tmpltId;
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

            var dbi = new TemplateDbInterface();
            var entry = new Hashtable();

            entry["pCUID"] = cuid;
            
            foreach (JObject item in list)
            {
                var rec = item.ToObject<Hashtable>();

                // fill out entry as appropriate for template; TODO - review sproc and code below; just stubbed out for now..

                //entry["pTmpltId"] = (rec["tmpltId"] ?? "0").ToString();
                //entry["pTmpltDefId"] = (rec["tmpltDefId"] ?? "0").ToString();
                //entry["pId"] = (rec["id"] ?? "").ToString();
                //entry["pItemDsc"] = (rec["dsc"] ?? "0").ToString();
                //entry["pTblNm"] = (rec["tbl"] ?? "").ToString().ToLower();
                //entry["pColNm"] = (rec["col"] ?? "").ToString();
                //entry["pOldVal"] = (rec["oldVal"] ?? "").ToString();
                //entry["pNewVal"] = (rec["newVal"] ?? "").ToString();
                //entry["pAction"] = (rec["action"] ?? "C").ToString();

                try
                {
                    dbi.UPDATE_AUDIT_LOG(entry);
                }
                catch (Exception ex)
                {
                    var log = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                    errors.Add(new Exception(log + "\n" + ex.Message));
                }
            }
        }
        #endregion PRIVATE METHODS

        //[HttpGet]
        //[Route("{id}/{tmpltType}")]
        //[ResponseType(typeof(ITemplate))]
        //public async Task<IHttpActionResult> GetTemplate(long id, string tmpltType)
        //{
        //    IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
        //    ITemplate template = null;
        //    TemplateManager manager = new TemplateManager();
        //    TemplateType.Type templateType = TemplateType.Type.NOT_SET;
        //    string json = "{}";
        //    string referrer = "NULL";
        //    bool isBaseTemplate = false;
        //    bool isForEdit = false;

        //    if (Request.Headers.Referrer != null)
        //        referrer = Request.Headers.Referrer.OriginalString;

        //    if (query != null && query.Count() == 2)
        //    {
        //        foreach (KeyValuePair<string, string> que in query)
        //        {
        //            //
        //            // 2020 - 01 - 20: mwj - taking this part out because I'm not sure that we need it
        //            //
        //            //if (que.Key == "baseTmplt")
        //            //{
        //            //    if (!Boolean.TryParse(que.Value, out isBaseTemplate))
        //            //    {
        //            //        logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

        //            //        return BadRequest("Invalid request sent to the server");
        //            //    }
        //            //}

        //            if (que.Key == "forEdit")
        //            {
        //                if (!Boolean.TryParse(que.Value, out isForEdit))
        //                {
        //                    logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

        //                    return BadRequest("Invalid request sent to the server");
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

        //        return BadRequest("Invalid request sent to the server");
        //    }

        //    try
        //    {
        //        // 
        //        // 2020-01-20: mwj, adding check for ANY/UNK to determine if tmplt type is base or overall; commented out the section above
        //        // becuase I'm not sure we should be sending that, I think the app should know this not the user...
        //        // 
        //        if( tmpltType == "ANY" || tmpltType == "UNK" )
        //        {
        //            var db = new TemplateDbInterface();
        //            var tmplt = db.GET_TMPLT(id);
        //            isBaseTemplate = (tmplt.BaseTemplateInd == "Y");
        //            tmpltType = tmplt.TemplateType.ToUpper();
        //        }
        //        if (Enum.TryParse<TemplateType.Type>(tmpltType, out templateType))
        //            template = await manager.GetTemplate(id, templateType, isBaseTemplate, isForEdit);
        //        else
        //        {
        //            logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

        //            return BadRequest("Invalid request sent to the server");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "TemplateController.GetTemplate({0}, {1})", id, tmpltType);

        //        return InternalServerError();
        //    }

        //    if (template != null)
        //    {
        //        try
        //        {
        //            json = JsonConvert.SerializeObject(template, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new JsonHelper("options") });
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error(ex, "Unable to serialize response from TemplateDbInterface.GetTemplate({0}, {1})", id, tmpltType);

        //            return InternalServerError();
        //        }
        //    }

        //    return Ok(json);
        //}

        [HttpGet]
        [Route("{id}/{tmpltType}")]
        [ResponseType(typeof(List<string>))]
        public async Task<IHttpActionResult> GetTemplate(long id, string tmpltType)
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            ITemplate template = null;
            TemplateManager manager = new TemplateManager();
            TemplateType.Type templateType = TemplateType.Type.NOT_SET;
            string json = "{}";
            string referrer = "NULL";
            bool isBaseTemplate = false;
            //bool isForEdit = false;

            var data = new List<String>();
            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;
            try
            {
                ComplexTemplate.TemplateObject baseTmplt = null;
                ComplexTemplate cmplxTmplt = null;

                var db = new TemplateDbInterface();
                var tmplt = db.GET_TMPLT(id);
                isBaseTemplate = tmplt.BaseTemplateInd == "Y";
                tmpltType = tmplt.TemplateType.ToUpper();

                if( isBaseTemplate == false )
                {
                    cmplxTmplt = db.GET_COMPLEX_TMPLT(id);
                    baseTmplt = db.GET_TMPLT(cmplxTmplt.BaseTemplateID);

                    data.Add(Newtonsoft.Json.JsonConvert.SerializeObject(baseTmplt));
                    data.Add(Newtonsoft.Json.JsonConvert.SerializeObject(tmplt));
                    data.Add(Newtonsoft.Json.JsonConvert.SerializeObject(cmplxTmplt));
                }
                else
                {
                    data.Add(Newtonsoft.Json.JsonConvert.SerializeObject(tmplt));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateController.GetTemplate({0}, {1})", id, tmpltType);

                return InternalServerError();
            }

            return Ok(data);
            //if (template != null)
            //{
            //    try
            //    {
            //        json = JsonConvert.SerializeObject(template, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new JsonHelper("options") });
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex, "Unable to serialize response from TemplateDbInterface.GetTemplate({0}, {1})", id, tmpltType);

            //        return InternalServerError();
            //    }
            //}

            //return Ok(json);
        }

        [HttpGet]
        [Route("drawing/{id}/{tmpltType}")]
        [ResponseType(typeof(ITemplate))]
        public async Task<IHttpActionResult> GetTemplateDrawing(long id, string tmpltType)
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            ITemplate template = null;
            TemplateManager manager = new TemplateManager();
            TemplateType.Type templateType = TemplateType.Type.NOT_SET;
            string json = "{}";
            string referrer = "NULL";
            bool isBaseTemplate = false;
            bool isForEdit = false;

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query != null && query.Count() == 2)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "baseTmplt")
                    {
                        if (!Boolean.TryParse(que.Value, out isBaseTemplate))
                        {
                            logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

                            return BadRequest("Invalid request sent to the server");
                        }
                    }

                    if (que.Key == "forEdit")
                    {
                        if (!Boolean.TryParse(que.Value, out isForEdit))
                        {
                            logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

                            return BadRequest("Invalid request sent to the server");
                        }
                    }
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }

            try
            {
                if (Enum.TryParse<TemplateType.Type>(tmpltType, out templateType))
                    template = await manager.GetTemplate(id, templateType, isBaseTemplate, isForEdit);
                else
                {
                    logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);

                    return BadRequest("Invalid request sent to the server");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TemplateController.GetTemplateDrawing({0}, {1})", id, tmpltType);

                return InternalServerError();
            }

            if (template != null)
            {
                try
                {
                    template.CreateDrawing(isForEdit);

                    json = JsonConvert.SerializeObject(template.TemplateDrawing, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new JsonHelper("options") });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from TemplateDbInterface.GetTemplateDrawing({0}, {1})", id, tmpltType);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }        

        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateTemplate()
        {
            string json = "";
            JObject tmplt = null;
            long tmpltId = 0;
            TemplateManager manager = null; 

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to parse JSON for Template record");

                return InternalServerError(ex);
            }

            try
            {
                tmplt = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                manager = new TemplateManager();

                await manager.UpdateTemplate(tmplt);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.UpdateTemplate - unable to persist template id {0} from JSON: {1}", tmpltId, json);

                return InternalServerError(ex);
            }

            return Ok("{\"Status\": \"Updated Successfully\"}");
        }
       
        [HttpGet]
        [Route("search/all")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchAll()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();   
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...               
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                if (que.Key.EndsWith("Id"))
                {
                    args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "" : que.Value);
                }
                else
                {
                    args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
                }
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                items = await dbi.SEARCH_TMPLT(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.SearchAll() failed for Template");
                return InternalServerError();
            }
        }        
        

        [HttpGet]
        [Route("searchTmpltSpec")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchTmpltSpec()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...   
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                items = await dbi.searchTmpltSpec(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.SearchTmpltSpec() failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("searchHlpMnrMtl")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchHlpMnrMtl()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...   
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                items = await dbi.SEARCH_HLP_MNR_MTL(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.SearchTmpltSpec() failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("searchBaseTmplt")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchBaseTmplt()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...   
            var baseTmpid = 0L;
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "" : que.Value);
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                baseTmpid = await dbi.SearchBaseTmplt(args);
                return Ok(baseTmpid);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.SearchBaseTmplt() failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("findTmpltByName")]
        [ResponseType(typeof(List<string>))]
        public async Task<IHttpActionResult> FindTmpltByName()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...   
            var resultVal = string.Empty;
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "" : que.Value);
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                resultVal = await dbi.FindTmpltByName(args);
                return Ok(resultVal);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.FindTmpltByName() failed for Template");
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("createTmplt")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> CreateTmplt()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var tmpltId = 0L;
            var args = new Hashtable();
            string referrer = "NULL";
            string json = "";
            TemplateManager manager = new TemplateManager();
            JObject item = null;

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content");

                return InternalServerError();
            }

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                IEnumerator<KeyValuePair<string, JToken>> enumerator = item.GetEnumerator();
               
                while(enumerator.MoveNext())
                {
                    args.Add(enumerator.Current.Key, enumerator.Current.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to deserialize request.");

                return InternalServerError(ex);
            }

            try
            {
                if (args.Count > 0)
                {
                    tmpltId = await manager.CreateTemplate(args);
                }
                return Ok(tmpltId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CreateTemplate() failed.");

                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("getBaseOverallTmpltDtls")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> getBaseOverallTmpltDtls()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var dbi = new TemplateDbInterface();
            List<Hashtable> items = new List<Hashtable>(); // either return no items or at least something...   
            var args = new Hashtable();
            string referrer = "NULL";

            if (Request.Headers.Referrer != null)
                referrer = Request.Headers.Referrer.OriginalString;

            if (query == null || query.Count() == 0)
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", referrer, Request.RequestUri.OriginalString);
                return BadRequest("Invalid request sent to the server");
            }

            foreach (KeyValuePair<string, string> que in query)
            {
                args[que.Key] = (string.IsNullOrEmpty(que.Value) ? "%" : que.Value);
            }

            try
            {
                //dbi.UPDATE_APP_LOG("SearchAll()", "SYS", args);              
                items = await dbi.GET_TMPLT(args);
                return Ok(items);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.getBaseOverallTmpltDtls() failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("export/{tmpltId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> ExportTemplate(long tmpltId)
        {
            var dbi = new TemplateDbInterface();
            var masterList = new List<Hashtable>();

            try
            {
                var collection = new Hashtable();
                var cols = new Hashtable();

                cols["COLS"] = "DB,COL,NAMES,GO,HERE";
                cols["HDRS"] = "Excel,Col,Names,Go,Here";

                masterList.Insert(0, cols);
                
                return Ok(masterList);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Contorller.ExportTemplate failed for Template #"+tmpltId.ToString());
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("containmentrules")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetContainmentRules()
        {
            var dbi = new TemplateDbInterface();
            List<Hashtable> list = null;

            try
            {
                list = await dbi.GET_CONTAINMENT_RULES();
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetContainmentRules() failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("changelog/{id}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> GetAuditLog(long id)
        {
            var dbi = new TemplateDbInterface();
            List<Hashtable> list = null;

            try
            {
                list = await dbi.GET_AUDIT_LOG(id);
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetAuditLog failed for Template #"+id.ToString());
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("getShelfTmplt/{id}")]
        [ResponseType(typeof(Hashtable))]
        public async Task<IHttpActionResult> GetShelfTmplt(long id)
        {
            var dbi = new TemplateDbInterface();
            Hashtable item = null;

            try
            {
                item = await dbi.GET_SHELF_TMPLT(id);
                return Ok(item);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetShelfTmplt failed for Template #" + id.ToString());
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("search/equip/{tmpltId}/{cat}/{context}/{match}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> SearchForEquip(string tmpltId, string cat, string context, string match)
        {
            var dbi = new TemplateDbInterface();

            try
            {
                var list = await dbi.SEARCH_FOR_EQUIP(tmpltId, cat, context, match);
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.SearchForEquip({0},{1},{2},{3}) failed", tmpltId, cat, context, match);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("getOptionList")]
        [ResponseType(typeof(List<DbInterface.Template.Option>))]
        public async Task<IHttpActionResult> GetOptionList()
        {
            var rv = new List<DbInterface.Template.Option>();
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();

            if (query.Count() == 0)
            {
                return Ok(rv); // nothing requested...
            }

            var cat = (from q in query where q.Key == "CAT" select q.Value).FirstOrDefault();
            var dbi = new TemplateDbInterface();
            List<DbInterface.Template.Option> list = null;

            try
            {
                list = await dbi.GET_OPTION_LIST(cat);

                if (list == null)
                {
                    list = new List<DbInterface.Template.Option>();
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetOptionList failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("getBayChoices")]
        [ResponseType(typeof(List<CenturyLink.Network.Engineering.Material.Editor.Models.Option>))]
        public async Task<IHttpActionResult> GetBayChoices()
        {
            CenturyLink.Network.Engineering.Material.Editor.Models.Option options = new CenturyLink.Network.Engineering.Material.Editor.Models.Option();
            List<string> list = new List<string>();
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            BayTemplateDbInterface dbInterface = new BayTemplateDbInterface();
            string featureTypeID = String.Empty;
            string templateID = String.Empty;
            string json = "{}";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "feattypid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            featureTypeID = "%";
                        else
                            featureTypeID = que.Value;
                    }
                    else if (que.Key == "templateid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            templateID = "%";
                        else
                            templateID = que.Value;
                    }
                }
            }

            try
            {
                list = await dbInterface.GetBayChoices(featureTypeID, templateID);

                try
                {
                    json = JsonConvert.SerializeObject(list, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from TemplateController.GetBayChoices");
                    return InternalServerError();
                }

                return Ok(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetOptionList failed for Template");
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("getComplexTemplate")]
        [ResponseType(typeof(ComplexTemplate))]
        public async Task<IHttpActionResult> GetComplexTemplate()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            TemplateDbInterface templateDbInterface = new TemplateDbInterface();
            string templateID = String.Empty;
            string json = "{}";
            ComplexTemplate complexTemplate = new ComplexTemplate();

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "templateid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            return Ok("unsuccessful");
                        else
                            templateID = que.Value;
                    }
                }
            }

            try
            {
                complexTemplate = templateDbInterface.GET_COMPLEX_TMPLT(long.Parse(templateID));

                try
                {
                    json = JsonConvert.SerializeObject(complexTemplate, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from TemplateController.GetComplexTemplate");
                    return InternalServerError();
                }

                return Ok(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Controller.GetComplexTemplate failed for Template");
                return InternalServerError();
            }
        }
    }
}
