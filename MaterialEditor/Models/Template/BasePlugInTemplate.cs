using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class BasePlugInTemplate : BaseTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ITemplateDbInterface plugInTemplateDbInterface = null;
        private ISpecificationDbInterface plugInSpecDbInterface = null;
        private Drawing drawing = null;

        public BasePlugInTemplate() : base()
        {
            InitializeAttributes();
        }

        public BasePlugInTemplate(long templateId) : base(templateId)
        {
            InitializeAttributes();
        }

        [JsonProperty(Utility.TemplateType.JSON.Type)]
        public override string TemplateType
        {
            get
            {
                return Type.ToString();
            }
        }

        [JsonIgnore]
        public override TemplateType.Type Type
        {
            get
            {
                return Utility.TemplateType.Type.PLUG_IN;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Drawing)]
        public override Drawing TemplateDrawing
        {
            get
            {
                return drawing;
            }
        }

        [JsonIgnore]
        public override ITemplateDbInterface DbInterface
        {
            get
            {
                if (plugInTemplateDbInterface == null)
                    plugInTemplateDbInterface = new PlugInTemplateDbInterface();

                return plugInTemplateDbInterface;
            }
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long specId = long.Parse((string)tmpltValues["pSpecId"]);

            plugInTemplateDbInterface = new PlugInTemplateDbInterface();

            tmpltId = ((PlugInTemplateDbInterface)plugInTemplateDbInterface).CreateBaseTemplate(specId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"]);

            return tmpltId;
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

        public override void PersistUpdates(JObject updatedTmplt)
        {
            string updatedName = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Name, updatedTmplt);
            string updatedDescription = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Description, updatedTmplt).ToUpper();
            bool updatedCompleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Completed, updatedTmplt);
            bool updatedPropagated = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Propagated, updatedTmplt);
            bool updatedDeleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Deleted, updatedTmplt);
            bool updatedRetired = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Retired, updatedTmplt);
            bool updatedInProgress = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.UpdateInProgress, updatedTmplt);


            try
            {
                if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
                    !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
                    !UpdateInProgress.Equals(updatedInProgress))
                {
                    plugInTemplateDbInterface = new PlugInTemplateDbInterface();

                    plugInTemplateDbInterface.StartTransaction();

                    ((TemplateDbInterfaceImpl)plugInTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, true, IsHighLevelPartTemplate,
                        IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);
                }

                if (plugInTemplateDbInterface != null)
                    plugInTemplateDbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (plugInTemplateDbInterface != null)
                    plugInTemplateDbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (plugInTemplateDbInterface != null)
                    plugInTemplateDbInterface.Dispose();
            }
        }


        public override void InitializeAttributes()
        {

        }

        public override void CreateDrawing(bool isForEdit)
        {
            Group group = new Group();
            DrawingObject plugIn = new DrawingObject();
            DrawingObject plugInInternalSpacer = null;
            ObjectAttribute plugInInternalSpacerAttributes = null;
            ObjectAttribute plugInAttributes = new ObjectAttribute();
            Dictionary<long, PlugInRoleTypeSpecification> plgInRolelist = null;
            decimal extrnDpth = 0;
            decimal extrnHgth = 0;
            decimal extrnWdth = 0;

            plugInSpecDbInterface = new PlugInSpecificationDbInterface();
            plgInRolelist = ((PlugInSpecificationDbInterface)plugInSpecDbInterface).GetPlugInRoleTypes();

            //PlugIn plugInMaterial = new PlugIn();
            ObjectAttribute groupAttributes = new ObjectAttribute();

            long plgInRoleID = SpecificationId;

            var plgInRoleli = from plgInRole in plgInRolelist
                              where plgInRole.Key.Equals(SpecificationId)
                              select plgInRole
                             ;
            decimal left = 0;
            decimal top = 0;
            decimal textBuffer = 0.66M;

            foreach (var res in plgInRoleli.AsEnumerable())
            {
                extrnDpth = (decimal)res.Value.ExternalDepth;
                extrnHgth = (decimal)res.Value.ExternalHeight;
                extrnWdth = (decimal)res.Value.ExternalWidth;
            }

            try
            {
                if (TemplateScale == null)
                    TemplateScale = new Scale(50, 50, "50"); //plugInInternalSpacerHeightUOM

                top = TemplateScale.Top;
                left = TemplateScale.Left;

                drawing = new Drawing();
                // First Rectangle inside the canvas
                groupAttributes.CanDrag = true; // false;
                groupAttributes.CanResize = true; // false;
                groupAttributes.Top = top + 50;
                groupAttributes.Left = left;
                groupAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(20);
                groupAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(30);
                //groupAttributes.Scale = scale;
                groupAttributes.FontFamily = "Courier New";
                groupAttributes.FontSize = 12;

                // Text holding outside rectangle
                plugInAttributes.CanDrag = true; // false;
                plugInAttributes.CanResize = true; // false;
                plugInAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(20);
                plugInAttributes.IsSelectable = true; // false;
                plugInAttributes.Left = left + 30;
                plugInAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                plugInAttributes.StrokeWidth = 5; // .5M;
                plugInAttributes.Top = top;
                plugInAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(30);
                plugInAttributes.Fill = "white";
                plugInAttributes.ScaleX = 1;
                plugInAttributes.ScaleY = 1;

                plugIn.ActualObjectType = Utility.TemplateType.ActualObjectType.PLUG_IN;
                plugIn.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                plugIn.TemplateId = TemplateId;
                plugIn.Attributes = plugInAttributes;

                group.AddDrawingObject(plugIn);

                if (extrnWdth < extrnHgth)
                    left += TemplateScale.ConvertPhysicalDimensionToPixels((extrnWdth - extrnWdth) / 2);

                // top += TemplateScale.ConvertPhysicalDimensionToPixels((extrnHgth - 2) - ((BaySpecification)AssociatedSpecification).MountingPositionOffset);
                top += TemplateScale.ConvertPhysicalDimensionToPixels(10);

                //-------------------------------
                for (int i = 0; i < 10; i++)
                {
                    DrawingObject label = new DrawingObject();
                    ObjectAttribute labelAttributes = new ObjectAttribute();

                    plugInInternalSpacer = new DrawingObject();
                    plugInInternalSpacerAttributes = new ObjectAttribute();

                    labelAttributes.FontFamily = "Courier New";
                    labelAttributes.FontSize = 12;
                    labelAttributes.Stroke = "black";
                    labelAttributes.Top = top + 9;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);

                    label.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                    label.Text = i + " text";
                    labelAttributes.Left = left + TemplateScale.ConvertPhysicalDimensionToPixels(5) - (label.Text.Length * labelAttributes.FontSize * textBuffer);
                    label.Attributes = labelAttributes;
                    //Text holding main small rectngle
                    plugInInternalSpacerAttributes.CanDrag = true; // false;
                    plugInInternalSpacerAttributes.CanResize = true; // false;
                    plugInInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(2);
                    plugInInternalSpacerAttributes.IsSelectable = true; // false;
                    plugInInternalSpacerAttributes.Left = left + 50;
                    plugInInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                    plugInInternalSpacerAttributes.StrokeWidth = 1;
                    plugInInternalSpacerAttributes.Top = top + 8;
                    plugInInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(4);
                    plugInInternalSpacerAttributes.Fill = "";
                    //plugInInternalSpacerAttributes.ScaleX = 1;
                    //plugInInternalSpacerAttributes.ScaleY = 1;

                    plugInInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.PLUG_IN;
                    plugInInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                    plugInInternalSpacer.TemplateId = TemplateId;
                    plugInInternalSpacer.Attributes = plugInInternalSpacerAttributes;

                    group.AddDrawingObject(plugInInternalSpacer);
                    group.AddDrawingObject(label);

                    top = top - TemplateScale.ConvertPhysicalDimensionToPixels(1);
                }
                //--------------------------------
                group.CanDrag = true; // false;
                group.CanResize = true; // false;
                group.Category = Utility.TemplateType.Category.BASE;
                group.HoverCursor = "default";
                group.IsSelectable = true; // false;
                group.SpecificationRevisionId = SpecificationRevisionId;
                group.TemplateId = TemplateId;
                group.Type = Utility.TemplateType.Type.PLUG_IN;
                group.BaseTemplateId = TemplateId;
                group.Attributes = groupAttributes;

                drawing.AddGroup(group);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create drawing: {0}", ex.Message);
            }
        }

        //public override decimal ConvertPhysicalDimensionToPixels(decimal dimension)
        //{
        //    //TODO: account for all units, not just inches
        //    //1 inch = 10 pixels
        //    decimal pixelsPerInch = 10;

        //    return pixelsPerInch * dimension;
        //}

        //public override decimal ConvertPixelsToPhysicalDimension(decimal pixels)
        //{
        //    throw new NotImplementedException();
        //}
    }
}