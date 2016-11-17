using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock.Matchers
{
    public static class TheRequest
    {
        public static MatchingFunc Is(string uriPrefix, string uriTemplate, params string[] methods)
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

                UriTemplateMatch match = new UriTemplate(uriTemplate).Match(new Uri(uriPrefix), req.RequestUri);
                if (match == null)
                {
                    return false;
                }

                var parameters = new List<KeyValuePair<string, object>>();
                Copy(parameters, match.BoundVariables);
                return new MatchingResult(true, parameters);
            };
        }

        static void Copy(ICollection<KeyValuePair<string, object>> target, NameValueCollection collection)
        {
            foreach (string k in collection.AllKeys)
            {
                target.Add(new KeyValuePair<string, object>(k, collection[k]));
            }
        }
    }
}