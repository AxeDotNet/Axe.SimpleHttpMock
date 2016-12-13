using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.ServerImpl
{
    public delegate HttpResponseMessage RequestHandlingFunc(
        HttpRequestMessage request,
        IDictionary<string, object> bindedParameters,
        CancellationToken cancellationToken);
}