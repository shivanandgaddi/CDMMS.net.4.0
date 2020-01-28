using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Controllers
{
    [RoutePrefix("api/replacement")]
    public class ReplacementPartController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("{id}/{typ}")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> GetReplacementMaterial(int id, string typ)
        {
            MaterialDbInterface dbInterface = new MaterialDbInterface();
            List<MaterialItem> material = null;
            string json = "{}";

            try
            {
                //typ can have two valid values
                //1. S - For superceded/replaced material
                //2. C - For chaining material
                if ("S".Equals(typ))
                    material = await dbInterface.GetReplacementMaterialInfoAsync(id);
                else if ("C".Equals(typ))
                    material = await dbInterface.GetChainingMaterialInfoAsync(id);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (material != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(material, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from MaterialDbInterface.GetReplacementMaterialInfoAsync({0})", id);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }

        [HttpPost]
        [Route("update/{parentOrChildrenOf}/{id}/{typ}")]
        public async Task<IHttpActionResult> UpdateRelationship(string parentOrChildrenOf, long id, string typ)
        {
            string json = "";
            string status = "";
            long otherId = -1;
            JArray itemsToAssociate = null;
            MaterialDbInterface dbInterface = null;
            long childId = -1;
            long parentId = -1;

            //typ can have two valid values
            //1. S - For superceded/replaced material
            //2. C - For chaining material

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
                itemsToAssociate = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json);

                if (itemsToAssociate != null)
                {
                    dbInterface = new MaterialDbInterface();

                    dbInterface.StartTransaction();

                    if ("children".Equals(parentOrChildrenOf)) //Clear all associated children. 
                    {
                        parentId = id;

                        if ("S".Equals(typ))
                            status = await dbInterface.UpdateReplacementAssociation(parentId, 0, "");
                        else if ("C".Equals(typ))
                            status = await dbInterface.UpdateChainingAssociation(parentId, 0, "");
                    }
                    else
                        childId = id;

                    for (int i = 0; i < itemsToAssociate.Count; i++)
                    {
                        string comment = itemsToAssociate[i].Value<string>("cmnt");

                        if (long.TryParse(itemsToAssociate[i].Value<string>("id"), out otherId))
                        {
                            if ("children".Equals(parentOrChildrenOf))
                                childId = otherId;
                            else
                                parentId = otherId;

                            if ("S".Equals(typ))
                                status = await dbInterface.UpdateReplacementAssociation(parentId, childId, comment);
                            else if ("C".Equals(typ))
                                status = await dbInterface.UpdateChainingAssociation(parentId, childId, comment);
                        }
                    }

                    dbInterface.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                dbInterface.RollbackTransaction();

                //logger.Error(ex, "Unable to persist material item id {0} from JSON: {1}", materialItemId, json);

                return InternalServerError();
            }

            return Ok("success");
        }

        [Route("chained/{id}")]
        [ResponseType(typeof(MaterialItem))]
        public async Task<IHttpActionResult> GetChainedInMaterial(int id)
        {
            LOSDBMaterialManager manager = new LOSDBMaterialManager();
            List<MaterialItem> material = null;
            string json = "{}";

            try
            {
                material = await manager.GetChainedInMaterialItemsAsync(id);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            if (material != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(material, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to serialize response from LOSDBMaterialManager.GetChainedInMaterialItemsAsync({0})", id);

                    return InternalServerError();
                }
            }

            return Ok(json);
        }
    }
}
