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
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;


namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/specn")]
    public class SpecificationController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("search")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> Search()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            List<Dictionary<string, SpecificationAttribute>> items = null;
            string json = "{}";
            string specDesc = "";
            string status = "";
            string id = "";
            string specificationName = "";
            string modelName = "";
            string materialCode = "";
            string specificationClass = "";
            string specificationType = "";

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
                    else if (que.Key == "status")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            status = "%";
                        else
                            status = que.Value;
                    }
                    else if (que.Key == "desc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specDesc = "%";
                        else
                            specDesc = que.Value;
                    }
                    else if (que.Key == "name")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationName = "%";
                        else
                            specificationName = que.Value;
                    }
                    else if (que.Key == "modelname")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            modelName = "%";
                        else
                            modelName = que.Value;
                    }
                    else if (que.Key == "materialcode")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            materialCode = "%";
                        else
                            materialCode = que.Value;
                    }
                    else if (que.Key == "class")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationClass = "%";
                        else
                        {
                            if (que.Value.StartsWith("Interim"))
                                specificationClass = "Interim";
                            else
                                specificationClass = que.Value;
                        }
                    }
                    else if (que.Key == "specnType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationType = "%";
                        else
                            specificationType = que.Value;
                    }
                }

                try
                {
                    items = await dbInterface.SearchSpecificationsAsync(specificationType, specificationClass, id, specificationName, specDesc, status, modelName, materialCode);

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

        [HttpGet]
        [Route("search/exact")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> SearchExact()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            List<Dictionary<string, SpecificationAttribute>> items = null;
            string json = "{}";
            string id = "";
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        id = que.Value;
                    }
                }

                try
                {
                    items = await dbInterface.SearchSpecByIdAsync(id);

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
        [HttpGet]
        [Route("specnameduplicate")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> GetModelDuplicate()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            string specName = "";
            int id = 0;
            string specNameDuplicate = String.Empty;
            SpecificationDbInterface specDbInterface = new SpecificationDbInterface();

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        id = int.Parse(que.Value);
                    }
                    if (que.Key == "specname")
                    {
                        specName = que.Value;
                    }
                }

                try
                {
                    specNameDuplicate = await specDbInterface.GetSpecNameDuplicate(specName, id);

                    return Ok(specNameDuplicate);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;

                    return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }
        }

        [HttpGet]
        [Route("getdeletedindicator")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> GetDeletedIndicator()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            int specId = 0;
            int featTypeId = 0;
            string deletedIndicator = String.Empty;
            SpecificationDbInterface specDbInterface = new SpecificationDbInterface();

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "specid")
                    {
                        specId = int.Parse(que.Value);
                    }
                    if (que.Key == "feattypid")
                    {
                        featTypeId = int.Parse(que.Value);
                    }
                }

                try
                {
                    deletedIndicator = specDbInterface.GetDeletedIndicator(specId, featTypeId);

                    return Ok(deletedIndicator);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;

                    return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }
        }

        [HttpGet]
        [Route("weightconversion")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> GetWeightConversion()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            string weight = String.Empty;
            string uomCode = String.Empty;
            string conversion = String.Empty;
            SpecificationDbInterface specDbInterface = new SpecificationDbInterface();

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "weight")
                    {
                        weight = que.Value;
                    }
                    if (que.Key == "uomcd")
                    {
                        uomCode = que.Value;
                    }
                }

                try
                {
                    conversion = await specDbInterface.GetWeightConversion(uomCode);

                    return Ok(conversion);
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

        [Route("{id}/{specType}")]
        [ResponseType(typeof(ISpecification))]
        public async Task<IHttpActionResult> GetSpecification(int id, string specType)
        {
            ISpecification specification = null;
            SpecificationManager manager = new SpecificationManager();
            SpecificationType.Type specificationType = SpecificationType.Type.NOT_SET;
            string json = "{}";

            try
            {
                if (Enum.TryParse<SpecificationType.Type>(specType, out specificationType))
                    specification = await manager.GetSpecification(id, specificationType);
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

            if (specification != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(specification.Attributes); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetMaterial({0})", id);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateSpecification()
        {
            string json = "";
            long specificationId = 0;
            long[] idArray = null;
            bool isGeneric = false;
            JObject item = null;
            SpecificationManager manager = new SpecificationManager();

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

                if (bool.TryParse((string)item.SelectToken("Gnrc.bool"), out isGeneric))
                {
                    if (isGeneric)
                        long.TryParse((string)item.SelectToken("id.value"), out specificationId);
                    else
                        long.TryParse((string)item.SelectToken("RvsnId.value"), out specificationId);
                }
                else
                    long.TryParse((string)item.SelectToken("RvsnId.value"), out specificationId);

                idArray = await manager.PersistSpecification(item, specificationId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", specificationId, json);

                return InternalServerError(ex);
            }

            return Ok("{\"Status\": \"Success\", \"Id\": " + idArray[0] + ", \"SpecWTD\": " + idArray[1] + ", \"MtlWTD\": " + idArray[2] + ", \"MtlItmId\": " + idArray[3] + "}");
        }

        [HttpPost]
        [Route("newassociatepart")]
        public async Task<IHttpActionResult> UpdateNewPartAssociation()
        {
            string json = String.Empty;
            string oldCDMMSId = String.Empty;
            string newCDMMSId = String.Empty;
            string specnID = String.Empty;
            string cuid = String.Empty;
            string newSpecName = String.Empty;
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
                oldCDMMSId = (string)item.SelectToken("oldCDMMSId");
                if (oldCDMMSId == String.Empty)
                {
                    oldCDMMSId = "0";
                }
                newCDMMSId = (string)item.SelectToken("newCDMMSId");
                specnID = (string)item.SelectToken("specnID");
                cuid = (string)item.SelectToken("cuid");
                newSpecName = (string)item.SelectToken("specnm");
                SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                dbInterface.UpdateAssociatedParts(long.Parse(oldCDMMSId), long.Parse(newCDMMSId), long.Parse(specnID), cuid, newSpecName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to determine if root part number {0} exists for mfr_id {1}", 0, 0);

                return InternalServerError();
            }

            return Ok("successful");
        }

        [HttpPost]
        [Route("unassociatepart")]
        public async Task<IHttpActionResult> UnassociatePart()
        {
            string json = String.Empty;
            string oldCDMMSId = String.Empty;
            string specnID = String.Empty;
            string cuid = String.Empty;
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
                oldCDMMSId = (string)item.SelectToken("oldCDMMSId");
                specnID = (string)item.SelectToken("specnID");
                cuid = (string)item.SelectToken("cuid");
                SpecificationDbInterface dbInterface = new SpecificationDbInterface();
                dbInterface.UnassociatePart(long.Parse(oldCDMMSId), long.Parse(specnID), cuid);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to UnassociatePart {0}, {1}, {2}", oldCDMMSId, specnID, cuid);

                return InternalServerError();
            }

            return Ok("successful");
        }

        [HttpGet]
        [Route("searchmtl")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> SearchMaterialToAssociate()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            List<Dictionary<string, SpecificationAttribute>> items = null;
            string json = "{}";
            string specificationType = "";
            string cdmmsID = "";
            string materialCode = "";
            string partNumber = "";
            string vendorCode = "";
            string description = "";
            string recordOnly = "N";
            string roleType = "";
            string cleiValue = "";
            string tableName = "";
            string columnName = "";
            string altColumnName = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "source")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationType = "%";
                        else
                            specificationType = que.Value;
                    }
                    else if (que.Key == "material_item_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cdmmsID = "%";
                        else
                            cdmmsID = que.Value;
                    }
                    else if (que.Key == "product_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            materialCode = "%";
                        else
                            materialCode = que.Value;
                    }
                    else if (que.Key == "mfg_part_no")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            partNumber = "%";
                        else
                            partNumber = que.Value;
                    }
                    else if (que.Key == "mfg_id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            vendorCode = "%";
                        else
                            vendorCode = que.Value;
                    }
                    else if (que.Key == "mat_desc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            description = "%";
                        else
                            description = que.Value;
                    }
                    else if (que.Key == "isRo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            recordOnly = "N";
                        else
                            recordOnly = que.Value;
                    }
                    else if (que.Key == "RoleType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            roleType = "%";
                        else
                            roleType = que.Value;

                    }
                    else if (que.Key == "clei")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cleiValue = "%";
                        else
                            cleiValue = que.Value;

                    }



                }

                try
                {
                    items = await dbInterface.SearchPartsToAssociateAsync(specificationType, materialCode, cdmmsID, vendorCode, partNumber, description, recordOnly, roleType, cleiValue);

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
                    logger.Error(ex, "Unable to serialize response from SpecificationDbInterface.SearchAssociatedPartsAsync");

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
        [Route("getassociatedmtl")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> GetAssociatedMaterial()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            List<Dictionary<string, SpecificationAttribute>> items = null;
            string json = "{}";
            string specificationType = "";
            string tableName = "";
            string rvsID = "";
            string columnName = "";
            string altColumnName = "";
            string recordOnly = "";
            string specID = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "source")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationType = "%";
                        else
                            specificationType = que.Value;
                    }

                    if (que.Key == "RvsnId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            rvsID = "%";
                        else
                            rvsID = que.Value;
                    }

                    if (que.Key == "isRO")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            recordOnly = "N";
                        else
                            recordOnly = que.Value;
                    }
                    if (que.Key == "specId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specID = "";
                        else
                            specID = que.Value;
                    }
                }

                switch (specificationType)
                {
                    case "PLUG_IN":
                        tableName = "RME_PLG_IN_MTRL_REVSN";
                        columnName = "REVSN_NO";
                        altColumnName = "RME_PLG_IN_MTRL_REVSN_ID";
                        break;
                    case "BAY":
                        tableName = "rme_bay_mtrl_revsn";
                        columnName = "rme_bay_mtrl_revsn_id";
                        altColumnName = "bay_specn_revsn_alt_id";
                        break;
                    case "CARD":
                        tableName = "rme_card_mtrl_revsn";
                        columnName = "rme_card_mtrl_revsn_id";
                        altColumnName = "card_specn_revsn_alt_id";
                        break;
                    case "NODE":
                        tableName = "rme_node_mtrl_revsn";
                        columnName = "rme_node_mtrl_revsn_id";
                        altColumnName = "node_specn_revsn_alt_id";
                        break;
                    case "SHELF":
                        tableName = "rme_shelf_mtrl_revsn";
                        columnName = "rme_shelf_mtrl_revsn_id";
                        altColumnName = "shelf_specn_revsn_alt_id";
                        break;
                    case "BAY_EXTENDER":
                        tableName = "rme_bay_extndr_mtrl_revsn";
                        columnName = "rme_bay_extndr_mtrl_revsn_id";
                        altColumnName = "bay_extndr_specn_revsn_alt_id";
                        break;

                }

                try
                {
                    items = await dbInterface.SearchAssociatedPartsAsync(tableName, columnName, rvsID, altColumnName, recordOnly, specID, specificationType);

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
                    logger.Error(ex, "Unable to serialize response from SpecificationDbInterface.SearchAssociatedPartsAsync");

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
        [Route("getndsspecid/{specId}/{specTyp}/{wrkId}")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> GetNDSSpecificationId(long specId, string specTyp, long wrkId)
        {
            ISpecification specification = null;
            SpecificationManager manager = new SpecificationManager();
            SpecificationType.Type specificationType = SpecificationType.Type.NOT_SET;
            string json = "{}";

            try
            {
                if (Enum.TryParse<SpecificationType.Type>(specTyp, out specificationType))
                    specification = await manager.GetSpecification(specId, specificationType);
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

            if (specification != null)
            {
                try
                {
                    json = await manager.PersistNDSSpecificationId(specification, wrkId);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to persist NDS Specification ID for CDMMS ID ({0})", specId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("updatendsspecid/{specId}/{specTyp}/{ndsSpecId}")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> UpdateNDSSpecificationId(long specId, string specTyp, long ndsSpecId)
        {
            ISpecification specification = null;
            SpecificationManager manager = new SpecificationManager();
            SpecificationType.Type specificationType = SpecificationType.Type.NOT_SET;
            string json = "{}";

            try
            {
                if (Enum.TryParse<SpecificationType.Type>(specTyp, out specificationType))
                    specification = await manager.GetSpecification(specId, specificationType);
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

            if (specification != null)
            {
                try
                {
                    json = await manager.PersistNDSSpecificationId(ndsSpecId, specification);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to persist NDS Specification ID for CDMMS ID ({0})", specId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("associatemtl")]
        public async Task<IHttpActionResult> AssociateMaterial()
        {
            string json = "";
            string source = "";
            JObject itemsToAssociate = null;
            ISpecificationDbInterface dbInterface = null;

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
                itemsToAssociate = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (itemsToAssociate != null)
                {
                    source = itemsToAssociate.Value<string>("source");

                    switch (source)
                    {
                        case "BAY":
                            dbInterface = new BaySpecificationDbInterface();

                            break;
                        case "CARD":
                            dbInterface = new CardSpecificationDbInterface();

                            break;
                        case "NODE":
                            dbInterface = new NodeSpecificationDbInterface();

                            break;
                        case "SHELF":
                            dbInterface = new ShelfSpecificationDbInterface();

                            break;
                        case "PLUG_IN":
                            dbInterface = new PlugInSpecificationDbInterface();

                            break;
                        case "BAY_EXTENDER":
                            dbInterface = new BayExtenderSpecificationDbInterface();
                            break;
                    }

                    if (dbInterface != null)
                    {
                        dbInterface.StartTransaction();

                        dbInterface.AssociateMaterial(itemsToAssociate);

                        dbInterface.CommitTransaction();
                    }
                }
            }
            catch (Exception ex)
            {
                dbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to associate material JSON: {0}", json);

                return InternalServerError();
            }

            return Ok("success");
        }

        [HttpGet]
        [Route("associatepart")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> AssociatePartToSpecPart()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            string specificationType = "";
            string specnID = "";
            string materialRevID = "";
            string RevID = "";
            string tableName = "";
            string specnIDColumnName = "";
            string materialRevIDColumnName = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "source")
                    {
                        specificationType = que.Value;
                    }
                    else if (que.Key == "specn_id")
                    {
                        specnID = que.Value;
                    }
                    else if (que.Key == "material_rev_id")
                    {
                        materialRevID = que.Value;
                    }
                    else if (que.Key == "Rev_id")
                    {
                        RevID = que.Value;
                    }
                }

                switch (specificationType)
                {
                    case "BAY":
                        tableName = "rme_bay_mtrl_revsn";
                        specnIDColumnName = "bay_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_bay_mtrl_revsn_id";
                        break;
                    case "CARD":
                        tableName = "rme_card_mtrl_revsn";
                        specnIDColumnName = "card_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_card_mtrl_revsn_id";
                        break;
                    case "NODE":
                        tableName = "rme_node_mtrl_revsn";
                        specnIDColumnName = "node_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_node_mtrl_revsn_id";
                        break;
                    case "SHELF":
                        tableName = "rme_shelf_mtrl_revsn";
                        specnIDColumnName = "shelf_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_shelf_mtrl_revsn_id";
                        break;
                    case "PLUG_IN":
                        tableName = "RME_PLG_IN_MTRL_REVSN";
                        specnIDColumnName = "REVSN_NO";
                        materialRevIDColumnName = "RME_PLG_IN_MTRL_REVSN_ID";
                        break;
                }

                try
                {
                    await dbInterface.AssociatePartToSpecPartAsync(tableName, specnIDColumnName, materialRevIDColumnName, specnID, materialRevID, RevID, specificationType);
                }
                catch (Exception)
                {
                    return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }

            return Ok("Success");
        }

        [HttpGet]
        [Route("disassociatepart")]
        [ResponseType(typeof(SpecificationAttribute))]
        public async Task<IHttpActionResult> DisassociatePartToSpecPart()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            SpecificationDbInterface dbInterface = new SpecificationDbInterface();
            string specificationType = "";
            string materialRevID = "";
            string tableName = "";
            string specnIDColumnName = "";
            string materialRevIDColumnName = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "source")
                    {
                        specificationType = que.Value;
                    }
                    else if (que.Key == "material_rev_id")
                    {
                        materialRevID = que.Value;
                    }
                }

                switch (specificationType)
                {
                    case "BAY":
                        tableName = "rme_bay_mtrl_revsn";
                        specnIDColumnName = "bay_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_bay_mtrl_revsn_id";
                        break;
                    case "CARD":
                        tableName = "rme_card_mtrl_revsn";
                        specnIDColumnName = "card_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_card_mtrl_revsn_id";
                        break;
                    case "NODE":
                        tableName = "rme_node_mtrl_revsn";
                        specnIDColumnName = "node_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_node_mtrl_revsn_id";
                        break;
                    case "SHELF":
                        tableName = "rme_shelf_mtrl_revsn";
                        specnIDColumnName = "shelf_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_shelf_mtrl_revsn_id";
                        break;
                    case "BAY_EXTENDER":
                        tableName = "rme_bay_extndr_mtrl_revsn";
                        specnIDColumnName = "bay_extndr_specn_revsn_alt_id";
                        materialRevIDColumnName = "rme_bay_extndr_mtrl_revsn_id";
                        break;
                    case "PLUG_IN":
                        tableName = "RME_PLG_IN_MTRL";
                        specnIDColumnName = "PLG_IN_SPECN_ID";
                        materialRevIDColumnName = "MTRL_ID";
                        break;
                }

                try
                {
                    await dbInterface.DisassociatePartToSpecPartAsync(tableName, specnIDColumnName, materialRevIDColumnName, materialRevID, specificationType);
                }
                catch (Exception)
                {
                    return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                return BadRequest("Invalid request sent to the server");
            }

            return Ok("Success");
        }

        [HttpPost]
        [Route("insertdimension/{table}")]
        public async Task<IHttpActionResult> InsertDimension(string table)
        {
            string json = "";
            string status = "";
            JObject item = null;
            SpecificationManager manager = new SpecificationManager();

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

                status = await manager.PersistDimension(item, table);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist dimensions to table {0} from JSON: {1}", table, json);

                return InternalServerError();
            }

            return Ok("{\"Status\": \"" + status + "\"}");
        }

        [HttpGet]
        [Route("getdimensions/{table}")]
        public async Task<IHttpActionResult> GetDimensions(string table)
        {
            string json = "{}";

            try
            {
                if ("bay_itnl_dpth".Equals(table))
                {
                    BayInternalDbInterface dbInterface = new BayInternalDbInterface();
                    Dictionary<long, BayInternalDepth> depths = await dbInterface.GetInternalDepthsAsync();
                    SpecificationAttribute sa = null;

                    if (depths != null)
                    {
                        System.Collections.IDictionaryEnumerator dictionaryEnumerator = depths.GetEnumerator();

                        if (dictionaryEnumerator != null)
                        {
                            object key;
                            Specification obj;

                            while (dictionaryEnumerator.MoveNext())
                            {
                                key = dictionaryEnumerator.Key;
                                obj = (Specification)dictionaryEnumerator.Value;

                                if (sa == null)
                                {
                                    sa = new SpecificationAttribute(true, "BayItnlDpthLst");

                                    sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                }

                                sa.ObjectList.Add(obj.Attributes);
                            }
                        }
                    }

                    try
                    {
                        json = JsonConvert.SerializeObject(sa, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from BayInternalDbInterface.GetDimensions");

                        return InternalServerError();
                    }
                }
                else if ("bay_itnl_wdth".Equals(table))
                {
                    BayInternalDbInterface dbInterface = new BayInternalDbInterface();
                    Dictionary<long, BayInternalWidth> widths = await dbInterface.GetInternalWidthsAsync();
                    SpecificationAttribute sa = null;

                    if (widths != null)
                    {
                        System.Collections.IDictionaryEnumerator dictionaryEnumerator = widths.GetEnumerator();

                        if (dictionaryEnumerator != null)
                        {
                            object key;
                            Specification obj;

                            while (dictionaryEnumerator.MoveNext())
                            {
                                key = dictionaryEnumerator.Key;
                                obj = (Specification)dictionaryEnumerator.Value;

                                if (sa == null)
                                {
                                    sa = new SpecificationAttribute(true, "BayItnlWdthLst");

                                    sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                }

                                sa.ObjectList.Add(obj.Attributes);
                            }
                        }
                    }

                    try
                    {
                        json = JsonConvert.SerializeObject(sa, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from BayInternalDbInterface.GetDimensions");

                        return InternalServerError();
                    }
                }
                else if ("bay_extndr_intl_hgt".Equals(table))
                {
                    BayExtenderSpecificationDbInterface dbInterface = new BayExtenderSpecificationDbInterface();
                    Dictionary<long, BayInternalHeight> heights = await dbInterface.GetExtndInternalHeightAsync();
                    SpecificationAttribute sa = null;

                    if (heights != null)
                    {
                        System.Collections.IDictionaryEnumerator dictionaryEnumerator = heights.GetEnumerator();

                        if (dictionaryEnumerator != null)
                        {
                            object key;
                            Specification obj;

                            while (dictionaryEnumerator.MoveNext())
                            {
                                key = dictionaryEnumerator.Key;
                                obj = (Specification)dictionaryEnumerator.Value;

                                if (sa == null)
                                {
                                    sa = new SpecificationAttribute(true, "BayItnlHghtLst");

                                    sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                }

                                sa.ObjectList.Add(obj.Attributes);
                            }
                        }
                    }

                    try
                    {
                        json = JsonConvert.SerializeObject(sa, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from BayInternalDbInterface.GetDimensions");

                        return InternalServerError();
                    }
                }
                else if ("bay_extndr_intl_wdth".Equals(table))
                {
                    BayExtenderSpecificationDbInterface dbInterface = new BayExtenderSpecificationDbInterface();
                    Dictionary<long, BayInternalWidth> widths = await dbInterface.GetExtndInternalWidthAsync();
                    SpecificationAttribute sa = null;

                    if (widths != null)
                    {
                        System.Collections.IDictionaryEnumerator dictionaryEnumerator = widths.GetEnumerator();

                        if (dictionaryEnumerator != null)
                        {
                            object key;
                            Specification obj;

                            while (dictionaryEnumerator.MoveNext())
                            {
                                key = dictionaryEnumerator.Key;
                                obj = (Specification)dictionaryEnumerator.Value;

                                if (sa == null)
                                {
                                    sa = new SpecificationAttribute(true, "BayItnlWdthLst");

                                    sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                }

                                sa.ObjectList.Add(obj.Attributes);
                            }
                        }
                    }

                    try
                    {
                        json = JsonConvert.SerializeObject(sa, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from BayInternalDbInterface.GetDimensions");

                        return InternalServerError();
                    }
                }
                else if ("bay_extndr_intl_dpth".Equals(table))
                {
                    BayExtenderSpecificationDbInterface dbInterface = new BayExtenderSpecificationDbInterface();
                    Dictionary<long, BayInternalDepth> depths = await dbInterface.GetExtndInternalDepthAsync();
                    SpecificationAttribute sa = null;

                    if (depths != null)
                    {
                        System.Collections.IDictionaryEnumerator dictionaryEnumerator = depths.GetEnumerator();

                        if (dictionaryEnumerator != null)
                        {
                            object key;
                            Specification obj;

                            while (dictionaryEnumerator.MoveNext())
                            {
                                key = dictionaryEnumerator.Key;
                                obj = (Specification)dictionaryEnumerator.Value;

                                if (sa == null)
                                {
                                    sa = new SpecificationAttribute(true, "BayItnlDpthLst");

                                    sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                }

                                sa.ObjectList.Add(obj.Attributes);
                            }
                        }
                    }

                    try
                    {
                        json = JsonConvert.SerializeObject(sa, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from BayInternalDbInterface.GetDimensions");

                        return InternalServerError();
                    }
                }
            }
            catch (Exception ex)
            {
                json = "{\"status\": \"EXCEPTION\"}";
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
            string duplicateSpecNameErrorMessage = "";

            try
            {
                status = await dbInterface.GetSendToNdsStatus(id, "CATALOG_SPEC");

                json = "{\"status\": \"" + status["status"] + "\"";

                if ("ERROR".Equals(status["status"]))
                {
                    if (status["notes"] != null && !string.IsNullOrEmpty(status["notes"]))
                    {
                        duplicateSpecNameErrorMessage = ConfigurationManager.Value(Common.Utility.APPLICATION_NAME.CATALOG_SPEC, "dupSpecNmMessage");

                        if (string.IsNullOrEmpty(duplicateSpecNameErrorMessage))
                            logger.Warn("No value found in database for configuration values [CATALOG_SPEC], [dupSpecNmMessage]");
                        else
                        {
                            string[] messageArray = duplicateSpecNameErrorMessage.Split(new string[] { "XXX" }, StringSplitOptions.None);

                            if (messageArray.Count() == 2)
                            {
                                if (status["notes"].StartsWith(messageArray[0]))
                                {
                                    string[] nextWord = messageArray[1].Split(new char[] { ' ' });
                                    long ndsSpecId = 0;
                                    int startIndex = messageArray[0].Length;
                                    int lengthOfId = 15;
                                    bool success = false;

                                    while (!success && lengthOfId > 0)
                                    {
                                        success = long.TryParse(status["notes"].Substring(startIndex, lengthOfId), out ndsSpecId);
                                        lengthOfId--;
                                    }

                                    if (success)
                                    {
                                        json += ", \"notes\": \"" + "Process Failed: Specification already exists in NDS with the same name. Specifications are not allowed with duplicate names. Updating existing specification names may also cause updates to NDS templates. Would you like to update the existing specification?" + "\"";

                                        json += ", \"ndsId\": " + ndsSpecId;
                                    }
                                    else
                                    {
                                        json += ", \"notes\": \"" + status["notes"] + "\"";

                                        logger.Error("Unable to parse NDS Specification ID from response: {0}", status["notes"]);
                                    }
                                }
                                else
                                    json += ", \"notes\": \"" + status["notes"] + "\"";
                            }
                            else
                                logger.Warn("Verify value found in database for configuration values [CATALOG_SPEC], [dupSpecNmMessage]");
                        }
                    }
                }
                else
                {
                    if (status["notes"] != null && !string.IsNullOrEmpty(status["notes"]))
                        json += ", \"notes\": \"" + status["notes"] + "\"";
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

        // newly added method by rxjohn
        [HttpGet]
        [Route("getAlias/{id}/{specType}")]
        [ResponseType(typeof(SpecificationAlias))]
        public async Task<IHttpActionResult> GetAliasDtls(int id, string specType)
        {
            List<SpecificationAlias> alias = null;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            string json = "{}";

            try
            {

                alias = await dbInteraction.GetAliasAsync(id, specType);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (alias != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(alias); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetMaterial({0})", id);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        // newly added method by rxjohn for getting slot Assignment details 
        [HttpGet]
        [Route("getSlotAssign/{shelfId}")]
        [ResponseType(typeof(SpecificationAlias))]
        public async Task<IHttpActionResult> GetSlotAssignDtls(int shelfId)
        {
            List<SlotAssignment> slots = null;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            string json = "{}";

            try
            {

                slots = await dbInteraction.GetSlotAssignAsync(shelfId);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (slots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(slots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetSlotAssignAsync({0})", shelfId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        // newly added method by rxjohn for updating slot Assignment details 
        [HttpPost]
        [Route("UpdateSlotAssign")]
        [ResponseType(typeof(SpecificationAlias))]
        public async Task<IHttpActionResult> UpdateSlotAssignDtls()
        {
            string json = "";
            string actionCode = "";
            int shelfId = 0;
            string str = "";
            dynamic item = null;
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
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
                shelfId = (Int32)item.shelfId;
                item = item["slotDtls"];
                List<SlotAssignment> list = JsonConvert.DeserializeObject<List<SlotAssignment>>(Convert.ToString(item));

                str = await dbInterface.UpdateSlotAssignAsync(actionCode, shelfId, list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        [HttpGet]
        [Route("deleteShelfSlotAssign/{defId}/{actionCode}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteSlotAssignDtls(int defId, string actionCode)
        {
            string response = string.Empty;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            try
            {

                response = await dbInteraction.deleteSlotAssignAsync(defId, actionCode);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(response);
        }

        // added for including delete functionality for slot assignment screen.
        [HttpGet]
        [Route("deleteCardSlotAssign/{defId}/{actionCode}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteCardSlotAssign(int defId, string actionCode)
        {
            string response = string.Empty;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            try
            {

                response = await dbInteraction.deleteCardSlotAssignAsync(defId, actionCode);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("UpdateCardtoSlotAssign")]
        [ResponseType(typeof(SpecificationAlias))]
        // this function is added for Card - Slot assignment functionality
        public async Task<IHttpActionResult> UpdateCardtoSlotAssignDtls()
        {
            string json = "";
            string actionCode = "";
            int cardId = 0;
            string str = "";
            dynamic item = null;
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
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
                cardId = (Int32)item.cardId;
                item = item["slotDtls"];
                List<SlotAssignment> list = JsonConvert.DeserializeObject<List<SlotAssignment>>(Convert.ToString(item));

                str = await dbInterface.UpdateCardSlotAssignAsync(actionCode, cardId, list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        // added for viewing card to slot assignments.
        [HttpGet]
        [Route("getCardtoSlotAssign/{cardId}")]
        [ResponseType(typeof(SpecificationAlias))]
        public async Task<IHttpActionResult> GetCardtoSlotAssignDtls(int cardId)
        {
            List<SlotAssignment> slots = null;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            string json = "{}";

            try
            {

                slots = await dbInteraction.GetCardtoSlotAssignAsync(cardId);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (slots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(slots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardtoSlotAssignAsync({0})", cardId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        //function to populate Port types dropdown list in Port Sequence screen.
        [HttpGet]
        [Route("GetPortTypes/{portRoleTypeId}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetPortTypesDtls(int portRoleTypeId)
        {
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await dbInterface.GetPortTypes(portRoleTypeId);
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
                logger.Error(ex, "Unable to serialize response from dbInterface.GetPortTypes(portRoleType) ({0})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("SpecUseTypes/{SpecId}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> SpecUseTypes(int SpecId)
        {
            PortSpecificationDbInterface dbInterface = new PortSpecificationDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await dbInterface.GetSpecUseTypes(SpecId);
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
                logger.Error(ex, "Unable to serialize response from dbInterface.GetSpecUseTypes() ({0})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to populate connector types dropdown list in Port Sequence screen.
        [HttpGet]
        [Route("GetConnectorTypes")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetConnectorTypesDtls()
        {
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await dbInterface.GetConnectorTypes();
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
                logger.Error(ex, "Unable to serialize response from dbInterface.GetConnectorTypes() ({0})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }


        [HttpPost]
        [Route("UpdateCardtoPortAssign")]
        [ResponseType(typeof(SpecificationAlias))]
        // this function is added for Card - Slot assignment functionality
        public async Task<IHttpActionResult> UpdateCardtoPortAssignDtls()
        {
            string json = "";
            string actionCode = "";
            int cardId = 0;
            string str = "";
            dynamic item = null;
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
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
                cardId = (Int32)item.cardId;
                item = item["portDtls"]; // check this variable name. 
                List<PortAssignment> list = JsonConvert.DeserializeObject<List<PortAssignment>>(Convert.ToString(item));

                str = await dbInterface.UpdateCardPortAssignAsync(actionCode, cardId, list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        // added for viewing card to slot assignment details in Port sequence screen.
        [HttpGet]
        [Route("getCardtoPortAssign/{cardId}")]
        [ResponseType(typeof(PortAssignment))]
        public async Task<IHttpActionResult> GetCardtoPortAssignDtls(int cardId)
        {
            List<PortAssignment> slots = null;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            string json = "{}";

            try
            {

                slots = await dbInteraction.GetCardtoPortAssignAsync(cardId);


            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (slots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(slots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardtoSlotAssignAsync({0})", cardId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        // added for including delete functionality for port sequence assignment screen.
        [HttpGet]
        [Route("deleteCardPortAssign/{cardId}/{actionCode}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteCardPortAssign(int cardId, string actionCode)
        {
            string response = string.Empty;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            try
            {

                response = await dbInteraction.deleteCardPortAssignAsync(cardId, actionCode);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(response);
        }

        // newly added method by rxjohn for getting slot Assignment details 
        [HttpGet]
        [Route("getQualifiedSlots/{depth}/{height}/{width}")]
        [ResponseType(typeof(List<SlotAssignment>))]
        public async Task<IHttpActionResult> getQualifiedSlotsDtls(float depth, float height, float width)
        {
            List<SlotAssignment> slots = null;
            SlotSpecificationDbInterface dbInteraction = new SlotSpecificationDbInterface();

            string json = "{}";

            try
            {

                slots = await dbInteraction.GetQualifiedSlotsForCardAsync(depth, height, width);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (slots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(slots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from GetQualifiedSlotsForCardAsync.GetSlotAssignAsync({depth,height,width})");

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("UpdateCardtoQualifiedSlotAssign")]
        [ResponseType(typeof(SpecificationAlias))]
        // this function is added for Card - qualified slot assignment functionality
        public async Task<IHttpActionResult> UpdateCardtoQualifiedSlotAssignDtls()
        {
            string json = "";
            string actionCode = "";
            int cardId = 0;
            int slotId, crdStPs = 0;
            string str = "";
            dynamic item = null;
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
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
                cardId = (Int32)item.cardId;
                slotId = (Int32)item.slotId; // check this variable name. 
                crdStPs = (Int32)item.cardStartPos;

                str = await dbInterface.UpdateCardtoQualifiedSlotsAssignAsync(actionCode, cardId, slotId, crdStPs);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        //function to populate card position types dropdown list in Add Card to Slot functionality.
        [HttpGet]
        [Route("GetCardPositionTypes")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetCardPositionTypesDtls()
        {
            SlotSpecificationDbInterface dbInterface = new SlotSpecificationDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await dbInterface.GetCardPositionTypes();
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
                logger.Error(ex, "Unable to serialize response from dbInterface.GetCardPositionTypes()", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("getCardtoPluginAssign/{cardId}")]
        [ResponseType(typeof(CardPluginAssignment))]
        public async Task<IHttpActionResult> GetCardtoPluginAssignDtls(int cardId)
        {
            List<CardPluginAssignment> slots = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                slots = await dbInteraction.GetCardtoPluginAssignAsync(cardId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (slots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(slots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardtoSlotAssignAsync({0})", cardId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("UpdateCardtoPluginAssign")]
        [ResponseType(typeof(string))]
        // this function is added for Card - Slot assignment functionality
        public async Task<IHttpActionResult> UpdateCardtoPluginAssignDtls()
        {
            string json = "";
            string actionCode = "";
            int cardId = 0;
            string str = "";
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                cardId = (Int32)item.cardId;
                item = item["plginDtls"]; // check this variable name. 
                List<CardPluginAssignment> list = JsonConvert.DeserializeObject<List<CardPluginAssignment>>(Convert.ToString(item));

                str = await dbInterface.UpdateCardPluginAssignAsync(actionCode, cardId, list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }
        [HttpGet]
        [Route("getPlugInroleTypes")]
        [ResponseType(typeof(PlugInSpecificationDbInterface.RoleTypeList))]
        public async Task<IHttpActionResult> getPlugInroleTypes()
        {

            PlugInSpecificationDbInterface dbInteraction = new PlugInSpecificationDbInterface();
            List<PlugInSpecificationDbInterface.RoleTypeList> plgnRole = null;
            string json = "{}";

            try
            {
                plgnRole = await dbInteraction.GetPlugInRoleTypesList();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (plgnRole != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(plgnRole); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardtoSlotAssignAsync");

                    return InternalServerError();
                }
            }

            return Ok(json);
        }
        [HttpGet]
        [Route("getCardhasSlotDtls/{cardSpecId}")]
        [ResponseType(typeof(CardSpecificationDbInterface.cardPortsDtls))]
        public async Task<IHttpActionResult> GetCardhasSlotDtls(int cardSpecId)
        {
            List<CardSpecificationDbInterface.CardHasSlots> cardSlotDtls = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                cardSlotDtls = await dbInteraction.GetSlotList(cardSpecId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardSlotDtls != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardSlotDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetSlotList({0})", cardSpecId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("getCardSpecnWithSlots/{slotQuantity}/{slotDefId}/{SlotType}/{SlotSpecId}")]
        [ResponseType(typeof(CardSpecificationDbInterface.ManageSlots))]
        public async Task<IHttpActionResult> GetCardSpecnWithSlots(int slotQuantity, int slotDefId, string SlotType, int SlotSpecId)
        {
            List<CardSpecificationDbInterface.ManageSlots> cardMangeSlots = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                cardMangeSlots = await dbInteraction.GetCardSpecnWithSlots(slotQuantity, slotDefId, SlotType, SlotSpecId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardMangeSlots != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardMangeSlots); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetCardSpecnWithSlots({0})", slotDefId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("insertSelectedCardSlot")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> insertSelectedCardSlot()
        {
            string json = "";
            int cardId = 0;
            int slotSpecId = 0;
            int seqNum = 0;
            int pSlotQty = 0;
            int pSlotDefid = 0;
            string actionCode = string.Empty;
            string response = string.Empty;

            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                cardId = (int)item.cardId;
                slotSpecId = (int)item.slotSpecId;
                seqNum = (int)item.seqNum;
                pSlotQty = (int)item.SlotQty;
                actionCode = item.actionCode;
                pSlotDefid = (int)item.SlotDefid;

                response = await dbInterface.insertCardSlot(cardId, slotSpecId, seqNum, pSlotQty, pSlotDefid, actionCode);

                if (response != null)
                {
                    try
                    {
                        json = JsonConvert.SerializeObject(response); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.insertCardSlot():{}");

                        return InternalServerError();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("saveCardSlotsDtls")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> saveCardSlotsDtls()
        {
            string json = "";
            int cardId = 0;
            List<CardSpecificationDbInterface.CardHasSlots> CardSlotDtls = null;
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                cardId = (int)item.cardId;
                item = item["slotDtls"]; // check this variable name. 
                List<CardSpecificationDbInterface.CardHasSlots> list = JsonConvert.DeserializeObject<List<CardSpecificationDbInterface.CardHasSlots>>(Convert.ToString(item));

                CardSlotDtls = await dbInterface.saveCardSlotDtls(list, cardId);

                if (CardSlotDtls != null)
                {
                    try
                    {
                        json = JsonConvert.SerializeObject(CardSlotDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.saveCardSlotsDtls():{}");

                        return InternalServerError();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", json);
                return InternalServerError(ex);
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("saveCardSpecnSlots")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> saveCardSpecnSlots()
        {
            // mwj: change route name to better reflect what it is doing (replaces "insertSlotManageDtls") 
            string json = "";
            int cardId = 0;
            string str = "";
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                item = item["slotMngDtls"]; // check this variable name. 
                List<CardSpecificationDbInterface.ManageSlots> list = JsonConvert.DeserializeObject<List<CardSpecificationDbInterface.ManageSlots>>(Convert.ToString(item));
                str = await dbInterface.insertUpdateManageList(list, cardId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification from JSON: {0}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        [HttpPost]
        [Route("cardSlotSeqvalidations")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> cardSlotSeqvalidations()
        {
            // mwj: change route name to better reflect what it is doing (replaces "insertSlotManageDtls") 
            string json = "";
            string reqType = "";
            decimal validationVal = 0;
            string str = string.Empty;
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                reqType = item.reqType;
                validationVal = (decimal)item.validationVal;
                str = await dbInterface.CardSlotSeqvalidations(reqType, validationVal);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification from JSON: {0}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        [HttpPost]
        [Route("getSlotQuantity")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> getSlotQuantity()
        {
            string json = "";
            int slotDefId = 0;
            int slotQntity = 0;
            dynamic item = null;
            string str = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                slotDefId = item.slotDefId;
                slotQntity = item.cardSlotQntity;

                str = await dbInterface.getCardSlotQuantity(slotDefId, slotQntity);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", slotDefId, json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        [HttpPost]
        [Route("getcardslotseqdtls")]
        [ResponseType(typeof(CardSpecificationDbInterface.cardSlotsSeq))]
        public async Task<IHttpActionResult> getCardSlotSeqDtls()
        {
            string json = "";
            string specname = "";
            string specdesc = "";
            string revname = "";
            string status = "";
            dynamic item = null;
            List<CardSpecificationDbInterface.cardSlotsSeq> cardSlotsSeqDtls = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                specname = (string)item.specname;
                specdesc = (string)item.specdesc;
                revname = (string)item.revname;
                status = (string)item.status;

                cardSlotsSeqDtls = await dbInterface.GetCardSlotSeqList(specname, specdesc, revname, status);

                if (cardSlotsSeqDtls != null)
                {
                    try
                    {
                        json = JsonConvert.SerializeObject(cardSlotsSeqDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetCardSlotSeqList()");

                        return InternalServerError();
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", specname, json);
                return InternalServerError(ex);
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("getCardhasPortDtls/{cardSpecId}")]
        [ResponseType(typeof(CardSpecificationDbInterface.cardPortsDtls))]
        public async Task<IHttpActionResult> GetCardHasPortDtls(int cardSpecId)
        {
            List<CardSpecificationDbInterface.cardPortsDtls> cardPortsDtls = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                cardPortsDtls = await dbInteraction.GetCardPortsList(cardSpecId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardPortsDtls != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardPortsDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetCardPortsList({0})", cardSpecId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("getcardPortTypeDtls")]
        [ResponseType(typeof(CardSpecificationDbInterface.cardPortTypeList))]
        public async Task<IHttpActionResult> getcardPortTypeDtls()
        {
            string json = "";
            string PortType = "";
            string PortRole = "";
            string PortDesc = "";
            string dualPort = "";
            string Transrate = "";
            string AssignPorts = "";
            dynamic item = null;
            List<CardSpecificationDbInterface.cardPortTypeList> cardPortDtls = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                PortType = (string)item.PortType;
                PortRole = (string)item.PortRole;
                PortDesc = (string)item.PortDesc;
                dualPort = (string)item.dualPort;
                Transrate = (string)item.Transrate;
                AssignPorts = (string)item.AssignPorts;

                cardPortDtls = await dbInterface.GetCardSPortTypeList(PortType, PortRole, PortDesc, dualPort, Transrate, AssignPorts);

                if (cardPortDtls != null)
                {
                    try
                    {
                        json = JsonConvert.SerializeObject(cardPortDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetCardSlotSeqList()");

                        return InternalServerError();
                    }
                }
                else
                {
                    json = null;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", PortType, json);
                return InternalServerError(ex);
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("getcardCnctrTypeDtls")]
        [ResponseType(typeof(CardSpecificationDbInterface.cardCnctrTypeList))]
        public async Task<IHttpActionResult> getcardCnctrTypeDtls()
        {
            string json = "";
            string cnctrName = "";
            string cnctrAliases = "";
            string cnctrDesc = "";

            dynamic item = null;
            List<CardSpecificationDbInterface.cardCnctrTypeList> cardCnctrDtls = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                cnctrName = (string)item.cnctrName;
                cnctrAliases = (string)item.cnctrAliases;
                cnctrDesc = (string)item.cnctrDesc;

                cardCnctrDtls = await dbInterface.GetCardSCnctrTypeList(cnctrName, cnctrAliases, cnctrDesc);

                if (cardCnctrDtls != null)
                {
                    try
                    {
                        json = JsonConvert.SerializeObject(cardCnctrDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to serialize response from CardSpecificationDbInterface.GetCardSCnctrTypeList()");

                        return InternalServerError();
                    }
                }
                else
                {
                    json = null;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification id {0} from JSON: {1}", cnctrName, json);
                return InternalServerError(ex);
            }

            return Ok(json);
        }


        [HttpPost]
        [Route("updateportDtls")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> updatePortDtls()
        {
            string json = "";
            int portDefId = 0;
            int portTypeId = 0;
            int cnctrTypeId = 0;
            string TypeName = string.Empty;
            dynamic item = null;
            string str = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                portDefId = item.portDefId;
                portTypeId = item.portTypeId;
                cnctrTypeId = item.cnctrTypeId;
                TypeName = item.TypeName;

                str = await dbInterface.updateCardPortDtls(portDefId, portTypeId, cnctrTypeId, TypeName);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist port details update , port def id {0} from JSON: {1}", portDefId, json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }

        [Route("getnumber")]
        [ResponseType(typeof(SpecificationAttribute))]
        public string GetNumberBayExtender()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            BayExtenderSpecificationDbInterface dbInterface = new BayExtenderSpecificationDbInterface();
            string json = "{}";
            string id = String.Empty;
            string choice = String.Empty;
            string returnNumber = String.Empty;

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            id = "";
                        else
                            id = que.Value;
                    }
                    else if (que.Key == "choice")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            choice = "";
                        else
                            choice = que.Value;
                    }
                }

                try
                {
                    returnNumber = dbInterface.GetBayInternalNumberByID(id, choice);

                    if (returnNumber == "")
                        return "0";
                }
                catch (Exception ex)
                {
                    //return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(returnNumber, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SpecificationDbInterface.Search");

                    //return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                //return BadRequest("Invalid request sent to the server");
            }

            return returnNumber;
        }

        [HttpGet]
        [Route("getnumberbayinternal")]
        [ResponseType(typeof(SpecificationAttribute))]
        public string GetNumberBayInternal()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            BayInternalDbInterface dbInterface = new BayInternalDbInterface();
            string json = "{}";
            string id = String.Empty;
            string choice = String.Empty;
            string returnNumber = String.Empty;

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            id = "";
                        else
                            id = que.Value;
                    }
                    else if (que.Key == "choice")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            choice = "";
                        else
                            choice = que.Value;
                    }
                }

                try
                {
                    returnNumber = dbInterface.GetBayInternalNumberByID(id, choice);

                    if (returnNumber == "")
                        return "0";
                }
                catch (Exception ex)
                {
                    //return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(returnNumber, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SpecificationDbInterface.Search");

                    //return InternalServerError();
                }
            }
            else
            {
                logger.Info("Received invalid request. Referrer: {0}. Url: {1}", Request.Headers.Referrer.OriginalString, Request.RequestUri.OriginalString);

                //return BadRequest("Invalid request sent to the server");
            }

            return returnNumber;
        }
        //-- AB48512 Port Configure changes starts--
        [HttpGet]
        [Route("GetCardPortsSeqDtls/{cardPortDefId}")]
        [ResponseType(typeof(CardSpecificationDbInterface.CardPortConfiguration))]
        public async Task<IHttpActionResult> GetCardPortsSeqDtls(int cardPortDefId)
        {
            CardSpecificationDbInterface.cardPortsDtls cardPortSeqDtls = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                cardPortSeqDtls = await dbInteraction.GetCardPortSeqDetails(cardPortDefId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardPortSeqDtls != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardPortSeqDtls); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardPortsSeqDtls({0})", cardPortDefId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("GetCardPortsConfigDtls/{cardPortDefId}")]
        [ResponseType(typeof(CardSpecificationDbInterface.CardPortConfiguration))]
        public async Task<IHttpActionResult> GetCardPortsConfigDtls(int cardPortDefId)
        {
            List<CardSpecificationDbInterface.CardPortConfiguration> cardMangePorts = null;
            CardSpecificationDbInterface dbInteraction = new CardSpecificationDbInterface();

            string json = "{}";

            try
            {
                cardMangePorts = await dbInteraction.GetCardPortConfigList(cardPortDefId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (cardMangePorts != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(cardMangePorts); //, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from SlotSpecificationDbInterface.GetCardPortDetailsList({0})", cardPortDefId);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("SaveCardPortsConfigurations")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> SaveCardPortsConfigurations()
        {
            string json = "";

            string str = "";
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                item = item["portconfigDtls"]; // check this variable name. 
                List<CardSpecificationDbInterface.CardPortConfiguration> list = JsonConvert.DeserializeObject<List<CardSpecificationDbInterface.CardPortConfiguration>>(Convert.ToString(item));

                str = await dbInterface.SaveCardPortConfigurations(list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification from JSON: {0}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }
        [HttpPost]
        [Route("DeleteCardPortsConfigurations")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteCardPortsConfigurations()
        {
            string json = "";

            string str = "";
            dynamic item = null;
            CardSpecificationDbInterface dbInterface = new CardSpecificationDbInterface();

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
                item = item["portconfigDeleteList"]; // check this variable name. 
                List<CardSpecificationDbInterface.CardPortConfiguration> list = JsonConvert.DeserializeObject<List<CardSpecificationDbInterface.CardPortConfiguration>>(Convert.ToString(item));

                str = await dbInterface.DeleteCardPortConfigurations(list);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to persist specification from JSON: {0}", json);
                return InternalServerError(ex);
            }

            return Ok(str);
        }
        //-- AB48512 Port Configure changes ends--
    }
}