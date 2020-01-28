using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/drop")]
    public class dropDownController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        // GET api/<controller>
        [HttpGet]
        [Route("materialtypecd/{approvedstatus?}" )]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> Searchformaterialtypecd(string approvedstatus="ALL")
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";
            string selecttable = "";
            if (query != null && query.Count() > 0)
            {
                selecttable = query.First().Value;
                try
                {
                    items = await dbInterface.GettypeCD(selecttable,approvedstatus);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from dropDownDbInterface.GettypeCD()");

                    return InternalServerError();
                }
            }
            else
            {
                //TODO
            }
            return Ok(json);
        }
        [HttpGet]
        [Route("materialflowthru/{flowthruind?}")]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> Searchformaterialflowthru(string flowthruind="ALL")
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";
            string selecttable = "";
            if (query != null && query.Count() > 0)
            {
                selecttable = query.First().Value;
                try
                {
                    items = await dbInterface.Getflowthru(selecttable,flowthruind);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from dropDownDbInterface.Getflowthru(string originatingfieldname)");

                    return InternalServerError();
                }
            }
            return Ok(json);
        }
        [HttpGet]
        [Route("updation")]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> UpdateTypeCD()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";
            string id = "";
            string name = "";
            string effestartdate = "";
            string effenddate = "";
            string username = "";
            string lastupdated = "";
            string user = "";

            try
            {
                if (query != null && query.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in query)
                    {
                        if (kvp.Key == "materialid")
                        {
                            id = kvp.Value;
                        }
                        else if (kvp.Key == "appname")
                        {
                            name = kvp.Value;
                        }
                        else if (kvp.Key == "startdate")
                        {
                            effestartdate = kvp.Value;
                        }
                        else if (kvp.Key == "enddate")
                        {
                            effenddate = kvp.Value;
                        }
                        
                        else if (kvp.Key == "lastupdated")
                        {
                            lastupdated = kvp.Value;
                        }
                        else if (kvp.Key == "user")
                        {
                            user = kvp.Value;
                        }


                    }

                }
                items = await dbInterface.UpdatematerialtypeCD(id, name, effestartdate, effenddate, lastupdated,user);

                json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from dropDownDbInterface. Updatematerialflowthru(string flowthruid, string originatingfieldname, string fielddescription, string flowthruind, string originatingsystem)");

                return InternalServerError();
            }

            return Ok(json);
        }
        [HttpGet]
        [Route("updationflowthru")]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> Updateflowthru()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";

            string id = "";
            string name = "";
            string description = "";
            string flowthru = "";
            string orgsystem = "";
            string user = "";



            try
            {
                if (query != null && query.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in query)
                    {
                        if (kvp.Key == "flowthruid")
                        {
                            id = kvp.Value;
                        }
                        else if (kvp.Key == "originatingfieldname")
                        {
                            name = kvp.Value;
                        }
                        else if (kvp.Key == "fielddescription")
                        {
                            description = kvp.Value;
                        }
                        else if (kvp.Key == "flowthruind")
                        {
                            flowthru = kvp.Value;
                        }
                        else if (kvp.Key == "originatingsystem")
                        {
                            orgsystem = kvp.Value;
                        }
                        else if (kvp.Key == "user")
                        {
                            user = kvp.Value;
                        }
                    }
                }
                items = await dbInterface.Updatematerialflowthru(id, name, description, flowthru, orgsystem,user);
                json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from dropDownDbInterface. UpdatematerialtypeCD()");

                return InternalServerError();

            }

            return Ok(json);
        }

        [HttpGet]
        [Route("{appname}")]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> DeleteTypeCD(string appname)
        {
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";

            try
            {

                items = await dbInterface.DeleteMaterialtypecd(appname);

                json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from dropDownDbInterface.DeleteMaterialtypecd({0} )", appname);
                return InternalServerError();
            }
            return Ok(json);
        }

        [HttpGet]
        [Route("insert")]
        [ResponseType(typeof(Drop))]
        public async Task<IHttpActionResult> InserttypeCD()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            dropDownsDbInterface dbInterface = new dropDownsDbInterface();
            List<Drop> items = null;
            string json = "{}";
            string id = "";
            string name = "";
            string effestartdate = "";
            string effenddate = "";
            string user = "";

            try
            {
                if (query != null && query.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in query)
                    {

                        if (kvp.Key == "appname")
                        {
                            name = kvp.Value;
                        }
                        else if (kvp.Key == "effectivedate")
                        {
                            effestartdate = kvp.Value;
                        }
                        else if (kvp.Key == "enddate")
                        {
                            effenddate = kvp.Value;
                        }
                        else if (kvp.Key == "user")
                        {
                            user = kvp.Value;
                        }
                    }
                    items = await dbInterface.InsertmaterialtypeCD(name, effestartdate, effenddate,user);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from dropDownDbInterface.InsertmaterialtypeCD()");

                return InternalServerError();

            }
            return Ok(json);
        }
    }
}

