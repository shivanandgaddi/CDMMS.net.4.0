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
    public class CardSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private long materialItemId = 0;
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, SlotConsumption> slotConsumptionList = null;
        private Dictionary<long, SpecificationRole> roleList = null;   
      //  private Dictionary<long, ManageSlots> MangSlotList = null;
        private Dictionary<long, SpecificationRole> useTypeList = null;
        private ISpecificationDbInterface dbInterface = null;
        private IMaterial card = null;
        
        public CardSpecification() : base()
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public CardSpecification(long specificationId) : base(specificationId)
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public CardSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateSlotConsumptionList();
            PopulateRoleList();
        }

        public void PopulateSlotConsumptionList()
        {
            slotConsumptionList = ((CardSpecificationDbInterface)DbInterface).GetSlotConsumptionList();
        }

        public void PopulateRoleList()
        {
            roleList = ((CardSpecificationDbInterface)DbInterface).GetRoleList(SpecificationId);
            useTypeList = ((CardSpecificationDbInterface)DbInterface).GetUseTypeList(SpecificationId);
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.SltCnsmptnLst, "")]
        public Dictionary<long, SlotConsumption> SlotConsumptionList
        {
            get
            {
                return slotConsumptionList;
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
        [MaterialJsonProperty(SpecificationType.JSON.CardNDSUseTyp)]
        public string CardNDSUseTyp
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
        [MaterialJsonProperty(SpecificationType.JSON.Clei)]
        public string CLEI
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Heci)]
        public string HECI
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
        [MaterialJsonProperty(SpecificationType.JSON.CardUseTypId)]
        public string CardUseTypId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SltCnsmptnId)]
        public int SlotConsumptionId
        {
            get;
            set;
        }

        //[JsonIgnore]
        //[MaterialJsonProperty(true, SpecificationType.JSON.CrdPstn, OptionValues = new string[] { "Left/Upper", "Left/Upper", "Right/Lower", "Right/Lower" })]
        //public string Position
        //{
        //    get;
        //    set;
        //}

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Slts, true, "")]
        public bool HasSlots
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Prts, true, "")]
        public bool HasPorts
        {
            get;
            set;
        }

        //[JsonIgnore]
        //[MaterialJsonProperty(SpecificationType.JSON.Itrm, true, "")]
        //public bool IsInterim
        //{
        //    get;
        //    set;
        //}

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.RO, true, "")]
        public bool IsRecordOnly
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
        [MaterialJsonProperty(SpecificationType.JSON.RvsnId)]
        public long RevisionId
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
        [MaterialJsonProperty(SpecificationType.JSON.MtlItmId)]
        public long MaterialRevisionId
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
        public override IMaterial AssociatedMaterial
        {
            get
            {
                return card;
            }
            set
            {
                card = value;
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
                    dbInterface = new CardSpecificationDbInterface();

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
                    //attributes[SpecificationType.JSON.Itrm].IsEditable = false;
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

                    if (card != null)
                    {
                        SpecificationAttribute cardAttr = new SpecificationAttribute();
                        Dictionary<string, SpecificationAttribute> dictionary = new Dictionary<string, SpecificationAttribute>();

                        cardAttr.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();

                        foreach (string key in card.Attributes.Keys)
                        {
                            SpecificationAttribute attr = new SpecificationAttribute();

                            attr.Name = key;

                            
                            if (card.Attributes[key].Options != null)
                            {
                                //if (MaterialType.JSON.DimUom.Equals(key))
                                //{
                                    attr.Options = card.Attributes[key].Options;
                                    attr.Value = card.Attributes[key].Value;
                                //}
                                //else
                                //{
                                //    foreach (Option option in card.Attributes[key].Options)
                                //    {
                                //        if (card.Attributes[key].Value == option.Value)
                                //        {
                                //            attr.Value = option.Text;
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                            else
                                attr.Value = card.Attributes[key].Value;

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

                        cardAttr.ObjectList.Add(dictionary);

                        attributes.Add("Card", cardAttr);
                    }
                }
                else
                {
                    //attributes[SpecificationType.JSON.Itrm].IsEditable = true;
                    attributes[SpecificationType.JSON.RO].IsEditable = true;
                    attributes[SpecificationType.JSON.Cmplt].IsEditable = true;
                    attributes[SpecificationType.JSON.Dltd].IsEditable = true;
                }

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
                return SpecificationType.Type.CARD;
            }
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long specificationId = 0;
            long rvsnId = 0; //JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            long slotConsumptionId = 0;
            int slotConsumptionRowNumber = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltCnsmptnId);
            int uomId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            //string positionValue = null;
            string rvsnName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            bool completed = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool propagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool deleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            //bool isInterim = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Itrm);
            bool ro = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            bool hasSlotsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Slts);
            bool hasPortsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prts);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string useType = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.CardNDSUseTyp);

            SpecificationAttribute roleTypeList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst);

            materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.SpecificationId));

            if (string.IsNullOrEmpty(rvsnName))
                rvsnName = name;

            try
            {
                materialItemId = long.Parse((string)((JArray)((JObject)updatedSpecification.SelectToken("Card")).SelectToken("list"))[0].SelectToken("id.value"));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve material item id.");

                throw ex;
            }

            try
            {
                if (slotConsumptionRowNumber > 0)
                {
                    //positionValue = slotConsumptionList[slotConsumptionRowNumber].Position;
                    slotConsumptionId = slotConsumptionList[slotConsumptionRowNumber].SpecificationId;
                }

                //DbInterface.StartTransaction();

                specificationId = ((CardSpecificationDbInterface)DbInterface).CreateCardSpecification(name, description, slotConsumptionId, hasSlotsInd, hasPortsInd,straightThruIndicator, useType); //, isInterim);

                //if (isInterim)
                //    ((CardSpecificationDbInterface)DbInterface).CreateInterimCardSpecification(specificationId, depth, height, width, uomId, completed, propagated);
                //else
                rvsnId = ((CardSpecificationDbInterface)DbInterface).CreateCardSpecificationRevision(specificationId, rvsnName, ro, deleted, completed, propagated);

                ((CardSpecificationDbInterface)DbInterface).AssociateMaterial(rvsnId, materialItemId);

                if (roleTypeList != null && roleTypeList.ObjectList != null)
                {
                    for (int i = 0; i < roleTypeList.ObjectList.Count; i++)
                    {
                        SpecificationAttribute selected = roleTypeList.ObjectList[i][SpecificationType.JSON.Slctd];
                        SpecificationAttribute id = roleTypeList.ObjectList[i][SpecificationType.JSON.SpecificationId];
                        SpecificationAttribute priorityNumber = roleTypeList.ObjectList[i][SpecificationType.JSON.PrtyNo];

                        if (selected.BoolValue)
                            ((CardSpecificationDbInterface)DbInterface).InsertCardSpecificationRole(specificationId, int.Parse(id.Value), int.Parse(priorityNumber.Value));
                    }
                }

                //DbInterface.CommitTransaction();

                if (propagated)
                    notifyNDS = true;
            }
            catch (Exception ex)
            {
                //DbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to create new Card specification");

                throw ex;
            }
            //finally
            //{
            //    DbInterface.Dispose();
            //}

            return specificationId;
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            long rvsnId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            long updatedSlotConsumptionId = 0;
            int updatedSlotConsumptionRowNumber = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.SltCnsmptnId);            
            int updatedUomId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.DimUom);
            decimal updatedDepth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Dpth);
            decimal updatedHeight = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Hght);
            decimal updatedWidth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.Wdth);
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            //string updatedPositionValue = null; 
            string updatedRvsnName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            bool updatedCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            //bool updatedIsInterim = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Itrm);
            bool updatedRo = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            bool updatedHasSlotsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Slts);
            bool updatedHasPortsInd = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prts);
            string updatedStraightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.StrghtThru);
            string updatedUseType = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.CardNDSUseTyp);

            bool positionUpdated = false;
            SpecificationAttribute updatedRoleTypeList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.SpcnRlTypLst);

            materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.SpecificationId));

            if (updatedSlotConsumptionRowNumber > 0)
            {
                //updatedPositionValue = slotConsumptionList[updatedSlotConsumptionRowNumber].Position;
                updatedSlotConsumptionId = slotConsumptionList[updatedSlotConsumptionRowNumber].SpecificationId;
            }

            //if ((string.IsNullOrEmpty(Position) && !string.IsNullOrEmpty(updatedPositionValue)) || (!string.IsNullOrEmpty(Position) && string.IsNullOrEmpty(updatedPositionValue)))
            //    positionUpdated = true;
            //else if (!string.IsNullOrEmpty(Position) && !string.IsNullOrEmpty(updatedPositionValue) && Position != updatedPositionValue)
            //    positionUpdated = true;

            if (Name != updatedName || Description != updatedDescription || SlotConsumptionId != updatedSlotConsumptionId || positionUpdated || HasSlots != updatedHasSlotsInd || HasPorts != updatedHasPortsInd || StraightThruIndicator != updatedStraightThruIndicator || UseType != updatedUseType)
            {
                ((CardSpecificationDbInterface)DbInterface).UpdateCardSpecification(SpecificationId, updatedName, updatedDescription, updatedSlotConsumptionId, updatedHasSlotsInd, updatedHasPortsInd,updatedStraightThruIndicator, updatedUseType); //, updatedIsInterim);

                notifyNDS = true;
            }

            //if (IsInterim && (IsCompleted != updatedCompleted || IsPropagated != updatedPropagated || Depth != updatedDepth || Height != updatedHeight || Width != updatedWidth || DimensionsUnitOfMeasure != updatedUomId))
            //{
            //    ((CardSpecificationDbInterface)DbInterface).UpdateInterimCardSpecification(SpecificationId, updatedDepth, updatedHeight, updatedWidth, updatedUomId, updatedCompleted, updatedPropagated);

            //    notifyNDS = true;
            //}
            //else 
            
            if (RevisionName != updatedRvsnName || IsRecordOnly != updatedRo || IsCompleted != updatedCompleted || IsPropagated != updatedPropagated || IsDeleted != updatedDeleted)
            {
                ((CardSpecificationDbInterface)DbInterface).UpdateCardSpecificationRevision(RevisionId, SpecificationId, updatedRvsnName, updatedRo, updatedDeleted, updatedCompleted, updatedPropagated);

                notifyNDS = true;
            }

            if (materialItemId > 0 && (card == null || card.MaterialItemId != materialItemId))
            {
                if (card != null)
                    ((CardSpecificationDbInterface)DbInterface).AssociateMaterial(0, card.MaterialItemId);

                ((CardSpecificationDbInterface)DbInterface).AssociateMaterial(RevisionId, materialItemId);

                notifyNDS = true;
            }

            ((CardSpecificationDbInterface)DbInterface).DeleteCardSpecificationRole(SpecificationId);

            if (updatedRoleTypeList != null && updatedRoleTypeList.ObjectList != null)
            {
                for (int i = 0; i < updatedRoleTypeList.ObjectList.Count; i++)
                {
                    SpecificationAttribute updatedSelected = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.Slctd];
                    SpecificationAttribute updatedId = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.SpecificationId];
                    SpecificationAttribute priorityNumber = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.PrtyNo];

                    if (updatedSelected.BoolValue)
                        ((CardSpecificationDbInterface)DbInterface).InsertCardSpecificationRole(SpecificationId, int.Parse(updatedId.Value), int.Parse(priorityNumber.Value));                 
                }
            }

            //if (updatedRoleTypeList != null && updatedRoleTypeList.ObjectList != null && RoleList != null)
            //{
            //    for (int i = 0; i < updatedRoleTypeList.ObjectList.Count; i++)
            //    {
            //        SpecificationAttribute updatedSelected = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.Slctd];
            //        SpecificationAttribute updatedId = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.SpecificationId];
            //        SpecificationAttribute priorityNumber = updatedRoleTypeList.ObjectList[i][SpecificationType.JSON.PrtyNo];

            //        if (updatedSelected.BoolValue)
            //        {
            //            if (RoleList[long.Parse(updatedId.Value)].IsSelected)
            //            {
            //                if (RoleList[long.Parse(updatedId.Value)].PriorityNumber != int.Parse(priorityNumber.Value))
            //                {
            //                    ((CardSpecificationDbInterface)DbInterface).UpdateCardSpecificationRole(SpecificationId, int.Parse(updatedId.Value), int.Parse(priorityNumber.Value));

            //                    notifyNDS = true;
            //                }
            //            }
            //            else
            //            {
            //                ((CardSpecificationDbInterface)DbInterface).InsertCardSpecificationRole(SpecificationId, int.Parse(updatedId.Value), int.Parse(priorityNumber.Value));

            //                notifyNDS = true;
            //            }
            //        }
            //        else if (RoleList[long.Parse(updatedId.Value)].IsSelected)
            //        {
            //            ((CardSpecificationDbInterface)DbInterface).DeleteCardSpecificationRole(SpecificationId, int.Parse(updatedId.Value));

            //            notifyNDS = true;
            //        }
            //    }
            //}
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            decimal height = 0;
            decimal depth = 0;
            decimal width = 0;
            decimal normalCurrentDrain = 0;
            decimal maxCurrentDrain = 0;
            decimal cardWeight = 0;
            decimal heatDissipation = 0;
            int uomId = 0;
            int normalCurrentDrainUom = 0;
            int maxCurrentDrainUom = 0;
            int cardWeightUom = 0;
            int heatDissipationUom = 0;
            int rotationId = 0;

            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.Hght), out height);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.Dpth), out depth);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.Wdth), out width);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.ElcCurrDrnNrm), out normalCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.ElcCurrDrnMx), out maxCurrentDrain);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.Wght), out cardWeight);
            decimal.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.HtDssptn), out heatDissipation);

            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.ElcCurrDrnNrmUom), out normalCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.ElcCurrDrnMxUom), out maxCurrentDrainUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.WghtUom), out cardWeightUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", MaterialType.JSON.HtDssptnUom), out heatDissipationUom);
            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.DimUom), out uomId);

            int.TryParse(GetMaterialAttributeValue(updatedSpecification, "Card", SpecificationType.JSON.RotationAngl), out rotationId);

            if (uomId == 0)
                uomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

            notifyNDS = ((CardSpecificationDbInterface)DbInterface).UpdateCardSpecificationMaterial(materialItemId, height, depth, width, uomId, normalCurrentDrain,
                normalCurrentDrainUom, maxCurrentDrain, maxCurrentDrainUom, cardWeight, cardWeightUom, heatDissipation, heatDissipationUom);
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
            ((CardSpecificationDbInterface)DbInterface).UpsertNDSSpecificationId(SpecificationId, ndsId);
        }

        public class SlotConsumption : Specification
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            private Dictionary<string, SpecificationAttribute> attributes = null;
            private ISpecificationDbInterface dbInterface = null;

            public SlotConsumption() : base()
            {
                
            }

            public SlotConsumption(long specificationId) : base(specificationId)
            {
                
            }

            public SlotConsumption(long specificationId, string specificationName) : base(specificationId, specificationName)
            {
                
            }

            [JsonIgnore]
            [MaterialJsonProperty(true, SpecificationType.JSON.SltCnsmptnQty)]
            public int Quantity
            {
                get;
                set;
            }

            [JsonIgnore]
            [MaterialJsonProperty(SpecificationType.JSON.PrtyNo)]
            public int PriorityNumber
            {
                get;
                set;
            }

            [JsonIgnore]
            [MaterialJsonProperty(SpecificationType.JSON.SltCnsmptnTyp)]
            public string SlotConsumptionType
            {
                get;
                set;
            }

            [JsonIgnore]
            [MaterialJsonProperty(SpecificationType.JSON.CrdPstn)]
            public string Position
            {
                get;
                set;
            }

            [JsonIgnore]
            [MaterialJsonProperty(SpecificationType.JSON.RwNum)]
            public int RowNumber
            {
                get;
                set;
            }

            [JsonIgnore]
            [MaterialJsonProperty(SpecificationType.JSON.Dflt)]
            public string DefaultIndicator
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
                        dbInterface = new CardSpecificationDbInterface();

                    return dbInterface;
                }
            }

            public override Dictionary<string, SpecificationAttribute> Attributes
            {
                get
                {
                    if (attributes == null)
                        attributes = GetAttributes(this, true);

                    return attributes;
                }

                set
                {
                    attributes = value;
                }
            }

            public override SpecificationType.Type Type
            {
                get
                {
                    return SpecificationType.Type.CARD_SLOT_CONSUMPTION;
                }
            }

            public override IMaterial AssociatedMaterial
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override long AssociatedMaterialId
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            //public override long PersistObject(JObject updatedSpecification, ref bool notifyNDS)
            //{
            //    throw new NotImplementedException();
            //}

            public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
            {
                throw new NotImplementedException();
            }

            public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
            {
                throw new NotImplementedException();
            }

            public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
            {
                throw new NotImplementedException();
            }

            public override void PersistNDSSpecificationId(long ndsId)
            {
                ((CardSpecificationDbInterface)DbInterface).UpsertNDSSpecificationId(SpecificationId, ndsId);
            }
        }       
    }   
}