using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Matchers;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class HttpAuthorityMatcherFacts : HttpServerFactBase
    {
        [Theory]
        [InlineData("http://www.baidu.com/user/login", "www.baidu.com")]
        [InlineData("http://www.baidu.com:8080/user/login", "www.baidu.com:8080")]
        [InlineData("http://user:pass@localhost:1080/user/login", "localhost:1080")]
        public async Task should_match_authority(string requestUri, string authority)
        {
            var server = new MockHttpServer();

            server.When(req => req.AuthorityIs(authority)).Response(HttpStatusCode.OK);

            HttpClient client = CreateClient(server);

            HttpResponseMessage response = await client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}