using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Axe.SimpleHttpMock.Test
{
    public class HttpServerFactBase : IDisposable
    {
        readonly List<HttpClient> clients = new List<HttpClient>();

        protected HttpClient CreateClient(HttpMessageHandler messageHandler)
        {
            var httpClient = new HttpClient(messageHandler);
            clients.Add(httpClient);
            return httpClient;
        }

        public void Dispose()
        {
            clients.ForEach(c => c.Dispose());
        }
    }
}