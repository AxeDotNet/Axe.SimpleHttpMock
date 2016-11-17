using System;
using System.Net.Http;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    public static class MockHttpServerExtension
    {
        public static WithServiceClause WithService(
            this MockHttpServer server,
            string serviceUriPrefix)
        {
            return new WithServiceClause(server, serviceUriPrefix);
        }
    }
}