using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public abstract class Material : IMaterial
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private enum DATABASE_ACTION { NONE, INSERT, UPDATE, DELETE };
        private long materialItemId = 0;
        private long materialId = 0;
        //private long materialCategoryId = 0;
        private long manufacturerId = 0;
        private long specificationId = 0;
        private long specificationRevisionId = 0;
        private long replacesMaterialItemId = 0;
        private long replacedByMaterialItemId = 0;
        private int laborId = 0;
        private const string Int32 = "Int32";
        private const string Int64 = "Int64";
        private const string Decimal = "Decimal";
        private const string Double = "Double";
        private const string DateTime = "DateTime";
        private const string Zero = "0";
        public static readonly string MTRL_TABLE = "MTRL";
        public static readonly string MTRL_ID = "MTRL_ID";
        public static readonly string MFR_ID = "MFR_ID";
        public static readonly string PART_NO = "PART_NO";
        public static readonly string MTRL_DSC = "MTRL_DSC";
        public static readonly string MTRL_CAT_ID = "MTRL_CAT_ID";
        public static readonly string FEAT_TYP_ID = "FEAT_TYP_ID";
        public static readonly string LBR_ID = "LBR_ID";
        private string rootPartNumber = string.Empty;
        private string catalogDescription = string.Empty;
        private string manufacturer = string.Empty;
        private string manufacturerName = string.Empty;

        private string materialCode = string.Empty;
        private string specificationName = string.Empty;
        private string specificationRevisionName = string.Empty;
        private string replacesMaterialPartNumber = string.Empty;
        private string replacedByMaterialPartNumber = string.Empty;
        private Dictionary<string, DatabaseDefinition> materialDefinition = null;
        private Dictionary<string, Attribute> attributes = null;

        public Material()
        {
        }

        public Material(long materialItemId)
        {
            this.materialItemId = materialItemId;
        }

        public Material(long materialItemId, long materialId)
        {
            this.materialItemId = materialItemId;
            this.materialId = materialId;
        }

        //public Material(long materialItemId, long materialId, long materialCategoryId)
        //{
        //    this.materialItemId = materialItemId;
        //    this.materialId = materialId;
        //    this.materialCategoryId = materialCategoryId;
        //}

        //public Material(long materialItemId, long materialId, long materialCategoryId, long featureTypeId)
        //{
        //    this.materialItemId = materialItemId;
        //    this.materialId = materialId;
        //    this.materialCategoryId = materialCategoryId;
        //    this.featureTypeId = featureTypeId;
        //}

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.MaterialItemId)]
        public long MaterialItemId
        {
            get
            {
                return materialItemId;
            }
            set
            {
                materialItemId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.MtrlId)]
        public long MaterialId
        {
            get
            {
                return materialId;
            }
            set
            {
                materialId = value;
            }
        }

        //public long MaterialCategoryId
        //{
        //    get
        //    {
        //        return materialCategoryId;
        //    }
        //    set
        //    {
        //        materialCategoryId = value;
        //    }
        //}

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.MfgId)]
        public long ManufacturerId
        {
            get
            {
                return manufacturerId;
            }
            set
            {
                manufacturerId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.PrdctId)]
        public string MaterialCode
        {
            get
            {
                return materialCode;
            }
            set
            {
                materialCode = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.Mfg)]
        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
            set
            {
                manufacturer = value;
            }
        }
        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.MfgNm)]
        public string ManufacturerName
        {
            get
            {
                return manufacturerName;
            }
            set
            {
                manufacturerName = value;
            }
        }

        //public long FeatureTypeId
        //{
        //    get
        //    {
        //        return featureTypeId;
        //    }
        //    set
        //    {
        //        featureTypeId = value;
        //    }
        //}

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.LbrId, false, true)]
        public int LaborId
        {
            get
            {
                return laborId;
            }
            set
            {
                laborId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RtPrtNbr)]
        public string RootPartNumber
        {
            get
            {
                return rootPartNumber;
            }
            set
            {
                rootPartNumber = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.CtlgDesc)]
        public string CatalogDescription
        {
            get
            {
                return catalogDescription;
            }
            set
            {
                catalogDescription = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RplcsPrtNbr)]
        public string ReplacesMaterialPartNumber
        {
            get
            {
                return replacesMaterialPartNumber;
            }
            set
            {
                replacesMaterialPartNumber = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RplcsMtlItmId)]
        public long ReplacesMaterialItemId
        {
            get
            {
                return replacesMaterialItemId;
            }
            set
            {
                replacesMaterialItemId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecNm)]
        public string SpecificationName
        {
            get
            {
                return specificationName;
            }
            set
            {
                specificationName = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecRvsnNm)]
        public string SpecificationRevisionName
        {
            get
            {
                return specificationRevisionName;
            }
            set
            {
                specificationRevisionName = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecId)]
        public long SpecificationId
        {
            get
            {
                return specificationId;
            }
            set
            {
                specificationId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecPrpgt)]
        public bool SpecificationPropagated
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecRvsnId)]
        public long SpecificationRevisionId
        {
            get
            {
                return specificationRevisionId;
            }
            set
            {
                specificationRevisionId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RplcdByPrtNbr)]
        public string ReplacedByMaterialPartNumber
        {
            get
            {
                return replacedByMaterialPartNumber;
            }
            set
            {
                replacedByMaterialPartNumber = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RplcdByMtlItmId)]
        public long ReplacedByMaterialItemId
        {
            get
            {
                return replacedByMaterialItemId;
            }
            set
            {
                replacedByMaterialItemId = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.RO, true, "")]
        public bool IsRecordOnly
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.ROPblshd, true, "")]
        public bool IsRecordOnlyPublished
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.HvSpec, true, "")]
        public virtual bool MayHaveSpecifications
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public abstract bool HasRevisions
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(MaterialType.JSON.SpecTyp)]
        public virtual string SpecificationType
        {
            get
            {
                return "NA";
            }
        }

        protected Dictionary<string, Attribute> GetAttributes(IMaterial material)
        {
            Dictionary<string, Attribute> attributes = null;

            if (material != null)
            {
                Type materialType = material.GetType();
                ReferenceDbInterface dbInterface = null;

                foreach (PropertyInfo p in materialType.GetProperties())
                {
                    MaterialJsonProperty jsonProperty = (MaterialJsonProperty)p.GetCustomAttribute(typeof(MaterialJsonProperty));

                    if (jsonProperty != null)
                    {
                        string value = string.Empty;

                        try
                        {
                            value = p.GetValue(material).ToString();
                        }
                        catch (Exception ex)
                        {
                        }

                        if (!string.IsNullOrEmpty(value) || jsonProperty.HasOptionValues)
                        {
                            Attribute attr = new Attribute();

                            attr.Name = jsonProperty.Name;

                            if (MaterialType.JSON.SetLgth.Equals(jsonProperty.Name)) //Think of a better way instead of hardcoding this
                                attr.IspOrOsp = "I";

                            if (attributes == null)
                                attributes = new Dictionary<string, Attribute>();

                            if (jsonProperty.HasOptionValues)
                            {
                                if (dbInterface == null)
                                    dbInterface = new ReferenceDbInterface();

                                Task t = Task.Run(async () =>
                                {
                                    NameValueCollection parameters = null;

                                    if (jsonProperty.Name.Equals(MaterialType.JSON.LbrId))
                                    {
                                        parameters = new NameValueCollection();

                                        parameters.Add("Id", material.MaterialItemId.ToString());
                                        parameters.Add("retcsr", ReferenceDbInterface.CURSOR);
                                    }

                                    attr.Options = await dbInterface.GetListOptionsForAttribute(jsonProperty.Name, parameters);
                                });

                                t.Wait();

                                if (string.IsNullOrEmpty(value) || "0".Equals(value))
                                {
                                    if (attr.Options != null)
                                    {
                                        for (int i = 1; i < attr.Options.Count; i++)
                                        {
                                            if ("Y".Equals(attr.Options[i].DefaultValue))
                                            {
                                                value = attr.Options[i].Value;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if ("Boolean".Equals(p.PropertyType.Name))
                            {
                                bool bValue = false;

                                if (!Boolean.TryParse(value, out bValue))
                                    logger.Info("Unable to parse [{0}] as boolean. MaterialItemId: {1}", value, material.MaterialItemId);

                                attr.BoolValue = bValue;

                                if (!attributes.ContainsKey(jsonProperty.Name))
                                    attributes.Add(jsonProperty.Name, attr);
                            }
                            else
                            {
                                if (Zero.Equals(value))
                                {
                                    string typeName = p.PropertyType.Name;

                                    if (!jsonProperty.HasOptionValues && (jsonProperty.IgnoreNumericZeroValues && (Int32.Equals(typeName) || Int64.Equals(typeName) || Decimal.Equals(typeName) || Double.Equals(typeName))))
                                    {
                                        //skip it
                                    }
                                    else
                                    {
                                        attr.Value = value;

                                        if (!attributes.ContainsKey(jsonProperty.Name))
                                            attributes.Add(jsonProperty.Name, attr);
                                    }
                                }
                                else
                                {
                                    attr.Value = value;

                                    if(!attributes.ContainsKey(jsonProperty.Name))
                                        attributes.Add(jsonProperty.Name, attr);
                                }
                            }
                        }
                    }
                }
            }

            return attributes;
        }

        public virtual void PersistMaterialUpdates(JObject updatedMaterialItem, ref bool notifyNDS, ref bool notifyNDSSpecification)
        {
            try
            {
                string cuid = (string)updatedMaterialItem.SelectToken("cuid");

                updatedMaterialItem["LstUid"]["value"] = cuid;
                updatedMaterialItem["LstDt"]["value"] = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                DbInterface.StartTransaction();

                PersistUpdates(updatedMaterialItem, cuid, ref notifyNDS, ref notifyNDSSpecification);

                if (!IsRecordOnly)
                {                    
                    PersistSAPUpdates(updatedMaterialItem, cuid, SAPOverrideAttributeNames, ref notifyNDS);
                    PersistAdditionalAttributeUpdates(updatedMaterialItem, cuid, AdditionalAttributeNames, ref notifyNDS);
                    UpdateLastCuidAndDate(cuid);
                }

                DbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                DbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (DbInterface != null)
                    DbInterface.Dispose();
            }
        }

        public void UpdateLastCuidAndDate(string cuid)
        {
            long lastCuidId = 0;
            long lastDateId = 0;

            long.TryParse(Attributes["LstUid"].MaterialItemAttributesDefId, out lastCuidId);
            long.TryParse(Attributes["LstDt"].MaterialItemAttributesDefId, out lastDateId);

            if (lastCuidId > 0 && lastDateId > 0)
            {
                DbInterface.UpdateMaterialItemAttributes(cuid, materialItemId, lastCuidId, cuid);
                DbInterface.UpdateMaterialItemAttributes(System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), materialItemId, lastDateId, cuid);
            }
        }
        
        public void UpdateLastCuidAndDate(string cuid, JObject updatedMaterialItem)
        {
            long lastCuidId = 0;
            long lastDateId = 0;

            long.TryParse(JsonHelper.DeserializeAttribute(updatedMaterialItem, "LstUid").MaterialItemAttributesDefId, out lastCuidId);
            long.TryParse(JsonHelper.DeserializeAttribute(updatedMaterialItem, "LstDt").MaterialItemAttributesDefId, out lastDateId);

            if (lastCuidId > 0 && lastDateId > 0)
            {
                DbInterface.UpdateMaterialItemAttributes(cuid, materialItemId, lastCuidId, cuid);
                DbInterface.UpdateMaterialItemAttributes(System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), materialItemId, lastDateId, cuid);
            }
        }

        [JsonIgnore]
        public virtual Dictionary<string, DatabaseDefinition> MaterialDefinition
        {
            get
            {
                if (materialDefinition == null)
                {
                    materialDefinition = new Dictionary<string, DatabaseDefinition>();

                    materialDefinition.Add(MaterialType.JSON.MtrlId, new DatabaseDefinition(MTRL_TABLE, MTRL_ID));
                    materialDefinition.Add(MaterialType.JSON.Mfg, new DatabaseDefinition(MTRL_TABLE, MFR_ID));
                    materialDefinition.Add(MaterialType.JSON.PrtNo, new DatabaseDefinition(MTRL_TABLE, PART_NO));
                    materialDefinition.Add(MaterialType.JSON.CtlgDesc, new DatabaseDefinition(MTRL_TABLE, MTRL_DSC));
                    materialDefinition.Add(MaterialType.JSON.MtlCtgry, new DatabaseDefinition(MTRL_TABLE, MTRL_CAT_ID));
                    materialDefinition.Add(MaterialType.JSON.FtrTyp, new DatabaseDefinition(MTRL_TABLE, FEAT_TYP_ID));
                    materialDefinition.Add(MaterialType.JSON.LbrId, new DatabaseDefinition(MTRL_TABLE, LBR_ID));
                }

                return materialDefinition;
            }
        }

        public void AddAdditionalAttribute(Dictionary<string, Attribute> attributes, Attribute attribute)
        {
            try
            {
                if (attributes == null)
                    attributes = new Dictionary<string, Attribute>();

                attributes.Add(attribute.Name, attribute);
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }

        public abstract IMaterialDbInterface DbInterface
        {
            get;
        }

        public abstract Dictionary<string, Attribute> Attributes
        {
            get;
            //{
            //    if (attributes == null)
            //        attributes = new Dictionary<string, Attribute>();
            //    else
            //        attributes.Clear();



            //    return attributes;
            //}
            set;
            //{
            //    attributes = value;
            //}
        }

        //public abstract Dictionary<string, DatabaseDefinition> MaterialDefinition
        //{
        //    get;
        //}

        public abstract int MaterialCategoryId
        {
            get;
        }

        public abstract int FeatureTypeId
        {
            get;
        }
        public abstract StringCollection SAPOverrideAttributeNames
        {
            get;
        }
        public abstract Dictionary<string, Attribute> AdditionalAttributeNames
        {
            get;
        }

        public abstract StringCollection RecordOnlyAttributeNames
        {
            get;
        }

        public abstract void PersistUpdates(JObject updatedMaterialItem, string cuid, ref bool notifyNDS, ref bool notifyNDSSpecification);

        public abstract long[] PersistObject(IMaterial currentMaterialItem, JObject updatedMaterialItem, long mtrlId, ref bool notifyNDS, ref bool notifyNDSSpecification);

        public abstract void CreateRevision(JObject updatedMaterialItem, ref bool notifyNDSSpecification);

        public void PersistSAPUpdates(JObject updatedMaterialItem, string cuid, StringCollection attributeNameCollection, ref bool notifyNDS)
        {
            if (attributeNameCollection != null)
            {
                foreach (string name in attributeNameCollection)
                {
                    //string key = keyValue.Key;
                    Models.Attribute updatedAttribute = JsonHelper.DeserializeAttribute(updatedMaterialItem, name);
                    DATABASE_ACTION action = DATABASE_ACTION.NONE;

                    if (updatedAttribute != null)
                    {
                        if (!MaterialType.JSON.PrdctId.Equals(name))
                        {
                            if (MaterialType.JSON.HzrdInd.Equals(name)) //A true value from the GUI (i.e. checkbox is checked) equals 'X' in the database
                            {
                                action = DetermineDatabaseAction(updatedAttribute.BoolValue.ToString(), updatedAttribute.Source, name, true);

                                if (action == DATABASE_ACTION.INSERT || action == DATABASE_ACTION.UPDATE)
                                {
                                    if (updatedAttribute.BoolValue)
                                        updatedAttribute.Value = "X";
                                    else
                                        updatedAttribute.Value = "";
                                }
                            }
                            else
                                action = DetermineDatabaseAction(updatedAttribute.Value, updatedAttribute.Source, name, false);

                            switch (action)
                            {
                                case DATABASE_ACTION.DELETE:
                                    DbInterface.DeleteMaterialItemAttributes(materialItemId, ConvertStringToLong(updatedAttribute.MaterialItemAttributesDefId));
                                    notifyNDS = true;

                                    break;
                                case DATABASE_ACTION.INSERT:
                                    DbInterface.InsertMaterialItemAttributes(updatedAttribute.Value, materialItemId, updatedAttribute.Name, cuid);
                                    notifyNDS = true;

                                    break;
                                case DATABASE_ACTION.UPDATE:
                                    DbInterface.UpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, ConvertStringToLong(updatedAttribute.MaterialItemAttributesDefId), cuid);
                                    notifyNDS = true;

                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void PersistRecordOnlyUpdates(JObject updatedMaterialItem, string cuid, StringCollection attributeNameCollection, ref bool notifyNDS)
        {
            if (attributeNameCollection != null)
            {
                foreach (string name in attributeNameCollection)
                {
                    Models.Attribute updatedAttribute = JsonHelper.DeserializeAttribute(updatedMaterialItem, name);

                    if (updatedAttribute != null)
                    {
                        if (!MaterialType.JSON.PrdctId.Equals(name))
                        {
                            if (MaterialType.JSON.HzrdInd.Equals(name)) //A true value from the GUI (i.e. checkbox is checked) equals 'X' in the database
                                DbInterface.InsertUpdateMaterialItemAttributes(updatedAttribute.BoolValue ? "Y" : "N", materialItemId, updatedAttribute.Name, cuid);
                            else
                            {
                                if (MaterialType.JSON.LstUid.Equals(name))
                                    updatedAttribute.Value = cuid;
                                else if (MaterialType.JSON.LstDt.Equals(name))
                                    updatedAttribute.Value = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                                    DbInterface.InsertUpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, updatedAttribute.Name, cuid);
                            }
                        }
                    }
                }
            }
        }

        public void PersistAdditionalAttributeUpdates(JObject updatedMaterialItem, string cuid, Dictionary<string, Attribute> attributeNameCollection, ref bool notifyNDS)
        {
            if (attributeNameCollection != null)
            {
                foreach (string name in attributeNameCollection.Keys)
                {
                    Models.Attribute updatedAttribute = JsonHelper.DeserializeAttributeNotNull(updatedMaterialItem, name);
                    DATABASE_ACTION action = DATABASE_ACTION.NONE;

                    if (("Bay".Equals(this.GetType().Name) || "BayExtender".Equals(this.GetType().Name))
                        && (name.Equals(MaterialType.JSON.IntrlDpth) || name.Equals(MaterialType.JSON.IntrlHght) || name.Equals(MaterialType.JSON.IntrlWdth)))
                    {
                        //Skip. Values are set on the specification screen.
                    }
                    else
                    {
                        if (Attributes.ContainsKey(name))
                        {
                            action = DetermineDatabaseAction(Attributes[name].Value, updatedAttribute.Value, Attributes[name].MaterialItemAttributesDefId, updatedAttribute.MaterialItemAttributesDefId, name);

                            switch (action)
                            {
                                case DATABASE_ACTION.DELETE:
                                    DbInterface.DeleteMaterialItemAttributes(materialItemId, ConvertStringToLong("0".Equals(updatedAttribute.MaterialItemAttributesDefId) ? Attributes[name].MaterialItemAttributesDefId : updatedAttribute.MaterialItemAttributesDefId));

                                    if (Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                        notifyNDS = true;

                                    break;
                                case DATABASE_ACTION.INSERT:
                                    DbInterface.InsertMaterialItemAttributes(updatedAttribute.Value, materialItemId, updatedAttribute.Name, cuid);

                                    if (Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                        notifyNDS = true;

                                    break;
                                case DATABASE_ACTION.UPDATE:
                                    DbInterface.UpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, ConvertStringToLong("0".Equals(updatedAttribute.MaterialItemAttributesDefId) ? Attributes[name].MaterialItemAttributesDefId : updatedAttribute.MaterialItemAttributesDefId), cuid);

                                    if (Attributes[name].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.NDS))
                                        notifyNDS = true;

                                    break;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(updatedAttribute.Value))
                                DbInterface.InsertUpdateMaterialItemAttributes(updatedAttribute.Value, materialItemId, name, cuid);
                        }
                    }
                }
            }
        }

        private DATABASE_ACTION DetermineDatabaseAction(string updatedValue, string updatedSource, string attributeName, bool checkBoolValue)
        {
            DATABASE_ACTION action = DATABASE_ACTION.NONE;
            string currentValue = "";

            if (Attributes.ContainsKey(attributeName))
            {
                if (checkBoolValue)
                    currentValue = Attributes[attributeName].BoolValue.ToString();
                else
                    currentValue = Attributes[attributeName].Value;

                if (currentValue == updatedValue)
                {
                    if (Attributes[attributeName].Source == MaterialType.SourceSystem(SOURCE_SYSTEM.SAP) && updatedSource == MaterialType.JSON.OVERRIDE)
                        action = DATABASE_ACTION.DELETE;
                }
                else
                {
                    if (updatedSource == MaterialType.JSON.OVERRIDE)
                    {
                        if (string.IsNullOrEmpty(updatedValue) || "0".Equals(updatedValue))
                            action = DATABASE_ACTION.DELETE; //The override value was cleared/deleted from the GUI. Assume the user wants to revert back to the SAP value.
                        else
                            action = DATABASE_ACTION.UPDATE;
                    }
                    else
                        action = DATABASE_ACTION.INSERT;
                }
            }
            else if (IsRecordOnly)
            {
            }

            return action;
        }

        private DATABASE_ACTION DetermineDatabaseAction(string currentValue, string updatedValue, string currentDefId, string updatedDefId, string attributeName)
        {
            DATABASE_ACTION action = DATABASE_ACTION.NONE;

            if (currentValue != updatedValue)
            {
                if (string.IsNullOrEmpty(updatedValue) || ("0".Equals(updatedValue) && !"Stts".Equals(attributeName)))
                    action = DATABASE_ACTION.DELETE;
                else
                {
                    if (ConvertStringToLong(currentDefId) > 0 || ConvertStringToLong(updatedDefId) > 0)
                        action = DATABASE_ACTION.UPDATE;
                    else
                        action = DATABASE_ACTION.INSERT;
                }
            }

            return action;
        }

        private long ConvertStringToLong(string stringToConvert)
        {
            long convertedLong = long.MinValue;
            bool conversionSuccessful = long.TryParse(stringToConvert, out convertedLong);

            if (!conversionSuccessful)
                throw new Exception(string.Format("Unable to convert [{0}] to long.", stringToConvert));

            return convertedLong;
        }

        protected bool BaseMaterialAttributesChanged(string updatedRootPartNumber, long updatedMfrId, string updatedCatalogDescription, int updatedLaborId, ref bool notifyNDS)
        {
            bool didChange = false;

            if (!rootPartNumber.Equals(updatedRootPartNumber))
                didChange = true;

            if (manufacturerId != updatedMfrId)
                didChange = true;

            if (laborId != updatedLaborId)
                didChange = true;

            if (!catalogDescription.Equals(updatedCatalogDescription))
                didChange = true;

            if (didChange)
                notifyNDS = true;

            return didChange;
        }

        protected int GetManufacturerId(string manufacturerCode)
        {
            int mfrId = ReferenceDbInterface.GetManufacturerId(manufacturerCode);

            if (mfrId == 0)
            {
                Exception ex = new Exception("The manufacturer " + manufacturerCode + " does not currently exist in the CDMMS manufacturer table. Please have the manufacturer added before attempting to save again.");

                ex.Source = "INVALID_MFR";

                throw ex;
            }

            return mfrId;
        }
    }
}