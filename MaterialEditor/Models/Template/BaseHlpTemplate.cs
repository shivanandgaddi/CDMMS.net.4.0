using System;
using System.Collections;
using System.Collections.Generic;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class BaseHlpTemplate : BaseTemplate
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ITemplateDbInterface hlpTemplateDbInterface = null;
        private TemplateAttribute rotationAngleAttribute = null;
        private Drawing drawing = null;

        public BaseHlpTemplate() : base()
        {
            InitializeAttributes();
        }

        public BaseHlpTemplate(long templateId) : base(templateId)
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
                return Utility.TemplateType.Type.HIGH_LEVEL_PART;
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
                if (hlpTemplateDbInterface == null)
                    hlpTemplateDbInterface = new HlpTemplateDbInterface();

                return hlpTemplateDbInterface;
            }
        }

        public override long PersistTemplate(Hashtable tmpltValues)
        {
            long tmpltId = 0;
            long specRvnsId = long.Parse((string)tmpltValues["pSpecId"]);

            hlpTemplateDbInterface = new HlpTemplateDbInterface();

            tmpltId = ((HlpTemplateDbInterface)hlpTemplateDbInterface).CreateBaseTemplate(specRvnsId, (string)tmpltValues["pTmpName"], (string)tmpltValues["pTmpDesc"]);

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
            //int updatedRotationAngleId = 0;
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
                    hlpTemplateDbInterface = new HlpTemplateDbInterface();

                    hlpTemplateDbInterface.StartTransaction();

                    ((TemplateDbInterfaceImpl)hlpTemplateDbInterface).UpdateTemplate(updatedName, updatedDescription, true, IsHighLevelPartTemplate,
                        IsCommonConfigTemplate, updatedCompleted, updatedPropagated, updatedInProgress, updatedRetired, updatedDeleted, TemplateId);
                }              

                if (hlpTemplateDbInterface != null)
                    hlpTemplateDbInterface.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (hlpTemplateDbInterface != null)
                    hlpTemplateDbInterface.RollbackTransaction();

                throw ex;
            }
            finally
            {
                if (hlpTemplateDbInterface != null)
                    hlpTemplateDbInterface.Dispose();
            }
        }

        public override void InitializeAttributes()
        {
            
        }

        public override void CreateDrawing(bool isForEdit)
        {
            List<Slot> slots = ((HlpTemplateDbInterface)DbInterface).GetSlots(SpecificationId);
            Group group = new Group();
            DrawingObject shelf = new DrawingObject();
            ObjectAttribute groupAttributes = new ObjectAttribute();
            ObjectAttribute shelfAttributes = new ObjectAttribute();
            Shelf shelfMaterial = (Shelf)AssociatedSpecification.AssociatedMaterial;
            decimal left = 0;
            decimal top = 0;

            try
            {
                if (TemplateScale == null)
                    TemplateScale = new Scale(shelfMaterial.Height, shelfMaterial.Width, "in"); //TODO remove hardcoded "in"

                left = TemplateScale.Left;
                top = TemplateScale.Top;

                drawing = new Drawing();

                groupAttributes.CanDrag = false;
                groupAttributes.CanResize = false;
                groupAttributes.Top = top;
                groupAttributes.Left = left;
                groupAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(shelfMaterial.Height);
                groupAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(shelfMaterial.Width);
                groupAttributes.FontFamily = "Courier New";
                groupAttributes.FontSize = 12;

                shelfAttributes.Top = top;
                shelfAttributes.Left = left;
                shelfAttributes.Height = TemplateScale.ConvertPhysicalDimensionToPixels(shelfMaterial.Height);
                shelfAttributes.Width = TemplateScale.ConvertPhysicalDimensionToPixels(shelfMaterial.Width);
                shelfAttributes.Stroke = "red";
                shelfAttributes.Fill = "white";

                shelf.ActualObjectType = Utility.TemplateType.ActualObjectType.SHELF;
                shelf.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                shelf.TemplateId = TemplateId;
                shelf.Attributes = shelfAttributes;

                group.AddDrawingObject(shelf);

                if (slots != null)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        DrawingObject slot = new DrawingObject();
                        ObjectAttribute slotAttribute = new ObjectAttribute();

                        slotAttribute.Fill = "white";
                        slotAttribute.Height = TemplateScale.ConvertPhysicalDimensionToPixels(slots[i].Height);
                        slotAttribute.Left = left + TemplateScale.ConvertPhysicalDimensionToPixels(slots[i].X);
                        slotAttribute.Stroke = "green";
                        slotAttribute.Top = top + TemplateScale.ConvertPhysicalDimensionToPixels(slots[i].Y);
                        slotAttribute.Width = TemplateScale.ConvertPhysicalDimensionToPixels(slots[i].Width);

                        slot.ActualObjectType = Utility.TemplateType.ActualObjectType.SLOT;
                        slot.FabricObjectType = Utility.TemplateType.DrawingObjectType.RECTANGLE;
                        slot.TemplateId = TemplateId;
                        slot.Attributes = slotAttribute;

                        group.AddDrawingObject(slot);
                    }
                }

                group.CanDrag = true; // false;
                group.CanResize = true; // false;
                group.Category = Utility.TemplateType.Category.BASE;
                group.HoverCursor = "default";
                group.IsSelectable = true; // false;
                group.SpecificationRevisionId = SpecificationRevisionId;
                group.TemplateId = TemplateId;
                group.Type = Utility.TemplateType.Type.SHELF;
                group.BaseTemplateId = TemplateId;
                group.Attributes = groupAttributes;

                drawing.AddGroup(group);
            }
            catch(Exception ex)
            {
                logger.Error("Failed to create drawing: {0}", ex.Message);
            }
        }

        //public override decimal ConvertPhysicalDimensionToPixels(decimal dimension)
        //{
        //    //TODO: account for all units, not just inches
        //    //1 inch = 27 pixels
        //    decimal pixelsPerInch = 27;

        //    return pixelsPerInch * dimension;
        //}

        //public override decimal ConvertPixelsToPhysicalDimension(decimal pixels)
        //{
        //    throw new NotImplementedException();
        //}
    }
}