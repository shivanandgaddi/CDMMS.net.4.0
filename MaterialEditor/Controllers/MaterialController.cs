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
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/material")]
    public class MaterialController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("{id}/{source?}")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> GetMaterial(int id, string source = "active")
        {
            //source may be four values:
            //'active' (default value if nothing is sent) -> get the SAP values, any overridden values plus the additional attributes
            //'sap' -> get the raw SAP values
            //'losdb' -> get the LOSDB values
            //'ro' -> Record only
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            IMaterial material = null;
            string json = "{}";

            try
            {
                if ("sap".Equals(source))
                    material = await dbInterface.GetMaterialItemSAPAsync(id);
                else if ("losdb".Equals(source))
                {
                    LOSDBMaterialManager manager = new LOSDBMaterialManager();

                    material = await manager.GetMaterialItemAsync(id);
                }
                else if ("active".Equals(source) || "ro".Equals(source))
                {
                    //if (MockDataHelper.UseMockData)
                    //{
                    //    long dataId = 225;

                    //    json = await MockDataHelper.GetData(dataId);

                    //    return Ok(json);
                    //}

                    MaterialItemManager manager = new MaterialItemManager();

                    material = await manager.GetActiveMaterialItemAsync(id, source);
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

            if (material != null)
            {
                try
                {
                    if ("ro".Equals(source) && id == 0)
                        json = JsonConvert.SerializeObject(material.Attributes);
                    else if ("losdb".Equals(source))
                        json = JsonConvert.SerializeObject(material, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    else
                        json = JsonConvert.SerializeObject(material.Attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetMaterial({0}, {1})", id, source);

                    return InternalServerError();
                }
            }
            else
            {
                if ("losdb".Equals(source))
                    json = "{}";
                else
                    return InternalServerError();
            }

            return Ok(json);
        }

        //losdb search 
        [HttpGet]
        [Route("losdbsearch")]
        [ResponseType(typeof(List<MaterialItem>))]
        public async Task<IHttpActionResult> Search()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";
            string description = "%";
            string partNumber = "";
            string cleicd = "";
            string compatibleclei = "";
            string clmc = "";
            //source: 'ro', material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "part_no")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            partNumber = "%";
                        else
                            partNumber = que.Value;
                    }
                    else if (que.Key == "clei_cd")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cleicd = "%";
                        else
                            cleicd = que.Value;
                    }
                    //else if (que.Key == "item_desc")
                    //{
                    //    if (string.IsNullOrEmpty(que.Value))
                    //        description = "%";
                    //    else
                    //        description = que.Value;
                    //}
                    else if (que.Key == "compatibleequipmentclei7")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            compatibleclei = "%";
                        else
                            compatibleclei = que.Value;
                    }
                    else if (que.Key == "mfr_cd")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "%";
                        else
                            clmc = que.Value;
                    }

                }

                try
                {
                    items = await dbInterface.SearchLosdb(partNumber, cleicd, description, compatibleclei, clmc);

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

        [HttpGet]
        [Route("search/{fieldName}")]
        [ResponseType(typeof(SearchResult))]
        public async Task<IHttpActionResult> Search(string fieldName)
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<SearchResult> items = null;
            string json = "{}";
            string searchValue = "";

            if (query != null && query.Count() == 1)
            {
                searchValue = query.First().Value;

                try
                {
                    items = await dbInterface.SearchMaterialItemAsync(fieldName, searchValue);

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
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.SearchMaterialItemAsync({0}, {1})", fieldName, searchValue);

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
        [Route("searchall")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> SearchAll()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";
            string mtlDesc = "";
            string prtNo = "";
            string prdctId = "";
            string specificationName = "";
            string itemStatus = "";
            string featureType = "";
            string cableType = "";
            string materialCategory = "";
            string status = "";
            string cdmmsid = "";
            string clmc = "";
            //string lastupdt = "";
            string startdt = "";
            string enddt = "";
            string userid = "";
            string heciclei = "";
            string standaloneCleiSearch = "";
            string exactMatch = "N";
            
            if (query != null && query.Count() >0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "PrdctId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            prdctId = "%";
                        else
                            prdctId = que.Value;
                    }
                    else if (que.Key == "PrtNo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            prtNo = "%";
                        else
                            prtNo = que.Value;
                    }
                    else if (que.Key == "MtlDesc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            mtlDesc = "%";
                        else
                        {
                            mtlDesc = que.Value;
                            mtlDesc = mtlDesc.Replace("'", "''");
                        }
                        
                    }
                    else if (que.Key == "clmc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "%";
                        else
                            clmc = que.Value;
                    }
                    else if (que.Key == "cdmmsid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cdmmsid = "-99";
                        else
                            cdmmsid = que.Value;
                    }
                    else if (que.Key == "status")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            status = "%";
                        else
                            status = que.Value;
                    }
                    else if (que.Key == "CableType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cableType = "%";
                        else
                            cableType = que.Value;
                    }
                    else if (que.Key == "FeatureType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            featureType = "%";
                        else
                            featureType = que.Value;
                    }
                    else if (que.Key == "ItemStatus")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            itemStatus = "%";
                        else
                            itemStatus = que.Value;
                    }
                    else if (que.Key == "SpecificationName")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationName = "%";
                        else
                            specificationName = que.Value;
                    }
                    else if (que.Key == "MaterialCategory")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            materialCategory = "%";
                        else
                            materialCategory = que.Value;
                    }
                    //else if (que.Key == "LastUpdate")
                    //{
                    //    if (string.IsNullOrEmpty(que.Value))
                    //        lastupdt = "%";
                    //    else
                    //        lastupdt = que.Value;
                    //}
                    else if (que.Key == "StartDate")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            startdt = "%";
                        else
                            startdt = que.Value;
                    }
                    else if (que.Key == "EndDate")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            enddt = "%";
                        else
                            enddt = que.Value;
                    }
                    else if (que.Key == "UserID")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            userid = "%";
                        else
                            userid = que.Value;
                    }
                    else if (que.Key == "HeciClei")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            heciclei = "%";
                        else
                            heciclei = que.Value;
                    }
                    else if (que.Key == "StandaloneCleiSearch")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            standaloneCleiSearch = "%";
                        else
                            standaloneCleiSearch = que.Value;
                    }
                    else if (que.Key == "ExactMatch")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            exactMatch = "N";
                        else
                            exactMatch = que.Value;
                    }
                }

                #region trim off user entered preceding or trailing '%' signs because these mess up the decode statements and are unnecessary
                if (exactMatch == "N")
                {
                    if (mtlDesc != "%")
                    {
                        mtlDesc = mtlDesc.Trim('%');
                    }
                }
                #endregion

                try
                {
                    items = await dbInterface.SearchMaterialItemAllAsync(prdctId, prtNo, mtlDesc, clmc, cdmmsid, status, cableType, featureType, itemStatus, specificationName, materialCategory,userid, heciclei, standaloneCleiSearch, exactMatch, startdt, enddt);

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
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.SearchAll");

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
        [Route("searchallronew")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> SearchAllRONew()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";
            string mtlDesc = "";
            string prtNo = "";
            string prdctId = "";
            string specificationName = "";
            string featureType = "";
            string cableType = "";
            string materialCategory = "";
            string status = "";
            string cdmmsid = "";
            string clmc = "";
            string lastupdt = "";
            string userid = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "PrdctId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            prdctId = "%";
                        else
                            prdctId = que.Value;
                    }
                    else if (que.Key == "PrtNo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            prtNo = "%";
                        else
                            prtNo = que.Value;
                    }
                    else if (que.Key == "MtlDesc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            mtlDesc = "%";
                        else
                            mtlDesc = que.Value;
                    }
                    else if (que.Key == "clmc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "%";
                        else
                            clmc = que.Value;
                    }
                    else if (que.Key == "cdmmsid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cdmmsid = "%";
                        else
                            cdmmsid = que.Value;
                    }
                    else if (que.Key == "status")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            status = "%";
                        else
                            status = que.Value;
                    }
                    else if (que.Key == "CableType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            cableType = "%";
                        else
                            cableType = que.Value;
                    }
                    else if (que.Key == "FeatureType")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            featureType = "%";
                        else
                            featureType = que.Value;
                    }
                    else if (que.Key == "SpecificationName")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            specificationName = "%";
                        else
                            specificationName = que.Value;
                    }
                    else if (que.Key == "MaterialCategory")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            materialCategory = "%";
                        else
                            materialCategory = que.Value;
                    }
                    else if (que.Key == "LastUpdate")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            lastupdt = "%";
                        else
                            lastupdt = que.Value;
                    }
                    else if (que.Key == "UserID")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            userid = "%";
                        else
                            userid = que.Value;
                    }
                }

                try
                {
                    items = await dbInterface.SearchMaterialItemAllRONewAsync(prdctId, prtNo, mtlDesc, clmc, cdmmsid, status, cableType, featureType, specificationName, materialCategory, lastupdt, userid);

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
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.SearchAllRONew");

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
        [Route("getexistingrootpartro")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> GetExistingRootPartsRO()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            string returnString = String.Empty;
            string json = "{}";
            string prtNo = "";
            string clmc = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "PrtNo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            prtNo = "%";
                        else
                            prtNo = que.Value;
                    }
                    else if (que.Key == "clmc")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            clmc = "%";
                        else
                            clmc = que.Value;
                    }
                }

                try
                {
                    returnString = await dbInterface.GetExistingRootPartsRO(prtNo, clmc);

                    if (returnString == null)
                        return Ok("none_found");
                }
                catch (Exception)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(returnString, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetExistingRootPartsRO");

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

        //private async Task<MaterialItem> GetActiveMaterialItemAsync(long materialItemId)
        //{
        //    MaterialItem activeItem = null;
        //    MaterialItem losdbMaterial = null;
        //    MaterialItem sapMaterial = null;
        //    MaterialDbInterface dbInterface = new MaterialDbInterface();
        //    MaterialDbInterface sapDbInterface = new MaterialDbInterface();
        //    MaterialItemManager manager = new MaterialItemManager();
        //    string sourceOfRecord = string.Empty;
        //    List<Task> tasks = new List<Task>();
        //    List<Models.Attribute> attributes = null;
        //    List<Models.Attribute> overrideAttributes = null;

        //    //How to determine the active material:
        //    //1. Default values are always from SAP.
        //    //2. Check for the source of record in material_item.
        //    //  a. If it is LOSDB, the LOSDB material overrides equivalent SAP values and can not be manually overridden.
        //    //3. SAP values (which are not also LOSDB values if source of record is LOSDB) may be overridden manually. Values are stored in material_item_attributes.
        //    //4. NDS and COEFM additional attributes. Values are stored in material_item_attributes.

        //    sourceOfRecord = dbInterface.GetMaterialSourceOfRecord(materialItemId);

        //    tasks.Add(Task.Run(async () =>
        //    {
        //        losdbMaterial = await dbInterface.GetMaterialItemLOSDBAsync(materialItemId);
        //    }));

        //    tasks.Add(Task.Run(async () =>
        //    {
        //        sapMaterial = await sapDbInterface.GetMaterialItemSAPAsync(materialItemId);
        //    }));

        //    tasks.Add(Task.Run(async () =>
        //    {
        //        attributes = await sapDbInterface.GetAdditionalAttributesAsync(materialItemId);
        //    }));

        //    tasks.Add(Task.Run(async () =>
        //    {
        //        overrideAttributes = await sapDbInterface.GetAttributeOverridesAsync(materialItemId, MaterialType.SourceSystem(SOURCE_SYSTEM.SAP));
        //    }));

        //    Task.WaitAll(tasks.ToArray());

        //    activeItem = manager.GetActiveMaterialItem(materialItemId, sourceOfRecord, sapMaterial, losdbMaterial, attributes, overrideAttributes);

        //    //TODO - if an error occurs in any individual task send error message back to display in that portion of the screen
        //    //(Instead of the "No data found" message, "An internal error occurred while retrieving data")            

        //    return activeItem;
        //}

        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMaterialItem()
        {
            string json = "";
            string[] status = null;
            long materialItemId = 0;
            //long workToDoId = 0;
            long[] idArray = null;
            JObject item = null;
            //MaterialItem currentMaterialItem = null;
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

                idArray = await manager.PersistMaterialItem(item, materialItemId);

                if(idArray.Length == 4)
                    status = new string[] { "success", idArray[0].ToString(), idArray[2].ToString(), idArray[3].ToString(), manager.MaterialSpecificationType };
                else
                    status = new string[] { "success", idArray[0].ToString(), idArray[2].ToString(), "0", manager.MaterialSpecificationType };
            }
            catch (Exception ex)
            {
                json = json.Replace("\"", "'").Substring(0, 3900);

                logger.Error(ex, "Unable to persist material item id {0} from JSON: {1}", materialItemId, json);

                //return InternalServerError(ex);
                return Ok(ex.Message);
            }

            //return Ok("{\"Status\": \"Success\", \"Id\": " + workToDoId + "}");
            return Ok(status);
        }

        [HttpGet]
        [Route("ndsStatus/{id}")]
        public async Task<IHttpActionResult> SendToNdsStatus(int id)
        {
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            NameValueCollection status = null;
            string json = "{}";

            try
            {
                status = await dbInterface.GetSendToNdsStatus(id, Constants.ApplicationName(APPLICATION_NAME.CATALOG_UI));

                json = "{\"status\": \"" + status["status"] + "\"";

                if (status["notes"] != null && !string.IsNullOrEmpty(status["notes"]))
                    json += ", \"notes\": \"" + status["notes"] + "\"";

                json += "}";

                json = json.Replace("\n", " ");
            }
            catch (Exception ex)
            {
                json = "{\"status\": \"EXCEPTION\"}";
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("assoclosdb/{id}")]
        public async Task<IHttpActionResult> GetAssociatedLOSDBMaterial(long id)
        {
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetAssociatedLOSDBMaterial(id);

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
                logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetAssociatedLOSDBMaterial({0})", id);

                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("sendToNDS/{id}")]
        public async Task<IHttpActionResult> SendToNDS(long id)
        {
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            long workToDoId = 0;
            //string json = "{}";

            try
            {
                workToDoId = dbInterface.InsertWorkToDo(id, "CATALOG_UI", "MATERIAL_ITEM");
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            try
            {
                //json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetAssociatedLOSDBMaterial({0})", id);

                return InternalServerError();
            }

            return Ok(workToDoId);
        }

        [HttpPost]
        [Route("rtprtnbrexists")]
        public async Task<IHttpActionResult> RevisionExists()
        {
            string exists = "";
            string json = "";
            string partNumber = "";
            long mfrId = 0;
            JObject item = null;
            MaterialDbInterface dbInterface = new MaterialDbInterface();

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

                partNumber = (string)item.SelectToken("prtNbr");

                long.TryParse((string)item.SelectToken("mfrId"), out mfrId);

                exists = await dbInterface.RootPartNumberExists(partNumber, mfrId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to determine if root part number {0} exists for mfr_id {1}", partNumber, mfrId);

                return InternalServerError();
            }

            return Ok(exists);
        }
    }
}
