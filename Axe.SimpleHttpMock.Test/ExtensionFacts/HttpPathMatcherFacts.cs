using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Matchers;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class HttpPathMatcherFacts : HttpServerFactBase
    {
        [Fact]
        public async Task should_use_absolute_path()
        {
            var server = new MockHttpServer();

            server.When(req => req.PathIs("user/login"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);

            await Assert.ThrowsAsync<FormatException>(
                async () => await client.GetAsync("http://it.should.match/user/login"));
        }

        [Fact]
        public async Task should_match_http_path()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.PathIs("/user/login"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(
                "http://it.should.match/user/login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task should_match_http_path_case_sensitively()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.PathIs("/user/Login"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(
                "http://it.should.match/user/login");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task should_match_http_path_case_insensitive_if_specified()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.PathIs("/user/Login", StringCompareMethod.CaseInsensitive))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(
                "http://it.should.match/user/login");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task should_not_contains_query_string()
        {
            var server = new MockHttpServer();
            server
                .When(req => req.PathIs("/user/login"))
                .Response((req, q) => req.CreateResponse(HttpStatusCode.OK));

            HttpClient client = CreateClient(server);
            HttpResponseMessage response = await client.GetAsync(
                "http://it.should.match/user/login?userId=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}