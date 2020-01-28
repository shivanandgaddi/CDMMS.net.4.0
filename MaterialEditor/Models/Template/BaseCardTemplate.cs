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
    public class BaseCardTemplate : BaseTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ITemplateDbInterface cardTemplateDbInterface = null;
        private ISpecificationDbInterface cardSpecDbInterface = null;
        private Drawing drawing = null;
        private Dictionary<long, Dictionary<string, string>> cardSlotList = new Dictionary<long, Dictionary<string, string>>();
        private Dictionary<long, Dictionary<string, string>> cardPortList = new Dictionary<long, Dictionary<string, string>>();
        private long tempId = 0;
        public BaseCardTemplate() : base()
        {
            InitializeAttributes();
        }

        public BaseCardTemplate(long templateId) : base(templateId)
        {
            tempId = templateId;
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
                return Utility.TemplateType.Type.CARD;
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
                if (cardTemplateDbInterface == null)
                    cardTemplateDbInterface = new CardTemplateDbInterface();

                return cardTemplateDbInterface;
            }
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long specId = long.Parse((string)tmpltValues["pSpecId"]);

            cardTemplateDbInterface = new CardTemplateDbInterface();

            tmpltId = ((CardTemplateDbInterface)cardTemplateDbInterface).CreateBaseTemplate(specId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"]);

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
                    cardTemplateDbInterface = new CardTemplateDbInterface();

                    cardTemplateDbInterface.StartTransaction();

                    ((TemplateDbInterfaceImpl)cardTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, true, IsHighLevelPartTemplate,
                        IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);
                }

                if (cardTemplateDbInterface != null)
                    cardTemplateDbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (cardTemplateDbInterface != null)
                    cardTemplateDbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (cardTemplateDbInterface != null)
                    cardTemplateDbInterface.Dispose();
            }
        }


        public override void InitializeAttributes()
        {
            cardTemplateDbInterface = new CardTemplateDbInterface();
            cardSlotList = ((CardTemplateDbInterface)cardTemplateDbInterface).getCardWithSlotDtls(tempId);
            cardPortList = ((CardTemplateDbInterface)cardTemplateDbInterface).getCardWithPortDtls(tempId);            
        }

        public override void CreateDrawing(bool isForEdit)
        {
            Group group = new Group();
            DrawingObject card = new DrawingObject();
            DrawingObject cardSlot = new DrawingObject();
            DrawingObject cardInternalSpacer = null;
            DrawingObject slotInternalSpacer = null;
            ObjectAttribute cardInternalSpacerAttributes = null;
            ObjectAttribute slotInternalSpacerAttributes = null;

            ObjectAttribute cardAttributes = new ObjectAttribute();
            ObjectAttribute slotAttributes = new ObjectAttribute();

            //decimal extrnDpth = 0;
            decimal extrnHgth = 0;
            decimal extrnWdth = 0;

            cardSpecDbInterface = new CardSpecificationDbInterface();

            ObjectAttribute groupAttributes = new ObjectAttribute();

            long plgInRoleID = SpecificationId;
            decimal left = 0;
            decimal top = 0;
            decimal textBuffer = 0.66M;

            try
            {
                if (TemplateScale == null)
                    TemplateScale = new Scale(50, 50, "50"); //cardInternalSpacerHeightUOM

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
                cardAttributes.CanDrag = true; // false;
                cardAttributes.CanResize = true; // false;
                cardAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(20);
                cardAttributes.IsSelectable = true; // false;
                cardAttributes.Left = left + 30;
                cardAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                cardAttributes.StrokeWidth = 5; // .5M;
                cardAttributes.Top = top;
                cardAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(30);
                cardAttributes.Fill = "white";
                cardAttributes.ScaleX = 1;
                cardAttributes.ScaleY = 1;

                card.ActualObjectType = Utility.TemplateType.ActualObjectType.CARD;
                card.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                card.TemplateId = TemplateId;
                card.Attributes = cardAttributes;
                group.AddDrawingObject(card);

                //Slot Text holding outside rectangle
                //slotAttributes.CanDrag = true; // false;
                //slotAttributes.CanResize = true; // false;
                //slotAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(15);
                //slotAttributes.IsSelectable = true; // false;
                //slotAttributes.Left = left + 200;
                //slotAttributes.Stroke = "red"; // rgba(0,0,0,1)";
                //slotAttributes.StrokeWidth = 5; // .5M;
                //slotAttributes.Top = top + 30;
                //slotAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(15);
                //slotAttributes.Fill = "white";
                //slotAttributes.ScaleX = 1;
                //slotAttributes.ScaleY = 1;

                cardSlot.ActualObjectType = Utility.TemplateType.ActualObjectType.SLOT;
                cardSlot.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                cardSlot.TemplateId = TemplateId;
                cardSlot.Attributes = slotAttributes;
                group.AddDrawingObject(cardSlot);

                if (extrnWdth < extrnHgth)
                    left += TemplateScale.ConvertPhysicalDimensionToPixels((extrnWdth - extrnWdth) / 2);

                // top += TemplateScale.ConvertPhysicalDimensionToPixels((extrnHgth - 2) - ((BaySpecification)AssociatedSpecification).MountingPositionOffset);
                top += TemplateScale.ConvertPhysicalDimensionToPixels(10);

                //-------------------------------
                for (int i = 0; i < 10; i++)
                {
                    DrawingObject label = new DrawingObject();
                    ObjectAttribute labelAttributes = new ObjectAttribute();

                    cardInternalSpacer = new DrawingObject();
                    cardInternalSpacerAttributes = new ObjectAttribute();

                    labelAttributes.FontFamily = "Courier New";
                    labelAttributes.FontSize = 12;
                    labelAttributes.Stroke = "black";
                    labelAttributes.Top = top + 9;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);

                    label.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                    label.Text = i + " CARD";
                    labelAttributes.Left = left + TemplateScale.ConvertPhysicalDimensionToPixels(5) - (label.Text.Length * labelAttributes.FontSize * textBuffer);
                    label.Attributes = labelAttributes;
                    //Text holding main small rectngle
                    cardInternalSpacerAttributes.CanDrag = true; // false;
                    cardInternalSpacerAttributes.CanResize = true; // false;
                    cardInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(2);
                    cardInternalSpacerAttributes.IsSelectable = true; // false;
                    cardInternalSpacerAttributes.Left = left + 50;
                    cardInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                    cardInternalSpacerAttributes.StrokeWidth = 1;
                    cardInternalSpacerAttributes.Top = top + 8;
                    cardInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(4);
                    cardInternalSpacerAttributes.Fill = "";
                    //cardInternalSpacerAttributes.ScaleX = 1;
                    //cardInternalSpacerAttributes.ScaleY = 1;

                    cardInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.CARD;
                    cardInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                    cardInternalSpacer.TemplateId = TemplateId;
                    cardInternalSpacer.Attributes = cardInternalSpacerAttributes;

                    group.AddDrawingObject(cardInternalSpacer);
                    group.AddDrawingObject(label);

                    top = top - TemplateScale.ConvertPhysicalDimensionToPixels(1);
                }

                for (int i = 0; i < 4; i++)    // i < cardSlotList.count
                {
                    DrawingObject slotlabel = new DrawingObject();
                    ObjectAttribute slotlabelAttributes = new ObjectAttribute();

                    slotInternalSpacer = new DrawingObject();
                    slotInternalSpacerAttributes = new ObjectAttribute();

                    slotlabelAttributes.FontFamily = "Courier New";
                    slotlabelAttributes.FontSize = 12;
                    slotlabelAttributes.Stroke = "black";
                    slotlabelAttributes.Top = top + 105;// - ConvertPhysicalDimensionToPixels(bayInternalSpacerHeight / 2);

                    slotlabel.FabricObjectType = Utility.TemplateType.DrawingObjectType.TEXT;
                    slotlabel.Text = i + " SLOT";
                    slotlabelAttributes.Left = left + 230;
                    slotlabel.Attributes = slotlabelAttributes;
                    //Text holding main small rectngle
                    slotInternalSpacerAttributes.CanDrag = true; // false;
                    slotInternalSpacerAttributes.CanResize = true; // false;                   

                    slotInternalSpacerAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(2);
                    slotInternalSpacerAttributes.IsSelectable = true; // false;
                    slotInternalSpacerAttributes.Left = left + 220;
                    slotInternalSpacerAttributes.Stroke = "green"; // "rgba(0,0,0,1)";
                    slotInternalSpacerAttributes.StrokeWidth = 1;
                    slotInternalSpacerAttributes.Top = top + 100;
                    slotInternalSpacerAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(4);
                    slotInternalSpacerAttributes.Fill = "";
                    //cardInternalSpacerAttributes.ScaleX = 1;
                    //cardInternalSpacerAttributes.ScaleY = 1;

                    slotInternalSpacer.ActualObjectType = Utility.TemplateType.ActualObjectType.SLOT;
                    slotInternalSpacer.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                    slotInternalSpacer.TemplateId = TemplateId;
                    slotInternalSpacer.Attributes = slotInternalSpacerAttributes;

                    group.AddDrawingObject(slotInternalSpacer);
                    group.AddDrawingObject(slotlabel);

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
                group.Type = Utility.TemplateType.Type.CARD;
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