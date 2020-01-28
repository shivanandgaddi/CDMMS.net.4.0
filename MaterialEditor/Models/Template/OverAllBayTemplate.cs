using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class OverAllBayTemplate : OverAllTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Drawing drawing = null;
        private ITemplateDbInterface bayTemplateDbInterface = null;
        private ISpecificationDbInterface baySpecDbInterface = null;
        private TemplateAttribute bayChoices = null;

        public OverAllBayTemplate() : base()
        {
            InitializeAttributes();
        }

        public OverAllBayTemplate(long templateId) : base(templateId)
        {
            InitializeAttributes();
        }

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplateId)]
        public long BaseTemplateId
        {
            get;
            set;
        }      
        public long rtnAnglDgrNo
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.BayExtndrSpecnRevsnAltId)]
        public long BayExtndrSpecnRevsnAltId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationRevisionId)]
        public override long SpecificationRevisionId
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.SpecificationRevisionId;
                else
                    return 0;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.SpecificationRevisionId = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationId)]
        public override long SpecificationId
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.SpecificationId;
                else
                    return 0;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.SpecificationId = value;
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

        [JsonProperty(Utility.TemplateType.JSON.AssociatedSpecification)]
        public override ISpecification AssociatedSpecification
        {
            get
            {
                if (TemplateBase != null)
                    return TemplateBase.AssociatedSpecification;
                else
                    return null;
            }
            set
            {
                if (TemplateBase != null)
                    TemplateBase.AssociatedSpecification = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.BayChoices)]
        public new TemplateAttribute BayChoices
        {
            get
            {
                if (bayChoices.Options == null)
                {
                    bayChoices.Options = GetOptions(Utility.TemplateType.JSON.BayChoices);
                    bayChoices.Options[0].Value = "0";
                    SetBayChoicesId(0);
                }

                return bayChoices;
            }
        }

        public void SetBayChoicesId(int id)
        {
            if (bayChoices != null)
            {
                if (id == 0)
                {
                    bayChoices.Value = "";
                }
                else
                {
                    bayChoices.Value = id.ToString();
                }
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

        //public override decimal ConvertPhysicalDimensionToPixels(decimal dimension)
        //{
        //    throw new NotImplementedException();
        //}

        //public override decimal ConvertPixelsToPhysicalDimension(decimal pixels)
        //{
        //    throw new NotImplementedException();
        //}

        public override void CreateDrawing(bool isForEdit)
        {
            //throw new NotImplementedException();

            //Group group = new Group();
            //DrawingObject bay = new DrawingObject();
            //DrawingObject bayExtender = new DrawingObject();
            //DrawingObject bayInternalSpacer = null;
            ////DrawingObject bayInternalSpacer = null;
            //ObjectAttribute bayInternalSpacerAttributes = null;
            ////ObjectAttribute bayInternalSpacerAttributes = null;

            //ObjectAttribute cardAttributes = new ObjectAttribute();
            //ObjectAttribute slotAttributes = new ObjectAttribute();

            ////decimal extrnDpth = 0;
            //decimal extrnHgth = 0;
            //decimal extrnWdth = 0;

            //baySpecDbInterface = new BaySpecificationDbInterface();

            //ObjectAttribute groupAttributes = new ObjectAttribute();

            //long plgInRoleID = SpecificationId;
            //decimal left = 0;
            //decimal top = 0;
            //decimal textBuffer = 0.66M;

            try
            {
                //if (TemplateScale == null)
                //    TemplateScale = new Scale(50, 50, "50"); //bayInternalSpacerHeightUOM

                //top = TemplateScale.Top;
                //left = TemplateScale.Left;

                drawing = new Drawing();
                // First Rectangle inside the canvas
                //groupAttributes.CanDrag = true; // false;
                //groupAttributes.CanResize = true; // false;
                //groupAttributes.Top = top + 50;
                //groupAttributes.Left = left;
                //groupAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(20);
                //groupAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(30);
                ////groupAttributes.Scale = scale;
                //groupAttributes.FontFamily = "Courier New";
                //groupAttributes.FontSize = 12;

                //// Text holding outside rectangle
                //cardAttributes.CanDrag = true; // false;
                //cardAttributes.CanResize = true; // false;
                //cardAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(20);
                //cardAttributes.IsSelectable = true; // false;
                //cardAttributes.Left = left + 30;
                //cardAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                //cardAttributes.StrokeWidth = 5; // .5M;
                //cardAttributes.Top = top;
                //cardAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(30);
                //cardAttributes.Fill = "white";
                //cardAttributes.ScaleX = 1;
                //cardAttributes.ScaleY = 1;

                //bay.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY;
                //bay.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                //bay.TemplateId = TemplateId;
                //bay.Attributes = cardAttributes;
                //group.AddDrawingObject(bay);

                ////Slot Text holding outside rectangle
                ////slotAttributes.CanDrag = true; // false;
                ////slotAttributes.CanResize = true; // false;
                ////slotAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(15);
                ////slotAttributes.IsSelectable = true; // false;
                ////slotAttributes.Left = left + 200;
                ////slotAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                ////slotAttributes.StrokeWidth = 5; // .5M;
                ////slotAttributes.Top = top + 30;
                ////slotAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(15);
                ////slotAttributes.Fill = "white";
                ////slotAttributes.ScaleX = 1;
                ////slotAttributes.ScaleY = 1;

                //bayExtender.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY_EXTENDER;
                //bayExtender.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                //bayExtender.TemplateId = TemplateId;
                //bayExtender.Attributes = slotAttributes;
                //group.AddDrawingObject(bayExtender);

                //if (extrnWdth < extrnHgth)
                //    left += TemplateScale.ConvertPhysicalDimensionToPixels((extrnWdth - extrnWdth) / 2);

                //// top += TemplateScale.ConvertPhysicalDimensionToPixels((extrnHgth - 2) - ((BaySpecification)AssociatedSpecification).MountingPositionOffset);
                //top += TemplateScale.ConvertPhysicalDimensionToPixels(10);

                ////-------------------------------
                //for (int i = 0; i < 10; i++)
                //{
                //    DrawingObject label = new DrawingObject();
                //    ObjectAttribute labelAttributes = new ObjectAttribute();

                //    bayInternalSpacer = new DrawingObject();
                //    bayInternalSpacerAttributes = new ObjectAttribute();

                //    labelAttributes.FontFamily = "Courier New";
                //    labelAttributes.FontSize = 12;
                //    labelAttributes.Stroke = "black";
                //    labelAttributes.Top = top + 9;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);

                //    label.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                //    label.Text = i + " Bay";
                //    labelAttributes.Left = left + TemplateScale.ConvertPhysicalDimensionToPixels(5) - (label.Text.Length * labelAttributes.FontSize * textBuffer);
                //    label.Attributes = labelAttributes;
                //    //Text holding main small rectngle
                //    bayInternalSpacerAttributes.CanDrag = true; // false;
                //    bayInternalSpacerAttributes.CanResize = true; // false;
                //    bayInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(2);
                //    bayInternalSpacerAttributes.IsSelectable = true; // false;
                //    bayInternalSpacerAttributes.Left = left + 50;
                //    bayInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                //    bayInternalSpacerAttributes.StrokeWidth = 1;
                //    bayInternalSpacerAttributes.Top = top + 8;
                //    bayInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(4);
                //    bayInternalSpacerAttributes.Fill = "";
                //    //bayInternalSpacerAttributes.ScaleX = 1;
                //    //bayInternalSpacerAttributes.ScaleY = 1;

                //    bayInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY_SPACER;
                //    bayInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                //    bayInternalSpacer.TemplateId = TemplateId;
                //    bayInternalSpacer.Attributes = bayInternalSpacerAttributes;

                //    group.AddDrawingObject(bayInternalSpacer);
                //    group.AddDrawingObject(label);

                //    top = top - TemplateScale.ConvertPhysicalDimensionToPixels(1);
                //}

                //for (int i = 0; i < 4; i++)    // i < cardSlotList.count
                //{
                //    DrawingObject baylabel = new DrawingObject();
                //    ObjectAttribute baylabelAttributes = new ObjectAttribute();

                //    bayInternalSpacer = new DrawingObject();
                //    bayInternalSpacerAttributes = new ObjectAttribute();

                //    baylabelAttributes.FontFamily = "Courier New";
                //    baylabelAttributes.FontSize = 12;
                //    baylabelAttributes.Stroke = "black";
                //    baylabelAttributes.Top = top + 105;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);

                //    baylabel.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                //    baylabel.Text = i + " SLOT";
                //    baylabelAttributes.Left = left + 230;
                //    baylabel.Attributes = baylabelAttributes;
                //    //Text holding main small rectngle
                //    bayInternalSpacerAttributes.CanDrag = true; // false;
                //    bayInternalSpacerAttributes.CanResize = true; // false;                   

                //    bayInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(2);
                //    bayInternalSpacerAttributes.IsSelectable = true; // false;
                //    bayInternalSpacerAttributes.Left = left + 220;
                //    bayInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                //    bayInternalSpacerAttributes.StrokeWidth = 1;
                //    bayInternalSpacerAttributes.Top = top + 100;
                //    bayInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(4);
                //    bayInternalSpacerAttributes.Fill = "";
                //    //cardInternalSpacerAttributes.ScaleX = 1;
                //    //cardInternalSpacerAttributes.ScaleY = 1;

                //    bayInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.BAY_INTERNAL;
                //    bayInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                //    bayInternalSpacer.TemplateId = TemplateId;
                //    bayInternalSpacer.Attributes = bayInternalSpacerAttributes;

                //    group.AddDrawingObject(bayInternalSpacer);
                //    group.AddDrawingObject(baylabel);

                //    top = top - TemplateScale.ConvertPhysicalDimensionToPixels(1);
                //}
                ////--------------------------------
                //group.CanDrag = true; // false;
                //group.CanResize = true; // false;
                //group.Category = Utility.TemplateType.Category.OVERALL;
                //group.HoverCursor = "default";
                //group.IsSelectable = true; // false;
                //group.SpecificationRevisionId = SpecificationRevisionId;
                //group.TemplateId = TemplateId;
                //group.Type = Utility.TemplateType.Type.BAY;
                //group.BaseTemplateId = TemplateId;
                //group.Attributes = groupAttributes;

                drawing.AddGroup(null);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create drawing: {0}", ex.Message);
            }
        }

        public override void InitializeAttributes()
        {
            bayChoices = new TemplateAttribute(Utility.TemplateType.JSON.BayChoices);
            //throw new NotImplementedException();
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long baseTmpltId = 0;

            bayTemplateDbInterface = new BayTemplateDbInterface();

            if (long.TryParse((string)tmpltValues["pBaseTmpltId"], out baseTmpltId))
                tmpltId = ((BayTemplateDbInterface)bayTemplateDbInterface).CreateOverAllTemplate(baseTmpltId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"], (string)tmpltValues["cuid"]);
            else
            {
                logger.Error("Unable to persist overall template, cannot parse base template id from parameters.");

                tmpltId = -1;
            }

            return tmpltId;
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

            string TemplateSpecId = JsonHelper.GetStringValue(Utility.TemplateType.JSON.SpecificationId, updatedTmplt);

            long BaseTemplateId = JsonHelper.GetLongValue("BaseTemplateID", updatedTmplt);
            long MaterialCatId = JsonHelper.GetLongValue("MaterialCatID", updatedTmplt);
            long FeatTypeId = JsonHelper.GetLongValue("FeatureTypeID", updatedTmplt);
            string UserCuid = JsonHelper.GetStringValue("CUID", updatedTmplt);
            long BayExtndrSpecnRevsnAltId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.BayExtndrSpecnRevsnAltId, updatedTmplt);

            //int updatedRotationAngleId = 0;
            long rtnAngleId = JsonHelper.GetLongValue("RotationAngleID", updatedTmplt);

            try
            {
                //if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
                //    !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
                //    !UpdateInProgress.Equals(updatedInProgress))
                //{
                    bayTemplateDbInterface = new BayTemplateDbInterface();

                    bayTemplateDbInterface.StartTransaction();

                    ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateTemplate(updatedName
                            , updatedDescription
                            , false // BaseTemplateInd
                            , hlpTmpltInd
                            , comnCnfgTmpltInd
                            , updatedCompleted
                            , updatedPropagated
                            , updatedInProgress
                            , updatedRetired
                            , updatedDeleted
                            , TemplateId
                            );

                    long resval = ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateOverAllTemplate(TemplateId, BaseTemplateId, MaterialCatId, FeatTypeId, UserCuid, rtnAngleId, BayExtndrSpecnRevsnAltId);
                    if (resval > 0)
                    {
                        return;
                    }

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
        //    //int updatedRotationAngleId = 0;
        //    bool updatedCompleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Completed, updatedTmplt);
        //    bool updatedPropagated = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Propagated, updatedTmplt);
        //    bool updatedDeleted = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Deleted, updatedTmplt);
        //    bool updatedRetired = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.Retired, updatedTmplt);
        //    bool updatedInProgress = JsonHelper.GetBoolValue(Utility.TemplateType.JSON.UpdateInProgress, updatedTmplt);
        //    string TemplateSpecId = JsonHelper.GetStringValue(Utility.TemplateType.JSON.SpecificationId, updatedTmplt);

        //    long BaseTemplateId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.BaseTemplateId, updatedTmplt);
        //    long MaterialCatId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.MtrlCatId, updatedTmplt);
        //    long FeatTypeId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.FeatTypId, updatedTmplt);
        //    string UserCuid = JsonHelper.GetStringValue(Utility.TemplateType.JSON.UserCuid, updatedTmplt);            
        //    long BayExtndrSpecnRevsnAltId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.BayExtndrSpecnRevsnAltId, updatedTmplt);

        //    long rtnAngleId = JsonHelper.GetLongValue(Utility.TemplateType.JSON.RtnAnglId,updatedTmplt);          


        //    try
        //    {
        //        if (!Name.Equals(updatedName) || !Description.Equals(updatedDescription) || !IsCompleted.Equals(updatedCompleted) ||
        //            !IsPropagated.Equals(updatedPropagated) || !IsDeleted.Equals(updatedDeleted) || !IsRetired.Equals(updatedRetired) ||
        //            !UpdateInProgress.Equals(updatedInProgress))
        //        {
        //            bayTemplateDbInterface = new BayTemplateDbInterface();

        //            bayTemplateDbInterface.StartTransaction();

        //            ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, false, IsHighLevelPartTemplate,
        //                IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);

        //            long resval = ((BayTemplateDbInterface)bayTemplateDbInterface).UpdateOverAllTemplate(TemplateId, BaseTemplateId, MaterialCatId, FeatTypeId, UserCuid, rtnAngleId, BayExtndrSpecnRevsnAltId);
        //            if (resval > 0)
        //            {
        //                return;
        //            }

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
    }
}