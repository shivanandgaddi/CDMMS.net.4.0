using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class TemplateManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TemplateManager()
        {

        }

        public async Task<string> GetTemplateType(long templateId)
        {
            var db = new TemplateDbInterface();
            var typ = await db.GetTmpltTyp(templateId);
            return typ;
        }
        public async Task<ITemplate> GetTemplate(long templateId, TemplateType.Type tmpltType, bool isBaseTemplate, bool isForEdit)
        {
            ITemplate template = null;

            await Task.Run(() =>
            {
                if (templateId > 0)
                {
                    template = TemplateFactory.GetTemplateInstance(templateId, tmpltType, isBaseTemplate);

                    if(template != null)
                        template.CreateDrawing(isForEdit);
                }
                else
                    template = TemplateFactory.GetTemplateClass(tmpltType, isBaseTemplate);
            });

            return template;
        }

        public async Task<long> CreateTemplate(Hashtable tmpltValues)
        {
            ITemplate template = null;
            TemplateType.Type tmpltType = TemplateType.Type.NOT_SET;

            if (!Enum.TryParse<TemplateType.Type>((string)tmpltValues["pTmpTyp"], out tmpltType))
            {
                logger.Info("Unable to parse template type value: {0}", tmpltValues["pTmpTyp"]);

                return 0;
            }

            await Task.Run(() =>
            {
                template = TemplateFactory.GetTemplateClass(tmpltType, "Base".Equals(tmpltValues["pTmpType"]));

                if (template != null)
                    template.TemplateId = template.PersistTemplate(tmpltValues);
            });

            if (template != null)
                return template.TemplateId;
            else
                return 0;
        }

        public async Task UpdateTemplate(JObject updatedTmplt)
        {
            ITemplate template = null;
            TemplateType.Type templateType = TemplateType.Type.NOT_SET;
            string tmpltType = "";
            long templateId = 0;
            bool isBaseTemplate = true;

            await Task.Run(() =>
            {
                tmpltType = (string)updatedTmplt.SelectToken("TemplateType");

                if (!long.TryParse((string)updatedTmplt.SelectToken("TemplateID"), out templateId))
                    throw new Exception("Unable to parse template id from updated template object.");

                if (!Enum.TryParse<TemplateType.Type>(tmpltType, out templateType))
                    throw new Exception("Unable to parse template type from updated template object.");

                if (!bool.TryParse((string)updatedTmplt.SelectToken("BaseTemplateInd"), out isBaseTemplate))
                    throw new Exception("Unable to parse base template from updated template object.");

                template = TemplateFactory.GetTemplateInstance(templateId, templateType, isBaseTemplate);

                if (template != null)
                    template.PersistUpdates(updatedTmplt);
            });
        }
    }

    public class TemplateFactory
    {
        private TemplateFactory()
        {

        }

        public static ITemplate GetTemplateInstance(long templateId, TemplateType.Type tmpltType, bool isBaseTemplate)
        {
            ITemplate template = null;
            ITemplateDbInterface dbInterface = null;

            switch (tmpltType)
            {
                case TemplateType.Type.BAY:
                    dbInterface = new BayTemplateDbInterface();

                    break;
                case TemplateType.Type.SHELF:
                    dbInterface = new ShelfTemplateDbInterface();

                    break;
                case TemplateType.Type.PLUG_IN:
                    dbInterface = new PlugInTemplateDbInterface();

                    break;
                case TemplateType.Type.CARD:
                    dbInterface = new CardTemplateDbInterface();

                    break;
                case TemplateType.Type.HIGH_LEVEL_PART:
                    dbInterface = new HlpTemplateDbInterface();

                    break;
                default:
                    break;
            }

            if (template == null && dbInterface != null)
                template = dbInterface.GetTemplate(templateId, isBaseTemplate);

            return template;
        }

        public static ITemplate GetTemplateClass(TemplateType.Type tmpltType, bool isBaseTemplate)
        {
            ITemplate template = null;

            switch (tmpltType)
            {
                case TemplateType.Type.BAY:
                    if (isBaseTemplate)
                        template = new BaseBayTemplate();
                    else
                        template = new OverAllBayTemplate();

                    break;
                case TemplateType.Type.SHELF:
                    if (isBaseTemplate)
                        template = new BaseShelfTemplate();
                    else
                        template = new OverAllShelfTemplate();
                    break;
                case TemplateType.Type.PLUG_IN:
                    if (isBaseTemplate)
                        template = new BasePlugInTemplate();
                    break;

                case TemplateType.Type.CARD:
                    if (isBaseTemplate)
                        template = new BaseCardTemplate();
                    else
                        template = new OverAllCardTemplate();

                    break;
                case TemplateType.Type.HIGH_LEVEL_PART:
                    if (isBaseTemplate)
                        template = new BaseHlpTemplate();
                    else
                        template = new OverAllHlpTemplate();

                    break;
                default:
                    break;
            }

            return template;
        }
    }
}