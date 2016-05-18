using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock
{
    public interface IRequestHandler
    {
        bool IsMatch(HttpRequestMessage request);
        HttpResponseMessage Handle(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}