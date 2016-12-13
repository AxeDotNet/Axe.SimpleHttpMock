using System.Collections.Generic;
using System.Net.Http;

namespace Axe.SimpleHttpMock.ServerImpl
{
    public class CallingHistoryContext
    {
        public CallingHistoryContext(
            HttpRequestMessage request, 
            IDictionary<string, object> parameters)
        {
            Request = request;
            Parameters = parameters;
        }

        public HttpRequestMessage Request { get; }
        public IDictionary<string, object> Parameters { get; } 
    }
}