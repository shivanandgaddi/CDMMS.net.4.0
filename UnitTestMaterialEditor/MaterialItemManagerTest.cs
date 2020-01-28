using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;
using CenturyLink.Network.Engineering.Material.Editor.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class MaterialItemManagerTest
    {
        [TestMethod]
        public void TestGetActiveMaterialItemAsync()
        {
            MaterialItemManager manager = new MaterialItemManager();
            long materialItemId = //343810;
                344182; 
                //342312;
            string source = "ro";
            IMaterial material = null;

            Task.Run(async () =>
            {
                material = await manager.GetActiveMaterialItemAsync(materialItemId, source);
            });

            while (material == null)
            {
                Thread.Sleep(50000);
            }

            Assert.IsTrue(material.IsRecordOnly);
        }

        [TestMethod]
        public void TestPersistMaterialItemConnectorizedCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            #region json
            string json = Json.connectorizedCable;
            #endregion

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemVariableLengthCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            #region json
            string json = Json.variableLengthCable;
            #endregion

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemBayExtender()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            #region json
            string json = Json.bayExtender;
            #endregion

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemCard()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            #region json
            string json = Json.card;
            #endregion

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewCard()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.cardNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewBay()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.bayNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewBayExtender()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.bayExtenderNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewBulkCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.bulkCableNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewConnectorizedCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.connectorizedCableNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewHighLevelPart()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.highLevelPartNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewMinorMaterial()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.minorMaterialNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewNonRmeCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.nonRmeCableNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewNonRmeNonCable()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.nonRmeNonCableNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewPlugIn()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.plugInNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestPersistMaterialItemNewShelf()
        {
            JObject item = null;
            long materialItemId = 0;
            long workToDoId = -1;
            long[] ids = null;
            MaterialItemManager manager = new MaterialItemManager();
            string json = Json.shelfNew;

            try
            {
                item = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

                if (!long.TryParse((string)item.SelectToken("id.value"), out materialItemId))
                {
                    Assert.Fail();
                }

                Task.Run(async () =>
                {
                    ids = await manager.PersistMaterialItem(item, materialItemId);

                    workToDoId = ids[2];
                });

                while (workToDoId < 0)
                {
                    Thread.Sleep(50000);
                }

                Assert.IsTrue(workToDoId >= 0);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}
