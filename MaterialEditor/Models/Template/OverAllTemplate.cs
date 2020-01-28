using System;
using System.Collections;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public abstract class OverAllTemplate : TemplateImpl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private BaseTemplate baseTemplate = null;

        public OverAllTemplate()
        {

        }

        public OverAllTemplate(long templateId) : base(templateId)
        {

        }

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplate)]
        public override bool IsBaseTemplate
        {
            get
            {
                return false;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplateId)]
        public long BaseTemplateId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.ParentBaseTemplate)]
        public BaseTemplate TemplateBase
        {
            get
            {
                return baseTemplate;
            }
            set
            {
                baseTemplate = value;
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