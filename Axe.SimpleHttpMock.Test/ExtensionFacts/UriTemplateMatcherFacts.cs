using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Axe.SimpleHttpMock.Matchers;
using Xunit;

namespace Axe.SimpleHttpMock.Test.ExtensionFacts
{
    public class UriTemplateMatcherFacts : HttpServerFactBase
    {
        [Theory]
        [InlineData("http://www.baidu.com/user/login")]
        [InlineData("http://WWW.BAIDU.COM/user/login")]
        [InlineData("http://www.baidu.com/user/LOGIN")]
        [InlineData("http://www.baidu.com/user/login?with=queryString")]
        public async void should_match_uri_template(string actualUri)
        {
            var mockHttpServer = new MockHttpServer();
            mockHttpServer
                .When(TheRequest.Is("http://www.baidu.com/user", "/login", "GET"))
                .Response(HttpStatusCode.OK);

            var client = new HttpClient(mockHttpServer);
            HttpResponseMessage response = await client.GetAsync(actualUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("http://www.baidu.com/user/login/extension_not_supported")]
        [InlineData("http://www.baidu.com/")]
        [InlineData("http://www.wtf.com/user/login")]
        [InlineData("https://www.baidu.com/user/login")]
        public async void should_return_not_found_if_not_match(string actualUri)
        {
            var mockHttpServer = new MockHttpServer();
            mockHttpServer
                .When(TheRequest.Is("http://www.baidu.com/user", "login", "GET"))
                .Response(HttpStatusCode.OK);

            var client = new HttpClient(mockHttpServer);
            HttpResponseMessage response = await client.GetAsync(actualUri);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void should_get_parameter_bindings()
        {
            var mockHttpServer = new MockHttpServer();
            IDictionary<string, object> parameters = null;
            mockHttpServer
                .When(TheRequest.Is("http://www.baidu.com/user", "{userId}/{type}?name={name}", "GET"))
                .Response(p =>
                {
                    parameters = p;
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            var client = new HttpClient(mockHttpServer);
            HttpResponseMessage response = await client.GetAsync(
                "http://www.baidu.com/user/12/tax?name=myName&another=whatever");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("12", parameters["userId"]);
            Assert.Equal("tax", parameters["type"]);
            Assert.Equal("myName", parameters["name"]);
        }
    }
}