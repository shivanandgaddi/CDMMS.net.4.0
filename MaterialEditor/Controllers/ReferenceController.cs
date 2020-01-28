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
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/reference")]
    public class ReferenceController : ApiController
    {
        [Route("{shortName}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetReference(string shortName)
        {
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();
            List<Option> data = null;
            string json = "{}";
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NameValueCollection parameters = null;

            try
            {
                if (query != null && query.Count() > 0)
                {
                    IEnumerator<KeyValuePair<string, string>> enumerator = query.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, string> current = enumerator.Current;

                        if (parameters == null)
                            parameters = new NameValueCollection();

                        parameters.Add(current.Key, current.Value);
                    }
                }

                data = await dbInterface.GetListOptionsForAttribute(shortName, parameters);

                json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("autocomplete/{fieldName}/{searchValue}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> GetReferenceAutoComplete(string fieldName, string searchValue)
        {
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();
            List<AutocompleteOption> data = null;
            string json = "{}";
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            NameValueCollection parameters = null;

            try
            {
                if (query != null && query.Count() > 0)
                {
                    IEnumerator<KeyValuePair<string, string>> enumerator = query.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, string> current = enumerator.Current;

                        if (parameters == null)
                            parameters = new NameValueCollection();

                        parameters.Add(current.Key, current.Value);
                    }
                }

                data = await dbInterface.GetListOptionsForAutoComplete(fieldName, searchValue, parameters);

                json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
            }

            return Ok(json);
        }

        [HttpGet]
        [Route("checkFtrTypIsLocatable/{id}")]
        [ResponseType(typeof(List<Option>))]
        public async Task<IHttpActionResult> CheckLocatable(int id)
        {
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();
            string locType = "Non Locatable";

            if (id > 0)
            {
                try
                {
                    locType = await dbInterface.CheckLocatableFeatureType(id);
                }
                catch (Exception ex)
                {
                    return InternalServerError();
                }
            }

            return Ok(locType);
        }
    }
}
