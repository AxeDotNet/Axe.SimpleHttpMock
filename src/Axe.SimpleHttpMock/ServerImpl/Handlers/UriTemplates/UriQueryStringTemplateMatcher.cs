using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriQueryStringTemplateMatcher
    {
        static readonly char[] queryMark = {'?'};
        readonly Dictionary<string, UriTemplateElement> queryElements;

        public UriQueryStringTemplateMatcher(Uri fakeBaseAddressTemplate)
        {
            string query = fakeBaseAddressTemplate.Query.TrimStart(queryMark);
            queryElements = GetQueryElements(query);
        }

        static IEnumerable<KeyValuePair<string, string>> GetQueryCollection(string query)
        {
            query = query.TrimStart(queryMark);
            if (string.IsNullOrEmpty(query))
            {
                yield break;
            }

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                int equalSignIndex = pair.IndexOf('=');
                if (equalSignIndex > 0)
                {
                    var val = pair.Substring(equalSignIndex + 1);
                    if (!string.IsNullOrEmpty(val))
                    {
                        val = Uri.UnescapeDataString(val);
                    }

                    yield return new KeyValuePair<string, string>(pair.Substring(0, equalSignIndex), val);
                }
            }
        }

        static Dictionary<string, UriTemplateElement> GetQueryElements(string query)
        {
            return GetQueryCollection(query)
                .ToDictionary(item => item.Key, item => new UriTemplateElement(item.Value));
        }
        
        public MatchingResult IsMatch(string queryString)
        {
            Dictionary<string, string> queryStringPairs = GetQueryCollection(queryString)
                .ToDictionary(p => p.Key, p => p.Value);
            if (!ContainsAllNonVariablePairs(queryStringPairs)) { return false; }

            var variables = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> pair in queryStringPairs)
            {
                if (!queryElements.ContainsKey(pair.Key)) { continue; }
                UriTemplateElement element = queryElements[pair.Key];
                if (!element.IsMatch(pair.Value)) { return false; }
                if (element.IsVariable)
                {
                    variables.Add(element.Value, pair.Value);
                }
            }

            return new MatchingResult(true, variables.Concat(GetNotCapturedVariables(variables)));
        }

        IEnumerable<KeyValuePair<string, object>> GetNotCapturedVariables(
            IReadOnlyDictionary<string, object> variables)
        {
            return queryElements.Values
                .Where(v => v.IsVariable && !variables.ContainsKey(v.Value))
                .Select(v => new KeyValuePair<string, object>(v.Value, string.Empty));
        }

        bool ContainsAllNonVariablePairs(IReadOnlyDictionary<string, string> queryStringPairs)
        {
            return queryElements.Where(p => !p.Value.IsVariable)
                .All(p => queryStringPairs.ContainsKey(p.Key));
        }
    }
}