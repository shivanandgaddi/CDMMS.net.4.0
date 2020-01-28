using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class TemplateType
    {
        public enum Type { BAY, CARD, NODE, PLUG_IN, SHELF, HIGH_LEVEL_PART, COMMON_CONFIG, NOT_SET }
        public enum Category { BASE, OVERALL }
        public enum DrawingObjectType { RECTANGLE, CIRCLE, LINE, TEXT }
        public enum ActualObjectType { BAY, BAY_EXTENDER, BAY_INTERNAL, BAY_SPACER, CARD, NODE, PLUG_IN, SHELF, SLOT, NOT_SET}

        public class JSON
        {
            public const string TemplateId = "tmpltId";
            public const string Name = "nm";
            public const string Type = "typ";
            public const string SpecificationRevisionId = "specRvsnId";
            public const string SpecificationId = "specId";
            public const string Description = "dsc";
            public const string Completed = "cmpltd";
            public const string Propagated = "prpgtd";
            public const string Deleted = "dltd";
            public const string Retired = "rtrd";
            public const string CUID = "cuid";
            public const string LastUpdatedDate = "lastupdtdt";
            public const string UpdateInProgress = "updtInPrgss";
            public const string BaseTemplate = "baseTmplt";
            public const string HighLevelPartTemplate = "hiLvlPrt";
            public const string CommonConfigTemplate = "comnCnfg";
            public const string AssociatedSpecification = "assctdSpec";
            public const string FrontRearIndicator = "frntRr";
            public const string RotationAngle = "rtnAngl";
            public const string Drawing = "drwng";
            public const string TemplateCategory = "tmpltCtgry";
            public const string BaseTemplateId = "bsTmpltId";
            public const string ActualObjectType = "actlTyp";
            public const string Objects = "objcts";
            public const string UniqueId = "uId";
            public const string AddUpdateDelete = "addUpdtDlt";
            public const string Attributes = "attributes";
            public const string Radius = "radius";
            public const string Left = "left";
            public const string Top = "top";
            public const string X = "x";
            public const string Y = "y";
            public const string Width = "width";
            public const string Height = "height";
            public const string ScaleX = "scaleX";
            public const string ScaleY = "scaleY";
            public const string Fill = "fill";
            public const string StrokeWidth = "strokeWidth";
            public const string Stroke = "stroke";
            public const string FontSize = "fontSize";
            public const string FontFamily = "fontFamily";
            public const string LockMovementX = "lockMovementX";
            public const string LockMovementY = "lockMovementY";
            public const string HoverCursor = "hoverCursor";
            public const string LockUniScaling = "lockUniScaling";
            public const string Selectable = "selectable";
            public const string CanBeMoved = "canBeMoved";
            public const string Text = "txt";
            public const string ParentBaseTemplate = "bsTmplt";

            // The below properties defined for COMPLEX_TMPLT data insertion/modification
            public const string BaseTmpId = "BaseTmpId";
            public const string MtrlCatId = "MtrlCatId";
            public const string FeatTypId = "FeatTypId";
            public const string BayExtndrSpecnRevsnAltId = "BayExtndrSpecnRevsnAltId";
            public const string CardSpecPrtsPrtId = "CardSpecPrtsPrtId";
            public const string CardSpecSltsSltId = "CardSpecSltsSltId";
            public const string ShelfSpecSltsSltId = "ShelfSpecSltsSltId";
            public const string AsnblNdSpecPrtAsmtId = "AsnblNdSpecPrtAsmtId";
            public const string HlpnMtrlRevId = "HlpnMtrlRevId";
            public const string CmnCofgId = "CmnCofgId";
            public const string LabelName = "LabelNm";
            public const string RtnAnglId = "RtnAnglId";
            public const string PortTypId = "PortTypId";
            public const string CnctrTypId = "CnctrTypId";
            public const string UserComment = "UserCmnt";
            public const string UserCuid = "Cuid";
            public const string FrntrearInd = "FrntrearInd";
            public const string BayChoices = "BayChoices";
        }
    }
}