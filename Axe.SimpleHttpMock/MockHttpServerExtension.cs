using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    public static class MockHttpServerExtension
    {
        public static MockHttpServer AddHandler(
            this MockHttpServer server,
            IEnumerable<Func<HttpRequestMessage, bool>> matches,
            Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handleFunc)
        {
            server.AddHandler(new RequestHandler(matches, handleFunc));
            return server;
        }

        public static WhenClause When(this MockHttpServer server)
        {
            ThrowIfServerIsNull(server);
            return new WhenClause(server);
        }

        public static WhenClause WhenAll(this MockHttpServer server)
        {
            ThrowIfServerIsNull(server);
            var whenClause = new WhenClause(server);
            whenClause.AddCondition(req => true);
            return whenClause;
        }

        public static WhenClause UriSatisfy(this WhenClause whenClause, Func<Uri, bool> uriMatcher)
        {
            Func<HttpRequestMessage, bool> match = req => uriMatcher(req.RequestUri);
            whenClause.AddCondition(match);
            return whenClause;
        }

        public static WhenClause UriMatches(
            this WhenClause whenClause,
            string pattern,
            bool caseSensitive = false)
        {
            RegexOptions options = RegexOptions.Singleline | RegexOptions.CultureInvariant;
            if (!caseSensitive) { options |= RegexOptions.IgnoreCase; }
            return UriSatisfy(whenClause,
                uri => new Regex(pattern, options).IsMatch(uri.AbsoluteUri));
        }

        public static WhenClause UriContains(
            this WhenClause whenClause,
            string subString,
            bool caseSensitive = false)
        {
            return UriSatisfy(whenClause,
                uri =>
                {
                    string requestUri = caseSensitive
                        ? uri.AbsoluteUri
                        : uri.AbsoluteUri.ToLower(CultureInfo.InvariantCulture);
                    string testString = caseSensitive
                        ? subString
                        : subString.ToLower(CultureInfo.InvariantCulture);
                    return requestUri.Contains(testString);
                });
        }

        public static void Response(
            this WhenClause whenClause,
            Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
        {
            List<Func<HttpRequestMessage, bool>> matches = GetMatches(whenClause);
            MockHttpServer server = whenClause.Server;
            server.AddHandler(matches, (req, c) => responseFunc(req));
        }

        public static void ResponseJson<T>(
            this WhenClause whenClause,
            HttpStatusCode statusCode,
            T payload)
        {
            List<Func<HttpRequestMessage, bool>> matches = GetMatches(whenClause);
            MockHttpServer server = whenClause.Server;
            server.AddHandler(matches,
                (req, c) =>
                {
                    HttpResponseMessage response = req.CreateResponse(statusCode);
                    response.Content = new ObjectContent<T>(payload, new JsonMediaTypeFormatter());
                    return response;
                });
        }

        public static void ResponseStatusCode(
            this WhenClause whenClause,
            HttpStatusCode statusCode)
        {
            List<Func<HttpRequestMessage, bool>> matches = GetMatches(whenClause);
            MockHttpServer server = whenClause.Server;
            server.AddHandler(matches, (req, c) => req.CreateResponse(statusCode));
        }

        static List<Func<HttpRequestMessage, bool>> GetMatches(WhenClause whenClause)
        {
            if (whenClause == null)
            {
                throw new ArgumentNullException(nameof(whenClause));
            }

            List<Func<HttpRequestMessage, bool>> matches = whenClause.Matches;
            if (!matches.Any())
            {
                throw new InvalidOperationException("Please use When() to specify conditions");
            }
            return matches;
        }

        static void ThrowIfServerIsNull(MockHttpServer server)
        {
            if (server != null) return;
            throw new ArgumentNullException(nameof(server));
        }
    }
}