using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.LOSDB.Service.DBInterface;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;
using CenturyLink.Network.Engineering.Common.DbInterface;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Business
{
    public class PartMatching
    {
        DBInterface.LOSDBDbInterface losdbDbInterface = new DBInterface.LOSDBDbInterface();

        public void HandleMatchingParts(bool initialDataLoad)
        {
            int match = 0;
            int nomatch = 0;
            int count = 0;
            string lastMaterialID = "";

            List<EquipmentObject> equipmentObjects = new List<EquipmentObject>();

            equipmentObjects = losdbDbInterface.GetNewEquipment(initialDataLoad);  // this does step 1 and 2, matching on compatibleequipmentclei7,

            foreach (EquipmentObject equipmentObject in equipmentObjects)
            {
                count++;
                if (equipmentObject.Drawing == "1")
                {
                    equipmentObject.Drawing = String.Empty;
                }

                

                // step 3.a.  try to match ordering code with SAP part mfg_part_no in mtl_item_sap
                string materialIDStep3a = "";
                string materialIDStep3b = "";
                string materialIDStep3c = "";
                if (equipmentObject.OrderingCode != null && equipmentObject.OrderingCode != "")
                {
                    materialIDStep3a = losdbDbInterface.GetPartNumberEqualMatch(equipmentObject.OrderingCode);
                    if (materialIDStep3a == "")
                    {
                        materialIDStep3a = losdbDbInterface.GetPartNumberMinusEqualEqualMatch(equipmentObject.OrderingCode);
                        if (materialIDStep3a == "")  // added by Chris Mahan.  also check the ordering code in ies_ea_main_extn. if match, get prod_id from clei code
                        {
                            if (equipmentObject.AlternateOrderingCode != null && equipmentObject.AlternateOrderingCode != "")
                            {
                                materialIDStep3a = losdbDbInterface.GetPartNumberEqualMatch(equipmentObject.AlternateOrderingCode);
                                if (materialIDStep3a == "")
                                {
                                    materialIDStep3a = losdbDbInterface.GetPartNumberMinusEqualEqualMatch(equipmentObject.AlternateOrderingCode);
                                }
                            }
                        }
                    }
                }
                if (materialIDStep3a != "")
                {
                    equipmentObject.PassedStep3a = true;
                    equipmentObject.MaterialID = materialIDStep3a;
                    equipmentObject.Passed = "Step 3a";
                }
                else
                {
                    // step 3.b.  
                    if (equipmentObject.PartNumber != null && equipmentObject.PartNumber != "")
                    {
                        materialIDStep3b = losdbDbInterface.GetPartNumberEqualMatch(equipmentObject.PartNumber);
                        if (materialIDStep3b == "")
                        {
                            if (equipmentObject.LsOrSrs != String.Empty)
                            {
                                materialIDStep3b = losdbDbInterface.GetPartNumberLikeMatch(equipmentObject.PartNumber + "%" + equipmentObject.LsOrSrs);
                            }
                            if (materialIDStep3b == "")
                            {
                                materialIDStep3b = losdbDbInterface.GetPartNumberMinusEqualEqualMatch(equipmentObject.PartNumber);
                                if (materialIDStep3b == "")
                                {
                                    if (equipmentObject.LsOrSrs != String.Empty)
                                    {
                                        materialIDStep3b = losdbDbInterface.GetPartNumberLikeMinusEqualMatch(equipmentObject.PartNumber + "%" + equipmentObject.LsOrSrs);
                                    }
                                }
                            }
                        }
                    }
                    if (materialIDStep3b != "")
                    {
                        equipmentObject.PassedStep3b = true;
                        equipmentObject.MaterialID = materialIDStep3b;
                        equipmentObject.Passed = "Step 3b";
                    }
                    else
                    {
                        // step 3.c.
                        if (equipmentObject.Drawing != null && equipmentObject.Drawing != "")
                        {
                            materialIDStep3c = losdbDbInterface.GetPartNumberEqualMatch(equipmentObject.Drawing);
                            if (materialIDStep3c == "")
                            {
                                if (equipmentObject.DrawingISS != String.Empty)
                                {
                                    materialIDStep3c = losdbDbInterface.GetPartNumberLikeMatch(equipmentObject.Drawing + "%" + equipmentObject.DrawingISS);
                                }
                                if (materialIDStep3c == "")
                                {
                                    materialIDStep3c = losdbDbInterface.GetPartNumberMinusEqualEqualMatch(equipmentObject.Drawing);
                                    if (materialIDStep3c == "")
                                    {
                                        if (equipmentObject.DrawingISS != String.Empty)
                                        {
                                            materialIDStep3c = losdbDbInterface.GetPartNumberLikeMinusEqualMatch(equipmentObject.Drawing + "%" + equipmentObject.DrawingISS);
                                        }
                                    }
                                }
                            }
                        }
                        if (materialIDStep3c != "")
                        {
                            equipmentObject.PassedStep3c = true;
                            equipmentObject.MaterialID = materialIDStep3c;
                            equipmentObject.Passed = "Step 3c";
                        }
                    }
                }
                
                if ((equipmentObject.PassedStep3a || equipmentObject.PassedStep3b || equipmentObject.PassedStep3c) && equipmentObject.MaterialID != lastMaterialID)
                {
                    try
                    {
                        // go ahead and take the fast route and make the assocation.  need to write the prod_id here regardless of
                        // whether the part has revisions or not
                        match++;
                        lastMaterialID = equipmentObject.MaterialID;

                        //losdbDbInterface.InsertMatchTemp(equipmentObject.ProdID, equipmentObject.VendorCode, equipmentObject.Drawing,
                        //    equipmentObject.DrawingISS, equipmentObject.PartNumber, equipmentObject.OrderingCode,
                        //    equipmentObject.LsOrSrs, equipmentObject.AlternateOrderingCode, equipmentObject.MaterialID,
                        //    equipmentObject.Passed);

                        //continue;
                        losdbDbInterface.UpdateMaterialItem(equipmentObject.MaterialID, equipmentObject.ProdID, equipmentObject.EquipmentCatalogItemID);
                        losdbDbInterface.UpdateSAPMaterialWithAssociatedPart(equipmentObject.MaterialID, equipmentObject.ProdID);

                        string materialCatID = losdbDbInterface.GetMaterialCatID(equipmentObject.MaterialID);

                        // at this point, we will also need to add a revision baseline if necessary based on the feature type of the SAP part
                        FeatureType featureType = new FeatureType();
                        featureType = losdbDbInterface.GetFeatureByMtrlID(equipmentObject.MaterialID);

                        // get key data from ies_invntry, ies_ea_clei_extn, ies_ea_main_extn and ies_ea_electrical_extn based on the prod_id
                        string revisionIDColumnName = "";
                        string aliasIDColumnName = "";
                        string revisionID = "";
                        List<string> equipmentCatalogItemIDs = new List<string>();

                        if (materialCatID == "3")  // low level part
                        {
                            // first we need a list of the eqpt_ctlg_item_id based on prod_id from ies_invntry
                            //equipmentCatalogItemIDs = losdbDbInterface.GetInventoryEquipmentCatalogItemIDs(equipmentObject.ProdID);

                            // use the matched equipmentCatalogItemID only
                            equipmentCatalogItemIDs.Add(equipmentObject.EquipmentCatalogItemID);

                            if (equipmentCatalogItemIDs.Count > 0)
                            {
                                foreach (string equipmentCatalogItemID in equipmentCatalogItemIDs)
                                {
                                    // add the equipment IDs to the appropriate *_alias_val table

                                    if (featureType.CDMMSAliasValTableName.ToUpper() == "MTRL_ALIAS_VAL" && featureType.FeatureTypeID <= 10)
                                    {
                                        revisionIDColumnName = "mtrl_id";
                                        aliasIDColumnName = "mtrl_alias_id";
                                        revisionID = equipmentObject.MaterialID;
                                        losdbDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIDColumnName, aliasIDColumnName,
                                        revisionID, "7", equipmentCatalogItemID);
                                    }
                                    else
                                    {
                                        revisionIDColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                                        aliasIDColumnName = "rme_mtrl_revsn_alias_id";
                                        revisionID = losdbDbInterface.GetRevisionID(revisionIDColumnName, equipmentObject.MaterialID);
                                        losdbDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIDColumnName, aliasIDColumnName,
                                        revisionID, "3", equipmentCatalogItemID);
                                    }
                                }
                            }

                            // Get the list of electrical keys 
                            List<string> electricalKeys = new List<string>();
                            electricalKeys = losdbDbInterface.GetElectricalKeys(equipmentObject.ProdID, equipmentObject.EquipmentCatalogItemID);

                            revisionIDColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                            aliasIDColumnName = "rme_mtrl_revsn_alias_id";
                            revisionID = losdbDbInterface.GetRevisionID(revisionIDColumnName, equipmentObject.MaterialID);
                            foreach (string electricalKey in electricalKeys)
                            {
                                losdbDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIDColumnName, aliasIDColumnName,
                                        revisionID, "7", electricalKey);
                            }

                            // Get the list of comp clei keys
                            List<string> compCleiKeys = new List<string>();
                            compCleiKeys = losdbDbInterface.GetCompCleiKeys(equipmentObject.ProdID, equipmentObject.EquipmentCatalogItemID);

                            revisionIDColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                            aliasIDColumnName = "rme_mtrl_revsn_alias_id";
                            revisionID = losdbDbInterface.GetRevisionID(revisionIDColumnName, equipmentObject.MaterialID);
                            foreach (string compCleiKey in compCleiKeys)
                            {
                                losdbDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIDColumnName, aliasIDColumnName,
                                        revisionID, "5", compCleiKey);
                            }

                            // Get the list of clei codes from main extn
                            List<string> cleiCodes = new List<string>();
                            cleiCodes = losdbDbInterface.GetCleiCodes(equipmentObject.ProdID, equipmentObject.EquipmentCatalogItemID);

                            revisionIDColumnName = GetAliasColumnName(featureType.CDMMSAliasValTableName);
                            aliasIDColumnName = "rme_mtrl_revsn_alias_id";
                            revisionID = losdbDbInterface.GetRevisionID(revisionIDColumnName, equipmentObject.MaterialID);
                            foreach (string cleiCode in cleiCodes)
                            {
                                losdbDbInterface.InsertEquipmentIDAliasVal(featureType.CDMMSAliasValTableName, revisionIDColumnName, aliasIDColumnName,
                                        revisionID, "6", cleiCode);
                            }
                        }
                        else if (materialCatID == "2")
                        {
                            // write to minor material and no revisions
                            revisionID = losdbDbInterface.GetHlpRevisionID(equipmentObject.MaterialID);

                            //equipmentCatalogItemIDs = losdbDbInterface.GetInventoryEquipmentCatalogItemIDs(equipmentObject.ProdID);
                            // use the matched equipmentCatalogItemID only
                            equipmentCatalogItemIDs.Add(equipmentObject.EquipmentCatalogItemID);

                            foreach (string equipmentCatalogItemID in equipmentCatalogItemIDs)
                            {
                                losdbDbInterface.InsertHighLevelPart(revisionID, "4", equipmentCatalogItemID);
                            }
                        }
                        else if (materialCatID == "1")
                        {
                            // write to high level part and no revisions
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    //losdbDbInterface.InsertMatchTemp(equipmentObject.ProdID, equipmentObject.VendorCode, equipmentObject.Drawing,
                    //    equipmentObject.DrawingISS, equipmentObject.PartNumber, equipmentObject.OrderingCode,
                    //    equipmentObject.LsOrSrs, equipmentObject.AlternateOrderingCode, equipmentObject.MaterialID,
                    //    equipmentObject.Passed);

                    nomatch++;
                    // Add to the possible association table
                    MaterialItemDBInterface materialItemDBInterface = new MaterialItemDBInterface();

                    if (materialIDStep3a != "")
                    {
                        //materialItemDBInterface.InsertPossibleAssociation(materialIDStep3a, equipmentObject.ProdID);
                    }
                    if (materialIDStep3b != "")
                    {
                        //materialItemDBInterface.InsertPossibleAssociation(materialIDStep3b, equipmentObject.ProdID);
                    }
                    if (materialIDStep3c != "")
                    {
                        //materialItemDBInterface.InsertPossibleAssociation(materialIDStep3c, equipmentObject.ProdID);
                    }
                }
            }
        }

        private string GetAliasColumnName(string aliasTableName)
        {
            string aliasColumnName = "";

            if (aliasTableName.ToUpper() == "MTRL_ALIAS_VAL")
            {
                aliasColumnName = "mtrl_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("RV_ALS_VAL"))
            {
                aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 10) + "revsn_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("REVSN_ALIAS_VAL"))
            {
                aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 15) + "revsn_id";
            }
            else if (aliasTableName.ToUpper().EndsWith("ALIAS_VAL"))
            {
                aliasColumnName = aliasTableName.Substring(0, aliasTableName.Length - 9) + "id";
            }

            return aliasColumnName;
        }

        public void DoMaterialItemUpdates()
        {
            losdbDbInterface.DoMaterialItemUpdates();
        }
    }
}
