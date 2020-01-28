using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/modifiedpart")]
    public class ModifiedPartController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("save/{update?}")]
        [ResponseType(typeof(string[]))]
        public async Task<IHttpActionResult> Save(string update = "")
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            ModifiedPartDbInterface dbInterface = new ModifiedPartDbInterface();
            IDictionary<string, string> IDvalue = null;
            Dictionary<string, ModifiedPart> dicMP = new Dictionary<string, ModifiedPart>();
            Dictionary<string, ModifiedPart> existMP = new Dictionary<string, ModifiedPart>();
            string id = "", productID = "", publish = "N", cuid = "", materialID = "";
            string[] savestatus;
            long workToDoId = 0;
            ModifiedPart existingMP = null;

            try
            {
                if (query != null && query.Count() > 0)
                {
                    IDvalue = await dbInterface.GetMaterialItemDef(true);

                    foreach (KeyValuePair<string, string> que in query)
                    {
                        if (que.Value != "")
                        {
                            if (IDvalue.ContainsKey(que.Key))
                            {
                                if (que.Key == "LastUpdatedUserID")
                                {
                                    cuid = que.Value.ToUpper();
                                }
                                IDvalue.TryGetValue(que.Key, out id);
                                ModifiedPart MP = new ModifiedPart(que.Key, id, que.Value);
                                dicMP.Add(que.Key, MP);
                            }
                            else if (que.Key == "ProductID")
                            {
                                productID = que.Value.ToUpper();
                            }
                            else if (que.Key == "Publish")
                            {
                                if (que.Value == "true")
                                { publish = "Y"; }
                                else
                                { publish = "N"; }
                            }
                            else if (que.Key == "MaterialID")
                            {
                                materialID = que.Value;
                            }
                        }
                    }

                    if (!dicMP.ContainsKey("LastUpdatedDate"))
                    {
                        if (IDvalue.ContainsKey("LastUpdatedDate"))
                        {
                            IDvalue.TryGetValue("LastUpdatedDate", out id);
                            ModifiedPart MP = new ModifiedPart("LastUpdatedDate", id, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                            dicMP.Add("LastUpdatedDate", MP);
                        }
                    }
                    else
                    {
                        dicMP["LastUpdatedDate"].SValue = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    
                    try
                    {
                        if (update == "")
                        {
                            savestatus = await dbInterface.SaveMaterialItem(dicMP, productID, cuid, publish);
                        }
                        else
                        {
                            existingMP = await dbInterface.SearchAllModifiedParts(materialID);

                            foreach (KeyValuePair<string, string> eMP in existingMP.Attributes)
                            {                               
                                    if (IDvalue.ContainsKey(eMP.Key))
                                    {
                                        IDvalue.TryGetValue(eMP.Key, out id);
                                        ModifiedPart MP = new ModifiedPart(eMP.Key, id, eMP.Value);
                                        existMP.Add(eMP.Key, MP);
                                    }                                
                            }

                            savestatus = await dbInterface.Update(dicMP, existMP, materialID, cuid, publish);
                        }

                        if ("success".Equals(savestatus[0]) && "Y".Equals(publish))
                        {
                            MaterialDbInterface mtlDbInterface = new MaterialDbInterface();
                            long materialItemId = 0;
                            bool parsed = false;

                            parsed = long.TryParse(savestatus[1], out materialItemId);

                            if (parsed)
                            {
                                workToDoId = mtlDbInterface.InsertWorkToDo(materialItemId, "CATALOG_UI", null);

                                savestatus[2] = workToDoId.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                else
                {
                    logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);
                    return BadRequest("Invalid request sent to the server");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(savestatus);
        }

        [HttpGet]
        [Route("search/{recordtype}")]
        [ResponseType(typeof(SearchResult))]
        public async Task<IHttpActionResult> Search(string recordType)
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            ModifiedPartDbInterface dbInterface = new ModifiedPartDbInterface();
            List<SearchResult> items = null;
            string json = "{}";
            string searchValue = "";
            string searchBy = "";

            if (query != null && query.Count() == 2)
            {
                searchValue = query.First().Value;
                searchBy = query.Last().Value;

                try
                {
                    items = await dbInterface.SearchRecordOnlyPartAsync(searchValue, recordType, searchBy);

                    if (items == null)
                        return Ok("no_results");
                }
                catch (Exception ex)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.SearchMaterialItemAsync({0})", searchValue);

                    return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("searchAll/{id}")]
        [ResponseType(typeof(SearchResult))]
        public async Task<IHttpActionResult> SearchAll(string id)
        {
            ModifiedPartDbInterface dbInterface = new ModifiedPartDbInterface();
            ModifiedPart items = null;
            string json = "{}";
            try
            {
                items = await dbInterface.SearchAllModifiedParts(id);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            try
            {
                json = JsonConvert.SerializeObject(items.Attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }

        [HttpGet]
        [Route("dropdown/{fieldname}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> CollectDropdownAttributes(string fieldname)
        {
            ModifiedPartDbInterface dbInterface = new ModifiedPartDbInterface();
            List<Option> options = null;
            string json = "{}";
            try
            {
                options = await dbInterface.GetDropDownAttributes(fieldname);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            try
            {
                json = JsonConvert.SerializeObject(options, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
       

    }
}