using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.Matchers;

namespace Axe.SimpleHttpMock
{
    public class WithServiceClause
    {
        readonly MockHttpServer server;
        readonly string serviceUriPrefix;

        public WithServiceClause(MockHttpServer server, string serviceUriPrefix)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            if (serviceUriPrefix == null)
            {
                throw new ArgumentNullException(nameof(serviceUriPrefix));
            }

            var uri = new Uri(serviceUriPrefix, UriKind.Absolute);

            this.server = server;
            this.serviceUriPrefix = uri.AbsoluteUri;
        }
        
        void AddUriTemplateHandler(
            MatchingFunc requestMatchFunc,
            RequestHandlingFunc responseFunc,
            string name,
            bool defaultHandler = false)
        {
            if (requestMatchFunc == null)
            {
                throw new ArgumentNullException(nameof(requestMatchFunc));
            }

            if (responseFunc == null)
            {
                throw new ArgumentNullException(nameof(responseFunc));
            }

            if (defaultHandler)
            {
                server.AddDefaultHandler(new RequestHandler(requestMatchFunc, responseFunc, name));
            }
            else
            {
                server.AddHandler(new RequestHandler(requestMatchFunc, responseFunc, name));
            }
        }

        public WithServiceClause Default(
            Func<HttpRequestMessage, HttpResponseMessage> responseFunc, string name = null)
        {
            if (responseFunc == null)
            {
                throw new ArgumentNullException(nameof(responseFunc));
            }

            AddUriTemplateHandler(
                req =>
                    req.RequestUri != null &&
                    req.RequestUri.AbsoluteUri.StartsWith(serviceUriPrefix, StringComparison.InvariantCultureIgnoreCase),
                (req, p, c) => responseFunc(req),
                name,
                true);
            return this;
        }

        public WithServiceClause Default(HttpStatusCode statusCode, string name = null)
        {
            return Default(req => statusCode.AsResponse(), name);
        }

        public WithServiceClause Default<T>(T content, string name = null)
        {
            return Default(req => content.AsResponse(), name);
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
            MatchingFunc matchingFunc = RequestTemplateMatcher.CreateMatchingDelegate(
                serviceUriPrefix,
                uriTemplate,
                methods);
            AddUriTemplateHandler(matchingFunc, (req, @params, c) => responseFunc(req, @params), name);
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
            MatchingFunc matchingFunc = RequestTemplateMatcher.CreateMatchingDelegate(
                serviceUriPrefix,
                uriTemplate,
                methods);
            AddUriTemplateHandler(matchingFunc, (req, @params, c) => responseFunc(@params), name);
            return this;
        }

        public WithServiceClause Api(
            string uriTemplate,
            string method,
            HttpStatusCode statusCode,
            string name = null)
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

        public WithServiceClause Api(
            string uriTemplate,
            HttpStatusCode statusCode,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, _ => statusCode.AsResponse(), name);
        }

        public WithServiceClause Api<T>(
            string uriTemplate,
            string method,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, method, _ => response.AsResponse(), name);
        }

        public WithServiceClause Api<T>(
            string uriTemplate,
            string[] methods,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, methods, _ => response.AsResponse(), name);
        }

        public WithServiceClause Api<T>(
            string uriTemplate,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, _ => response.AsResponse(), name);
        }

        public MockHttpServer Done()
        {
            return server;
        }

        static void ThrowIfNull<T>(T value, string name)
        {
            if (value != null) { return; }
            throw new ArgumentNullException(name);
        }
    }
}