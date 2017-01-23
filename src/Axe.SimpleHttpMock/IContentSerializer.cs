using System.Net.Http;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// Represents an HTTP content serializer.
    /// </summary>
    public interface IContentSerializer
    {
        /// <summary>
        /// Format object to <see cref="HttpContent"/>.
        /// </summary>
        /// <param name="payload">The object to format.</param>
        /// <returns>Http content.</returns>
        HttpContent Format(object payload);
    }
}