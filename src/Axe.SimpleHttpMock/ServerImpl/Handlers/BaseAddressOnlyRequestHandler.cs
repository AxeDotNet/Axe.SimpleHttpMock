using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    class BaseAddressOnlyRequestHandler : RequestHandlerBase
    {
        readonly Uri baseAddress;
        readonly Func<HttpRequestMessage, HttpResponseMessage> handlingFunc;

        public BaseAddressOnlyRequestHandler(
            string baseAddress,
            Func<HttpRequestMessage, HttpResponseMessage> handlingFunc,
            string name) : base(name)
        {
            baseAddress.ThrowIfNull(nameof(baseAddress));
            handlingFunc.ThrowIfNull(nameof(handlingFunc));

            this.baseAddress = new Uri(baseAddress, UriKind.Absolute);
            this.handlingFunc = handlingFunc;
        }

        public override MatchingResult IsMatch(HttpRequestMessage request)
        {
            return baseAddress.IsBaseAddressMatch(request.RequestUri);
        }

        protected override HttpResponseMessage CreateResponse(
            HttpRequestMessage request,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            return handlingFunc(request);
        }
    }
}