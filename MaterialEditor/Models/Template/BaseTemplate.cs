using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public abstract class BaseTemplate : TemplateImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private long specificationRevisionId = 0;
        private long specificationId = 0;
        private ISpecification specification = null;        

        public BaseTemplate()
        {

        }

        public BaseTemplate(long templateId) : base(templateId)
        {
            
        }        

        [JsonProperty(Utility.TemplateType.JSON.SpecificationRevisionId)]
        public override long SpecificationRevisionId
        {
            get
            {
                return specificationRevisionId;
            }
            set
            {
                specificationRevisionId = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationId)]
        public override long SpecificationId
        {
            get
            {
                return specificationId;
            }
            set
            {
                specificationId = value;
            }
        }
        

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplate)]
        public override bool IsBaseTemplate
        {
            get
            {
                return true;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.HighLevelPartTemplate)]
        public override bool IsHighLevelPartTemplate
        {
            get
            {
                return false;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.CommonConfigTemplate)]
        public override bool IsCommonConfigTemplate
        {
            get
            {
                return false;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.AssociatedSpecification)]
        public override ISpecification AssociatedSpecification {
            get
            {
                return specification;
            }
            set
            {
                specification = value;
            }
        }

        public override abstract string TemplateType { get; }
        public override abstract TemplateType.Type Type { get; }
        public override abstract ITemplateDbInterface DbInterface { get; }
        public override abstract void InitializeAttributes();
        public override abstract long PersistTemplate(Hashtable tmpltValues);
        public override abstract void PersistUpdates(JObject updatedTmplt);
    }
}