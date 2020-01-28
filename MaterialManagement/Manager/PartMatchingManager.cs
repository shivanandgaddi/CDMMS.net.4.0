using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.ExceptionManager;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Material.Management.Business.DTO;
using CenturyLink.Network.Engineering.Material.Management.Business.Utility;
using CenturyLink.Network.Engineering.TypeLibrary;

namespace CenturyLink.Network.Engineering.Material.Management.Manager
{
    class PartMatchingManager
    {
        public class Item
        {
            public string materialCode = String.Empty;
            public string productID = String.Empty;
            public string equipmentCatalogItemID = String.Empty;
        }
        public void MatchSAPPartToLOSDBPart(CatalogItem catalogItem)
        {
            bool foundOneA = false;
            bool foundOneB = false;
            bool foundOneC = false;
            
            MaterialItemDBInterface materialItemDBInterface = new MaterialItemDBInterface();
            List<LOSDBItem> losdbItems = new List<LOSDBItem>();
            List<Item> items = new List<Item>();

            if (!catalogItem.MATNR.StartsWith("R"))
            {
                if (catalogItem.HECI_CODE != null && catalogItem.HECI_CODE != "") // step 1.  heci code is not null
                {

                    List<IESEaCompCleiExtn> IesEaCompCleiExtnList = materialItemDBInterface.GetIESEaCompCleiExtn(catalogItem.HECI_CODE);
                    foreach (IESEaCompCleiExtn iesEaCompCleiExtn in IesEaCompCleiExtnList)
                    {
                        List<LOSDBItem> tempLosdbItems = new List<LOSDBItem>();
                        tempLosdbItems = materialItemDBInterface.GetLOSDBItems(iesEaCompCleiExtn.CLEICode);
                        foreach (LOSDBItem tempItem in tempLosdbItems)
                        {
                            losdbItems.Add(tempItem);
                        }
                    }
                }
                if (catalogItem.MFRNR != null && catalogItem.MFRNR != "") // step 2.  match on mfg_id
                {
                    //List<LOSDBItem> equipmentItems = new List<LOSDBItem>();
                    //equipmentItems = materialItemDBInterface.GetLOSDBEquipment(catalogItem.MFRNR); // could be many of these

                    //foreach(LOSDBItem tempItem in equipmentItems)
                    //{
                    //    LOSDBItem tempLosdbItem = new LOSDBItem();
                    //    tempLosdbItem = materialItemDBInterface.GetLOSDBItems2(tempItem.ProdID);
                    //    losdbItems.Add(tempLosdbItem);
                    //}

                    List<LOSDBItem> tempLosdbItems = new List<LOSDBItem>();
                    tempLosdbItems = materialItemDBInterface.GetLOSDBItems3(catalogItem.MFRNR);
                    foreach (LOSDBItem tempItem in tempLosdbItems)
                    {
                        losdbItems.Add(tempItem);
                    }
                }

                // step 3. now that we have gathered a list of LOSDB Items, one way or another based on heci code,
                // process these for matches 

                foreach (LOSDBItem losdbItem in losdbItems)
                {
                    // step 3.a. If ies_invntry.ordg_cd is not null, then match the 
                    // mtl_item_sap.mfg_part_no (MFRPN) to the ies_invntry. ordg_cd.  
                    // If this doesn’t work look for a “=” at the end of the mfg_part_no

                    if (losdbItem.OrderingCode != null && losdbItem.OrderingCode != "")
                    {
                        if (catalogItem.MFRPN == losdbItem.OrderingCode)
                        {
                            // this is a match
                            Item item = new Item();
                            item.materialCode = catalogItem.MATNR;
                            item.productID = losdbItem.ProdID;
                            item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                            items.Add(item);
                            foundOneA = true;
                            continue;
                        }
                        else
                        {
                            if (catalogItem.MFRPN.TrimEnd('=') == losdbItem.OrderingCode)
                            {
                                // this is a match
                                Item item = new Item();
                                item.materialCode = catalogItem.MATNR;
                                item.productID = losdbItem.ProdID;
                                item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                                items.Add(item);
                                foundOneA = true;
                                continue;
                            }
                        }
                    }

                    // step 3.b. If the ies_eqpt.part_no is not null, then match the 
                    // mtl_item_sap.mfg_part_no to the ies_eqpt.part_no.  If this doesn’t work try to 
                    // match the mtl_item_sap.mfg_part_no to the combination of ies_eqpt.part_no and 
                    // ies_invntry.ls_or_srs in that order with a wild card in the middle due to a 
                    // combination of delimiters being possible.  If neither of those work look for 
                    // a “=” at the end of the mfg_part_no and remove it and then retry both rules.  
                    // If a match is not found go to step c.

                    if (losdbItem.PartNumber != null && losdbItem.PartNumber != "")
                    {
                        if (catalogItem.MFRPN == losdbItem.PartNumber)
                        {
                            // this is a match
                            Item item = new Item();
                            item.materialCode = catalogItem.MATNR;
                            item.productID = losdbItem.ProdID;
                            item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                            items.Add(item);
                            foundOneB = true;
                            continue;
                        }
                        else if (losdbItem.LsOrSrs != null && losdbItem.LsOrSrs != "")
                        {
                            if (catalogItem.MFRPN.StartsWith(losdbItem.PartNumber) && catalogItem.MFRPN.EndsWith(losdbItem.LsOrSrs))
                            {
                                // this is a match
                                Item item = new Item();
                                item.materialCode = catalogItem.MATNR;
                                item.productID = losdbItem.ProdID;
                                item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                                items.Add(item);
                                foundOneB = true;
                                continue;
                            }
                        }
                        else if (catalogItem.MFRPN.TrimEnd('=') == losdbItem.PartNumber)
                        {
                            // this is a match
                            Item item = new Item();
                            item.materialCode = catalogItem.MATNR;
                            item.productID = losdbItem.ProdID;
                            item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                            items.Add(item);
                            foundOneB = true;
                            continue;
                        }
                        else if (losdbItem.LsOrSrs != null && losdbItem.LsOrSrs != "")
                        {
                            if (catalogItem.MFRPN.TrimEnd('=').StartsWith(losdbItem.PartNumber) && catalogItem.MFRPN.TrimEnd('=').EndsWith(losdbItem.LsOrSrs))
                            {
                                // this is a match
                                Item item = new Item();
                                item.materialCode = catalogItem.MATNR;
                                item.productID = losdbItem.ProdID;
                                item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                                items.Add(item);
                                foundOneB = true;
                                continue;
                            }
                        }
                    }

                    // 3.c. If the ies_eqpt.drwg is not null, then match mtl_item_sap.mfg_part_no 
                    // to ies_eqpt.drwg.  If this doesn’t work try to match the mtl_item_sap.mfg_part_no 
                    // to the combination of ies_eqpt.drwg and ies_eqpt.drwg_iss in that order with a 
                    // wild card in the middle due to a combination of delimiters being possible.  
                    // If neither of those work look for a “=” at the end of the mfg_part_no and remove 
                    // it and then retry both rules.  If a match is not found then go immediately 
                    // to manual intervention.
                    if (losdbItem.Drawing != null && losdbItem.Drawing != "")
                    {
                        if (catalogItem.MFRPN == losdbItem.Drawing)
                        {
                            // this is a match
                            Item item = new Item();
                            item.materialCode = catalogItem.MATNR;
                            item.productID = losdbItem.ProdID;
                            item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                            items.Add(item);
                            foundOneC = true;
                            continue;
                        }
                        else if (losdbItem.DrawingISS != null && losdbItem.DrawingISS != "")
                        {
                            if (catalogItem.MFRPN.StartsWith(losdbItem.Drawing) && catalogItem.MFRPN.EndsWith(losdbItem.DrawingISS))
                            {
                                // this is a match
                                Item item = new Item();
                                item.materialCode = catalogItem.MATNR;
                                item.productID = losdbItem.ProdID;
                                item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                                items.Add(item);
                                foundOneC = true;
                                continue;
                            }
                        }
                        else if (catalogItem.MFRPN.TrimEnd('=') == losdbItem.Drawing)
                        {
                            // this is a match
                            Item item = new Item();
                            item.materialCode = catalogItem.MATNR;
                            item.productID = losdbItem.ProdID;
                            item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                            items.Add(item);
                            foundOneC = true;
                            continue;
                        }
                        else if (losdbItem.DrawingISS != null && losdbItem.DrawingISS != "")
                        {
                            if (catalogItem.MFRPN.TrimEnd('=').StartsWith(losdbItem.Drawing) && catalogItem.MFRPN.TrimEnd('=').EndsWith(losdbItem.DrawingISS))
                            {
                                // this is a match
                                Item item = new Item();
                                item.materialCode = catalogItem.MATNR;
                                item.productID = losdbItem.ProdID;
                                item.equipmentCatalogItemID = losdbItem.EquipmentCatalogItemID;
                                items.Add(item);
                                foundOneC = true;
                                continue;
                            }
                        }
                    }
                }

                if (foundOneA || foundOneB || foundOneC)
                {
                    foreach (Item item in items)
                    {
                        AddMatch(item.materialCode, item.productID, item.equipmentCatalogItemID);
                    }
                }
            }
        }

        private void AddMatch(string mtlCode, string prodID, string equipmentCatalogItemID)
        {
            // Before we add, check to see if this combination exists.
            MaterialItemDBInterface materialItemDBInterface = new MaterialItemDBInterface();
            if (!materialItemDBInterface.DoesPartAssociationExist(mtlCode, prodID, equipmentCatalogItemID))
            {
                materialItemDBInterface.InsertPossibleAssociation(mtlCode, prodID, equipmentCatalogItemID);
            }
        }
    }
}
