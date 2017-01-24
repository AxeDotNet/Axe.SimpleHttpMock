using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.UriTemplates
{
    class SimpleUriTemplate
    {
        static readonly char[] Slashes = {'\\', '/'};
        static readonly char[] VariableBraces = {'{', '}'};

        public static MatchingResult IsMatch(string baseAddress, string template, string uri)
        {
            baseAddress.ThrowIfNull(nameof(baseAddress));
            template.ThrowIfNull(nameof(template));
            uri.ThrowIfNull(nameof(uri));

            var baseAddressUri = new Uri(baseAddress, UriKind.Absolute);
            var uriToExamine = new Uri(uri, UriKind.Absolute);

            if (!IsUriPrefixMatch(baseAddressUri, uriToExamine))
            {
                return false;
            }

            string[] relativePathToExamine = TrimStart(
                GetSegments(uriToExamine),
                GetSegments(baseAddressUri));
            if (relativePathToExamine == null)
            {
                return false;
            }

            return SegmentMatch(template, relativePathToExamine);
        }

        static bool IsUriPrefixMatch(Uri baseAddressUri, Uri uriToExamine)
        {
            return baseAddressUri.Scheme == uriToExamine.Scheme &&
                baseAddressUri.Host == uriToExamine.Host &&
                baseAddressUri.Port == uriToExamine.Port &&
                baseAddressUri.UserInfo == uriToExamine.UserInfo;
        }

        static MatchingResult SegmentMatch(string template, string[] pathSegments)
        {
            string[] templateSegments = template.Split(Slashes, StringSplitOptions.RemoveEmptyEntries);
            if (templateSegments.Length != pathSegments.Length) { return false; }

            var capturedVariables = new List<KeyValuePair<string, object>>();
            for (int i = 0; i < templateSegments.Length; i++)
            {
                string ts = templateSegments[i];
                if (IsVariable(ts))
                {
                    capturedVariables.Add(new KeyValuePair<string, object>(GetVariableName(ts), pathSegments[i]));
                    continue;
                }

                if (!ts.Equals(pathSegments[i], StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
            }

            return new MatchingResult(true, capturedVariables);
        }

        static string GetVariableName(string templateSegment)
        {
            return templateSegment.Trim(VariableBraces);
        }

        static bool IsVariable(string segment)
        {
            return !string.IsNullOrEmpty(segment) && segment[0] == '{' &&
                segment[segment.Length - 1] == '}';
        }

        static string[] GetSegments(Uri uri)
        {
            return uri.Segments.Select(s => s.Trim(Slashes)).ToArray();
        }

        static string[] TrimStart(IReadOnlyList<string> arrayToTrim, IReadOnlyCollection<string> trimSequences)
        {
            return trimSequences.Where((t, i) => !arrayToTrim[i].Equals(t, StringComparison.InvariantCultureIgnoreCase)).Any()
                ? null 
                : arrayToTrim.Skip(trimSequences.Count).ToArray();
        }
    }
}