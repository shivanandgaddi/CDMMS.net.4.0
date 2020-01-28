using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class BayInternalWidth : Specification
    {
        private Dictionary<string, SpecificationAttribute> attributes = null;

        public BayInternalWidth()
        {
        }

        public BayInternalWidth(long id)
        {
            WidthId = id;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.BAY_INTERNAL_WIDTH;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthId)]
        public long WidthId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdth)]
        public decimal Width
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthUomId)]
        public int WidthUnitOfMeasureId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BayIntlWdthUom)]
        public string WidthUnitOfMeasure
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