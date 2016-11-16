using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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

        public MockHttpServer Response(
            HttpStatusCode statusCode,
            object payload = null,
            MediaTypeFormatter formatter = null)
        {
            return Response(
                req =>
                {
                    ObjectContent<object> content = payload == null
                        ? null
                        : new ObjectContent<object>(
                            payload,
                            formatter ?? new JsonMediaTypeFormatter());

                    return new HttpResponseMessage(statusCode)
                    {
                        Content = content
                    };
                });
        }

        public MockHttpServer Response(Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
        {
            return Response((req, c) => responseFunc(req));
        }
    }
}