using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class UnitConverter
    {
        private UnitConverter()
        {
        }

        public static decimal ConvertToInches(decimal valueToConvert, string unitToConvertFrom, ref bool wasSuccessful)
        {
            decimal convertedValue = 0;

            switch (unitToConvertFrom)
            {
                case "in":
                case "Inch":
                    convertedValue = valueToConvert;
                    wasSuccessful = true;

                    break;
                case "ft":
                case "Feet":
                    convertedValue = valueToConvert * 12;
                    wasSuccessful = true;

                    break;
                case "mile":
                case "Mile":
                    convertedValue = valueToConvert * 12 * 5280;
                    wasSuccessful = true;

                    break;
                case "yd":
                case "Yard":
                    convertedValue = valueToConvert * 12 * 3;
                    wasSuccessful = true;

                    break;
                case "usfeet":
                case "US Survey Feet":
                    convertedValue = valueToConvert * (decimal)12.000024;
                    wasSuccessful = true;

                    break;
                case "m":
                case "Meter":
                    convertedValue = valueToConvert * (decimal)39.37;
                    wasSuccessful = true;

                    break;
                default:
                    wasSuccessful = false;

                    break;
            }

            return convertedValue;
        }
    }
}