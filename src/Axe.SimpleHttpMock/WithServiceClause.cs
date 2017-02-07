using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.Handlers;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The class to register handlers to a mocked service.
    /// </summary>
    public class WithServiceClause
    {
        readonly MockHttpServer server;
        readonly string serviceUriPrefix;

        /// <summary>
        /// Create a service registration clause.
        /// </summary>
        /// <param name="server">The mocked HTTP server.</param>
        /// <param name="serviceUriPrefix">The base address of a standalone service.</param>
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

        /// <summary>
        /// Add a default handler. If no other handler can handle the request, the default handler will do it.
        /// </summary>
        /// <param name="responseFunc">The function that accept a request and create a stub-response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Default(
            Func<HttpRequestMessage, HttpResponseMessage> responseFunc, string name = null)
        {
            if (responseFunc == null)
            {
                throw new ArgumentNullException(nameof(responseFunc));
            }
            AddDefaultHandler(
                serviceUriPrefix,
                responseFunc,
                name);
            return this;
        }

        /// <summary>
        /// Add a default handler. If no other handler can handle the request, the default handler will do it.
        /// </summary>
        /// <param name="statusCode">The status code that will be returned by default handler.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Default(HttpStatusCode statusCode, string name = null)
        {
            return Default(req => statusCode.AsResponse(), name);
        }

        /// <summary>
        /// Add a default handler. If no other handler can handle the request, the default handler will do it.
        /// </summary>
        /// <typeparam name="T">The response content type.</typeparam>
        /// <param name="content">The content payload of the response. The content will be serialized as JSON.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Default<T>(T content, string name = null)
        {
            return Default(req => content.AsResponse(), name);
        }

        /// <summary>Add an API handler using uri template.</summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="responseFunc">The delegate to generate stub-response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, responseFunc, name);
        }

        /// <summary>Add an API handler using uri template.</summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="method">The accepted HTTP method. This parameter is case insensitive.</param>
        /// <param name="responseFunc">The delegate to generate stub-response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            string method,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, new[] {method}, responseFunc, name);
        }

        /// <summary>Add an API handler using uri template.</summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="methods">The accepted HTTP methods collection. This parameter is case insensitive.</param>
        /// <param name="responseFunc">The delegate to generate stub-response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            AddUriTemplateHandler(
                serviceUriPrefix,
                uriTemplate,
                methods,
                (req, @params, c) => responseFunc(req, @params),
                name);
            
            return this;
        }

        /// <summary>
        /// Add an API handler using uri template ignoring the HTTP methods.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="responseFunc">
        /// The delegate to generate stub-response. The first parameter is the binded parameter while the second one is the request.
        /// </param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, responseFunc, name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="method">The accepted HTTP method. This parameter is case insensitive.</param>
        /// <param name="responseFunc">
        /// The delegate to generate stub-response. The first parameter is the binded parameter while the second one is the request.
        /// </param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            string method,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            return Api(uriTemplate, new[] {method}, responseFunc, name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="methods">The accepted HTTP methods collection. This parameter is case insensitive.</param>
        /// <param name="responseFunc">
        /// The delegate to generate stub-response. The first parameter is the binded parameter while the second one is the request.
        /// </param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>
        /// The <see cref="WithServiceClause"/> instance.
        /// </returns>
        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            Func<IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            AddUriTemplateHandler(
                serviceUriPrefix,
                uriTemplate,
                methods,
                (req, @params, c) => responseFunc(@params),
                name);

            return this;
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="method">The accepted HTTP method. This parameter is case insensitive.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            string method,
            HttpStatusCode statusCode,
            string name = null)
        {
            return Api(uriTemplate, new[] {method}, statusCode, name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="methods">The accepted HTTP methods collection. This parameter is case insensitive.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            string[] methods,
            HttpStatusCode statusCode,
            string name = null)
        {
            return Api(uriTemplate, methods, _ => statusCode.AsResponse(), name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api(
            string uriTemplate,
            HttpStatusCode statusCode,
            string name = null)
        {
            return Api(uriTemplate, (string[]) null, _ => statusCode.AsResponse(), name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="method">The accepted HTTP method. This parameter is case insensitive.</param>
        /// <param name="response">The payload of the response. The payload will then be converted to JSON content.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api<T>(
            string uriTemplate,
            string method,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, method, _ => response.AsResponse(), name);
        }

        /// <summary>
        /// Add an API handler using uri template.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="methods">The accepted HTTP methods collection. This parameter is case insensitive.</param>
        /// <param name="response">The payload of the response. The payload will then be converted to JSON content.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api<T>(
            string uriTemplate,
            string[] methods,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, methods, _ => response.AsResponse(), name);
        }

        /// <summary>
        /// Add an API handler using uri template ignoring the HTTP methods.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="uriTemplate">The relative uri template of the API.</param>
        /// <param name="response">The payload of the response. The payload will then be converted to JSON content.</param>
        /// <param name="name">
        /// The name of the handler. If there is no tracing requirement, leave this value as <c>null</c>. The default
        /// value of this parameter is <c>null</c>.
        /// </param>
        /// <returns>The <see cref="WithServiceClause"/> instance.</returns>
        public WithServiceClause Api<T>(
            string uriTemplate,
            T response,
            string name = null)
        {
            ThrowIfNull(response, nameof(response));
            return Api(uriTemplate, _ => response.AsResponse(), name);
        }

        public WithServiceClause RegexApi(
            string relativePathRegex,
            string[] methods,
            Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
            string name = null)
        {
            server.AddHandler(
                new RegexRequestHandler(
                    serviceUriPrefix,
                    relativePathRegex,
                    (req, @param, c) => responseFunc(req, @param),
                    name));
            return this;
        }

        /// <summary>
        /// Finish defining current service and prepare to start another.
        /// </summary>
        /// <returns>The mocked HTTP server.</returns>
        public MockHttpServer Done()
        {
            return server;
        }

        void AddDefaultHandler(
            string baseAddress,
            Func<HttpRequestMessage, HttpResponseMessage> handlingFunc,
            string name)
        {
            server.AddDefaultHandler(
                new BaseAddressOnlyRequestHandler(
                    baseAddress,
                    handlingFunc,
                    name));
        }

        void AddUriTemplateHandler(
            string baseAddress,
            string template,
            string[] methods,
            RequestHandlingFunc handlingFunc,
            string name)
        {
            server.AddHandler(
                new UriTemplateRequestHandler(
                    baseAddress,
                    template,
                    methods,
                    handlingFunc,
                    name));
        }

        static void ThrowIfNull<T>(T value, string name)
        {
            if (value != null) { return; }
            throw new ArgumentNullException(name);
        }
    }
}