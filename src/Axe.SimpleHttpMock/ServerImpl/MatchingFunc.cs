using System.Net.Http;

namespace Axe.SimpleHttpMock.ServerImpl
{
    public delegate MatchingResult MatchingFunc(HttpRequestMessage request);
}