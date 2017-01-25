using System;
using System.Linq;
using Axe.SimpleHttpMock.ServerImpl.UriTemplates;

namespace Axe.SimpleHttpMock.ServerImpl.Matchers
{
    static class RequestTemplateMatcher
    {
        public static MatchingFunc CreateMatchingDelegate(string uriPrefix, string uriTemplate, params string[] methods)
        {
            return req =>
            {
                if (methods != null && 
                    methods.Length != 0 &&
                    !methods.Any(m => m.Equals(req.Method.Method, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }

                if (!req.RequestUri.AbsoluteUri.StartsWith(uriPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return new UriTemplate(uriTemplate).IsMatch(new Uri(uriPrefix), req.RequestUri);
            };
        }
    }
}