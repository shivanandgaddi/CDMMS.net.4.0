using System.Collections.Generic;
using System.Collections.Specialized;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public interface IMaterial
    {
        long MaterialItemId
        {
            get;
            set;
        }

        long MaterialId
        {
            get;
            set;
        }

        long ManufacturerId
        {
            get;
            set;
        }

        string MaterialCode
        {
            get;
            set;
        }

        string Manufacturer
        {
            get;
            set;
        }
        string ManufacturerName
        {
            get;
            set;
        }

        int LaborId
        {
            get;
            set;
        }

        string RootPartNumber
        {
            get;
            set;
        }

        string CatalogDescription
        {
            get;
            set;
        }

        string SpecificationName
        {
            get;
            set;
        }

        string SpecificationRevisionName
        {
            get;
            set;
        }

        long SpecificationId
        {
            get;
            set;
        }

        long SpecificationRevisionId
        {
            get;
            set;
        }

        bool SpecificationPropagated
        {
            get;
            set;
        }

        string ReplacesMaterialPartNumber
        {
            get;
            set;
        }

        long ReplacesMaterialItemId
        {
            get;
            set;
        }

        string ReplacedByMaterialPartNumber
        {
            get;
            set;
        }

        long ReplacedByMaterialItemId
        {
            get;
            set;
        }

        bool IsRecordOnly
        {
            get;
            set;
        }

        bool IsRecordOnlyPublished
        {
            get;
            set;
        }

        bool HasRevisions
        {
            get;
            set;
        }

        bool MayHaveSpecifications
        {
            get;
            set;
        }

        Dictionary<string, DatabaseDefinition> MaterialDefinition
        {
            get;
        }

        IMaterialDbInterface DbInterface
        {
            get;
        }

        Dictionary<string, Attribute> Attributes
        {
            get;
            set;
        }

        StringCollection SAPOverrideAttributeNames
        {
            get;
        }

        StringCollection RecordOnlyAttributeNames
        {
            get;
        }

        Dictionary<string, Attribute> AdditionalAttributeNames
        {
            get;
        }

        int MaterialCategoryId
        {
            get;
        }

        int FeatureTypeId
        {
            get;
        }

        void PersistMaterialUpdates(JObject updatedMaterialItem, ref bool notifyNDS, ref bool notifyNDSSpecification);

        long[] PersistObject(IMaterial currentMaterialItem, JObject updatedMaterialItem, long mtrlId, ref bool notifyNDS, ref bool notifyNDSSpecification);

        void CreateRevision(JObject updatedMaterialItem, ref bool notifyNDSSpecification);
    }
}
