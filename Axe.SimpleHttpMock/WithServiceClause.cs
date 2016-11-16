using System;
using System.Collections.Generic;
using System.Net;
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
            return Api(uriTemplate, (string[])null, responseFunc);
        }

        public WithServiceClause Api(string uriTemplate, string method, Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            return Api(uriTemplate, new [] {method}, responseFunc);
        }

        public WithServiceClause Api(string uriTemplate, string[] methods, Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, methods);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            return Api(uriTemplate, (string[])null, responseFunc);
        }

        public WithServiceClause Api(string uriTemplate, string method, Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            return Api(uriTemplate, new [] { method }, responseFunc);
        }

        public WithServiceClause Api(string uriTemplate, string[] methods, Func<IDictionary<string, object>, HttpResponseMessage> responseFunc)
        {
            Func<HttpRequestMessage, MatchingResult> matchingFunc = TheRequest.Is(serviceUriPrefix, uriTemplate, methods);
            server.When(matchingFunc).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, string method, HttpStatusCode statusCode)
        {
            return Api(uriTemplate, new[] {method}, statusCode);
        }

        public WithServiceClause Api(string uriTemplate, string[] methods, HttpStatusCode statusCode)
        {
            return Api(uriTemplate, methods, _ => statusCode.AsResponse());
        }

        public WithServiceClause Api(string uriTemplate, HttpStatusCode statusCode)
        {
            return Api(uriTemplate, (string[])null, _ => statusCode.AsResponse());
        }
        
        public MockHttpServer Done()
        {
            return server;
        }
    }
}