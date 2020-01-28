using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class EntityList
    {
        [JsonProperty("entities")]
        public IList<Entity> Entities
        {
            get;
            set;
        }
    }

    public class Entity
    {
        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("wc_id")]
        public string WireCenterId
        {
            get;
            set;
        }

        [JsonProperty("cl_feature_id")]
        public string FeatureId
        {
            get;
            set;
        }
        
        [JsonProperty("attributes")]
        public IList<Attribute> AttributeList
        {
            get;
            set;
        }
    }

    public class Attribute
    {
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("value")]
        public object Value
        {
            get;
            set;
        }

        [JsonIgnore]
        public long Id
        {
            get;
            set;
        }

        [JsonIgnore]
        public long ParentId
        {
            get;
            set;
        }
    }
}
