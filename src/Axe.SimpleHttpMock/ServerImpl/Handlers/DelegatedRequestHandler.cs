using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    /// <summary>
    /// Represents a delegated request handler. This is a good start if you want to
    /// implement your own request handler.
    /// </summary>
    public sealed class DelegatedRequestHandler : RequestHandlerBase
    {
        readonly RequestHandlingFunc handleFunc;
        readonly MatchingFunc matcher;

        /// <summary>
        /// Create a <see cref="DelegatedRequestHandler"/> instance.
        /// </summary>
        /// <param name="matcher">
        /// A delegate determines that if current handler can handle the HTTP request message.
        /// </param>
        /// <param name="handleFunc">
        /// A delegate to create and return HTTP response message. Usually this is defined by
        /// test writer.
        /// </param>
        /// <param name="name">
        /// The name of the handler. This parameter is very helpful if you want to track
        /// the calling history.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="matcher"/> is <c>null</c> or the <paramref name="handleFunc"/>
        /// is <c>null</c>.
        /// </exception>
        public DelegatedRequestHandler(MatchingFunc matcher, RequestHandlingFunc handleFunc, string name)
            : base(name)
        {
            this.handleFunc = handleFunc ?? throw new ArgumentNullException(nameof(handleFunc));
            this.matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        }

        /// <summary>
        /// Using user defined delegate to check if current handler can handle certain request.
        /// </summary>
        /// <param name="request">The actual HTTP request message.</param>
        /// <returns>
        /// <c>true</c>, if the request can be handled. Otherwise, <c>false</c>. The matching
        /// result can also contains the parameters extracted from the request.
        /// </returns>
        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            return matcher(request);
        }

        /// <summary>
        /// Create stub request for matched HTTP calls using the handling delegate.
        /// </summary>
        /// <param name="request">The actual request message.</param>
        /// <param name="parameters">The parameters extracted from matching function.</param>
        /// <param name="cancellationToken">The callcellation token for the async process.</param>
        /// <returns>The http response message</returns>
        protected override HttpResponseMessage CreateResponse(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            return handleFunc(request, parameters, cancellationToken);
        }
    }
}