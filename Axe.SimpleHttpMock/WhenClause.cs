using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Axe.SimpleHttpMock
{
    public class WhenClause
    {
        internal WhenClause(MockHttpServer server)
        {
            Server = server;
        }

        internal void AddCondition(Func<HttpRequestMessage, bool> match)
        {
            Matches.Add(match);
        }

        internal List<Func<HttpRequestMessage, bool>> Matches { get; } =
            new List<Func<HttpRequestMessage, bool>>();

        internal MockHttpServer Server { get; }
    }
}