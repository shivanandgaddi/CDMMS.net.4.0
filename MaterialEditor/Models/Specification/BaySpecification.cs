using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class BaySpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, BayInternalSpecification> bayInternalList = null;
        private long bayInternalId = 0;
        private long materialItemId = 0;
        private ISpecificationDbInterface dbInterface = null;
        private IMaterial bay = null;

        public BaySpecification() : base()
        {
            PopulateBayInternalList();
        }

        public BaySpecification(long specificationId) : base(specificationId)
        {
            PopulateBayInternalList();
        }

        public BaySpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateBayInternalList();
        }

        public void PopulateBayInternalList()
        {
            bayInternalList = ((BaySpecificationDbInterface)DbInterface).GetBayInternals();
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlLst, "")]
        public Dictionary<long, BayInternalSpecification> BayInternalList
        {
            get
            {
                return bayInternalList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.DimUom)]
        public int MountingPositionOffsetUnitOfMeasureId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MntngPosOfst)]
        public decimal MountingPositionOffset
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StrtLbl)]
        public string LabelStartingPosition
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.WllMnt)]
        public string WallMountIndicator
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

        //MODEL_NO column was removed from the database. BAY_SPECN.BAY_SPECN_NM will now contain the "Model" for a bay
        //[JsonIgnore]
        //[MaterialJsonProperty(SpecificationType.JSON.MdlNo)]
        //public string ModelNumber
        //{
        //    get;
        //    set;
        //}

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MidPln)]
        public string MidPlaneIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Gnrc)]
        public bool IsGeneric
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.DualSd)]
        public string DualSDIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayRlTypId)]
        public int RoleTypeId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayUseTypId)]
        public string BayUseTypeId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlDpth)]
        public decimal ExternalDepth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlHgt)]
        public decimal ExternalHeight
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlWdth)]
        public decimal ExternalWidth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.XtnlDimUom)]
        public int ExternalDimensionsUnitOfMeasure
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
        [MaterialJsonProperty(SpecificationType.JSON.NdsMfr)]
        public override string NDSManufacturer
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RvsnId)]
        public long RevisionId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MxWght)]
        public decimal MaxWeightCapacity
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.MxWghtUom)]
        public int MaxWeightCapacityUnitOfMeasure
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RO)]
        public bool IsRecordOnly
        {
            get;
            set;
        }

        [JsonIgnore]
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return bay;
            }
            set
            {
                bay = value;
            }
        }

        [JsonIgnore]
        public override long AssociatedMaterialId
        {
            get
            {
                return materialItemId;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlId)]
        public long BayInternalId
        {
            get
            {
                return bayInternalId;
            }
            set
            {
                bayInternalId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.BAY;
            }
        }        

        private string FormSpecificationName(JObject updatedSpecification)
        {
            string name = "BA-";
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            string wallMountDefaultIndicator = ReferenceDbInterface.GetWallMountAllowedDefaultIndicator(wallMountIndicator, "bay_specn");
            string straightThruDefaultIndicator = ReferenceDbInterface.GetStraightThruDefaultIndicator(straightThruIndicator, "bay_specn");
            string midPlaneDefaultIndicator = ReferenceDbInterface.GetMidPlaneDefaultIndicator(midPlaneIndicator, "bay_specn");
            string wsm = "";
            string stringHeight = "";
            string stringWidth = "";
            string stringDepth = "";
            int mountingPositionOffsetUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            int mountingPositionQty = 0;
            int externalDimensionsUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.XtnlDimUom);
            int bayInternalId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.BayIntlId);
            int intWidth = 0;
            decimal bayInternalWidth = 0;
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlDpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlHgt);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlWdth);
            decimal mountingPositionOffsetValue = 0; //JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MntngPosOfst);
            bool wasSuccessful = false;
            SpecificationAttribute bayInternalList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.BayIntlLst);
            SpecificationAttribute externalDimensions = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.XtnlDimUom);
            SpecificationAttribute mountingPositionOffset = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.DimUom);

            if ("N".Equals(wallMountDefaultIndicator))
                wsm = "W";

            if ("N".Equals(straightThruDefaultIndicator))
                wsm += "S";

            if ("N".Equals(midPlaneDefaultIndicator))
                wsm += "M";

            if (bayInternalId > 0)
            {
                for (int i = 0; i < bayInternalList.ObjectList.Count; i++)
                {
                    SpecificationAttribute internalWidth = bayInternalList.ObjectList[i][SpecificationType.JSON.BayIntlWdth];
                    SpecificationAttribute id = bayInternalList.ObjectList[i][SpecificationType.JSON.SpecificationId];
                    SpecificationAttribute internalWidthUom = bayInternalList.ObjectList[i][SpecificationType.JSON.BayIntlWdthUom];
                    SpecificationAttribute mpQty = bayInternalList.ObjectList[i][SpecificationType.JSON.MntngPosQty];
                    SpecificationAttribute mpOffset = bayInternalList.ObjectList[i][SpecificationType.JSON.MntngPosDist];
                    int intId = int.Parse(id.Value);

                    if (bayInternalId == intId)
                    {
                        bayInternalWidth = UnitConverter.ConvertToInches(decimal.Parse(internalWidth.Value), internalWidthUom.Value, ref wasSuccessful);
                        mountingPositionQty = int.Parse(mpQty.Value);

                        mountingPositionOffsetValue = decimal.Parse(mpOffset.Value.Split(new char[] { ' ' })[0]);

                        break;
                    }
                }
            }

            for (int i = 1; i < externalDimensions.Options.Count; i++)
            {
                int id = int.Parse(externalDimensions.Options[i].Value);

                if (externalDimensionsUom == id)
                {
                    depth = UnitConverter.ConvertToInches(depth, externalDimensions.Options[i].Text, ref wasSuccessful);
                    height = UnitConverter.ConvertToInches(height, externalDimensions.Options[i].Text, ref wasSuccessful);
                    width = UnitConverter.ConvertToInches(width, externalDimensions.Options[i].Text, ref wasSuccessful);

                    break;
                }
            }

            //for (int i = 1; i < mountingPositionOffset.Options.Count; i++)
            //{
            //    int id = int.Parse(mountingPositionOffset.Options[i].Value);

            //    if (mountingPositionOffsetUom == id)
            //    {
            //        mountingPositionOffsetValue = UnitConverter.ConvertToInches(mountingPositionOffsetValue, mountingPositionOffset.Options[i].Text, ref wasSuccessful);

            //        break;
            //    }
            //}


            //BA-||wdth_no||"_||xtnl_hgt_no||x||xtnl_wdth_no||x||xtnl_dpth_no||_MP||mntng_pos_qty||x||mntng_pos_dist_no||_||W||S||M

            if (width <= 19)
                intWidth = 19;
            else if (width > 19 && width <= 23)
                intWidth = 23;
            else
                intWidth = int.Parse(decimal.Round(width).ToString());

            if (bayInternalWidth > 0)
                name += int.Parse(decimal.Round(bayInternalWidth).ToString()) + "\"" + "_";
            else
                name += "00\"" + "_";

            stringHeight = height.ToString("F2");
            stringWidth = width.ToString("F2");
            stringDepth = depth.ToString("F2");

            name += stringHeight + "x" + stringWidth + "x" + stringDepth + "_MP";

            if (bayInternalId > 0)
                name += mountingPositionQty + "x" + mountingPositionOffsetValue.ToString("F2");
            else
                name += "000x0.00";

            if (!string.IsNullOrEmpty(wsm))
                name += "_" + wsm;

            if (name.Length > 40)
                throw new Exception("Unable to update the specification. The system generated name is greater than 40 characters. Maximum length of the name is 40 characters.");

            return name;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate)
        {
            long specificationId = 0;
            long bayInternalId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlId);
            //long updatedRvsnId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            int maxWeightUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);            
            int mountingPositionOffsetUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            int roleTypeId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.BayRlTypId);
            decimal maxWeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            decimal mountingPositionOffset = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MntngPosOfst);
            string name = ""; //JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string startLabelIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrtLbl);
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            string dualSdIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.DualSd);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            bool isGeneric = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);

            if (isGeneric)
                name = FormSpecificationName(updatedSpecification);
            else
            {
                name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.SpecificationId));
            }

            if (string.IsNullOrEmpty(wallMountIndicator))
                wallMountIndicator = "N";

            if (string.IsNullOrEmpty(straightThruIndicator))
                straightThruIndicator = "N";

            if (string.IsNullOrEmpty(midPlaneIndicator))
                midPlaneIndicator = "N";

            try
            {
                //DbInterface.StartTransaction();

                specificationId = ((BaySpecificationDbInterface)DbInterface).CreateBaySpecification(name, description, mountingPositionOffset, mountingPositionOffsetUom, startLabelIndicator, wallMountIndicator, straightThruIndicator, 
                    midPlaneIndicator, bayInternalId, dualSdIndicator, isGeneric ? "Y" : "N");

                if (isGeneric)
                    CreateGenericSpecification(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);
                else
                    CreateSpecificationRevision(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);

                if (roleTypeId > 0)
                    ((BaySpecificationDbInterface)DbInterface).CreateBaySpecificationRole(specificationId, roleTypeId);

                //DbInterface.CommitTransaction();

                if (isPropagated)
                    notifyNDSOfSpecnUpdate = true;
            }
            catch (Exception ex)
            {
                //DbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to create new Bay specification");

                throw ex;
            }
            //finally
            //{
            //    DbInterface.Dispose();
            //}

            return specificationId;
        }

        private void CreateGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlDpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlHgt);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlWdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.XtnlDimUom);

            ((BaySpecificationDbInterface)DbInterface).CreateGenericBaySpecification(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N", isDeleted ? "Y" : "N", depth, height, width, uom);
        }

        private void CreateSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            decimal weight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            long revisionId = 0;            

            if (string.IsNullOrEmpty(name))
                name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);

            revisionId = ((BaySpecificationDbInterface)DbInterface).CreateBaySpecificationRevision(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N", isDeleted ? "Y" : "N", weight, uom, isRO ? "Y" : "N", name);

            ((BaySpecificationDbInterface)DbInterface).AssociateMaterial(revisionId, materialItemId);
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            long updatedBayInternalId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlId);
            long updatedRvsnId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            int updatedMaxWeightUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);            
            int updatedMountingPositionOffsetUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            decimal updatedMountingPositionOffset = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MntngPosOfst);
            decimal updatedMaxWeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            string updatedName = ""; //JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string updatedStartLabelIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrtLbl);
            string updatedWallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string updatedStraightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string updatedMidPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            string updatedDualSdIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.DualSd);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            if (IsGeneric)
                updatedName = FormSpecificationName(updatedSpecification);
            else
            {
                updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.SpecificationId));
            }

            if (BayInternalId != updatedBayInternalId || MountingPositionOffset != updatedMountingPositionOffset || MountingPositionOffsetUnitOfMeasureId != updatedMountingPositionOffsetUom ||
                Name != updatedName || Description != updatedDescription || LabelStartingPosition != updatedStartLabelIndicator || WallMountIndicator != updatedWallMountIndicator ||
                StraightThruIndicator != updatedStraightThruIndicator || MidPlaneIndicator != updatedMidPlaneIndicator || DualSDIndicator != updatedDualSdIndicator)
            {
                ((BaySpecificationDbInterface)DbInterface).UpdateBaySpecification(SpecificationId, updatedName, updatedDescription, updatedMountingPositionOffset, updatedMountingPositionOffsetUom, updatedStartLabelIndicator,
                    updatedWallMountIndicator, updatedStraightThruIndicator, updatedMidPlaneIndicator, updatedBayInternalId, updatedDualSdIndicator);

                notifyNDS = true;
            }

            if (IsGeneric)
                PersistGenericSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);
            else
                PersistSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);

            PersistSpecificationRole(updatedSpecification, ref notifyNDS);

            if (!updatedIsPropagated)
                notifyNDS = false;
        }

        private void PersistGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {            
            decimal updatedDepth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlDpth);
            decimal updatedHeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlHgt);
            decimal updatedWidth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlWdth);
            int updatedUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.XtnlDimUom);            

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || ExternalDepth != updatedDepth || ExternalHeight != updatedHeight ||
                ExternalWidth != updatedWidth || ExternalDimensionsUnitOfMeasure != updatedUom)
            {
                ((BaySpecificationDbInterface)DbInterface).UpdateGenericBaySpecification(SpecificationId, isCompleted, isPropagated, isDeleted, updatedDepth, updatedHeight, updatedWidth, updatedUom);

                notifyNDS = true;
            }            
        }

        private void PersistSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            decimal updatedWeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            int updatedUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);
            
            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || RevisionName != updatedName || MaxWeightCapacity != updatedWeight || MaxWeightCapacityUnitOfMeasure != updatedUom)
            {
                ((BaySpecificationDbInterface)DbInterface).UpdateBaySpecificationRevision(RevisionId, updatedName, isCompleted, isPropagated, isDeleted, updatedWeight, updatedUom);

                notifyNDS = true;
            }

            if (materialItemId > 0 && (bay == null || bay.MaterialItemId != materialItemId))
            {
                if(bay != null)
                    ((BaySpecificationDbInterface)DbInterface).AssociateMaterial(0, bay.MaterialItemId);

                ((BaySpecificationDbInterface)DbInterface).AssociateMaterial(RevisionId, materialItemId);

                notifyNDS = true;
            }
        }

        private void PersistSpecificationRole(JObject updatedSpecification, ref bool notifyNDS)
        {
            int updatedRole = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.BayRlTypId);

            if (RoleTypeId != updatedRole)
            {
                ((BaySpecificationDbInterface)DbInterface).UpdateBaySpecificationRole(SpecificationId, updatedRole);

                notifyNDS = true;
            }
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            decimal height = 0;
            decimal depth = 0;
            decimal width = 0;
            decimal plannedHeatGeneration = 0;
            decimal normalCurrentDrain = 0;
            decimal maxCurrentDrain = 0;
            decimal bayWeight = 0;
            decimal heatDissipation = 0;
            int uomId = 0;
            int plannedHeatGenerationUom = 0;
            int normalCurrentDrainUom = 0;
            int maxCurrentDrainUom = 0;
            int bayWeightUom = 0;
            int heatDissipationUom = 0;

            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.Hght), out height);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.Dpth), out depth);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.Wdth), out width);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.HtGntn), out plannedHeatGeneration);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.ElcCurrDrnNrm), out normalCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.ElcCurrDrnMx), out maxCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.Wght), out bayWeight);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.HtDssptn), out heatDissipation);

            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.HtGntnUom), out plannedHeatGenerationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.ElcCurrDrnNrmUom), out normalCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.ElcCurrDrnMxUom), out maxCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.WghtUom), out bayWeightUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", MaterialType.JSON.HtDssptnUom), out heatDissipationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Bay", SpecificationType.JSON.DimUom), out uomId);

            if(uomId == 0)
                uomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

            notifyNDS = ((BaySpecificationDbInterface)DbInterface).UpdateBaySpecificationMaterial(materialItemId, height, depth, width, uomId, plannedHeatGeneration, plannedHeatGenerationUom, normalCurrentDrain,
                normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, bayWeight, bayWeightUom, heatDissipation, heatDissipationUom);
        }

        public override Dictionary<string, SpecificationAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = GetAttributes(this, true);

                    if (SpecificationId > 0)
                    {
                        attributes[SpecificationType.JSON.Gnrc].IsEditable = false;
                        attributes[SpecificationType.JSON.RO].IsEditable = false;

                        if (!IsCompleted)
                        {
                            attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                            attributes[SpecificationType.JSON.Prpgtd].IsEditable = false;
                        }
                        else if (!IsPropagated)
                            attributes[SpecificationType.JSON.Prpgtd].IsEditable = true;

                        if (!IsDeleted)
                            attributes[SpecificationType.JSON.Dltd].IsEditable = true;

                        if (bay != null)
                        {
                            SpecificationAttribute bayAttr = new SpecificationAttribute();
                            Dictionary<string, SpecificationAttribute> dictionary = new Dictionary<string, SpecificationAttribute>();

                            bayAttr.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();

                            foreach (string key in bay.Attributes.Keys)
                            {
                                SpecificationAttribute attr = new SpecificationAttribute();

                                attr.Name = key;

                                if (bay.Attributes[key].Options != null)
                                {
                                    //if (MaterialType.JSON.DimUom.Equals(key))
                                    //{
                                        attr.Options = bay.Attributes[key].Options;
                                        attr.Value = bay.Attributes[key].Value;
                                    //}
                                    //else
                                    //{
                                    //    foreach (Option option in bay.Attributes[key].Options)
                                    //    {
                                    //        if (bay.Attributes[key].Value == option.Value)
                                    //        {
                                    //            attr.Value = option.Text;
                                    //            break;
                                    //        }
                                    //    }
                                    //}
                                }
                                else
                                    attr.Value = bay.Attributes[key].Value;

                                if (MaterialType.JSON.DimUom.Equals(key) || MaterialType.JSON.Dpth.Equals(key) || MaterialType.JSON.Hght.Equals(key) || MaterialType.JSON.Wdth.Equals(key))
                                    attr.IsEditable = true;

                                dictionary.Add(key, attr);
                            }

                            if (!dictionary.ContainsKey(MaterialType.JSON.Dpth))
                                dictionary.Add(MaterialType.JSON.Dpth, new SpecificationAttribute(false, MaterialType.JSON.Dpth));

                            if (!dictionary.ContainsKey(MaterialType.JSON.Hght))
                                dictionary.Add(MaterialType.JSON.Hght, new SpecificationAttribute(false, MaterialType.JSON.Hght));

                            if (!dictionary.ContainsKey(MaterialType.JSON.Wdth))
                                dictionary.Add(MaterialType.JSON.Wdth, new SpecificationAttribute(false, MaterialType.JSON.Wdth));

                            bayAttr.ObjectList.Add(dictionary);

                            attributes.Add("Bay", bayAttr);
                        }
                    }
                    else
                    {
                        attributes[SpecificationType.JSON.Gnrc].IsEditable = true;
                        attributes[SpecificationType.JSON.RO].IsEditable = true;
                        attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                        attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                    }
                }

                return attributes;
            }

            set
            {
                attributes = value;
            }
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
            ((BaySpecificationDbInterface)DbInterface).UpsertNDSSpecificationId(IsGeneric ? SpecificationId : RevisionId, ndsId, IsGeneric);
        }

        [JsonIgnore]
        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new BaySpecificationDbInterface();

                return dbInterface;
            }
        }
    }
}