using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        readonly HandlerFunc m_handleFunc;
        readonly MatchingFunc m_matcher;
        readonly List<CallingContext> m_callingHistories = new List<CallingContext>(); 

        internal RequestHandler(MatchingFunc matcher, HandlerFunc handleFunc, string name = null)
        {
            if (handleFunc == null)
            {
                throw new ArgumentNullException(nameof(handleFunc));
            }

            if (matcher == null)
            {
                throw new ArgumentNullException(nameof(matcher));
            }

            m_handleFunc = handleFunc;
            m_matcher = matcher;
            Name = name;
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            return m_matcher(request);
        }

        public IDictionary<string, object> GetParameters(HttpRequestMessage request)
        {
            return m_matcher(request).Parameters;
        }

        public HttpResponseMessage Handle(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            m_callingHistories.Add(new CallingContext(request, parameters));
            return m_handleFunc(request, parameters, cancellationToken);
        }

        public string Name { get; }

        public IReadOnlyCollection<CallingContext> CallingHistories => m_callingHistories.AsReadOnly();
    }
}