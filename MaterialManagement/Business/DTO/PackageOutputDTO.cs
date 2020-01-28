using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.Material.Management.Business.DTO
{
    [Serializable]
    internal class PackageOutputDTO
    {
        #region Constructor
        public PackageOutputDTO()
        {

        }
        #endregion

        #region private

        string sMsgCode;
        string sMsgText;
        int nOutStatus;

        #endregion

        #region public

        public string MsgCode
        {
            get
            {
                return sMsgCode;
            }
            set
            {
                sMsgCode = value;
            }
        }

        public string MsgText
        {
            get
            {
                return sMsgText;
            }
            set
            {
                sMsgText = value;
            }
        }

        public int ReturnValue
        {
            get
            {
                return nOutStatus;
            }
            set
            {
                nOutStatus = value;
            }
        }

        #endregion
    }
}
