using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Utility
{
    public class MaterialEditorException : Exception
    {
        public enum ERROR_CODE { GENERAL = 100, RT_PART_NO_EXISTS = 101 };
        private string errorMessage = "";
        private string dataJSON = "";
        private ERROR_CODE errorCode = ERROR_CODE.GENERAL;

        public MaterialEditorException() : base()
        {
        }

        public MaterialEditorException(string message) : base(message)
        {
            errorMessage = message;
        }

        public override string Message
        {
            get
            {
                string message = "{\"errCd\": " + (int)errorCode + ", \"msg\": \"" + errorMessage + "\"";

                if (!string.IsNullOrEmpty(dataJSON))
                    message += ", \"data\": " + dataJSON;

                message += "}";

                return message;
            }
        }

        public string DataJSON
        {
            set
            {
                dataJSON = value;
            }
        }

        public ERROR_CODE ErrorCode
        {
            set
            {
                errorCode = value;
            }
        }
    }
}