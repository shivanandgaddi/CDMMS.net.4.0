using System;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Web.Script.Serialization;

//Added code by
//Author : Shivanand S Gaddi
//Date : 17/01/2019
//Description : Card Has slot checkbox pop up window details and modification

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public class CardHasSlots : Specification
    {
        private Dictionary<string, SpecificationAttribute> attributes = null;
        private ISpecificationDbInterface dbInterface = null;

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotDefId)]
        public int cardSlotDefId
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotSpecId)]
        public int cardSlotSpecId
        {
            get;
            set;
        }
        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotSeq)]
        public int SlotSequence
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotTyp)]
        public string SlotType
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.SlotQty )]
        public int SlotQuantity
        {
            get;
            set;
        }

        [JsonIgnore]
        [MaterialJsonProperty(SpecificationType.JSON.unSelectToRemove, true, "")]
        public bool UnSelectToRemove
        {
            get;
            set;
        }
        [ScriptIgnore]
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
        [ScriptIgnore]
        public override SpecificationType.Type Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [ScriptIgnore]
        public override IMaterial AssociatedMaterial
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        [ScriptIgnore]
        public override long AssociatedMaterialId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [ScriptIgnore]
        public override string NDSManufacturer
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
       
        public override void Persist(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }
     
        public override long PersistNewSpecification(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }

        public override void PersistMaterial(JObject updatedSpecification, ref bool notifyNDS)
        {
            throw new NotImplementedException();
        }

        public override void PersistNDSSpecificationId(long ndsId)
        {
            throw new NotImplementedException();
        }
    }       
}