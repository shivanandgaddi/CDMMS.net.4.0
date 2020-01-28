using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public abstract class Specification : ISpecification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string Int32 = "Int32";
        private const string Int64 = "Int64";
        private const string Decimal = "Decimal";
        private const string Double = "Double";
        private const string DateTime = "DateTime";
        private const string Zero = "0";
        private long id = 0;
        private string description = "";
        private string useType = "";
        private string name = "";

        public Specification()
        {
        }

        public Specification(long specificationId)
        {
            id = specificationId;
        }

        public Specification(long specificationId, string specificationName)
        {
            id = specificationId;
            name = specificationName;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Desc)]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.ShelfNDSUseTyp)]
        public string UseType
        {
            get
            {
                return useType;
            }

            set
            {
                useType = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Name)]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SpecificationId)]
        public long SpecificationId
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.NDSSpecificationId)]
        public long NDSSpecificationId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Cmplt, true, "")]
        public bool IsCompleted
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Prpgtd, true, "")]
        public bool IsPropagated
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Dltd, true, "")]
        public bool IsDeleted
        {
            get;
            set;
        }

        protected Dictionary<string, SpecificationAttribute> GetAttributes(ISpecification specification, bool returnAllAttributes)
        {
            Dictionary<string, SpecificationAttribute> attributes = null;

            if (specification != null)
            {
                Type specificationType = specification.GetType();
                ReferenceDbInterface dbInterface = null;

                foreach (PropertyInfo propertyInfo in specificationType.GetProperties())
                {
                    MaterialJsonProperty jsonProperty = (MaterialJsonProperty)propertyInfo.GetCustomAttribute(typeof(MaterialJsonProperty));

                    if (jsonProperty != null)
                    {
                        if (jsonProperty.IsObjectList)
                        {
                            Type propertyType = propertyInfo.PropertyType.GetInterface("System.Collections.IDictionary");

                            if (propertyType != null)
                            {
                                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)propertyInfo.GetValue(specification);

                                if (dictionary != null)
                                {
                                    System.Collections.IDictionaryEnumerator dictionaryEnumerator = dictionary.GetEnumerator();

                                    if (dictionaryEnumerator != null)
                                    {
                                        object key;
                                        Specification obj;
                                        SpecificationAttribute sa = null;

                                        while (dictionaryEnumerator.MoveNext())
                                        {
                                            key = dictionaryEnumerator.Key;
                                            obj = (Specification)dictionaryEnumerator.Value;

                                            if (sa == null)
                                            {
                                                sa = new SpecificationAttribute(true, jsonProperty.Name);

                                                sa.ObjectList = new List<Dictionary<string, SpecificationAttribute>>();
                                            }

                                            sa.ObjectList.Add(obj.Attributes);

                                            if (jsonProperty.HasOptionValues)
                                            {
                                                if (jsonProperty.OptionValues != null)
                                                {
                                                    sa.Options = new List<Option>();

                                                    sa.Options.Add(new Option("", ""));

                                                    try
                                                    {
                                                        for (int i = 0; i < jsonProperty.OptionValues.Length; i = i + 2)
                                                        {
                                                            sa.Options.Add(new Option(jsonProperty.OptionValues[i], jsonProperty.OptionValues[i + 1]));
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        logger.Info(ex, "Error attempting to create option list for specification id {0}, property {1}.", SpecificationId, jsonProperty.Name);
                                                    }
                                                }
                                                else
                                                {
                                                    if (dbInterface == null)
                                                        dbInterface = new ReferenceDbInterface();

                                                    Task t = Task.Run(async () =>
                                                    {
                                                        sa.Options = await dbInterface.GetOptions(jsonProperty.Name, null);
                                                    });

                                                    t.Wait();
                                                }
                                            }
                                        }

                                        if (attributes == null)
                                            attributes = new Dictionary<string, SpecificationAttribute>();

                                        if(sa != null)
                                            attributes.Add(sa.Name, sa);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string value = string.Empty;

                            try
                            {
                                value = propertyInfo.GetValue(specification).ToString();
                            }
                            catch (Exception ex)
                            {
                            }

                            if (returnAllAttributes || ((!returnAllAttributes && !string.IsNullOrEmpty(value)) || jsonProperty.HasOptionValues))
                            {
                                SpecificationAttribute attr = new SpecificationAttribute();

                                attr.Name = jsonProperty.Name;

                                if (attributes == null)
                                    attributes = new Dictionary<string, SpecificationAttribute>();

                                if (jsonProperty.HasOptionValues)
                                {
                                    if (jsonProperty.OptionValues != null)
                                    {
                                        attr.Options = new List<Option>();

                                        attr.Options.Add(new Option("", ""));

                                        try
                                        {
                                            for (int i = 0; i < jsonProperty.OptionValues.Length; i = i + 2)
                                            {
                                                attr.Options.Add(new Option(jsonProperty.OptionValues[i], jsonProperty.OptionValues[i + 1]));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Info(ex, "Error attempting to create option list for specification id {0}, property {1}.", SpecificationId, jsonProperty.Name);
                                        }
                                    }
                                    else
                                    {
                                        if (dbInterface == null)
                                            dbInterface = new ReferenceDbInterface();

                                        Task t = Task.Run(async () =>
                                        {
                                            attr.Options = await dbInterface.GetOptions(jsonProperty.Name, null);
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
                                }

                                if ("Boolean".Equals(propertyInfo.PropertyType.Name))
                                {
                                    bool bValue = false;

                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        if (!Boolean.TryParse(value, out bValue))
                                            logger.Info("Unable to parse [{0}] as boolean. SpecificationId: {1}", value, specification.SpecificationId);
                                    }

                                    attr.BoolValue = bValue;

                                    attributes.Add(jsonProperty.Name, attr);
                                }
                                else
                                {
                                    if (Zero.Equals(value))
                                    {
                                        string typeName = propertyInfo.PropertyType.Name;

                                        if (!returnAllAttributes && !jsonProperty.HasOptionValues && (jsonProperty.IgnoreNumericZeroValues && (Int32.Equals(typeName) || Int64.Equals(typeName) || Decimal.Equals(typeName) || Double.Equals(typeName))))
                                        {
                                            //skip it
                                        }
                                        else
                                        {
                                            attr.Value = value;

                                            attributes.Add(jsonProperty.Name, attr);
                                        }
                                    }
                                    else
                                    {
                                        attr.Value = value;

                                        if (!attributes.ContainsKey(jsonProperty.Name))
                                        {
                                            attributes.Add(jsonProperty.Name, attr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return attributes;
        }

        public abstract ISpecificationDbInterface DbInterface
        {
            get;
        }

        public abstract Dictionary<string, SpecificationAttribute> Attributes
        {
            get;
            set;
        }

        public abstract SpecificationType.Type Type
        {
            get;
        }

        public abstract IMaterial AssociatedMaterial
        {
            get;
            set;
        }

        public abstract long AssociatedMaterialId
        {
            get;
        }

        public abstract string NDSManufacturer
        {
            get;
            set;
        }

        protected string GetMaterialAttributeValue(JObject updatedSpecification, string materialObjectName, string attributeName)
        {
            string value = "";

            try
            {
                value = (string)((JArray)((JObject)updatedSpecification.SelectToken(materialObjectName)).SelectToken("list"))[0].SelectToken(attributeName + ".value");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve material attribute value: {0}, {1}", materialObjectName, attributeName);

                throw ex;
            }

            return value;
        }

        public virtual void PersistUpdates(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate, ref bool notifyNDSOfMtlUpdate)
        {
            string cuid = (string)updatedSpecification.SelectToken("cuid");

            try
            {
                DbInterface.StartTransaction();

                Persist(updatedSpecification, ref notifyNDSOfSpecnUpdate);

                if(AssociatedMaterial != null)
                    PersistMaterial(updatedSpecification, ref notifyNDSOfMtlUpdate);

                DbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                DbInterface.RollbackTransaction();

                logger.Error(ex);

                throw ex;
            }
            finally
            {
                if (DbInterface != null)
                    DbInterface.Dispose();
            }
        }

        public virtual long PersistObject(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate, ref bool notifyNDSOfMtlUpdate)
        {
            long specificationId = 0;
            bool isGeneric = JsonHelper.GetSpecificationBoolValue(updatedSpecification, SpecificationType.JSON.Gnrc);

            try
            {
                DbInterface.StartTransaction();

                specificationId = PersistNewSpecification(updatedSpecification, ref notifyNDSOfSpecnUpdate);

                if(!isGeneric)
                    PersistMaterial(updatedSpecification, ref notifyNDSOfMtlUpdate);

                DbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                DbInterface.RollbackTransaction();

                logger.Error(ex);

                throw ex;
            }
            finally
            {
                if (DbInterface != null)
                    DbInterface.Dispose();
            }

            return specificationId;
        }

        public virtual void PersistMaterialUpdates(JObject updatedSpecification, ref bool notifyNDS)
        {
        }

        public abstract void Persist(JObject updatedSpecification, ref bool notifyNDS);

        public abstract long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS);

        public abstract void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS);
        public abstract void PersistNDSSpecificationId(long ndsId);
    }
}