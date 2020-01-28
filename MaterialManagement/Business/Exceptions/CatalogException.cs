using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.ExceptionManager;
using CenturyLink.Network.Engineering.Material.Management.Business.Utility;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Exceptions
{
    [System.Serializable()]
    public class CatalogException : ApplicationException
    {
        public const int UNKNOWN = -1;

        // Default constructor
        /// <summary>
        /// Sentry Severity
        /// </summary>
        private ExceptionConstants.Severity eSentrySeverity;


        private int errorCode = UNKNOWN;

        /// <summary>
        /// Sentry Identifier
        /// </summary>
        private Constants.SentryIdentifier eSentryIdentifier;
        private bool logThis;
        /// <summary>
        /// Sentry Information
        /// </summary>
        private string sInformation;

        /// <summary>
        /// Read only property for Sentry Severity
        /// </summary>

        public ExceptionConstants.Severity SentrySeverity
        {
            get
            {
                return this.eSentrySeverity;
            }
        }
        /// <summary>
        /// Read only property for Sentry Identifier
        /// </summary>
        public Constants.SentryIdentifier SentryIdentifier
        {
            get
            {
                return this.eSentryIdentifier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ErrorCode
        {
            get
            {
                return errorCode;
            }
            set
            {
                errorCode = value;
            }
        }
        public bool LogThis
        {
            get
            {
                return this.logThis;
            }

        }
        /// <summary>
        /// Read only property for Sentry Information
        /// </summary>
        public string Information
        {
            get
            {
                return this.sInformation;
            }
        }

        public CatalogException()
            : base()
        {

        }

        /// <summary>
        /// Constructor with exception message
        /// </summary>
        /// <param name="message">Exception Message.</param>
        /// <param name="error">Error code to be published.</param>
        public CatalogException(string message, int error)
            : base(message)
        {
            this.errorCode = error;
        }

        public CatalogException(string message, int error, bool LogThis)
            : this(message, null, ExceptionConstants.Severity.Unknown, Constants.SentryIdentifier.Unknown, null, LogThis, error)
        {

        }

        /// <summary>
        /// Constructor with exception message
        /// </summary>
        /// <param name="message">Exception Message</param>
        public CatalogException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">inner message</param>
        public CatalogException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
        /// <summary>
        /// Custom Constructor with inner message
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">inner message</param>
        /// <param name="eSentrySeverity">Sentry Severity</param>
        /// <param name="eSentryIdentifier">Sentry Identifier</param>
        /// <param name="sInformation">Sentry Information</param>
        public CatalogException(string message,
        System.Exception inner,
        ExceptionConstants.Severity eSentrySeverity,
        Constants.SentryIdentifier eSentryIdentifier,
        string sInformation)
            : this(message, inner, eSentrySeverity, eSentryIdentifier, sInformation, false)
        {

        }
        /// <summary>
        /// Custom Constructor without inner message
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="eSentrySeverity">Sentry Severity</param>
        /// <param name="eSentryIdentifier">Sentry Identifier</param>
        /// <param name="sInformation">Sentry Information</param>

        //		public BidAwardException(string message, ExceptionConstants.Severity eSentrySeverity, 
        //			ConstantUtility.SentryIdentifier eSentryIdentifier, string sInformation,bool LogThis) : this( message,null ,eSentrySeverity,eSentryIdentifier,sInformation,LogThis)
        //		{
        //			
        //		}
        public CatalogException(string message, System.Exception inner,
        ExceptionConstants.Severity eSentrySeverity,
        Constants.SentryIdentifier eSentryIdentifier, string sInformation, bool LogThis)
            : this(message, inner, eSentrySeverity, eSentryIdentifier, sInformation, LogThis, -1)
        {

        }

        public CatalogException(string message, System.Exception inner,
        ExceptionConstants.Severity eSentrySeverity,
        Constants.SentryIdentifier eSentryIdentifier, string sInformation, bool LogThis, int errorCode)
            : base(message, inner)
        {
            this.eSentryIdentifier = eSentryIdentifier;
            this.eSentrySeverity = eSentrySeverity;
            this.sInformation = sInformation;
            this.logThis = LogThis;
            this.errorCode = errorCode;
        }
        /// <summary>
        /// Protected constructor to de-serialize data
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming Context</param>
        protected CatalogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
