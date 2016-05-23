using System;
using System.Linq;
using System.Net.Http;

namespace Axe.SimpleHttpMock.Matchers
{
    public static class HttpMethodMatcherExtensions
    {
        public static bool IsMethod(this HttpRequestMessage request, params string[] methods)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (methods == null || methods.Length == 0) { return false; }
            if (request.Method == null) { return false; }

            return methods.Contains(request.Method.Method, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}