using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.UriTemplates
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
            if (!IsUriPrefixMatch(baseAddress, uri))
            {
                return false;
            }

            string[] relativePathToExamine = GetRelativePathSegments(
                GetSegments(uri),
                GetSegments(baseAddress));
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

        static bool IsUriPrefixMatch(Uri baseAddressUri, Uri uriToExamine)
        {
            return baseAddressUri.Scheme == uriToExamine.Scheme &&
                baseAddressUri.Host == uriToExamine.Host &&
                baseAddressUri.Port == uriToExamine.Port &&
                baseAddressUri.UserInfo == uriToExamine.UserInfo;
        }

        static string[] GetRelativePathSegments(IReadOnlyList<string> pathSegments, IReadOnlyCollection<string> basePathSegments)
        {
            return basePathSegments.Where((t, i) => !pathSegments[i].Equals(t, StringComparison.InvariantCultureIgnoreCase)).Any()
                ? null
                : pathSegments.Skip(basePathSegments.Count).ToArray();
        }

        static string[] GetSegments(Uri uri)
        {
            return uri.Segments.Select(s => s.Trim(Slashes)).ToArray();
        }
    }
}