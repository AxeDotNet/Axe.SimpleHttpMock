using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.ServerImpl;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The mocked http server representing all external systems.
    /// </summary>
    public class MockHttpServer : HttpMessageHandler
    {
        readonly List<IRequestHandler> handlers = new List<IRequestHandler>(32);
        readonly List<IRequestHandler> defaultHandlers = new List<IRequestHandler>();
        readonly Dictionary<string, IRequestHandler> namedHandlers = new Dictionary<string, IRequestHandler>();
        IServerLogger logger = new DummyLogger();

        /// <summary>
        /// Add a http handler to the mocked server.
        /// </summary>
        /// <param name="handler">The handler that will accept certain messages and generate desired response.</param>
        /// <remarks>
        /// We do not want to make things complex so we introduce no scoring machinism to decide which request handler
        /// to pick. But we will pick the latest matched handler.
        /// </remarks>
        public void AddHandler(IRequestHandler handler)
        {
            handlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                namedHandlers.Add(handler.Name, handler);
            }
        }

        /// <summary>
        /// Add a default handler to mocked server. The default handler will be called if no handler matches the
        /// request.
        /// </summary>
        /// <param name="handler">The default request handler.</param>
        public void AddDefaultHandler(IRequestHandler handler)
        {
            defaultHandlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                namedHandlers.Add(handler.Name, handler);
            }
        }
        
        /// <summary>
        /// Consume the received request asynchronously using the registered handlers. if no handler is available
        /// a 404 response will be returned.
        /// </summary>
        /// <param name="request">The request received.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The generated response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string requestBriefing = $"{request.Method.Method} {request.RequestUri}";
            Logger.Log($"[Mock Server] Receiving request: {requestBriefing}");

            KeyValuePair<IRequestHandler, MatchingResult>? match = GetMatchedHandler(handlers, request);
            if (match == null)
            {
                match = GetMatchedHandler(defaultHandlers, request);
                if (match == null)
                {
                    Logger.Log($"[Mock Server] Cannot find matched handler for request: {requestBriefing}");
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"No mock-handler available for current request {requestBriefing}")
                    };
                }
            }

            IRequestHandler handler = match.Value.Key;
            MatchingResult matchingResult = match.Value.Value;

            Logger.Log($"[Mock Server] Matched handler found with name '{handler.Name}'");
            HttpResponseMessage response = await handler.HandleAsync(
                request,
                matchingResult.Parameters,
                cancellationToken,
                Logger).ConfigureAwait(false);
            Logger.Log($"[Mock Server] The request '{requestBriefing}' generates response '{response.StatusCode}'");
            return response;
        }

        static KeyValuePair<IRequestHandler, MatchingResult>? GetMatchedHandler(IList<IRequestHandler> handlers, HttpRequestMessage request)
        {
            var matchedHandler = handlers
                .Select(handler => new { Handler = handler, MatchingResult = handler.IsMatch(request)})
                .LastOrDefault(pair => pair.MatchingResult.IsMatch);
            if (matchedHandler == null) { return null; }
            return new KeyValuePair<IRequestHandler, MatchingResult>(matchedHandler.Handler, matchedHandler.MatchingResult);
        }

        /// <summary>
        /// Get named handler to retrive calling history or do some verification.
        /// </summary>
        /// <param name="handlerName">The name of the handler</param>
        /// <returns>The tracing information of the named handler.</returns>
        /// <exception cref="KeyNotFoundException">
        /// The <paramref name="handlerName"/> cannot be found in the registered handlers.
        /// </exception>
        public IRequestHandlerTracer this[string handlerName] => GetNamedHandlerTracer(handlerName);

        /// <summary>
        /// Get or set the logger for current mocked http server. You can get some detailed information which
        /// may of help for diagnostic.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The logger specified is <c>null</c>.
        /// </exception>
        public IServerLogger Logger
        {
            get => logger;
            set => logger = value ?? throw new ArgumentNullException(nameof(value));
        }

        IRequestHandlerTracer GetNamedHandlerTracer(string name)
        {
            if (namedHandlers.ContainsKey(name))
            {
                return namedHandlers[name];
            }

            throw new KeyNotFoundException(
                $"Cannot find a handler called \"{name}\". Please make sure you have provide a name argument when you are defining an API.");
        }
    }
}