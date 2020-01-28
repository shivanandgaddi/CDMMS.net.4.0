using System;
using System.Collections.Generic;
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
    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {

        [HttpGet]
        [Route("search")]
        [ResponseType(typeof(CategoryItem))]
        public async Task<IHttpActionResult> Search()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            CategoryDbInterface dbInterface = new CategoryDbInterface();
            List<CategoryItem> items = null;
            string json = "{}";
            string searchValue = "";

            if (query != null && query.Count() == 1)
            {
                searchValue = query.First().Value;

                try
                {
                    items = await dbInterface.SearchCategoryItemAsync(searchValue);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
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
        [Route("{id}")]
        [ResponseType(typeof(CategoryItem))]
        public async Task<IHttpActionResult> DeleteCategory(string id)
        {
            CategoryDbInterface dbInterface = new CategoryDbInterface();
            List<CategoryItem> items = null;
            string json = "{}";

            try
            {

                items = await dbInterface.DeleteCategoryItemAsync(id);

                json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(json);
        }


        [HttpGet]
        [Route("updation")]
        [ResponseType(typeof(CategoryItem))]
        public async Task<IHttpActionResult> UpdateCategory()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            CategoryDbInterface dbInterface = new CategoryDbInterface();
            List<CategoryItem> items = null;
            string json = "{}";
            string id = "";
            string name = "";
            string nameold = "";
            string effstartdate = "";
            string effenddate = "";
            string user = "";

            try
            {
                if (query != null && query.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in query)
                    {
                        if (kvp.Key == "mtlgrp")
                        {
                            id = kvp.Value;
                        }
                        else if (kvp.Key == "appname")
                        {
                            name = kvp.Value;
                        }
                        else if (kvp.Key == "appnameold")
                        {
                            nameold = kvp.Value;
                        }
                        else if (kvp.Key == "startdate")
                        {
                            effstartdate = kvp.Value;
                        }
                        else if (kvp.Key == "enddate")
                        {
                            effenddate = kvp.Value;
                        }
                        else if(kvp.Key=="user")
                        {
                            user = kvp.Value;
                        }

                    }

                    items = await dbInterface.UpdateCategoryItemAsync(id, name, nameold,effstartdate, effenddate,user);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }


            return Ok(json);
        }

        [HttpGet]
        [Route("insert")]
        [ResponseType(typeof(CategoryItem))]
        public async Task<IHttpActionResult> InsertCategory()
        {
            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            CategoryDbInterface dbInterface = new CategoryDbInterface();
            List<CategoryItem> items = null;
            string json = "{}";
            string id = "";
            string name = "";
            string effstartdate = "";
            string effenddate = "";
            string user = "";

            try
            {
                if (query != null && query.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in query)
                    {
                        if (kvp.Key == "mtlgrp")
                        {
                            id = kvp.Value;
                        }
                        else if (kvp.Key == "appname")
                        {
                            name = kvp.Value;
                        }
                        else if (kvp.Key == "startdate")
                        {
                            effstartdate = kvp.Value;
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
                    items = await dbInterface.InsertCategoryItemAsync(id, name, effstartdate, effenddate,user);

                    json = JsonConvert.SerializeObject(items, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok(json);
        }



    }
}
