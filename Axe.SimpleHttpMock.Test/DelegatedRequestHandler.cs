using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock.Test
{
    class DelegatedHandler : IRequestHandler
    {
        readonly Func<HttpRequestMessage, MatchingResult> m_matchFunc;
        readonly Func<HttpRequestMessage, dynamic, CancellationToken, HttpResponseMessage> m_handleFunc;

        public DelegatedHandler(
            Func<HttpRequestMessage, MatchingResult> matchFunc,
            Func<HttpRequestMessage, dynamic, CancellationToken, HttpResponseMessage> handleFunc)
        {
            m_matchFunc = matchFunc;
            m_handleFunc = handleFunc;
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matchFunc(request);
        }

        public IDictionary<string, object> GetParameters(HttpRequestMessage request)
        {
            return m_matchFunc(request).Parameters;
        }

        public HttpResponseMessage Handle(HttpRequestMessage request, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return m_handleFunc(request, parameters, cancellationToken);
        }
    }
}