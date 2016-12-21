using System.Collections.Generic;
using System.Net.Http;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// Represents a calling history. A calling history contains a cloned HTTP request
    /// and the binded parameter (according to the implementation of
    /// <see cref="MatchingFunc"/>). The developer can then use the information to do
    /// verify jobs.
    /// </summary>
    public class CallingHistoryContext
    {
        internal CallingHistoryContext(
            HttpRequestMessage request, 
            IDictionary<string, object> parameters)
        {
            Request = request;
            Parameters = parameters;
        }

        /// <summary>
        /// Get the captured HTTP request object to the matched route. 
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Get binded parameters. If no parameter is available the property returns an 
        /// empty dictionary.
        /// </summary>
        public IDictionary<string, object> Parameters { get; } 
    }
}