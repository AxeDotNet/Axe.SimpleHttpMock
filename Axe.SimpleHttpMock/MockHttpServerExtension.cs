using System;
using System.Net.Http;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    public static class MockHttpServerExtension
    {
        public static WhenClause When(
            this MockHttpServer server,
            Func<HttpRequestMessage, MatchingResult> requestMatchFunc)
        {
            return new WhenClause(server, requestMatchFunc);
        }
    }
}