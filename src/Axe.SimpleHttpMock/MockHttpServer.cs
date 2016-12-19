using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    public class MockHttpServer : HttpMessageHandler
    {
        readonly List<IRequestHandler> m_handlers = new List<IRequestHandler>(32);
        readonly List<IRequestHandler> m_defaultHandlers = new List<IRequestHandler>();
        readonly Dictionary<string, IRequestHandler> m_namedHandlers = new Dictionary<string, IRequestHandler>(); 

        public void AddHandler(IRequestHandler handler)
        {
            m_handlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                m_namedHandlers.Add(handler.Name, handler);
            }
        }

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
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
            }

            return matchedHandler.HandleAsync(
                request,
                matchedHandler.GetParameters(request),
                cancellationToken);
        }

        public IRequestHandlerTracer this[string handlerName] => GetNamedHandlerTracer(handlerName);

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