using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class Slot
    {
        public Slot()
        {

        }

        public long SlotSpecificationId
        {
            get;
            set;
        }

        public long SlotsSlotsId
        {
            get;
            set;
        }

        public decimal Height
        {
            get;
            set;
        }

        public decimal Width
        {
            get;
            set;
        }

        public decimal X
        {
            get;
            set;
        }

        public decimal Y
        {
            get;
            set;
        }

        public int SlotNumber
        {
            get;
            set;
        }

        public string LabelName
        {
            get;
            set;
        }

        public string UnitOfMeasure
        {
            get;
            set;
        }
    }
}