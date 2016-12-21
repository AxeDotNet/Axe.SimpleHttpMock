using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// Represents a delegated request handler. This is a good start if you want to
    /// implement your own request handler.
    /// </summary>
    public class DelegatedRequestHandler : IRequestHandler
    {
        readonly RequestHandlingFunc m_handleFunc;
        readonly MatchingFunc m_matcher;
        readonly List<CallingHistoryContext> m_callingHistories = new List<CallingHistoryContext>(); 

        /// <summary>
        /// Create a <see cref="DelegatedRequestHandler"/> instance.
        /// </summary>
        /// <param name="matcher">
        /// A delegate determines that if current handler can handle the HTTP request message.
        /// </param>
        /// <param name="handleFunc">
        /// A delegate to create and return HTTP response message. Usually this is defined by
        /// test writer.
        /// </param>
        /// <param name="name">
        /// The name of the handler. This parameter is very helpful if you want to track
        /// the calling history.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="matcher"/> is <c>null</c> or the <paramref name="handleFunc"/>
        /// is <c>null</c>.
        /// </exception>
        public DelegatedRequestHandler(MatchingFunc matcher, RequestHandlingFunc handleFunc, string name)
        {
            if (handleFunc == null)
            {
                throw new ArgumentNullException(nameof(handleFunc));
            }

            if (matcher == null)
            {
                throw new ArgumentNullException(nameof(matcher));
            }

            m_handleFunc = handleFunc;
            m_matcher = matcher;
            Name = name;
        }

        /// <summary>
        /// Using user defined delegate to check if current handler can handle certain request.
        /// </summary>
        /// <param name="request">The actual HTTP request message.</param>
        /// <returns>
        /// <c>true</c>, if the request can be handled. Otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matcher(request);
        }

        /// <summary>
        /// Using user defined delegate to get the binded parameters.
        /// </summary>
        /// <param name="request">The actual HTTP request message.</param>
        /// <returns>
        /// The binded parameters.
        /// </returns>
        public IDictionary<string, object> GetParameters(HttpRequestMessage request)
        {
            return m_matcher(request).Parameters;
        }
        
        public async Task<HttpResponseMessage> HandleAsync(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            if (Name != null)
            {
                HttpRequestMessage cloned = await CloneHttpRequestMessageAsync(request);
                m_callingHistories.Add(new CallingHistoryContext(cloned, parameters));
            }

            return m_handleFunc(request, parameters, cancellationToken);
        }

        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            
            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                    {
                        clone.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }
            
            clone.Version = req.Version;

            foreach (KeyValuePair<string, object> prop in req.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public string Name { get; }

        public IReadOnlyCollection<CallingHistoryContext> CallingHistories => m_callingHistories.AsReadOnly();
    }
}