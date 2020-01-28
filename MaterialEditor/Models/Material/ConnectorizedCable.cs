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
    public class ConnectorizedCable : MaterialRevision
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string featureType = "Connectorized/Set Length";
        private static readonly string materialCategoryType = MaterialType.MTL_CTGRY_PART_MATERIAL_TYPE;
        public static readonly string RME_CNCTRZD_CABL_MTRL_TABLE = "RME_CNCTRZD_CABL_MTRL";
        public static readonly string VAR_LGTH_CABL_MTRL_ID = "VAR_LGTH_CABL_MTRL_ID";
        public static readonly string SET_LGTH_NO = "SET_LGTH_NO";
        public static readonly string SET_LGTH_UOM_ID = "SET_LGTH_UOM_ID";
        public static readonly string CABL_TYP_ID = "CABL_TYP_ID";
        //private string variableLengthCableProductId = string.Empty;
        //private long variableLengthCableMtrlId = 0;
        //private int setLengthUnitOfMeasureId = 0;
        //private int cableTypeId = 0;
        //private int setLength = 0;
        private Dictionary<string, Attribute> attributes = null;
        private Dictionary<string, Attribute> additionalAttributes = null;
        private Dictionary<string, DatabaseDefinition> materialDefinition = null;
        private IMaterialDbInterface dbInterface = null;        
        private StringCollection sapAttributes = null;
        private StringCollection recordOnlyAttributeNames = null;

        public ConnectorizedCable() : base()
        {
        }

        public ConnectorizedCable(long materialItemId) : base(materialItemId)
        {

        }

        public ConnectorizedCable(long materialItemId, long materialId) : base(materialItemId, materialId)
        {
            //attributes = DbInterface.GetMaterialAttributes(materialItemId, MaterialDefinition);
            //attributes = ((ConnectorizedCableDbInterface)DbInterface).Test(this);
        }

        [MaterialJsonProperty(MaterialType.JSON.VarCbl)]
        public string VariableLengthCableProductId
        {
            get;
            set;
        }

        [MaterialJsonProperty(MaterialType.JSON.VarCblId)]
        public long VariableLengthCableMtrlId
        {
            get;
            set;
        }

        [MaterialJsonProperty(true, MaterialType.JSON.SetLgthUom)]
        public int SetLengthUnitOfMeasureId
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

        [MaterialJsonProperty(MaterialType.JSON.SetLgth)]
        public decimal SetLength
        {
            get;
            set;
        }

        [JsonIgnore]
        public override string RevisionTableName
        {
            get
            {
                return "rme_cnctrzd_cabl_mtrl_revsn";
            }
        }

        public override Dictionary<string, Attribute> Attributes
        {
            get
            {
                if(attributes == null)
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
                    dbInterface = new ConnectorizedCableDbInterface();

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

                    materialDefinition.Add(MaterialType.JSON.VarCblId, new DatabaseDefinition(RME_CNCTRZD_CABL_MTRL_TABLE, VAR_LGTH_CABL_MTRL_ID));
                    materialDefinition.Add(MaterialType.JSON.SetLgth, new DatabaseDefinition(RME_CNCTRZD_CABL_MTRL_TABLE, SET_LGTH_NO));
                    materialDefinition.Add(MaterialType.JSON.SetLgthUom, new DatabaseDefinition(RME_CNCTRZD_CABL_MTRL_TABLE, SET_LGTH_UOM_ID));
                    materialDefinition.Add(MaterialType.JSON.CblTypId, new DatabaseDefinition(RME_CNCTRZD_CABL_MTRL_TABLE, CABL_TYP_ID));
                }

                return materialDefinition;
            }
        }

        public override StringCollection SAPOverrideAttributeNames
        {
            get
            {
                if(sapAttributes == null)
                    sapAttributes = new StringCollection();

                sapAttributes.Add(MaterialType.JSON.Apcl);
                sapAttributes.Add(MaterialType.JSON.MtlType);
                sapAttributes.Add(MaterialType.JSON.HzrdInd);
                sapAttributes.Add(MaterialType.JSON.UOM);

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
            long updatedAssociatedCableId = 0;
            long updatedVariableLengthId = 0;
            long updatedSpecificationId = 0;
            int updatedUomId = 0;
            int updatedLaborId = 0;
            int updatedOrderableId = 0;
            int updatedCableTypeId = 0;
            decimal updatedSetLength = 0;
            string updatedRootPartNumber = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.RtPrtNbr).Value;
            string updatedCatalogDescription = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CtlgDesc).Value;
            string updatedRevisionNumber = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.Rvsn).Value;
            string updatedBaseRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.BaseRvsnInd).Value;
            string updatedCurrentRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CurrRvsnInd).Value;
            string updatedRetiredRevisionInd = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.RetRvsnInd).Value;
            string updatedClei = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CLEI).Value;

            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.MfgId).Value, out updatedMfrId);
            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.VarCblId).Value, out updatedAssociatedCableId);
            long.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.VarCblId).Value, out updatedVariableLengthId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.LbrId).Value, out updatedLaborId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.SetLgthUom).Value, out updatedUomId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.OrdblId).Value, out updatedOrderableId);
            int.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.CblTypId).Value, out updatedCableTypeId);
            decimal.TryParse(JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, MaterialType.JSON.SetLgth).Value, out updatedSetLength);

            if (BaseMaterialAttributesChanged(updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId, ref notifyNDS))
                DbInterface.UpdateBaseMaterial(MaterialItemId, MaterialId, updatedRootPartNumber, updatedMfrId, updatedCatalogDescription, updatedLaborId);

            if (RevisionDataChanged(updatedRevisionNumber, updatedBaseRevisionInd, updatedCurrentRevisionInd, updatedRetiredRevisionInd, updatedClei, updatedOrderableId, ref notifyNDS))
                ((ConnectorizedCableDbInterface)DbInterface).UpdateRevisionData(MaterialItemId, updatedRevisionNumber, updatedBaseRevisionInd, updatedCurrentRevisionInd, updatedRetiredRevisionInd, updatedClei, updatedOrderableId);

            if (VariableLengthCableMtrlId != updatedVariableLengthId || SetLength != updatedSetLength || SetLengthUnitOfMeasureId != updatedUomId || CableTypeId != updatedCableTypeId)
            {
                ((ConnectorizedCableDbInterface)DbInterface).UpdateCableData(MaterialId, updatedVariableLengthId, updatedSetLength, updatedUomId, updatedCableTypeId, updatedSpecificationId);

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
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Apcl);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.UOM);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Stts);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.AmpsDrn);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.NumMtgSpcs);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MtgPltSz);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.MxEqpPos);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Gge);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.HzrdInd);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.GgeUnt);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Hght);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Wdth);
                    recordOnlyAttributeNames.Add(MaterialType.JSON.Dpth);
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
            int setLengthUomId = 0;
            int orderableMaterialStatusId = 0;
            int cableTypeId = 0;
            decimal setLength = 0;
            long materialItemId = 0;
            long specId = 0;
            long variableCableId = 0;
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
                setLengthUomId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.SetLgthUom);
                orderableMaterialStatusId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.OrdblId);
                cableTypeId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.CblTypId);
                setLength = JsonHelper.GetDecimalValue(updatedMaterialItem, MaterialType.JSON.SetLgth);
                materialItemId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MaterialItemId);
                specId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId);
                variableCableId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.VarCblId);
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

                if (currentMaterialItem != null && (currentMaterialItem.MaterialCategoryId != materialCategoryId || currentMaterialItem.FeatureTypeId != featureTypeId))
                    isMaterialTypeChange = true;

                DbInterface.StartTransaction();

                if (isMaterialTypeChange && (currentMaterialItem is MinorMaterial || currentMaterialItem is NonRackMountedEquipment))
                    ((MaterialDbInterfaceImpl)DbInterface).UpdateRootPartNumber(currentMaterialItem.MaterialItemId, currentMaterialItem.RootPartNumber + "-X");

                //PROCEDURE CREATE_CNCTRZD_CABL(pMfrId IN mtrl.mfr_id%TYPE, pRtPrtNbr IN mtrl.rt_part_no%TYPE, pCatId IN mtrl.mtrl_cat_id%TYPE,
                //pRoInd IN mtrl.rcrds_only_ind % TYPE, pDsc IN mtrl.mtrl_dsc % TYPE, pCmpltInd IN mtrl.cmplt_ind % TYPE, 
                //pPrpgtInd IN mtrl.prpgt_ind % TYPE, pRetInd IN mtrl.ret_mtrl_ind % TYPE, pLbrId IN mtrl.lbr_id % TYPE, 
                //pFeatTyp IN mtrl.feat_typ_id % TYPE, pSpecnInitInd IN mtrl.specn_init_ind % TYPE, pVarCblMtrlId IN NUMBER, pSetLgthNo IN NUMBER, 
                //pUomId IN NUMBER, pCblTypId IN NUMBER, pSpecId IN NUMBER, pMtlItmId IN NUMBER, 
                //pRevsnNo IN VARCHAR2, pMtrlCd IN VARCHAR2, pBaseInd IN VARCHAR2, pCurrInd IN VARCHAR2, pRvsnRetInd IN VARCHAR2, 
                //pOrdblId IN NUMBER, pClei IN VARCHAR2, oMtrlId OUT NUMBER)

                materialIdArray = ((ConnectorizedCableDbInterface)DbInterface).CreateConnectorizedCable(mfrId, rootPartNumber, materialCategoryId, recordOnly, description,
                    completionInd, propagationInd, retiredInd, laborId, featureTypeId, specificationInitInd, variableCableId, setLength, setLengthUomId, materialItemId, 
                    cableTypeId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId, specId, materialCode, mtrlId);

                if (recordOnly)
                {
                    MaterialItemId = materialIdArray[1];

                    if (isMaterialTypeChange)
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, mtrlId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

                    PersistRecordOnlyUpdates(updatedMaterialItem, cuid, RecordOnlyAttributeNames, ref notifyNDS);
                }
                else
                {
                    if (isMaterialTypeChange)
                        ((MaterialDbInterfaceImpl)DbInterface).ChangeMaterialItemType(currentMaterialItem.MaterialId, mtrlId, materialItemId, currentMaterialItem.MaterialCategoryId, currentMaterialItem.FeatureTypeId, materialCategoryId, featureTypeId);

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
            int orderableMaterialStatusId = 0;
            long materialItemId = 0;
            long specId = 0;
            long mtrlId = 0;
            string revisionNumber = "";
            string baseRevisionInd = "";
            string currentRevisionInd = "";
            string retiredRevisionInd = "";
            string clei = "";
            string materialCode = "";

            try
            {
                orderableMaterialStatusId = JsonHelper.GetIntValue(updatedMaterialItem, MaterialType.JSON.OrdblId);
                materialItemId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MaterialItemId);
                specId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.SpecId);
                mtrlId = JsonHelper.GetLongValue(updatedMaterialItem, MaterialType.JSON.MtrlId);
                revisionNumber = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.Rvsn);
                baseRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.BaseRvsnInd);
                currentRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CurrRvsnInd);
                retiredRevisionInd = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.RetRvsnInd);
                clei = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.CLEI);
                materialCode = JsonHelper.GetStringValue(updatedMaterialItem, MaterialType.JSON.PrdctId);

                if (string.IsNullOrEmpty(baseRevisionInd))
                    baseRevisionInd = "Y";

                if (string.IsNullOrEmpty(currentRevisionInd))
                    currentRevisionInd = "Y";

                if (string.IsNullOrEmpty(retiredRevisionInd))
                    retiredRevisionInd = "N";

                DbInterface.StartTransaction();

                ((ConnectorizedCableDbInterface)DbInterface).CreateRevision(materialItemId, mtrlId, revisionNumber, baseRevisionInd, currentRevisionInd, retiredRevisionInd, clei, orderableMaterialStatusId, materialCode);

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