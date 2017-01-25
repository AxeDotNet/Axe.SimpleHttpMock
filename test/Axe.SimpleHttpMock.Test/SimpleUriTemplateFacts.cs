using System;
using System.Collections.Generic;
using System.Linq;
using Axe.SimpleHttpMock.ServerImpl;
using Axe.SimpleHttpMock.ServerImpl.UriTemplates;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class SimpleUriTemplateFacts
    {
        const string baseAddress = "http://www.base.address.com/app";

        [Theory]
        [InlineData("/", "http://www.base.address.com:80/App")]
        [InlineData("", "http://www.base.address.com:80/App")]
        [InlineData("/path/subpath/", baseAddress + "/path/subpath")]
        [InlineData("/path/{variable}", "http://www.base.address.com:80/App/path/subpath")]
        [InlineData("/path/{variable}/subpath", baseAddress + "/Path/2/subpath")]
        [InlineData("/path/{variable}/subpath/", baseAddress + "/path/2/sUbpath")]
        [InlineData("/path/{variable1}/{variable2}", baseAddress + "/path/2/subpath")]
        [InlineData("/path/{variable1}/divider/{variable2}", baseAddress + "/path/2/DIVIDER/subpath")]
        public void should_match_simple_path_variable(string template, string matchedUri)
        {
            Assert.True(new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(matchedUri)));
        }

        [Theory]
        [InlineData("/path/", "http://www.base.address.com:80/app/path/subpath")]
        [InlineData("/path/{variable}/subpath", baseAddress + "/path/subpath")]
        [InlineData("/path/{variable}/subpath/", baseAddress + "/path/2/3/subpath")]
        [InlineData("/path/{variable1}/{variable2}", baseAddress + "/v/2/subpath")]
        [InlineData("/path/{variable1}/{variable2}", baseAddress + "/path/2/subpath/additional")]
        [InlineData("/path/{variable1}/divider/{variable2}", baseAddress + "/path/2/divider")]
        public void should_not_match_simple_path_variable(string template, string notMatchedUri)
        {
            Assert.False(new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(notMatchedUri)));
        }

        [Fact]
        public void should_not_collapse_path_segment()
        {
            Assert.False(new UriTemplate("/path").IsMatch(
                new Uri(baseAddress),
                new Uri("http://www.base.address.com/path")));
        }

        [Theory]
        [InlineData("path?name=value", baseAddress + "/path?name=value")]
        [InlineData("path?name1=value1&name2=value2", baseAddress + "/path?name1=value1&name2=value2")]
        [InlineData("path?name2=value2&name1=value1", baseAddress + "/path?name1=value1&name2=value2")]
        public void should_match_explicitly_specified_query_strings(string template, string matchedUri)
        {
            Assert.True(new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(matchedUri)));
        }

        [Theory]
        [InlineData("path", baseAddress + "/path?name=value")]
        [InlineData("path", baseAddress + "/path?name1=value1&name2=value2")]
        [InlineData("path?name1=value1", baseAddress + "/path?name1=value1&name2=value2")]
        public void should_match_if_template_does_not_explicitly_specify_query_strings(string template, string matchedUri)
        {
            Assert.True(new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(matchedUri)));
        }

        [Theory]
        [InlineData("path?name=value", baseAddress + "/path")]
        [InlineData("path?name=value&name1={variableName1}", baseAddress + "/path")]
        public void should_not_match_query_strings(string template, string notMatchedUri)
        {
            Assert.False(new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(notMatchedUri)));
        }

        [Theory]
        [MemberData("CaptureVariableCases")]
        public void should_capture_path_variables(string template, string matchedUri, IEnumerable<KeyValuePair<string, object>> variables)
        {
            MatchingResult result = new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(matchedUri));
            Assert.True(result);
            Assert.True(variables.All(v => result.Parameters[v.Key].Equals(v.Value)));
        }

        [Theory]
        [MemberData("CaptureQueryStringVariableCases")]
        public void should_capture_query_string_variables(string template, string matchedUri, IEnumerable<KeyValuePair<string, object>> variables)
        {
            MatchingResult result = new UriTemplate(template).IsMatch(new Uri(baseAddress), new Uri(matchedUri));
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
                        baseAddress + "/path/2/subpath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable", "2")
                        }
                    },
                    new object[]
                    {
                        "/path/{variable1}/{variable2}",
                        baseAddress + "/path/2/subPath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "2"),
                            new KeyValuePair<string, object>("variable2", "subPath")
                        }
                    },
                    new object[]
                    {
                        "/path/{variable1}/divider/{variable2}",
                        baseAddress + "/path/2/divider/Subpath",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "2"),
                            new KeyValuePair<string, object>("variable2", "Subpath")
                        }
                    }
                };
            }
        }

        public static IEnumerable<object[]> CaptureQueryStringVariableCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "path?name1={variable1}",
                        baseAddress + "/path?name1=v1",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "v1")
                        }
                    },
                    new object[]
                    {
                        "path?name1={variable1}&name2={variable2}",
                        baseAddress + "/path?name1=v1&name2=v2",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "v1"),
                            new KeyValuePair<string, object>("variable2", "v2")
                        }
                    },
                    new object[]
                    {
                        "path?name2={variable2}&name1={variable1}",
                        baseAddress + "/path?name1=v1&name2=v2",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "v1"),
                            new KeyValuePair<string, object>("variable2", "v2")
                        }
                    },
                    new object[]
                    {
                        "path?name1={variable1}&name2={variable2}",
                        baseAddress + "/path?name1=v1",
                        new[]
                        {
                            new KeyValuePair<string, object>("variable1", "v1"),
                            new KeyValuePair<string, object>("variable2", string.Empty)
                        }
                    },
                    new object[]
                    {
                        "path?{this_is_not_variable}={this_is_variable}",
                        baseAddress + "/path?{this_is_not_variable}=v1",
                        new[]
                        {
                            new KeyValuePair<string, object>("this_is_variable", "v1")
                        }
                    }
                };
            }
        } 
    }
}