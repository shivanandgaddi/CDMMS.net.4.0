using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class ComplexTemplate
    {
        public ComplexTemplate()
        {
        }

        public int TemplateID { get; set; }
        public int BaseTemplateID { get; set; }
        public int MaterialCatID { get; set; }
        public int FeatureTypeID { get; set; }
        public int BayExtenderSpecnRevisionAltID { get; set; }
        public int CardSpecnWithPartsPartsID { get; set; }
        public int CardSpecnWithSlotsSlotsID { get; set; }
        public int ShelfSpecnWithSlotsSlotsID { get; set; }
        public int AssignableNDSpecnWithPartsAsmtID { get; set; }
        public int HlpMaterialRevsnID { get; set; }
        public int CommonConfigID { get; set; }
        public string LabelName { get; set; }
        public int RotationAngleID { get; set; }
        public int PortTypeID { get; set; }
        public int ConnectorTypeID { get; set; }
        public string UserComment { get; set; }
        public string CUID { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string FrontRearInd { get; set; }
        public string MaterialCatType { get; set; }
        public string FeatureType { get; set; }
        public string BayExtenderSpecnRevisionName { get; set; }
        public string RootPartNumber { get; set; }
        public string MaterialCode { get; set; }
        public string CommonConfigName { get; set; }
        public string RotationAngleDegreeNumber { get; set; }
        public string PortType { get; set; }
        public string ConnectorTypeCode { get; set; }
        public List<ComplextTemplateDefObject> ComplextTemplateDefList { get; set; }

        public class ComplextTemplateDefObject
        {
            public int TemplateDefID { get; set; }
            public int AssociatedTemplateID { get; set; }
            public int ParentTemplateID { get; set; }
            public int XCoordNo { get; set; }
            public int YCoordNo { get; set; }
            public string LabelName { get; set; }
            public int RotationAngleID { get; set; }
            public string FrontRearInd { get; set; }
            public string TemplateName { get; set; }
            public string AssociatedTemplateName { get; set; }
            public string ParentTemplateName { get; set; }
            public string RotationAngleDegreeNumber { get; set; }
            public TemplateObject AssociatedTemplate { get; set; }
            public TemplateObject ParentTemplate { get; set; }
        }

        public class TemplateObject
        {
            public int TemplateID { get; set; }
            public string TemplateName { get; set; }
            public int TemplateTypeID { get; set; }
            public string TemplateType { get; set; }
            public string TemplateDescription { get; set; }
            public string BaseTemplateInd { get; set; }
            public string HlpTmpltInd { get; set; }
            public string CommonConfigTemplateInd { get; set; }
            public string CompletedInd { get; set; }
            public string PropagatedInd { get; set; }
            public string UpdateInProgressInd { get; set; }
            public string RetiredTemplateInd { get; set; }
            public string DeletedInd { get; set; }
        }
    }
}