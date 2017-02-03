using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriTemplate
    {
        static readonly char[] Slashes = { '\\', '/' };
        static readonly Uri fakeBaseAddress = new Uri("http://axe.simplehttp.mock");
        
        readonly UriTemplatePathMatcher m_uriTemplatePathMatcher;
        readonly UriQueryStringTemplateMatcher m_uriQueryStringTemplateMatcher;

        public UriTemplate(string template)
        {
            var fakeBaseAddressTemplate = new Uri(fakeBaseAddress, template);
            m_uriTemplatePathMatcher = new UriTemplatePathMatcher(fakeBaseAddressTemplate);
            m_uriQueryStringTemplateMatcher = new UriQueryStringTemplateMatcher(fakeBaseAddressTemplate);
        }

        public MatchingResult IsMatch(Uri baseAddress, Uri uri)
        {
            string[] relativePathToExamine = GetRelativePathSegments(baseAddress, uri);
            if (relativePathToExamine == null)
            {
                return false;
            }

            MatchingResult pathMatchingResult = m_uriTemplatePathMatcher.IsMatch(relativePathToExamine);
            if (!pathMatchingResult) { return false; }

            MatchingResult queryMatchingResult = m_uriQueryStringTemplateMatcher.IsMatch(uri.Query);
            if (!queryMatchingResult) { return false; }

            return new MatchingResult(
                true,
                pathMatchingResult.Parameters.Concat(queryMatchingResult.Parameters));
        }

        public static bool IsBaseAddressMatch(Uri baseAddress, Uri uri)
        {
            return EnumerateRelativePathSegments(baseAddress, uri) != null;
        }

        static bool IsUriPrefixMatch(Uri baseAddressUri, Uri uriToExamine)
        {
            return baseAddressUri.Scheme == uriToExamine.Scheme &&
                baseAddressUri.Host == uriToExamine.Host &&
                baseAddressUri.Port == uriToExamine.Port &&
                baseAddressUri.UserInfo == uriToExamine.UserInfo;
        }

        static string[] GetRelativePathSegments(Uri baseAddress, Uri uri)
        {
            return EnumerateRelativePathSegments(baseAddress, uri)?.ToArray();
        }

        static IEnumerable<string> EnumerateRelativePathSegments(Uri baseAddress, Uri uri)
        {
            if (!IsUriPrefixMatch(baseAddress, uri))
            {
                return null;
            }

            IReadOnlyList<string> pathSegments = GetSegments(uri);
            IReadOnlyCollection<string> basePathSegments = GetSegments(baseAddress);

            return basePathSegments
                .Where((t, i) => !pathSegments[i].Equals(t, StringComparison.InvariantCultureIgnoreCase)).Any()
                ? null
                : pathSegments.Skip(basePathSegments.Count);
        }

        static string[] GetSegments(Uri uri)
        {
            return uri.Segments.Select(s => s.Trim(Slashes)).ToArray();
        }
    }
}