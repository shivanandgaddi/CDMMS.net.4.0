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
    public class BayExtenderSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, BayInternalHeight> bayInternalHeightList = null;
        private Dictionary<long, BayInternalDepth> bayInternalDepthList = null;
        private Dictionary<long, BayInternalWidth> bayInternalWidthList = null;
        private long bayInternalId = 0;
        private long materialItemId = 0;
        private ISpecificationDbInterface dbInterface = null;
        private IMaterial bayExtender = null;

        public BayExtenderSpecification() : base()
        {
            PopulateBayInternalHeightList();
            PopulateBayInternalDepthList();
            PopulateBayInternalWidthList();
        }

        public BayExtenderSpecification(long specificationId) : base(specificationId)
        {
            PopulateBayInternalHeightList();
            PopulateBayInternalDepthList();
            PopulateBayInternalWidthList();
        }

        public BayExtenderSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateBayInternalHeightList();
            PopulateBayInternalDepthList();
            PopulateBayInternalWidthList();
        }

        private void PopulateBayInternalHeightList()
        {
            bayInternalHeightList = ((BayExtenderSpecificationDbInterface)DbInterface).GetBayInternalHeight();
        }

        private void PopulateBayInternalDepthList()
        {
            bayInternalDepthList = ((BayExtenderSpecificationDbInterface)DbInterface).GetBayInternalDepth();
        }

        private void PopulateBayInternalWidthList()
        {
            bayInternalWidthList = ((BayExtenderSpecificationDbInterface)DbInterface).GetBayInternalWidth();
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlHghtLst, "")]
        public Dictionary<long, BayInternalHeight> BayInternalHeightList
        {
            get
            {
                return bayInternalHeightList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlDpthLst, "")]
        public Dictionary<long, BayInternalDepth> BayInternalDepthList
        {
            get
            {
                return bayInternalDepthList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlWdthLst, "")]
        public Dictionary<long, BayInternalWidth> BayInternalWidthList
        {
            get
            {
                return bayInternalWidthList;
            }
        }

        ////MODEL_NO column was removed from the database. BAY_EXTNDR_SPECN.BAY_EXTNDR_SPECN_NM will now contain the "Model" for a bay
        //[JsonIgnore]
        //[MaterialJsonProperty(SpecificationType.JSON.MdlNo)]
        //public string ModelNumber
        //{
        //    get;
        //    set;
        //}

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Gnrc)]
        public bool IsGeneric
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayXtnUseTypId)]
        public string BayXtnUseTypeId
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
                return bayExtender;
            }
            set
            {
                bayExtender = value;
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
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthId)]
        public long BayInternalDepthId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlHghtId)]
        public long BayInternalHeightId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthId)]
        public long BayInternalWidthId
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
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.BAY_EXTENDER;
            }
        }

        private string FormSpecificationName(JObject updatedSpecification)
        {
            string name = "BE-";
            
            decimal depth = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlDpth);
            decimal height = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlHgt);
            decimal width = JsonHelper.GetSpecificationDecimalValue(updatedSpecification, SpecificationType.JSON.XtnlWdth);

            //NOT COMPLETE
            name += height + "X" + width + "X" + depth;

            return name;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate)
        {
            long specificationId = 0;
            //long updatedRvsnId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);
            bool isGeneric = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);

            if (!isGeneric)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "BayExtndr", SpecificationType.JSON.SpecificationId));

            try
            {
                specificationId = ((BayExtenderSpecificationDbInterface)DbInterface).CreateBayExtenderSpecification(name, description, isGeneric ? "Y" : "N");

                if (isGeneric)
                    CreateGenericSpecification(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);
                else
                    CreateSpecificationRevision(updatedSpecification, isCompleted, isPropagated, isDeleted, specificationId);

                if (isPropagated)
                    notifyNDSOfSpecnUpdate = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new Bay specification");

                throw ex;
            }

            return specificationId;
        }

        private void CreateGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            long depth = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlDpthId);
            long height = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlHghtId);
            long width = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlWdthId);

            ((BayExtenderSpecificationDbInterface)DbInterface).CreateGenericBayExtenderSpecification(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N", isDeleted ? "Y" : "N", depth, height, width);
        }

        private void CreateSpecificationRevision(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, long specificationId)
        {
            string name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);
            bool isRO = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.RO);
            long revisionId = 0;

            if (string.IsNullOrEmpty(name))
                name = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);

            revisionId = ((BayExtenderSpecificationDbInterface)DbInterface).CreateBayExtenderSpecificationRevision(specificationId, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N", isDeleted ? "Y" : "N", isRO ? "Y" : "N", name);

            ((BayExtenderSpecificationDbInterface)DbInterface).AssociateMaterial(revisionId, materialItemId);
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            long updatedBayInternalId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlId);
            long updatedRvsnId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.RvsnId);
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            if (!IsGeneric)
                materialItemId = long.Parse(GetMaterialAttributeValue(updatedSpecification, "BayExtndr", SpecificationType.JSON.SpecificationId));

            if (Name != updatedName || Description != updatedDescription)
            {
                ((BayExtenderSpecificationDbInterface)DbInterface).UpdateBayExtenderSpecification(SpecificationId, updatedName, updatedDescription);

                notifyNDS = true;
            }

            if (IsGeneric)
                PersistGenericSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);
            else
                PersistSpecification(updatedSpecification, updatedIsCompleted, updatedIsPropagated, updatedIsDeleted, ref notifyNDS);

            if (!updatedIsPropagated)
                notifyNDS = false;
        }

        private void PersistGenericSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            long updatedDepth = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlDpthId);
            long updatedHeight = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlHghtId);
            long updatedWidth = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlWdthId);

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || BayInternalDepthId != updatedDepth || BayInternalHeightId != updatedHeight ||
                BayInternalWidthId != updatedWidth)
            {
                ((BayExtenderSpecificationDbInterface)DbInterface).UpdateGenericBayExtenderSpecification(SpecificationId, isCompleted, isPropagated, isDeleted, updatedDepth, updatedHeight, updatedWidth);

                notifyNDS = true;
            }
        }

        private void PersistSpecification(JObject updatedSpecification, bool isCompleted, bool isPropagated, bool isDeleted, ref bool notifyNDS)
        {
            string updatedName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.RvsnNm);

            if (IsCompleted != isCompleted || IsPropagated != isPropagated || IsDeleted != isDeleted || RevisionName != updatedName)
            {
                ((BayExtenderSpecificationDbInterface)DbInterface).UpdateBayExtenderSpecificationRevision(RevisionId, updatedName, isCompleted, isPropagated, isDeleted);

                notifyNDS = true;
            }

            if (materialItemId > 0 && (bayExtender == null || bayExtender.MaterialItemId != materialItemId))
            {
                if (bayExtender != null)
                    ((BayExtenderSpecificationDbInterface)DbInterface).AssociateMaterial(0, bayExtender.MaterialItemId);

                ((BayExtenderSpecificationDbInterface)DbInterface).AssociateMaterial(RevisionId, materialItemId);

                notifyNDS = true;
            }
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            long height = long.Parse(GetMaterialAttributeValue(updatedSpecification, "BayExtndr", MaterialType.JSON.IntlHghtId));
            long depth = long.Parse(GetMaterialAttributeValue(updatedSpecification, "BayExtndr", MaterialType.JSON.IntlDpthId));
            long width = long.Parse(GetMaterialAttributeValue(updatedSpecification, "BayExtndr", MaterialType.JSON.IntlWdthId));

            notifyNDS = ((BayExtenderSpecificationDbInterface)DbInterface).UpdateBayExtenderSpecificationMaterial(materialItemId, height, depth, width);
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

                        if (bayExtender != null)
                        {
                            SpecificationAttribute bayAttr = new SpecificationAttribute();
                            Dictionary<string, SpecificationAttribute> dictionary = new Dictionary<string, SpecificationAttribute>();

                            bayAttr.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();

                            foreach (string key in bayExtender.Attributes.Keys)
                            {
                                SpecificationAttribute attr = new SpecificationAttribute();

                                attr.Name = key;

                                if (bayExtender.Attributes[key].Options != null)
                                {
                                    //if (MaterialType.JSON.DimUom.Equals(key))
                                    //{
                                        attr.Options = bayExtender.Attributes[key].Options;
                                        attr.Value = bayExtender.Attributes[key].Value;
                                    //}
                                    //else
                                    //{
                                    //    foreach (Option option in bayExtender.Attributes[key].Options)
                                    //    {
                                    //        if (bayExtender.Attributes[key].Value == option.Value)
                                    //        {
                                    //            attr.Value = option.Text;
                                    //            break;
                                    //        }
                                    //    }
                                    //}
                                }
                                else
                                    attr.Value = bayExtender.Attributes[key].Value;

                                //if (MaterialType.JSON.DimUom.Equals(key) || MaterialType.JSON.Dpth.Equals(key) || MaterialType.JSON.Hght.Equals(key) || MaterialType.JSON.Wdth.Equals(key))
                                    //attr.IsEditable = true;

                                dictionary.Add(key, attr);
                            }

                            //if (!dictionary.ContainsKey(MaterialType.JSON.Dpth))
                            //    dictionary.Add(MaterialType.JSON.Dpth, new SpecificationAttribute(false, MaterialType.JSON.Dpth));

                            //if (!dictionary.ContainsKey(MaterialType.JSON.Hght))
                            //    dictionary.Add(MaterialType.JSON.Hght, new SpecificationAttribute(false, MaterialType.JSON.Hght));

                            //if (!dictionary.ContainsKey(MaterialType.JSON.Wdth))
                            //    dictionary.Add(MaterialType.JSON.Wdth, new SpecificationAttribute(false, MaterialType.JSON.Wdth));

                            bayAttr.ObjectList.Add(dictionary);

                            attributes.Add("BayExtndr", bayAttr);
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
            ((BayExtenderSpecificationDbInterface)DbInterface).UpsertNDSSpecificationId(IsGeneric ? SpecificationId : RevisionId, ndsId, IsGeneric);
        }

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new BayExtenderSpecificationDbInterface();

                return dbInterface;
            }
        }
    }
}