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
    public class BayInternalSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private Dictionary<long, BayInternalDepth> internalDepthList = null;
        private Dictionary<long, BayInternalWidth> internalWidthList = null;
        private long internalDepthId = 0;
        private long internalWidthId = 0;
        private decimal depth = 0;
        private decimal width = 0;
        private string depthUOM = "";
        private string widthUOM = "";
        private ISpecificationDbInterface dbInterface = null;

        public BayInternalSpecification() : base()
        {
            PopulateDepthAndWidthLists();
        }

        public BayInternalSpecification(long specificationId) : base(specificationId)
        {
            PopulateDepthAndWidthLists();
        }

        public BayInternalSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            PopulateDepthAndWidthLists();
        }

        public BayInternalSpecification(long specificationId, bool populateDepthAndWidth) : base(specificationId)
        {
            if (populateDepthAndWidth)
                PopulateDepthAndWidthLists();
        }

        private void PopulateDepthAndWidthLists()
        {
            internalDepthList = ((BayInternalDbInterface)DbInterface).GetInternalDepths();
            internalWidthList = ((BayInternalDbInterface)DbInterface).GetInternalWidths();
        }

        
        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlDpthLst, "")]
        public Dictionary<long, BayInternalDepth> InternalDepthList
        {
            get
            {
                return internalDepthList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.BayIntlWdthLst, "")]
        public Dictionary<long, BayInternalWidth> InternalWidthList
        {
            get
            {
                return internalWidthList;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthId)]
        public long InternalDepthId
        {
            get
            {
                return internalDepthId;
            }
            set
            {
                internalDepthId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthId)]
        public long InternalWidthId
        {
            get
            {
                return internalWidthId;
            }
            set
            {
                internalWidthId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpth)]
        public decimal Depth
        {
            get
            {
                if (internalDepthId > 0 && internalDepthList != null)
                {
                    if (internalDepthList.ContainsKey(internalDepthId))
                        return internalDepthList[internalDepthId].Depth;
                    else
                        return depth;
                }
                else
                    return depth;
            }
            set
            {
                depth = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthUomId)]
        public int DepthUnitOfMeasureId
        {
            get
            {
                if (internalDepthId > 0 && internalDepthList != null)
                {
                    if (internalDepthList.ContainsKey(internalDepthId))
                        return internalDepthList[internalDepthId].DepthUnitOfMeasureId;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthUom)]
        public string DepthUnitOfMeasure
        {
            get
            {
                if (internalDepthId > 0 && internalDepthList != null)
                {
                    if (internalDepthList.ContainsKey(internalDepthId))
                        return internalDepthList[internalDepthId].DepthUnitOfMeasure;
                    else
                        return depthUOM;
                }
                else
                    return depthUOM;
            }
            set
            {
                depthUOM = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdth)]
        public decimal Width
        {
            get
            {
                if (internalWidthId > 0 && internalWidthList != null)
                {
                    if (internalWidthList.ContainsKey(internalWidthId))
                        return internalWidthList[internalWidthId].Width;
                    else
                        return width;
                }
                else
                    return width;
            }
            set
            {
                width = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthUomId)]
        public int WidthUnitOfMeasureId
        {
            get
            {
                if (internalWidthId > 0 && internalWidthList != null)
                {
                    if (internalWidthList.ContainsKey(internalWidthId))
                        return internalWidthList[internalWidthId].WidthUnitOfMeasureId;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthUom)]
        public string WidthUnitOfMeasure
        {
            get
            {
                if (internalWidthId > 0 && internalWidthList != null)
                {
                    if (internalWidthList.ContainsKey(internalWidthId))
                        return internalWidthList[internalWidthId].WidthUnitOfMeasure;
                    else
                        return widthUOM;
                }
                else
                    return widthUOM;
            }
            set
            {
                widthUOM = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Nmnl)]
        public string WidthNominalIndicator
        {
            get
            {
                if (internalWidthId > 0 && internalWidthList != null)
                {
                    if (internalWidthList.ContainsKey(internalWidthId))
                        return internalWidthList[internalWidthId].NominalIndicator;
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlHght)]
        public decimal Height
        {
            get
            {
                decimal height = 0;

                if (MountingPositionQuantity != null && MountingPositionDistance != null)
                {
                    string distString = "";                    

                    try
                    {
                        distString = MountingPositionDistance.Split(' ')[0];
                    }
                    catch (Exception ex)
                    {
                    }

                    if (!string.IsNullOrEmpty(distString))
                    {
                        decimal.TryParse(distString, out height);

                        height *= MountingPositionQuantity;
                    }
                }

                return height;
            }

            set
            {
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlHghtUom)]
        public string HeightUnitOfMeasure
        {
            get
            {
                return "in";
            }

            set
            {
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MntngPosQty)]
        public int MountingPositionQuantity
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(true, SpecificationType.JSON.MntngPosDistId)]
        public int MountingPositionDistanceId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlUseTypId)]
        public string BayIntlUseTypeId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.MntngPosDist)]
        public string MountingPositionDistance
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWllMnt)]
        public string WallMountIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlStrghtThru)]
        public string StraightThruIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlMidPln)]
        public string MidPlaneIndicator
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
                return SpecificationType.Type.BAY_INTERNAL;
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

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new BayInternalDbInterface();

                return dbInterface;
            }
        }

        private string FormSpecificationName(JObject updatedSpecification)
        {
            string name = "BI-";
            string existingName = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlWllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlStrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlMidPln);
            string wallMountDefaultIndicator = ReferenceDbInterface.GetWallMountAllowedDefaultIndicator(wallMountIndicator, "bay_itnl");
            string straightThruDefaultIndicator = ReferenceDbInterface.GetStraightThruDefaultIndicator(straightThruIndicator, "bay_itnl");
            string midPlaneDefaultIndicator = ReferenceDbInterface.GetMidPlaneDefaultIndicator(midPlaneIndicator, "bay_itnl");
            string wsm = "";
            string stringHeight = "";
            string stringWidth = "";
            string stringDepth = "";
            int mountingPositionDistanceId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosDistId);
            int mountingPositionQuantity = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosQty);
            int depthId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.BayIntlDpthId);
            int widthId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.BayIntlWdthId);
            int intWidth = 0;
            decimal height = 0;
            decimal depth = 0; 
            decimal width = 0;
            decimal mountingPositionDistance = 0;
            bool wasSuccessful = false;
            SpecificationAttribute depthList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.BayIntlDpthLst);
            SpecificationAttribute widthList = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.BayIntlWdthLst);
            SpecificationAttribute mountingPositionDistanceAttribute = JsonHelper.DeserializeSpecificationAttribute(updatedSpecification, SpecificationType.JSON.MntngPosDistId);

            //if (!string.IsNullOrEmpty(existingName) && existingName.StartsWith("Generic_"))
            //    name = "Generic_" + name;

            //Next join from the bay_itnl table to the wll_mnt_allow table where the wll_mnt_allow_ind columns are equal. Exactly one record will be returned. If the
            //bay_itnl_dflt_ind column equals 'N' on the record, then retain a 'W' for use later;
            if ("N".Equals(wallMountDefaultIndicator))
                wsm = "W";

            if ("N".Equals(straightThruDefaultIndicator))
                wsm += "S";

            if ("N".Equals(midPlaneDefaultIndicator))
                wsm += "M";

            for (int i = 0; i < depthList.ObjectList.Count; i++)
            {
                SpecificationAttribute internalDepth = depthList.ObjectList[i][SpecificationType.JSON.BayIntlDpth];
                SpecificationAttribute internalDepthUom = depthList.ObjectList[i][SpecificationType.JSON.BayIntlDpthUom];
                int intId = int.Parse(depthList.ObjectList[i][SpecificationType.JSON.BayIntlDpthId].Value);

                if (depthId == intId)
                {
                    depth = UnitConverter.ConvertToInches(decimal.Parse(internalDepth.Value), internalDepthUom.Value, ref wasSuccessful);

                    break;
                }
            }

            for (int i = 0; i < widthList.ObjectList.Count; i++)
            {
                SpecificationAttribute internalWidth = widthList.ObjectList[i][SpecificationType.JSON.BayIntlWdth];
                SpecificationAttribute internalWidthUom = widthList.ObjectList[i][SpecificationType.JSON.BayIntlWdthUom];
                int intId = int.Parse(widthList.ObjectList[i][SpecificationType.JSON.BayIntlWdthId].Value);

                if (widthId == intId)
                {
                    width = UnitConverter.ConvertToInches(decimal.Parse(internalWidth.Value), internalWidthUom.Value, ref wasSuccessful);

                    break;
                }
            }

            for (int i = 1; i < mountingPositionDistanceAttribute.Options.Count; i++)
            {
                int id = int.Parse(mountingPositionDistanceAttribute.Options[i].Value);

                if (mountingPositionDistanceId == id)
                {
                    string text = mountingPositionDistanceAttribute.Options[i].Text;
                    string uom = text.Split(' ')[1];

                    mountingPositionDistance = decimal.Parse(text.Split(' ')[0]);

                    mountingPositionDistance = UnitConverter.ConvertToInches(mountingPositionDistance, uom, ref wasSuccessful);

                    break;
                }
            }

            if (width <= 19)
                intWidth = 19;
            else if (width > 19 && width <= 23)
                intWidth = 23;
            else
                intWidth = int.Parse(decimal.Round(width).ToString());

            height = mountingPositionDistance * mountingPositionQuantity;

            stringHeight = height.ToString("F2");
            stringWidth = width.ToString("F2");
            stringDepth = depth.ToString("F2");

            //BI-|| wdth_no || "_||mntng_pos_dist_no*mntng_pos_qty||x||wdth_no||x||dpth_no||_MP||mntng_pos_qty || x || mntng_pos_dist_no || _ || W || S || M
            //BI-19"_134.75x19.00x21.25_MP77x1.75_WSM

            name += intWidth + "\"_" + stringHeight + "x" + stringWidth + "x" + stringDepth + "_MP" + mountingPositionQuantity + "x" + mountingPositionDistance.ToString("F2");

            if (!string.IsNullOrEmpty(wsm))
                name += "_" + wsm;

            return name;
        }

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            long bayInternalId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.SpecificationId);
            long bayInternalDepthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlDpthId);
            long bayInternalWidthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlWdthId);
            int mountingPositionQty = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosQty);
            int mountingPositionDistanceId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosDistId);
            string name = "";// JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string description = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string wallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlWllMnt);
            string straightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlStrghtThru);
            string midPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlMidPln);
            bool isCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool isPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool isDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            if (string.IsNullOrEmpty(wallMountIndicator))
                wallMountIndicator = "N";

            if (string.IsNullOrEmpty(straightThruIndicator))
                straightThruIndicator = "N";

            if (string.IsNullOrEmpty(midPlaneIndicator))
                midPlaneIndicator = "N";

            name = FormSpecificationName(updatedSpecification);

            try
            {
                //DbInterface.StartTransaction();

                bayInternalId = ((BayInternalDbInterface)DbInterface).CreateBayInternalSpecification(name, description, mountingPositionQty, mountingPositionDistanceId, wallMountIndicator, straightThruIndicator,
                    midPlaneIndicator, isCompleted ? "Y" : "N", isPropagated ? "Y" : "N", isDeleted ? "Y" : "N", bayInternalDepthId, bayInternalWidthId);

                //DbInterface.CommitTransaction();

                if (isPropagated)
                    notifyNDS = true;
            }
            catch (Exception ex)
            {
                //DbInterface.RollbackTransaction();

                logger.Error(ex, "Unable to create new Bay Internal specification");

                throw ex;
            }
            //finally
            //{
            //    DbInterface.Dispose();
            //}

            return bayInternalId;
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            long updatedDepthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlDpthId);
            long updatedWidthId = JsonHelper.GetSpecificationLongValue(updatedSpecification, SpecificationType.JSON.BayIntlWdthId);
            int updatedMountingPositionQty = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosQty);
            int updatedMountingPositionDistanceId = JsonHelper.GetSpecificationIntValue(updatedSpecification, SpecificationType.JSON.MntngPosDistId);
            string updatedName = "";// JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Name);
            string updatedDescription = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.Desc);
            string updatedWallMountIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlWllMnt);
            string updatedStraightThruIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlStrghtThru);
            string updatedMidPlaneIndicator = JsonHelper.GetSpecificationStringValue(updatedSpecification, SpecificationType.JSON.BayIntlMidPln);
            bool updatedIsCompleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Cmplt);
            bool updatedIsPropagated = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Prpgtd);
            bool updatedIsDeleted = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Dltd);

            updatedName = FormSpecificationName(updatedSpecification);

            if (InternalDepthId != updatedDepthId || InternalWidthId != updatedWidthId || MountingPositionQuantity != updatedMountingPositionQty || MountingPositionDistanceId != updatedMountingPositionDistanceId ||
                Name != updatedName || Description != updatedDescription || WallMountIndicator != updatedWallMountIndicator || StraightThruIndicator != updatedStraightThruIndicator || MidPlaneIndicator != updatedMidPlaneIndicator || 
                IsCompleted != updatedIsCompleted || IsPropagated != updatedIsPropagated || IsDeleted != updatedIsDeleted)
            {
                ((BayInternalDbInterface)DbInterface).UpdateBayInternalSpecification(SpecificationId, updatedName, updatedDescription, updatedMountingPositionQty, updatedMountingPositionDistanceId, 
                    updatedWallMountIndicator, updatedStraightThruIndicator, updatedMidPlaneIndicator, updatedIsCompleted ? "Y" : "N", updatedIsPropagated ? "Y" : "N", updatedIsDeleted ? "Y" : "N", updatedDepthId, updatedWidthId);

                notifyNDS = true;
            }

            if (!updatedIsPropagated)
                notifyNDS = false;
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
            ((BayInternalDbInterface)DbInterface).UpsertNDSSpecificationId(SpecificationId, ndsId);
        }
    }
}