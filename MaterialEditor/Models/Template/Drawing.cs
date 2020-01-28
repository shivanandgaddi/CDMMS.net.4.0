using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using NLog;
using Newtonsoft.Json;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class Drawing
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private List<Group> groupings = null;

        public Drawing()
        {

        }

        public List<Group> Groupings
        {
            get
            {
                return groupings;
            }
        }

        public void AddGroup(Group group)
        {
            if (groupings == null)
                groupings = new List<Group>();

            groupings.Add(group);
        }
    }

    public class Group
    {
        private TemplateType.Type type = Utility.TemplateType.Type.NOT_SET;
        private TemplateType.Category category = Utility.TemplateType.Category.BASE;
        private List<DrawingObject> drawingObjects = null;
        private List<string> drawingObjectIdList = null;
        private string uniqueId = "";

        public Group()
        {
            uniqueId = GetDrawingObjectUniqueId();
        }

        [JsonProperty(Utility.TemplateType.JSON.TemplateId)]
        public long TemplateId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.BaseTemplateId)]
        public long BaseTemplateId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.SpecificationRevisionId)]
        public long SpecificationRevisionId
        {
            get;
            set;
        }        

        [JsonProperty(Utility.TemplateType.JSON.Type)]
        public string TemplateType
        {
            get
            {
                return type.ToString();
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.UniqueId)]
        public string UniqueId
        {
            get
            {
                return uniqueId;
            }
        }

        [JsonIgnore]
        public TemplateType.Type Type
        {
            set
            {
                type = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.TemplateCategory)]
        public string TemplateCategory
        {
            get
            {
                return category.ToString();
            }
        }

        [JsonIgnore]
        public TemplateType.Category Category
        {
            set
            {
                category = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.HoverCursor)]
        public string HoverCursor
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.LockMovementX)]
        public string LockMovementX
        {
            get
            {
                if (CanDrag)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.LockMovementY)]
        public string LockMovementY
        {
            get
            {
                if (CanDrag)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.LockUniScaling)]
        public string LockUniScaling
        {
            get
            {
                if (CanResize)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Selectable)]
        public string Selectable
        {
            get
            {
                if (IsSelectable)
                    return "true";
                else
                    return "false";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.CanBeMoved)]
        public bool CanDrag
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool CanResize
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool IsSelectable
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Objects)]
        public List<DrawingObject> DrawingObjects
        {
            get
            {
                return drawingObjects;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Attributes)]
        public ObjectAttribute Attributes
        {
            get;
            set;
        }

        public void AddDrawingObject(DrawingObject drawingObject)
        {
            if (drawingObjects == null)
                drawingObjects = new List<DrawingObject>();

            drawingObject.UniqueId = GetDrawingObjectUniqueId();

            drawingObjects.Add(drawingObject);
        }

        public string GetDrawingObjectUniqueId()
        {
            string uniqueId = "";
            Random generator = new Random();

            if(drawingObjectIdList == null)
            {
                drawingObjectIdList = new List<string>();

                uniqueId = generator.Next(1, 10000).ToString("D4");
            }
            else
            {
                do
                {
                    uniqueId = generator.Next(1, 10000).ToString("D4");
                }
                while (drawingObjectIdList.Contains(uniqueId));
            }

            drawingObjectIdList.Add(uniqueId);

            return uniqueId;
        }
    }

    public class DrawingObject
    {
        private TemplateType.DrawingObjectType fabricObjectType;
        private TemplateType.ActualObjectType actualObjectType = TemplateType.ActualObjectType.NOT_SET;
        private string uniqueId = "";

        public DrawingObject()
        {

        }

        public DrawingObject(string id)
        {
            uniqueId = id;
        }

        [JsonProperty(Utility.TemplateType.JSON.TemplateId)]
        public long TemplateId
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Text)]
        public string Text
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Type)]
        public string Type
        {
            get
            {
                return fabricObjectType.ToString();
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.ActualObjectType)]
        public string ObjectType
        {
            get
            {
                return actualObjectType.ToString();
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.UniqueId)]
        public string UniqueId
        {
            get
            {
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.AddUpdateDelete)]
        public string AddUpdateDeleteIndicator
        {
            get;
            set;
        }

        [JsonIgnore]
        public TemplateType.DrawingObjectType FabricObjectType
        {
            set
            {
                fabricObjectType = value;
            }
        }

        [JsonIgnore]
        public TemplateType.ActualObjectType ActualObjectType
        {
            set
            {
                actualObjectType = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Attributes)]
        public ObjectAttribute Attributes
        {
            get;
            set;
        }
    }

    public class ObjectAttribute
    {
        private decimal scaleX = 1;
        private decimal scaleY = 1;

        public ObjectAttribute()
        {

        }

        [JsonProperty(Utility.TemplateType.JSON.Radius)]
        public decimal Radius
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Left)]
        public decimal Left
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Top)]
        public decimal Top
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.X)]
        public decimal X
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Y)]
        public decimal Y
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Width)]
        public decimal Width
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Height)]
        public decimal Height
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.StrokeWidth)]
        public decimal StrokeWidth
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.ScaleX)]
        public decimal ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {
                scaleX = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.ScaleY)]
        public decimal ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                scaleY = value;
            }
        }

        [JsonIgnore]
        public decimal Scale
        {
            set
            {
                scaleX = value;
                scaleY = value;
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.FontSize)]
        public int FontSize
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Fill)]
        public string Fill
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.Stroke)]
        public string Stroke
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.FontFamily)]
        public string FontFamily
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.HoverCursor)]
        public string HoverCursor
        {
            get;
            set;
        }

        [JsonProperty(Utility.TemplateType.JSON.LockMovementX)]
        public string LockMovementX
        {
            get
            {
                if (CanDrag)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.LockMovementY)]
        public string LockMovementY
        {
            get
            {
                if (CanDrag)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.LockUniScaling)]
        public string LockUniScaling
        {
            get
            {
                if (CanResize)
                    return "false";
                else
                    return "true";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.Selectable)]
        public string Selectable
        {
            get
            {
                if (IsSelectable)
                    return "true";
                else
                    return "false";
            }
        }

        [JsonProperty(Utility.TemplateType.JSON.CanBeMoved)]
        public bool CanDrag
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool CanResize
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool IsSelectable
        {
            get;
            set;
        }
    }
}