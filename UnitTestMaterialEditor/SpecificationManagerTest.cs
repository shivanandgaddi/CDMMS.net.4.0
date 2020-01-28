using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Material.Editor.Models;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class SpecificationManagerTest
    {
        [TestMethod]
        public void TestPersistSpecificationBay()
        {
            JObject item = null;
            long specificationId = 0;
            long[] workToDoId = null;
            SpecificationManager manager = new SpecificationManager();
            string json = Json.baySpecification;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out specificationId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    workToDoId = await manager.PersistSpecification(item, specificationId);
                });

                while (workToDoId[0] < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId[0] >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistObjectBay()
        {
            JObject item = null;
            long specificationId = 0;
            long[] workToDoId = null;
            SpecificationManager manager = new SpecificationManager();
            string json = Json.baySpecificationNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                Task.Run(async () =>
                {
                    workToDoId = await manager.PersistSpecification(item, specificationId);
                });

                while (workToDoId[0] < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId[0] >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistObjectCard()
        {
            JObject item = null;
            long specificationId = 0;
            long[] workToDoId = null;
            SpecificationManager manager = new SpecificationManager();
            string json = Json.cardSpecificationNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                Task.Run(async () =>
                {
                    workToDoId = await manager.PersistSpecification(item, specificationId);
                });

                while (workToDoId[0] < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId[0] >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistSpecificationCard()
        {
            JObject item = null;
            long specificationId = 178;
            long[] workToDoId = null;
            SpecificationManager manager = new SpecificationManager();
            string json = Json.cardSpecificationUpdate;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                Task.Run(async () =>
                {
                    workToDoId = await manager.PersistSpecification(item, specificationId);
                });

                while (workToDoId[0] < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId[0] >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}
