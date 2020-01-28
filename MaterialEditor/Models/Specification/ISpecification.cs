using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public interface ISpecification
    {
        long SpecificationId
        {
            get;
            set;
        }

        long NDSSpecificationId
        {
            get;
            set;
        }

        string Name
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        string UseType
        {
            get;
            set;
        }

        string NDSManufacturer
        {
            get;
            set;
        }

        SpecificationType.Type Type
        {
            get;
        }

        IMaterial AssociatedMaterial
        {
            get;
            set;
        }

        long AssociatedMaterialId
        {
            get;
        }

        bool IsCompleted
        {
            get;
            set;
        }

        bool IsPropagated
        {
            get;
            set;
        }

        bool IsDeleted
        {
            get;
            set;
        }

        void PersistUpdates(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate, ref bool notifyNDSOfMtlUpdate);

        long PersistObject(JObject updatedSpecification, ref bool notifyNDSOfSpecnUpdate, ref bool notifyNDSOfMtlUpdate);

        void PersistMaterialUpdates(JObject updatedSpecification, ref bool notifyNDS);

        void PersistNDSSpecificationId(long ndsId);

        ISpecificationDbInterface DbInterface
        {
            get;
        }

        Dictionary<string, SpecificationAttribute> Attributes
        {
            get;
            set;
        }
    }
}
