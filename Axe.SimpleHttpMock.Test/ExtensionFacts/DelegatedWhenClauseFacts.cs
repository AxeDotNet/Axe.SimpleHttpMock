using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class DelegatedWhenClauseFacts : HttpServerFactBase
    {
        [Fact]
        public async Task should_return_expected_response_if_match()
        {
            var server = new MockHttpServer();
            server.When(_ => true)
                .Response((req, c) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://should.return.ok.com/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task should_get_first_matched_response()
        {
            var server = new MockHttpServer();
            server.When(_ => false)
                .Response((req, c) => req.CreateResponse(HttpStatusCode.OK))
                .When(_ => true)
                .Response((req, c) => req.CreateResponse(HttpStatusCode.NoContent))
                .When(_ => true)
                .Response((req, c) => req.CreateResponse(HttpStatusCode.Continue));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://should.return.ok.com/");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}