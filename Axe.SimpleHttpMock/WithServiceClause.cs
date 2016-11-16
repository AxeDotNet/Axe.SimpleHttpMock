using System;
using System.Collections.Generic;
using System.Net.Http;
using Axe.SimpleHttpMock.Handlers;
using Axe.SimpleHttpMock.Matchers;

namespace Axe.SimpleHttpMock
{
    public class WithServiceClause
    {
        readonly MockHttpServer server;
        readonly string serviceUriPrefix;

        public WithServiceClause(MockHttpServer server, string serviceUriPrefix)
        {
            this.server = server;
            this.serviceUriPrefix = serviceUriPrefix;
        }

        public WithServiceClause Api(string uriTemplate, Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, null);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, string[] methods, Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, methods);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, null);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, string[] methods, Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, methods);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public MockHttpServer Done()
        {
            return server;
        }
    }
}