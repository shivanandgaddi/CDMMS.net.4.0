using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class BayInternalDepth : Specification
    {
        private Dictionary<string, SpecificationAttribute> attributes = null;

        public BayInternalDepth()
        {
        }

        public BayInternalDepth(long id)
        {
            DepthId = id;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthId)]
        public long DepthId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpth)]
        public decimal Depth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthUomId)]
        public int DepthUnitOfMeasureId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlDpthUom)]
        public string DepthUnitOfMeasure
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Nmnl)]
        public string NominalIndicator
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
        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                throw new NotImplementedException();
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

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.BAY_INTERNAL_DEPTH;
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