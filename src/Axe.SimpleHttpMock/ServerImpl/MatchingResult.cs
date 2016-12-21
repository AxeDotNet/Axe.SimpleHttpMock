using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl
{
    /// <summary>
    /// Represents the result which determines that if an <see cref="IRequestHandler"/>
    /// instance can handle certain HTTP request message.
    /// </summary>
    public class MatchingResult
    {
        /// <summary>
        /// Get if current <see cref="IRequestHandler"/> instance can handle certain
        /// HTTP request message. If it can handle the message, returns <c>true</c>,
        /// or else returns <c>false</c>.
        /// </summary>
        public bool IsMatch { get; }

        /// <summary>
        /// Get binded parameters against the request.
        /// </summary>
        /// <remarks>
        /// We will not define how the parameters are binded and which part of the request
        /// will be binded as parameter. It is implementation specific against certain
        /// <see cref="MatchingFunc"/>.
        /// </remarks>
        public IDictionary<string, object> Parameters { get; }

        static readonly Dictionary<string, object> EmptyDictionary =
            new Dictionary<string, object>();

        /// <summary>
        /// Create a new <see cref="MatchingResult"/> instance.
        /// </summary>
        /// <param name="isMatch">
        /// <c>true</c> if current handler can handle the request. Otherwise, <c>false</c>.
        /// </param>
        /// <param name="parameters">
        /// The binded parametes agains the request.
        /// </param>
        public MatchingResult(bool isMatch, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            IsMatch = isMatch;
            Parameters = CreateParameters(parameters);
        }

        IDictionary<string, object> CreateParameters(
            IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null)
            {
                return EmptyDictionary;
            }

            return parameters.ToDictionary(
                o => o.Key,
                o => o.Value,
                StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Implicitly convert <see cref="MatchingResult"/> instance to <see cref="bool"/>.
        /// This can ease the judgement process.
        /// </summary>
        /// <param name="result">
        /// The <see cref="MatchingResult"/> instance to convert.
        /// </param>
        public static implicit operator bool(MatchingResult result)
        {
            return result.IsMatch;
        }

        /// <summary>
        /// Create a <see cref="MatchingResult"/> instance without binded parameters.
        /// </summary>
        /// <param name="value">
        /// <c>true</c>, if current <see cref="IRequestHandler"/> can handle the request.
        /// Otherwise, <c>false</c>.
        /// </param>
        public static implicit operator MatchingResult(bool value)
        {
            return new MatchingResult(value, EmptyDictionary);
        }
    }
}