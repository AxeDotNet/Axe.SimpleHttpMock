using System;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Test
{
    class DelegatedHandler : IRequestHandler
    {
        readonly Func<HttpRequestMessage, bool> m_matchFunc;
        readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> m_handleFunc;

        public DelegatedHandler(
            Func<HttpRequestMessage, bool> matchFunc,
            Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handleFunc)
        {
            m_matchFunc = matchFunc;
            m_handleFunc = handleFunc;
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matchFunc(request);
        }

        public HttpResponseMessage Handle(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return m_handleFunc(request, cancellationToken);
        }
    }
}