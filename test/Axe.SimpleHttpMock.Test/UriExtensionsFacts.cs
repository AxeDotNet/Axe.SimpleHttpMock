using System;
using System.Collections.Generic;
using System.Linq;
using Axe.SimpleHttpMock.ServerImpl.Handlers;
using Xunit;

namespace Axe.SimpleHttpMock.Test
{
    public class UriExtensionsFacts
    {
        [Theory]
        [MemberData(nameof(MatchedBaseAddressCases))]
        public void should_match_base_address(string baseAddress, string actualUri)
        {
            Assert.True(new Uri(baseAddress).IsBaseAddressMatch(new Uri(actualUri)));
        }

        [Theory]
        [MemberData(nameof(NotMatchedBaseAddressCases))]
        public void should_not_match_base_address(string baseAddress, string actualUri)
        {
            Assert.False(new Uri(baseAddress).IsBaseAddressMatch(new Uri(actualUri)));
        }

        [Theory]
        [MemberData(nameof(MatchedBaseAddressSegmentCases))]
        public void should_get_relative_path_segments(string baseAddress, string actualUri, string[] expectedSegments)
        {
            Assert.Equal(expectedSegments, new Uri(baseAddress).GetRelativePathSegments(new Uri(actualUri)));
        }

        [Theory]
        [MemberData(nameof(NotMatchedBaseAddressCases))]
        public void should_get_null_if_path_does_not_match(string baseAddress, string notMatchedUri)
        {
            Assert.Null(new Uri(baseAddress).GetRelativePathSegments(new Uri(notMatchedUri)));
        }

        [Theory]
        [MemberData(nameof(RelativeUriCases))]
        public void should_get_relative_uri(string baseAddress, string actualUri, string expectedRelativeUri)
        {
            Assert.Equal(expectedRelativeUri, new Uri(baseAddress).GetRelativeUri(new Uri(actualUri)));
        }

        [Theory]
        [MemberData(nameof(NotMatchedBaseAddressCases))]
        public void should_get_null_if_uris_do_not_match(string baseAddress, string notMatchedUri)
        {
            Assert.Null(new Uri(baseAddress).GetRelativeUri(new Uri(notMatchedUri)));
        }

        public static IEnumerable<object[]> NotMatchedBaseAddressCases
        {
            get
            {
                return new[]
                {
                    new object[] {"http://www.base.com", "http://www.another.com"},
                    new object[] {"http://www.base.com/not-match", "http://www.base.com/path"},
                    new object[] {"http://www.base.com:80", "http://www.base.com:8080/path/subpath"},
                    new object[] {"http://www.base.com/path/subpath", "http://www.base.com/not-match/path/subpath/third-level"},
                    new object[] {"http://twer:password@www.base.com/path/subpath?does-not=care", "http://twer:not-match@www.base.com/path/subpath/third-level"}
                };
            }
        }

        public static IEnumerable<object[]> MatchedBaseAddressSegmentCases
        {
            get { return matchedCases.Select(c => c.Take(3).ToArray()); }
        }

        public static IEnumerable<object[]> MatchedBaseAddressCases
        {
            get { return matchedCases.Select(c => c.Take(2).ToArray()); }
        }

        public static IEnumerable<object[]> RelativeUriCases
        {
            get { return matchedCases.Select(c => new[] { c[0], c[1], c[3] }); }
        }

        static readonly IEnumerable<object[]> matchedCases = new[]
        {
            new object[] {"http://www.base.com", "http://www.base.com", new string[0], string.Empty},
            new object[] {"http://www.base.com", "http://www.base.com/", new string[0], string.Empty},
            new object[] {"http://www.base.com/", "http://www.base.com", new string[0], string.Empty},
            new object[] {"http://www.base.com/", "http://www.base.com/", new string[0], string.Empty},
            new object[] {"http://www.base.com", "http://www.base.com/path", new[] {"path"}, "path"},
            new object[] {"http://www.base.com", "http://www.base.com/path/subpath", new[] {"path", "subpath"}, "path/subpath"},
            new object[] {"http://www.base.com:80", "http://www.base.com/path/subpath", new[] {"path", "subpath"}, "path/subpath"},
            new object[] {"http://www.base.com:80/", "http://www.base.com/path/subpath", new[] {"path", "subpath"}, "path/subpath"},
            new object[] {"http://www.base.com/path", "http://www.base.com/path/subpath", new[] {"subpath"}, "subpath"},
            new object[] {"http://www.base.com/path/", "http://www.base.com/path/subpath", new[] {"subpath"}, "subpath"},
            new object[] {"http://www.base.com/path/subpath", "http://www.base.com/path/subpath/third-level", new[] {"third-level"}, "third-level"},
            new object[] {"http://www.base.com/path/subpath", "http://www.base.com/path/subpath/?key=value", new string[0], "?key=value"},
            new object[] {"http://www.base.com/path/subpath", "http://www.base.com/path/subpath?key=value", new string[0], "?key=value"},
            new object[] {"http://www.base.com/path/subpath?does-not=care", "http://www.base.com/path/subpath?key=value", new string[0], "?key=value"},
            new object[] {"http://www.base.com/path/subpath?does-not=care", "http://www.base.com/path/subpath/third-level", new[] {"third-level"}, "third-level"},
            new object[] {"http://twer:password@www.base.com/path/subpath?does-not=care", "http://twer:password@www.base.com/path/subpath/third-level", new[] {"third-level"}, "third-level"},
            new object[] {"http://www.base.com/path/subpath", "http://www.base.com/Path/Subpath?key=value", new string[0], "?key=value"},
            new object[] {"http://www.base.com/", "http://www.base.com/Path/Subpath/", new[] {"Path", "Subpath"}, "Path/Subpath/"}
        };
    }
}