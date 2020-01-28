using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class SlotSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, CardSpecification.SlotConsumption> slotConsumptionList = null;
        private Dictionary<long, SpecificationRole> roleList = null;
        private ISpecificationDbInterface dbInterface = null;
        private int slotConsumptionId = 0;

        public SlotSpecification() : base()
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public SlotSpecification(long specificationId) : base(specificationId)
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public SlotSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public void PopulateSlotConsumptionList()
        {
            slotConsumptionList = ((SlotSpecificationDbInterface)DbInterface).GetSlotConsumptionList();
        }

        public void PopulateRoleList()
        {
            roleList = ((SlotSpecificationDbInterface)DbInterface).GetRoleList(SpecificationId);
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.SltCnsmptnLst, "")]
        public Dictionary<long, CardSpecification.SlotConsumption> SlotConsumptionList
        {
            get
            {
                return slotConsumptionList;
            }
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotUseTypId)]
        public string SlotUseTypId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RvsnNm)]
        public string RevisionName
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.SpcnRlTypLst, "")]
        public Dictionary<long, SpecificationRole> RoleList
        {
            get
            {
                return roleList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SltCnsmptnId)]
        public int SlotConsumptionId
        {
            get
            {
                if (slotConsumptionId == 0 && slotConsumptionList != null)
                {
                    foreach (KeyValuePair<long, CardSpecification.SlotConsumption> keyValue in slotConsumptionList)
                    {
                        if ("Y".Equals(keyValue.Value.DefaultIndicator))
                        {
                            slotConsumptionId = (int)keyValue.Value.SpecificationId;

                            break;
                        }
                    }
                }

                return slotConsumptionId;
            }
            set
            {
                slotConsumptionId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SbSlt)]
        public string SubSlotInd
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Dpth)]
        public decimal Depth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Hght)]
        public decimal Height
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Wdth)]
        public decimal Width
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.DimUom)]
        public int DimensionsUnitOfMeasure
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StrghtThru)]
        public string StraightThruIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NdsMfr)]
        public override string NDSManufacturer
        {
            get;
            set;
        }

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new SlotSpecificationDbInterface();

                return dbInterface;
            }
        }

        public override Dictionary<string, SpecificationAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = GetAttributes(this, true);

                if (SpecificationId > 0)
                {
                    if (!IsCompleted)
                    {
                        attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = false;
                    }
                    else if (!IsPropagated)
                        attributes[SpecificationType.JSON.Prpgtd].IsEditable = true;

                    if (!IsDeleted)
                        attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }
                else
                {
                    attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                    attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }

                return attributes;
            }

            set
            {
                attributes = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.SLOT;
            }
        }

        [JsonIgnore]
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        [JsonIgnore]
        public override long AssociatedMaterialId
        {
            get
            {
                return 0;
            }
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedRevisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            int updatedslotConsmptnId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltCnsmptnId);
            string updatedsubslotInd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.SbSlt);
            decimal updateddepth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal updatedHeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal updatedWidth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int updatedUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            string updatedStraightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);

            if (Name != updatedName || RevisionName != updatedRevisionName || Description != updatedDescription || SlotConsumptionId != updatedslotConsmptnId || SubSlotInd != updatedsubslotInd ||
                Depth != updateddepth || Height != updatedHeight || Width != updatedWidth || DimensionsUnitOfMeasure != updatedUom || IsCompleted != updatedIsCompleted
                || IsPropagated != updatedIsPropagated || IsDeleted != updatedIsDeleted || StraightThruIndicator != updatedStraightThruIndicator)
            {
                ((SlotSpecificationDbInterface)DbInterface).UpdateSlotSpecification(SpecificationId, updatedName, updatedDescription, updatedslotConsmptnId, updatedsubslotInd,
                    updateddepth, updatedHeight, updatedWidth, updatedUom, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, updatedStraightThruIndicator, updatedRevisionName);

                notifyNDS = true;
            }

            PersistSpecificationRole(updatedSpecification, ref notifyNDS);

            if (!updatedIsPropagated)
                notifyNDS = false;
        }

        private void PersistSpecificationRole(JObject updatedSpecification, ref bool notifyNDS)
        {
            List<Dictionary<string, SpecificationAttribute>> updatedRoleList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst).ObjectList;

            foreach (KeyValuePair<long, SpecificationRole> role in RoleList)
            {
                //shelfRole.IsSelected = DataReaderHelper.GetNonNullValue(reader, "is_selected") == "Y" ? true : false;
                //shelfRole.PriorityNumber = int.Parse(DataReaderHelper.GetNonNullValue(reader, "shelf_role_typ_prty_no", true));
                //shelfRole.RoleType = DataReaderHelper.GetNonNullValue(reader, "shelf_role_typ");
                //if(updatedRole["Slctd"].BoolValue==RoleList[SpecificationType.JSON.SpcnRlTypLst]
                //    int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value)
            }
            
            ((SlotSpecificationDbInterface)DbInterface).DeleteSlotSpecificationRole(SpecificationId);

            foreach (Dictionary<string, SpecificationAttribute> updatedRole in updatedRoleList)
            {
                if (updatedRole["Slctd"].BoolValue)
                {
                    ((SlotSpecificationDbInterface)DbInterface).InsertSlotSpecificationRole(SpecificationId,
                        int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value));
                }
            }

            notifyNDS = true;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string revisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            int slotConsmptnId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltCnsmptnId);
            string subslotInd = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.SbSlt);
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);

            try
            {
                specificationId = ((SlotSpecificationDbInterface)DbInterface).CreateSlotSpecification(name, description, slotConsmptnId, subslotInd, depth, height, width,
                    uom, isCompleted, isPropagated, isDeleted, straightThruIndicator, revisionName);

                CreateSlotSpecificationRole(updatedSpecification, specificationId);

                if (isPropagated)
                    notifyNDS = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new Slot specification");

                throw ex;
            }

            return specificationId;
        }
           
        private void CreateSlotSpecificationRole(JObject updatedSpecification, long specificationId)
        {
            List<Dictionary<string, SpecificationAttribute>> updatedRoleList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst).ObjectList;
            foreach (Dictionary<string, SpecificationAttribute> updatedRole in updatedRoleList)
            {
                if (updatedRole["Slctd"].BoolValue)
                {
                    ((SlotSpecificationDbInterface)DbInterface).InsertSlotSpecificationRole(specificationId,
                        int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value));
                }
            }
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
        }
    }
}