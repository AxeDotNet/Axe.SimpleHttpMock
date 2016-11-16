using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class MockHttpServerFacts : HttpServerFactBase
    {
        [Fact]
        public async Task should_return_404_if_there_is_no_handler()
        {
            var server = new MockHttpServer();

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://uri.does.not.exist");
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task should_return_desired_message_if_handler_matches()
        {
            var server = new MockHttpServer();

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            server.AddHandler(new DelegatedHandler(
                _ => true,
                (r, p, c) => expectedResponse));
            
            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.matches");
            
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task should_return_404_if_no_matched_handler()
        {
            var server = new MockHttpServer();
            
            server.AddHandler(new DelegatedHandler(
                _ => false,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.OK)));

            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task should_handle_by_first_matched_handler()
        {
            var server = new MockHttpServer();

            const string expectedResponseMessage = "First handler response";
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedResponseMessage)
            };

            server.AddHandler(new DelegatedHandler(
                _ => true,
                (r, p, c) => expectedResponse));
            server.AddHandler(new DelegatedHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.BadRequest)));


            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, response);
        }
    }
}