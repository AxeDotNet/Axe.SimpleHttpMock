using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        readonly Func<HttpRequestMessage, IDictionary<string, object>, CancellationToken, HttpResponseMessage> m_handleFunc;
        readonly Func<HttpRequestMessage, MatchingResult> m_matcher;

        internal RequestHandler(
            Func<HttpRequestMessage, MatchingResult> matcher,
            Func<HttpRequestMessage, IDictionary<string, object>, CancellationToken, HttpResponseMessage> handleFunc)
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
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matcher(request);
        }

        public IDictionary<string, object> GetParameters(HttpRequestMessage request)
        {
            return m_matcher(request).Parameters;
        }

        public HttpResponseMessage Handle(HttpRequestMessage request, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return m_handleFunc(request, (dynamic)parameters, cancellationToken);
        }
    }
}