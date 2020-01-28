using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public class Node : MaterialRevision
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string featureType = "Node";
        private static readonly string materialCategoryType = MaterialType.MTL_CTGRY_PART_MATERIAL_TYPE;
        public static readonly string RME_NODE_MTRL_TABLE = "RME_NODE_MTRL";
        public static readonly string HGT_NO = "HGT_NO";
        public static readonly string DPTH_NO = "DPTH_NO";
        public static readonly string DIM_UOM_ID = "DIM_UOM_ID";
        public static readonly string WDTH_NO = "WDTH_NO";
        private Dictionary<string, Attribute> attributes = null;
        private Dictionary<string, Attribute> additionalAttributes = null;
        private Dictionary<string, DatabaseDefinition> materialDefinition = null;
        private IMaterialDbInterface dbInterface = null;
        private StringCollection sapAttributes = null;
        private StringCollection recordOnlyAttributeNames = null;

        public Node() : base()
        {
        }

        public Node(long materialItemId) : base(materialItemId)
        {

        }

        public Node(long materialItemId, long materialId) : base(materialItemId, materialId)
        {

        }

        [MaterialJsonProperty(MaterialType.JSON.Hght)]
        public decimal Height
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.Wdth)]
        public decimal Width
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.Dpth)]
        public decimal Depth
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.DimUom)]
        public int DimensionsUnitOfMeasureId
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.HtDssptn, false)]
        public decimal HeatDissipation
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.HtDssptnUom)]
        public int HeatDissipationUnitOfMeasure
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.Wght, false)]
        public decimal Weight
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.WghtUom)]
        public int WeightUnitOfMeasure
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.HtGntn, false)]
        public decimal PlannedHeatGeneration
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.HtGntnUom)]
        public int PlannedHeatGenerationUnitOfMeasure
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.ElcCurrDrnNrm, false)]
        public decimal NormalElectricalCurrentDrain
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.ElcCurrDrnNrmUom)]
        public int NormalElectricalCurrentDrainUnitOfMeasure
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.ElcCurrDrnMx, false)]
        public decimal MaxElectricalCurrentDrain
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.ElcCurrDrnMxUom)]
        public int MaxElectricalCurrentDrainUnitOfMeasure
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.HvSpec, true, "")]
        public override bool MayHaveSpecifications
        {
            get
            {
                return true;
            }
            set
            {

            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecTyp)]
        public override string SpecificationType
        {
            get
            {
                return "NODE";
            }
        }

        [JsonIgnore]
        public override string RevisionTableName
        {
            get
            {
                return "rme_node_mtrl_revsn";
            }
        }

        public override Dictionary<string, Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = GetAttributes(this);

                if (attributes != null && attributes.ContainsKey(MaterialType.JSON.HasRvsns))
                    attributes[MaterialType.JSON.HasRvsns].IsEditable = true;

                return attributes;
            }
            set
            {
                attributes = value;
            }
        }

        public override IMaterialDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new NodeDbInterface();

                return dbInterface;
            }
        }

        [MaterialJsonProperty(true, MaterialType.JSON.FtrTyp)]
        public override int FeatureTypeId
        {
            get
            {
                return FeatureType.Id(featureType);
            }
        }

        [MaterialJsonProperty(true, MaterialType.JSON.MtlCtgry)]
        public override int MaterialCategoryId
        {
            get
            {
                return MaterialCategory.Id(materialCategoryType);
            }
        }

        public override Dictionary<string, DatabaseDefinition> MaterialDefinition
        {
            get
            {
                if (materialDefinition == null)
                {
                    materialDefinition = base.MaterialDefinition;

                    materialDefinition.Add(MaterialType.JSON.Dpth, new DatabaseDefinition(RME_NODE_MTRL_TABLE, DPTH_NO));
                    materialDefinition.Add(MaterialType.JSON.Hght, new DatabaseDefinition(RME_NODE_MTRL_TABLE, HGT_NO));
                    materialDefinition.Add(MaterialType.JSON.Wdth, new DatabaseDefinition(RME_NODE_MTRL_TABLE, WDTH_NO));
                    materialDefinition.Add(MaterialType.JSON.DimUom, new DatabaseDefinition(RME_NODE_MTRL_TABLE, DIM_UOM_ID));
                }

                return materialDefinition;
            }
        }

        public override StringCollection SAPOverrideAttributeNames
        {
            get
            {
                if (sapAttributes == null)
                    sapAttributes = new StringCollection();

                sapAttributes.Add(MaterialType.JSON.Apcl);
                sapAttributes.Add(MaterialType.JSON.MtlType);
                sapAttributes.Add(MaterialType.JSON.HzrdInd);
                sapAttributes.Add(MaterialType.JSON.HECI);
                //sapAttributes.Add(MaterialType.JSON.UOM);

                return sapAttributes;
            }
        }

        public override Dictionary<string, Attribute> AdditionalAttributeNames
        {
            get
            {
                if (additionalAttributes == null)
                {
                    additionalAttributes = new Dictionary<string, Attribute>();

                    Attribute accountCode = new Attribute(true, MaterialType.JSON.AccntCd);
                    Attribute specName = new Attribute(true, MaterialType.JSON.SpecNm);
                    Attribute status = new Attribute(true, MaterialType.JSON.Stts); //This should be moved up the Material class
                    Attribute locationPositionIndicator = new Attribute(true, MaterialType.JSON.LctnPosInd);
                    Attribute defaultPositionScheme = new Attribute(true, MaterialType.JSON.PosSchm);
                    Attribute equipmentClass = new Attribute(true, MaterialType.JSON.EqptCls);
                    Attribute productType = new Attribute(true, MaterialType.JSON.PrdTyp);
                    Attribute partNumberTypeCode = new Attribute(true, MaterialType.JSON.PrtNbrTypCd);
                    Attribute height = new Attribute(true, MaterialType.JSON.IntrlHght);
                    Attribute width = new Attribute(true, MaterialType.JSON.IntrlWdth);
                    Attribute depth = new Attribute(true, MaterialType.JSON.IntrlDpth);
                    Attribute ampsDrain = new Attribute(true, MaterialType.JSON.AmpsDrn);
                    Attribute numberMountingSpaces = new Attribute(true, MaterialType.JSON.NumMtgSpcs);
                    Attribute firstMountPosition = new Attribute(true, MaterialType.JSON.MntPosHght);
                    Attribute mountingPlateSize = new Attribute(true, MaterialType.JSON.MtgPltSz);
                    Attribute maxEquipmentPositions = new Attribute(true, MaterialType.JSON.MxEqpPos);
                    Attribute gaugeUnit = new Attribute(true, MaterialType.JSON.GgeUnt);
                    Attribute frameName = new Attribute(true, MaterialType.JSON.FrmNm);
                    Attribute equipmentWeight = new Attribute(true, MaterialType.JSON.EqpWght);
                    Attribute voltageDescription = new Attribute(true, MaterialType.JSON.VltDsc);
                    Attribute gauge = new Attribute(true, MaterialType.JSON.Gge);

                    AddAdditionalAttribute(additionalAttributes, accountCode);
                    AddAdditionalAttribute(additionalAttributes, specName);
                    AddAdditionalAttribute(additionalAttributes, status);
                    AddAdditionalAttribute(additionalAttributes, locationPositionIndicator);
                    AddAdditionalAttribute(additionalAttributes, defaultPositionScheme);
                    AddAdditionalAttribute(additionalAttributes, equipmentClass);
                    AddAdditionalAttribute(additionalAttributes, productType);
                    AddAdditionalAttribute(additionalAttributes, partNumberTypeCode);
                    AddAdditionalAttribute(additionalAttributes, height);
                    AddAdditionalAttribute(additionalAttributes, width);
                    AddAdditionalAttribute(additionalAttributes, depth);
                    AddAdditionalAttribute(additionalAttributes, ampsDrain);
                    AddAdditionalAttribute(additionalAttributes, numberMountingSpaces);
                    AddAdditionalAttribute(additionalAttributes, firstMountPosition);
                    AddAdditionalAttribute(additionalAttributes, mountingPlateSize);
                    AddAdditionalAttribute(additionalAttributes, maxEquipmentPositions);
                    AddAdditionalAttribute(additionalAttributes, gaugeUnit);
                    AddAdditionalAttribute(additionalAttributes, frameName);
                    AddAdditionalAttribute(additionalAttributes, equipmentWeight);
                    AddAdditionalAttribute(additionalAttributes, voltageDescription);
                    AddAdditionalAttribute(additionalAttributes, gauge);
                }

                return additionalAttributes;
            }
        }

        public override void PersistUpdates(JObject updatedMaterialItem, string cuid, ref bool notifyNDS, ref bool notifyNDSSpecification)
        {
            long updatedMfrId = 0;
            long specnRevisionId = 0;
            int updatedUomId = 0;
            int updatedLaborId = 0;
            int updatedOrderableId = 0;
            int updatedHeatGenerationUomId = 0;
            int updatedHeatDissipationUom = 0;
            int updatedWeightUom = 0;
            int normalElectricalCurrentDrainUom = 0;
            int maxElectricalCurrentDrainUom = 0;
            decimal updatedHeight = 0;
            decimal updatedDepth = 0;
            decimal updatedWidth = 0;
            decimal updatedHeatGeneration = 0;
            decimal updatedHeatDissipation = 0;
            decimal updatedWeight = 0;
            decimal normalElectricalCurrentDrain = 0;
            decimal maxElectricalCurrentDrain = 0;            
            string updatedRootPartNumber = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.RtPrtNbr).Value;
            string updatedCatalogDescription = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CtlgDesc).Value;
            string updatedRevisionNumber = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Rvsn).Value;
            string updatedBaseRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.BaseRvsnInd).Value;
            string updatedCurrentRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CurrRvsnInd).Value;
            string updatedRetiredRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.RetRvsnInd).Value;
            string updatedClei = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CLEI).Value;
            
            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.MfgId).Value, out updatedMfrId);
            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.SpecRvsnId).Value, out specnRevisionId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.LbrId).Value, out updatedLaborId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.DimUom).Value, out updatedUomId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.OrdblId).Value, out updatedOrderableId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.HtGntnUom).Value, out updatedHeatGenerationUomId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.HtDssptnUom).Value, out updatedHeatDissipationUom);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.WghtUom).Value, out updatedWeightUom);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrmUom).Value, out normalElectricalCurrentDrainUom);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMxUom).Value, out maxElectricalCurrentDrainUom);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Hght).Value, out updatedHeight);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Wdth).Value, out updatedWidth);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Dpth).Value, out updatedDepth);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.HtGntn).Value, out updatedHeatGeneration);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.HtDssptn).Value, out updatedHeatDissipation);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Wght).Value, out updatedWeight);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrm).Value, out normalElectricalCurrentDrain);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMx).Value, out maxElectricalCurrentDrain);

            //public void UpdateRevisionData(long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd,
            //string clei, int orderableMaterialStatusId, decimal plannedHeatGeneration, int plannedHeatGenerationUom, decimal normalDrain, int normalDrainUom,
            //decimal maxDrain, int maxDrainUom, decimal heatDissipation, int heatDissipationUom, decimal weight, int weightUom)

            if (BaseMaterialAttributesChanged(updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId, ref notifyNDS))
                DbInterface.UpdateBaseMaterial(MaterialItemId, MaterialId, updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId);

            if (CatalogDescription != updatedCatalogDescription && SpecificationId > 0)
            {
                notifyNDSSpecification = SpecificationPropagated;

                ((NodeDbInterface)DbInterface).UpdateNodeSpecificationDescription(SpecificationId, string.IsNullOrEmpty(updatedCatalogDescription) ? Attributes["ItmDesc"].Value : updatedCatalogDescription);
            }

            if (RevisionDataChanged(updatedRevisionNumber, updatedBaseRevisionInd, updatedCurrentRevisionInd, updatedRetiredRevisionInd, updatedClei, updatedOrderableId, ref notifyNDS) ||
                PlannedHeatGeneration != updatedHeatGeneration || PlannedHeatGenerationUnitOfMeasure != updatedHeatGenerationUomId ||
                HeatDissipation != updatedHeatDissipation || HeatDissipationUnitOfMeasure != updatedHeatDissipationUom ||
                Weight != updatedWeight || WeightUnitOfMeasure != updatedWeightUom || NormalElectricalCurrentDrain != normalElectricalCurrentDrain || NormalElectricalCurrentDrainUnitOfMeasure != normalElectricalCurrentDrainUom ||
                MaxElectricalCurrentDrain != maxElectricalCurrentDrain || MaxElectricalCurrentDrainUnitOfMeasure != maxElectricalCurrentDrainUom || SpecificationRevisionId != specnRevisionId)
            {
                ((NodeDbInterface)DbInterface).UpdateRevisionData(MaterialItemId, updatedRevisionNumber, updatedBaseRevisionInd, updatedCurrentRevisionInd,
                    updatedRetiredRevisionInd, updatedClei, updatedOrderableId, updatedHeatGeneration, updatedHeatGenerationUomId, 
                    updatedHeatDissipation, updatedHeatDissipationUom, updatedWeight, updatedWeightUom, normalElectricalCurrentDrain, normalElectricalCurrentDrainUom,
                    maxElectricalCurrentDrain, maxElectricalCurrentDrainUom, specnRevisionId);
            }

            if (Height != updatedHeight || Depth != updatedDepth || Width != updatedWidth ||
                DimensionsUnitOfMeasureId != updatedUomId)
            {
                ((NodeDbInterface)DbInterface).UpdateNodeData(MaterialId, updatedDepth, updatedHeight, updatedWidth, updatedUomId);

                notifyNDS = true;
            }

            if (IsRecordOnly)
            {
                PersistRecordOnlyUpdates(updatedMaterialItem, cuid, RecordOnlyAttributeNames, ref notifyNDS);
            }
        }

        [JsonIgnore]
        public override StringCollection RecordOnlyAttributeNames
        {
            get
            {
                if (recordOnlyAttributeNames == null)
                {
                    recordOnlyAttributeNames = new StringCollection();

                    recordOnlyAttributeNames.Add(MaterialType.JSON.SpecNm);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.SetLgth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Apcl);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.UOM);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.SetLgthUom);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Stts);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.AmpsDrn);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.NumMtgSpcs);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MtgPltSz);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MxEqpPos);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Gge);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.HzrdInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.GgeUnt);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.EqpWght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.AccntCd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MtlType);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PrdTyp);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.EqptCls);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LctnPosInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PrtNbrTypCd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LctnPosInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MntPosHght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlHght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlDpth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.IntrlWdth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.PosSchm);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LstUid);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.LstDt);
                    
                }

                return recordOnlyAttributeNames;
            }
        }

        public override long[] PersistObject(IMaterial currentMaterialItem, JObject updatedMaterialItem, long mtrlId, ref bool notifyNDS, ref bool notifyNDSSpecification)
        {
            int mfrId = 0;
            int materialCategoryId = 0;
            int laborId = 0;
            int featureTypeId = 0;
            int dimensionsUomId = 0;
            int heatDissipationUom = 0;
            int heatGenerationUom = 0;
            int orderableMaterialStatusId = 0;
            int weightUom = 0;
            int normalElectricalCurrentDrainUom = 0;
            int maxElectricalCurrentDrainUom = 0;
            decimal depth = 0;
            decimal height = 0;
            decimal width = 0;
            decimal heatDissipation = 0;
            decimal heatGeneration = 0;
            decimal weight = 0;
            decimal normalElectricalCurrentDrain = 0;
            decimal maxElectricalCurrentDrain = 0;
            long specnRevisionAltId = 0;
            long materialItemId = 0;
            long specId = 0;
            long[] materialIdArray = null;
            string revisionNumber = "";
            string baseRevisionInd = "";
            string currentRevisionInd = "";
            string retiredRevisionInd = "";
            string clei = "";
            string materialCode = "";
            string rootPartNumber = "";
            string retiredInd = "";
            string description = "";
            string propagationInd = "";
            string completionInd = "";
            string specificationInitInd = "";
            string cabinetIndicator = "";
            string cuid = "";
            bool recordOnlyPublished = false;
            bool recordOnly = false;
            bool isMaterialTypeChange = false;

            try
            {
                mfrId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MfgId);
                materialCategoryId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MtlCtgry);
                laborId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.LbrId);
                featureTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.FtrTyp);
                dimensionsUomId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.DimUom);
                heatDissipationUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.HtDssptnUom);
                heatGenerationUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.HtGntnUom);
                orderableMaterialStatusId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.OrdblId);
                weightUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.WghtUom);
                normalElectricalCurrentDrainUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrmUom);
                maxElectricalCurrentDrainUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMxUom);
                depth = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Dpth);
                height = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Hght);
                width = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Wdth);
                heatDissipation = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.HtDssptn);
                heatGeneration = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.HtGntn);
                weight = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Wght);
                normalElectricalCurrentDrain = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrm);
                maxElectricalCurrentDrain = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMx);
                materialItemId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MaterialItemId);
                specId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId);
                specnRevisionAltId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecRvsnId);
                revisionNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.Rvsn);
                baseRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.BaseRvsnInd);
                currentRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CurrRvsnInd);
                retiredRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetRvsnInd);
                clei = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CLEI);
                materialCode = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrdctId);
                rootPartNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RtPrtNbr);
                retiredInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetInd);
                description = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CtlgDesc);
                propagationInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrpgtnInd);
                completionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CmpltnInd);
                specificationInitInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.SpecInitInd);
                cuid = (string)updatedMaterialItem.SelectToken("cuid");
                cabinetIndicator = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CabInd);
                recordOnlyPublished = JsonHelper.GetBoolValue(updatedMaterialItem, MaterialType.JSON.ROPblshd);
                recordOnly = JsonHelper.GetBoolValue(updatedMaterialItem, MaterialType.JSON.RO);

                if (string.IsNullOrEmpty(rootPartNumber))
                    rootPartNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrtNo);

                if (mfrId == 0)
                    mfrId = GetManufacturerId(JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.Mfg));

                if (laborId == 0)
                    laborId = 995;

                if (string.IsNullOrEmpty(baseRevisionInd))
                    baseRevisionInd = "Y";

                if (string.IsNullOrEmpty(currentRevisionInd))
                    currentRevisionInd = "Y";

                if (string.IsNullOrEmpty(retiredRevisionInd))
                    retiredRevisionInd = "N";

                if (string.IsNullOrEmpty(specificationInitInd))
                    specificationInitInd = "N";

                if (string.IsNullOrEmpty(retiredInd))
                    retiredInd = "N";

                if (string.IsNullOrEmpty(completionInd))
                    completionInd = "Y";

                if (string.IsNullOrEmpty(propagationInd))
                    propagationInd = "Y";

                if (string.IsNullOrEmpty(cabinetIndicator))
                    cabinetIndicator = "N";

                if (dimensionsUomId == 0)
                    dimensionsUomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

                if (currentMaterialItem != null && (currentMaterialItem.MaterialCategoryId != materialCategoryId || currentMaterialItem.FeatureTypeId != featureTypeId))
                    isMaterialTypeChange = true;

                DbInterface.StartTransaction();

                if (isMaterialTypeChange && (currentMaterialItem is MinorMaterial || currentMaterialItem is NonRackMountedEquipment))
                    ((MaterialDbInterfaceImpl)DbInterface).UpdateRootPartNumber(currentMaterialItem.MaterialItemId, currentMaterialItem.RootPartNumber + "-X");

                //CreateNode(int mfrId, string rootPartNumber, int materialCategoryId, bool recordOnly, string description, string completionInd, string propagationInd,
                //string retiredInd, int laborId, int featureTypeId, string specificationInitInd, decimal depth, decimal height, decimal width, int dimensionsUomId, 
                //long materialItemId, string revisionNumber, string baseRevisionInd, string currentRevisionInd, string retiredRevisionInd, string clei, int orderableMaterialStatusId, long specId,
                //decimal plannedHeatGeneration, int plannedHeatGenerationUomId, decimal weight, int weightUomId, decimal heatDissipation, int heatDissipationUomId, string materialCode)

                materialIdArray = ((NodeDbInterface)DbInterface).CreateNode(mfrId, rootPartNumber, materialCategoryId, recordOnly, description, completionInd, propagationInd, retiredInd, laborId, 
                    featureTypeId, specificationInitInd, depth, height, width, dimensionsUomId, materialItemId, revisionNumber, baseRevisionInd, currentRevisionInd,
                    retiredRevisionInd, clei, orderableMaterialStatusId, specId, heatGeneration, heatGenerationUom, weight, weightUom, heatDissipation, heatDissipationUom, materialCode, mtrlId,
                    normalElectricalCurrentDrain, normalElectricalCurrentDrainUom, MaxElectricalCurrentDrain, maxElectricalCurrentDrainUom, specnRevisionAltId);

                if (recordOnly)
                {
                    MaterialItemId = materialIdArray[1];

                    if (isMaterialTypeChange)
                    {
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, mtrlId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

                        if (currentMaterialItem.SpecificationId > 0)
                        {
                            ((NodeDbInterface)DbInterface).ChangeSpecificationType(currentMaterialItem.SpecificationId, currentMaterialItem.SpecificationRevisionId, currentMaterialItem.FeatureTypeId, featureTypeId);
                            ((NodeDbInterface)DbInterface).AssociateMaterial(currentMaterialItem.SpecificationRevisionId, materialItemId);

                            notifyNDSSpecification = currentMaterialItem.SpecificationPropagated;
                        }
                    }

                    PersistRecordOnlyUpdates(updatedMaterialItem, cuid, RecordOnlyAttributeNames, ref notifyNDS);
                }
                else
                {
                    if (isMaterialTypeChange)
                    {
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, mtrlId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

                        if (currentMaterialItem.SpecificationId > 0)
                        {
                            ((NodeDbInterface)DbInterface).ChangeSpecificationType(currentMaterialItem.SpecificationId, currentMaterialItem.SpecificationRevisionId, currentMaterialItem.FeatureTypeId, featureTypeId);
                            ((NodeDbInterface)DbInterface).AssociateMaterial(currentMaterialItem.SpecificationRevisionId, materialItemId);

                            notifyNDSSpecification = currentMaterialItem.SpecificationPropagated;
                        }
                    }

                    UpdateLastCuidAndDate(cuid, updatedMaterialItem);
                    PersistSAPUpdates(updatedMaterialItem, cuid, SAPOverrideAttributeNames, ref notifyNDS);
                    PersistAdditionalAttributeUpdates(updatedMaterialItem, cuid, AdditionalAttributeNames, ref notifyNDS);
                }

                DbInterface.CommitTransaction();

                notifyNDS = true;
            }
            catch (Exception ex)
            {
                DbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                DbInterface.Dispose();
            }

            return materialIdArray;
        }

        public override void CreateRevision(JObject updatedMaterialItem, ref bool notifyNDSSpecification)
        {
            int heatDissipationUom = 0;
            int heatGenerationUom = 0;
            int orderableMaterialStatusId = 0;
            int weightUom = 0;
            int normalElectricalCurrentDrainUom = 0;
            int maxElectricalCurrentDrainUom = 0;
            decimal heatDissipation = 0;
            decimal heatGeneration = 0;
            decimal weight = 0;
            decimal normalElectricalCurrentDrain = 0;
            decimal maxElectricalCurrentDrain = 0;
            long specnRevisionAltId = 0;
            long materialItemId = 0;
            long mtrlId = 0;
            long specId = 0;
            string revisionNumber = "";
            string baseRevisionInd = "";
            string currentRevisionInd = "";
            string retiredRevisionInd = "";
            string clei = "";
            string materialCode = "";
            string retiredInd = "";

            try
            {
                heatDissipationUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.HtDssptnUom);
                heatGenerationUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.HtGntnUom);
                orderableMaterialStatusId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.OrdblId);
                weightUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.WghtUom);
                normalElectricalCurrentDrainUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrmUom);
                maxElectricalCurrentDrainUom = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMxUom);
                heatDissipation = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.HtDssptn);
                heatGeneration = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.HtGntn);
                weight = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Wght);
                normalElectricalCurrentDrain = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnNrm);
                maxElectricalCurrentDrain = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.ElcCurrDrnMx);
                materialItemId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MaterialItemId);
                mtrlId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MtrlId);
                specId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId);
                specnRevisionAltId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.RvsnSpecId);
                revisionNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.Rvsn);
                baseRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.BaseRvsnInd);
                currentRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CurrRvsnInd);
                retiredRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetRvsnInd);
                clei = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CLEI);
                materialCode = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrdctId);
                retiredInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetInd);

                if (string.IsNullOrEmpty(baseRevisionInd))
                    baseRevisionInd = "Y";

                if (string.IsNullOrEmpty(currentRevisionInd))
                    currentRevisionInd = "Y";

                if (string.IsNullOrEmpty(retiredRevisionInd))
                    retiredRevisionInd = "N";

                if (string.IsNullOrEmpty(retiredInd))
                    retiredInd = "N";

                ((NodeDbInterface)DbInterface).CreateRevision(materialItemId, mtrlId, revisionNumber, baseRevisionInd, currentRevisionInd,
                    retiredRevisionInd, clei, orderableMaterialStatusId, specId, heatGeneration, heatGenerationUom, weight, weightUom, heatDissipation, heatDissipationUom, materialCode,
                    normalElectricalCurrentDrain, normalElectricalCurrentDrainUom, maxElectricalCurrentDrain, maxElectricalCurrentDrainUom, specnRevisionAltId);

                DbInterface.CommitTransaction();

                if(specId > 0)
                    notifyNDSSpecification = true;
            }
            catch (Exception ex)
            {
                DbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                DbInterface.Dispose();
            }
        }
    }
}