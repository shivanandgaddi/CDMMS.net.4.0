using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    [Serializable]
    public class CatalogItem : System.IComparable
    {
        #region Private
        private long batchId;
        private long stageId;
        private string matnr = String.Empty;
        private string heciCode = String.Empty;
        private string mfrpn = string.Empty;
        private string mfrnr = String.Empty;
        private string apclCode = String.Empty;
        private string meins = String.Empty;
        private string matkl = String.Empty;
        private string zmaktx = String.Empty;
        private string name1 = String.Empty;
        private string lvorm = String.Empty;
        private string iccCode = String.Empty;
        private string aicCode = String.Empty;
        private string bstme = String.Empty;
        private string umrez = String.Empty;
        private string umren = String.Empty;
        private string poText = String.Empty;
        private string verpr = String.Empty;
        private string kzumw = String.Empty;
        private string plifz = String.Empty;
        private string mtart = String.Empty;
        private string zheight = String.Empty;
        private string zwidth = String.Empty;
        private string zdepth = String.Empty;
        private string lastUpdatedUserId;
        private string recordType = string.Empty; //Used to differentiate between coefm and ospfm xml records
        private string sValidStatus = null;
        private DateTime lastUpdatedTimeStamp;
        private DateTime ersda;
        private DateTime laeda;
        private DateTime dateSent;
        private string reviewIndicator = String.Empty;
        private string flowThruIndicator = String.Empty;
        private string emailBody = String.Empty;
        private string needsToBeReviewed = "Y";
        private bool failedProcessing = false;
        #endregion

        #region Public Properties
        public string MATNR
        {
            get
            {
                return matnr.TrimStart('0');
            }
            set
            {
                matnr = value.TrimStart('0');
            }
        }

        public string HECI_CODE
        {
            get
            {
                return heciCode;
            }
            set
            {
                heciCode = value;
            }
        }

        public string MFRPN
        {
            get
            {
                return mfrpn;
            }
            set
            {
                mfrpn = value;
            }
        }

        public string MFRNR
        {
            get
            {
                return mfrnr;
            }
            set
            {
                mfrnr = value;
            }
        }

        public string APCL_CODE
        {
            get
            {
                return apclCode;
            }
            set
            {
                apclCode = value;
            }
        }

        public string MEINS
        {
            get
            {
                return meins;
            }
            set
            {
                meins = value;
            }
        }

        public string MATKL
        {
            get
            {
                return matkl;
            }
            set
            {
                matkl = value;
            }
        }

        public string ZMAKTX
        {
            get
            {
                return zmaktx;
            }
            set
            {
                zmaktx = value;
            }
        }

        public string NAME1
        {
            get
            {
                return name1;
            }
            set
            {
                name1 = value;
            }
        }

        public string LVORM
        {
            get
            {
                return lvorm;
            }
            set
            {
                lvorm = value;
            }
        }

        public string ICC_CODE
        {
            get
            {
                return iccCode;
            }
            set
            {
                iccCode = value;
            }
        }

        public string AIC_CODE
        {
            get
            {
                return aicCode;
            }
            set
            {
                aicCode = value;
            }
        }

        public DateTime DATE_SENT
        {
            get
            {
                return dateSent;
            }
            set
            {
                dateSent = value;
            }
        }

        public string BSTME
        {
            get
            {
                return bstme;
            }
            set
            {
                bstme = value;
            }
        }

        public string UMREZ
        {
            get
            {
                return umrez;
            }
            set
            {
                umrez = value;
            }
        }

        public string UMREN
        {
            get
            {
                return umren;
            }
            set
            {
                umren = value;
            }
        }

        public DateTime ERSDA
        {
            get
            {
                return ersda;
            }
            set
            {
                ersda = value;
            }
        }

        public DateTime LAEDA
        {
            get
            {
                return laeda;
            }
            set
            {
                laeda = value;
            }
        }

        public string PO_TEXT
        {
            get
            {
                return poText;
            }
            set
            {
                poText = value;
            }
        }

        public string VERPR
        {
            get
            {
                return verpr;
            }
            set
            {
                verpr = value;
            }
        }

        public string KZUMW
        {
            get
            {
                return kzumw;
            }
            set
            {
                kzumw = value;
            }
        }

        public string PLIFZ
        {
            get
            {
                return plifz;
            }
            set
            {
                plifz = value;
            }
        }

        public string MTART
        {
            get
            {
                return mtart;
            }
            set
            {
                mtart = value;
            }
        }

        public string ZHEIGHT
        {
            get
            {
                return zheight;
            }
            set
            {
                zheight = value;
            }
        }

        public string ZWIDTH
        {
            get
            {
                return zwidth;
            }
            set
            {
                zwidth = value;
            }
        }

        public string ZDEPTH
        {
            get
            {
                return zdepth;
            }
            set
            {
                zdepth = value;
            }
        }

        public long BatchID
        {
            get
            {
                return batchId;
            }
            set
            {
                batchId = value;
            }
        }

        public long StageID
        {
            get
            {
                return stageId;
            }
            set
            {
                stageId = value;
            }
        }

        public string LastUpdatedUserID
        {
            get
            {
                return lastUpdatedUserId;
            }
            set
            {
                lastUpdatedUserId = value;
            }
        }

        public DateTime LastUpdatedTimeStmp
        {
            get
            {
                return lastUpdatedTimeStamp;
            }
            set
            {
                lastUpdatedTimeStamp = value;
            }
        }

        public string ValidStatus
        {
            get
            {
                return sValidStatus;
            }
            set
            {
                sValidStatus = value;
            }
        }

        public string RecordType
        {
            get
            {
                return recordType;
            }
            set
            {
                recordType = value;
            }
        }

        public string ReviewIndicator
        {
            get
            {
                return reviewIndicator;
            }
            set
            {
                reviewIndicator = value;
            }
        }

        public string FlowThruIndicator
        {
            get
            {
                return flowThruIndicator;
            }
            set
            {
                flowThruIndicator = value;
            }
        }

        public string EmailBody
        {
            get
            {
                return emailBody;
            }
            set
            {
                emailBody = value;
            }
        }

        public string NeedsToBeReviewed
        {
            get
            {
                return needsToBeReviewed;
            }
            set
            {
                needsToBeReviewed = value;
            }
        }

        public bool FailedProcessing
        {
            get
            {
                return failedProcessing;
            }
            set
            {
                failedProcessing = value;
            }
        }
        #endregion        

        #region Sort On Last Updated Time stamp
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
            if (!(obj is CatalogItem))
                throw new InvalidCastException("This object is not of type CatalogItem");

            CatalogItem catStgPart1 = (CatalogItem)obj;
            CatalogItem catStgPart2 = this;

            //Error while inserting into Material Item Staging Table - ORA-24381: error(s) 
            //in array DML ORA-01400: cannot insert NULL into ("NECTAS_OWNER"."MATERIAL_CATALOG_STAGING"."BATCH_ID") Error Production fix

            // Sorting of Records with different Product IDs and same Timestamp was not happening in order which has been fixed.
            // Sort on LastUpdatedTimeStamp,If the LastUpdatedTimestamp is the same,sort on Product ID - sxvija2,10/20/2008

            //no need to rewrite the code again, we have int.CompareTo ready to use
            if (catStgPart1.LastUpdatedTimeStmp.CompareTo(catStgPart2.LastUpdatedTimeStmp) < 0)
                return 1;
            else if (catStgPart1.LastUpdatedTimeStmp.CompareTo(catStgPart2.LastUpdatedTimeStmp) == 0)
                return catStgPart1.MATKL.CompareTo(catStgPart2.MATKL);
            else
                return -1;
        }
        #endregion
    }
}
