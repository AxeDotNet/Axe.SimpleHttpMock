using System.Text;
using Axe.SimpleHttpMock.ServerImpl.ContentFormatter;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// Represents a collection of content formatters
    /// </summary>
    public class ContentFormatters
    {
        /// <summary>
        /// Get a JSON serializer.
        /// </summary>
        public static IContentSerializer JsonSerializer { get; } = new JsonContentFormatter(Encoding.UTF8);

        /// <summary>
        /// Get a JSON deserializer.
        /// </summary>
        public static IContentDeserializer JsonDeserializer { get; } = new JsonContentFormatter(Encoding.UTF8);
    }
}