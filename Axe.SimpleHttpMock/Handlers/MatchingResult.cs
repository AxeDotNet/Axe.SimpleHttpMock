using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.Handlers
{
    public class MatchingResult
    {
        public bool IsMatch { get; }
        public IDictionary<string, object> Parameters { get; }
        static readonly Dictionary<string, object> emptyDictionary = new Dictionary<string, object>();

        public MatchingResult(bool isMatch, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            IsMatch = isMatch;
            Parameters = CreateParameters(parameters);
        }

        IDictionary<string, object> CreateParameters(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null)
            {
                return emptyDictionary;
            }

            return parameters.ToDictionary(o => o.Key, o => o.Value, StringComparer.InvariantCultureIgnoreCase);
        }

        public static implicit operator bool(MatchingResult result)
        {
            return result.IsMatch;
        }

        public static implicit operator MatchingResult(bool value)
        {
            return new MatchingResult(value, emptyDictionary);
        }
    }
}