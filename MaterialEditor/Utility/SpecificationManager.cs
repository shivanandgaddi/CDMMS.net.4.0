using System;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class SpecificationManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SpecificationManager()
        {
        }

        public async Task<ISpecification> GetSpecification(long specificationId, SpecificationType.Type specType)
        {
            ISpecification specification = null;

            await Task.Run(() =>
            {
                if (specificationId > 0)
                    specification = SpecificationFactory.GetSpecificationInstance(specificationId, specType);
                else
                    specification = SpecificationFactory.GetSpecificationClass(specType);
            });

            return specification;
        }

        public async Task<long[]> PersistSpecification(JObject updatedSpecification, long specificationId)
        {
            long[] idArray = new long[4];
            string specificationType = (string)updatedSpecification.SelectToken("Typ.value");
            bool notifyNDSOfSpecnUpdate = true;
            bool notifyNDSOfMtlUpdate = false;
            bool isNewSpecification = false;
            bool sendToNDS = true;
            ISpecification currentSpecification = null;
            SpecificationType.Type specificationTypeEnum = SpecificationType.Type.NOT_SET;
            MaterialDbInterface dbInterface = null;

            bool.TryParse(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_UI, "specSendToNDS"), out sendToNDS);            

            if (Enum.TryParse<SpecificationType.Type>(specificationType, out specificationTypeEnum))
            {
                if (specificationId > 0)
                {
                    currentSpecification = await GetSpecification(specificationId, specificationTypeEnum);

                    currentSpecification.PersistUpdates(updatedSpecification, ref notifyNDSOfSpecnUpdate, ref notifyNDSOfMtlUpdate);

                    if (currentSpecification.NDSSpecificationId == 0)
                        isNewSpecification = true;
                }
                else
                {
                    currentSpecification = SpecificationFactory.GetSpecificationClass(specificationTypeEnum);

                    specificationId = currentSpecification.PersistObject(updatedSpecification, ref notifyNDSOfSpecnUpdate, ref notifyNDSOfMtlUpdate);

                    isNewSpecification = true;
                }

                notifyNDSOfSpecnUpdate = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);

                idArray[0] = specificationId;

                if (notifyNDSOfMtlUpdate || notifyNDSOfSpecnUpdate)
                {
                    dbInterface = new MaterialDbInterface();

                    if (sendToNDS && notifyNDSOfSpecnUpdate)
                        idArray[1] = dbInterface.InsertWorkToDo(specificationId, "CATALOG_SPEC", isNewSpecification ? "INSERT" : "UPDATE");
                    else
                        idArray[1] = 0;

                    if (currentSpecification.AssociatedMaterialId > 0)
                        idArray[2] = dbInterface.InsertWorkToDo(currentSpecification.AssociatedMaterialId, "CATALOG_UI", null);
                    else
                        idArray[2] = 0;

                    idArray[3] = currentSpecification.AssociatedMaterialId;
                }
                else
                {
                    idArray[1] = 0;
                    idArray[2] = 0;
                    idArray[3] = 0;
                }
            }
            else
                throw new Exception("Unable to determine specification type.");

            return idArray;
        }

        public async Task<string> PersistDimension(JObject dimension, string tableName)
        {
            string status = "Success";
            BaySpecificationDbInterface dbInterface = null;
            BayExtenderSpecificationDbInterface dbInterfacebayextn = null;
            decimal dimensionValue = 0;
            int uomId = 0;

            try
            {
                dimensionValue = dimension.SelectToken("dimension").Value<decimal>();
                uomId = dimension.SelectToken("uom").Value<int>();

                switch (tableName)
                {
                    case "bay_itnl_dpth":
                        dbInterface = new BaySpecificationDbInterface();                        

                        dbInterface.InsertBayInternalDepth(dimensionValue, uomId);

                        break;
                    case "bay_itnl_wdth":
                        dbInterface = new BaySpecificationDbInterface();

                        dbInterface.InsertBayInternalWidth(dimensionValue, uomId);

                        break;

                    case "bay_extndr_intl_wdth":
                        dbInterfacebayextn = new BayExtenderSpecificationDbInterface();

                        dbInterfacebayextn.InsertBayExtenderWidth(dimensionValue, uomId);

                        break;
                    case "bay_extndr_intl_dpth":
                        dbInterfacebayextn = new BayExtenderSpecificationDbInterface();

                        dbInterfacebayextn.InsertBayExtenderDepth(dimensionValue, uomId);

                        break;
                    case "bay_extndr_intl_hgt":
                        dbInterfacebayextn = new BayExtenderSpecificationDbInterface();

                        dbInterfacebayextn.InsertBayExtenderHeight(dimensionValue, uomId);

                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                status = "Error";
            }

            return status;
        }

        public async Task<string> PersistNDSSpecificationId(ISpecification specification, long workToDoId)
        {
            MaterialDbInterface materialDbInterface = new MaterialDbInterface();
            long ndsSpecId = 0;
            string status = "SUCCESS";

            await Task.Run(() =>
            {
                try
                {
                    ndsSpecId = materialDbInterface.GetWorkToDoPniId(workToDoId);

                    if (ndsSpecId > 0)
                        specification.PersistNDSSpecificationId(ndsSpecId);
                    else
                    {
                        logger.Error("No NDS specification id found in WORK_TO_DO table: {0}, {1}", specification.SpecificationId, workToDoId);

                        status = "ERROR";
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    status = "ERROR";
                }
            });

            return status;
        }

        public async Task<string> PersistNDSSpecificationId(long ndsSpecId, ISpecification specification)
        {
            MaterialDbInterface materialDbInterface = new MaterialDbInterface();
            string status = "{\"wtd_id\": ";
            long workToDoId = 0;

            await Task.Run(() =>
            {
                try
                {
                    if (ndsSpecId > 0)
                    {
                        specification.PersistNDSSpecificationId(ndsSpecId);

                        workToDoId = materialDbInterface.InsertWorkToDo(specification.SpecificationId, "CATALOG_SPEC", "UPDATE");

                        status += workToDoId + "}";
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    status = "ERROR";
                }
            });

            return status;
        }
    }
}