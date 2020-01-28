using System;
using System.Collections.Generic;
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
    [RoutePrefix("api/variablecable")]
    public class VariableLengthCableController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("{id}")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> GetCableInformation(long id)
        {
            List<MaterialItem> material = null;
            ConnectorizedCableDbInterface dbInterface = new ConnectorizedCableDbInterface();
            string json = "{}";

            try
            {
                if (MockDataHelper.UseMockData)
                {
                    long dataId = 223;

                    json = await MockDataHelper.GetData(dataId);

                    return Ok(json);
                }

                material = await dbInterface.GetAssociatedCable(id);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (material != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(material, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetMaterial({0})", id);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("search")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> Search()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            ConnectorizedCableDbInterface dbInterface = new ConnectorizedCableDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";
            string description = "";
            string partNumber = "";
            string productId = "";
            string source = "";
            string cdmmsid = "";
            string clmc = "";
            //source: 'ro', material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp

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
                    else if (que.Key == "source")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            source = "%";
                        else
                            source = que.Value;
                    }
                }

                try
                {
                    items = await dbInterface.SearchForCablesToAssociateAsync(productId, partNumber, description, clmc, cdmmsid, source);

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
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("update/{source}/{id}")]
        public async Task<IHttpActionResult> UpdateAssociation(string source, long id)
        {
            string json = "";
            string status = "";
            long materialItemId = -1;
            JArray itemsToAssociate = null;
            ConnectorizedCableDbInterface dbInterface = null;

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();

                json = json.Replace("\\", "").Trim('"');
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content");

                return InternalServerError();
            }

            try
            {
                itemsToAssociate = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);

                if (itemsToAssociate != null)
                {
                    dbInterface = new ConnectorizedCableDbInterface();

                    dbInterface.StartTransaction();

                    if ("ro".Equals(source)) //Clear all associated children. 
                        status = await dbInterface.UpdateCableAssociation(id, 0, source);

                    for (int i = 0; i < itemsToAssociate.Count; i++)
                    {
                        if(long.TryParse(itemsToAssociate[i].Value<string>(), out materialItemId))
                            status = await dbInterface.UpdateCableAssociation(materialItemId, id, source);
                    }

                    dbInterface.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                dbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to persist material item id {0} from JSON: {1}", materialItemId, json);

                return InternalServerError();
            }

            return Ok("Success");
        }
    }
}
