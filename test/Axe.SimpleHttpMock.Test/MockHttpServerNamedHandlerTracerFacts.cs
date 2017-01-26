using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.Handlers;
using Newtonsoft.Json;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class MockHttpServerNamedHandlerTracerFacts : HttpServerFactBase
    {
        [Fact]
        public void should_be_able_to_get_named_handler_tracer()
        {
            var server = new MockHttpServer();

            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server["handlerName"];
            Assert.NotNull(handlerTracer);
            Assert.Throws<KeyNotFoundException>(() => server["notExist"]);
        }

        [Fact]
        public void should_be_able_to_verify_not_called_from_named_handler_tracer()
        {
            var server = new MockHttpServer();
            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server["handlerName"];
            handlerTracer.VerifyNotCalled();
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyHasBeenCalled());
        }

        [Fact]
        public async Task should_be_able_to_verify_called_from_named_handler_tracer()
        {
            var server = new MockHttpServer();
            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server["handlerName"];

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches");
            
            handlerTracer.VerifyHasBeenCalled();
            handlerTracer.VerifyHasBeenCalled(1);
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyNotCalled());
        }

        [Fact]
        public async Task should_get_request_from_calling_history_of_named_handler_tracer()
        {
            var server = new MockHttpServer();
            server.AddHandler(new DelegatedRequestHandler(
                _ => true,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server["handlerName"];

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches/");

            CallingHistoryContext callingHistoryContext = handlerTracer.CallingHistories.Single();

            Assert.Equal("http://uri.that.matches/", callingHistoryContext.Request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task should_verify_binded_parameters_from_named_handler_tracer()
        {
            var server = new MockHttpServer();
            server.AddHandler(new DelegatedRequestHandler(
                _ => new MatchingResult(true, new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("p1", "v1"),
                    new KeyValuePair<string, object>("p2", "v2")
                }), 
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                "handlerName"));

            IRequestHandlerTracer handlerTracer = server["handlerName"];

            HttpClient httpClient = CreateClient(server);
            await httpClient.GetAsync("http://uri.that.matches/");

            handlerTracer.VerifyBindedParameter("p1", "v1");
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyBindedParameter("p1", "v2"));
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyBindedParameter("p15", "v2"));
            Assert.Throws<VerifyException>(() => handlerTracer.VerifyBindedParameter("p1", (string)null));
        }
        
        [Fact]
        public async Task should_verify_api_called_or_not_called()
        {
            var httpServer = new MockHttpServer();
            httpServer
                .WithService("http://www.base.com")
                .Api("api1", "GET", HttpStatusCode.OK, "api1")
                .Api("api2", "GET", HttpStatusCode.OK, "api2");

            HttpClient client = CreateClient(httpServer);

            await client.GetAsync("http://www.base.com/api1");

            httpServer["api1"].VerifyHasBeenCalled();
            Assert.Throws<VerifyException>(
                () => httpServer["api1"].VerifyNotCalled());
            httpServer["api2"].VerifyNotCalled();
            Assert.Throws<VerifyException>(
                () => httpServer["api2"].VerifyHasBeenCalled());
        }

        [Fact]
        public async Task should_verify_api_called_times()
        {
            var httpServer = new MockHttpServer();
            httpServer
                .WithService("http://www.base.com")
                .Api("api1", "GET", HttpStatusCode.OK, "api1")
                .Api("api2", "GET", HttpStatusCode.OK, "api2");

            HttpClient client = CreateClient(httpServer);

            await client.GetAsync("http://www.base.com/api1");
            await client.GetAsync("http://www.base.com/api1");

            IRequestHandlerTracer tracer = httpServer["api1"];
            tracer.VerifyHasBeenCalled(2);
            Assert.Throws<VerifyException>(() => tracer.VerifyHasBeenCalled(3));
        }

        [Fact]
        public async void should_get_request_content_from_calling_history()
        {
            var server = new MockHttpServer();
            server.WithService("http://www.base.com")
                .Api("login", "POST", HttpStatusCode.OK, "login");

            var client = new HttpClient(server);

            HttpResponseMessage response = await client.PostAsync(
                "http://www.base.com/login",
                new { username = "n", password = "p" },
                new JsonMediaTypeFormatter());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actualRequestContent = await server["login"].SingleOrDefaultRequestContentAsync(
                new { username = string.Empty, password = string.Empty });

            Assert.Equal("n", actualRequestContent.username);
            Assert.Equal("p", actualRequestContent.password);
        }

        [Fact]
        public async void should_get_request_conent_from_calling_history_even_if_original_request_has_been_disposed()
        {
            var server = new MockHttpServer();
            server.WithService("http://www.base.com")
                .Api("login", "POST", HttpStatusCode.OK, "login");

            var client = new HttpClient(server);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://www.base.com/login")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { username = "n", password = "p" }), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);
            response.Dispose();
            request.Dispose();

            var actualRequestContent = await server["login"].SingleOrDefaultRequestContentAsync(
                new { username = string.Empty, password = string.Empty });

            Assert.Equal("n", actualRequestContent.username);
            Assert.Equal("p", actualRequestContent.password);
        }
    }
}