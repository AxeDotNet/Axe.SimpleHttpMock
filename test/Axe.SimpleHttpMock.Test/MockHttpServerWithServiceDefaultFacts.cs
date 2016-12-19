using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class MockHttpServerWithServiceDefaultFacts : HttpServerFactBase
    {
        [Fact]
        public async Task should_change_default_behavior_if_default_exists()
        {
            var server = new MockHttpServer();

            server
                .WithService("http://www.base.com")
                .Default(req => HttpStatusCode.Found.AsResponse());

            HttpClient client = CreateClient(server);

            HttpResponseMessage response = await client.GetAsync("http://www.base.com/any-uri");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        [Fact]
        public async Task should_override_by_another_default_definition()
        {
            var server = new MockHttpServer();

            server
                .WithService("http://www.base.com")
                .Default(req => HttpStatusCode.Found.AsResponse())
                .Default(req => HttpStatusCode.Accepted.AsResponse());

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync("http://www.base.com/any-uri");
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task should_not_confuse_with_default_behavior_of_different_services()
        {
            var server = new MockHttpServer();

            server
                .WithService("http://www.base.com")
                .Default(req => HttpStatusCode.Found.AsResponse())
                .Done()
                .WithService("http://www.another.com")
                .Default(req => HttpStatusCode.NotAcceptable.AsResponse())
                .Done();

            HttpClient client = CreateClient(server);
            HttpResponseMessage responseBase = await client.GetAsync("http://www.base.com");
            HttpResponseMessage responseAnother = await client.GetAsync("http://www.another.com");
            HttpResponseMessage responseNotExist = await client.GetAsync("http://www.not-exist.com");

            Assert.Equal(HttpStatusCode.Found, responseBase.StatusCode);
            Assert.Equal(HttpStatusCode.NotAcceptable, responseAnother.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, responseNotExist.StatusCode);
        }

        [Fact]
        public async Task should_be_override_by_more_specific_api()
        {
            var server = new MockHttpServer();

            server
                .WithService("http://www.base.com")
                .Api("api", HttpStatusCode.OK)
                .Default(req => HttpStatusCode.Found.AsResponse());

            HttpClient client = CreateClient(server);

            HttpResponseMessage responseDefault = await client.GetAsync("http://www.base.com/any-uri");
            HttpResponseMessage responseApi = await client.GetAsync("http://www.base.com/api");

            Assert.Equal(HttpStatusCode.Found, responseDefault.StatusCode);
            Assert.Equal(HttpStatusCode.OK, responseApi.StatusCode);
        }

        [Fact]
        public async Task should_be_traced()
        {
            var server = new MockHttpServer();

            server
                .WithService("http://www.base.com")
                .Api("api", HttpStatusCode.OK)
                .Default(req => HttpStatusCode.Found.AsResponse(), "default");

            HttpClient client = CreateClient(server);

            await client.PostAsJsonAsync("http://www.base.com/not-exist", new {});

            server["default"].VerifyHasBeenCalled();
        }

        [Fact]
        public async Task should_get_expected_response_for_different_default_setup_styles()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            await TestDefaultStyles(s => s.Default(req => expectedStatusCode.AsResponse()), expectedStatusCode);
            await TestDefaultStyles(s => s.Default(expectedStatusCode), expectedStatusCode);
            await TestDefaultStyles(s => s.Default(new {}), expectedStatusCode);
        }

        public async Task TestDefaultStyles(
            Action<WithServiceClause> defaultSetup, 
            HttpStatusCode expectedStatusCode)
        {
            WithServiceClause clause = new MockHttpServer()
                .WithService("http://www.base.com")
                .Api("api", HttpStatusCode.OK);

            defaultSetup(clause);

            MockHttpServer server = clause.Done();
            HttpClient client = CreateClient(server);

            HttpResponseMessage response = await client.GetAsync("http://www.base.com/not-exist");

            Assert.Equal(response.StatusCode, expectedStatusCode);
        }
    }
}