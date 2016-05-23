using System;
using System.Net.Http;
using System.Threading;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    public class WhenClause
    {
        readonly MockHttpServer m_server;
        readonly Func<HttpRequestMessage, bool> m_requestMatchFunc;

        public WhenClause(MockHttpServer server, Func<HttpRequestMessage, bool> requestMatchFunc)
        {
            if (requestMatchFunc == null)
            {
                throw new ArgumentNullException(nameof(requestMatchFunc));
            }

            m_server = server;
            m_requestMatchFunc = requestMatchFunc;
        }

        public MockHttpServer Response(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFunc)
        {
            if (responseFunc == null)
            {
                throw new ArgumentNullException(nameof(responseFunc));
            }

            m_server.AddHandler(new RequestHandler(m_requestMatchFunc, responseFunc));
            return m_server;
        }
    }
}