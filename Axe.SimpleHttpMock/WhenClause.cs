using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    class WhenClause
    {
        readonly MockHttpServer m_server;
        readonly MatchingFunc m_requestMatchFunc;
        readonly string m_name;

        public WhenClause(MockHttpServer server, MatchingFunc requestMatchFunc, string name)
        {
            if (requestMatchFunc == null)
            {
                throw new ArgumentNullException(nameof(requestMatchFunc));
            }

            m_server = server;
            m_requestMatchFunc = requestMatchFunc;
            m_name = name;
        }

        public MockHttpServer Response(HandlerFunc responseFunc)
        {
            if (responseFunc == null)
            {
                throw new ArgumentNullException(nameof(responseFunc));
            }

            m_server.AddHandler(new RequestHandler(m_requestMatchFunc, responseFunc, m_name));
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

        public MockHttpServer Response(Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            return Response((req, p, c) => responseFunc(req, p));
        }

        public MockHttpServer Response(Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            return Response((req, p) => responseFunc(p));
        }
    }
}