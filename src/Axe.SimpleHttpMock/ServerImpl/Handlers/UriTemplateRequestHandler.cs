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

        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            if (!request.IsMethodMatch(m_methods))
            {
                return false;
            }

            return m_uriTemplate.IsMatch(m_baseAddress, request.RequestUri);
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