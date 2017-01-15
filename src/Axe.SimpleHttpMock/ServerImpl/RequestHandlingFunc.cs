using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// This delegate defines the customized delegate that accepts request and binded
    /// parameters, and generate desired response.
    /// </summary>
    /// <param name="request">The request message received.</param>
    /// <param name="bindedParameters">The binded parameters from the request. This is implementation specific.</param>
    /// <param name="cancellationToken">The cancellation token passed to the async method.</param>
    /// <returns>The generated response message.</returns>
    public delegate HttpResponseMessage RequestHandlingFunc(
        HttpRequestMessage request,
        IDictionary<string, object> bindedParameters,
        CancellationToken cancellationToken);
}