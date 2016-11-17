using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class WithServiceFacts : HttpServerFactBase
    {
        [Fact]
        public async void should_get_service_defined_api_result()
        {
            var httpServer = new MockHttpServer();
            httpServer
                .WithService("http://www.baidu.com")
                .Api("user/login", _ => HttpStatusCode.OK.AsResponse(), "1")
                .Api("user/login/{userId}", _ => HttpStatusCode.Accepted.AsResponse(), "2")
                .Api("another", _ => HttpStatusCode.Ambiguous.AsResponse(), "3")
                .Done()
                .WithService("http://www.sina.com")
                .Api("user/login", _ => HttpStatusCode.Continue.AsResponse(), "4")
                .Done();

            var client = new HttpClient(httpServer);

            await AssertGetStatusCode(client, "http://www.baidu.com/user/login", HttpStatusCode.OK);
            await AssertGetStatusCode(client, "http://www.baidu.com/user/login/2", HttpStatusCode.Accepted);
            await AssertGetStatusCode(client, "http://www.baidu.com/another", HttpStatusCode.Ambiguous);
            await AssertGetStatusCode(client, "http://www.sina.com/user/login", HttpStatusCode.Continue);

            AssertCalled(httpServer, "1");
            AssertCalled(httpServer, "2");
            AssertCalled(httpServer, "3");
            AssertCalled(httpServer, "4");
        }

        [Fact]
        public async void should_verify_request_content()
        {
            var server = new MockHttpServer();
            server.WithService("http://www.baidu.com")
                .Api("login", "POST",
                    (p, r) =>
                    {
                        // read all the content to move stream position to end.
                        // so that we can test if cloned request position is at
                        // the starting point.
                        p.Content.ReadAsStringAsync().Wait();
                        return HttpStatusCode.OK.AsResponse();
                    }, "login");

            var client = new HttpClient(server);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://www.baidu.com/login")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { username = "n", password = "p" }), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);

            request.Dispose();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            await server.GetNamedHandlerTracer("login").VerifyAnonymousRequestContentAsync(
                new {username = string.Empty, password = string.Empty},
                c => c.username == "n" && c.password == "p");
        }

        static void AssertCalled(MockHttpServer httpServer, string handlerName)
        {
            IRequestHandlerTracer tracer = httpServer.GetNamedHandlerTracer(handlerName);
            tracer.VerifyHasBeenCalled();
        }

        // ReSharper disable once UnusedParameter.Local
        static async Task AssertGetStatusCode(HttpClient client, string uri, HttpStatusCode expectedStatusCode)
        {
            HttpResponseMessage responseOK = await client.GetAsync(uri);
            Assert.Equal(expectedStatusCode, responseOK.StatusCode);
        }
    }
}