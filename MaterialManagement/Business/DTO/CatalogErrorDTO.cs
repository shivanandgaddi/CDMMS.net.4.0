using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.Material.Management.Business.DTO
{
    internal class CatalogErrorDTO : System.IComparable
    {
        #region Constructor
        public CatalogErrorDTO()
        {

        }
        #endregion

        #region Private


        long lBatchID;
        long lStageID;
        string sMaterialCode;

        string sErrorCode;
        string sErrorMsg;

        string sLastUpdatedUserID;
        DateTime dtLastUpdatedTimeStmp;

        #endregion

        #region Public

        public long BatchID
        {
            get
            {
                return lBatchID;
            }
            set
            {
                lBatchID = value;
            }
        }

        public long StageID
        {
            get
            {
                return lStageID;
            }
            set
            {
                lStageID = value;
            }
        }

        public string MaterialCode
        {
            get
            {
                return sMaterialCode;
            }
            set
            {
                sMaterialCode = value;
            }
        }

        public string ErrorCode
        {
            get
            {
                return sErrorCode;
            }
            set
            {
                sErrorCode = value;
            }
        }

        public string ErrorMsg
        {
            get
            {
                return sErrorMsg;
            }
            set
            {
                sErrorMsg = value;
            }
        }

        public string LastUpdatedUserID
        {
            get
            {
                return sLastUpdatedUserID;
            }
            set
            {
                sLastUpdatedUserID = value;
            }
        }

        public DateTime LastUpdatedTimeStmp
        {
            get
            {
                return dtLastUpdatedTimeStmp;
            }
            set
            {
                dtLastUpdatedTimeStmp = value;
            }
        }

        #endregion

        #region Sort On StageID

        /// 
        /// Less than zero if this instance is less than obj. 
        /// Zero if this instance is equal to obj. 
        /// Greater than zero if this instance is greater than obj. 
        ///
        ///
        /// This method uses the predefined method Int32.CompareTo 
        ///
        public int CompareTo(object obj)
        {
            if (!(obj is CatalogErrorDTO))
                throw new InvalidCastException("This object is not of type CatalogItemStagingDTO");

            CatalogErrorDTO catErrPart = (CatalogErrorDTO)obj;
            //no need to rewrite the code again, we have int.CompareTo ready to use
            return this.StageID.CompareTo(catErrPart.StageID);
        }

        #endregion
    }
}
