using System;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The extension class to ease the way adding handler to mocked http server.
    /// </summary>
    public static class MockHttpServerExtension
    {
        /// <summary>
        /// Mock a service. A service can contains a set of APIs, of which the base addresses
        /// are all the same.
        /// </summary>
        /// <param name="server">The mocked http server.</param>
        /// <param name="serviceUriPrefix">The base address for current service.</param>
        /// <returns>The service registration clause.</returns>
        /// <exception cref="UriFormatException">
        /// The <paramref name="serviceUriPrefix"/> is not a valid absolute URI.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="server"/> is <c>null</c> or the <paramref name="serviceUriPrefix"/> is <c>null</c>.
        /// </exception>
        public static WithServiceClause WithService(
            this MockHttpServer server,
            string serviceUriPrefix)
        {
            return new WithServiceClause(server, serviceUriPrefix);
        }
    }
}