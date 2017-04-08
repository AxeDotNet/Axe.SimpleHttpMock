using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Axe.SimpleHttpMock.Migration;
using Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    class UriTemplateRequestHandler : RequestHandlerBase
    {
        readonly RequestHandlingFunc handlingFunc;
        readonly string[] methods;
        readonly UriTemplate uriTemplate;
        readonly Uri baseAddress;

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

            this.handlingFunc = handlingFunc;
            this.methods = methods ?? EmptyArray<string>.Instance;
            uriTemplate = new UriTemplate(template);
            this.baseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            if (!request.IsMethodMatch(methods))
            {
                return false;
            }

            return uriTemplate.IsMatch(baseAddress, request.RequestUri);
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