using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/assemblyunit")]
    public class AssemblyUnitController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //function to populate Labor Id dropdown list
        [HttpGet]
        [Route("lbrIdsList")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> CollectlbrIdsList()
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await auDbInterface.GetlbrIdsList();
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetlbrIdsList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to populate selected Labor Id attributes
        [HttpGet]
        [Route("lbrIdAttribute/{id}")]
        [ResponseType(typeof(Models.AssemblyUnit))]
        public async Task<IHttpActionResult> CollectlbrIdAttribute(string id)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            AssemblyUnit LaborIddesc = new AssemblyUnit();
            string json = "{}";
            long laborId;

            try
            {
                if (string.IsNullOrEmpty(id))
                    laborId = 0;
                else
                    laborId = long.Parse(id);

                LaborIddesc = await auDbInterface.GetlbrIdAttributes(laborId);
                LaborIddesc.AttributesMtrlClsfnLst = await auDbInterface.GetlbrIdMtrlClassification(laborId);
                LaborIddesc.AttrLbrIdAssemblyUnitLst = await auDbInterface.GetlbrIdAssemblyUnits(laborId);

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(LaborIddesc, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from one of the labor id attribute functions ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //populate selected Labor Id Calculation Operations
        [HttpGet]
        [Route("calcOperations/{id}")]
        [ResponseType(typeof(Models.AssemblyUnit))]
        public async Task<IHttpActionResult> GetCalculationOperations(string id)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Dictionary<string, Models.Attribute>> operations = new List<Dictionary<string, Models.Attribute>>();
            string json = "{}";
            long calcId;
            try
            {
                if (string.IsNullOrEmpty(id))
                    calcId = 0;
                else
                    calcId = long.Parse(id);
                operations = await auDbInterface.GetCalculationOperations(calcId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(operations);//, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetCalculationOperations ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        
        // function to enable Delete Calculation button
        [HttpGet]
        [Route("calcCanBeDeleted/{id}")]
        [ResponseType(typeof(Models.AssemblyUnit))]
        public async Task<IHttpActionResult> GetCalculationCanBeDeleted(string id)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            bool returnValue = false;
            string json = "{}";
            long calcId;
            try
            {
                if (string.IsNullOrEmpty(id))
                    calcId = 0;
                else
                    calcId = long.Parse(id);
                returnValue = await auDbInterface.GetCalculationCanBeDeleted(calcId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(returnValue);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetCalculationOperations ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        // function to delete the calculation
        [HttpGet]
        [Route("calcDeleteCalc/{id}")]
        [ResponseType(typeof(Models.AssemblyUnit))]
        public async Task<IHttpActionResult> DeleteCalculation(string id)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            long calcId;
            string returnValue = "";
            try
            {
                if (string.IsNullOrEmpty(id))
                    calcId = 0;
                else
                    calcId = long.Parse(id);
                await auDbInterface.DeleteCalculation(calcId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(returnValue);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetCalculationOperations ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }

        //function to populate Calculation dropdown list
        [HttpGet]
        [Route("CalcList")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> CollectCalcList()
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await auDbInterface.GetCalcList();
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetCalcList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to populate UOM dropdown list
        [HttpGet]
        [Route("UOMList")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> CollectUOMList()
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await auDbInterface.GetUomList();
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetUomList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to Show assembly units Search results with input values
        [HttpGet]
        [Route("SearchAU")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> CollectAssemblyUnitsSrch()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Dictionary<string, Models.Attribute>> AUlist = new List<Dictionary<string, Models.Attribute>>();
            string json = "{}";
            string auNm = "", retInd = "";
            long id = 0, calcID = 0;
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "id")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            id = 0;
                        else
                            id = long.Parse(que.Value);
                    }
                    else if (que.Key == "auNm")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            auNm = "%";
                        else
                            auNm = que.Value;
                    }
                    else if (que.Key == "calcID")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            calcID = 0;
                        else
                            calcID = long.Parse(que.Value);
                    }
                    else if (que.Key == "retInd")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            retInd = "%";
                        else
                            retInd = que.Value;
                    }
                }
                try
                {
                    AUlist = await auDbInterface.GetAssemblyUnits(id, auNm, calcID, retInd);
                }
                catch (Exception ex)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(AUlist, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from auDbInterface.GetAssemblyUnits ({0}, {1})", ex.Message, ex.StackTrace);
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
        //function to populate existing operator list
        [HttpGet]
        [Route("operators")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetOperatorList()
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await auDbInterface.GetOperatorList();
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetOperatorList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to populate existing operator term list
        [HttpGet]
        [Route("terms")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetOperatorTermList()
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await auDbInterface.GetOperatorTermList();
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetOperatorTermList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
        //function to save new labor id title and desc
        [HttpGet]
        [Route("SaveNewLaborId")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> SaveNewLaborId()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            long lbrId=0;
            string title = "", desc = "", status = "";
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "title")
                    {
                        title = que.Value;
                    }
                    else if (que.Key == "desc")
                    {
                        desc = que.Value;

                    }
                    else if (que.Key == "newLaborId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            lbrId = 0;
                        else
                            lbrId = long.Parse(que.Value);
                    }
                }
                try
                {
                    status = await auDbInterface.InsertlbrIdAttributes(lbrId,title, desc);

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
                    logger.Error(ex, "Unable to serialize response from auDbInterface.InsertlbrIdAttributes ({0}, {1})", ex.Message, ex.StackTrace);
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
        //function to Edit labor id title and desc
        [HttpGet]
        [Route("EditLaborId")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> EditLaborId()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            string title = "", desc = "", status = "";
            long lbrid = 0;
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "title")
                    {
                        title = que.Value;
                    }
                    else if (que.Key == "desc")
                    {
                        desc = que.Value;
                    }
                    else if (que.Key == "lbrid")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            lbrid = 0;
                        else
                            lbrid = long.Parse(que.Value);
                    }
                }
                try
                {
                    status = await auDbInterface.UpdatelbrIdAttributes(lbrid, title, desc);

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
                    logger.Error(ex, "Unable to serialize response from auDbInterface.UpdatelbrIdAttributes ({0}, {1})", ex.Message, ex.StackTrace);
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
        //function to Add/Update labor id Mtrl classification
        [HttpPost]
        [Route("SaveLbrIdMtrlclsfn/{lbrid}")]
        public async Task<IHttpActionResult> SaveLbrIdMtrlclsfn(string lbrid)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "";
            string status = "";
            JArray mtrlClsfnLst = null;
            long id;
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
                mtrlClsfnLst = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);
                if (string.IsNullOrEmpty(lbrid))
                    id = 0;
                else
                    id = long.Parse(lbrid);
                status = await auDbInterface.SaveLbrclassification(id, mtrlClsfnLst);
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.SaveLbrclassification ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
        //function to Update Existing labor id Assembly Units association
        [HttpPost]
        [Route("UpdateLbrIdAU/{lbrid}")]
        public async Task<IHttpActionResult> UpdateLbrIdAU(string lbrid)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            string status = "";
            JArray LbrIdexAULstNew = null;
            JArray LbrIdexAULstExisting = null;
            JObject item = null;
            long lLbrid;
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
                LbrIdexAULstNew = (Newtonsoft.Json.Linq.JArray)item["saveJSONNew"];
                LbrIdexAULstExisting = (Newtonsoft.Json.Linq.JArray)item["saveJSONExisting"];
                if (string.IsNullOrEmpty(lbrid))
                    lLbrid = 0;
                else
                    lLbrid = long.Parse(lbrid);
                status = await auDbInterface.UpdateLbrIdAU(lLbrid, LbrIdexAULstExisting, LbrIdexAULstNew);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            try
            {
                //  json = JsonConvert.SerializeObject(status, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.UpdateLbrIdexistingAU ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
        //function to Add/Update labor id Assembly Units
        [HttpPost]
        [Route("SaveLbrIdAuAltU/{lbrid}")]
        public async Task<IHttpActionResult> SaveLbrIdAuAltU(string lbrid)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            string status = "";
            JArray AuAltUnitLst = null;
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
                AuAltUnitLst = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.SaveLbrIdAuAltUnit ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
        //function to Show assembly units Search results in popup window
        [HttpGet]
        [Route("SearchAUforLbrId/{lbrid}")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> CollectAssemblyUnitsforLaborId(string lbrid)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Dictionary<string, Models.Attribute>> AUforLbrIdLst = new List<Dictionary<string, Models.Attribute>>();
            string json = "{}";
            long lLbrid;
            try
            {
                if (string.IsNullOrEmpty(lbrid))
                    lLbrid = 0;
                else
                    lLbrid = long.Parse(lbrid);
                AUforLbrIdLst = await auDbInterface.GetAssemblyUnitsforLbrIdPU(lLbrid);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(AUforLbrIdLst, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetAssemblyUnitsforLbrIdPU ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
        //function to Show assembly units Fireworks Search results in Fireworks popup
        [HttpGet]
        [Route("SearchFireworks")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> CollectFireworksNmSrch()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Dictionary<string, Models.Attribute>> FWlist = new List<Dictionary<string, Models.Attribute>>();
            string json = "{}";
            string name = "", notRetired = "", section = "";
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "name")
                    {
                        name = que.Value;
                    }
                    else if (que.Key == "notRetired")
                    {
                        notRetired = que.Value;
                    }
                    else if (que.Key == "section")
                    {
                        section = que.Value;
                    }

                }
                try
                {
                    FWlist = await auDbInterface.GetFireworks(name, notRetired, section);
                }
                catch (Exception ex)
                {
                    return InternalServerError();
                }

                try
                {
                    json = JsonConvert.SerializeObject(FWlist, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from auDbInterface.GetFireworks ({0}, {1})", ex.Message, ex.StackTrace);
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

        //function to save selected name from fireworks popup.
        [HttpGet]
        [Route("SaveFireworks")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> SaveFireworksAu()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            string name = "", AuSK = "", cuid = "", status = "";
            long calcId = 0, uomId = 0;
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "name")
                    {
                        name = que.Value;
                    }
                    else if (que.Key == "calcId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            calcId = 0;
                        else
                            calcId = long.Parse(que.Value);
                    }
                    else if (que.Key == "uomId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            uomId = 0;
                        else
                            uomId = long.Parse(que.Value);
                    }
                    else if (que.Key == "AuSK")
                    {
                        AuSK = que.Value;
                    }
                    else if (que.Key == "cuid")
                    {
                        cuid = que.Value;
                    }
                }
                try
                {
                    status = await auDbInterface.InsertFireworksAu(name, calcId, uomId, AuSK, cuid);

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
                    logger.Error(ex, "Unable to serialize response from auDbInterface.InsertFireworksAu ({0}, {1})", ex.Message, ex.StackTrace);
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

        //function to add new Operations in Calc popup
        [HttpGet]
        [Route("SaveAddOperations")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> SaveAddOperations()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "{}";
            string opNm = "", status = "";
            long auOpOprId = 0, dataTermId = 0, constantNo = 0, varTermId = 0;
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "opNm")
                    {
                        opNm = que.Value;
                    }
                    else if (que.Key == "auOpOprId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            auOpOprId = 0;
                        else
                            auOpOprId = long.Parse(que.Value);
                    }
                    else if (que.Key == "dataTermId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            dataTermId = 0;
                        else
                            dataTermId = long.Parse(que.Value);
                    }
                    else if (que.Key == "constantNo")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            constantNo = 0;
                        else
                            constantNo = long.Parse(que.Value);
                    }
                    else if (que.Key == "varTermId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            varTermId = 0;
                        else
                            varTermId = long.Parse(que.Value);                       
                    }
                }
                try
                {
                    status = await auDbInterface.InsertOperations(opNm, auOpOprId, dataTermId, constantNo, varTermId);
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
                    logger.Error(ex, "Unable to serialize response from auDbInterface.InsertFireworksAu ({0}, {1})", ex.Message, ex.StackTrace);
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
        //function to Add/Update labor id Mtrl classification
        [HttpPost]
        [Route("SaveOprforCalculation/{calcNm}")]
        public async Task<IHttpActionResult> SaveOprCalculation(string calcNm)
        {
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            string json = "";
            string status = "";
            JArray oprCalcLst = null;
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
                oprCalcLst = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);
                status = await auDbInterface.SaveOprforCalculation(calcNm,oprCalcLst);
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
                logger.Error(ex, "Unable to serialize response from auDbInterface.SaveOprforCalculation ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
        [HttpGet]
        [Route("filterLaborIds")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> filterLaborIds()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            AssemblyUnitDbInterface auDbInterface = new AssemblyUnitDbInterface();
            List<Dictionary<string, Models.Attribute>> AUlist = new List<Dictionary<string, Models.Attribute>>();
            string json = "{}";
            string pMtrlCatId = "", pFeatTypId = "", pCablTypId="";
            long id = 0, calcID = 0,auNm=0;
            List<Option> options = null;
            if (query != null && query.Count() > 0)
            {
                foreach (KeyValuePair<string, string> que in query)
                {
                    if (que.Key == "pMtrlCatId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            id = 0;
                        else
                            id = long.Parse(que.Value);
                    }
                    else if (que.Key == "pFeatTypId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            auNm = 0;
                        else
                            auNm = long.Parse(que.Value);
                    }
                    else if (que.Key == "pCablTypId")
                    {
                        if (string.IsNullOrEmpty(que.Value))
                            calcID = 0;
                        else
                            calcID = long.Parse(que.Value);
                    }                   
                }
                try
                {
                    options = await auDbInterface.filterLaborIds(id, auNm, calcID);
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
            try
            {
                json = JsonConvert.SerializeObject(options, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from auDbInterface.GetlbrIdsList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }
            return Ok(json);
        }
    }
}
