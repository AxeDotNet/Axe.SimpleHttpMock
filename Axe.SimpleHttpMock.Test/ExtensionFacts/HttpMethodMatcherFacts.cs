using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Matchers;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class HttpMethodMatcherFacts : HttpServerFactBase
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("get")]
        [InlineData("Get")]
        public async Task should_match_http_method(string matchingMethod)
        {
            var server = new MockHttpServer();
            server
                .When(req => req.IsMethod(matchingMethod))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://it.should.match");
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task should_return_404_if_method_not_match()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.IsMethod("post"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://it.should.match");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task should_match_multiple_method()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.IsMethod("get", "post"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response =
                await client.PostAsync("http://it.should.match", new {}, new JsonMediaTypeFormatter());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}