using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CenturyLink.Network.Engineering.Material.Management.Business.Exceptions;
using CenturyLink.Network.Engineering.Material.Management.Manager;
using CenturyLink.Network.Engineering.TypeLibrary;
using CenturyLink.Network.Engineering.Common.Logging;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Utility;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Utility
{
    internal class ParseCatalogXML
    {
        bool bValidXML = true;
        //Holds all error messages in one single xml load and validation process
        StringBuilder schemaErrorMessages = new StringBuilder();
        public static StringDictionary MtlGrpAppNamList = null;

        //Method to validate xml format
        public bool ValidateXml(ref string xmlMessage)
        {
            try
            {
                xmlMessage = xmlMessage.Replace("encoding=\"utf-16\"", "");
                //xmlMessage = xmlMessage.Replace("nm:", String.Empty);
                //xmlMessage = xmlMessage.Replace("n0:", String.Empty);
                XmlDocument asset = new XmlDocument();

                //Get schema file path, configured in app.config file
                string schemaPath = ConfigurationManager.Value(APPLICATION_NAME.CATALOG_SVC, "catalogSchemaPath");
                XmlTextReader schemaReader = new XmlTextReader(schemaPath);
                XmlSchema schema = XmlSchema.Read(schemaReader, ValidationError);

                asset.Schemas.Add(schema);

                //TODO make sure message field array will always have one item in array for a batch process
                MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(Convert.ToString(xmlMessage)));

                asset.Load(mStream);

                //Below method will validate xml and will call ValidationError as many times as many schema errors in xml
                asset.Validate(ValidationError);

                if (!bValidXML)
                {
                    string sErrMsg = "Error in xml schema - " + schemaErrorMessages;
                    CatalogException schemaException = new CatalogException(schemaErrorMessages.ToString());

                    //ExceptionManager.PublishException(schemaException, (int)Constants.SentryIdentifier.ValidateXml, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
                }

                return bValidXML;
            }
            catch (Exception ex)
            {
                EventLogger.LogInfo("Error in ValidateXML" + ex.Message);
                // XML validation failed with error(s),so make bValidXML flag false
                bValidXML = false;

                string sErrMsg = "Error in method ValidateXml - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.ValidateXml, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);

                return bValidXML;
            }
        }

        /// <summary>
        /// ValidationEventHandler Call-back Method on Schema Validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arguments">will contain error for a specific schema/format failure</param>
        private void ValidationError(object sender, ValidationEventArgs arguments)
        {
            bValidXML = false;

            schemaErrorMessages.Append(arguments.Message + Environment.NewLine);
        }

        // Mthod to create a collection of catalog dto for xml message to process, 
        // if xml reading has errors it will give null
        public List<CatalogItem> PopulateDTOFromMessage(string message, ref List<CatalogItem> falloutCatalogCollection)
        {
            string fieldName = string.Empty;
            List<CatalogItem> CatalogItemCollection = null;

            try
            {
                // Contains dto field to db column mapping
                NameValueCollection fieldConfigurations = ApplicationFieldConfiguration.GetFieldConfiguration();

                string[] AllStagingFieldName = fieldConfigurations.AllKeys;

                Type CatalogItemType = typeof(CatalogItem);
                //List of dto properties
                PropertyInfo[] CatalogItemAllProperties = CatalogItemType.GetProperties();
                EventLogger.LogInfo("CatalogItemAllProperties count is " + CatalogItemAllProperties.Count().ToString());

                //Recieved xml data
                string catalogXML = Convert.ToString(message);
                XDocument xCatalogItemsDoc = XDocument.Parse(catalogXML);
                EventLogger.LogInfo("message is " + message);
                EventLogger.LogInfo("xCatalogItemsDoc is " + xCatalogItemsDoc.ToString());

                //Contains as many item elements as many catalog items in recieved xml
                IEnumerable<XElement> AllItemInfoElements = xCatalogItemsDoc.Descendants("ROOT").Descendants("DOC_HEADER");
                int itemCount = AllItemInfoElements.Count();

                CatalogItemCollection = new List<CatalogItem>(itemCount);

                CatalogItem currentItemInfo = null;
                IEnumerable<XElement> CurrentItemDetails = null;
                int currentItemNodePosition = 0;
                //Loop through all item nodes and create a dto and add to dto collection
                EventLogger.LogInfo("Number of AllItemInfoElements is " + itemCount.ToString());
                foreach (XElement element in AllItemInfoElements)
                {
                    bool isFallout = false;
                    currentItemNodePosition++;

                    // has all fields for this current catalog item
                    CurrentItemDetails = element.Descendants();

                    string mtlGrpAppNam = "";
                    //Below method will give true if this xml node has common mtl-grp for both coefm and opsfm in db
                    // if it gives true, then 2 records will be saved into staging table for this single xml,
                    // one as coefm and another one as ospfm record, where record_type column will be set to relavane app name in db.
                    bool isHavingCommonMtlGrp = IsHavingCommonMtlGrp(CurrentItemDetails, out mtlGrpAppNam);
                    int repeatCount = 1;

                    // if this item's material type code is not mathcing to OSPFM/COEFM needs, then ignore this item
                    // update - instead of ignoring this item, add it to the fallout collection to be inserted to the
                    // material_catalog_fallout table later on
                    bool hasDataInAllCOEFMReqField = true;
                    if (!(IsValidItemForLoad(CurrentItemDetails, currentItemNodePosition, ref hasDataInAllCOEFMReqField)))
                    {
                        isFallout = true;
                    }

                    //When this xml item node has common mtl grpId, and it has data in all the req fields,
                    // Only then this item node will be inserted as opsfm and coefm record, otherwise it will be taken just as OSPFM record
                    if (isHavingCommonMtlGrp && string.IsNullOrEmpty(mtlGrpAppNam) && hasDataInAllCOEFMReqField)
                    {
                        repeatCount++;
                    }
                    else if (!isHavingCommonMtlGrp && Convert.ToString(mtlGrpAppNam).ToUpper() == "COEFM" && !hasDataInAllCOEFMReqField)
                    {
                        continue;
                    }

                    for (int i = 0; i < repeatCount; i++)
                    {
                        currentItemInfo = new CatalogItem();

                        // Loop through all dto fields and set it value from xml data
                        // but ONLY if current dto field name is same as xml field node name, 
                        // as those are ONLY mapped for db update, will ignore other dto fields
                        foreach (PropertyInfo currentProperty in CatalogItemAllProperties)
                        {
                            // set "current xml field name to look for" same as "dto prop name"
                            string currentXMLFieldName = currentProperty.Name;

                            // check if this dto is found into xml node, will set CurrentFieldValue to null, if no such nodes 
                            var CurrentFieldValue = CurrentItemDetails.Where(x => x.Name.LocalName.ToString() == currentXMLFieldName).FirstOrDefault();

                            if (CurrentFieldValue != null)
                            {
                                // set current dto prop to CurrentFieldValue node's value, 
                                // after parsing value as per dto prop's datatype
                                TypeCastDTOValues(currentItemInfo, currentProperty, CurrentFieldValue.Value);
                            }
                        }

                        ////*SetRecordType(currentItemInfo);

                        if (!isFallout)
                        {
                            if (isHavingCommonMtlGrp && string.IsNullOrEmpty(mtlGrpAppNam))
                            {
                                if (i == 0)
                                {
                                    currentItemInfo.RecordType = "OSPFM";
                                }
                                else if (i == 1)
                                {
                                    currentItemInfo.RecordType = "COEFM";
                                }
                                CatalogItemCollection.Add(currentItemInfo);
                            }
                            else
                            {
                                currentItemInfo.RecordType = mtlGrpAppNam;
                                CatalogItemCollection.Add(currentItemInfo);
                            }
                        }
                        else
                        {
                            falloutCatalogCollection.Add(currentItemInfo);
                        }
                        currentItemInfo = null;

                    }
                    CurrentItemDetails = null;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error in method PopulateDTOFromMessage - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.PopulateDTOFromMessage, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }

            return CatalogItemCollection;
        }

        // This method return true if this xml node has mtl_grp_id which is defined for both ospfm, and coefm app_name, also will give mtlGrpAppName as "" 
        // else will return false, and will give mtlGrpAppName with correct app_name for this mtl_grp
        private bool IsHavingCommonMtlGrp(IEnumerable<XElement> CurrentItemDetails, out string mtlGrpAppName)
        {
            bool isHavingCommonMtlGrp = false;

            string materialGroupXMLFieldName = "MATKL";
            // it will set obtained/searched currentNode to null, if no such node found in this current item node                    
            var materialGroupNode = CurrentItemDetails.Where(x => x.Name.LocalName.ToString() == materialGroupXMLFieldName).FirstOrDefault();
            string materialGroupCode = "";

            if (materialGroupNode != null && !string.IsNullOrEmpty(materialGroupNode.Value))
            {
                materialGroupCode = Convert.ToString(materialGroupNode.Value).ToUpper();
            }

            CatalogProcessingManager catalogMgr = new CatalogProcessingManager();

            if (MtlGrpAppNamList == null)
            {
                MtlGrpAppNamList = catalogMgr.FetchMaterialGrps(null, true);
            }

            //if grp code is not listed in table then below line will throw exception and we can ignore that excpetion as grp is not provided
            string COEFMGrpAppName = "";
            try
            {
                COEFMGrpAppName = MtlGrpAppNamList[(materialGroupCode + "~COE-FM")];
                if (string.IsNullOrEmpty(COEFMGrpAppName))
                {
                    COEFMGrpAppName = "";
                }
            }
            catch
            {
                COEFMGrpAppName = "";
            }

            string OSPFMGrpAppName = "";
            try
            {
                OSPFMGrpAppName = MtlGrpAppNamList[(materialGroupCode + "~OSP-FM")];
                if (string.IsNullOrEmpty(OSPFMGrpAppName))
                {
                    OSPFMGrpAppName = "";
                }
            }
            catch
            {
                OSPFMGrpAppName = "";
            }

            if (!string.IsNullOrEmpty(COEFMGrpAppName) && !string.IsNullOrEmpty(OSPFMGrpAppName) && COEFMGrpAppName.Trim().ToUpper().Equals("COE-FM") && OSPFMGrpAppName.Trim().ToUpper().Equals("OSP-FM"))
            {
                isHavingCommonMtlGrp = true;
                mtlGrpAppName = "";
            }
            else
            {
                isHavingCommonMtlGrp = false;

                if (string.IsNullOrEmpty(OSPFMGrpAppName))
                {
                    if (string.IsNullOrEmpty(COEFMGrpAppName))
                    {
                        mtlGrpAppName = "";
                    }
                    else
                    {
                        mtlGrpAppName = "COEFM";
                    }
                }
                else
                {
                    mtlGrpAppName = "OSPFM";
                }
            }

            return isHavingCommonMtlGrp;
        }

        public static string GetRecordType(CatalogItem currentItemInfo)
        {
            string RecordType = string.Empty;

            try
            {
                RecordType = currentItemInfo.RecordType.ToUpper().Trim();

                return RecordType;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsValidItemForLoad(IEnumerable<XElement> CurrentItemDetails, int currentItemNodePosition, ref bool hasDataInAllCOEFMReqField)
        {
            bool IsValidItemForLoad = false;

            //make sure material type code for current item is same as either of OSPFMMaterialTypeCd or COEFMMaterialTypeCd key value
            // and also material group is there in material group table
            // then only this item is valid for OSPFM/COEFM
            string materialTypeXMLFieldName = "MTART";
            string materialGroupXMLFieldName = "MATKL";

            // check if MTART and MATKL nodes are found into current item xml node, 
            // it will set obtained/searched currentNode to null, if no such node found in this current item node
            var materialTypeNode = CurrentItemDetails.Where(x => x.Name.LocalName.ToString() == materialTypeXMLFieldName).FirstOrDefault();
            var materialGroupNode = CurrentItemDetails.Where(x => x.Name.LocalName.ToString() == materialGroupXMLFieldName).FirstOrDefault();

            if (materialTypeNode != null && !string.IsNullOrEmpty(materialTypeNode.Value) && materialGroupNode != null && !string.IsNullOrEmpty(materialGroupNode.Value))
            {
                string materialTypeCode = Convert.ToString(materialTypeNode.Value).ToUpper();
                string materialGroupCode = Convert.ToString(materialGroupNode.Value).ToUpper();

                CatalogProcessingManager catalogMgr = new CatalogProcessingManager();
                //*ArrayList alMaterialGrps = catalogMgr.FetchMaterialGrps(null,true);

                if (MtlGrpAppNamList == null)
                {
                    MtlGrpAppNamList = catalogMgr.FetchMaterialGrps(null, true);
                }

                if (true)
                {
                    ////If this is ospfm record, also it has materialGroupCode non-null value, then it should have material group listed in material group table
                    //if (materialTypeCode.Equals(ApplicationFieldConfiguration.OSPFMMaterialTypeCd))
                    //{
                    //    //*if (materialGroupCode.Trim() != string.Empty && !(alMaterialGrps.Contains(materialGroupCode)))
                    //    if (materialGroupCode.Trim() != string.Empty && !(MtlGrpAppNamList.ContainsKey(materialGroupCode)))
                    //    {
                    //        return false;
                    //    }
                    //}

                    //*if (!(MtlGrpAppNamList.ContainsKey(materialGroupCode)))
                    if (!(MtlGrpAppNamList.ContainsKey(materialGroupCode + "~OSP-FM")) && !(MtlGrpAppNamList.ContainsKey(materialGroupCode + "~COE-FM")))
                    {
                        // If failure of material group code, add this item to the material_catalog_fallout table
                        return false;
                    }

                    #region validate for coefm req fields, only if this is having coefm mtl grpId.

                    if (!HasDataInAllRequiredFieldsCOEFM(materialGroupCode, CurrentItemDetails, currentItemNodePosition))
                    {
                        //IsValidItemForLoad = false;
                        hasDataInAllCOEFMReqField = false;
                    }
                    else
                    {
                        //IsValidItemForLoad = true;
                        hasDataInAllCOEFMReqField = true;
                    }
                    //Let this dto pass through, even if it doesn't have all the coefm req fields
                    IsValidItemForLoad = true;

                    #endregion
                }
                else
                {
                    IsValidItemForLoad = false;
                }
            }
            else
            {
                IsValidItemForLoad = false;
                string missingFields = string.Empty;

                if (materialTypeNode == null || string.IsNullOrEmpty(materialTypeNode.Value))
                {
                    missingFields += "MTART,";
                }

                if (materialGroupNode == null || string.IsNullOrEmpty(materialGroupNode.Value))
                {
                    missingFields += "MATKL,";
                }

                if (missingFields != "")
                {
                    missingFields = missingFields.Remove(missingFields.Length - 1, 1);
                }

                //get MATNR Field value for this catalog item,
                string MATNRFieldValue = GetMATNRFieldValue(CurrentItemDetails);
                if (string.IsNullOrEmpty(MATNRFieldValue))
                {
                    MATNRFieldValue = "MATNR DATA NOT PROVIDED";
                }

                string requiredFieldValueMissingErrors = "Catalog Item at position:'" + Convert.ToString(currentItemNodePosition) + "' in xml having MATNR: '" + MATNRFieldValue + "' is invalid as it does not contain XMLNode/Value for '" + missingFields + "' field(s).";
                CatalogException MissingFieldsError = new CatalogException(requiredFieldValueMissingErrors);
                //ExceptionManager.PublishException(MissingFieldsError, (int)Constants.SentryIdentifier.IsValidItemForLoad, requiredFieldValueMissingErrors, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }

            return IsValidItemForLoad;
        }

        private bool HasDataInAllRequiredFieldsCOEFM(string materialGroupCode, IEnumerable<XElement> CurrentItemDetails, int currentItemNodePosition)
        {
            List<string> RequiredFieldsCOEFM = ApplicationFieldConfiguration.RequiredFieldsCOEFM;
            string requiredFieldValueMissingErrors = string.Empty;
            bool HasDataInAllRequiredFields = true;
            string requiredFieldsHavingNoDatainXML = string.Empty;

            //Check if MATNRField is having value for this catalog item, if not log an error and consider this as invalid item, and ignore it for loading
            string MATNRFieldValue = GetMATNRFieldValue(CurrentItemDetails);
            if (string.IsNullOrEmpty(MATNRFieldValue))
            {
                requiredFieldValueMissingErrors = "Catalog Item at position:'" + Convert.ToString(currentItemNodePosition) + "' in xml is invalid as it does not contain value for 'MATNR' required field";
                CatalogException PrimaryKeyFieldEmptyError = new CatalogException(requiredFieldValueMissingErrors);
                //ExceptionManager.PublishException(PrimaryKeyFieldEmptyError, (int)Constants.SentryIdentifier.HasDataInAllRequiredFieldsCOEFM, requiredFieldValueMissingErrors, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);

                return false;
            }

            #region validate presence of other required fields - ONLY if this is COEFM record.

            //if this grp code is not listed in table then below try section will throw exception
            // and we can ignore that excpetion as grp is not provided in xml
            string grpAppName = "";
            try
            {
                grpAppName = MtlGrpAppNamList[materialGroupCode + "~COE-FM"];
                if (string.IsNullOrEmpty(grpAppName))
                {
                    //Ignore if this grp code is not listed in table
                    grpAppName = "";
                }
            }
            catch
            {
                //Ignore if this grp code is not listed in table
                grpAppName = "";
            }

            if (grpAppName.ToUpper().Equals("COE-FM"))
            {
                foreach (string currentSAPField in RequiredFieldsCOEFM)
                {
                    // get current required field node details form item node in xml, 
                    // it will set obtained/searched currentNode to null, if no such node found in this current item node
                    var currentRequiredFieldNode = CurrentItemDetails.Where(x => x.Name.LocalName.ToString().ToUpper() == currentSAPField).FirstOrDefault();
                    if (currentRequiredFieldNode != null)
                    {
                        string currentRequiredFieldNodeValue = Convert.ToString(currentRequiredFieldNode.Value);
                        if (string.IsNullOrEmpty(currentRequiredFieldNodeValue))
                        {
                            HasDataInAllRequiredFields = false;
                            requiredFieldsHavingNoDatainXML += currentSAPField + ", ";
                        }
                    }
                    else
                    {
                        HasDataInAllRequiredFields = false;
                        requiredFieldsHavingNoDatainXML += currentSAPField + ", ";
                    }
                }
            }

            #endregion

            if (!HasDataInAllRequiredFields)//* && !string.IsNullOrEmpty(requiredFieldsHavingNoDatainXML))
            {
                //Remove "," at last position
                if (!(string.IsNullOrEmpty(requiredFieldsHavingNoDatainXML)))
                {
                    requiredFieldsHavingNoDatainXML = requiredFieldsHavingNoDatainXML.Remove((requiredFieldsHavingNoDatainXML.Length - 2), 2);
                }
                else
                {
                    requiredFieldsHavingNoDatainXML = Convert.ToString(requiredFieldsHavingNoDatainXML);
                }

                requiredFieldValueMissingErrors = "Catalog Item at position:'" + Convert.ToString(currentItemNodePosition) + "' in xml having MATNR: '" + MATNRFieldValue + "' is invalid as it does not contain value for these COEFM-required field(s): '" + requiredFieldsHavingNoDatainXML + "'";
                CatalogException requiredFieldEmptyError = new CatalogException(requiredFieldValueMissingErrors);
                //ExceptionManager.PublishException(requiredFieldEmptyError, (int)Constants.SentryIdentifier.HasDataInAllRequiredFieldsCOEFM, requiredFieldValueMissingErrors, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }

            return HasDataInAllRequiredFields;
        }

        private string GetMATNRFieldValue(IEnumerable<XElement> CurrentItemDetails)
        {
            var MATNRFieldNode = CurrentItemDetails.Where(x => x.Name.LocalName.ToString().ToUpper() == "MATNR").FirstOrDefault();
            if (MATNRFieldNode != null)
            {
                return Convert.ToString(MATNRFieldNode.Value);
            }
            else
            {
                return string.Empty; //*" DATA NOT PROVIEDED FOR MATNR";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentItemInfo">dto object for which prop need value setting</param>
        /// <param name="currentProperty">current property of this dto object for which we need to set value</param>
        /// <param name="CurrentFieldValue">node having value to set into current property</param>
        /// Note: In this method "currentProperty.SetValue(***" method sets a value to given class property for an object
        private void TypeCastDTOValues(CatalogItem currentItemInfo, PropertyInfo currentProperty, string CurrentFieldValue)
        {
            CurrentFieldValue = CurrentFieldValue.Trim();

            switch (Convert.ToString(currentProperty.PropertyType.Name.ToUpper()))
            {
                case ("INT32"):
                    currentProperty.SetValue(currentItemInfo, int.Parse(CurrentFieldValue), null);
                    break;
                case "BOOL":
                    currentProperty.SetValue(currentItemInfo, Convert.ToBoolean(CurrentFieldValue), null);
                    break;
                case "DATETIME":
                    if (string.IsNullOrEmpty(CurrentFieldValue))
                    {
                        currentProperty.SetValue(currentItemInfo, DateTime.Parse("01/01/0001"), null);
                    }
                    else
                    {
                        currentProperty.SetValue(currentItemInfo, DateTime.Parse(CurrentFieldValue), null);
                    }
                    break;
                case "DECIMAL":
                    currentProperty.SetValue(currentItemInfo, decimal.Parse(CurrentFieldValue), null);
                    break;
                default:
                    currentProperty.SetValue(currentItemInfo, Convert.ToString(CurrentFieldValue), null);
                    break;
            }
        }

        // Method to give dto's property value as per format required by oracle query
        // this method gets used to create oracle db query
        public string GetTypeCastedValue(PropertyInfo currentProperty, object CurrentFieldValue)
        {
            string formatedValue = string.Empty;
            try
            {
                switch (Convert.ToString(currentProperty.PropertyType.Name.ToUpper()))
                {
                    case ("INT32"):
                    case "DECIMAL":
                    case "STRING":
                        CurrentFieldValue = CurrentFieldValue.ToString().Replace("'", "''");
                        formatedValue = "'" + CurrentFieldValue + "'";
                        break;
                    case "BOOL":
                        bool tempBoolValue = bool.Parse(CurrentFieldValue.ToString());
                        formatedValue = (tempBoolValue) ? "'Y'" : "'N'";
                        break;
                    case "DATETIME":
                        if ((DateTime.Parse("01/01/0001") == DateTime.Parse(CurrentFieldValue.ToString())))
                        {
                            formatedValue = "null";
                        }
                        else
                        {
                            formatedValue = "to_date('" + Convert.ToString(
                                DateTime.Parse(CurrentFieldValue.ToString())) + "','MM/DD/RRRR HH:MI:SS PM')";
                        }
                        break;
                    default:
                        CurrentFieldValue = CurrentFieldValue.ToString().Replace("'", "''");
                        formatedValue = "'" + CurrentFieldValue + "'";
                        break;
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error in method GetTypeCastedValue - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.GetTypeCastedValue, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }
            return formatedValue;
        }

        //public static bool StoreRecievedXML(string xmlMessage)
        //{
        //    bool isStoredSuccessfully = false;
        //    string filePath = "";
        //    StreamWriter writer = null;

        //    try
        //    {
        //        if (ApplicationFieldConfiguration.shallEnableRecievedXMLStorage && ApplicationFieldConfiguration.isXMLStorageFolderCreated)
        //        {
        //            filePath = ApplicationFieldConfiguration.recievedXMLStoragePath;

        //            filePath += "SAPCatalogXML_" + DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + ".xml";

        //            writer = File.CreateText(filePath);
        //            writer.Write(xmlMessage);
        //            isStoredSuccessfully = true;

        //            //log an info message
        //            ApplicationFieldConfiguration.LogInfoMsg("System stored Recieved SAPCatalogXML message in a file at path: '" + filePath + "'.");
        //        }
        //    }
        //    catch
        //    {
        //        //Ignore and log an info message
        //        ApplicationFieldConfiguration.LogInfoMsg("System could not store Recieved SAPCatalogXML message, as error occurred while trying to create and write message to file at path: '" + filePath + "'!");
        //    }
        //    finally
        //    {
        //        if (writer != null)
        //        {
        //            writer.Close();
        //            writer.Dispose();
        //        }
        //    }

        //    return isStoredSuccessfully;
        //}
    }
}
