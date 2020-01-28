using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json.Linq;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public interface ITemplate
    {
        long TemplateId
        {
            get;
            set;
        }

        long SpecificationRevisionId
        {
            get;
            set;
        }

        long SpecificationId
        {
            get;
            set;
        }

        TemplateType.Type Type
        {
            get;
        }

        string TemplateType
        {
            get;
        }

        string Name
        {
            get;
            set;
        }
        
        string Description
        {
            get;
            set;
        }

        List<Option> BayChoices
        {
            get;
            set;
        }

        bool IsCompleted
        {
            get;
            set;
        }

        bool IsPropagated
        {
            get;
            set;
        }

        bool IsDeleted
        {
            get;
            set;
        }

        bool IsRetired
        {
            get;
            set;
        }

        bool UpdateInProgress
        {
            get;
            set;
        }

        bool IsBaseTemplate
        {
            get;
        }

        bool IsHighLevelPartTemplate
        {
            get;
        }

        bool IsCommonConfigTemplate
        {
            get;
        }

        ISpecification AssociatedSpecification
        {
            get;
            set;
        }

        ITemplateDbInterface DbInterface
        {
            get;
        }

        Drawing TemplateDrawing
        {
            get;
        }

        Scale TemplateScale
        {
            get;
            set;
        }

        void PersistUpdates(JObject updatedTmplt);

        long PersistTemplate(Hashtable tmpltValues);

        //decimal ConvertPhysicalDimensionToPixels(decimal dimension);

        //decimal ConvertPixelsToPhysicalDimension(decimal pixels);

        void InitializeAttributes();

        void CreateDrawing(bool isForEdit);
    }
}
