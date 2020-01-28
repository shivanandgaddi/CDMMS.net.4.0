using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.Data;
using CenturyLink.Network.Engineering.Material.Management.Business.DTO;
using CenturyLink.Network.Engineering.Material.Management.Business.Exceptions;
using CenturyLink.Network.Engineering.Material.Management.Business.Utility;
using CenturyLink.Network.Engineering.TypeLibrary;
using CenturyLink.Network.Engineering.Common.Logging;

namespace CenturyLink.Network.Engineering.Material.Management.Manager
{
    internal class CatalogProcessingManager
    {
        private IAccessor dbManager;

        public CatalogProcessingManager()
        {
            wasCurrentDBUpdateSuccessfull = true;

            dbManager = DataAccessorFactory.GetDataAccessor(DataAccessorFactory.DATABASE.Oracle, System.Configuration.ConfigurationManager.AppSettings["dbConnectionString"]);
        }

        //Prop to convey status of db current db operation
        public bool wasCurrentDBUpdateSuccessfull
        {
            get; set;
        }

        /// <summary>
        /// Insert Catalog Info from Bus into MATERIAL_CATALOG_STAGING table
        /// </summary>
        /// <param name="alCatalogStgingInserts">List of records to be inserted</param>		
        public void InsertIntoCatalogStagingTbl(List<CatalogItem> dtoList, long lBatchID, bool isFallout) //*, OSPFMValidations Validations)
        {
            #region Declarations

            ParseCatalogXML parseItem = new ParseCatalogXML();

            // has list of all db field names for query
            ArrayList paramNames = new ArrayList();
            // has collection of values for all db fields in paramNames, for all the dtos and will be used in final query
            ArrayList paramValues = new ArrayList();

            //Contains param value for one db field at a time
            string[] tempParamValues;
            int count = 0;

            string sErrMsg = String.Empty;

            #endregion

            //TODO decide on whether or not, we need to have any specific flow/logic to ingore any kind of invalid catalog items 
            // separately for OSPFM and COEFM ones.
            List<CatalogItem> dtoValidList = dtoList; //* dtoList.Where(s => (s.ValidStatus.ToUpper() != "INVALID")).ToList();
            int ValidDTOCount = dtoValidList.Count;

            try
            {
                // has xml field to db column mapping list
                NameValueCollection fieldXMLKeyToDBNamePair = ApplicationFieldConfiguration.GetFieldConfiguration();
                EventLogger.LogInfo("fieldXMLKeyToDBNamePair count is " + fieldXMLKeyToDBNamePair.Count.ToString());

                Type CatalogItemType = typeof(CatalogItem);
                // All properties in dto
                PropertyInfo[] CatalogItemAllProperties = CatalogItemType.GetProperties();

                // loop for each xml field in mapping
                foreach (string currentXMLFieldKey in fieldXMLKeyToDBNamePair.AllKeys)
                {
                    //Add field param name. here paramname is equal to db fieldname in xml config file
                    // which inture is equal to a dto property name, which required update into db
                    paramNames.Add(fieldXMLKeyToDBNamePair.Get(currentXMLFieldKey).ToUpper());

                    #region Add field value for above added param name

                    // Note: Here value is taken from DTO prop matching to the above related XML config field key/name, not db fieldname
                    tempParamValues = new string[ValidDTOCount];

                    // take property from dto which has same name as current xml field name in mapping
                    PropertyInfo currentProperty = (PropertyInfo)(CatalogItemAllProperties.Where(x => x.Name.ToUpper() == currentXMLFieldKey.ToUpper()).First());

                    // get dto value for this dto field, and add to current paramvalue collection
                    foreach (CatalogItem catalogStagingDTO in dtoValidList)
                    {
                        tempParamValues[count++] = parseItem.GetTypeCastedValue(currentProperty, currentProperty.GetValue(catalogStagingDTO, null));
                    }

                    // add this specific field's value collection into paramValues collection
                    paramValues.Add(tempParamValues);
                    count = 0;
                    tempParamValues = null;

                    #endregion
                }

                #region Add field param name and values for primary keys and default columns

                paramNames.Add("BATCH_ID");
                paramNames.Add("STAGE_ID");
                paramNames.Add("LAST_UPDTD_USERID");
                paramNames.Add("LAST_UPDTD_TMSTMP");
                
                if(!isFallout)
                {
                    paramNames.Add("VALID_STATUS");
                    paramNames.Add("NEEDS_TO_BE_REVIEWED");
                }
                string[] arrBatchId = new string[ValidDTOCount];
                string[] arrStageId = new string[ValidDTOCount];
                string[] arrUpdatedUserId = new string[ValidDTOCount];
                string[] arrTimeStampId = new string[ValidDTOCount];
                string[] arrNeedsToBeReviewed = new string[ValidDTOCount];

                long currentStage_Id = 1;
                for (int i = 0; i < ValidDTOCount; i++)
                {
                    arrBatchId[i] = "'" + lBatchID.ToString() + "'";
                    arrStageId[i] = "'" + currentStage_Id.ToString() + "'";
                    dtoValidList[i].StageID = currentStage_Id;
                    dtoValidList[i].BatchID = lBatchID;
                    currentStage_Id++;

                    if (!isFallout)
                    {
                        arrNeedsToBeReviewed[i] = "'Y'";
                    }
                    arrUpdatedUserId[i] = "'CATALOG_SVC'";
                    arrTimeStampId[i] = "sysdate";
                }

                paramValues.Add(arrBatchId);
                paramValues.Add(arrStageId);
                paramValues.Add(arrUpdatedUserId);
                paramValues.Add(arrTimeStampId);

                if(!isFallout)
                {
                    tempParamValues = new string[ValidDTOCount];
                    foreach (CatalogItem catalogStagingDTO in dtoValidList)
                    {
                        if (catalogStagingDTO.ValidStatus == null)
                        {
                            tempParamValues[count++] = "NULL";
                        }
                        else
                        {
                            tempParamValues[count++] = "'" + catalogStagingDTO.ValidStatus + "'";
                        }
                    }
                    paramValues.Add(tempParamValues);
                    paramValues.Add(arrNeedsToBeReviewed);
                }

                #endregion

                string[] catalogBatchQuerries = CreateQuery(paramNames, paramValues, ValidDTOCount, isFallout);

                dbManager.BeginTransaction();

                for (int currentBatchIndex = 0; currentBatchIndex < catalogBatchQuerries.Length; currentBatchIndex++)
                {
                    StringBuilder currentCatalogBatchQuery = new StringBuilder("BEGIN ");
                    currentCatalogBatchQuery.Append(catalogBatchQuerries[currentBatchIndex]);
                    currentCatalogBatchQuery.Append(" END;");

                    //EventLogger.LogInfo("Insert query: " + currentCatalogBatchQuery.ToString());
                    dbManager.ExecuteNonQuery(CommandType.Text, currentCatalogBatchQuery.ToString());
                }
                dbManager.CommitTransaction();

                //Update batch status to 'IS' to convey "DATA INSERTED INTO STAGING"
                InsertIntoBatchProcTbl("IS", lBatchID);

                wasCurrentDBUpdateSuccessfull = true;

            }
            catch (Exception ex)
            {
                EventLogger.LogInfo("Exception: " + ex.Message);
                dbManager.RollbackTransaction();

                wasCurrentDBUpdateSuccessfull = false;

                sErrMsg = "Error in InsertIntoCatalogStagingTbl method  - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.InsertIntoCatalogStagingTbl, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.InsertIntoCatalogStagingTbl, false);
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        /// <summary>
        /// This Method creates DB insert/update query for given nfield name and field value pair, 
        /// holding catalog dto data
        /// </summary>
        /// <param name="paramNames">holds all db column names to keep in query</param>
        /// <param name="paramValues">holds collection of values for each db field in paramnames- has data from dto collection</param>
        /// <param name="dtoCount">number of dto to make query for</param>
        /// <returns></returns>
        private string[] CreateQuery(ArrayList paramNames, ArrayList paramValues, int dtoCount, bool isFallout)
        {
            //TODO remove this variable after OSPFM record processing work, not use din COEFM record's DB processing
            StringBuilder updateString = new StringBuilder();

            //Hold main insert query for db insert per catalog item
            StringBuilder insertString = new StringBuilder();
            // a placeholder in insertString, which will be replaced with columns names in final insert query
            string columnNamesPlaceHolder = "COLUMNNAMES ";
            // holds current db field name to add to query
            string currentDBFieldName = string.Empty;
            //Hold final insert query for db insert having query for all dto catalog item
            string strBulkInsert = "";

            if(!isFallout)
            {
                insertString.Append(@"Insert into MATERIAL_CATALOG_STAGING (" + columnNamesPlaceHolder);
            }
            else
            {
                insertString.Append(@"Insert into MATERIAL_CATALOG_FALLOUT (" + columnNamesPlaceHolder);
            }
            
            // will hold all column names string for "insert query"
            StringBuilder tempColumnNames = new StringBuilder();

            #region handles db fields not present in xml field to db columns mapping xml file

            insertString.Append(":BATCH_ID,");
            insertString.Append(":STAGE_ID,");
            tempColumnNames.Append("BATCH_ID,");
            tempColumnNames.Append("STAGE_ID,");

            #endregion

            // loop through each db field name as per xml to db field mapping
            foreach (string tempDBField in paramNames)
            {
                currentDBFieldName = tempDBField;

                // to handle - not to add primary keys into update query string, makes update/insert query
                // as per oracle query format for insert/update for all required db fields for staging table
                if ((currentDBFieldName.ToUpper() != "BATCH_ID") && (currentDBFieldName != "STAGE_ID"))
                {
                    tempColumnNames.Append(currentDBFieldName + ",");

                    updateString.Append("DEST." + currentDBFieldName + "= :" + currentDBFieldName + ",");
                    insertString.Append(":" + currentDBFieldName + ",");
                }
            }

            #region Region for specfic field level handling

            //This region is for specfic field level handling and for fields not defined in shema shall be added/manipulated here
            //commented as this is added in paramnames.
            //    tempColumnNames.Append("VALID_STATUS" + ",");
            //    insertString.Append("null,");

            #endregion

            //Remove last ',', if it is there at last position                
            tempColumnNames = (tempColumnNames.ToString().LastIndexOf(',') == (tempColumnNames.Length - 1)) ? tempColumnNames.Remove(tempColumnNames.Length - 1, 1) : tempColumnNames;
            tempColumnNames.Append(" ) values(");

            updateString = (updateString.ToString().LastIndexOf(',') == (updateString.Length - 1)) ? updateString.Remove(updateString.Length - 1, 1) : updateString;
            updateString.Append(" where Stage_Id = :STAGE_ID");

            insertString = (insertString.ToString().LastIndexOf(',') == (insertString.Length - 1)) ? insertString.Remove(insertString.Length - 1, 1) : insertString;
            insertString.Append(")");

            // Final insert string formatted to have ":DBfieldName" as placeholder and will getv replced with value to set in insert query
            strBulkInsert = insertString.ToString().Replace(columnNamesPlaceHolder, tempColumnNames.ToString());


            int TotalBatchCount = 0;//(dtoCount / ApplicationFieldConfiguration.oracleBatchRecordSize);
            int recordsInLastBatch = 0;//dtoCount % (ApplicationFieldConfiguration.oracleBatchRecordSize);
            if (recordsInLastBatch > 0)
            {
                TotalBatchCount++;
            }

            // It will contain insert query per dto item
            string[] catalogBatchQuerries = new string[dtoCount]; // TODO Investigate why this was set to TotalBatchCount which is always zero
            StringBuilder currentCatalogBatchQuery = new StringBuilder();
            StringBuilder tempQuery = null;
            int currentParamNameValuesItemIndex = 0;
            string tempDBKeyName = string.Empty;
            int recordsAddedToCurrentQuery = 0;
            int currentBatchQueryIndex = 0;

            // loop for dto count times and create insert query epr dto item and add to final query for catalog insert
            for (int iCurrentDTOQueryIndex = 0; iCurrentDTOQueryIndex < dtoCount; iCurrentDTOQueryIndex++)
            {
                // Insert query with fieldnames as placeholder and those all to be set with value for each field
                tempQuery = new StringBuilder(strBulkInsert.ToString());

                // will index current db field name as per xml field to db column mapping
                currentParamNameValuesItemIndex = 0;

                // in paramvalues collection, take each dto field values pertaining to this current indexed dto (as per iCurrentDTOQueryIndex)
                // and set it to related DB field values placeholder in tempQuery, which makes final insert query for this dto item
                foreach (string[] a in paramValues)
                {
                    tempDBKeyName = paramNames[currentParamNameValuesItemIndex].ToString();
                    tempQuery = tempQuery.Replace(":" + tempDBKeyName, a[iCurrentDTOQueryIndex].Trim());
                    ++currentParamNameValuesItemIndex;
                }

                currentCatalogBatchQuery.Append(tempQuery + ";\n");

                recordsAddedToCurrentQuery++;

                //if (recordsAddedToCurrentQuery == ApplicationFieldConfiguration.oracleBatchRecordSize || (iCurrentDTOQueryIndex == dtoCount - 1))
                {
                    recordsAddedToCurrentQuery = 0;
                    catalogBatchQuerries[currentBatchQueryIndex++] = currentCatalogBatchQuery.ToString();
                    currentCatalogBatchQuery = new StringBuilder();
                }
            }

            //*catalogQuery.Append(" END;");

            return catalogBatchQuerries;
        }

        /// <summary>
        /// Insert/Update Batch Processing Table
        /// </summary>
        /// <param name="sLoadStatus">PROC_STATUS_CD</param>
        /// <param name="nBatchID">Batch ID for which the Status needs to be updated</param>
        /// <returns>Batch ID after a new insert</returns>
        public long InsertIntoBatchProcTbl(string sLoadStatus, long nBatchID)
        {

            string strSQL = String.Empty;
            if (nBatchID != -1)
                strSQL = "UPDATE MATERIAL_CATALOG_BATCH SET PROC_STATUS_CD = '" + sLoadStatus + "',LAST_UPDTD_USERID = 'CATALOG_SVC'," +
                         "LAST_UPDTD_TMSTMP = SYSDATE WHERE BATCH_ID = " + nBatchID;
            else
                strSQL = "BEGIN " +
                         " INSERT INTO MATERIAL_CATALOG_BATCH VALUES(CDMMS_OWNER.BATCH_ID_SEQ.nextval,'" + sLoadStatus + "',SYSDATE,'CATALOG_SVC',SYSDATE); " +
                         " SELECT cdmms_owner.batch_id_seq.currval INTO :1 FROM DUAL; END;";

            IDbDataParameter[] param = dbManager.GetParameterArray(1);
            param[0] = dbManager.GetParameter("batchId", DbType.Int64, 0, ParameterDirection.Output);

            long retBatchID = -1;
            try
            {
                if (nBatchID == -1)
                {
                    dbManager.ExecuteNonQuery(CommandType.Text, strSQL, param);
                    retBatchID = long.Parse(param[0].Value.ToString());
                }
                else
                {
                    dbManager.ExecuteNonQuery(CommandType.Text, strSQL);
                    retBatchID = nBatchID;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while inserting/updating and fetching Batch ID - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.InsertIntoBatchProcTbl, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.InsertIntoBatchProcTbl, false);
            }

            return retBatchID;
        }

        /// <summary>
        /// Fetch all the Nectas Material Groups for comparison during Validation
        /// </summary>
        /// <returns>List of all Nectas Material Groups</returns>
        public StringDictionary FetchMaterialGrps(string appName, bool shallSelectAllGroups)
        {
            string sQuery = string.Empty;
            StringDictionary MtlGrpAppNamList = new StringDictionary();

            if (shallSelectAllGroups)
            {
                sQuery = @"select distinct MTL_GRP_ID,app_nm from MATERIAL_GROUP where upper(app_nm) in ('COE-FM','OSP-FM')";
            }
            else
            {
                sQuery = @"select distinct MTL_GRP_ID from MATERIAL_GROUP where upper(app_nm)='" + appName.ToUpper() + "'";
            }

            sQuery += " and sysdate between EFFECTIVE_START_DATE and EFFECTIVE_END_DATE";

            IDataReader reader = null;
            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {

                    string sMaterialGrp = Convert.ToString(reader[0]);
                    string sMaterialGrpAppName = "";

                    if (shallSelectAllGroups)
                    {
                        sMaterialGrpAppName = Convert.ToString(reader[1]);
                    }
                    else
                    {
                        sMaterialGrpAppName = "";
                    }

                    MtlGrpAppNamList.Add((sMaterialGrp + "~" + sMaterialGrpAppName), sMaterialGrpAppName);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while fetching the list of all Material Groups - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchMaterialGrps, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchMaterialGrps, false);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }
            return MtlGrpAppNamList;
        }

        /// <summary>
        /// Fetch all the Material Type Codes for comparison during Validation
        /// </summary>
        /// <returns>List of all Material Type Codes</returns>
        public ArrayList FetchMaterialTypeCodes()
        {
            string OSPFMMaterialTypeCd = "";//ApplicationFieldConfiguration.OSPFMMaterialTypeCd;

            string sQuery = @"select MTL_TYPE_CD from MATERIAL_TYPE_CD where APP_NAME='OSPFM' and MTL_TYPE_CD='" + OSPFMMaterialTypeCd + "'";
            ArrayList alAllMaterialTypeCodes = new ArrayList();

            IDataReader reader = null;
            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {
                    string sMaterialTypeCode = reader[0].ToString();

                    alAllMaterialTypeCodes.Add(sMaterialTypeCode);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while fetching the list of all Material Type Codes - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchMaterialTypeCodes, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchMaterialTypeCodes, false);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }
            return alAllMaterialTypeCodes;
        }


        /// <summary>
        /// Fetch the complete list of Error Codes from MAT_CAT_ERROR_CD
        /// for manipulation
        /// </summary>
        /// <returns>List of all Catalog Error Codes</returns>

        public ArrayList FetchCatalogErrorCodes()
        {
            string sQuery = @"select ERROR_CD,ERROR_MESSAGE from MAT_CAT_ERROR_CD";
            ArrayList alAllErrors = new ArrayList();

            IDataReader reader = null;
            CatalogErrorDTO catErrDTO = null;

            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {
                    catErrDTO = new CatalogErrorDTO();

                    catErrDTO.ErrorCode = reader[0].ToString();
                    catErrDTO.ErrorMsg = reader[1].ToString();

                    alAllErrors.Add(catErrDTO);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Fetching the list of all Catalog Error Codes - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchCatalogErrorCodes, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchCatalogErrorCodes, false);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }

            return alAllErrors;
        }

        public ArrayList FetchCatalogErrorTexts()
        {
            string sQuery = @"select ERROR_CD,ERROR_TEXT from MAT_CAT_ERROR_CD order by ERROR_CD";
            ArrayList alAllErrors = new ArrayList();

            IDataReader reader = null;
            CatalogErrorDTO catErrDTO = null;

            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {
                    catErrDTO = new CatalogErrorDTO();

                    catErrDTO.ErrorCode = reader[0].ToString();
                    catErrDTO.ErrorMsg = reader[1].ToString();

                    alAllErrors.Add(catErrDTO);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Fetching the list of all Catalog Error Codes - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchCatalogErrorCodes, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchCatalogErrorCodes, false);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }

            return alAllErrors;
        }

        /// <summary>
        /// Fetch all the ICC Codes 
        /// </summary>
        /// <returns>List of all ICC Codes</returns>

        /// <summary>
        /// Fetch all the ICC Codes to compare against during Catalog Validation
        /// </summary>
        /// <returns>List of all ICC Codes</returns>
        //Should check the table name
        public ArrayList FetchICCCodes()
        {
            string sQuery = @"SELECT ICC_CD FROM ICC_MAIN_SUB_ACCT";
            ArrayList alAllICCCodes = new ArrayList();

            IDataReader reader = null;
            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {
                    string sICCCd = reader[0].ToString();

                    alAllICCCodes.Add(sICCCd);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Fetching the list of all ICC Codes - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchICCCodes, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchICCCodes, false);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }
            return alAllICCCodes;
        }


        /// <summary>
        /// Archive the Data older than the period specified in the config from Staging to Staging History
        /// and from Fallout to Staging History
        /// </summary>
        /// <param name="sArchivePeriod">Archive Period</param>
        public void ArchiveCatalogData(string sArchivePeriod)
        {
            string sQuery = "begin ";
            sQuery += @" insert into MATERIAL_CATALOG_STAGING_HIST " +
                " (BATCH_ID, STAGE_ID, MTL_CD, HECI, MFG_PART_NO, MANUFACTURER, APCL, UOM, MTL_GRP, MTL_DESC, MFG_NM, ITEM_STATUS, " +
                " ICC_CD, AIC_CD, PROC_DT, ALT_UOM, DT_CREATED, LAST_CHG_DT, PO_TEXT, MVG_AVG_PRICE_AMT, HAZARD_IND, PLAND_DEL_TM, " +
                " MTL_TYPE, VALID_STATUS, LOAD_STATUS, LAST_UPDTD_USERID, LAST_UPDTD_TMSTMP, CONV_RATE1, CONV_RATE2, RECORD_TYPE, " +
                " ZHEIGHT, ZWIDTH, ZDEPTH, FALLOUT_IND, NEEDS_TO_BE_REVIEWED) " +
                " select A.BATCH_ID AS BATCH_ID, A.STAGE_ID AS STAGE_ID, A.MTL_CD AS MTL_CD, A.HECI AS HECI, A.MFG_PART_NO AS MFG_PART_NO,  " +
                " A.MANUFACTURER AS MANUFACTURER, A.APCL AS APCL, A.UOM AS UOM, A.MTL_GRP AS MTL_GRP, A.MTL_DESC AS MTL_DESC, " +
                " A.MFG_NM AS MFG_NM, A.ITEM_STATUS AS ITEM_STATUS, A.ICC_CD AS ICC_CD, A.AIC_CD AS AIC_CD, A.PROC_DT AS PROC_DT, " +
                " A.ALT_UOM AS ALT_UOM, A.DT_CREATED AS DT_CREATED, A.LAST_CHG_DT AS LAST_CHG_DT, A.PO_TEXT AS PO_TEXT, " +
                " A.MVG_AVG_PRICE_AMT AS MVG_AVG_PRICE_AMT, A.HAZARD_IND AS HAZARD_IND, A.PLAND_DEL_TM AS PLAND_DEL_TM, A.MTL_TYPE AS MTL_TYPE, " +
                " A.VALID_STATUS AS VALID_STATUS, A.LOAD_STATUS AS LOAD_STATUS, A.LAST_UPDTD_USERID AS LAST_UPDTD_USERID, " +
                " A.LAST_UPDTD_TMSTMP AS LAST_UPDTD_TMSTMP, A.CONV_RATE1 AS CONV_RATE1, A.CONV_RATE2 AS CONV_RATE2, A.RECORD_TYPE AS RECORD_TYPE, " +
                " A.ZHEIGHT AS ZHEIGHT, A.ZWIDTH AS ZWIDTH, A.ZDEPTH AS ZDEPTH, 'N' AS FALLOUT_IND, A.NEEDS_TO_BE_REVIEWED AS NEEDS_TO_BE_REVIEWED " +
                " from MATERIAL_CATALOG_STAGING A where A.LAST_UPDTD_TMSTMP < (sysdate - " + sArchivePeriod + "); ";

            //sQuery += @" insert into MATERIAL_CATALOG_STAGING_HIST " +
            //    " (BATCH_ID, STAGE_ID, MTL_CD, HECI, MFG_PART_NO, MANUFACTURER, APCL, UOM, MTL_GRP, MTL_DESC, MFG_NM, ITEM_STATUS, " +
            //    " ICC_CD, AIC_CD, PROC_DT, ALT_UOM, DT_CREATED, LAST_CHG_DT, PO_TEXT, MVG_AVG_PRICE_AMT, HAZARD_IND, PLAND_DEL_TM, " +
            //    " MTL_TYPE, LAST_UPDTD_USERID, LAST_UPDTD_TMSTMP, CONV_RATE1, CONV_RATE2, RECORD_TYPE, " +
            //    " ZHEIGHT, ZWIDTH, ZDEPTH, FALLOUT_IND) " +
            //    " select A.BATCH_ID AS BATCH_ID, A.STAGE_ID AS STAGE_ID, A.MTL_CD AS MTL_CD, A.HECI AS HECI, A.MFG_PART_NO AS MFG_PART_NO,  " +
            //    " A.MANUFACTURER AS MANUFACTURER, A.APCL AS APCL, A.UOM AS UOM, A.MTL_GRP AS MTL_GRP, A.MTL_DESC AS MTL_DESC, " +
            //    " A.MFG_NM AS MFG_NM, A.ITEM_STATUS AS ITEM_STATUS, A.ICC_CD AS ICC_CD, A.AIC_CD AS AIC_CD, A.PROC_DT AS PROC_DT, " +
            //    " A.ALT_UOM AS ALT_UOM, A.DT_CREATED AS DT_CREATED, A.LAST_CHG_DT AS LAST_CHG_DT, A.PO_TEXT AS PO_TEXT, " +
            //    " A.MVG_AVG_PRICE_AMT AS MVG_AVG_PRICE_AMT, A.HAZARD_IND AS HAZARD_IND, A.PLAND_DEL_TM AS PLAND_DEL_TM, A.MTL_TYPE AS MTL_TYPE, " +
            //    " A.LAST_UPDTD_USERID AS LAST_UPDTD_USERID, " +
            //    " A.LAST_UPDTD_TMSTMP AS LAST_UPDTD_TMSTMP, A.CONV_RATE1 AS CONV_RATE1, A.CONV_RATE2 AS CONV_RATE2, A.RECORD_TYPE AS RECORD_TYPE, " +
            //    " A.ZHEIGHT AS ZHEIGHT, A.ZWIDTH AS ZWIDTH, A.ZDEPTH AS ZDEPTH, 'Y' AS FALLOUT_IND " +
            //    " from MATERIAL_CATALOG_FALLOUT A where A.LAST_UPDTD_TMSTMP < (sysdate - " + sArchivePeriod + "); ";


            sQuery += " delete from MATERIAL_CATALOG_STAGING where LAST_UPDTD_TMSTMP < (sysdate - " + sArchivePeriod + "); ";
            sQuery += " end;";

            try
            {
                int i = dbManager.ExecuteNonQuery(CommandType.Text, sQuery);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Archiving Catalog Staging Data - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.ArchiveCatalogData, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.ArchiveCatalogData, false);
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        /// <summary>
        /// Data Load into Mtl_Item_SAP table
        /// </summary>
        /// <param name="dtoCatalogPart">A Part's data</param>
        /// <param name="nNectasChg">Change in Nectas Attirbutes or not</param>
        /// <returns>Package Output Status DTO</returns>
        public PackageOutputDTO InsertIntoMtlPSTable(CatalogItem dtoCatalogPart, ref int nNectasChg)
        {
            string sMsgCode = "";
            int nOutStatus = -1;
            string sMsgText = "";
            nNectasChg = -1;

            PackageOutputDTO pckOut = new PackageOutputDTO();

            object[] oParams = new object[6];

            oParams[0] = dtoCatalogPart.BatchID;
            oParams[1] = dtoCatalogPart.StageID;
            oParams[2] = nNectasChg;
            oParams[3] = sMsgCode;
            oParams[4] = sMsgText;
            oParams[5] = nOutStatus;

            try
            {
                object retCreateVal = dbManager.ExecuteNonQuerySP("OSPFM_ME_CATALOG_SAVEDATA_PKG.OSPFMCatalogDataLoad", oParams);

                pckOut.MsgCode = oParams[3].ToString();
                pckOut.MsgText = oParams[4].ToString();
                pckOut.ReturnValue = Int32.Parse(oParams[5].ToString());

                nNectasChg = Int32.Parse(oParams[2].ToString());
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while inserting into Material Item PS Table - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.InsertIntoMtlPSTable, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.InsertIntoMtlPSTable, false);
            }
            finally
            {
                dbManager.Dispose();
            }
            return pckOut;
        }

        /// <summary>
        /// Insert into Error Table in case of Validation Errors
        /// </summary>
        /// <param name="alValidationErrors">List of Validation Errors to be inserted in Staging Error Table</param>
        public void InsertIntoCatalogErrorTable(ArrayList alValidationErrors)
        {
            ArrayList paramNames = new ArrayList();
            ArrayList paramValues = new ArrayList();

            paramNames.Add("batchid");
            paramNames.Add("stageid");
            paramNames.Add("errorcd");

            string[] arBatchID = new string[alValidationErrors.Count];
            string[] arStageID = new string[alValidationErrors.Count];
            string[] arErrorCd = new string[alValidationErrors.Count];

            try
            {
                string strSQL = "INSERT INTO MATL_CATALOG_STAGING_ERRORS (BATCH_ID, STAGE_ID, APP_NM,ERROR_CD, LAST_UPDTD_USERID, LAST_UPDTD_TMSTMP) VALUES(:batchid,:stageid,'OSPFM',:errorcd,'CATALOG_SVC',SYSDATE)";

                int i = 0;
                foreach (CatalogErrorDTO catInsertErrDTO in alValidationErrors)
                {
                    arBatchID[i] = catInsertErrDTO.BatchID.ToString();
                    arStageID[i] = catInsertErrDTO.StageID.ToString();
                    arErrorCd[i] = catInsertErrDTO.ErrorCode;
                    i++;
                }

                paramValues.Add(arBatchID);
                paramValues.Add(arStageID);
                paramValues.Add(arErrorCd);

                if (alValidationErrors.Count > 0)
                    dbManager.ExecuteNonQuery(CommandType.Text, strSQL, paramNames, paramValues);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Inserting Errors for Catalog - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.InsertIntoCatalogErrorTable, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.InsertIntoCatalogErrorTable, false);
            }
            finally
            {
                dbManager.Dispose();
            }
        }

        /// <summary>
        /// Fetches the Initial DateTime of the Batch
        /// </summary>
        /// <param name="lBatchID">Batch ID to fetch the Timestamp of the Bus message</param>
        /// <returns>Timestamp of the Batch</returns>
        public DateTime FetchBatchTimeStmp(long lBatchID)
        {
            string sQuery = @"SELECT RECEIVE_DT FROM MATERIAL_CATALOG_BATCH WHERE BATCH_ID = " + lBatchID;

            IDataReader rdBatch = null;
            DateTime dtBatchRecvd = DateTime.MinValue;
            try
            {
                rdBatch = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (rdBatch.Read())
                {
                    dtBatchRecvd = DateTime.Parse(rdBatch[0].ToString());
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Fetching Timestamp of Bus for Catalog - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchBatchTimeStmp, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchBatchTimeStmp, false);

            }
            finally
            {
                if (rdBatch != null)
                    rdBatch.Close();

                dbManager.Dispose();
            }
            return dtBatchRecvd;
        }

        /// <summary>
        /// Fetch all the Valid Nectas UOMs to compare against during Catalog Validation
        /// </summary>
        /// <returns>List of all Nectas Valid UOMs</returns>
        public ArrayList FetchNectasUOMs()
        {
            string sQuery = @"SELECT MTL_VALUE FROM MTL_ITEM_REF WHERE MTL_VALUE_USAGE = 'UNIT_OF_MRMT'";
            ArrayList alNectasUOMs = new ArrayList();

            IDataReader reader = null;
            try
            {
                reader = dbManager.ExecuteDataReader(CommandType.Text, sQuery);

                while (reader.Read())
                {
                    string sUOM = reader[0].ToString();

                    alNectasUOMs.Add(sUOM);
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error while Fetching the list of Nectas Valid UOMs - " + ex.Message;

                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.FetchNectasUOMs, sErrMsg, ExceptionConstants.Severity.Major);
                throw new CatalogException(sErrMsg, (int)Constants.SentryIdentifier.FetchNectasUOMs, false);

            }
            finally
            {
                if (reader != null)
                    reader.Close();

                dbManager.Dispose();
            }
            return alNectasUOMs;
        }
    }
}
