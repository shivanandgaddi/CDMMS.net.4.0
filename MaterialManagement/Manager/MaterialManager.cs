using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
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
    public class MaterialManager
    {
        private bool needErrorLogging = false;
        private string customResponsebackMesssage = string.Empty;
        private string errorMessagetoLog = string.Empty;
        //Set default Severity for logging error/infomration
        private ExceptionConstants.Severity ErrorInfoSeverity = ExceptionConstants.Severity.Major;
        //Set default error method to "ProcessMessage" method
        private int sentryIdentifier = (int)CenturyLink.Network.Engineering.Material.Management.Business.Utility.Constants.SentryIdentifier.ProcessMessage;

        public bool ValidateXml(ref string xml)
        {
            ParseCatalogXML parseItem = new ParseCatalogXML();

            return parseItem.ValidateXml(ref xml);
        }

        public void ProcessMessage(string message)
        {
            //Log start info            
            ApplicationFieldConfiguration.LogInfoMsg("XML Processing started");

            try
            {
                // below object takes care of DB operations
                CatalogProcessingManager catalogMgr = new CatalogProcessingManager();
                OSPFMSendMail NotifySendMail = new OSPFMSendMail();
                OSPFMValidations Validations = new OSPFMValidations();
                ArrayList alCatalog = new ArrayList();
                long lCurrStageID = -1;
                long lPrevStageID = -1;
                long lBatchID = -1;
                bool bHasDataLoadErrors = false;
                ArrayList alSuccessMails = new ArrayList();
                ArrayList alFailureMails = new ArrayList();
                List<CatalogItem> CatalogItemCollection = null;
                List<CatalogItem> FalloutCatalogItemCollection = new List<CatalogItem>();
                List<string> deleteStrings = new List<string>();  // make a collection to hold the deletes
                string validationText = string.Empty;

                // validate xml schema
                EventLogger.LogInfo("Validating XML");
                bool isValidXML = ValidateXml(ref message);

                // if any xml schema/format error found, then stop xml processing any further
                if (isValidXML)
                {
                    ParseCatalogXML parseItem = new ParseCatalogXML();

                    // create a collection of dto for each catalog details node in recieved xml data
                    // create a collection of dto for each catalog details node that falls through the cracks
                    EventLogger.LogInfo("Creating Collection");
                    CatalogItemCollection = parseItem.PopulateDTOFromMessage(message, ref FalloutCatalogItemCollection);

                    deleteStrings.Add("The following parts have the deletion indicator set to X<br /><br />");
                    int count = 0;
                    foreach (CatalogItem thisItem in CatalogItemCollection)
                    { 
                        if (thisItem.LVORM == "X")
                        {
                            // This record has been flagged for a delete, so let's get what we need for an email.
                            deleteStrings.Add("Part Number: " + thisItem.MFRPN + "<br />");
                            deleteStrings.Add("CLMC: " + thisItem.MFRNR + "<br />");
                            deleteStrings.Add("APCL: " + thisItem.APCL_CODE + "<br />");
                            deleteStrings.Add("Descr: " + thisItem.PO_TEXT + "<br /><br />");
                            count++;
                        }
                    }
                    deleteStrings.Add("<br />");
                    if (count == 0)
                    {
                        deleteStrings = new List<string>();
                    }

                    // if there is atleast one catalogitem detail found, then process dto to insert to DB
                    if (CatalogItemCollection != null && CatalogItemCollection.Count > 0)
                    {
                        //wasCurrentXMLProcessingSuccessfull = true;

                        //Insert batch record with batch status as 'RD' to convey "RECEIVE DATA"
                        lBatchID = catalogMgr.InsertIntoBatchProcTbl("RD", lBatchID);
                        EventLogger.LogInfo("Batch id is " + lBatchID.ToString());

                        // assign batchId to validation class object to use for all batchId referenced places
                        Validations.lBatchID = lBatchID;

                        #region History Archiving

                        EventLogger.LogInfo("Archiving");
                        ArchiveCatalogStagingData();

                        #endregion

                        #region Data Validations

                        validationText = "The following parts failed processing.<br /><br />";
                        Validations.DoValidation(ref CatalogItemCollection, lBatchID, ref validationText);
                        validationText += "The following parts were processed successfully.<br /><br />";

                        #endregion

                        // Move any invalid from CatalogItemCollection to FalloutCatalogItemCollection
                        try
                        {
                            for (int i = CatalogItemCollection.Count - 1; i >= 0; i--)
                            {
                                if (CatalogItemCollection[i].ValidStatus == "INVALID")
                                {
                                    FalloutCatalogItemCollection.Add(CatalogItemCollection[i]);
                                    CatalogItemCollection.RemoveAt(i);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string mike = ex.Message;
                        }

                        //Log start info            
                        ApplicationFieldConfiguration.LogInfoMsg("OSPFM validations completed...");

                        // Insert all dto in collection to DB
                        EventLogger.LogInfo("Inserting to material_catalog_staging");
                        catalogMgr.InsertIntoCatalogStagingTbl(CatalogItemCollection, lBatchID, false);
                        EventLogger.LogInfo("Inserting to material_catalog_fallout");
                        catalogMgr.InsertIntoCatalogStagingTbl(FalloutCatalogItemCollection, lBatchID, true); // true for fallout collection

                        // Instead of sending a response to Tibco send message to logging table
                        if (catalogMgr.wasCurrentDBUpdateSuccessfull)
                        {
                            needErrorLogging = false;
                            //needResponseback = true;
                            customResponsebackMesssage = "DB/XML processing Status: XML processing was successfull.";
                        }
                        else
                        {
                            needErrorLogging = true;
                            ErrorInfoSeverity = ExceptionConstants.Severity.Major;
                            sentryIdentifier = (int)CenturyLink.Network.Engineering.Material.Management.Business.Utility.Constants.SentryIdentifier.ProcessMessage;
                            //needResponseback = true;
                            customResponsebackMesssage = "DB/XML processing Status: XML processing was unsuccessfull as DB operation didn't succeed. Please refer application error log for more details.";

                            errorMessagetoLog = customResponsebackMesssage;
                        }

                        #region Insert Error into Error Table and Build Mail for Validation Failure
                        Validations.InsertIntoStagingErrorTable(CatalogItemCollection);
                        #endregion                           

                        #region Insert into mtl_item_SAP table
                        // Flow through indicator logic
                        // If the catalog item qualifies for flow through, send it to the work_to_do table and mtl_item_sap table
                        // If not flow through, then add it to the mtl_item_sap table with a review_ind = 'Y' so the GUI can pull it up
                        // A flow through target will be a record that already exists, and pass all the flow through logic in material_flow_thru
                        // If not a flow through, the record will be set for user intervention
                        CheckFlowThru(ref CatalogItemCollection);

                        int nNectasChgAttb = -1;

                        PackageOutputDTO pckOut = new PackageOutputDTO();

                        lPrevStageID = -1;
                        lCurrStageID = -1;

                        foreach (CatalogItem catDataLoad in CatalogItemCollection)
                        {
                            // mtl_item_sap is now the "table of record" for both outside and inside
                            // plant so we should have non-OSPFM candidates here

                            //if (!(ParseCatalogXML.GetRecordType(catDataLoad).Equals("OSPFM")))
                            //    continue;

                            if (Validations.IsValidDTO(catDataLoad))
                            {
                                lCurrStageID = catDataLoad.StageID;

                                /*Update Batch Process Table*/
                                if (lPrevStageID == -1)
                                    catalogMgr.InsertIntoBatchProcTbl("LS", catDataLoad.BatchID); // LS - DATA LOAD START

                                if (lPrevStageID != lCurrStageID)
                                {
                                    //Build Mail Body and add to Success and Failure Arraylist
                                    if (catDataLoad.ValidStatus == "VALID")
                                    {
                                        MaterialItemDBInterface materialItemDBInterface = new MaterialItemDBInterface();

                                        // Validate against mtl_item_category table and insert into Mtl_Item_SAP Table
                                        // Only insert if the record qualifies as a flow-thru
                                        if(catDataLoad.FlowThruIndicator == "Y")
                                        {
                                            pckOut = catalogMgr.InsertIntoMtlPSTable(catDataLoad, ref nNectasChgAttb);

                                            bHasDataLoadErrors = bHasDataLoadErrors && (pckOut.ReturnValue == -1 ? true : false);

                                            if (pckOut.ReturnValue != -1)
                                                alSuccessMails.Add(NotifySendMail.BuildSuccessMail(catDataLoad.MATNR, catDataLoad.LVORM, catDataLoad.ZMAKTX, nNectasChgAttb));
                                            else
                                                alFailureMails.Add(NotifySendMail.BuildFailureMail(catDataLoad.MATNR, catDataLoad.LVORM, catDataLoad.ZMAKTX, pckOut));
                                        }
                                    }
                                    lPrevStageID = catDataLoad.StageID;
                                }
                            }
                        }

                        //StringCollection productIDs = new StringCollection();
                        //Client client = new Client(ConfigurationManager.Value("NDSServer"), ConfigurationManager.Value("NDSPort"));
                        //string[] results = null;

                        foreach (CatalogItem catDataLoad in CatalogItemCollection)
                        {
                            // After inserting/updating into mtl_item_sap check the flow through indicator
                            // for an insert to the work_to_do table.  do this after the update of the mtl_item_sap table.

                            //CheckForDuplicateFlowThru(ref CatalogItemCollection);  // Check for overriding instances when there are multiple updates in one xml file.

                            //if (!productIDs.Contains(catDataLoad.MATNR) && catDataLoad.FlowThruIndicator == "Y")
                            if (catDataLoad.FlowThruIndicator == "Y")
                            {                                
                                WorkDbInterface workDbInterface = new WorkDbInterface();

                                ChangeSet changeSet = new ChangeSet();
                                changeSet.ChangeSetId = long.Parse(catDataLoad.MATNR);

                                //productIDs.Add(catDataLoad.MATNR);

                                ChangeRecord changeRecord = new ChangeRecord();
                                changeRecord.TableName = "MTL_ITEM_SAP";

                                changeRecord.PniId = catDataLoad.StageID.ToString();
                                changeSet.ProjectId = catDataLoad.BatchID;

                                long workToDoID = workDbInterface.InsertWorkToDo(changeSet, changeRecord, "CATALOG_SVC");

                                if (workToDoID > 0)
                                {
                                    // This was a flow thru so add to the modified email text
                                    AddToEmail(catDataLoad, ref validationText, true);

                                    // Update the needs_to_be_reviewed in material_staging to N
                                    MaterialItemDBInterface matl = new MaterialItemDBInterface();
                                    matl.UpdateNeedsToBeReviewed(catDataLoad.BatchID, catDataLoad.StageID);
                                }

                                // MFORD commented due to the SWConnector call moving to the NEISL windows service for licensing purposes
                                //if (workToDoID > 0)
                                //{
                                //    // insert into work_to_do successful
                                //    try
                                //    {
                                //        client.Connect();
                                //        workDbInterface.InsertMasterPart(catDataLoad.MATNR);  // Need to make sure the product_id is in material_item
                                //        results = client.RunProcedure(ConfigurationManager.Value("NDSStoredProc"), workToDoID.ToString());
                                //    }
                                //    catch(Exception ex)
                                //    {
                                //        EventLogger.LogException(ex); // Log the exception but continue with processing so we update with change set error
                                //    }

                                //    if (results != null && results.Length > 0  && results[0].ToString() == "Update Success")
                                //    {
                                //        // call to SWConnector successful

                                //        EventLogger.LogInfo(string.Format("Result from SWConnector call to procedure {0}: {1}", ConfigurationManager.Value("NDSStoredProc"), results[0]));
                                //        workDbInterface.UpdateWorkToDo(workToDoID, CHANGE_SET_STATUS.COMPLETED);

                                //        // Update the needs_to_be_reviewed in material_staging to N
                                //        MaterialItemDBInterface matl = new MaterialItemDBInterface();
                                //        matl.UpdateNeedsToBeReviewed(catDataLoad.BatchID, catDataLoad.StageID);

                                //        // Send the changes out in an email
                                //        // SendUpdateEmail(catDataLoad);
                                //    }
                                //    else
                                //    {
                                //        // call to SWConnector had a failure
                                //        // since the flow thru failed mtl_item_sap needs to be changed to be manually looked at

                                //        EventLogger.LogInfo(string.Format("No results returned from SWConnector call to procedure {0}. Unable to process change set id {1} further.", ConfigurationManager.Value("NDSStoredProc"), changeSet.ChangeSetId));
                                //        workDbInterface.UpdateWorkToDo(workToDoID, CHANGE_SET_STATUS.ERROR);
                                //    }
                                //}
                                //else
                                //{
                                //    // insert into work_to_do failed.  nothing to do here since the needs_to_be_reviewed indicator is defaulted to Y
                                //}
                            }
                            else
                            {
                                // Didn't qualify for flow thru, build email to notify users
                                if( !catDataLoad.FailedProcessing )
                                {
                                    AddToEmail(catDataLoad, ref validationText, false);
                                }
                            }
                        }

                        if (!bHasDataLoadErrors)
                            catalogMgr.InsertIntoBatchProcTbl("LP", lBatchID); //LP - DATA LOAD PASS

                        #endregion                           

                        #region Send Mail
                        if( deleteStrings != null && deleteStrings.Count > 0)
                        {
                            SendDeleteEmail(deleteStrings);
                        }
                        if( validationText != string.Empty )
                        {
                            SendFailUpdateEmail(validationText);
                        }
                        #endregion
                    }
                    else
                    {
                        //wasCurrentXMLProcessingSuccessfull = false;

                        needErrorLogging = true;
                        ErrorInfoSeverity = ExceptionConstants.Severity.Normal;
                        sentryIdentifier = (int)CenturyLink.Network.Engineering.Material.Management.Business.Utility.Constants.SentryIdentifier.ProcessMessage;
                        //needResponseback = true;
                        customResponsebackMesssage = "XML message does not have any valid catalog item detail to process.";
                        errorMessagetoLog = customResponsebackMesssage;
                    }

                    if (FalloutCatalogItemCollection.Count > 0)
                    {
                        // insert the fallout catalog items to material_catalog_fallout table
                    }
                }
                else
                {
                    needErrorLogging = true;
                    ErrorInfoSeverity = ExceptionConstants.Severity.Major;
                    sentryIdentifier = (int)CenturyLink.Network.Engineering.Material.Management.Business.Utility.Constants.SentryIdentifier.ProcessMessage;
                    //needResponseback = true;
                    customResponsebackMesssage = "XML message is not in required format. Please refer to the application error log for more details.";
                    errorMessagetoLog = customResponsebackMesssage;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Unable to process XML message as error has occurred in method ProcessMessage - " + ex.Message;
                EventLogger.LogException(ex, sErrMsg);
            }
        }

        private static void ArchiveCatalogStagingData()
        {
            //we can read the number of days for archiving from the system_configuration database

            CatalogProcessingManager oManager = new CatalogProcessingManager();

            oManager.ArchiveCatalogData(Common.Configuration.ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "ArchivePeriodInDays"));
        }

        private void CheckFlowThru(ref List<CatalogItem> catalogItemCollection)
        {
            try
            {
                MaterialFlowThruDbInterface materialDB = new MaterialFlowThruDbInterface();
                MaterialItemDBInterface materialItemDB = new MaterialItemDBInterface();

                // Populate the flow thru indicators as designed by the users
                Dictionary<string, string> flowThruIndicators = materialDB.GetFlowThruIndicators("SAP");

                for (int i = 0; i < catalogItemCollection.Count(); i++)
                {
                    bool anyNo = false;
                    bool foundOne = false;
                    List<string> emailBodyCollection = new List<string>();

                    if (!catalogItemCollection[i].FailedProcessing)
                    {
                        // Set the record's flow thru indicator to N as a default
                        // This means that all new material will have a flow thru indicator of N
                        catalogItemCollection[i].FlowThruIndicator = "N";

                        // Get the original catalog item record, if there is any
                        CatalogItem originalCatalogItem = new CatalogItem();
                        originalCatalogItem = materialItemDB.GetMtlPSTableNew(catalogItemCollection[i].MATNR);
                        
                        if (originalCatalogItem.MATNR != null && originalCatalogItem.MATNR != "")
                        {
                            // Found the original, now need to compare fields.  Any change to a field that has a No as a flow thru indicator
                            // means this record does not qualify for flow thru
                            foreach (KeyValuePair<string, string> entry in flowThruIndicators)
                            {
                                // Using reflection to get object values
                                string newValue = "";
                                string originalValue = "";
                                PropertyInfo prop = catalogItemCollection[i].GetType().GetProperty(entry.Key);
                                if (prop != null)
                                {
                                    newValue = prop.GetValue(catalogItemCollection[i]).ToString();
                                }
                                PropertyInfo prop2 = originalCatalogItem.GetType().GetProperty(entry.Key);
                                if (prop != null)
                                {
                                    originalValue = prop2.GetValue(originalCatalogItem).ToString();
                                }

                                if (newValue != originalValue)
                                {
                                    if (entry.Value == "N")
                                    {
                                        // if the field from the XML file does not match the original value as stored in mtl_item_sap
                                        // and the flow thru indicator is No then this record does not qualify for flow thru
                                        anyNo = true;
                                    }
                                    else
                                    {
                                        foundOne = true;
                                        // found a difference so let's store this away so that we can have the beginings
                                        // of an email 
                                        emailBodyCollection.Add("The " + entry.Key + " has changed from " + originalValue + " to " + newValue);
                                    }
                                }
                            }
                        }
                    }
                    if (anyNo)  // Any one item that's different but has a No indicator voids any chance of having flow thru
                    {
                        catalogItemCollection[i].FlowThruIndicator = "N";
                    }
                    else if (foundOne)
                    {
                        catalogItemCollection[i].FlowThruIndicator = "Y";
                        // Set the email body from the changes
                        foreach(String change in emailBodyCollection)
                        {
                            catalogItemCollection[i].EmailBody += change + "<br />";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Unable to process Check Flow Thru - " + ex.Message;
                EventLogger.LogException(ex, sErrMsg);
            }
        }
        private void CheckForDuplicateFlowThru(ref List<CatalogItem> catalogItemCollection)
        {
            StringDictionary productIDs = new StringDictionary();
            for (int i = 0; i < catalogItemCollection.Count(); i++)
            {
                // Make a list of all product IDs and indicators.  Let flow thru indicator of N override Y
                if (productIDs.ContainsKey(catalogItemCollection[i].MATNR))
                {
                    if (catalogItemCollection[i].FlowThruIndicator == "N" && productIDs[catalogItemCollection[i].MATNR] == "Y")
                    {
                        productIDs[catalogItemCollection[i].MATNR] = "N";  // N always overrids Y
                    }
                }
                else  // Add it
                {
                    productIDs.Add(catalogItemCollection[i].MATNR, catalogItemCollection[i].FlowThruIndicator);
                }
            }
            for (int i = 0; i < catalogItemCollection.Count(); i++)
            {
                catalogItemCollection[i].FlowThruIndicator = productIDs[catalogItemCollection[i].MATNR];
            }
        }

        //private void SendUpdateEmail(CatalogItem catalogItem)
        //{
        //    SmtpClient smtpClient = new SmtpClient(ConfigurationManager.Value("SMTPServer"));
        //    MailMessage mail = new MailMessage();
        //    mail.Subject = "CDMMS Update For " + catalogItem.MATNR;
        //    mail.Body = catalogItem.EmailBody;
        //    mail.From = new MailAddress(ConfigurationManager.Value("MailFrom"));
        //    mail.To.Add(new MailAddress(ConfigurationManager.Value("MailTo")));
        //    mail.IsBodyHtml = true;
        //    smtpClient.Send(mail);
        //}

        private void AddToEmail(CatalogItem catalogItem, ref string validationText, bool flowThru)
        {
            try
            {
                validationText += "Part Number: " + catalogItem.MFRPN + "<br />";
                validationText += "CLMC: " + catalogItem.MFRNR + "<br />";
                validationText += "APCL: " + catalogItem.APCL_CODE + "<br />";
                if (flowThru)
                {
                    validationText += "Descr: " + catalogItem.PO_TEXT + ", Modified Flow Thru" + "<br /><br />";
                }
                else
                {
                    // Is this part new?
                    CatalogItem originalCatalogItem = new CatalogItem();
                    MaterialItemDBInterface materialItemDB = new MaterialItemDBInterface();
                    originalCatalogItem = materialItemDB.GetMtlPSTableNew(catalogItem.MATNR);
                    if (originalCatalogItem.MATNR == string.Empty)
                    {
                        validationText += "Descr: " + catalogItem.PO_TEXT + ", New" + "<br /><br />";

                        // For all new parts, run through the new US198047 LOSDB part matching process
                        PartMatchingManager partMatchingManager = new PartMatchingManager();
                        partMatchingManager.MatchSAPPartToLOSDBPart(catalogItem);
                    }
                    else
                    {
                        validationText += "Descr: " + catalogItem.PO_TEXT + ", Modified" + "<br /><br />";
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SendDeleteEmail(List<string> deleteStrings)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "SMTPServer"));
                MailMessage mail = new MailMessage();
                mail.Subject = "Catalog Deletion Notification";
                foreach (string dString in deleteStrings)
                {
                    mail.Body += dString;
                }
                mail.From = new MailAddress(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "MailFrom"));
                mail.To.Add(new MailAddress(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "MailTo")));
                mail.IsBodyHtml = true;
                smtpClient.Send(mail);
            }
            catch (Exception)
            {
            }
        }

        private void SendFailUpdateEmail(string validationString)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "SMTPServer"));
                MailMessage mail = new MailMessage();
                mail.Subject = "Catalog Notification";
                mail.Body = validationString;
                mail.From = new MailAddress(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "MailFrom"));
                mail.To.Add(new MailAddress(ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "MailTo")));
                mail.IsBodyHtml = true;
                smtpClient.Send(mail);
            }
            catch(Exception)
            {
            }
        }
    }
}
