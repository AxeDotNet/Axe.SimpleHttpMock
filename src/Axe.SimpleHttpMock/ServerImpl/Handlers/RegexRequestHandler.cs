using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Axe.SimpleHttpMock.Migration;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    class RegexRequestHandler : RequestHandlerBase
    {
        readonly string[] m_methods;
        readonly RequestHandlingFunc handlingFunc;
        readonly Uri baseAddress;
        readonly Regex relativeUriRegex;

        public RegexRequestHandler(
            string baseAddress,
            string relativeUriRegex,
            string[] methods,
            RequestHandlingFunc handlingFunc,
            string name) : base(name)
        {
            baseAddress.ThrowIfNull(nameof(baseAddress));
            relativeUriRegex.ThrowIfNull(nameof(relativeUriRegex));
            handlingFunc.ThrowIfNull(nameof(handlingFunc));

            m_methods = methods ?? EmptyArray<string>.Instance;
            this.handlingFunc = handlingFunc;
            this.baseAddress = new Uri(baseAddress);
            this.relativeUriRegex = new Regex(relativeUriRegex);
        }

        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            if (!request.IsMethodMatch(m_methods))
            {
                return false;
            }

            return IsRelativeUriMatch(request);
        }

        MatchingResult IsRelativeUriMatch(HttpRequestMessage request)
        {
            string relativeUri = baseAddress.GetRelativeUri(request.RequestUri);
            if (relativeUri == null) { return false; }
            Match match = relativeUriRegex.Match(relativeUri);
            if (!match.Success) { return false; }
            string[] groupNames = relativeUriRegex.GetGroupNames();
            GroupCollection capturedGroups = match.Groups;
            return new MatchingResult(
                true,
                groupNames.Select(name =>
                {
                    Group capturedGroup = capturedGroups[name];
                    return capturedGroup.Success
                        ? new KeyValuePair<string, object>(name, capturedGroup.Value)
                        : new KeyValuePair<string, object>(name, null);
                }));
        }

        protected override HttpResponseMessage CreateResponse(
            HttpRequestMessage request, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken)
        {
            return handlingFunc(request, parameters, cancellationToken);
        }
    }
}