using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using Newtonsoft.Json;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class MaterialDbInterfaceTest
    {
        private string dbConnectionStringDev1 = "Data Source= (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = cdmms01ddb.test.intranet)(PORT = 1535)) (CONNECT_DATA = (SID = cdmms01d)));Pooling=true;Min Pool Size=0;Validate Connection=true;Password=cdmms;User ID=cdmms_app;";
        private string dbConnectionStringDev2 = "Data Source= (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = cdmms02ddb.test.intranet)(PORT = 1536)) (CONNECT_DATA = (SID = cdmms02d)));Pooling=true;Min Pool Size=0;Validate Connection=true;Password=cdmms;User ID=cdmms_app;";

        [TestMethod]
        public void TestConstants()
        {
            APPLICATION_NAME app = APPLICATION_NAME.UNSET;
            bool success = Enum.TryParse<APPLICATION_NAME>("NEISL", out app);

            Assert.IsTrue(success);

            success = Enum.TryParse<APPLICATION_NAME>("XYZ", out app);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void TestGetMaterialSourceOfRecord()
        {
            MaterialDbInterface db = new MaterialDbInterface(dbConnectionStringDev1);
            long materialItemId = 99814;
            string source = "";

            source = db.GetMaterialSourceOfRecord(materialItemId);

            Assert.AreEqual("LOSDB", source);
        }

        [TestMethod]
        public void TestDateFormat()
        {
            DateTime now = DateTime.Now;
            string format = "yyyy/MM/dd HH:mm:ss";
            string date = "";

            date = now.ToString(format);
        }

        [TestMethod]
        public void TestMockData()
        {
            MockDataDbInterface db = new MockDataDbInterface(dbConnectionStringDev1);
            string data = "";
            int id = 225;
            bool useMockData = MockDataHelper.UseMockData;
            Type t = this.GetType();

            //data = db.GetData(id);

            useMockData = MockDataHelper.UseMockData;
        }

        [TestMethod]
        public void TestHasRevisions()
        {
            ConnectorizedCableDbInterface db = new ConnectorizedCableDbInterface(dbConnectionStringDev1);
            long materialItemId = 342312;
            long materialId = 100000000;
            IMaterial material = new ConnectorizedCable(materialItemId, materialId);
            IMaterial mtl = new VariableLengthCable();
            
            db.HasRevisions(material);

            Assert.AreEqual(false, material.HasRevisions);

            db.HasRevisions(mtl);
        }
    }
}
