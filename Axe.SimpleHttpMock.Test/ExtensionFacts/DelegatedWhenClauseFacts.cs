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
            new WhenClause(server, _ => true, null).Response(HttpStatusCode.OK);

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://should.return.ok.com/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task should_get_first_matched_response()
        {
            var server = new MockHttpServer();
            new WhenClause(server, _ => false, null).Response(HttpStatusCode.OK);
            new WhenClause(server, _ => true, null).Response(HttpStatusCode.NoContent);
            new WhenClause(server, _ => true, null).Response(HttpStatusCode.Continue);

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://should.return.ok.com/");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}