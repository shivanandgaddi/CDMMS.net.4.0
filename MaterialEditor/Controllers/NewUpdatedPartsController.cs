using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/newupdatedparts")]
    public class NewUpdatedPartsController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/<controller>
        [HttpGet]
        [Route("search")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> Search()
        {
            /*On Click Search. Input values will be taken for searching material in catalog staging*/
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";
            string mtlcode = "", descr = "", partno = "", heci = "", isnew = "", startdate = "", enddate = "", recordtype = "", viewrejected = "", rejectedcuid = "";
            string minnumber = "";
            string maxnumber = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "mtlcode")
                    { mtlcode = que.Value; }
                    else if (que.Key == "partno")
                    { partno = que.Value; }
                    else if (que.Key == "heci")
                    { heci = que.Value; }
                    else if (que.Key == "descr")
                    { descr = que.Value; }
                    else if (que.Key == "isnew")
                    { isnew = que.Value; }
                    else if (que.Key == "startdate")
                    { startdate = que.Value; }
                    else if (que.Key == "enddate")
                    { enddate = que.Value; }
                    else if (que.Key == "recordtype")
                    { recordtype = que.Value; }
                    else if (que.Key == "viewrejected")
                    { viewrejected = que.Value; }
                    else if (que.Key == "rejectedcuid")
                    { rejectedcuid = que.Value; }
                    else if (que.Key == "minsearch")
                    { minnumber = que.Value; }
                    else if (que.Key == "maxsearch")
                    { maxnumber = que.Value; }
                }

                try
                {
                    items = await dbInterface.SearchNewUpdatedPartsAsync(mtlcode, partno, heci, descr, isnew, recordtype, int.Parse(minnumber), int.Parse(maxnumber), startdate, enddate, viewrejected, rejectedcuid);
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
                    logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
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
        [Route("associateLTS/{mId}/{pId}/{eqCtId}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> AssociateLosdbtoSap(string mId, string pId, string eqCtId)
        {
            /*On Click Accept. Selected material code updates will be accepted.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string associatestatus = "";

            try
            {
                associatestatus = await dbInterface.AssociateLosdbtoSap(mId, pId, eqCtId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(associatestatus);
        }

        [HttpGet]
        [Route("associateLTSMtlCd/{mId}/{pId}/{eqptId}/{isNew?}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> AssociateLosdbtoSapMtlCode(string mId, string pId, string eqptId, string isNew = "Y")
        {
            /*On Click Accept. Selected material code updates will be accepted.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string associatestatus = "";

            try
            {
                MaterialItemManager materialItemManager = new MaterialItemManager();

                if (!"Y".Equals(isNew))
                    associatestatus = await materialItemManager.DeleteLOSDBAssociation(long.Parse(mId));

                if(!"D".Equals(isNew) && (string.IsNullOrEmpty(associatestatus) || "SUCCESS".Equals(associatestatus)))
                    associatestatus = materialItemManager.FinishLOSDBAssociation(long.Parse(mId), pId, eqptId);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

            return Ok(associatestatus);
        }

        [HttpGet]
        [Route("associateSTL/{mId}/{pId}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> AssociateSaptoLosdb(string mId, string pId)
        {
            /*On Click Accept. Selected material code updates will be accepted.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string associateSaptolosdb = "";

            try
            {
                associateSaptolosdb = await dbInterface.AssociateSaptoLosdb(mId, pId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(associateSaptolosdb);
        }

        //Sap detail
        [HttpGet]
        [Route("{id}/{recordtype?}")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> GetMaterial(int id, string recordtype = "")
        {
            /*On Click showdetails. Selected material code is taken and detailed updates from material catalog staging will be displayed here.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetDetailInformationStaging(id, recordtype);
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
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        //revision
        [HttpGet]
        [Route("revision/{rid}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetRevision(int rid)
        {
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetRevisionData(rid);
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
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        //insertrevision
        [HttpGet]
        [Route("insertrevision/{rid}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetInsertRevision(int rid)
        {
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string insertrevision = "";

            try
            {
                insertrevision = await dbInterface.GetInsertRevisionData(rid);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(insertrevision);
        }

        //LOSDB DEtail
        [HttpGet]
        [Route("losdb/{id}")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> GetMaterialLosdb(int id)
        {
            /*On Click showdetails. Selected material code is taken and detailed updates from material catalog staging will be displayed here.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetDetailInformationStagingLosdb(id);
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
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }       
     
        [HttpGet]
        [Route("possiblesap/{mid}")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> GetPossibleSap(int mid)
        {
            /*On Click showdetails. Selected material code is taken and detailed updates from material catalog staging will be displayed here.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetPossibleMatchSap(mid);
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
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("possiblesap/{mcode}")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> GetMaterialID(string mcode)
        {
            /*On Click showdetails. Selected material code is taken and detailed updates from material catalog staging will be displayed here.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string materialID = "";
            string json = "{}";

            try
            {
                materialID = await dbInterface.GetMaterialID(mcode);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(materialID, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.GetMaterialID({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        //Possible match LOSDB
        [HttpGet]
        [Route("possiblelosdb/{pid}")]
        [ResponseType(typeof(NewUpdatedParts))]
        public async Task<IHttpActionResult> GetPossibleLosdb(int pid)
        {
            /*On Click showdetails. Selected material code is taken and detailed updates from material catalog staging will be displayed here.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            List<NewUpdatedParts> items = null;
            string json = "{}";

            try
            {
                items = await dbInterface.GetPossibleMatchLosdb(pid);
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
                logger.Error(ex, "Unable to serialize response from newupdatedpartsDbInterface.SearchNewUpdatedPartsAsync({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("reject/{mId}/{cuid}/{recordtype?}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> RejectUpdatedParts(string mId, string cuid, string recordtype = "")
        {
            /*On Click Reject. Selected material code updates will be rejected.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string rejectStatus = "";

            try
            {
                rejectStatus = await dbInterface.RejectUpdatedPartsDB(mId, cuid, recordtype);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(rejectStatus);
        }

        [HttpGet]
        [Route("newreject")]
        public async Task<IHttpActionResult> NewReject()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            //MaterialItemManager materialItemManager = new MaterialItemManager();
            //string json = "{}";
            string materialcode = String.Empty;
            string cuid = String.Empty;
            string recordtype = String.Empty;

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "materialcode")
                    {
                        materialcode = que.Value;
                    }
                    else if (que.Key == "cuid")
                    {
                        cuid = que.Value;
                    }
                    else if (que.Key == "recordtype")
                    {
                        recordtype = que.Value;
                    }
                }
            }

            /*On Click Reject. Selected material code updates will be rejected.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string rejectStatus = "";

            try
            {
                rejectStatus = await dbInterface.RejectUpdatedPartsDB(materialcode, cuid, recordtype);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok(rejectStatus);
        }

        [HttpPost]
        [Route("accept/{mId}/{cuid}/{isnew}/{losdbprodid}/{losdbeqptid}/{clmc}/{recordtype?}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> AcceptUpdatedParts(string mId, string cuid, string isnew, string losdbprodid, string losdbeqptid, string clmc, string recordtype = "")
        {
            /*On Click Accept. Selected material code updates will be accepted.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            JObject jsonObject = null;
            string acceptStatus = "";
            string json = "";
            string desc = "";

            try
            {
                json = await this.Request.Content.ReadAsStringAsync();

                json = json.Replace("\\", "").Trim('"');

                jsonObject = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                desc = jsonObject.Value<string>("mtlDsc");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content");

                return InternalServerError();
            }

            if (losdbprodid == "0")
            {
                losdbprodid = String.Empty;
            }

            if (losdbeqptid == "0")
            {
                losdbeqptid = String.Empty;
            }

            try
            {
                acceptStatus = await dbInterface.AcceptUpdatedPartsDB(mId, cuid, recordtype, isnew, losdbprodid, losdbeqptid, clmc, desc);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok(acceptStatus);
        }

        [HttpGet]
        [Route("newaccept")]
        public async Task<IHttpActionResult> NewAcceptUpdatedParts()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string acceptStatus = "";
            string desc = "";
            string mId = "";
            string cuid = "";
            string isnew = "";
            string losdbprodid = "";
            string losdbeqptid = "";
            string clmc = "";
            string recordtype = "";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "desc")
                    {
                        desc = que.Value;
                    }
                    else if (que.Key == "materialcode")
                    {
                        mId = que.Value;
                    }
                    else if (que.Key == "cuid")
                    {
                        cuid = que.Value;
                    }
                    else if (que.Key == "isnew")
                    {
                        isnew = que.Value;
                    }
                    else if (que.Key == "losdbprodid")
                    {
                        losdbprodid = que.Value;
                    }
                    else if (que.Key == "ieseqptprodid")
                    {
                        losdbeqptid = que.Value;
                    }
                    else if (que.Key == "clmc")
                    {
                        clmc = que.Value;
                    }
                    else if (que.Key == "recordtype")
                    {
                        recordtype = que.Value;
                    }
                }
            }

            if (losdbprodid == "0")
            {
                losdbprodid = String.Empty;
            }

            if (losdbeqptid == "0")
            {
                losdbeqptid = String.Empty;
            }

            try
            {
                acceptStatus = await dbInterface.AcceptUpdatedPartsDB(mId, cuid, recordtype, isnew, losdbprodid, losdbeqptid, clmc, desc);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok(acceptStatus);
        }

        [HttpGet]
        [Route("acceptlosdb/{auditId}/{usraprvText}/{usrTmstp}/{apprvId}/{adtblpkcolnm}/{cdmmscolnm}/{newcolval}/{cdmmstblnm}/{losdbprod}/{adtblpkcolval}/{adprtcolnm}/{adprtcolval?}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> AcceptUpdatedPartslosdb(string auditId, string usraprvText, string usrTmstp, string apprvId, string adtblpkcolnm, string cdmmscolnm, string newcolval, string cdmmstblnm, string losdbprod, string adtblpkcolval, string adprtcolnm, string adprtcolval)
        {
            /*On Click Accept. Selected material code updates will be accepted.*/
            NewUpdatedPartsDbInterface dbInterface = new NewUpdatedPartsDbInterface();
            string acceptStatus = "";

            try
            {

                acceptStatus = await dbInterface.AcceptUpdatedPartsDBlosdb(auditId, usraprvText, usrTmstp, apprvId, adtblpkcolnm, cdmmscolnm, newcolval, cdmmstblnm, losdbprod, adtblpkcolval, adprtcolnm, adprtcolval);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(acceptStatus);
        }

        [HttpGet]
        [Route("partialcheck")]
        [ResponseType(typeof(List<PartialMatch>))]
        public IHttpActionResult CheckPartial()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            MaterialItemManager materialItemManager = new MaterialItemManager();
            List<PartialMatch> partialMatches = new List<PartialMatch>();
            string json = "{}";
            string rootPartNumber = String.Empty;
            string clmc = String.Empty;

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "mfg_part_no")
                    {
                        rootPartNumber = que.Value;
                    }
                    else if (que.Key == "clmc")
                    {
                        clmc = que.Value;
                    }
                }
            }

            try
            {
                partialMatches = materialItemManager.CheckPartialMatches(rootPartNumber, clmc);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(partialMatches, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from materialItemManager.CheckPartialMatches({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("getmaterialid")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetMaterialID()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            string materialItemID = String.Empty;
            string productID = String.Empty;
            string json = "{}";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "productid")
                    {
                        productID = que.Value;
                    }
                }
            }

            try
            {
                materialItemID = newUpdatedPartsDbInterface.GetMaterialItemID(productID);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(materialItemID, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from newUpdatedPartsDbInterface.GetMaterialID({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("insertgenericrevision")]
        [ResponseType(typeof(string))]
        public IHttpActionResult InsertGenericRevision()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NewUpdatedPartsDbInterface newUpdatedPartsDbInterface = new NewUpdatedPartsDbInterface();
            string revisionTableName = String.Empty;
            string materialItemID = String.Empty;
            string materialID = String.Empty;
            string revisionNumber = String.Empty;
            string productID = String.Empty;
            string baseRevisionInd = String.Empty;
            string currentRevisionInd = String.Empty;
            string retiredRevisionInd = String.Empty;
            string returnString = "fail";
            string json = "{}";

            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "revisiontablename")
                    {
                        revisionTableName = que.Value;
                    }
                    else if(que.Key == "materialitemid")
                    {
                        materialItemID = que.Value;
                    }
                    else if (que.Key == "materialid")
                    {
                        materialID = que.Value;
                    }
                    else if (que.Key == "revisionnumber")
                    {
                        revisionNumber = que.Value;
                    }
                    else if (que.Key == "materialcode")
                    {
                        productID = que.Value;
                    }
                    else if (que.Key == "baserevind")
                    {
                        baseRevisionInd = que.Value;
                    }
                    else if (que.Key == "currentrevind")
                    {
                        currentRevisionInd = que.Value;
                    }
                    else if (que.Key == "retrevind")
                    {
                        retiredRevisionInd = que.Value;
                    }
                }
            }

            try
            {
                returnString = newUpdatedPartsDbInterface.InsertGenericRevision(revisionTableName, materialItemID, materialID, revisionNumber,
                    productID, baseRevisionInd, currentRevisionInd, retiredRevisionInd);
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
                logger.Error(ex, "Unable to serialize response from newUpdatedPartsDbInterface.GetMaterialID({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
    }
}