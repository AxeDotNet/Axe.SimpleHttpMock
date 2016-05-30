using System;
using System.Net.Http;

namespace Axe.SimpleHttpMock.Matchers
{
    public static class HttpPathMatcherExtensions
    {
        public static bool PathIs(
            this HttpRequestMessage request,
            string absolutePath,
            StringCompareMethod compareMethod = StringCompareMethod.CaseSensitive)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (absolutePath == null) { throw new ArgumentNullException(nameof(absolutePath)); }

            StringComparison comparer = compareMethod == StringCompareMethod.CaseSensitive
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase;
            if (!absolutePath.StartsWith("/", comparer))
            {
                throw new FormatException("The absolute path should start with \"/\"");
            }

            Uri requestUri = request.RequestUri;
            if (requestUri == null) { return false; }
            string requestPath = requestUri.AbsolutePath;
            return requestPath.Equals(absolutePath);
        }
    }
}