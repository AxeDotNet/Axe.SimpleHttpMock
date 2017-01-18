using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.ServerImpl;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// <para>
    /// Represents a request handler. A request handler is like a common HTTP handler.
    /// It lays at the end of the request handling pipeline in responsible to generate
    /// a response according to the request.
    /// </para>
    /// <para>
    /// You can implement a request handler directly form this interface or you can try
    /// <see cref="DelegatedRequestHandler"/> to make it a little easier.
    /// </para>
    /// </summary>
    public interface IRequestHandler : IRequestHandlerTracer
    {
        /// <summary>
        /// Before the request can go into the handler. It must be checked if current handler
        /// is the right one to handle it. And this is what the method does.
        /// </summary>
        /// <param name="request">
        /// The actual HTTP request message.
        /// </param>
        /// <returns>
        /// <c>true</c>, if the request can be handled. Otherwise, <c>false</c>.
        /// </returns>
        bool IsMatch(HttpRequestMessage request);

        /// <summary>
        /// Accessing http request is powerful but not easy to use. So you can extract some
        /// useful information from the request before it goes into the handler. The extracting
        /// method is implementation specific.
        /// </summary>
        /// <param name="request">
        /// The actual HTTP request message.
        /// </param>
        /// <returns>
        /// The binded parameters. It is recommended to ignore the case of the key to make
        /// the parameter accessing easy to use.
        /// </returns>
        IDictionary<string, object> GetParameters(HttpRequestMessage request);

        /// <summary>
        /// The is the core function to create a response. This function will not be called
        /// if <see cref="IsMatch"/> returns <c>false</c>.
        /// </summary>
        /// <param name="request">The request message received.</param>
        /// <param name="parameters">The parameters that are extracted using the <see cref="GetParameters"/>
        /// method.</param>
        /// <param name="cancellationToken">
        /// The cancellation token passed to the async method.
        /// </param>
        /// <param name="logger">
        /// The verbose logger.
        /// </param>
        /// <returns>The generated http response message.</returns>
        Task<HttpResponseMessage> HandleAsync(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken,
            IServerLogger logger = null);
    }
}