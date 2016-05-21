using System;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> m_handleFunc;
        readonly Func<HttpRequestMessage, bool> m_matcher;

        internal RequestHandler(Func<HttpRequestMessage, bool> matcher,
            Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handleFunc)
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

        public HttpResponseMessage Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return m_handleFunc(request, cancellationToken);
        }
    }
}