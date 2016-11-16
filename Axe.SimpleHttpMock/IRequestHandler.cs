using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock
{
    public interface IRequestHandler
    {
        bool IsMatch(HttpRequestMessage request);
        IDictionary<string, object> GetParameters(HttpRequestMessage request);
        HttpResponseMessage Handle(HttpRequestMessage request, IDictionary<string, object> parameters, CancellationToken cancellationToken);
    }
}