using System;
using System.Linq;
using System.Net.Http;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    static class HttpMethodExtensions
    {
        public static bool IsMethodMatch(this HttpRequestMessage requestMessage, string[] methods)
        {
            bool ignoringMethods = methods == null || methods.Length == 0;
            if (ignoringMethods) { return true; }
            return methods.Any(m => m.Equals(requestMessage.Method.Method, StringComparison.OrdinalIgnoreCase));
        }
    }
}