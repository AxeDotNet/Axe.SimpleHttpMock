using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.UriTemplates
{
    class UriTemplatePathMatcher
    {
        static readonly char[] Slashes = { '\\', '/' };
        readonly UriTemplateElement[] m_elements;

        public UriTemplatePathMatcher(Uri fakeBaseAddressTemplate)
        {
            m_elements = fakeBaseAddressTemplate.Segments
                .Select(s => Uri.UnescapeDataString(s).Trim(Slashes))
                .Select(s => new UriTemplateElement(s))
                .Skip(1)
                .ToArray();
        }

        public MatchingResult IsMatch(string[] pathSegments)
        {
            if (pathSegments.Length != m_elements.Length)
            {
                return false;
            }

            var capturedVariables = new List<KeyValuePair<string, object>>();
            for (int i = 0; i < pathSegments.Length; ++i)
            {
                UriTemplateElement templateSegment = m_elements[i];
                string pathSegment = pathSegments[i];
                if (!templateSegment.IsMatch(pathSegment))
                {
                    return false;
                }

                if (templateSegment.IsVariable)
                {
                    capturedVariables.Add(new KeyValuePair<string, object>(templateSegment.Value, pathSegment));
                }
            }

            return new MatchingResult(true, capturedVariables);
        }
    }
}