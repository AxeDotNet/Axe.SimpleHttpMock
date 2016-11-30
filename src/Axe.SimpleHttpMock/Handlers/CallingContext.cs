using System.Collections.Generic;
using System.Net.Http;

namespace Axe.SimpleHttpMock.Handlers
{
    public class CallingContext
    {
        public CallingContext(
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