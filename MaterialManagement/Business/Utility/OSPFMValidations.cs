using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Management.Business.DTO;
using CenturyLink.Network.Engineering.Material.Management.Business.Exceptions;
using CenturyLink.Network.Engineering.Material.Management.Manager;
using CenturyLink.Network.Engineering.TypeLibrary;
using System.Net.Mail;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Utility
{
    internal class OSPFMValidations
    {
        CatalogProcessingManager catalogMgr = new CatalogProcessingManager();

        //08/05/2011
        ArrayList alSuccessMails = new ArrayList();
        ArrayList alFailureMails = new ArrayList();
        OSPFMSendMail SendMail = new OSPFMSendMail();
        public long lBatchID = -1;

        //*ArrayList alCatalogStagingDTOList = new ArrayList();
        ArrayList alValidationErrors = new ArrayList();
        //*ArrayList alCatalog = new ArrayList();
        bool bHasValidationErrors = false;

        #region Validating Data Fields

        public bool IsValidDTO(CatalogItem catStgDTO)
        {
            bool isValidDTO = true;
            if (catStgDTO.MATNR != string.Empty)
                isValidDTO = true;
            else if (catStgDTO.MATNR == string.Empty)
            {
                bHasValidationErrors = true;
                alFailureMails.Add(SendMail.BuildFailureMail(catStgDTO.MATNR, catStgDTO.LVORM, catStgDTO.ZMAKTX, catStgDTO.StageID, alValidationErrors));
                isValidDTO = false;

            }
            return isValidDTO;
        }
        public void DoValidation(ref List<CatalogItem> alCatalogStagingDTOList, long lBatchID, ref string validationErrors)
        {
            StringDictionary MtlGrpAppNamList = catalogMgr.FetchMaterialGrps("OSP-FM", false);
            ArrayList alCatalogErrors = catalogMgr.FetchCatalogErrorCodes();
            ArrayList alCatalogErrorTexts = catalogMgr.FetchCatalogErrorTexts();
            //ArrayList alICCForOrderableItems = catalogMgr.FetchICCCodes();  // Can skip this test
            ArrayList alValidUOMs = catalogMgr.FetchNectasUOMs();
            string sErrMsg = String.Empty;
            string failedEmailText = "The following parts failed processing.<br /><br />";

            #region
            long currentStage_Id = 0;
            catalogMgr.InsertIntoBatchProcTbl("VS", lBatchID);
            foreach (CatalogItem CatalogItem in alCatalogStagingDTOList)
            {
                string errorStrings = "";
                currentStage_Id++;

                //if ((CatalogItem.MATNR != String.Empty && MtlGrpAppNamList.ContainsKey(CatalogItem.MATKL + "~")) && (ParseCatalogXML.GetRecordType(CatalogItem).Equals("OSPFM")))
                if ((CatalogItem.MATNR != String.Empty && MtlGrpAppNamList.ContainsKey(CatalogItem.MATKL + "~")))
                {
                    CatalogItem.BatchID = lBatchID;
                    if (CatalogItem.MATNR.Substring(0,1) == "R")
                    {
                        CatalogItem.ValidStatus = "INVALID";
                        CatalogItem.FailedProcessing = true;
                    }

                    if (CatalogItem.MTART != "ZCTE")
                    {
                        CatalogItem.ValidStatus = "INVALID";
                        CatalogItem.FailedProcessing = true;
                    }

                    #region Part No Missing  (SAP field - MFRPN)
                    if (CatalogItem.MFRPN == String.Empty)
                    {
                        sErrMsg = ResolveErrMsg("E00001", CatalogItem.MATNR + "is:" + CatalogItem.MFRPN, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00001", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00001 " + GetErrorText(alCatalogErrorTexts, "E00001") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    if (CatalogItem.MFRPN.Length > 30 )
                    {
                        sErrMsg = ResolveErrMsg("E00011", CatalogItem.MATNR + "is:" + CatalogItem.MFRPN, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00011", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00011 " + GetErrorText(alCatalogErrorTexts, "E00011") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Material Code validations  (SAP field - MATNR)
                    //#region checking for empty
                    //if (CatalogItem.MATNR == String.Empty)
                    //{
                    //    //INV_ITEM_ID is missing for part %s
                    //    //Required Element
                    //    sErrMsg = ResolveErrMsg("E00001", CatalogItem.BatchID.ToString(), alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00001", sErrMsg,currentStage_Id));
                    //    CatalogItem.ValidStatus = "INVALID";
                    //}
                    //#endregion
                    if (CatalogItem.MATNR.Length > 18)
                    {
                        try
                        {
                            CatalogItem.LastUpdatedTimeStmp = DateTime.Parse((CatalogItem.LastUpdatedTimeStmp).ToString());
                        }
                        catch
                        {
                            CatalogItem.LastUpdatedTimeStmp = DateTime.MinValue;
                        }

                        //Required field INV_ITEM_ID data format does not match schema. Data Value received %s                            
                        sErrMsg = ResolveErrMsg("E00002", "for" + CatalogItem.MATNR + "is:" + CatalogItem.MATNR, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00002", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00002 " + GetErrorText(alCatalogErrorTexts, "E00002") + "<br />";
                        CatalogItem.FailedProcessing = true;

                        CatalogItem.MATNR = CatalogItem.MATNR.Substring(0, 18);
                    }
                    #endregion


                    #region Item Status Validations  (SAP field - LVORM)
                    if (CatalogItem.LVORM.Length > 1)
                    {
                        //Required field Item Current Status data format does not match schema.Data Value received for part %s
                        //Comparison with the Schema

                        sErrMsg = ResolveErrMsg("E00007", CatalogItem.MATNR + "is:" + CatalogItem.LVORM, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00007", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00007 " + GetErrorText(alCatalogErrorTexts, "E00007") + "<br />";
                        CatalogItem.FailedProcessing = true;

                        CatalogItem.LVORM = CatalogItem.LVORM.Substring(0, 1);
                    }
                    #endregion

                    #region CLMC Missing  (SAP field - MFRNR)
                    if (CatalogItem.MFRNR == String.Empty)
                    {
                        sErrMsg = ResolveErrMsg("E00005", CatalogItem.MATNR + "is:" + CatalogItem.MFRNR, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00005", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00005 " + GetErrorText(alCatalogErrorTexts, "E00005") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Description Missing  (SAP field - ZMAKTX)
                    if (CatalogItem.ZMAKTX == String.Empty)
                    {
                        sErrMsg = ResolveErrMsg("E00006", CatalogItem.MATNR + "is:" + CatalogItem.ZMAKTX, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00006", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00006 " + GetErrorText(alCatalogErrorTexts, "E00006") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region CLMC Validation  (SAP field - MFRNR)
                    if (CatalogItem.MFRNR != String.Empty)
                    {
                        // TODO MFORD - when we figure out what are good ones and what aren't E00008
                    }
                    #endregion

                    #region Length Unit Validation  (SAP field - MEINS)
                    if (CatalogItem.MEINS != String.Empty)
                    {
                        // TODO MFORD - when we figure out what are good ones and what aren't E00009
                    }
                    #endregion

                    #region MFG Part Number Bad Chars  (SAP field - MFRPN)
                    if (CatalogItem.MFRPN != String.Empty)
                    {
                        if (CatalogItem.MFRPN == "$")
                        {
                            sErrMsg = ResolveErrMsg("E00010", CatalogItem.MATNR + "is:" + CatalogItem.MFRPN, alCatalogErrors);
                            alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00010", sErrMsg, currentStage_Id));
                            CatalogItem.ValidStatus = "INVALID";
                            errorStrings += "E00010 " + GetErrorText(alCatalogErrorTexts, "E00010") + "<br />";
                            CatalogItem.FailedProcessing = true;
                        }
                        // TODO MFORD - when we figure out what are good ones and what aren't E00009
                    }
                    #endregion

                    #region ICC_CODE Missing  (SAP field - ICC_CODE)
                    if (CatalogItem.ICC_CODE == String.Empty)
                    {
                        sErrMsg = ResolveErrMsg("E00013", CatalogItem.MATNR + "is:" + CatalogItem.ICC_CODE, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00013", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00013 " + GetErrorText(alCatalogErrorTexts, "E00013") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region UOM Missing  (SAP field - MEINS )
                    if (CatalogItem.MEINS == String.Empty)
                    {
                        sErrMsg = ResolveErrMsg("E00014", CatalogItem.MATNR + "is:" + CatalogItem.MEINS, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00014", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00014 " + GetErrorText(alCatalogErrorTexts, "E00014") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Delete Indicator Missing  (SAP field - LVORM )
                    if (CatalogItem.LVORM != String.Empty && CatalogItem.LVORM != "X")
                    {
                        sErrMsg = ResolveErrMsg("E00025", CatalogItem.MATNR + "is:" + CatalogItem.LVORM, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00025", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00025 " + GetErrorText(alCatalogErrorTexts, "E00025") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Material Description Validations  (SAP field - ZMAKTX)

                    if (CatalogItem.ZMAKTX != null)
                    {
                        if (CatalogItem.ZMAKTX.Length > 100)
                        {
                            //Data content on field MTL_DESC is not valid.Value received for part %s

                            sErrMsg = ResolveErrMsg("E00012", CatalogItem.MATNR + " is:" + CatalogItem.ZMAKTX, alCatalogErrors);
                            alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00012", sErrMsg, currentStage_Id));
                            CatalogItem.ValidStatus = "INVALID";
                            errorStrings += "E00012 " + GetErrorText(alCatalogErrorTexts, "E00012") + "<br />";
                            CatalogItem.FailedProcessing = true;

                            //if (CatalogItem.ZMAKTX.IndexOf("'") > 0)
                            //    CatalogItem.ZMAKTX = CatalogItem.ZMAKTX.Substring(0, 99).Replace("'", "''");
                            //else
                            CatalogItem.ZMAKTX = CatalogItem.ZMAKTX.Substring(0, 100);
                        }
                    }
                    #endregion

                    #region UOM Validations  (SAP field - MEINS)

                    if (CatalogItem.MEINS == String.Empty)
                    {
                        //E00006:UOM is missing for part %s
                        //Required Element

                        sErrMsg = ResolveErrMsg("E00014", CatalogItem.MATNR, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00014", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00014 " + GetErrorText(alCatalogErrorTexts, "E00014") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    if (CatalogItem.MEINS.Length > 3)
                    {
                        //E00007:Required field UOM data format does not match schema.Data Value received for part %s
                        //Comparison with Schema
                        sErrMsg = ResolveErrMsg("E00015", CatalogItem.MATNR + " is:" + CatalogItem.MEINS, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00015", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00015 " + GetErrorText(alCatalogErrorTexts, "E00015") + "<br />";
                        CatalogItem.FailedProcessing = true;

                        CatalogItem.MEINS = CatalogItem.MEINS.Substring(0, 3);
                    }
                    //if (CatalogItem.MEINS.Length > 2)
                    //{
                    //    //E00008:Data content on field UOM is not valid. Value received for part %s                           
                    //    sErrMsg = ResolveErrMsg("E00016", CatalogItem.MATNR + " is:" + CatalogItem.MEINS, alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00016", sErrMsg, currentStage_Id));
                    //    CatalogItem.ValidStatus = "INVALID";
                    //}
                    if (!alValidUOMs.Contains(CatalogItem.MEINS))
                    {
                        //E00009:PS does not map to Nectas attribute UOM.Value received for part %s
                        sErrMsg = ResolveErrMsg("E00017", CatalogItem.MATNR + " is:" + CatalogItem.MEINS, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00017", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        errorStrings += "E00017 " + GetErrorText(alCatalogErrorTexts, "E00017") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Hazardous Material  (SAP field - KZUMW)
                    if (CatalogItem.KZUMW.Length > 1)
                    {
                        //Optional field HAZ_CLASS_CD data format does not match schema. Data Value received for part %s
                        sErrMsg = ResolveErrMsg("E00018", CatalogItem.MATNR + " is:" + CatalogItem.KZUMW, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00018", sErrMsg, currentStage_Id));
                        CatalogItem.ValidStatus = "INVALID";
                        CatalogItem.KZUMW = CatalogItem.KZUMW.Substring(0, 1);
                        errorStrings += "E00018 " + GetErrorText(alCatalogErrorTexts, "E00018") + "<br />";
                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region APCL Code Validations
                    //CatalogItem.APCLCode = pidDetailInfoDTO.PIDItmAttribDetails.APCLCode == null ? String.Empty : pidDetailInfoDTO.PIDItmAttribDetails.APCLCode;

                    if (CatalogItem.APCL_CODE.Length > 2)
                        CatalogItem.APCL_CODE = CatalogItem.APCL_CODE.Substring(0, 2);
                    #endregion

                    #region ICC Code Validations (SAP field - ICC_CODE)  --Skip This Validation
                    // Can skip ICC validation
                    //if (CatalogItem.ICC_CODE == String.Empty)   //Lvorm (Item Status) -- Item Current Status
                    //{
                    //    sErrMsg = ResolveErrMsg("E00019", CatalogItem.MATNR, alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00019", sErrMsg, currentStage_Id));
                    //    CatalogItem.ValidStatus = "INVALID";
                    //}
                    ////Data content on field ICC_CD is not valid. Value received for part %s.
                    //if (CatalogItem.ICC_CODE.Length > 4)
                    //{
                    //    sErrMsg = ResolveErrMsg("E00020", CatalogItem.MATNR + "is:" + CatalogItem.ICC_CODE, alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00020", sErrMsg, currentStage_Id));

                    //    sErrMsg = ResolveErrMsg("E00021", CatalogItem.MATNR + "is:" + CatalogItem.ICC_CODE, alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00021", sErrMsg, currentStage_Id));

                    //    CatalogItem.ValidStatus = "INVALID";

                    //    CatalogItem.ICC_CODE = CatalogItem.ICC_CODE.Substring(0, 4);
                    //}

                    //PS does not map to Nectas attribute ICC_CD. Value received for part %s
                    //Comparison against a defined valid set of values
                    //if (!alICCForOrderableItems.Contains(CatalogItem.ICC_CODE))
                    //{
                    //    sErrMsg = ResolveErrMsg("E00022", CatalogItem.MATNR + " is:" + CatalogItem.ICC_CODE, alCatalogErrors);
                    //    alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00022", sErrMsg, currentStage_Id));
                    //    CatalogItem.ValidStatus = "INVALID";
                    //}
                    #endregion

                    #region Manufacturer Info
                    //Manufacturer part number
                    if (CatalogItem.MFRPN.Length > 40)
                        CatalogItem.MFRPN = CatalogItem.MFRPN.Substring(0, 40);

                    //Manufacturer name   (Should check this column)
                    if (CatalogItem.NAME1.Length > 35)
                        CatalogItem.NAME1 = CatalogItem.NAME1.Substring(0, 35);

                    //Manufacture 
                    if (CatalogItem.MFRNR.Length > 10)
                        CatalogItem.MFRNR = CatalogItem.MFRNR.Substring(0, 10);

                    #endregion

                    #region Average Price Validations (SAP field - VERPR)

                    try
                    {
                        if (CatalogItem.VERPR != null && CatalogItem.VERPR != "")
                            CatalogItem.VERPR = Decimal.Parse(CatalogItem.VERPR).ToString();
                    }
                    catch (Exception ex)
                    {
                        sErrMsg = ResolveErrMsg("E00023", CatalogItem.MATNR + " is:" + CatalogItem.VERPR, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00023", sErrMsg, currentStage_Id));
                        errorStrings += "E00023 " + GetErrorText(alCatalogErrorTexts, "E00023") + "<br />";

                        ////Data content on field AVG_PRICE_AMT is not valid. Value received for part %s - E00011
                        sErrMsg = ResolveErrMsg("E00024", CatalogItem.MATNR + " is:" + CatalogItem.VERPR, alCatalogErrors);
                        alValidationErrors.Add(ConstructDTOOnError(CatalogItem, "E00024", sErrMsg, currentStage_Id));
                        errorStrings += "E00024 " + GetErrorText(alCatalogErrorTexts, "E00024") + "<br />";

                        //As it has non nummneric/invalid data for VERPR, so set it as "0.00" and log actual value coming form sap as part of error message
                        string invalidValueErrorMsg = "Catalog Item in xml having MATNR: '" + CatalogItem.MATNR + "' does not contain valid value for VERPR field, hence it has been set to 0. Actual value recieved from SAP:'" + CatalogItem.VERPR + "'.";
                        CatalogException InvalidValueError = new CatalogException(invalidValueErrorMsg);
                        //ExceptionManager.PublishException(InvalidValueError, (int)Constants.SentryIdentifier.DoValidation, invalidValueErrorMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);

                        CatalogItem.VERPR = "0";

                        CatalogItem.ValidStatus = "INVALID";

                        CatalogItem.FailedProcessing = true;
                    }
                    #endregion

                    #region Validations for ZDEPTH, ZWIDTH and ZHEIGHT
                    try
                    {
                        if(CatalogItem.ZDEPTH != null)
                        {
                            float.Parse(CatalogItem.ZDEPTH);
                        }
                    }
                    catch
                    {
                        CatalogItem.ZDEPTH = String.Empty;
                    }
                    try
                    {
                        if (CatalogItem.ZHEIGHT != null)
                        {
                            float.Parse(CatalogItem.ZHEIGHT);
                        }
                    }
                    catch
                    {
                        CatalogItem.ZHEIGHT = String.Empty;
                    }
                    try
                    {
                        if (CatalogItem.ZWIDTH != null)
                        {
                            float.Parse(CatalogItem.ZWIDTH);
                        }
                    }
                    catch
                    {
                        CatalogItem.ZWIDTH = String.Empty;
                    }
                    #endregion

                    CatalogItem.LastUpdatedUserID = "CATALOG_SVC";

                    //Validation Status into ValidStatus
                    if (CatalogItem.ValidStatus != "INVALID")
                        CatalogItem.ValidStatus = "VALID";
                }

                if (errorStrings != String.Empty)
                {
                    failedEmailText += "Part Number: " + CatalogItem.MFRPN + "<br />";
                    failedEmailText += "CLMC: " + CatalogItem.MFRNR + "<br />";
                    failedEmailText += "APCL: " + CatalogItem.APCL_CODE + "<br />";
                    failedEmailText += "Descr: " + CatalogItem.PO_TEXT + "<br />";
                    failedEmailText += errorStrings + "<br />";
                }

            }
            validationErrors = failedEmailText;
            #endregion
        }
        #endregion

        #region Insert Errors into Error Table and Build Mail For Validation Failure

        public void InsertIntoStagingErrorTable(List<CatalogItem> alCatalogStagingDTOList)
        {
            //catalogMgr.InsertIntoBatchProcTbl("VS", lBatchID);
            long lPrevStageID = -1;
            long lCurrStageID = -1;

            if (alValidationErrors.Count > 0)
            {
                foreach (CatalogItem currentCatalogInfo in alCatalogStagingDTOList)
                {
                    // if this item is not an OSPFM record, then ignore this item
                    //if (!(ParseCatalogXML.GetRecordType(currentCatalogInfo).Equals("OSPFM")))
                    //    continue;

                    if (IsValidDTO(currentCatalogInfo))
                    {
                        foreach (CatalogErrorDTO catErrDTO in alValidationErrors)
                        {
                            catErrDTO.BatchID = currentCatalogInfo.BatchID;
                            //check for MaterialCode in DTO as per requirement
                            if (catErrDTO.MaterialCode.Length > 18)
                                catErrDTO.MaterialCode = catErrDTO.MaterialCode.Substring(0, 18);

                            //if (catErrDTO.MaterialCode == currentCatalogInfo.MATNR && catErrDTO.LastUpdatedTimeStmp == currentCatalogInfo.LastUpdatedTimeStmp)
                            //    catErrDTO.StageID = currentCatalogInfo.StageID;
                        }
                    }
                }
                alValidationErrors.Sort();

                //Insert into Error Table
                catalogMgr.InsertIntoCatalogErrorTable(alValidationErrors);

                //Log start info            
                ApplicationFieldConfiguration.LogInfoMsg("Inserted data into CatalogStagingError table...");

                foreach (CatalogItem currentCatalogInfo in alCatalogStagingDTOList)
                {
                    // if this item is not an OSPFM record, then ignore this item
                    if (!(ParseCatalogXML.GetRecordType(currentCatalogInfo).Equals("OSPFM")))
                        continue;

                    if (IsValidDTO(currentCatalogInfo))
                    {
                        lCurrStageID = currentCatalogInfo.StageID;
                        if (lPrevStageID != lCurrStageID)
                        {
                            if (currentCatalogInfo.ValidStatus == "INVALID")
                            {
                                bHasValidationErrors = true;
                                alFailureMails.Add(SendMail.BuildFailureMail(currentCatalogInfo.MATNR, currentCatalogInfo.LVORM, currentCatalogInfo.ZMAKTX, currentCatalogInfo.StageID, alValidationErrors));
                            }
                            lPrevStageID = currentCatalogInfo.StageID;
                        }
                    }
                }

                if (bHasValidationErrors)
                    catalogMgr.InsertIntoBatchProcTbl("VF", lBatchID); // VF - DATA VALIDATION FAIL
                else
                    catalogMgr.InsertIntoBatchProcTbl("VP", lBatchID); // VP - DATA VALIDATION PASS
            }
        }
        #endregion



        #region Error Handling

        public string ResolveErrMsg(string sErrCd, string sContent, ArrayList alAllErrorCodes)
        {
            string sErrorText = "";
            foreach (CatalogErrorDTO oError in alAllErrorCodes)
            {
                if (oError.ErrorCode == sErrCd)
                {
                    sErrorText = oError.ErrorMsg;
                    break;
                }
            }
            string sResolvedErrorMessage = "";
            if (sContent == null || sContent == "" || sContent == string.Empty)
            {
                sResolvedErrorMessage = sErrCd + ":" + sErrorText.ToString();
            }
            else if (sErrCd == "E00001")
            {
                sResolvedErrorMessage = sErrCd + ":" + sErrorText.ToString().Replace("part %s", "batch - " + sContent);
            }
            else
            {
                sResolvedErrorMessage = sErrCd + ":" + sErrorText.ToString().Replace("%s", " - " + sContent);
            }

            return sResolvedErrorMessage;
        }

        private string GetErrorText (ArrayList errorTexts, string errorCode)
        {
            string errorText = "";
            foreach (CatalogErrorDTO dto in errorTexts)
            {
                if (errorCode == dto.ErrorCode)
                {
                    return dto.ErrorMsg;
                }
            }
            return errorText;
        }

        public CatalogErrorDTO ConstructDTOOnError(CatalogItem catStgDTO, string sErrorCode, string sErrMsg, long currentStage_Id)
        {
            CatalogErrorDTO catErrDTO = new CatalogErrorDTO();

            catErrDTO.MaterialCode = catStgDTO.MATNR;
            catErrDTO.ErrorCode = sErrorCode;
            catErrDTO.ErrorMsg = sErrMsg;
            catErrDTO.LastUpdatedTimeStmp = catStgDTO.LastUpdatedTimeStmp;

            if (catStgDTO.BatchID != -1)
                catErrDTO.BatchID = catStgDTO.BatchID;

            if (catStgDTO.StageID != -1)
                catErrDTO.StageID = catStgDTO.StageID;

            catErrDTO.StageID = currentStage_Id;

            return catErrDTO;

        }

        #endregion
    }
}
