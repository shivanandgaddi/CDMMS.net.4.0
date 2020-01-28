using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public abstract class TemplateImpl : ITemplate 
    {
        private ReferenceDbInterface referenceDbInterface = null;
        private long id = 0;
        private Scale scale = null;

        public TemplateImpl()
        {

        }

        public TemplateImpl(long templateId)
        {
            id = templateId;
        }

        protected List<Option> GetOptions(string attributeName)
        {
            List<Option> options = null;

            if (referenceDbInterface == null)
                referenceDbInterface = new ReferenceDbInterface();

            Task t = Task.Run(async () =>
            {
                options = await referenceDbInterface.GetOptions(attributeName);
            });

            t.Wait();

            return options;
        }

        [JsonProperty(Utility.TemplateType.JSON.TemplateId)]
        public long TemplateId
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

        [JsonProperty(Utility.TemplateType.JSON.Name)]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Description)]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.BayChoices)]
        public List<Option> BayChoices
        {
            get;
            set;
        }


        [JsonProperty(Utility.TemplateType.JSON.Completed)]
        public bool IsCompleted
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Propagated)]
        public bool IsPropagated
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Deleted)]
        public bool IsDeleted
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Retired)]
        public bool IsRetired
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.UpdateInProgress)]
        public bool UpdateInProgress
        {
            get;
            set;
        }

        [JsonIgnore]
        public Scale TemplateScale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public abstract long SpecificationRevisionId { get; set; }
        public abstract long SpecificationId { get; set; }
        public abstract TemplateType.Type Type { get; }
        public abstract string TemplateType { get; }
        public abstract bool IsBaseTemplate { get; }
        public abstract bool IsHighLevelPartTemplate { get; }
        public abstract bool IsCommonConfigTemplate { get; }
        public abstract ISpecification AssociatedSpecification { get; set; }
        public abstract ITemplateDbInterface DbInterface { get; }
        public abstract Drawing TemplateDrawing { get; }

        public abstract void InitializeAttributes();
        public abstract long PersistTemplate(Hashtable tmpltValues);
        public abstract void PersistUpdates(JObject updatedTmplt);
        public abstract void CreateDrawing(bool isForEdit);
        //public abstract decimal ConvertPhysicalDimensionToPixels(decimal dimension);
        //public abstract decimal ConvertPixelsToPhysicalDimension(decimal pixels);
    }
}