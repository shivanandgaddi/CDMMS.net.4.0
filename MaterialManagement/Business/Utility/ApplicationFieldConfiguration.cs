using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Utility;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Utility
{
    class ApplicationFieldConfiguration
    {
        //contains catalog item field name to db column mapping for catalog item staging table
        private static NameValueCollection fieldXMLKeyToDBNamePair = null;
        public static List<string> RequiredFieldsCOEFM = new List<string>();

        public static NameValueCollection GetFieldConfiguration()
        {
            try
            {
                // If object is not yet created then create it for the first time use, 
                // same will be used for further calls
                if (fieldXMLKeyToDBNamePair == null)
                {
                    fieldXMLKeyToDBNamePair = new NameValueCollection();

                    // look for xml having "catalog item field name to db column mapping" for catalog item staging table
                    XmlTextReader fieldConfigurationsReader = new XmlTextReader(ConfigurationManager.Value(Common.Utility.Constants.ApplicationName(APPLICATION_NAME.CATALOG_SVC), "localXSDPath"));
                    XDocument doc = XDocument.Load(fieldConfigurationsReader);

                    //get all nodes under StagingTableFields node for staging table mapping details
                    IEnumerable<XElement> AllElements = doc.Elements().Descendants("StagingTableFields").Descendants();

                    // For each node in StagingTableFields node, add a key value pair in collection
                    // key will be xml node name and value being nodevalue i.e. nothing but mapped db column name for this xml field
                    foreach (XElement element in AllElements)
                    {
                        fieldXMLKeyToDBNamePair.Add(element.Name.ToString().ToUpper(), element.Value.ToUpper());

                        //If this node has at least one attribute, then it will have attribute as "required", 
                        //which specifies this is a non nullable field for coefm record
                        if (element.HasAttributes)
                        {
                            RequiredFieldsCOEFM.Add(element.Name.ToString().ToUpper());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error in method ProcessMessage(string subject, string replySubject, MessageField[] fields) - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.GetFieldConfiguration, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }

            return fieldXMLKeyToDBNamePair;
        }

        // Gives mapped db column name for given catalog staging dto property
        public static string GetColumnName(string propertyName)
        {
            string columnName = string.Empty;

            try
            {
                propertyName = propertyName.ToUpper();

                switch (propertyName)
                {
                    #region handle dto properties for which there is no mapping in mapping xml
                    case "BATCHID":
                        {
                            columnName = "BATCHID";
                            break;
                        }
                    case "STAGEID":
                        {
                            columnName = "STAGEID";
                            break;
                        }
                    case "LASTUPDATEDUSERID":
                        {
                            columnName = "LAST_UPDTD_USERID";
                            break;
                        }
                    case "LASTUPDATEDTIMESTMP":
                        {
                            columnName = "LAST_UPDTD_TMSTMP";
                            break;
                        }
                    #endregion
                    // this default case handles any dto properties for which there is mapping in mapping xml
                    default:
                        {
                            columnName = fieldXMLKeyToDBNamePair.Get(propertyName);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                string sErrMsg = "Error in method ProcessMessage(string subject, string replySubject, MessageField[] fields) - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.GetCoulmnName, sErrMsg, ExceptionConstants.Severity.Major, ExceptionConstants.LogTo.All);
            }

            return columnName.ToUpper();
        }

        public static void LogInfoMsg(string sProcessInfoMsg)
        {
            try
            {
                //ExceptionManager.PublishException(sProcessInfoMsg, ExceptionConstants.LogTo.All);
            }
            catch
            {
                //Ignore as only info logging failed
            }
        }
    }
}
