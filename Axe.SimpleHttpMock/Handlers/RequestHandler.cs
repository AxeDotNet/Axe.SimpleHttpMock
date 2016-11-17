using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        readonly HandlerFunc m_handleFunc;
        readonly MatchingFunc m_matcher;
        readonly List<CallingContext> m_callingHistories = new List<CallingContext>(); 

        internal RequestHandler(MatchingFunc matcher, HandlerFunc handleFunc, string name)
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

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matcher(request);
        }

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
                m_callingHistories.Add(new CallingContext(cloned, parameters));
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

        public IReadOnlyCollection<CallingContext> CallingHistories => m_callingHistories.AsReadOnly();
    }
}