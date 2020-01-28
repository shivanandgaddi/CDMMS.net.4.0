using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification
{
    public interface ISpecificationDbInterface
    {
        void StartTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void Dispose();

        ISpecification GetSpecification(long specificationId);

        void AssociateMaterial(JObject jObject);
    }
}
