using System.Net;
using System.Net.Http;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.Handlers;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class RegexRequestHandlerFacts
    {
        const string baseAddress = "http://www.base.com/app";

        [Theory]
        [InlineData("path", "^path$")]
        [InlineData("path/1", "[0-9]")]
        [InlineData("path/1", "path")]
        public void should_match_regex_for_relative_uri(string relativePath, string regex)
        {
            RegexRequestHandler handler = CreateHandler(regex);
            Assert.True(handler.IsMatch(CreateRequestWithUri(relativePath)));
        }

        [Theory]
        [InlineData("path", "^path/1$")]
        [InlineData("path/1", "v")]
        [InlineData("path/1", "tv")]
        public void should_not_match_regex_for_relative_uri(string relativePath, string regex)
        {
            var handler = CreateHandler(regex);
            Assert.False(handler.IsMatch(CreateRequestWithUri(relativePath)));
        }

        [Fact]
        public void should_captured_named_variables()
        {
            RegexRequestHandler handler = CreateHandler("^path/(?<variable_name>[^/]+)");
            MatchingResult result = handler.IsMatch(CreateRequestWithUri("path/good/bye"));
            Assert.Equal("good", result.Parameters["variable_name"]);
        }

        static HttpRequestMessage CreateRequestWithUri(string relativePath)
        {
            return new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}/{relativePath}");
        }

        static RegexRequestHandler CreateHandler(string regex)
        {
            var handler = new RegexRequestHandler(
                baseAddress,
                regex,
                null,
                (r, p, c) => HttpStatusCode.OK.AsResponse(),
                null);
            return handler;
        }
    }
}