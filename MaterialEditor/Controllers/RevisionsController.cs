using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/revisions")]
    public class RevisionsController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("{mtrlId}/{mtlCategoryId}/{featureTypeId?}")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> GetRevisions(long mtrlId, int mtlCategoryId, int featureTypeId = 0)
        {
            List<MaterialItem> revisions = null;
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            IMaterial material = MaterialFactory.GetMaterialClass(mtlCategoryId, featureTypeId, mtrlId);
            
            string json = "{}";

            try
            {
                if (MockDataHelper.UseMockData)
                {
                    long dataId = 112;

                    json = await MockDataHelper.GetData(dataId);

                    return Ok(json);
                }

                if (material is MaterialRevision)
                {
                    revisions = await ((MaterialRevision)material).GetRevisions();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (revisions != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(revisions, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetRevisions({0}, {1}, {2})", mtrlId, mtlCategoryId, featureTypeId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("search/{mtrlId}")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> Search(long mtrlId)
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";
            string description = "";
            string partNumber = "";
            string productId = "";
            string cdmmsid = "";
            string clmc = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "product_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            productId = "%";
                        else
                            productId = que.Value;
                    }
                    else if (que.Key == "mfg_part_no")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            partNumber = "%";
                        else
                            partNumber = que.Value;
                    }
                    else if (que.Key == "mat_desc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            description = "%";
                        else
                            description = que.Value;
                    }
                    else if (que.Key == "mfg_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "%";
                        else
                            clmc = que.Value;
                    }
                    else if (que.Key == "material_item_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cdmmsid = "%";
                        else
                            cdmmsid = que.Value;
                    }
                }
            }

            try
            {
                if (MockDataHelper.UseMockData)
                {
                    long dataId = 111;

                    json = await MockDataHelper.GetData(dataId);

                    return Ok(json);
                }
                items = await dbInterface.SearchMaterialRevisions(mtrlId, cdmmsid, productId, clmc, partNumber, description);

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
                logger.Error(ex, "Unable to serialize response from ConnectorizedCableDbInterface.SearchForCablesToAssociateAsync");

                return InternalServerError();
            }
            //}
            //else
            //{
            //    logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

            //    return BadRequest("Invalid request sent to the server");
            //}

            return Ok(json);
        }

        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> Update()
        {
            string json = "";
            string status = "SUCCESS";
            long materialItemId = 0;
            JArray revisions = null;
            MaterialItemManager manager = new MaterialItemManager();

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
                revisions = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);

                if(revisions != null)
                    status = await manager.PersistMaterialRevisions(revisions);

                if (string.IsNullOrEmpty(status))
                    status = "SUCCESS";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist material item id {0} from JSON: {1}", materialItemId, json);

                return InternalServerError();
            }

            return Ok(status);
        }

        [HttpPost]
        [Route("updateSpec")]
        public async Task<IHttpActionResult> UpdateSpec()
        {
            string json = "";
            string status = "SUCCESS";
            long materialItemId = 0;
            JObject revision = null;
            MaterialItemManager manager = new MaterialItemManager();

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
                revision = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (revision != null)
                {
                    status = await manager.PersistSpecName(revision);
                }

                if (string.IsNullOrEmpty(status))
                    status = "SUCCESS";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update spec name for material item id {0} from JSON: {1}", materialItemId, json);

                return Ok(ex.Message);
            }

            return Ok(status);
        }

        [HttpPost]
        [Route("update/rtprtnbr")]
        public async Task<IHttpActionResult> UpdateRootPartNumber()
        {
            string json = "";
            string status = "SUCCESS";
            long materialItemId = 0;
            JObject rootPartNumber = null;
            MaterialItemManager manager = new MaterialItemManager();

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content in UpdateRootPartNumber()");

                return InternalServerError();
            }

            try
            {
                rootPartNumber = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (rootPartNumber != null)
                    status = await manager.PersistRootPartNumber(rootPartNumber);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist root part number update {0} from JSON: {1}", materialItemId, json);

                //return InternalServerError(ex);
                return Ok(ex.Message);
            }

            return Ok(status);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateRevision()
        {
            string json = "";
            string[] status = null;
            long materialItemId = 0;
            //long workToDoId = 0;
            long[] idArray = null;
            JObject item = null;
            MaterialItemManager manager = new MaterialItemManager();

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

                long.TryParse((string)item.SelectToken("id.value"), out materialItemId);

                //if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                //{
                //    logger.Error("Unable to parse material item id from JSON: {0}", json);

                //    return InternalServerError();
                //}

                //currentMaterialItem = await GetActiveMaterialItemAsync(materialItemId);

                idArray = await manager.PersistMaterialRevision(item, materialItemId);

                //if (idArray.Length == 4)
                //    status = new string[] { "success", idArray[0].ToString(), idArray[2].ToString(), idArray[3].ToString() };
                //else
                    status = new string[] { "success", idArray[0].ToString(), idArray[1].ToString() };
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist material item id {0} from JSON: {1}", materialItemId, json);

                return InternalServerError(ex);
            }

            //return Ok("{\"Status\": \"Success\", \"Id\": " + workToDoId + "}");
            return Ok(status);
        }
    }
}
