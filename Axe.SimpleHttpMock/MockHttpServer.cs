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
        readonly Dictionary<string, IRequestHandler> m_namedHandlers = new Dictionary<string, IRequestHandler>(); 

        public void AddHandler(IRequestHandler handler)
        {
            m_handlers.Add(handler);
            if (!string.IsNullOrEmpty(handler.Name))
            {
                m_namedHandlers.Add(handler.Name, handler);
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            IRequestHandler matchedHandler = m_handlers.FirstOrDefault(m => m.IsMatch(request));
            if (matchedHandler == null)
            {
                return Task.Factory.StartNew(
                    () => new HttpResponseMessage(HttpStatusCode.NotFound), cancellationToken);
            }

            return Task.Factory.StartNew(
                () => matchedHandler.Handle(request, matchedHandler.GetParameters(request), cancellationToken),
                cancellationToken);
        }

        public IRequestHandlerTracer GetNamedHandlerTracer(string name)
        {
            return m_namedHandlers[name];
        }
    }
}