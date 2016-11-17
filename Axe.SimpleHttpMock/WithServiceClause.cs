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

        public WithServiceClause Api(
            string uriTemplate,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, responseFunc, name);
        }

        public WithServiceClause Api(
            string uriTemplate,
            string method,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, new[] {method}, responseFunc, name);
        }

        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            MatchingFunc matchingFunc = TheRequest.Is(
                serviceUriPrefix,
                uriTemplate,
                methods);
            new WhenClause(server, matchingFunc, name).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(
            string uriTemplate,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, responseFunc, name);
        }

        public WithServiceClause Api(
            string uriTemplate,
            string method,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, new[] {method}, responseFunc, name);
        }

        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            MatchingFunc matchingFunc = TheRequest.Is(
                serviceUriPrefix,
                uriTemplate,
                methods);
            new WhenClause(server, matchingFunc, name).Response(responseFunc);
            return this;
        }

        public WithServiceClause Api(string uriTemplate, string method, HttpStatusCode statusCode, string name = null)
        {
            return Api(uriTemplate, new[] {method}, statusCode, name);
        }

        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            HttpStatusCode statusCode,
            string name = null)
        {
            return Api(uriTemplate, methods, _ => statusCode.AsResponse(), name);
        }

        public WithServiceClause Api(string uriTemplate, HttpStatusCode statusCode, string name = null)
        {
            return Api(uriTemplate, (string[]) null, _ => statusCode.AsResponse(), name);
        }

        public MockHttpServer Done()
        {
            return server;
        }
    }
}