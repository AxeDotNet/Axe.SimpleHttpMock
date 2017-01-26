using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// The base class of request handler. This class is a good start to create your own
    /// <see cref="IRequestHandler"/>.
    /// </summary>
    public abstract class RequestHandlerBase : IRequestHandler
    {
        readonly ConcurrentQueue<CallingHistoryContext> m_callingHistories 
            = new ConcurrentQueue<CallingHistoryContext>();

        /// <summary>
        /// Initialize request handler.
        /// </summary>
        /// <param name="name">
        /// The name of the request handler. <see cref="Name"/> for details. The default value is
        /// <c>null</c>.
        /// </param>
        protected RequestHandlerBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the request handler. This value is the key to access tracing information.
        /// So no tracing information will be recorded if this value is not specifieid. 
        /// Its default values is <code>null</code>. 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the calling history collection. Please note that since the calling may be in
        /// a concurrent manner. So the sequence of the calling history cannot be guarenteed.
        /// </summary>
        /// <remarks>
        /// If the <see cref="IRequestHandlerTracer.Name"/> is <c>null</c>, This property will returns empty collection.
        /// </remarks>
        public IReadOnlyCollection<CallingHistoryContext> CallingHistories => m_callingHistories.ToArray();

        /// <summary>
        /// Before the request can go into the handler. It must be checked if current handler
        /// is the right one to handle it. And this is what the method does.
        /// </summary>
        /// <param name="request">
        /// The actual HTTP request message.
        /// </param>
        /// <returns>
        /// <c>true</c>, if the request can be handled. Otherwise, <c>false</c>. The matching
        /// result can also contains the parameters extracted from the request.
        /// </returns>
        public abstract MatchingResult IsMatch(HttpRequestMessage request);

        /// <summary>
        /// The is the core function to create a response. This function will not be called
        /// if <see cref="IRequestHandler.IsMatch"/> returns <c>false</c>.
        /// </summary>
        /// <param name="request">The request message received.</param>
        /// <param name="parameters">The parameters that are extracted using the <see cref="IRequestHandler.IsMatch"/>
        /// method.</param>
        /// <param name="cancellationToken">
        /// The cancellation token passed to the async method.
        /// </param>
        /// <param name="logger">
        /// The verbose logger.
        /// </param>
        /// <returns>The generated http response message.</returns>
        public async Task<HttpResponseMessage> HandleAsync(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken,
            IServerLogger logger = null)
        {
            IServerLogger actualLogger = logger ?? new DummyLogger();

            if (Name != null)
            {
                HttpRequestMessage cloned = await CloneHttpRequestMessageAsync(request).ConfigureAwait(false);
                actualLogger.Log($"[Handler] Record calling history with name '{Name}'");
                m_callingHistories.Enqueue(new CallingHistoryContext(cloned, parameters));
            }

            try
            {
                return CreateResponse(request, parameters, cancellationToken);
            }
            catch (Exception error)
            {
                actualLogger.Log($"[Handler] Unexpected error occured during handler evaluation: {error.Message}. Will return Internal Server Error");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(error.ToString())
                };
            }
        }

        /// <summary>
        /// Create stub request for matched HTTP calls.
        /// </summary>
        /// <param name="request">The actual request message.</param>
        /// <param name="parameters">The parameters extracted from matching function.</param>
        /// <param name="cancellationToken">The callcellation token for the async process.</param>
        /// <returns>The http response message</returns>
        protected abstract HttpResponseMessage CreateResponse(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken);

        static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                    {
                        clone.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }

            clone.Version = req.Version;

            foreach (KeyValuePair<string, object> prop in req.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}