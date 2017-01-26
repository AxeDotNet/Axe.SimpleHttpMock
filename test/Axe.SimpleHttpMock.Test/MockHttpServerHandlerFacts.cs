using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.Handlers;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class MockHttpServerHandlerFacts : HttpServerFactBase
    {
        [Fact]
        public async Task should_return_404_if_there_is_no_handler()
        {
            var server = new MockHttpServer();

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://uri.does.not.exist");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(response.Content is StringContent);
        }

        [Fact]
        public async Task should_return_404_if_no_matched_handler()
        {
            var server = new MockHttpServer();

            server.AddHandler(new DelegatedRequestHandler(
                _ => false,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.OK),
                null));

            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void should_throw_if_service_address_is_not_an_absolute_uri()
        {
            var server = new MockHttpServer();

            Assert.Throws<UriFormatException>(() => server.WithService("/uri"));
        }

        [Fact]
        public async Task should_return_desired_message_if_handler_matches()
        {
            var server = new MockHttpServer();

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => expectedResponse,
                null));

            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.matches");

            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task should_handle_by_latest_matched_handler()
        {
            var server = new MockHttpServer();

            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.OK),
                null));
            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.BadRequest),
                null));

            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}