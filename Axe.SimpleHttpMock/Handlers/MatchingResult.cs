using System.Collections.Generic;

namespace Axe.SimpleHttpMock.Handlers
{
    public class MatchingResult
    {
        public bool IsMatch { get; }
        public IDictionary<string, object> Parameters { get; }
        static readonly Dictionary<string, object> emptyDictionary = new Dictionary<string, object>();

        public MatchingResult(bool isMatch, IDictionary<string, object> parameters)
        {
            IsMatch = isMatch;
            Parameters = parameters ?? emptyDictionary;
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