using System.Collections.Generic;
using System.Linq;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.UriTemplates;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class SimpleUriTemplateFacts
    {
        [Theory]
        [InlineData("/path/subpath/", "http://www.base.address.com:80/App/path/subpath")]
        [InlineData("/path/{variable}", "http://www.base.address.com:80/App/path/subpath")]
        [InlineData("/path/{variable}/subpath", "http://www.base.address.com/app/Path/2/subpath")]
        [InlineData("/path/{variable}/subpath/", "http://www.base.address.com/app/path/2/sUbpath")]
        [InlineData("/path/{variable1}/{variable2}", "http://www.base.address.com/app/path/2/subpath")]
        [InlineData("/path/{variable1}/divider/{variable2}", "http://www.base.address.com/app/path/2/DIVIDER/subpath")]
        public void should_match_simple_path_variable(string template, string matchedUri)
        {
            const string baseAddress = "http://www.base.address.com/app";
            Assert.True(SimpleUriTemplate.IsMatch(baseAddress, template, matchedUri));
        }

        [Theory]
        [InlineData("/path/", "http://www.base.address.com:80/app/path/subpath")]
        [InlineData("/path/{variable}/subpath", "http://www.base.address.com/app/path/subpath")]
        [InlineData("/path/{variable}/subpath/", "http://www.base.address.com/app/path/2/3/subpath")]
        [InlineData("/path/{variable1}/{variable2}", "http://www.base.address.com/app/v/2/subpath")]
        [InlineData("/path/{variable1}/{variable2}", "http://www.base.address.com/app/path/2/subpath/additional")]
        [InlineData("/path/{variable1}/divider/{variable2}", "http://www.base.address.com/app/path/2/divider")]
        public void should_not_match_simple_path_variable(string template, string notMatchedUri)
        {
            const string baseAddress = "http://www.base.address.com/app";
            Assert.False(SimpleUriTemplate.IsMatch(baseAddress, template, notMatchedUri));
        }
        
        [Theory]
        [MemberData("CaptureVariableCases")]
        public void should_capture_path_variables(string template, string matchedUri, IEnumerable<KeyValuePair<string, object>> variables)
        {
            const string baseAddress = "http://www.base.address.com/app";
            MatchingResult result = SimpleUriTemplate.IsMatch(baseAddress, template, matchedUri);
            Assert.True(result);
            Assert.True(variables.All(v => result.Parameters[v.Key].Equals(v.Value)));
        }

        public static IEnumerable<object[]> CaptureVariableCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "/path/{variable}",
                        "http://www.base.address.com:80/app/path/Subpath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable", "Subpath")
                        }
                    },
                    new object[]
                    {
                        "/path/{variable}/subpath",
                        "http://www.base.address.com/app/path/2/subpath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable", "2")
                        }
                    },
                    new object[]
                    {
                        "/path/{variable1}/{variable2}",
                        "http://www.base.address.com/app/path/2/subPath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "2"),
                            new KeyValuePair<string, object>("variable2", "subPath")
                        }
                    },
                    new object[]
                    {
                        "/path/{variable1}/divider/{variable2}",
                        "http://www.base.address.com/app/path/2/divider/Subpath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "2"),
                            new KeyValuePair<string, object>("variable2", "Subpath")
                        }
                    }
                };
            }
        }
    }
}