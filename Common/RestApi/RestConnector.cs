using System;
using System.Net;
using RestSharp;

namespace CenturyLink.Network.Engineering.Common.RestApi
{
    public abstract class RestConnector : IRestConnector
    {
        protected static string baseUrl = string.Empty;
        private string errorMessage = string.Empty;
        private string contentType = string.Empty;
        private long contentLength = 0;
        private HttpStatusCode statusCode = HttpStatusCode.Unused;
        private Exception errorException = null;
        private ResponseStatus responseStatus = ResponseStatus.None;

        private RestConnector()
        {
        }

        public RestConnector(string pBaseUrl)
        {
            baseUrl = pBaseUrl;
        }

        protected T Execute<T>(RestRequest request) where T : new()
        {
            RestClient client = new RestClient(baseUrl);
            var response = client.Execute<T>(request);

            statusCode = response.StatusCode;
            errorException = response.ErrorException;
            errorMessage = response.ErrorMessage;
            responseStatus = response.ResponseStatus;
            contentType = response.ContentType;
            contentLength = response.ContentLength;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if(response.ErrorException != null)
                    throw response.ErrorException;
            }

            return response.Data;
        }

        protected HttpStatusCode ResponseHttpStatusCode
        {
            get
            {
                return statusCode;
            }
        }

        protected Exception ErrorException
        {
            get
            {
                return errorException;
            }
        }

        protected string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
        }

        protected ResponseStatus RestResponseStatus
        {
            get
            {
                return responseStatus;
            }
        }

        protected string ResponseContentType
        {
            get
            {
                return contentType;
            }
        }

        protected long ContentLength
        {
            get
            {
                return contentLength;
            }
        }

        public abstract T Get<T>(RestRequest request) where T : new();

        public abstract T Post<T>(RestRequest request, object jsonBody) where T : new();
    }
}
