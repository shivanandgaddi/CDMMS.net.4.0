using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class MaterialJsonProperty : System.Attribute
    {
        private string name;
        private string[] optionValues = null;
        private bool ignoreNumericZeroValues = true;
        private bool hasOptionValues = false;
        private bool isBooleanValue = false;
        private bool isListValue = false; //Use only with SpecificationAttributes
        private List<string> useTypeValues = null;

        public MaterialJsonProperty(string jsonName)
        {
            name = jsonName;
        }

        public MaterialJsonProperty(string jsonName, bool isBoolValue, string dummy)
        {
            name = jsonName;
            isBooleanValue = isBoolValue;
        }

        public MaterialJsonProperty(string jsonName, bool ignoreNumericValuesThatEqualZero)
        {
            name = jsonName;
            ignoreNumericZeroValues = ignoreNumericValuesThatEqualZero;
        }

        public MaterialJsonProperty(bool hasAssociatedOptionValues, string jsonName)
        {
            name = jsonName;
            hasOptionValues = hasAssociatedOptionValues;
        }

        public MaterialJsonProperty(string jsonName, bool ignoreNumericValuesThatEqualZero, bool hasAssociatedOptionValues)
        {
            name = jsonName;
            ignoreNumericZeroValues = ignoreNumericValuesThatEqualZero;
            hasOptionValues = hasAssociatedOptionValues;
        }

        public MaterialJsonProperty(bool isObjectList, string jsonName, string dummy)
        {
            isListValue = isObjectList;
            name = jsonName;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool IgnoreNumericZeroValues
        {
            get
            {
                return ignoreNumericZeroValues;
            }
        }

        public bool HasOptionValues
        {
            get
            {
                return hasOptionValues;
            }
        }

        public string[] OptionValues
        {
            get
            {
                return optionValues;
            }
            set
            {
                optionValues = value;
            }
        }

        public bool IsBooleanValue
        {
            get
            {
                return isBooleanValue;
            }
        }

        public bool IsObjectList
        {
            get
            {
                return isListValue;
            }
        }
    }
}