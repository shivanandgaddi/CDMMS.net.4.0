using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace CenturyLink.Network.Engineering.Common.RestApi
{
    interface IRestConnector
    {
        T Get<T>(RestRequest request) where T : new();
        T Post<T>(RestRequest request, object jsonBody) where T : new();
    }
}
