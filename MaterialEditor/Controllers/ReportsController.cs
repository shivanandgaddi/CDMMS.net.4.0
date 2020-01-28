using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/Reports")]
    public class ReportsController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //function to populate Reports dropdown list
        [HttpGet]
        [Route("ReportsList")]
        [ResponseType(typeof(List<Dictionary<string, Models.Attribute>>))]
        public async Task<IHttpActionResult> LaborIDsList()
        {
             ReportsDbinterface rprtDbInterface = new ReportsDbinterface();
            List<Option> options = null;
            string json = "{}";

            try
            {
                options = await rprtDbInterface.GetAvailableReportsList();
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
                logger.Error(ex, "Unable to serialize response from rprtDbInterface.GetAvailableReportsList ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }

        //function to populate selected Labor Id attributes
        [HttpGet]
        [Route("LbrIdReport/{id}")]
        [ResponseType(typeof(Models.AssemblyUnit))]
        public async Task<IHttpActionResult> CollectlbrIdAttribute(string id)
        {
            ReportsDbinterface rprtDbInterface = new ReportsDbinterface();            
            Reports LbrIdReportscollection = new Reports();
            string json = "{}";
            long laborId;

            try
            {
                laborId = long.Parse(id);
                LbrIdReportscollection = await rprtDbInterface.GetlbrIdreports(laborId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            try
            {
                json = JsonConvert.SerializeObject(LbrIdReportscollection, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to serialize response from rprtDbInterface.GetlbrIdreports ({0}, {1})", ex.Message, ex.StackTrace);
                return InternalServerError();
            }

            return Ok(json);
        }
    }
}
