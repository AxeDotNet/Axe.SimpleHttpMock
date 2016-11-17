using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    public interface IRequestHandler : IRequestHandlerTracer
    {
        bool IsMatch(HttpRequestMessage request);

        IDictionary<string, object> GetParameters(HttpRequestMessage request);

        Task<HttpResponseMessage> HandleAsync(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken);
    }
}