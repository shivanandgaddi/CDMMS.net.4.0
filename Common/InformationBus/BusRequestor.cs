using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qwest.IB;
using CenturyLink.Network.Engineering.Common.Utility;

namespace CenturyLink.Network.Engineering.Common.InformationBus
{
    public class BusRequestor : IBusObject
    {
        public enum MessageFieldNameSelectorTypes { Default, AddYourOwn }; 

        private MessageFieldNameSelectorTypes messageFieldNameType = MessageFieldNameSelectorTypes.Default;
        private string busServiceConfigurationKey = "";
        private string busNetworkConfigurationKey = "";
        private string busDaemonConfigurationKey = "";
        private string busTimeoutConfigurationKey = "";
        private string busPublishSubjectConfigurationKey = "";
        private string configurationSystemName = "";
        private string correlationIdFieldName = Constants.correlationIdFieldName;
        private Requestor requestor = null;

        public BusRequestor()
        {
        }

        public BusRequestor(string busSubjectConfigurationKey)
        {
            busPublishSubjectConfigurationKey = busSubjectConfigurationKey;
        }

        public BusRequestor(MessageFieldNameSelectorTypes fieldNameType)
        {
            MessageFieldNameType = fieldNameType;
        }

        public BusRequestor(string configurationKeySystemName, string serviceConfigurationKey, string networkConfigurationKey, string daemonConfigurationKey, string timeoutConfigurationKey,
            string publishSubjectConfigurationKey)
        {
            configurationSystemName = configurationKeySystemName;
            busServiceConfigurationKey = serviceConfigurationKey;
            busNetworkConfigurationKey = networkConfigurationKey;
            busDaemonConfigurationKey = daemonConfigurationKey;
            busTimeoutConfigurationKey = timeoutConfigurationKey;
            busPublishSubjectConfigurationKey = publishSubjectConfigurationKey;

            requestor = new Requestor(Service, Network, Daemon);
        }

        public MessageFieldNameSelectorTypes MessageFieldNameType
        {
            get
            {
                return this.messageFieldNameType;
            }
            set
            {
                this.messageFieldNameType = value;
            }
        }

        public string[] Request(string requestData, string correlationId)
        {
            MessageField[] requestMessageFields = new MessageField[2];
            MessageField dataMessageField = new MessageField();
            MessageField correlationIdMessageField = new MessageField();
            string[] response = new string[2];

            if (requestor == null)
                requestor = new Requestor(Service, Network, Daemon);

            dataMessageField.FieldName = GetMessageFieldFieldNameForSubject();
            dataMessageField.Data = requestData;

            correlationIdMessageField.FieldName = correlationIdFieldName;
            correlationIdMessageField.Data = correlationId;

            requestMessageFields[0] = correlationIdMessageField;
            requestMessageFields[1] = dataMessageField;

            MessageField[] responseMessageFields = requestor.request(PublishSubject, requestMessageFields, Timeout);

            response[0] = GetDataForFieldName(correlationIdFieldName, responseMessageFields);
            response[1] = GetDataForFieldName(GetMessageFieldFieldNameForSubject(), responseMessageFields);

            return response;
        }

        public string Request(string requestData)
        {
            if (requestor == null)
                requestor = new Requestor(Service, Network, Daemon);

            MessageField messageField = new MessageField();

            messageField.FieldName = GetMessageFieldFieldNameForSubject();
            messageField.Data = requestData;

            MessageField[] responseMessageField = requestor.request(PublishSubject, requestData, Timeout);

            return GetDataForFieldName(GetMessageFieldFieldNameForSubject(), responseMessageField);
        }

        public string GetMessageFieldFieldNameForSubject()
        {
            /*
             * If another message field is needed, first add it to the MessageFieldNameSelectorTypes enum.
             * Then add logic here to check MessageFieldNameType.
             * For example:
            if (MessageFieldNameType == MessageFieldNameSelectorTypes.MY_CUSTOM_FIELD_NAME)
                return "CUSTOM_FIELD_NAME";
            */

            return "DATA";
        }

        private string GetDataForFieldName(string fieldName, MessageField[] messageField)
        {
            string responseString = null;

            for (int i = 0; i < messageField.Length; i++)
            {
                if (messageField[i].FieldName == fieldName)
                    responseString = (string)messageField[i].Data;
            }

            if (responseString == null)
                throw new Exception("Unable to locate the data for MessageField.FieldName: " + fieldName + " in the MessageField object.");

            return responseString;
        }

        #region Properties
        private string Service
        {
            get
            {
                /*
                 * Note: If the "real" project saves configuration values in the database uncomment the below if/else
                 * and replace "<Add custom logic here>" with the correct logic. Do the same for Network, Daemon and Timeout.
                */
                //if ("".Equals(busServiceConfigurationKey))
                return System.Configuration.ConfigurationManager.AppSettings["IB_SERVICE"];
                //else
                    //return <Add custom logic here>;
            }
        }

        private string Network
        {
            get
            {
                //if ("".Equals(busNetworkConfigurationKey))
                    return System.Configuration.ConfigurationManager.AppSettings["IB_NETWORK"];
                //else
                    //return <Add custom logic here>;
            }
        }

        private string Daemon
        {
            get
            {
                //if ("".Equals(busDaemonConfigurationKey))
                    return System.Configuration.ConfigurationManager.AppSettings["IB_DAEMON"];
                //else
                    //return < Add custom logic here >;
            }
        }

        private int Timeout
        {
            get
            {
                //if ("".Equals(busTimeoutConfigurationKey))
                    return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IB_TIMEOUT"]);
                //else
                    //return Convert.ToInt32(< Add custom logic here >);
            }
        }

        private string PublishSubject
        {
            get
            {
                if ("".Equals(busPublishSubjectConfigurationKey))
                    throw new Exception("Property BusPublishSubjectConfigurationKey must be set");

                return System.Configuration.ConfigurationManager.AppSettings[busPublishSubjectConfigurationKey];
            }
        }

        public string BusServiceConfigurationKey
        {
            set
            {
                busServiceConfigurationKey = value;
            }
        }

        public string BusNetworkConfigurationKey
        {
            set
            {
                busNetworkConfigurationKey = value;
            }
        }

        public string BusDaemonConfigurationKey
        {
            set
            {
                busDaemonConfigurationKey = value;
            }
        }

        public string BusListenerSubjectConfigurationKey
        {
            set
            {
                busPublishSubjectConfigurationKey = value;
            }
        }

        public string BusTimeoutConfigurationKey
        {
            set
            {
                busTimeoutConfigurationKey = value;
            }
        }
        #endregion

        public string GetConfigurationValues()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Configuration System name: " + configurationSystemName + Environment.NewLine);
            sb.Append("Service: " + busServiceConfigurationKey + " / " + Service + Environment.NewLine);
            sb.Append("Network: " + busNetworkConfigurationKey + " / " + Network + Environment.NewLine);
            sb.Append("Daemon: " + busDaemonConfigurationKey + " / " + Daemon + Environment.NewLine);
            sb.Append("Listener Subject: " + busPublishSubjectConfigurationKey + " / " + PublishSubject + Environment.NewLine);
            sb.Append("Timeout: " + busTimeoutConfigurationKey + " / " + Timeout + Environment.NewLine);

            return sb.ToString();
        }

        public void Dispose()
        {
            try
            {
                if (requestor != null)
                    requestor.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
