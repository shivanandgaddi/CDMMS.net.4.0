using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Net.Http;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.ShelfModel;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;


namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/specnModel")]
    public class AssignmentController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: shelfModel
        [HttpGet]
        [Route("search")]
        [ResponseType(typeof(shelf))]
        public async Task<IHttpActionResult> GetShelfDetails()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssignmentDbInterface dbInterface = new AssignmentDbInterface();
            List<Dictionary<string, string>> items = null;
            string json = "{}";
            string id = "";


            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            id = "%";
                        else
                            id = que.Value;
                    }
                }

                try
                {
                    items = await dbInterface.SearchSpecificationsAsync(id);

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
                    logger.Error(ex, "Unable to serialize response from SpecificationDbInterface.Search");

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
        [Route("update")]
        public async Task<IHttpActionResult> UpdateAssignment()
        {
            string json = "";
            string actionCode = "";
            string shelfStnumber = "";
            string str = "";
            dynamic item = null;
            AssignmentDbInterface dbiData = new AssignmentDbInterface();
            // List<shelfAssignment> lstshelf = new List<shelfAssignment>();

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
                actionCode = (string)item.actioncode;
                shelfStnumber = (string)item.shelfStnum;
                item = item["ShelfDtls"];
                List<shelf> list = JsonConvert.DeserializeObject<List<shelf>>(Convert.ToString(item));

                str = await dbiData.InsertAssignment(actionCode, shelfStnumber, list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }
        [HttpGet]
        [Route("getNodename/{nodeId}")]
        [ResponseType(typeof(CardPluginAssignment))]
        public async Task<IHttpActionResult> GetNodename(int nodeId)
        {
            string nodename = null;
            AssignmentDbInterface dbAssignament = new AssignmentDbInterface();

            string json = "{}";

            try
            {
                nodename = await dbAssignament.GetnodeNamefromdb(nodeId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (nodename != null || nodename != "")
            {
                try
                {
                    json = JsonConvert.SerializeObject(nodename); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from dbAssignament.GetnodeNamefromdb({0})", nodeId);

                    return InternalServerError();
                }
            }

            return Ok(nodename);
        }
        [HttpGet]
        [Route("getCardname/{cardId}")]
        [ResponseType(typeof(CardPluginAssignment))]
        public async Task<IHttpActionResult> GetCardname(int cardId)
        {
            string cardname = null;
            AssignmentDbInterface dbAssignament = new AssignmentDbInterface();

            string json = "{}";

            try
            {
                cardname = await dbAssignament.GetcardNamefromdb(cardId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardname != null || cardname != "")
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardname); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from dbAssignament.GetnodeNamefromdb({0})", cardId);

                    return InternalServerError();
                }
            }

            return Ok(cardname);
        }
    }
}
