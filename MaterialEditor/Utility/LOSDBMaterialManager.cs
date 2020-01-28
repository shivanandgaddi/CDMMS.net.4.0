using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.DbInterface.Material;
using CenturyLink.Network.Engineering.Material.Editor.Models.Material;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class LOSDBMaterialManager
    {
        //private static readonly string EQPT_CTLG_ITEM_ID = "eqpt_ctlg_item_id";
        //private static readonly string CLEI_CODE = "cleicode";
        //private static readonly string ELECTRICAL_KEY = "electrical_key";
        //private static readonly string COMP_CLEI_KEY = "comp_clei_key";
        //private static readonly string PROD_ID = "prod_id";
        MaterialDbInterface dbInterface = null;
        MaterialType materialType = null;

        public LOSDBMaterialManager()
        {
        }

        public async Task<MaterialItem> GetMaterialItemAsync(long materialItemId)
        {
            MaterialItem losdbMaterial = null;
            dbInterface = new MaterialDbInterface();
            KeyCollection keys = null;

            await Task.Run(() =>
            {
                materialType = new MaterialType();
                keys = GetKeyCollection(materialItemId);

                if (keys != null && KeysNotEmpty(keys))
                {
                    losdbMaterial = new MaterialItem(materialItemId);

                    losdbMaterial.Attributes["MtrlId"].Value = "-1";

                    if (keys.ProdId > 0 && !string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                    {
                        PopulateEquipmentValues(losdbMaterial, keys);
                        PopulateInventoryValues(losdbMaterial, keys);
                    }
                    else if (keys.ProdId == 0 && !string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                        PopulateInventoryAndEquipmentValues(losdbMaterial, keys);
                    else if (keys.ProdId > 0 && string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                        PopulateEquipmentAndInventoryValues(losdbMaterial, keys);

                    PopulateElectricalValues(losdbMaterial, keys);
                    PopulateCompatibleCLEIValues(losdbMaterial, keys);
                    PopulateMainExtensionValues(losdbMaterial, keys);

                    losdbMaterial.ElectricalItems = new List<MaterialItem.ElectricalItemObject>();
                    losdbMaterial.ElectricalItems = dbInterface.GetLOSDBElectrical(losdbMaterial, keys);

                    losdbMaterial.CompatibleSevenCLEIItem = new MaterialItem.CompatibleSevenItemObject();
                    losdbMaterial.CompatibleSevenCLEIItem = dbInterface.GetLOSDBCompClei7(losdbMaterial, keys);
                }
            });

            return losdbMaterial;
        }

        private bool KeysNotEmpty(KeyCollection keys)
        {
            bool notEmpty = false;

            if (keys != null)
            {
                if (keys.CLEIKey > 0 || keys.ElectricalKey > 0 || keys.ProdId > 0 || !string.IsNullOrEmpty(keys.CLEICode) || !string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                    notEmpty = true;
            }

            return notEmpty;
        }

        private KeyCollection GetKeyCollection(long materialItemId)
        {
            KeyCollection keys = null;

            keys = dbInterface.GetLOSDBMaterialKeys(materialItemId);

            return keys;
        }

        private void PopulateEquipmentValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && keys.ProdId > 0)
                dbInterface.GetLOSDBIesEqpt(losdbMaterial, keys.ProdId, ref valuesWereFound);

            if(!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESEquipment)
                {
                    losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateInventoryValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && !string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                dbInterface.GetLOSDBIesInventory(losdbMaterial, keys.EquipmentCatalogItemId, ref valuesWereFound);

            if(!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESInventory)
                {
                    if(!losdbMaterial.Attributes.ContainsKey(keyValue.Key))
                        losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateInventoryAndEquipmentValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && !string.IsNullOrEmpty(keys.EquipmentCatalogItemId))
                dbInterface.GetLOSDBIesInventoryAndEquipment(losdbMaterial, keys.EquipmentCatalogItemId, ref valuesWereFound);

            if (!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESInventory)
                {
                    if (!losdbMaterial.Attributes.ContainsKey(keyValue.Key))
                        losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }

                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESEquipment)
                {
                    if (!losdbMaterial.Attributes.ContainsKey(keyValue.Key))
                        losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateEquipmentAndInventoryValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null)
                dbInterface.GetLOSDBIesEquipmentAndInventory(losdbMaterial, keys.ProdId, ref valuesWereFound);

            if (!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESInventory)
                {
                    if (!losdbMaterial.Attributes.ContainsKey(keyValue.Key))
                        losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }

                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESEquipment)
                {
                    if (!losdbMaterial.Attributes.ContainsKey(keyValue.Key))
                        losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateCompatibleCLEIValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && keys.CLEIKey > 0)
                dbInterface.GetLOSDBCompatibleCLEI(losdbMaterial, keys.CLEIKey, ref valuesWereFound);

            if(!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESCompatibleCLEI)
                {
                    losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateElectricalValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && keys.ElectricalKey > 0)
                dbInterface.GetLOSDBElectrical(losdbMaterial, keys.ElectricalKey, ref valuesWereFound);

            if(!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESElectrical)
                {
                    losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        private void PopulateMainExtensionValues(MaterialItem losdbMaterial, KeyCollection keys)
        {
            bool valuesWereFound = false;

            if (keys != null && !string.IsNullOrEmpty(keys.CLEICode))
                dbInterface.GetLOSDBMainExtension(losdbMaterial, keys.CLEICode, ref valuesWereFound);

            if(!valuesWereFound)
            {
                foreach (KeyValuePair<string, DatabaseDefinition> keyValue in materialType.IESMainExtension)
                {
                    losdbMaterial.Attributes.Add(keyValue.Key, new Models.Attribute(keyValue.Key, keyValue.Value.IsNumber ? "0" : "", MaterialType.SourceSystem(SOURCE_SYSTEM.LOSDB)));
                }
            }
        }

        public async Task<List<MaterialItem>> GetChainedInMaterialItemsAsync(long materialId)
        {
            List<MaterialItem> losdbMaterial = null;
            dbInterface = new MaterialDbInterface();
            List<LOSDBMaterialManager.Mtrl> existingAssociatedMaterial = null;
            List<LOSDBMaterialManager.LosdbMtrl> cleiMtrl = null;
            List<LOSDBMaterialManager.LosdbMtrl> orderingCodeMtrl = null;
            List<LOSDBMaterialManager.LosdbMtrl> partNumberMtrl = null;
            List<LOSDBMaterialManager.LosdbMtrl> drawingMtrl = null;
            Hashtable ids = new Hashtable();

            await Task.Run(() =>
            {
                existingAssociatedMaterial = dbInterface.GetExistingAssociatedLOSDBMaterial(materialId);

                if (existingAssociatedMaterial != null)
                {
                    ids = new Hashtable();

                    foreach (Mtrl mtrl in existingAssociatedMaterial)
                    {
                        if (!ids.ContainsKey(mtrl.EquipmentCatalogItemId))
                            ids.Add(mtrl.EquipmentCatalogItemId, mtrl.ProdId);
                            
                        cleiMtrl = dbInterface.GetChainedInLOSDBMaterialByClei(mtrl);
                        orderingCodeMtrl = dbInterface.GetChainedInLOSDBMaterialByOrderingCode(mtrl);
                        partNumberMtrl = dbInterface.GetChainedInLOSDBMaterialByPartNumber(mtrl);
                        drawingMtrl = dbInterface.GetChainedInLOSDBMaterialByDrawing(mtrl);

                        if (cleiMtrl != null)
                        {
                            foreach (LosdbMtrl clei in cleiMtrl)
                            {
                                if (!ids.ContainsKey(clei.EquipmentCatalogItemId))
                                {
                                    ids.Add(clei.EquipmentCatalogItemId, clei.ProdId);

                                    if (losdbMaterial == null)
                                        losdbMaterial = new List<MaterialItem>();

                                    MaterialItem mi = new MaterialItem();

                                    mi.Attributes.Add("altInd", new Models.Attribute("altInd", clei.AlternateIndicator));
                                    mi.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, clei.CLEI));
                                    mi.Attributes.Add("drwg", new Models.Attribute("drwg", clei.Drawing));
                                    mi.Attributes.Add("drwgIss", new Models.Attribute("drwgIss", clei.DrawingIssue));
                                    mi.Attributes.Add("eqptCtlgItmId", new Models.Attribute("eqptCtlgItmId", clei.EquipmentCatalogItemId));
                                    mi.Attributes.Add("lsOrSrs", new Models.Attribute("lsOrSrs", clei.LsOrSrs));
                                    mi.Attributes.Add("ordrCd", new Models.Attribute("ordrCd", clei.OrderingCode));
                                    mi.Attributes.Add("prtNo", new Models.Attribute("prtNo", clei.PartNumber));
                                    mi.Attributes.Add("prdId", new Models.Attribute("prdId", clei.ProdId.ToString()));
                                    mi.Attributes.Add("vndr", new Models.Attribute("vndr", clei.Vendor));
                                    mi.Attributes.Add("compatibleequipmentclei7", new Models.Attribute("compatibleequipmentclei7", clei.CleiCompatible7));
                                    mi.Attributes.Add("pcnchange", new Models.Attribute("pcnchange", clei.PCNChange));

                                    losdbMaterial.Add(mi);
                                }
                            }
                        }

                        if (orderingCodeMtrl != null)
                        {
                            foreach (LosdbMtrl orderingCode in orderingCodeMtrl)
                            {
                                if (!ids.ContainsKey(orderingCode.EquipmentCatalogItemId))
                                {
                                    ids.Add(orderingCode.EquipmentCatalogItemId, orderingCode.ProdId);

                                    if (losdbMaterial == null)
                                        losdbMaterial = new List<MaterialItem>();

                                    MaterialItem mi = new MaterialItem();

                                    mi.Attributes.Add("altInd", new Models.Attribute("altInd", orderingCode.AlternateIndicator));
                                    mi.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, orderingCode.CLEI));
                                    mi.Attributes.Add("drwg", new Models.Attribute("drwg", orderingCode.Drawing));
                                    mi.Attributes.Add("drwgIss", new Models.Attribute("drwgIss", orderingCode.DrawingIssue));
                                    mi.Attributes.Add("eqptCtlgItmId", new Models.Attribute("eqptCtlgItmId", orderingCode.EquipmentCatalogItemId));
                                    mi.Attributes.Add("lsOrSrs", new Models.Attribute("lsOrSrs", orderingCode.LsOrSrs));
                                    mi.Attributes.Add("ordrCd", new Models.Attribute("ordrCd", orderingCode.OrderingCode));
                                    mi.Attributes.Add("prtNo", new Models.Attribute("prtNo", orderingCode.PartNumber));
                                    mi.Attributes.Add("prdId", new Models.Attribute("prdId", orderingCode.ProdId.ToString()));
                                    mi.Attributes.Add("vndr", new Models.Attribute("vndr", orderingCode.Vendor));
                                    mi.Attributes.Add("compatibleequipmentclei7", new Models.Attribute("compatibleequipmentclei7", orderingCode.CleiCompatible7));
                                    mi.Attributes.Add("pcnchange", new Models.Attribute("pcnchange", orderingCode.PCNChange));

                                    losdbMaterial.Add(mi);
                                }
                            }
                        }

                        if (partNumberMtrl != null)
                        {
                            foreach (LosdbMtrl partNumber in partNumberMtrl)
                            {
                                if (!ids.ContainsKey(partNumber.EquipmentCatalogItemId))
                                {
                                    ids.Add(partNumber.EquipmentCatalogItemId, partNumber.ProdId);

                                    if (losdbMaterial == null)
                                        losdbMaterial = new List<MaterialItem>();

                                    MaterialItem mi = new MaterialItem();

                                    mi.Attributes.Add("altInd", new Models.Attribute("altInd", partNumber.AlternateIndicator));
                                    mi.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, partNumber.CLEI));
                                    mi.Attributes.Add("drwg", new Models.Attribute("drwg", partNumber.Drawing));
                                    mi.Attributes.Add("drwgIss", new Models.Attribute("drwgIss", partNumber.DrawingIssue));
                                    mi.Attributes.Add("eqptCtlgItmId", new Models.Attribute("eqptCtlgItmId", partNumber.EquipmentCatalogItemId));
                                    mi.Attributes.Add("lsOrSrs", new Models.Attribute("lsOrSrs", partNumber.LsOrSrs));
                                    mi.Attributes.Add("ordrCd", new Models.Attribute("ordrCd", partNumber.OrderingCode));
                                    mi.Attributes.Add("prtNo", new Models.Attribute("prtNo", partNumber.PartNumber));
                                    mi.Attributes.Add("prdId", new Models.Attribute("prdId", partNumber.ProdId.ToString()));
                                    mi.Attributes.Add("vndr", new Models.Attribute("vndr", partNumber.Vendor));
                                    mi.Attributes.Add("compatibleequipmentclei7", new Models.Attribute("compatibleequipmentclei7", partNumber.CleiCompatible7));
                                    mi.Attributes.Add("pcnchange", new Models.Attribute("pcnchange", partNumber.PCNChange));

                                    losdbMaterial.Add(mi);
                                }
                            }
                        }

                        if (drawingMtrl != null)
                        {
                            foreach (LosdbMtrl drawing in drawingMtrl)
                            {
                                if (!ids.ContainsKey(drawing.EquipmentCatalogItemId))
                                {
                                    ids.Add(drawing.EquipmentCatalogItemId, drawing.ProdId);

                                    if (losdbMaterial == null)
                                        losdbMaterial = new List<MaterialItem>();

                                    MaterialItem mi = new MaterialItem();

                                    mi.Attributes.Add("altInd", new Models.Attribute("altInd", drawing.AlternateIndicator));
                                    mi.Attributes.Add(MaterialType.JSON.CLEI, new Models.Attribute(MaterialType.JSON.CLEI, drawing.CLEI));
                                    mi.Attributes.Add("drwg", new Models.Attribute("drwg", drawing.Drawing));
                                    mi.Attributes.Add("drwgIss", new Models.Attribute("drwgIss", drawing.DrawingIssue));
                                    mi.Attributes.Add("eqptCtlgItmId", new Models.Attribute("eqptCtlgItmId", drawing.EquipmentCatalogItemId));
                                    mi.Attributes.Add("lsOrSrs", new Models.Attribute("lsOrSrs", drawing.LsOrSrs));
                                    mi.Attributes.Add("ordrCd", new Models.Attribute("ordrCd", drawing.OrderingCode));
                                    mi.Attributes.Add("prtNo", new Models.Attribute("prtNo", drawing.PartNumber));
                                    mi.Attributes.Add("prdId", new Models.Attribute("prdId", drawing.ProdId.ToString()));
                                    mi.Attributes.Add("vndr", new Models.Attribute("vndr", drawing.Vendor));
                                    mi.Attributes.Add("compatibleequipmentclei7", new Models.Attribute("compatibleequipmentclei7", drawing.CleiCompatible7));
                                    mi.Attributes.Add("pcnchange", new Models.Attribute("pcnchange", drawing.PCNChange));

                                    losdbMaterial.Add(mi);
                                }
                            }
                        }
                    }
                }
            });

            return losdbMaterial;
        }

        public class KeyCollection
        {
            public long ProdId
            {
                get;
                set;
            }

            public long ElectricalKey
            {
                get;
                set;
            }

            public List<long> ElectricalKeyList
            {
                get;
                set;
            }

            public long CLEIKey
            {
                get;
                set;
            }

            public List<long> CLEIKeyList
            {
                get;
                set;
            }

            public string CLEICode
            {
                get;
                set;
            }

            public string EquipmentCatalogItemId
            {
                get;
                set;
            }
        }

        public class Mtrl
        {
            public long MtrlId
            {
                get;
                set;
            }

            public string EquipmentCatalogItemId
            {
                get;
                set;
            }

            public long ProdId
            {
                get;
                set;
            }

            public string Manufacturer
            {
                get;
                set;
            }

            public string AlternateIndicator
            {
                get;
                set;
            }
        }

        public class LosdbMtrl
        {
            public string EquipmentCatalogItemId
            {
                get;
                set;
            }

            public long ProdId
            {
                get;
                set;
            }

            public string OrderingCode
            {
                get;
                set;
            }

            public string Vendor
            {
                get;
                set;
            }

            public string CLEI
            {
                get;
                set;
            }

            public string LsOrSrs
            {
                get;
                set;
            }

            public string Drawing
            {
                get;
                set;
            }

            public string DrawingIssue
            {
                get;
                set;
            }

            public string PartNumber
            {
                get;
                set;
            }

            public string AlternateIndicator
            {
                get;
                set;
            }

            public string CleiCompatible7
            {
                get;
                set;
            }

            public string PCNChange
            {
                get;
                set;
            }
        }
    }
}