using System.Net.Http;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// This delegate determines if a current handler can handle certain HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request object to check.</param>
    /// <returns>
    /// The checking result. If the handler can handle the HTTP request, It should return
    /// positive result.
    /// </returns>
    public delegate MatchingResult MatchingFunc(HttpRequestMessage request);
}