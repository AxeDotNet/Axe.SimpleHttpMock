using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers
{
    static class UriExtensions
    {
        static readonly char[] Slashes = { '\\', '/' };

        static readonly char[] queryMark = { '?' };

        static bool IsUriPrefixMatch(Uri baseAddressUri, Uri uriToExamine)
        {
            return baseAddressUri.Scheme == uriToExamine.Scheme &&
                baseAddressUri.Host == uriToExamine.Host &&
                baseAddressUri.Port == uriToExamine.Port &&
                baseAddressUri.UserInfo == uriToExamine.UserInfo;
        }

        static string[] GetSegments(Uri uri)
        {
            return uri.Segments.Select(s => s.Trim(Slashes)).ToArray();
        }

        static IEnumerable<string> EnumerateRelativePathSegments(Uri baseAddress, Uri uri)
        {
            if (!IsUriPrefixMatch(baseAddress, uri))
            {
                return null;
            }

            IReadOnlyList<string> pathSegments = GetSegments(uri);
            IReadOnlyCollection<string> basePathSegments = GetSegments(baseAddress);

            bool noSameBase = basePathSegments
                .Where((t, i) => !pathSegments[i].Equals(
                    t,
#if NET_CORE
                    StringComparison.OrdinalIgnoreCase))
#else
                    StringComparison.InvariantCultureIgnoreCase))
#endif
                .Any();

            return noSameBase
                ? null
                : pathSegments.Skip(basePathSegments.Count);
        }

        static string TrimSlashes(string value)
        {
            return value.Trim(Slashes);
        }

        static string RemoveLeadingSlash(string value)
        {
            return value.TrimStart(Slashes);
        }

        public static bool IsBaseAddressMatch(this Uri baseAddress, Uri uri)
        {
            return EnumerateRelativePathSegments(baseAddress, uri) != null;
        }

        public static string[] GetRelativePathSegments(this Uri baseAddress, Uri uri)
        {
            return EnumerateRelativePathSegments(baseAddress, uri)?.ToArray();
        }

        public static string GetRelativeUri(this Uri baseAddress, Uri uri)
        {
            if (!IsUriPrefixMatch(baseAddress, uri)) { return null; }
            string baseAddressPath = TrimSlashes(baseAddress.AbsolutePath);
            string pathAndQuery = RemoveLeadingSlash(uri.PathAndQuery);
            if (string.IsNullOrEmpty(baseAddressPath)) { return pathAndQuery; }
            if (!pathAndQuery.StartsWith(
#if NET_CORE
                baseAddressPath, StringComparison.OrdinalIgnoreCase
#else
                baseAddressPath, true, CultureInfo.InvariantCulture
#endif
            ))
            {
                return null;
            }

            return baseAddressPath.Length == pathAndQuery.Length 
                ? string.Empty 
                : RemoveLeadingSlash(pathAndQuery.Substring(baseAddressPath.Length));
        }

        public static IEnumerable<KeyValuePair<string, string>> GetQueryCollection(this string queryString)
        {
            var query = queryString.TrimStart(queryMark);
            if (string.IsNullOrEmpty(query))
            {
                yield break;
            }

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                int equalSignIndex = pair.IndexOf('=');
                if (equalSignIndex > 0)
                {
                    var val = pair.Substring(equalSignIndex + 1);
                    if (!string.IsNullOrEmpty(val))
                    {
                        val = Uri.UnescapeDataString(val);
                    }

                    yield return new KeyValuePair<string, string>(pair.Substring(0, equalSignIndex), val);
                }
            }
        }
    }
}