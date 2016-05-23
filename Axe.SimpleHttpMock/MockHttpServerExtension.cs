using System;
using System.Net.Http;

namespace Axe.SimpleHttpMock
{
    public static class MockHttpServerExtension
    {
        public static WhenClause When(
            this MockHttpServer server,
            Func<HttpRequestMessage, bool> requestMatchFunc)
        {
            return new WhenClause(server, requestMatchFunc);
        }
    }
}