using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class BaseBayTemplate : BaseTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ITemplateDbInterface bayTemplateDbInterface = null;
        private TemplateAttribute rotationAngleAttribute = null;
        private TemplateAttribute frontRearAttribute = null;
        private Drawing drawing = null;

        public BaseBayTemplate() : base()
        {
            InitializeAttributes();
        }

        public BaseBayTemplate(long templateId) : base(templateId)
        {
            InitializeAttributes();
        }

        [JsonProperty(Utility.TemplateType.JSON.RotationAngle)]
        public TemplateAttribute RotationAngleId
        {
            get
            {
                if (rotationAngleAttribute.Options == null)
                {
                    rotationAngleAttribute.Options = GetOptions(Utility.TemplateType.JSON.RotationAngle);

                    rotationAngleAttribute.Options.RemoveAt(0);
                }

                return rotationAngleAttribute;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.FrontRearIndicator)]
        public TemplateAttribute FrontRearIndicator
        {
            get
            {
                if (frontRearAttribute.Options == null)
                {
                    frontRearAttribute.Options = new List<Option>();

                    frontRearAttribute.Options.Add(new Option("", ""));
                    frontRearAttribute.Options.Add(new Option("F", "Front"));
                    frontRearAttribute.Options.Add(new Option("R", "Rear"));
                }

                return frontRearAttribute;
            }
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
                return Utility.TemplateType.Type.BAY;
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
                if (bayTemplateDbInterface == null)
                    bayTemplateDbInterface = new BayTemplateDbInterface();

                return bayTemplateDbInterface;
            }
        }

        public void SetRotationAngleId(int id)
        {
            if (rotationAngleAttribute != null)
                rotationAngleAttribute.Value = id.ToString();
        }

        public void SetFrontRearIndicator(string frontRear)
        {
            if (frontRearAttribute != null)
                frontRearAttribute.Value = frontRear;
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long specRvnsId = long.Parse((string)tmpltValues["pSpecId"]);

            bayTemplateDbInterface = new BayTemplateDbInterface();

            tmpltId = ((BayTemplateDbInterface)bayTemplateDbInterface).CreateBaseTemplate(specRvnsId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"]);

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
            string updatedName = JsonHelper.GetStringValue("TemplateName", updatedTmplt);
            string updatedDescription = JsonHelper.GetStringValue("TemplateDescription", updatedTmplt).ToUpper();

            bool updatedCompleted = JsonHelper.GetBoolValue("CompletedInd", updatedTmplt);
            bool updatedPropagated = JsonHelper.GetBoolValue("PropagatedInd", updatedTmplt);
            bool updatedDeleted = JsonHelper.GetBoolValue("DeletedInd", updatedTmplt);
            bool updatedRetired = JsonHelper.GetBoolValue("RetiredTemplateInd", updatedTmplt);
            bool updatedInProgress = JsonHelper.GetBoolValue("UpdateInProgressInd", updatedTmplt);

            bool hlpTmpltInd = JsonHelper.GetBoolValue("HlpTmpltInd", updatedTmplt);
            bool comnCnfgTmpltInd = JsonHelper.GetBoolValue("CommonConfigTemplateInd", updatedTmplt);

            //int updatedRotationAngleId = 0;
            //TemplateAttribute updatedRotationAngle = JsonHelper.DeserializeAttribute(Utility.TemplateType.JSON.RotationAngle, updatedTmplt);
            //TemplateAttribute updatedFrontRearAttribute = JsonHelper.DeserializeAttribute(Utility.TemplateType.JSON.FrontRearIndicator, updatedTmplt);

            //if (updatedRotationAngle != null)
            //    int.TryParse(updatedRotationAngle.Value, out updatedRotationAngleId);

            try
            {
                //if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
                //    !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
                //    !UpdateInProgress.Equals(updatedInProgress))
                //{
                    bayTemplateDbInterface = new BayTemplateDbInterface();

                    bayTemplateDbInterface.StartTransaction();

                    ((TemplateDbInterfaceImpl)bayTemplateDbInterface).UpdateTemplate(updatedName
                        , updatedDescription
                        , true // isBaseTemplate
                        , hlpTmpltInd
                        , comnCnfgTmpltInd
                        , updatedCompleted
                        , updatedPropagated
                        , updatedInProgress
                        , updatedRetired
                        , updatedDeleted
                        , TemplateId
                        );
                //}

                //if (!RotationAngleId.Value.Equals(updatedRotationAngleId.ToString()) || FrontRearIndicatorChanged(updatedFrontRearAttribute.Value))
                //{
                //    if (bayTemplateDbInterface == null)
                //    {
                //        bayTemplateDbInterface = new BayTemplateDbInterface();

                //        bayTemplateDbInterface.StartTransaction();
                //    }

                //    ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateBaseTemplate(TemplateId, SpecificationRevisionId, updatedFrontRearAttribute.Value, updatedRotationAngleId);
                //}

                if (bayTemplateDbInterface != null)
                    bayTemplateDbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (bayTemplateDbInterface != null)
                    bayTemplateDbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (bayTemplateDbInterface != null)
                    bayTemplateDbInterface.Dispose();
            }
        }
        //public override void PersistUpdates(JObject updatedTmplt)
        //{
        //    string updatedName = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Name, updatedTmplt);
        //    string updatedDescription = JsonHelper.GetStringValue(Utility.TemplateType.JSON.Description, updatedTmplt).ToUpper();
        //    int updatedRotationAngleId = 0;
        //    bool updatedCompleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Completed, updatedTmplt);
        //    bool updatedPropagated = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Propagated, updatedTmplt);
        //    bool updatedDeleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Deleted, updatedTmplt);
        //    bool updatedRetired = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Retired, updatedTmplt);
        //    bool updatedInProgress = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.UpdateInProgress, updatedTmplt);
        //    TemplateAttribute updatedRotationAngle = JsonHelper.DeserializeAttribute(Utility.TemplateType.JSON.RotationAngle, updatedTmplt);
        //    TemplateAttribute updatedFrontRearAttribute = JsonHelper.DeserializeAttribute(Utility.TemplateType.JSON.FrontRearIndicator, updatedTmplt);

        //    if (updatedRotationAngle != null)
        //        int.TryParse(updatedRotationAngle.Value, out updatedRotationAngleId);

        //    try
        //    {
        //        if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
        //            !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
        //            !UpdateInProgress.Equals(updatedInProgress))
        //        {
        //            bayTemplateDbInterface = new BayTemplateDbInterface();

        //            bayTemplateDbInterface.StartTransaction();

        //            ((TemplateDbInterfaceImpl)bayTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, true, IsHighLevelPartTemplate,
        //                IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);
        //        }

        //        if (!RotationAngleId.Value.Equals(updatedRotationAngleId.ToString()) || FrontRearIndicatorChanged(updatedFrontRearAttribute.Value))
        //        {
        //            if (bayTemplateDbInterface == null)
        //            {
        //                bayTemplateDbInterface = new BayTemplateDbInterface();

        //                bayTemplateDbInterface.StartTransaction();
        //            }

        //            ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateBaseTemplate(TemplateId, SpecificationRevisionId, updatedFrontRearAttribute.Value, updatedRotationAngleId);
        //        }

        //        if (bayTemplateDbInterface != null)
        //            bayTemplateDbInterface.CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (bayTemplateDbInterface != null)
        //            bayTemplateDbInterface.RollbackTransaction();

        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (bayTemplateDbInterface != null)
        //            bayTemplateDbInterface.Dispose();
        //    }
        //}

        private bool FrontRearIndicatorChanged(string updatedValue)
        {
            bool didChange = false;

            if (frontRearAttribute.Value == null && !string.IsNullOrEmpty(updatedValue))
                didChange = true;
            else if (!frontRearAttribute.Value.Equals(updatedValue))
                didChange = true;

            return didChange;
        }

        public override void InitializeAttributes()
        {
            rotationAngleAttribute = new TemplateAttribute(Utility.TemplateType.JSON.RotationAngle);
            frontRearAttribute = new TemplateAttribute(Utility.TemplateType.JSON.FrontRearIndicator);
        }

        public override void CreateDrawing(bool isForEdit)
        {
            Group group = new Group();
            DrawingObject bay = new DrawingObject();
            DrawingObject bayInternalSpacer = null;
            ObjectAttribute bayAttributes = new ObjectAttribute();
            ObjectAttribute bayInternalSpacerAttributes = null;
            Bay bayMaterial = (Bay)AssociatedSpecification.AssociatedMaterial;
            ObjectAttribute groupAttributes = new ObjectAttribute();
            BayInternalSpecification bayInternalSpec = ((BaySpecification)AssociatedSpecification).BayInternalList[((BaySpecification)AssociatedSpecification).BayInternalId];
            string bayInternalSpacerHeightUOM = bayInternalSpec.MountingPositionDistance.Split(' ')[1];
            decimal bayInternalSpacerHeight = decimal.Parse(bayInternalSpec.MountingPositionDistance.Split(' ')[0]);
            decimal left = 0;
            decimal top = 0;
            decimal textBuffer = 0.66M;

            try
            {
                if (TemplateScale == null)
                    TemplateScale = new Scale(bayMaterial.ExternalHeight, bayMaterial.ExternalWidth, bayInternalSpacerHeightUOM);

                top = TemplateScale.Top;
                left = TemplateScale.Left;

                drawing = new Drawing();

                groupAttributes.CanDrag = true; // false;
                groupAttributes.CanResize = true; // false;
                groupAttributes.Top = top;
                groupAttributes.Left = left;
                groupAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(bayMaterial.ExternalHeight);
                groupAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(bayMaterial.ExternalWidth);
                //groupAttributes.Scale = scale;
                groupAttributes.FontFamily = "Courier New";
                groupAttributes.FontSize = 12;

                bayAttributes.CanDrag = true; // false;
                bayAttributes.CanResize = true; // false;
                bayAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(bayMaterial.ExternalHeight);
                bayAttributes.IsSelectable = true; // false;
                bayAttributes.Left = left;
                bayAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                bayAttributes.StrokeWidth = 5; // .5M;
                bayAttributes.Top = top;
                bayAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(bayMaterial.ExternalWidth);
                bayAttributes.Fill = "white";
                //bayAttributes.ScaleX = 1;
                //bayAttributes.ScaleY = 1;

                bay.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY;
                bay.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                bay.TemplateId = TemplateId;
                bay.Attributes = bayAttributes;

                group.AddDrawingObject(bay);

                if (bayInternalSpec.Width < bayMaterial.ExternalWidth)
                    left += TemplateScale.ConvertPhysicalDimensionToPixels((bayMaterial.ExternalWidth - bayInternalSpec.Width) / 2);

                top += TemplateScale.ConvertPhysicalDimensionToPixels((bayMaterial.ExternalHeight - bayInternalSpacerHeight) - ((BaySpecification)AssociatedSpecification).MountingPositionOffset);

                //bayInternalSpacerHeight = bayInternalSpec.MountingPositionQuantity * bayInternalSpec.MountingPositionDistance;

                for (int i = 0; i < bayInternalSpec.MountingPositionQuantity; i++) // bayInternalSpec.MountingPositionQuantity; i++)
                {
                    DrawingObject label = new DrawingObject();
                    ObjectAttribute labelAttributes = new ObjectAttribute();

                    bayInternalSpacer = new DrawingObject();
                    bayInternalSpacerAttributes = new ObjectAttribute();

                    labelAttributes.FontFamily = "Courier New";
                    labelAttributes.FontSize = 12;
                    labelAttributes.Stroke = "black";
                    labelAttributes.Top = top + 10;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);
                    
                    label.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                    label.Text = (bayInternalSpacerHeight * i) + bayInternalSpacerHeightUOM;
                    labelAttributes.Left = left + TemplateScale.ConvertPhysicalDimensionToPixels(bayInternalSpec.Width) - (label.Text.Length * labelAttributes.FontSize * textBuffer);
                    label.Attributes = labelAttributes;

                    bayInternalSpacerAttributes.CanDrag = true; // false;
                    bayInternalSpacerAttributes.CanResize = true; // false;
                    bayInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight);
                    bayInternalSpacerAttributes.IsSelectable = true; // false;
                    bayInternalSpacerAttributes.Left = left;
                    bayInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                    bayInternalSpacerAttributes.StrokeWidth = 1;
                    bayInternalSpacerAttributes.Top = top;
                    bayInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(bayInternalSpec.Width);
                    bayInternalSpacerAttributes.Fill = "white";
                    //bayInternalSpacerAttributes.ScaleX = 1;
                    //bayInternalSpacerAttributes.ScaleY = 1;

                    bayInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY_SPACER;
                    bayInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                    bayInternalSpacer.TemplateId = TemplateId;
                    bayInternalSpacer.Attributes = bayInternalSpacerAttributes;

                    group.AddDrawingObject(bayInternalSpacer);
                    group.AddDrawingObject(label);

                    top = top - TemplateScale.ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight);
                }

                group.CanDrag = true; // false;
                group.CanResize = true; // false;
                group.Category = Utility.TemplateType.Category.BASE;
                group.HoverCursor = "default";
                group.IsSelectable = true; // false;
                group.SpecificationRevisionId = SpecificationRevisionId;
                group.TemplateId = TemplateId;
                group.Type = Utility.TemplateType.Type.BAY;
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