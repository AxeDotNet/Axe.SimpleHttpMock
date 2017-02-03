using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriQueryStringTemplateMatcher
    {
        readonly Dictionary<string, UriTemplateElement> queryElements;

        public UriQueryStringTemplateMatcher(Uri uriWithQueryTemplate)
        {
            queryElements = GetQueryElements(uriWithQueryTemplate.Query);
        }

        static Dictionary<string, UriTemplateElement> GetQueryElements(string queryString)
        {
            return queryString.GetQueryCollection()
                .ToDictionary(item => item.Key, item => new UriTemplateElement(item.Value));
        }
        
        public MatchingResult IsMatch(string queryString)
        {
            Dictionary<string, string> queryStringPairs = queryString.GetQueryCollection()
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