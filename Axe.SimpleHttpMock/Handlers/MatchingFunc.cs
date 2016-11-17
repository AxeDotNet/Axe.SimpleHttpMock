using System.Net.Http;

namespace Axe.SimpleHttpMock.Handlers
{
    public delegate MatchingResult MatchingFunc(HttpRequestMessage request);
}