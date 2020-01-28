using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class MaterialFactoryTest
    {
        private string dbConnectionStringDev1 = "Data Source= (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = cdmms01ddb.test.intranet)(PORT = 1535)) (CONNECT_DATA = (SID = cdmms01d)));Pooling=true;Min Pool Size=0;Validate Connection=true;Password=cdmms;User ID=cdmms_app;";
        private string dbConnectionStringDev2 = "Data Source= (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = cdmms02ddb.test.intranet)(PORT = 1536)) (CONNECT_DATA = (SID = cdmms02d)));Pooling=true;Min Pool Size=0;Validate Connection=true;Password=cdmms;User ID=cdmms_app;";

        [TestMethod]
        public void TestGetMaterialInstance()
        {
            long materialItemId = 342312; //44781;
            MaterialDbInterface db = new MaterialDbInterface(dbConnectionStringDev1);
            NameValueCollection properties = null;
            IMaterial material = null;
            string json = "";
            long[] testArray = new long[] { 0, 1, 0 };

            testArray[2] = 2;

            Task.Run(async () =>
            {
                properties = await db.GetMaterialIdCategoryAndFeatureTypesAsync(materialItemId);

                material = MaterialFactory.GetMaterialInstance(materialItemId, properties);
            });

            while (material == null)
            {
                Thread.Sleep(5000);
            }

            Assert.IsTrue(material.Attributes.Count >= 1);

            json = JsonConvert.SerializeObject(material.Attributes, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        [TestMethod]
        public void TestGetMaterialInstanceConnectorizedCable()
        {
            long materialItemId = 342312;
            MaterialDbInterface db = new MaterialDbInterface(dbConnectionStringDev1);
            NameValueCollection properties = null;
            IMaterial material = null;

            Task.Run(async () =>
            {
                properties = await db.GetMaterialIdCategoryAndFeatureTypesAsync(materialItemId);

                material = MaterialFactory.GetMaterialInstance(materialItemId, properties);
            });

            while (material == null)
            {
                Thread.Sleep(5000);
            }

            Assert.IsTrue(material.Attributes.Count == 1);
        }

        [TestMethod]
        public void TestGetMaterialInstanceCard()
        {
            long materialItemId = 334502;
            MaterialDbInterface db = new MaterialDbInterface(dbConnectionStringDev1);
            NameValueCollection properties = null;
            IMaterial material = null;

            Task.Run(async () =>
            {
                properties = await db.GetMaterialIdCategoryAndFeatureTypesAsync(materialItemId);

                material = MaterialFactory.GetMaterialInstance(materialItemId, properties);
            });

            while (material == null)
            {
                Thread.Sleep(50000);
            }

            Assert.IsTrue(material is Card);
        }
    }
}
