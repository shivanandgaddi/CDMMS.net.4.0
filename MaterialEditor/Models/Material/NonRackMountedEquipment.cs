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
    public class NonRackMountedEquipment : Material
    {
        private static readonly string materialCategoryType = MaterialType.MTL_CTGRY_PART_MATERIAL_TYPE;
        public static readonly string NON_RME_NON_CABL_MTRL_TABLE = "NON_RME_NON_CABL_MTRL";
        public static readonly string DPTH_NO = "DPTH_NO";
        public static readonly string HGT_NO = "HGT_NO";
        public static readonly string LGTH_NO = "LGTH_NO";
        public static readonly string WDTH_NO = "WDTH_NO";
        public static readonly string DIM_UOM_ID = "DIM_UOM_ID";
        private int featureTypeId = 0;
        private Dictionary<string, DatabaseDefinition> materialDefinition = null;
        private Dictionary<string, Attribute> attributes = null;
        private Dictionary<string, Attribute> additionalAttributes = null;
        private IMaterialDbInterface dbInterface = null;
        private StringCollection sapAttributes = null;
        private StringCollection recordOnlyAttributeNames = null;

        public NonRackMountedEquipment(int featTypId)
        {
            featureTypeId = featTypId;
        }

        public NonRackMountedEquipment(long materialItemId, int featTypId) : base(materialItemId)
        {
            featureTypeId = featTypId;
        }

        public NonRackMountedEquipment(long materialItemId, long materialId, int featTypId) : base(materialItemId, materialId)
        {
            featureTypeId = featTypId;
        }

        [MaterialJsonProperty(MaterialType.JSON.CblInd, true, "")]
        public bool IsCable
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

        [MaterialJsonProperty(MaterialType.JSON.Lgth)]
        public decimal Length
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.SetLgthUom)]
        public long DimensionsUnitOfMeasureId
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.UOM)]
        public long MaterialUnitOfMeasureId
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.CblTypId)]
        public int CableTypeId
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.SpecId)]
        public long SpecificationId
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.SpecNm)]
        public string SpecificationName
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.FtrTyp)]
        public override int FeatureTypeId
        {
            get
            {
                return featureTypeId;
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

        public override IMaterialDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new NonRackMountedEquipmentDbInterface();

                return dbInterface;
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

        [MaterialJsonProperty(MaterialType.JSON.HasRvsns, true, "")]
        public override bool HasRevisions
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [MaterialJsonProperty(MaterialType.JSON.CLEI)]
        public string CLEI
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.OrdblId)]
        public int OrderableMaterialStatusId
        {
            get;
            set;
        }

        public override Dictionary<string, DatabaseDefinition> MaterialDefinition
        {
            get
            {
                if (materialDefinition == null)
                {
                    materialDefinition = base.MaterialDefinition;

                    materialDefinition.Add(MaterialType.JSON.Dpth, new DatabaseDefinition(NON_RME_NON_CABL_MTRL_TABLE, DPTH_NO));
                    materialDefinition.Add(MaterialType.JSON.Hght, new DatabaseDefinition(NON_RME_NON_CABL_MTRL_TABLE, HGT_NO));
                    materialDefinition.Add(MaterialType.JSON.Wdth, new DatabaseDefinition(NON_RME_NON_CABL_MTRL_TABLE, WDTH_NO));
                    materialDefinition.Add(MaterialType.JSON.Wdth, new DatabaseDefinition(NON_RME_NON_CABL_MTRL_TABLE, LGTH_NO));
                    materialDefinition.Add(MaterialType.JSON.UOM, new DatabaseDefinition(NON_RME_NON_CABL_MTRL_TABLE, DIM_UOM_ID));
                }

                return base.MaterialDefinition;
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
            long updatedSpecId = 0;
            int updatedUomId = 0;
            int updatedLaborId = 0;
            int updatedOrderableId = 0;
            int updatedMaterialUomId = 0;
            int updatedFeatureTypeId = 0;
            int updatedCableTypeId = 0;
            decimal updatedHeight = 0;
            decimal updatedDepth = 0;
            decimal updatedWidth = 0;
            decimal updatedLength = 0;
            string updatedRootPartNumber = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.RtPrtNbr).Value;
            string updatedCatalogDescription = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CtlgDesc).Value;
            string updatedClei = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CLEI).Value;
            bool updatedCableIndicator = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CblInd).BoolValue;

            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.MfgId).Value, out updatedMfrId);
            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.SpecId).Value, out updatedSpecId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.LbrId).Value, out updatedLaborId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.SetLgthUom).Value, out updatedUomId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.OrdblId).Value, out updatedOrderableId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.UOM).Value, out updatedMaterialUomId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.FtrTyp).Value, out updatedFeatureTypeId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CblTypId).Value, out updatedCableTypeId);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Hght).Value, out updatedHeight);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Wdth).Value, out updatedWidth);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Dpth).Value, out updatedDepth);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Lgth).Value, out updatedLength);

            if (BaseMaterialAttributesChanged(updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId, ref notifyNDS))
                DbInterface.UpdateBaseMaterial(MaterialItemId, MaterialId, updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId);

            if (IsCable != updatedCableIndicator || OrderableMaterialStatusId != updatedOrderableId || CLEI != updatedClei || MaterialUnitOfMeasureId != updatedMaterialUomId)
                ((NonRackMountedEquipmentDbInterface)DbInterface).UpdateNonRmeMtrl(MaterialId, MaterialCode, updatedOrderableId, updatedClei, updatedMaterialUomId, updatedCableIndicator);

            if (IsCable != updatedCableIndicator)
            {
                if (updatedCableIndicator)
                    ((NonRackMountedEquipmentDbInterface)DbInterface).InsertCableData(MaterialId, CableTypeId, SpecificationId);
                else
                    ((NonRackMountedEquipmentDbInterface)DbInterface).InsertNonCableData(MaterialId, updatedDepth, updatedHeight, updatedWidth, updatedLength, updatedUomId, updatedFeatureTypeId, updatedSpecId);
            }
            else
            {
                if (IsCable)
                {
                    if (CableTypeId != updatedCableTypeId || SpecificationId != updatedSpecId)
                    {
                        ((NonRackMountedEquipmentDbInterface)DbInterface).UpdateCableData(MaterialId, updatedCableTypeId, updatedSpecId);

                        notifyNDS = true;
                    }
                }
                else
                {
                    if (Height != updatedHeight || Depth != updatedDepth || Width != updatedWidth || Length != updatedLength || DimensionsUnitOfMeasureId != updatedUomId || featureTypeId != updatedFeatureTypeId)
                    {
                        ((NonRackMountedEquipmentDbInterface)DbInterface).UpdateNonCableData(MaterialId, updatedDepth, updatedHeight, updatedWidth, updatedLength, updatedUomId, updatedFeatureTypeId, updatedSpecId);

                        notifyNDS = true;
                    }
                }
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
            int uomId = 0;
            int orderableMaterialStatusId = 0;
            int cableTypeId = 0;
            int setLengthUomId = 0;
            long materialItemId = 0;
            long specId = 0;
            long outMaterialItemId = 0;
            decimal depth = 0;
            decimal height = 0;
            decimal width = 0;
            decimal length = 0;
            string clei = "";
            string materialCode = "";
            string rootPartNumber = "";
            string retiredInd = "";
            string description = "";
            string propagationInd = "";
            string completionInd = "";
            string specificationInitInd = "";
            string cuid = "";
            bool recordOnlyPublished = false;
            bool recordOnly = false;
            bool isCable = false;

            try
            {
                mfrId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MfgId);
                materialCategoryId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.MtlCtgry);
                laborId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.LbrId);
                featureTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.FtrTyp);
                uomId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.UOM);
                setLengthUomId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.SetLgthUom);
                orderableMaterialStatusId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.OrdblId);
                cableTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.CblTypId);
                specId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId);
                materialItemId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MaterialItemId);
                depth = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Dpth);
                height = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Hght);
                width = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Wdth);
                length = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.Lgth);
                clei = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CLEI);
                materialCode = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrdctId);
                rootPartNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RtPrtNbr);
                retiredInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetInd);
                description = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CtlgDesc);
                propagationInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrpgtnInd);
                completionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CmpltnInd);
                specificationInitInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.SpecInitInd);
                cuid = (string)updatedMaterialItem.SelectToken("cuid");
                recordOnlyPublished = JsonHelper.GetBoolValue(updatedMaterialItem, MaterialType.JSON.ROPblshd);
                recordOnly = JsonHelper.GetBoolValue(updatedMaterialItem, MaterialType.JSON.RO);

                isCable = featureTypeId == 30 ? true : false;

                if (string.IsNullOrEmpty(rootPartNumber))
                    rootPartNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrtNo);

                if (mfrId == 0)
                    mfrId = GetManufacturerId(JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.Mfg));

                if (laborId == 0)
                    laborId = 995;

                if (string.IsNullOrEmpty(specificationInitInd))
                    specificationInitInd = "N";

                if (string.IsNullOrEmpty(retiredInd))
                    retiredInd = "N";

                if (string.IsNullOrEmpty(completionInd))
                    completionInd = "Y";

                if (string.IsNullOrEmpty(propagationInd))
                    propagationInd = "Y";

                //if (setLengthUomId == 0)
                //    setLengthUomId = ReferenceDbInterface.GetDimensionsUnitOfMeasureId("in");

                DbInterface.StartTransaction();

                //PROCEDURE create_non_rme_mtrl(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pMtrlCd IN VARCHAR2, 
                //pCblInd IN VARCHAR2, pOrdblId IN NUMBER, pClei IN VARCHAR2, pMtrlUomId IN NUMBER, pMtlItmId IN NUMBER)

                outMaterialItemId = ((NonRackMountedEquipmentDbInterface)DbInterface).CreateNonRackMountedEquipment(mfrId, rootPartNumber, materialCategoryId, recordOnly, description, completionInd, propagationInd,
                    retiredInd, laborId, featureTypeId, specificationInitInd, uomId, materialItemId, clei, orderableMaterialStatusId, materialCode, isCable, mtrlId);

                if (isCable)
                    ((NonRackMountedEquipmentDbInterface)DbInterface).InsertCableData(outMaterialItemId, cableTypeId, specId);
                else
                    ((NonRackMountedEquipmentDbInterface)DbInterface).InsertNonCableData(outMaterialItemId, depth, height, width, length, setLengthUomId, featureTypeId, specId);

                if (recordOnly)
                {
                    MaterialItemId = outMaterialItemId;

                    if (currentMaterialItem != null && (currentMaterialItem.MaterialCategoryId != materialCategoryId || currentMaterialItem.FeatureTypeId != featureTypeId))
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, mtrlId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

                    PersistRecordOnlyUpdates(updatedMaterialItem, cuid, RecordOnlyAttributeNames, ref notifyNDS);
                }
                else
                {
                    if (currentMaterialItem != null && (currentMaterialItem.MaterialCategoryId != materialCategoryId || currentMaterialItem.FeatureTypeId != featureTypeId))
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, materialItemId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

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

            return new long[] { outMaterialItemId, outMaterialItemId, 0 };
        }

        public override void CreateRevision(JObject updatedMaterialItem, ref bool notifyNDSSpecification)
        {
            
        }
    }
}