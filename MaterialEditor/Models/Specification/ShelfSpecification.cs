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
    public class ShelfSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, SpecificationRole> roleList = null;
        private Dictionary<long, SpecificationRole> useTypeList = null;
        private ISpecificationDbInterface dbInterface = null;
        private IMaterial shelf = null;
        private long materialItemId = 0;

        public ShelfSpecification() : base()
        {
            PopulateRoleList();
        }

        public ShelfSpecification(long specificationId) : base(specificationId)
        {
            PopulateRoleList();
        }

        public ShelfSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateRoleList();
        }

        public void PopulateRoleList()
        {
            roleList = ((ShelfSpecificationDbInterface)DbInterface).GetRoleList(SpecificationId);
            useTypeList = ((ShelfSpecificationDbInterface)DbInterface).GetUseTypeList(SpecificationId);
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
        [MaterialJsonProperty(true, SpecificationType.JSON.SpcnUseTypLst, "")]
        public Dictionary<long, SpecificationRole> UseTypeList
        {
            get
            {
                return useTypeList;
            }
            set
            {
                useTypeList = value;
            }
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.ShelfUseTypId)]
        public string ShelfUseTypId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.ShelfNDSUseTyp)]
        public string ShelfNDSUseTyp
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Gnrc, true, "")]
        public bool IsGeneric
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RO, true, "")]
        public bool IsRecordOnly
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StrghtThru)]
        public string StraightThroughIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MidPln)]
        public string MidPlaneIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NdLvlMtrl)]
        public string NodeLevelMaterialIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.StrtSltNo)]
        public int StartingSlotNumber
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SltsRwQty)]
        public int SlotsPerRowQuantity
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.OrnttnId)]
        public int ObjectOrientationId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.LblNm)]
        public string LabelName
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
        [MaterialJsonProperty(true, SpecificationType.JSON.LblPosId)]
        public int LabelPositionId
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
        [MaterialJsonProperty(SpecificationType.JSON.RvsnNm)]
        public string RevisionName
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
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return shelf;
            }
            set
            {
                shelf = value;
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
        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new ShelfSpecificationDbInterface();

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

                    if (shelf != null)
                    {
                        SpecificationAttribute shelfAttr = new SpecificationAttribute();
                        Dictionary<string, SpecificationAttribute> dictionary = new Dictionary<string, SpecificationAttribute>();

                        shelfAttr.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();

                        foreach (string key in shelf.Attributes.Keys)
                        {
                            SpecificationAttribute attr = new SpecificationAttribute();

                            attr.Name = key;

                            if (shelf.Attributes[key].Options != null)
                            {
                                //if (MaterialType.JSON.DimUom.Equals(key))
                                //{
                                    attr.Options = shelf.Attributes[key].Options;
                                    attr.Value = shelf.Attributes[key].Value;
                                //}
                                //else
                                //{
                                //    foreach (Option option in shelf.Attributes[key].Options)
                                //    {
                                //        if (shelf.Attributes[key].Value == option.Value)
                                //        {
                                //            attr.Value = option.Text;
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                            else
                                attr.Value = shelf.Attributes[key].Value;

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

                        shelfAttr.ObjectList.Add(dictionary);

                        attributes.Add("Shelf", shelfAttr);
                    }
                }
                else
                {
                    attributes[SpecificationType.JSON.Gnrc].IsEditable = true;
                    attributes[SpecificationType.JSON.RO].IsEditable = true;
                    attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                    attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }
                //if (attributes[SpecificationType.JSON.SpcnUseTypLst].ObjectList != null)
                //{
                //    string mike = "made it here";
                //}
                //Initialize the role type list
                if (attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList != null)
                {
                    string selectedUseType = "";

                    //Find the first selected role type (if any)
                    for (int i = 0; i < attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList.Count; i++)
                    {
                        if (attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.Slctd].BoolValue)
                        {
                            selectedUseType = attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.UseTyp].Value;

                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(selectedUseType))
                    {
                        //Make sure only like use types are selectable
                        for (int i = 0; i < attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList.Count; i++)
                        {
                            if (selectedUseType.Equals(attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.UseTyp].Value))
                                attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.Slctd].IsEditable = true;
                            else
                            {
                                attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.Slctd].IsEditable = false;
                                attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.Slctd].BoolValue = false;
                                attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.PrtyNo].Value = "0";
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList.Count; i++)
                        {
                            attributes[SpecificationType.JSON.SpcnRlTypLst].ObjectList[i][SpecificationType.JSON.Slctd].IsEditable = true;
                        }
                    }
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
                return SpecificationType.Type.SHELF;
            }
        }

        private string FormSpecificationName(JObject updatedSpecification)
        {
            string name = "SH-";
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.WllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            //NOT COMPLETE
            name += height + "X" + width + "X" + depth;

            return name;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            bool isGeneric = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            string nodeLevelMaterial = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdLvlMtrl);
            decimal startingSlotNo = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.StrtSltNo);
            int orientationId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.OrnttnId);
            int slotsRowQuantity = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltsRwQty);
            string labelName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.LblNm);
            int labelPositionId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.LblPosId);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            string useType = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.ShelfNDSUseTyp);

            if (!isGeneric)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.SpecificationId));

            if (string.IsNullOrEmpty(straightThruIndicator))
                straightThruIndicator = "N";

            if (string.IsNullOrEmpty(midPlaneIndicator))
                midPlaneIndicator = "N";

            try
            {
                specificationId = ((ShelfSpecificationDbInterface)DbInterface).CreateShelfSpecification(name, description, isGeneric ? "Y" : "N", straightThruIndicator,
                    midPlaneIndicator, nodeLevelMaterial, startingSlotNo, orientationId, slotsRowQuantity, labelName, labelPositionId, useType);

                if (isGeneric)
                    CreateGenericSpecification(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);
                else
                    CreateSpecificationRevision(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);

                CreateShelfSpecificationRole(updatedSpecification, specificationId);

                if (isPropagated)
                    notifyNDS = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new Shelf specification");

                throw ex;
            }

            return specificationId;
        }

        private void CreateGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            ((ShelfSpecificationDbInterface)DbInterface).CreateGenericShelfSpecification(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N",
                isDeleted ? "Y" : "N", depth, height, width, uom);
        }

        private void CreateSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            string revisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            decimal weight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.MxWght);
            int uom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MxWghtUom);
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            long revisionId = 0;

            revisionId = ((ShelfSpecificationDbInterface)DbInterface).CreateShelfSpecificationRevision(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N",
                isDeleted ? "Y" : "N", weight, uom, isRO ? "Y" : "N", revisionName);

            ((ShelfSpecificationDbInterface)DbInterface).AssociateMaterial(revisionId, materialItemId);
        }

        private void CreateShelfSpecificationRole(JObject updatedSpecification, long specificationId)
        {
            List<Dictionary<string, SpecificationAttribute>> updatedRoleList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst).ObjectList;

            foreach (Dictionary<string, SpecificationAttribute> updatedRole in updatedRoleList)
            {
                if (updatedRole["Slctd"].BoolValue)
                {
                    ((ShelfSpecificationDbInterface)DbInterface).InsertShelfSpecificationRole(specificationId,
                        int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value));
                }
            }
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            bool updatedGeneric = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);
            string updatedStraightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string updatedMidPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.MidPln);
            string updatedNodeLvlMtrl = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.NdLvlMtrl);
            decimal updatedStartgSlotNo = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.StrtSltNo);
            int updatedSlotsRowQnty = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltsRwQty);
            int updatedOrientationId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.OrnttnId);
            string updatedLblName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.LblNm);
            int updatedLblPositionId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.LblPosId);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            string updatedUseType = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.ShelfNDSUseTyp);

            if (!IsGeneric)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.SpecificationId));

            if (Name != updatedName || Description != updatedDescription || StraightThroughIndicator != updatedStraightThruIndicator ||
                MidPlaneIndicator != updatedMidPlaneIndicator || NodeLevelMaterialIndicator != updatedNodeLvlMtrl || StartingSlotNumber != updatedStartgSlotNo
                || StartingSlotNumber != updatedSlotsRowQnty || ObjectOrientationId != updatedOrientationId || LabelName != updatedLblName ||
                LabelPositionId != updatedLblPositionId || UseType != updatedUseType)
            {
                ((ShelfSpecificationDbInterface)DbInterface).UpdateShelfSpecification(SpecificationId, updatedName, updatedDescription, updatedGeneric,
                    updatedStraightThruIndicator, updatedMidPlaneIndicator, updatedNodeLvlMtrl, updatedStartgSlotNo, updatedSlotsRowQnty, updatedOrientationId,
                    updatedLblName, updatedLblPositionId, updatedUseType);
                notifyNDS = true;
            }

            if (IsGeneric)
                PersistGenericSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);
            else
                PersistSpecificationRevision(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);

            PersistSpecificationRole(updatedSpecification, ref notifyNDS);

            if (!updatedIsPropagated)
                notifyNDS = false;
        }

        private void PersistGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            decimal updatedDepth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal updatedHeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal updatedWidth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            int updatedUom = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || Depth != updatedDepth || Height != updatedHeight ||
                Width != updatedWidth || DimensionsUnitOfMeasure != updatedUom)
            {
                ((ShelfSpecificationDbInterface)DbInterface).UpdateGenericShelfSpecification(SpecificationId, isCompleted, isPropagated, isDeleted, updatedDepth, updatedHeight, updatedWidth, updatedUom);

                notifyNDS = true;
            }
        }

        private void PersistSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            string updatedRevisionName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || RevisionName != updatedRevisionName || IsRecordOnly != isRO)
            {
                ((ShelfSpecificationDbInterface)DbInterface).UpdateShelfSpecificationRevision(RevisionId, SpecificationId, updatedRevisionName, isCompleted, isPropagated, isDeleted, isRO);

                notifyNDS = true;
            }

            if (materialItemId > 0 && (shelf == null || shelf.MaterialItemId != materialItemId))
            {
                if (shelf != null)
                    ((ShelfSpecificationDbInterface)DbInterface).AssociateMaterial(0, shelf.MaterialItemId);

                ((ShelfSpecificationDbInterface)DbInterface).AssociateMaterial(RevisionId, materialItemId);

                notifyNDS = true;
            }
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            decimal height = 0;
            decimal depth = 0;
            decimal width = 0;
            decimal normalCurrentDrain = 0;
            decimal maxCurrentDrain = 0;
            decimal shelfWeight = 0;
            decimal heatDissipation = 0;
            int uomId = 0;
            int normalCurrentDrainUom = 0;
            int maxCurrentDrainUom = 0;
            int shelfWeightUom = 0;
            int heatDissipationUom = 0;

            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.Hght), out height);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.Dpth), out depth);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.Wdth), out width);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.ElcCurrDrnNrm), out normalCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.ElcCurrDrnMx), out maxCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.Wght), out shelfWeight);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.HtDssptn), out heatDissipation);

            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.ElcCurrDrnNrmUom), out normalCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.ElcCurrDrnMxUom), out maxCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.WghtUom), out shelfWeightUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", MaterialType.JSON.HtDssptnUom), out heatDissipationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Shelf", SpecificationType.JSON.DimUom), out uomId);

            if (uomId == 0)
                uomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

            notifyNDS = ((ShelfSpecificationDbInterface)DbInterface).UpdateShelfSpecificationMaterial(materialItemId, height, depth, width, uomId, normalCurrentDrain,
                normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, shelfWeight, shelfWeightUom, heatDissipation, heatDissipationUom);
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

            ((ShelfSpecificationDbInterface)DbInterface).DeleteShelfSpecificationRole(SpecificationId);

            foreach (Dictionary<string, SpecificationAttribute> updatedRole in updatedRoleList)
            {
                if (updatedRole["Slctd"].BoolValue)
                {
                    ((ShelfSpecificationDbInterface)DbInterface).InsertShelfSpecificationRole(SpecificationId,
                        int.Parse(updatedRole["id"].Value), int.Parse(updatedRole["PrtyNo"].Value));
                }
            }

            notifyNDS = true;
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
            ((ShelfSpecificationDbInterface)DbInterface).UpsertNDSSpecificationId(SpecificationId, ndsId, IsGeneric);
        }
    }
}