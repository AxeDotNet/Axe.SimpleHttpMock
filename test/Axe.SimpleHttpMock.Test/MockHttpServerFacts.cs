using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Handlers;
using Newtonsoft.Json;
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
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => expectedResponse,
                null));
            
            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.matches");
            
            Assert.Equal(expectedResponse, response);
        }

        [Theory]
        [InlineData("http://www.base.com", "", "http://www.base.com")]
        [InlineData("http://www.base.com/", "", "http://www.base.com")]
        [InlineData("http://www.base.com", "", "http://www.base.com/")]
        [InlineData("http://www.base.com", "user", "http://www.base.com/user")]
        [InlineData("http://www.base.com", "user/", "http://www.base.com/user/")]
        [InlineData("http://www.base.com", "user", "http://www.base.com/user?query=string")]
        [InlineData("http://www.base.com", "user/", "http://www.base.com/user/?query=string")]
        [InlineData("http://www.base.com", "user/{userId}", "http://www.base.com/user/1.htm?query=string")]
        [InlineData("http://www.base.com", "user/{userId}/session", "http://www.base.com/user/1.htm/session?query=string")]
        [InlineData("http://www.base.com", "user?query=string", "http://www.base.com/user?query=string")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user?query=whatever")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user?query=")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user?query=&other=value")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user?other=value&query=")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user?other=value")]
        public async Task should_return_desired_message_if_handler_matches_for_uri(
            string serviceUri, string route, string requestUri)
        {
            var server = new MockHttpServer();
            var content = new {name = "hello"};
            server.WithService(serviceUri).Api(route, "GET", content);

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(requestUri);

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
            var actualContent = JsonConvert.DeserializeAnonymousType(
                await response.Content.ReadAsStringAsync(),
                content);
            Assert.Equal("hello", actualContent.name);
        }

        [Theory]
        [InlineData("", "GET")]
        [InlineData("", "PUT")]
        [InlineData("", "POST")]
        [InlineData("", "DELETE")]
        [InlineData("GET", "GET")]
        [InlineData("GET,PUT", "GET")]
        [InlineData("GET,PUT", "PUT")]
        public async Task should_return_desired_message_if_http_method_is_matched(
            string httpMethodConfig, string actualMethod)
        {
            string[] httpMethods = httpMethodConfig.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (httpMethods.Length == 0) { httpMethods = null; }

            var server = new MockHttpServer();
            server.WithService("http://www.base.com").Api("user", httpMethods, HttpStatusCode.OK);

            HttpRequestMessage request = new HttpRequestMessage(
                new HttpMethod(actualMethod), "http://www.base.com/user");
            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
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

        [Theory]
        [InlineData("http://www.base.com", "", "http://www.base.com/user")]
        [InlineData("http://www.base.com", "user", "http://www.base.com/user/")]
        [InlineData("http://www.base.com", "user/", "http://www.base.com/user")]
        [InlineData("http://www.base.com", "user", "http://www.base.com/user/?query=string")]
        [InlineData("http://www.base.com", "user/", "http://www.base.com/user?query=string")]
        [InlineData("http://www.base.com", "user/{userId}", "http://www.base.com/user/1.htm/?query=string")]
        [InlineData("http://www.base.com", "user/{userId}/session", "http://www.base.com/user/1/2/session?query=string")]
        [InlineData("http://www.base.com", "user?query=string", "http://www.base.com/user?parameter=string")]
        [InlineData("http://www.base.com", "user?query={value}", "http://www.base.com/user/?query=value")]
        public async Task should_return_404_if_no_handler_matches_for_uri(
            string serviceUri, string route, string requestUri)
        {
            var server = new MockHttpServer();
            server.WithService(serviceUri).Api(route, "GET", HttpStatusCode.OK);

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(requestUri);

            Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task should_handle_by_latest_matched_handler()
        {
            var server = new MockHttpServer();
            
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.OK), 
                null));
            server.AddHandler(new RequestHandler(
                _ => true,
                (r, p, c) => new HttpResponseMessage(HttpStatusCode.BadRequest),
                null));
            
            HttpClient httpClient = CreateClient(server);
            HttpResponseMessage response = await httpClient.GetAsync("http://uri.that.not.matches");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task should_handle_by_first_matched_uri()
        {
            var server = new MockHttpServer();
            server.WithService("http://www.base.com/user")
                .Api("account", HttpStatusCode.OK, "route 1")
                .Api("account", "GET", HttpStatusCode.OK, "route 2");

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://www.base.com/user/account");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            server.GetNamedHandlerTracer("route 2").VerifyHasBeenCalled();
            server.GetNamedHandlerTracer("route 1").VerifyNotCalled();
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

        [Fact]
        public async Task should_get_binding_parameters_of_uri()
        {
            var server = new MockHttpServer();
            server
                .WithService("http://www.base.com")
                .Api(
                    "user/{userId}/session/{sessionId}?p1={value1}&p2={value2}",
                    "GET",
                    _ => new {Parameter = _ }.AsResponse());

            HttpClient client = CreateClient(server);

            HttpResponseMessage response = await client.GetAsync(
                "http://www.base.com/user/12/session/28000?p1=v1&p2=v2");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string jsonString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeAnonymousType(
                jsonString,
                new {Parameter = default(Dictionary<string, object>)});

            Assert.Equal("12", content.Parameter["USERID"]);
            Assert.Equal("28000", content.Parameter["SESSIONID"]);
            Assert.Equal("v1", content.Parameter["VALUE1"]);
            Assert.Equal("v2", content.Parameter["VALUE2"]);
        }

        [Fact]
        public async Task should_get_optional_query_parameters_of_uri()
        {
            var server = new MockHttpServer();
            server
                .WithService("http://www.base.com")
                .Api(
                    "user/{userId}/session/{sessionId}?p1={value1}&p2={value2}",
                    "GET",
                    _ => new { Parameter = _ }.AsResponse());

            HttpClient client = CreateClient(server);

            HttpResponseMessage response = await client.GetAsync(
                "http://www.base.com/user/12/session/28000?p2=v2");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string jsonString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeAnonymousType(
                jsonString,
                new { Parameter = default(Dictionary<string, object>) });
            
            Assert.Equal(null, content.Parameter["VALUE1"]);
            Assert.Equal("v2", content.Parameter["VALUE2"]);
        }

        [Fact]
        public async Task should_verify_api_called()
        {
            var httpServer = new MockHttpServer();
            httpServer
                .WithService("http://www.base.com")
                .Api("api1", "GET", HttpStatusCode.OK, "api1")
                .Api("api2", "GET", HttpStatusCode.OK, "api2");

            HttpClient client = CreateClient(httpServer);

            await client.GetAsync("http://www.base.com/api1");

            httpServer.GetNamedHandlerTracer("api1").VerifyHasBeenCalled();
            Assert.Throws<VerifyException>(
                () => httpServer.GetNamedHandlerTracer("api1").VerifyNotCalled());
            httpServer.GetNamedHandlerTracer("api2").VerifyNotCalled();
            Assert.Throws<VerifyException>(
                () => httpServer.GetNamedHandlerTracer("api2").VerifyHasBeenCalled());
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

            IRequestHandlerTracer tracer = httpServer.GetNamedHandlerTracer("api1");
            tracer.VerifyHasBeenCalled(2);
            Assert.Throws<VerifyException>(() => tracer.VerifyHasBeenCalled(3));
        }

        [Fact]
        public async void should_get_request_conent_even_if_original_request_has_been_disposed()
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

            await server.GetNamedHandlerTracer("login").VerifyAnonymousRequestContentAsync(
                new { username = string.Empty, password = string.Empty },
                c => c.username == "n" && c.password == "p");
        }

        [Fact]
        public async void should_verify_request_content()
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
            await server.GetNamedHandlerTracer("login").VerifyAnonymousRequestContentAsync(
                new { username = string.Empty, password = string.Empty },
                c => c.username == "n" && c.password == "p");
        }
    }
}