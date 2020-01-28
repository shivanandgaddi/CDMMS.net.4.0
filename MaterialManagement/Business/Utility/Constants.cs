using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Utility
{
    public class Constants
    {
        public Constants()
        {

        }

        #region Sentry Identifier

        public enum SentryIdentifier
        {
            Information = 1000,
            Unknown = 1001,
            OnStart = 1002,
            Run = 1003,
            Stop = 1004,
            OnStop = 1005,
            StartBusListener = 1006,
            Message = 1007,
            ProcessMessage = 1008,
            ValidateXml = 1009,
            GetFieldConfiguration = 1010,
            GetCoulmnName = 1011,
            PopulateDTOFromMessage = 1012,
            GetTypeCastedValue = 1013,
            SendHeartbeatResponse = 1014,
            HandleLoggingResponseback = 1015,
            UnAuthorizedAccess,
            InsertIntoBatchProcTbl,
            InsertIntoCatalogStagingTbl,
            FetchNectasCategoryIDs,
            InsertIntoMtlPSTable,
            ValidateStagingData,
            ArchiveCatalogData,
            ParseXMLSchemaValidation,
            ParseXMLDataLoad,
            NotifySuccessfulTransactions,
            NotifyFailedTransactions,
            NotifyInfrastructureDataLoadFailure,
            NotifyNoCatalogDataReceived,
            FetchCatalogErrorCodes,
            FetchNectasUOMs,
            FetchICCCodes,
            FetchNectasCategories,
            SchemaValidationFailure,
            InsertIntoCatalogErrorTable,
            FetchBatchTimeStmp,
            FetchMaterialGrps,
            FetchMaterialTypeCodes,
            HasDataInAllRequiredFieldsCOEFM,
            IsValidItemForLoad,
            DoValidation
        }

        #endregion
    }
}
