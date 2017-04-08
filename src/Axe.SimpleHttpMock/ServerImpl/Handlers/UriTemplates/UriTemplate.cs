using System;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriTemplate
    {
        static readonly Uri FakeBaseAddress = new Uri("http://axe.simplehttp.mock");
        
        readonly UriTemplatePathMatcher uriTemplatePathMatcher;
        readonly UriQueryStringTemplateMatcher uriQueryStringTemplateMatcher;

        public UriTemplate(string template)
        {
            var fakeBaseAddressTemplate = new Uri(FakeBaseAddress, template);
            uriTemplatePathMatcher = new UriTemplatePathMatcher(fakeBaseAddressTemplate);
            uriQueryStringTemplateMatcher = new UriQueryStringTemplateMatcher(fakeBaseAddressTemplate);
        }

        public MatchingResult IsMatch(Uri baseAddress, Uri uri)
        {
            string[] relativePathToExamine = baseAddress.GetRelativePathSegments(uri);
            if (relativePathToExamine == null)
            {
                return false;
            }

            MatchingResult pathMatchingResult = uriTemplatePathMatcher.IsMatch(relativePathToExamine);
            if (!pathMatchingResult) { return false; }

            MatchingResult queryMatchingResult = uriQueryStringTemplateMatcher.IsMatch(uri.Query);
            if (!queryMatchingResult) { return false; }

            return new MatchingResult(
                true,
                pathMatchingResult.Parameters.Concat(queryMatchingResult.Parameters));
        }
    }
}