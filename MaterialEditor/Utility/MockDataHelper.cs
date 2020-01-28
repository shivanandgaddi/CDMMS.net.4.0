using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public static class MockDataHelper
    {
        private static string useMockDataValue = string.Empty;
        private static bool useMockData = false;

        public static bool UseMockData
        {
            get
            {
                if (string.IsNullOrEmpty(useMockDataValue))
                {
                    useMockDataValue = System.Configuration.ConfigurationManager.AppSettings["useMockData"];

                    if (string.IsNullOrEmpty(useMockDataValue))
                        useMockDataValue = "false";

                    if (!bool.TryParse(useMockDataValue, out useMockData))
                        useMockData = false;
                }

                return useMockData;
            }
        }

        public static async Task<string> GetData(long id)
        {
            string data = "{}";
            MockDataDbInterface dbInterface = new MockDataDbInterface();

            data = await dbInterface.GetData(id);

            return data;
        }
    }
}