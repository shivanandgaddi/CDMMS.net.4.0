namespace CenturyLink.Network.Engineering.Common.Utility
{
    public enum CHANGE_SET_STATUS { COMPLETED, ERROR, PROCESSING, READY };
    public enum CONFIGURATION_CATEGORY { ARM, CHANGE_SETS, POLLING, NDS_CONNECTOR };
    public enum APPLICATION_NAME { UNSET, FSLISTENER, CDMMS, CDMMS_DRAWING, CATALOG_SPEC, CATALOG_SVC, CATALOG_UI, MOS, FSREQUESTREPLY, PEG_LDAP_EXTRACTOR, NEISL };

    public class Constants
    {
        public static readonly string correlationIdFieldName = "CRID";

        public static string ChangeSetStatus(CHANGE_SET_STATUS status)
        {
            return System.Enum.GetName(typeof(CHANGE_SET_STATUS), status);
        }

        public static string ConfigurationCategory(CONFIGURATION_CATEGORY category)
        {
            return System.Enum.GetName(typeof(CONFIGURATION_CATEGORY), category);
        }

        public static string ApplicationName(APPLICATION_NAME application)
        {
            return System.Enum.GetName(typeof(APPLICATION_NAME), application);
        }
    }
}
