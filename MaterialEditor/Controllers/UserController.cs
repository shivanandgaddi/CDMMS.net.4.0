using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Authenticate(string id)
        {
            string result = "Invalid";
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();

            result = await dbInterface.ValidateSessionId(id);

            return Ok(result);
        }
    }
}
