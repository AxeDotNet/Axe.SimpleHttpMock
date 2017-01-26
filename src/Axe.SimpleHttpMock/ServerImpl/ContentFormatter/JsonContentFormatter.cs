using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Axe.SimpleHttpMock.ServerImpl.ContentFormatter
{
    class JsonContentFormatter : IContentSerializer, IContentDeserializer
    {
        const string MediaType = "application/json";
        readonly Encoding m_encoding;
        
        public JsonContentFormatter(Encoding encoding = null)
        {
            m_encoding = encoding ?? Encoding.UTF8;
        }
        
        public HttpContent Format(object payload)
        {
            string serialized = JsonConvert.SerializeObject(payload);
            return new StringContent(serialized, m_encoding, MediaType);
        }

        public Task<object> DeserializeAsync(HttpContent content)
        {
            return DeserializeContentAsync(content, JsonConvert.DeserializeObject);
        }

        public Task<T> DeserializeAsync<T>(HttpContent content)
        {
            return DeserializeContentAsync(content, JsonConvert.DeserializeObject<T>);
        }

        public Task<T> DeserializeAsync<T>(HttpContent content, T template)
        {
            return DeserializeContentAsync(
                content,
                s => JsonConvert.DeserializeAnonymousType(s, template));
        }

        static async Task<T> DeserializeContentAsync<T>(HttpContent content, Func<string, T> deserializingFunc)
        {
            string jsonStr = await content.ReadAsStringAsync().ConfigureAwait(false);
            return deserializingFunc(jsonStr);
        }
    }
}