using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Polling;
using CenturyLink.Network.Engineering.Common.Utility;
using CenturyLink.Network.Engineering.Common.DbInterface;
using CenturyLink.Network.Engineering.LOSDB.Service.Objects;
using CenturyLink.Network.Engineering.LOSDB.Service.DBInterface;
using CenturyLink.Network.Engineering.TypeLibrary;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace CenturyLink.Network.Engineering.LOSDB.Service.Business
{
    class HandleFlowThru
    {
        public void HandleFlowThruToNDS()
        {
            PartMatching partMatching = new PartMatching();

            //partMatching.DoMaterialItemUpdates(); // copy ies_eqpt_prod_id from alias_val into material_item

            // Initial Load - this is when the CDMMS-LOSDB tables are brand new and empty, only
            // data exists in the IES_* tables.
            //string initialDataLoad = ConfigurationManager.Value("LOSDB_SVC", "InitialDataLoad");
            string initialDataLoad = "NO";
            if (initialDataLoad == "YES")
            {
                partMatching.HandleMatchingParts(true);
            }
            else
            {
                // Do part matching with new records in ies_eqpt
                
                partMatching.HandleMatchingParts(false);

                // Get Changed objects from table AUDIT_IES_DA that match flow thru Y, NDS required Y,
                // action code is C and the cdmms_tbl_nm is not null
                DBInterface.LOSDBDbInterface losdbDbInterface = new DBInterface.LOSDBDbInterface();
                List<AuditObject> auditObjects = losdbDbInterface.GetAuditObjects("Y", "Y", "C");

                // Get the equivalent product ID (material code) that is the alias to the LOSDB product ID

                foreach (AuditObject auditObject in auditObjects)
                {
                    // only care about records that are meaningful to CDMMS
                    if (auditObject.CDMMSTableName != null && auditObject.CDMMSTableName != "")
                    {
                        // have a column and table name so go ahead and do an update
                        ProcessChanges(auditObject);
                    }
                    else { }  // CDMMS Table and Column name are both null
                }

                // Get the Adds with associated Deletes
                List<string> addIDs = new List<string>();
                addIDs = losdbDbInterface.GetListOfAddsWithDelete();

                foreach(string addID in addIDs)
                {
                    // get the associated Add and Delete records that make up the change
                    // and put the old valule into the Add object so it's in one place
                    AuditObject addAuditObject = new AuditObject();
                    addAuditObject = losdbDbInterface.GetAuditObjectByID(addID);
                    AuditObject deleteAuditObject = new AuditObject();
                    deleteAuditObject = losdbDbInterface.GetDeleteObject(addAuditObject.AuditIesColDefID.ToString(), addAuditObject.AuditTablePkColumnValue);
                    addAuditObject.OldColumnValue = deleteAuditObject.OldColumnValue;

                    ProcessChanges(addAuditObject);
                }

                // Add material will not be in audit_ies_da unless it's in combination with a Delete
                // and therefore it's a roundabout way to be a Change
                losdbDbInterface = new DBInterface.LOSDBDbInterface();
                auditObjects = new List<AuditObject>();
                auditObjects = losdbDbInterface.GetAuditObjects("Y", "Y", "A");

                // 1.  Get all Add objects from IES_VNDR_CD_TRNS and add them to MFR.  some will fail on unique contraint but that's ok
                List<VendorObject> vendorObjects = new List<VendorObject>();
                vendorObjects = losdbDbInterface.GetNewVendors();
                losdbDbInterface.InsertVendor(vendorObjects);

                foreach (AuditObject auditObject in auditObjects)
                {
                    // MIKE TODO
                    // 2.  Get the adds for everything else - get scripts from Tim
                    auditObjects = new List<AuditObject>();
                    auditObjects = losdbDbInterface.GetNonVendorAuditObjects("Y", "Y", "A");

                    // Group statements together for inserts

                }

                // Get Deleted objects from table AUDIT_IES_DA that match flow thru Y, NDS required Y and action code D

                // Get Deleted objects from table AUDIT_IES_DA for each of the 3 Delete categories of
                // vendor, inventory and equipment plus the 3 categories of comp clei extn, electrical
                // extn and main extn
                List<VendorObject> deletedVendorObjects = new List<VendorObject>();
                deletedVendorObjects = losdbDbInterface.GetDeletedVendors();

                List<EquipmentObject> deletedEquipmentObjects = new List<EquipmentObject>();
                deletedEquipmentObjects = losdbDbInterface.GetDeletedEquipment();

                List<InventoryObject> deletedInventoryObjects = new List<InventoryObject>();
                deletedInventoryObjects = losdbDbInterface.GetDeletedInventory();

                List<CompCleiExtnObject> deletedCompCleiExtnObjects = new List<CompCleiExtnObject>();
                deletedCompCleiExtnObjects = losdbDbInterface.GetDeletedCompCleiExtn();

                List<ElectricalExtnObject> deletedElectricalExtnObjects = new List<ElectricalExtnObject>();
                deletedElectricalExtnObjects = losdbDbInterface.GetDeletedElectricalExtn();

                List<MainExtnObject> deletedMainExtnObjects = new List<MainExtnObject>();
                deletedMainExtnObjects = losdbDbInterface.GetDeletedMainExtn();

                losdbDbInterface = new DBInterface.LOSDBDbInterface();
                auditObjects = new List<AuditObject>();
                auditObjects = losdbDbInterface.GetAuditObjects("Y", "Y", "D");

                // This is simplified to sending an email report of the deleted objects

                // Put together an excel spreadsheet for an email attachment
                /*
                Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                int row = 1;
                xlWorkSheet.Cells[row, 1] = "LOSDB Delete Report for " + DateTime.Today.ToShortDateString();
                row += 2;

                // Vendor Deletes
                xlWorkSheet.Cells[row, 1] = "Vendor Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "Vendor Name";
                xlWorkSheet.Cells[row, 2] = "Vendor Code";
                row++;
                foreach (VendorObject deletedVendorObject in deletedVendorObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedVendorObject.VendorName;
                    xlWorkSheet.Cells[row, 2] = deletedVendorObject.VendorCode;
                    row++;
                }

                // Equipment Deletes
                row++;
                xlWorkSheet.Cells[row, 1] = "Equipment Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "Vendor Code";
                xlWorkSheet.Cells[row, 2] = "Drawing";
                xlWorkSheet.Cells[row, 3] = "Drawing ISS";
                xlWorkSheet.Cells[row, 4] = "Description";
                row++;
                foreach (EquipmentObject deletedEquipmentObject in deletedEquipmentObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedEquipmentObject.VendorCode;
                    xlWorkSheet.Cells[row, 2] = deletedEquipmentObject.Drawing;
                    xlWorkSheet.Cells[row, 3] = deletedEquipmentObject.DrawingISS;
                    xlWorkSheet.Cells[row, 4] = deletedEquipmentObject.Description;
                    row++;
                }

                // Inventory Deletes
                row++;
                xlWorkSheet.Cells[row, 1] = "Inventory Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "CLEI Code";
                xlWorkSheet.Cells[row, 2] = "LS or SRS";
                xlWorkSheet.Cells[row, 3] = "Ordering Code";
                xlWorkSheet.Cells[row, 4] = "Description";
                row++;
                foreach (InventoryObject deletedInventoryObject in deletedInventoryObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedInventoryObject.CleiCode;
                    xlWorkSheet.Cells[row, 2] = deletedInventoryObject.LsOrSrs;
                    xlWorkSheet.Cells[row, 3] = deletedInventoryObject.OrderingCode;
                    xlWorkSheet.Cells[row, 4] = deletedInventoryObject.Description;
                    row++;
                }

                // Comp Clei Extn Deletes
                row++;
                xlWorkSheet.Cells[row, 1] = "Comp Clei Extn Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "CLEI Code";
                xlWorkSheet.Cells[row, 2] = "Compatible Equipment Clei 7";
                row++;
                foreach (CompCleiExtnObject deletedCompCleiExtnObject in deletedCompCleiExtnObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedCompCleiExtnObject.CleiCode;
                    xlWorkSheet.Cells[row, 2] = deletedCompCleiExtnObject.CompatibleEquipmentClei7;
                    row++;
                }

                // Electrical Extn Deletes
                row++;
                xlWorkSheet.Cells[row, 1] = "Electrical Extn Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "CLEI Code";
                xlWorkSheet.Cells[row, 2] = "ERInputVoltageFromUnit";
                xlWorkSheet.Cells[row, 3] = "ERInputVoltageFromValue";
                xlWorkSheet.Cells[row, 4] = "ERInputVoltageToUnit";
                xlWorkSheet.Cells[row, 5] = "ERInputVoltageToValue";
                xlWorkSheet.Cells[row, 6] = "ERInputVoltageFreqFromUnit";
                xlWorkSheet.Cells[row, 7] = "ERInputVoltageFreqFromValue";
                xlWorkSheet.Cells[row, 8] = "ERInputVoltageFreqToUnit";
                xlWorkSheet.Cells[row, 9] = "ERInputVoltageFreqToValue";
                xlWorkSheet.Cells[row, 10] = "ERInputCurrentFromUnit";
                xlWorkSheet.Cells[row, 11] = "ERInputCurrentFromValue";
                xlWorkSheet.Cells[row, 12] = "ERInputCurrentToUnit";
                xlWorkSheet.Cells[row, 13] = "ERInputCurrentToValue";
                xlWorkSheet.Cells[row, 14] = "ERFreeFormType";
                row++;
                foreach (ElectricalExtnObject deletedElectricalExtnObject in deletedElectricalExtnObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedElectricalExtnObject.CleiCode;
                    xlWorkSheet.Cells[row, 2] = deletedElectricalExtnObject.ERInputVoltageFromUnit;
                    xlWorkSheet.Cells[row, 3] = deletedElectricalExtnObject.ERInputVoltageFromValue;
                    xlWorkSheet.Cells[row, 4] = deletedElectricalExtnObject.ERInputVoltageToUnit;
                    xlWorkSheet.Cells[row, 5] = deletedElectricalExtnObject.ERInputVoltageToValue;
                    xlWorkSheet.Cells[row, 6] = deletedElectricalExtnObject.ERInputVoltageFreqFromUnit;
                    xlWorkSheet.Cells[row, 7] = deletedElectricalExtnObject.ERInputVoltageFreqFromValue;
                    xlWorkSheet.Cells[row, 8] = deletedElectricalExtnObject.ERInputVoltageFreqToUnit;
                    xlWorkSheet.Cells[row, 9] = deletedElectricalExtnObject.ERInputVoltageFreqToValue;
                    xlWorkSheet.Cells[row, 10] = deletedElectricalExtnObject.ERInputCurrentFromUnit;
                    xlWorkSheet.Cells[row, 11] = deletedElectricalExtnObject.ERInputCurrentFromValue;
                    xlWorkSheet.Cells[row, 12] = deletedElectricalExtnObject.ERInputCurrentToUnit;
                    xlWorkSheet.Cells[row, 13] = deletedElectricalExtnObject.ERInputCurrentToValue;
                    xlWorkSheet.Cells[row, 14] = deletedElectricalExtnObject.ERFreeFormType;
                    row++;
                }

                // Main Extn Deletes
                row++;
                xlWorkSheet.Cells[row, 1] = "Main Extn Deletes";
                row++;
                xlWorkSheet.Cells[row, 1] = "CLEI Code";
                xlWorkSheet.Cells[row, 2] = "Physical Description";
                xlWorkSheet.Cells[row, 3] = "Stenciling";
                row++;
                foreach (MainExtnObject deletedMainExtnObject in deletedMainExtnObjects)
                {
                    xlWorkSheet.Cells[row, 1] = deletedMainExtnObject.CleiCode;
                    xlWorkSheet.Cells[row, 2] = deletedMainExtnObject.PhysicalDescription;
                    xlWorkSheet.Cells[row, 3] = deletedMainExtnObject.Stenciling;
                    row++;
                }

                //xlWorkBook.SaveAs("c:\\csharp-Excel.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorkSheet);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(xlApp);
                */
            }
        }

        private void ProcessChanges (AuditObject addAuditObject)
        {
            Common.DbInterface.LOSDBDbInterface commonLOSDBDbInterface = new Common.DbInterface.LOSDBDbInterface();
            // based on what kind of record this is do the update
            if (addAuditObject.AuditTablePkColumnName == "VENDR_CD")
            {
                if (addAuditObject.CDMMSColumnName != null && addAuditObject.CDMMSColumnName != "")
                {
                    commonLOSDBDbInterface.UpdateVendor(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditTablePkColumnName);
                }
            }
            else if (addAuditObject.AuditTablePkColumnName == "PROD_ID")
            {
                // only care if this has CDMMS ramifications
                if (addAuditObject.CDMMSColumnName != null && addAuditObject.CDMMSColumnName != "")
                {
                    if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("REVSN"))
                    {
                        commonLOSDBDbInterface.UpdateEquipmentRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.LOSDBProdID);
                    }
                    else if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("MTRL"))
                    {
                        // there are none of these listed so far
                    }
                    else
                    {
                        commonLOSDBDbInterface.UpdateEquipment(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.LOSDBProdID);
                    }
                }
            }
            else if (addAuditObject.AuditTablePkColumnName == "EQPT_CTLG_ITEM_ID")
            {
                if (addAuditObject.CDMMSColumnName != null && addAuditObject.CDMMSColumnName != "")
                {
                    if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("REVSN"))
                    {
                        commonLOSDBDbInterface.UpdateInventoryRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditTablePkColumnValue);
                    }
                    else if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("MTRL"))
                    {
                        // there are none of these listed so far
                    }
                    else
                    {
                        commonLOSDBDbInterface.UpdateInventory(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditTablePkColumnValue);
                    }
                }
            }
            else if (addAuditObject.AuditTablePkColumnName == "COMP_CLEI_KEY" && addAuditObject.AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
            {
                if (addAuditObject.CDMMSColumnName != null && addAuditObject.CDMMSColumnName != "")
                {
                    if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("REVSN"))
                    {
                        commonLOSDBDbInterface.UpdateParentRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditParentTablePkColumnValue, "*_MTRL_REVSN");
                    }
                    else if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("MTRL"))
                    {
                        commonLOSDBDbInterface.UpdateParentRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditParentTablePkColumnValue, "*_MTRL");
                    }
                }
            }
            else if (addAuditObject.AuditTablePkColumnName == "ELECTRICAL_KEY" && addAuditObject.AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
            {
                if (addAuditObject.CDMMSColumnName != null && addAuditObject.CDMMSColumnName != "")
                {
                    if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("REVSN"))
                    {
                        commonLOSDBDbInterface.UpdateParentRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditParentTablePkColumnValue, "*_MTRL_REVSN");
                    }
                    else if (addAuditObject.CDMMSTableName.StartsWith("*") && addAuditObject.CDMMSTableName.ToUpper().EndsWith("MTRL"))
                    {
                        commonLOSDBDbInterface.UpdateParentRevision(addAuditObject.CDMMSTableName, addAuditObject.CDMMSColumnName, addAuditObject.NewColumnValue, addAuditObject.AuditParentTablePkColumnValue, "*_MTRL");
                    }
                }
            }
            else if (addAuditObject.AuditTablePkColumnName == "CLEICODE" && addAuditObject.AuditParentTablePKColumnName == "EQPT_CTLG_ITEM_ID")
            {
                // no CDMMS tables to update for this scenario
            }
        }

        private void SendToWorkToDo (AuditObject auditObject)
        {
            // MIKE TODO - This needs to change so that the CDMMS record gets 
            // sent to the work_to_do just like the other service
            // Insert into work_to_do table
            ChangeSet changeSet = new ChangeSet();
            changeSet.ChangeSetId = long.Parse(auditObject.SAPMaterialCode);

            //productIDs.Add(catDataLoad.MATNR);

            ChangeRecord changeRecord = new ChangeRecord();
            changeRecord.TableName = "NDS_MATERIAL_ITEM_VW";

            WorkDbInterface workDbInterface = new WorkDbInterface();
            long workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SVC");
        }
    }
}
