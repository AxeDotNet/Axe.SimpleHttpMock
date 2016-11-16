using System;
using System.Net.Http;

namespace Axe.SimpleHttpMock.Matchers
{
    public static class HttpAuthorityMatcherExtensions
    {
        public static bool AuthorityIs(
            this HttpRequestMessage request,
            string authority,
            StringCompareMethod compareMethod = StringCompareMethod.CaseInsensitive)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (authority == null) { throw new ArgumentNullException(nameof(authority)); }

            StringComparison comparer = compareMethod == StringCompareMethod.CaseSensitive
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase;
            
            Uri requestUri = request.RequestUri;
            if (requestUri == null) { return false; }
            return requestUri.Authority.Equals(authority, comparer);
        }
    }
}