using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;

namespace CenturyLink.Network.Engineering.Material.Editor.Models
{
    public static class FeatureType
    {
        private static Dictionary<string, Feature> featureTypes;

        static FeatureType()
        {
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();

            featureTypes = dbInterface.GetFeatureTypes();
        }

        public static int Id(string featureType)
        {
            if (featureTypes.ContainsKey(featureType))
                return featureTypes[featureType].Id;
            else
                return 0;
        }

        public class Feature
        {
            public int Id
            {
                get;
                set;
            }

            public int NdsId
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public string NdsType
            {
                get;
                set;
            }

            public string RMEIndicator
            {
                get;
                set;
            }

            public string CableIndicator
            {
                get;
                set;
            }

            public string RecordsOnlyAllowIndicator
            {
                get;
                set;
            }

            public string OrderableAllowIndicator
            {
                get;
                set;
            }

            public string SymbologyIndicator
            {
                get;
                set;
            }

            public string HeightIndicator
            {
                get;
                set;
            }

            public string WidthIndicator
            {
                get;
                set;
            }

            public string DepthIndicator
            {
                get;
                set;
            }

            public string LengthIndicator
            {
                get;
                set;
            }

            public string ContainedInAllowIndicator
            {
                get;
                set;
            }

            public string RootTableName
            {
                get;
                set;
            }

            public string RevisionTableName
            {
                get;
                set;
            }
        }
    }
}