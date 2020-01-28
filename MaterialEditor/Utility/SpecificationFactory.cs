using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Specification;
using CenturyLink.Network.Engineering.Material.Editor.Models.Specification;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class SpecificationFactory
    {
        private SpecificationFactory()
        {
        }

        public static ISpecification GetSpecificationInstance(long specificationId, SpecificationType.Type specType)
        {
            ISpecification specification = null;
            ISpecificationDbInterface dbInterface = null;

            switch (specType)
            {
                case SpecificationType.Type.BAY:
                    dbInterface = new BaySpecificationDbInterface();

                    break;
                case SpecificationType.Type.BAY_EXTENDER:
                    dbInterface = new BayExtenderSpecificationDbInterface();

                    break;
                case SpecificationType.Type.BAY_INTERNAL:
                    dbInterface = new BayInternalDbInterface();

                    break;
                case SpecificationType.Type.PLUG_IN:
                    dbInterface = new PlugInSpecificationDbInterface();

                    break;
                case SpecificationType.Type.NODE:
                    dbInterface = new NodeSpecificationDbInterface();

                    break;
                case SpecificationType.Type.CARD:
                    dbInterface = new CardSpecificationDbInterface();

                    break;
                case SpecificationType.Type.SHELF:
                    dbInterface = new ShelfSpecificationDbInterface();

                    break;
                case SpecificationType.Type.SLOT:
                    dbInterface = new SlotSpecificationDbInterface();

                    break;
                case SpecificationType.Type.PORT:
                    dbInterface = new PortSpecificationDbInterface();

                    break;
                default:
                    break;
            }

            if (specification == null && dbInterface != null)
                specification = dbInterface.GetSpecification(specificationId);

            return specification;
        }

        public static ISpecification GetSpecificationClass(SpecificationType.Type specType)
        {
            ISpecification specification = null;

            switch (specType)
            {
                case SpecificationType.Type.BAY:
                    specification = new BaySpecification();

                    break;
                case SpecificationType.Type.BAY_EXTENDER:
                    specification = new BayExtenderSpecification();

                    break;
                case SpecificationType.Type.BAY_INTERNAL:
                    specification = new BayInternalSpecification();

                    break;
                case SpecificationType.Type.PLUG_IN:
                    specification = new PlugInSpecification();

                    break;
                case SpecificationType.Type.NODE:
                    specification = new NodeSpecification();

                    break;
                case SpecificationType.Type.CARD:
                    specification = new CardSpecification();

                    break;
                case SpecificationType.Type.SHELF:
                    specification = new ShelfSpecification();

                    break;
                case SpecificationType.Type.SLOT:
                    specification = new SlotSpecification();

                    break;
                case SpecificationType.Type.PORT:
                    specification = new PortSpecification();

                    break;
                default:
                    break;
            }

            return specification;
        }
    }
}