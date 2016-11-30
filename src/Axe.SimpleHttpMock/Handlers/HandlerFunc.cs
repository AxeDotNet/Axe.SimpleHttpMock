using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Handlers
{
    public delegate HttpResponseMessage HandlerFunc(
        HttpRequestMessage request,
        IDictionary<string, object> bindedParameters,
        CancellationToken cancellationToken);
}