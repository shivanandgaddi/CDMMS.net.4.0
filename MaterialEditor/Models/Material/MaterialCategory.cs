using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Material
{
    public static class MaterialCategory
    {
        private static Dictionary<string, Category> categories;

        static MaterialCategory()
        {
            ReferenceDbInterface dbInterface = new ReferenceDbInterface();

            categories = dbInterface.GetMaterialCategories();
        }

        public static int Id(string categoryType)
        {
            if (categories.ContainsKey(categoryType))
                return categories[categoryType].Id;
            else
                return 0;
        }

        public class Category
        {
            private int id;
            private string type;
            private string rootTable;
            private string revisionTable;

            public int Id
            {
                get
                {
                    return id;
                }
                set
                {
                    id = value;
                }
            }

            public string Type
            {
                get
                {
                    return type;
                }
                set
                {
                    type = value;
                }
            }

            public string RootTable
            {
                get
                {
                    return rootTable;
                }
                set
                {
                    rootTable = value;
                }
            }

            public string RevisionTable
            {
                get
                {
                    return revisionTable;
                }
                set
                {
                    revisionTable = value;
                }
            }
        }
    }
}