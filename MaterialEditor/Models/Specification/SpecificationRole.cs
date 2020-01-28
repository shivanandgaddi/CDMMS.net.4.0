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
    public class SpecificationRole : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private ISpecificationDbInterface dbInterface = null;

        public SpecificationRole() : base()
        {

        }

        public SpecificationRole(long specificationId) : base(specificationId)
        {

        }

        public SpecificationRole(long specificationId, string specificationName) : base(specificationId, specificationName)
        {

        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SpcnRlTyp)]
        public string RoleType
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PrtyNo)]
        public int PriorityNumber
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Slctd, true, "")]
        public bool IsSelected
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
        [MaterialJsonProperty(SpecificationType.JSON.UseTyp)]
        public string RMEUseType
        {
            get;
            set;
        }

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new CardSpecificationDbInterface();

                return dbInterface;
            }
        }

        public override Dictionary<string, SpecificationAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = GetAttributes(this, true);

                return attributes;
            }

            set
            {
                attributes = value;
            }
        }

        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.ROLE;
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

        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }

        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
        }
    }
}