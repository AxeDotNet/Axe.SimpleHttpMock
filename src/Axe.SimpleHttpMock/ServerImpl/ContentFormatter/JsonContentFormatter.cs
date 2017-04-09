using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Axe.SimpleHttpMock.ServerImpl.ContentFormatter
{
    class JsonContentFormatter : IContentSerializer, IContentDeserializer
    {
        const string MediaType = "application/json";

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        readonly Encoding encoding;
        
        public JsonContentFormatter(Encoding encoding = null)
        {
            this.encoding = encoding ?? Encoding.UTF8;
        }
        
        public HttpContent Format(object payload)
        {
            string serialized = JsonConvert.SerializeObject(payload, JsonSerializerSettings);
            return new StringContent(serialized, encoding, MediaType);
        }

        public Task<object> DeserializeAsync(HttpContent content)
        {
            return DeserializeContentAsync(content, jsonStr => JsonConvert.DeserializeObject(jsonStr, JsonSerializerSettings));
        }

        public Task<T> DeserializeAsync<T>(HttpContent content)
        {
            return DeserializeContentAsync(content, jsonStr => JsonConvert.DeserializeObject<T>(jsonStr, JsonSerializerSettings));
        }

        public Task<T> DeserializeAsync<T>(HttpContent content, T template)
        {
            return DeserializeContentAsync(
                content,
                s => JsonConvert.DeserializeAnonymousType(s, template, JsonSerializerSettings));
        }

        static async Task<T> DeserializeContentAsync<T>(HttpContent content, Func<string, T> deserializingFunc)
        {
            string jsonStr = await content.ReadAsStringAsync().ConfigureAwait(false);
            return deserializingFunc(jsonStr);
        }
    }
}