using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
//using CenturyLink.Network.Engineering.Material.Editor.Models;
//using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
//using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    public class RequestReponse
    {
        public RequestReponse()
        {
            EC = 0;
            MSG = "SUCCESS";
            OV = "";
        }

        public int EC { set; get; }
        public string MSG { get; set; }
        public string OV { get; set;  }
    }
    [RoutePrefix("api/cad")]
    public class CadAttrController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //function to populate Reports dropdown list
        public CadAttrController()
        {

        }

        [HttpPost]
        [Route("InsertCadAttributes")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> InsertCadAttributes()
        {
            CadAttributesDbInterface cadDbInterface = new CadAttributesDbInterface();
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            var args = new Hashtable();
            string referrer = "NULL";
            string json = "";
            List<CadAttributes> myDeserializedObjList = null;
            string resp = string.Empty;

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

            if (json == "[]")
            {
                try
                {
                    myDeserializedObjList = (List<CadAttributes>)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(List<CadAttributes>));
                 
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to deserialize request.");

                    return InternalServerError(ex);
                }

                try
                {
                    resp = await cadDbInterface.Insert_cad_attributes(myDeserializedObjList);
                    return Ok(resp);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Controller.InsertsReportRequest() failed for Report");
                    return InternalServerError();
                }
            }
            return Ok("Requested save parameter values are empty.");
        }

        [HttpPost]
        [Route("attrs/update")]
        [ResponseType(typeof(List<RequestReponse>))]
        public async Task<IHttpActionResult> UpdateAttrs()
        {
            var rv = new RequestReponse();
            
            string json = "";
            
            try
            {
                // { cuid: "<cuid>", updates: [{},{}], deletes: [{},{}] }
                json = await this.Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to read request content");

                return InternalServerError();
            }

            if( json.Length == 0 || json == "[]")
            {
                rv.MSG = "requested save parameter values are empty.";
                return Ok(rv);
            }

            List<CadAttributes> updates = new List<CadAttributes>();
            List<CadAttributes> deletes = new List<CadAttributes>();

            var cuid = "";
            
            try
            {

                var data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
                cuid = data.Value<string>("cuid");

                var items = (Newtonsoft.Json.Linq.JArray)data["updates"];
                foreach( JObject o in items )
                {
                    updates.Add(o.ToObject<CadAttributes>());
                }

                items = (Newtonsoft.Json.Linq.JArray)data["deletes"];
                foreach (JObject o in items)
                {
                    deletes.Add(o.ToObject<CadAttributes>());
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to deserialize JSON request.");

                return InternalServerError(ex);
            }

            try
            {
                CadAttributesDbInterface dbi = new CadAttributesDbInterface();

                var dStatus = await dbi.UPDATE_ATTRS(cuid, "DELETE", deletes);
                var uStatus = await dbi.UPDATE_ATTRS(cuid, "UPDATE", updates);

                var ov = new Hashtable();
                ov["errors"] = new List<GenericResponse>();
                ov["updates"] = null;
                if (uStatus.Count > 0)
                {
                    var len = uStatus.Count;
                    for( var i = 0; i < len; i++ )
                    {
                        var item = uStatus[i];
                        updates[i].CAD_ATTR_ID = (item.EC == 0 ? item.OV : updates[i].CAD_ATTR_ID);
                        if( item.EC != 0 )
                        {
                            ((List<GenericResponse>)ov["errors"]).Add(item);
                        }
                    }
                }
                ov["updates"] = updates;
                rv.OV = Newtonsoft.Json.JsonConvert.SerializeObject(ov);
                return Ok(rv);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CadAttrController.UpdateAttrs() failed");

                //return InternalServerError(ex);
                rv.EC = -1;
                rv.MSG = ex.Message;
                rv.OV = ex.StackTrace;
            }

            return Ok(rv);
        }

        [HttpGet]
        [Route("attrs/list/{tmpltId}/{tmpltDefId}")]
        [ResponseType(typeof(List<Hashtable>))]
        public async Task<IHttpActionResult> ListAttrs(long tmpltId, long tmpltDefId = 0 )
        {
            var dbi = new CadAttributesDbInterface();
            var list = new List<Hashtable>();

            try
            {
                list = await dbi.LIST_ATTRS(tmpltId, tmpltDefId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var msg = String.Format("FAILURE: CadAttrController.ListAttrs({0},{1})", tmpltId, tmpltDefId);
                logger.Error(ex, msg);
                return InternalServerError();
            }
        }
    }
}
