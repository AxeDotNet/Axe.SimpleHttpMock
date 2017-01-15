using System.Collections.Concurrent;
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
        ConcurrentBag<object> m_diagnosticLogs;

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
        
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            IRequestHandler matchedHandler = m_handlers.LastOrDefault(m => m.IsMatch(request));
            if (matchedHandler == null)
            {
                matchedHandler = m_defaultHandlers.LastOrDefault(m => m.IsMatch(request));
                if (matchedHandler == null)
                {
                    LogDiagnosticInformation(new
                    {
                        request = request.RequestUri,
                        method = request.Method,
                        message = "No handler can handle this request."
                    });
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
            }

            return matchedHandler.HandleAsync(
                request,
                matchedHandler.GetParameters(request),
                cancellationToken);
        }

        void LogDiagnosticInformation(object log)
        {
            if (!EnableDiagnostic) { return; }
            m_diagnosticLogs.Add(log);
        }

        public IRequestHandlerTracer this[string handlerName] => GetNamedHandlerTracer(handlerName);

        public bool EnableDiagnostic
        {
            get { return m_diagnosticLogs != null; }
            set
            {
                if (value)
                {
                    if (m_diagnosticLogs == null)
                    {
                        m_diagnosticLogs = new ConcurrentBag<object>();
                    }
                }
                else
                {
                    m_diagnosticLogs = null;
                }
            }
        }

        public object[] DiagnosticInformation => m_diagnosticLogs.ToArray();

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