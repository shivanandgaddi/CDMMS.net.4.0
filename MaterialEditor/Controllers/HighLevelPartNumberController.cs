using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;


namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/highlevelpart")]

    public class HighLevelPartNumberController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("getCIThlp/{id}")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> GetContainedInTermsHLP(int id)
        {
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();
            List<Dictionary<string, Models.Attribute>> items = null;
            string json = "{}";

            if (id > 0)
            {
                try
                {
                    items = await dbInterface.GetContainedInTermsHLPList(id);
                    if (items == null)
                        return Ok("no_results");
                }
                catch (Exception)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from HighLevelPartNumberDbInterface.GetContainedInTermsHLPList({0})", id);
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
        [Route("getRelatedhlp/{materialItemID}/{featureTypeID}")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> GetRelatedToPartNumbers(int materialItemID, int featureTypeID)
        {
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();
            List<string> items = null;
            string json = "{}";

            if (materialItemID > 0)
            {
                try
                {
                    items = await dbInterface.GetRelatedToPartNumbers(materialItemID, featureTypeID);

                    if (items == null)
                        return Ok("no_results");
                }
                catch (Exception)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from HighLevelPartNumberDbInterface.GetContainedInTermsHLPList({0})", materialItemID);
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
        [Route("search/ro")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> Search()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();
            List<MaterialItem> items = new List<MaterialItem>();
            string json = "{}";
            string cdmmsid = "";
            string productId = "";
            string partNumber = "";
            string clmc = "";
            string description = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "cdmmsid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cdmmsid = "";
                        else
                            cdmmsid = que.Value;
                    }
                    else if (que.Key == "PrdctId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            productId = "";
                        else
                            productId = que.Value;
                    }
                    else if (que.Key == "PrtNo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            partNumber = "";
                        else
                            partNumber = que.Value;
                    }
                    else if (que.Key == "clmc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "";
                        else
                            clmc = que.Value;
                    }
                    else if (que.Key == "MtlDesc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            description = "";
                        else
                            description = que.Value;
                    }
                }
                try
                {
                    items = await dbInterface.SearchMaterialsForHLP(cdmmsid, productId, partNumber, clmc, description);

                    if (items.Count == 0)
                        return Ok("no_results");
                }
                catch (Exception)
                {
                    return InternalServerError();
                }
                try
                {
                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from HighLevelPartNumberDbInterface.SearchMaterialsForHLP");
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
        [Route("update/{prntMtrlCd}")]
        public async Task<IHttpActionResult> Update(string prntMtrlCd)
        {
            HighLevelPartDbInterface hlpDbInterface = new HighLevelPartDbInterface();
            string json = "{}";
            string status = "";
            string cuid = "";
            JArray citLstNew = null;
            JArray citLstExisting = null;
            JObject item = null;
            string catalogDescription = String.Empty;

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
                citLstNew = (Newtonsoft.Json.Linq.JArray)item["saveCITNew"];
                citLstExisting = (Newtonsoft.Json.Linq.JArray)item["saveCITExisting"];
                cuid = item.Value<string>("cuid");
                catalogDescription = item.Value<string>("catalogDescription");

                json = await hlpDbInterface.UpdateContainedInTermsList(prntMtrlCd, cuid, citLstNew, citLstExisting, catalogDescription);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            //try
            //{
            //    json = JsonConvert.SerializeObject(status, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex, "Unable to serialize response from hlpDbInterface.UpdateContainedInTermsList ({0}, {1})", ex.Message, ex.StackTrace);

            //    return InternalServerError();
            //}

            return Ok(json);
        }

        [HttpPost]
        [Route("validate/{id}/{prntFtrTyp?}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Validate(string id, string prntFtrTyp = "")
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();
            JObject item = null;
            string json = "{}";
            string resultValidated = "";
            JArray HlpLst = null;

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
                HlpLst = (Newtonsoft.Json.Linq.JArray)item["saveCITNew"];

                resultValidated = await dbInterface.ValidateChildHLP(prntFtrTyp, HlpLst);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content");
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(resultValidated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from HighLevelPartNumberDbInterface.ValidateChildHLP");
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("validateMtrlCd/{mtrlCd}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> ValidateMaterialCode(string mtrlCd)
        {
            HighLevelPartDbInterface dbInterface = new HighLevelPartDbInterface();
            string json = "{}";
            bool validMtrlCd ;
            string messageResult="",hlpCdmmsId="";

            try
            {
                validMtrlCd = await dbInterface.ValidHlpMaterialCode(mtrlCd);

                if (validMtrlCd)
                {
                    hlpCdmmsId = await dbInterface.GetCdmmsIdOfMtrlCd(mtrlCd);

                    if (hlpCdmmsId == "")
                    {
                        messageResult = "errorCDM";
                    }
                }
                else
                {
                    messageResult = "errorMC";
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            finally
            {
                dbInterface.Dispose();
            }

            try
            {
                if (messageResult == "")
                {
                    json = JsonConvert.SerializeObject(hlpCdmmsId, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                else
                {
                    json = JsonConvert.SerializeObject(messageResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from HighLevelPartNumberDbInterface.ValidHlpMaterialCode");
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("clone/{prntMtrlCd}")]
        public async Task<IHttpActionResult> CloneInsert(string prntMtrlCd)
        {
            HighLevelPartDbInterface hlpDbInterface = new HighLevelPartDbInterface();
            string json = "{}";
            string status = "";
            JArray citLst = null;            
            JObject item = null;

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
                citLst = (Newtonsoft.Json.Linq.JArray)item["cloneCIT"];
                status = await hlpDbInterface.CloneContainedInTermsList(prntMtrlCd, citLst );
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(status, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from hlpDbInterface.UpdateContainedInTermsList ({0}, {1})",
                    ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("ndsStatus/{id}")]
        public async Task<IHttpActionResult> SendToNdsStatus(int id)
        {
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            NameValueCollection status = null;
            string json = "{}";
            string containedInStatus = "";
            string duplicateMacroAssemblyDefinitionErrorMessage = "";

            try
            {
                status = await dbInterface.GetSendToNdsStatus(id, "CATALOG_HLPN");

                json = "{\"status\": \"" + status["status"] + "\"";

                if (!"PROCESSING".Equals(status["status"]))
                {
                    if ("ERROR".Equals(status["status"]))
                    {
                        if (status["notes"] != null && !string.IsNullOrEmpty(status["notes"]))
                        {
                            duplicateMacroAssemblyDefinitionErrorMessage = ConfigurationManager.Value(Common.Utility.APPLICATION_NAME.CATALOG_SVC, "dupMADefinitionMessage");

                            if (string.IsNullOrEmpty(duplicateMacroAssemblyDefinitionErrorMessage))
                                logger.Warn("No value found in database for configuration values [CATALOG_SVC], [dupMADefinitionMessage]");
                            else
                            {
                                string[] messageArray = duplicateMacroAssemblyDefinitionErrorMessage.Split(new string[] { "XXX" }, StringSplitOptions.None);

                                if (messageArray.Count() == 2)
                                {
                                    if (status["notes"].StartsWith(messageArray[0]))
                                    {
                                        string[] nextWord = messageArray[1].Split(new char[] { ' ' });
                                        long ndsMaId = 0;
                                        int startIndex = messageArray[0].Length;
                                        int lengthOfId = 15;
                                        bool success = false;

                                        while (!success && lengthOfId > 0)
                                        {
                                            success = long.TryParse(status["notes"].Substring(startIndex, lengthOfId), out ndsMaId);
                                            lengthOfId--;
                                        }

                                        if (success)
                                        {
                                            json += ", \"notes\": \"" + "Process Failed: Macro Assembly Definition already exists in NDS with the same code. Definitions are not allowed with duplicate names. Updating existing definitions may also cause updates to NDS templates. Would you like to update the existing definition?" + "\"";

                                            json += ", \"ndsId\": " + ndsMaId;
                                        }
                                        else
                                        {
                                            json += ", \"notes\": \"" + status["notes"] + "\"";

                                            logger.Error("Unable to parse Macro Assembly ID from response: {0}", status["notes"]);
                                        }
                                    }
                                    else
                                        json += ", \"notes\": \"" + status["notes"] + "\"";
                                }
                                else
                                    logger.Warn("Verify value found in database for configuration values [CATALOG_SVC], [dupMADefinitionMessage]");
                            }
                        }
                    }
                    else
                    {
                        containedInStatus = await dbInterface.GetHighLevelPartSendToNdsStatus(id, "CATALOG_HLPN");

                        json += ", \"notes\": \"" + status["notes"] + "; " + containedInStatus + "\"";
                    }
                }

                json += "}";

                json = json.Replace("\n", " ");
            }
            catch (Exception ex)
            {
                json = "{\"status\": \"EXCEPTION\"}";
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("updatendsmaid/{mtlItmId}/{maId}/{wtdId}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> UpdateNDSMacroAssemblyDefinitionId(long mtlItmId, long maId, long wtdId)
        {
            MaterialDbInterface dbInterface = null;
            string json = "Success";
            bool hadException = false;
            bool badRequest = false;

            await Task.Run(() =>
            {
                try
                {
                    if (mtlItmId > 0 && maId > 0 && wtdId > 0)
                    {
                        dbInterface = new MaterialDbInterface();

                        dbInterface.UpdateHighLevelPartMADefinitionId(mtlItmId, maId);
                        dbInterface.UpdateHighLevelPartWorkToDo(wtdId, "UPDATE", "STAGED");
                    }
                    else
                    {
                        logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                        badRequest = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception caught in HighLevelPartNumberController.UpdateNDSMacroAssemblyDefinitionId({0}, {1}, {2})", mtlItmId, maId, wtdId);

                    hadException = true;
                }
            });

            if(hadException)
                return InternalServerError();
            else if(badRequest)
                return BadRequest("Invalid request sent to the server");
            else
                return Ok(json);
        }
    }
}