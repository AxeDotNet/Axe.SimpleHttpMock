using System.Net.Http;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// Represents an HTTP content deserializer.
    /// </summary>
    public interface IContentDeserializer
    {
        /// <summary>
        /// Deserialize http content to an object.
        /// </summary>
        /// <param name="content"><see cref="HttpContent"/> instance.</param>
        /// <returns>A task with deserialized object.</returns>
        Task<object> DeserializeAsync(HttpContent content);

        /// <summary>
        /// Deserialize http content to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="content">The <see cref="HttpContent"/> instance.</param>
        /// <returns>A task with deserialized object.</returns>
        Task<T> DeserializeAsync<T>(HttpContent content);

        /// <summary>
        /// Deserialize http content to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="content">The <see cref="HttpContent"/> instance.</param>
        /// <param name="template">The template object.</param>
        /// <returns>A task with deserialized object.</returns>
        Task<T> DeserializeAsync<T>(HttpContent content, T template);
    }
}