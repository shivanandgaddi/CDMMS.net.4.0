using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class OverAllShelfTemplate : OverAllTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Drawing drawing = null;
        private ITemplateDbInterface shelfTemplateDbInterface = null;

        public OverAllShelfTemplate() : base()
        {

        }

        public OverAllShelfTemplate(long templateId) : base(templateId)
        {

        }

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplateId)]
        public long BaseTemplateId
        {
            get;
            set;
        }

        public long BayExtndrSpecnRevsnAltId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationRevisionId)]
        public override long SpecificationRevisionId
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.SpecificationRevisionId;
                else
                    return 0;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.SpecificationRevisionId = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationId)]
        public override long SpecificationId
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.SpecificationId;
                else
                    return 0;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.SpecificationId = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Type)]
        public override string TemplateType
        {
            get
            {
                return Type.ToString();
            }
        }

        [JsonIgnore]
        public override TemplateType.Type Type
        {
            get
            {
                return Utility.TemplateType.Type.SHELF;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.AssociatedSpecification)]
        public override ISpecification AssociatedSpecification
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.AssociatedSpecification;
                else
                    return null;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.AssociatedSpecification = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Drawing)]
        public override Drawing TemplateDrawing
        {
            get
            {
                return drawing;
            }
        }

        [JsonIgnore]
        public override ITemplateDbInterface DbInterface
        {
            get
            {
                if (shelfTemplateDbInterface == null)
                    shelfTemplateDbInterface = new ShelfTemplateDbInterface();

                return shelfTemplateDbInterface;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.HighLevelPartTemplate)]
        public override bool IsHighLevelPartTemplate
        {
            get
            {
                return false;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.CommonConfigTemplate)]
        public override bool IsCommonConfigTemplate
        {
            get
            {
                return false;
            }
        }

        //public override decimal ConvertPhysicalDimensionToPixels(decimal dimension)
        //{
        //    throw new NotImplementedException();
        //}

        //public override decimal ConvertPixelsToPhysicalDimension(decimal pixels)
        //{
        //    throw new NotImplementedException();
        //}

        public override void CreateDrawing(bool isForEdit)
        {
            throw new NotImplementedException();
        }

        public override void InitializeAttributes()
        {
            throw new NotImplementedException();
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long baseTmpltId = 0;

            shelfTemplateDbInterface = new ShelfTemplateDbInterface();

            if (long.TryParse((string)tmpltValues["pBaseTmpltId"], out baseTmpltId))
                tmpltId = ((ShelfTemplateDbInterface)shelfTemplateDbInterface).CreateOverAllTemplate(baseTmpltId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"], (string)tmpltValues["cuid"]);
            else
            {
                logger.Error("Unable to persist overall template, cannot parse base template id from parameters.");

                tmpltId = -1;
            }

            return tmpltId;
        }

        public override void PersistUpdates(JObject updatedTmplt)
        {
            string updatedName = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Name, updatedTmplt);
            string updatedDescription = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Description, updatedTmplt).ToUpper();
            //int updatedRotationAngleId = 0;
            bool updatedCompleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Completed, updatedTmplt);
            bool updatedPropagated = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Propagated, updatedTmplt);
            bool updatedDeleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Deleted, updatedTmplt);
            bool updatedRetired = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Retired, updatedTmplt);
            bool updatedInProgress = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.UpdateInProgress, updatedTmplt);
            string TemplateSpecId = JsonHelper.GetStringValue(Utility.TemplateType.JSON.SpecificationId, updatedTmplt);

            long BaseTemplateId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.BaseTemplateId, updatedTmplt);
            long MaterialCatId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.MtrlCatId, updatedTmplt);
            long FeatTypeId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.FeatTypId, updatedTmplt);
            string UserCuid = JsonHelper.GetStringValue(Utility.TemplateType.JSON.UserCuid, updatedTmplt);

            try
            {
                if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
                    !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
                    !UpdateInProgress.Equals(updatedInProgress))
                {
                    shelfTemplateDbInterface = new ShelfTemplateDbInterface();

                    shelfTemplateDbInterface.StartTransaction();

                    ((ShelfTemplateDbInterface)shelfTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, false, IsHighLevelPartTemplate,
                        IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);

                    long resval = ((ShelfTemplateDbInterface)shelfTemplateDbInterface).UpdateOverAllTemplate(TemplateId, BaseTemplateId, MaterialCatId, FeatTypeId, UserCuid);
                    if (resval > 0)
                    {
                        return;
                    }

                }

                if (shelfTemplateDbInterface != null)
                    shelfTemplateDbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (shelfTemplateDbInterface != null)
                    shelfTemplateDbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (shelfTemplateDbInterface != null)
                    shelfTemplateDbInterface.Dispose();
            }
        }
    }
}