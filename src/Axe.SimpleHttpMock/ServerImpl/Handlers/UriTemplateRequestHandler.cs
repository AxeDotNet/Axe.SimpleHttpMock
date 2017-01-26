using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Axe.SimpleHttpMock.Migration;
using Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    class UriTemplateRequestHandler : RequestHandlerBase
    {
        readonly RequestHandlingFunc m_handlingFunc;
        readonly string[] m_methods;
        readonly UriTemplate m_uriTemplate;
        readonly Uri m_baseAddress;
        readonly bool m_onlyForBaseAddress;

        public UriTemplateRequestHandler(
            string baseAddress, 
            string template, 
            string[] methods,
            RequestHandlingFunc handlingFunc,
            string name) 
            : base(name)
        {
            baseAddress.ThrowIfNull(nameof(baseAddress));
            template.ThrowIfNull(nameof(template));
            handlingFunc.ThrowIfNull(nameof(handlingFunc));

            m_handlingFunc = handlingFunc;
            m_methods = methods ?? EmptyArray<string>.Instance;
            m_uriTemplate = new UriTemplate(template);
            m_baseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        public UriTemplateRequestHandler(
            string baseAddress,
            RequestHandlingFunc handlingFunc,
            string name)
            : base(name)
        {
            baseAddress.ThrowIfNull(nameof(baseAddress));
            handlingFunc.ThrowIfNull(nameof(handlingFunc));

            m_handlingFunc = handlingFunc;
            m_methods = EmptyArray<string>.Instance;
            m_uriTemplate = new UriTemplate(string.Empty);
            m_baseAddress = new Uri(baseAddress, UriKind.Absolute);

            m_onlyForBaseAddress = true;
        }

        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            if (m_methods.Length != 0 &&
                !m_methods.Any(m => m.Equals(request.Method.Method, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            return m_onlyForBaseAddress
                ? (MatchingResult)m_uriTemplate.IsBaseAddressMatch(m_baseAddress, request.RequestUri)
                : m_uriTemplate.IsMatch(m_baseAddress, request.RequestUri);
        }

        protected override HttpResponseMessage CreateResponse(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            return m_handlingFunc(request, parameters, cancellationToken);
        }
    }
}