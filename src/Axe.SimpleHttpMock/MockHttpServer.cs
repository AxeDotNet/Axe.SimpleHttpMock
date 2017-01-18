using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The mocked http server representing all external systems.
    /// </summary>
    public class MockHttpServer : HttpMessageHandler
    {
        readonly List<IRequestHandler> m_handlers = new List<IRequestHandler>(32);
        readonly List<IRequestHandler> m_defaultHandlers = new List<IRequestHandler>();
        readonly Dictionary<string, IRequestHandler> m_namedHandlers = new Dictionary<string, IRequestHandler>();

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
            m_handlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                m_namedHandlers.Add(handler.Name, handler);
            }
        }

        /// <summary>
        /// Add a default handler to mocked server. The default handler will be called if no handler matches the
        /// request.
        /// </summary>
        /// <param name="handler">The default request handler.</param>
        public void AddDefaultHandler(IRequestHandler handler)
        {
            m_defaultHandlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                m_namedHandlers.Add(handler.Name, handler);
            }
        }
        
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
            IRequestHandler matchedHandler = m_handlers.LastOrDefault(m => m.IsMatch(request));
            if (matchedHandler == null)
            {
                Logger.Log($"[Mock Server] Cannot find matched handler for request: {requestBriefing}");

                matchedHandler = m_defaultHandlers.LastOrDefault(m => m.IsMatch(request));
                if (matchedHandler == null)
                {
                    Logger.Log($"[Mock Server] Cannot find default handler for request: {requestBriefing}");
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }

            Logger.Log($"[Mock Server] Matched handler found with name '{matchedHandler.Name}'");
            HttpResponseMessage response = await matchedHandler.HandleAsync(
                request,
                matchedHandler.GetParameters(request),
                cancellationToken).ConfigureAwait(false);
            Logger.Log($"[Mock Server] The request '{requestBriefing}' generates response '{response.StatusCode}'");
            return response;
        }

        public IRequestHandlerTracer this[string handlerName] => GetNamedHandlerTracer(handlerName);

        public IServerLogger Logger { get; set; } = new DummyLogger();

        IRequestHandlerTracer GetNamedHandlerTracer(string name)
        {
            if (m_namedHandlers.ContainsKey(name))
            {
                return m_namedHandlers[name];
            }

            throw new KeyNotFoundException(
                $"Cannot find a handler called \"{name}\". Please make sure you have provide a name argument when you are defining an API.");
        }
    }
}