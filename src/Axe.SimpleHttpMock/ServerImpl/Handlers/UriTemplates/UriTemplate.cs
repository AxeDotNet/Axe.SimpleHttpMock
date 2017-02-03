using System;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriTemplate
    {
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
            string[] relativePathToExamine = baseAddress.GetRelativePathSegments(uri);
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
    }
}