using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> m_handleFunc;
        readonly List<Func<HttpRequestMessage, bool>> m_matchers;

        static readonly List<Func<HttpRequestMessage, bool>> EmptyMatches =
            new List<Func<HttpRequestMessage, bool>>();

        internal RequestHandler(IEnumerable<Func<HttpRequestMessage, bool>> matchers,
            Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handleFunc)
        {
            if (handleFunc == null)
            {
                throw new ArgumentNullException(nameof(handleFunc));
            }

            m_handleFunc = handleFunc;
            m_matchers = matchers == null
                ? EmptyMatches
                : new List<Func<HttpRequestMessage, bool>>(matchers);
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matchers.All(isMatch => isMatch(request));
        }

        public HttpResponseMessage Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return m_handleFunc(request, cancellationToken);
        }
    }
}