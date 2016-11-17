using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Handlers;
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
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => expectedResponse,
                null));
            
            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.matches");
            
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task should_return_404_if_no_matched_handler()
        {
            var server = new MockHttpServer();
            
            server.AddHandler(new RequestHandler(
                _ => false,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.OK),
                null));

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

            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => expectedResponse,
                null));
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.BadRequest),
                null));
            
            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public void should_be_able_to_get_named_handler()
        {
            var server = new MockHttpServer();

            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server.GetNamedHandlerTracer("handlerName");
            Assert.NotNull(handlerTracer);
            Assert.Throws<KeyNotFoundException>(() => server.GetNamedHandlerTracer("notExist"));
        }

        [Fact]
        public void should_get_correct_state_for_non_called_handler()
        {
            var server = new MockHttpServer();
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server.GetNamedHandlerTracer("handlerName");
            handlerTracer.VerifyNotCalled();
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyHasBeenCalled());
        }

        [Fact]
        public async Task should_get_correct_state_for_called_handler()
        {
            var server = new MockHttpServer();
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server.GetNamedHandlerTracer("handlerName");

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches");

            handlerTracer.VerifyHasBeenCalled();
            handlerTracer.VerifyHasBeenCalled(1);
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyNotCalled());
        }

        [Fact]
        public async Task should_get_request_of_calling_handler()
        {
            var server = new MockHttpServer();
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server.GetNamedHandlerTracer("handlerName");

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches/");

            CallingContext callingContext = handlerTracer.CallingHistories.Single();

            Assert.Equal("http://uri.that.matches/", callingContext.Request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task should_get_binding_parameters_of_calling_handler()
        {
            var server = new MockHttpServer();
            server.AddHandler(new RequestHandler(
                _ => new MatchingResult(true, new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("p1", "v1"),
                    new KeyValuePair<string, object>("p2", "v2")
                }), 
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server.GetNamedHandlerTracer("handlerName");

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches/");

            handlerTracer.VerifyBindedParameter("p1", "v1");
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyBindedParameter("p1", "v2"));
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyBindedParameter("p15", "v2"));
        }
    }
}