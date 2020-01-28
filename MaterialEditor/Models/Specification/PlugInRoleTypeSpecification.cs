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
    public class PlugInRoleTypeSpecification : Specification
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private ISpecificationDbInterface dbInterface = null;

        public PlugInRoleTypeSpecification() : base()
        {
            
        }

        public PlugInRoleTypeSpecification(long specificationId) : base(specificationId)
        {
            
        }

        public PlugInRoleTypeSpecification(long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            
        }

        public void PopulateBayInternalList()
        {
            
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.Typ)]
        public override SpecificationType.Type Type
        {
            get
            {
                return SpecificationType.Type.PLUG_IN_ROLE_TYPE;
            }
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PlgInRlTyp)]
        public string PlugInRoleType
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.BiDrctnl)]
        public string BiDirectionalAllowIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlHgt)]
        public decimal ExternalHeight
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlWdth)]
        public decimal ExternalWidth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlDpth)]
        public decimal ExternalDepth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.XtnlDimUom)]
        public string ExternalDimensionsUnitOfMeasure
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PlgInRlTypDsc)]
        public string TypeDescription
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.PlgInRlTypNm)]
        public string TypeName
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.CnctrHgt)]
        public decimal ConnectorHeight
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.CnctrWdth)]
        public decimal ConnectorWidth
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.CnctrUom)]
        public string ConnectorDimensionsUnitOfMeasure
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

        public override ISpecificationDbInterface DbInterface
        {
            get
            {
                if (dbInterface == null)
                    dbInterface = new PlugInSpecificationDbInterface();

                return dbInterface;
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