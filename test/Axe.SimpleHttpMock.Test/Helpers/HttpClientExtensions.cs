using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

#if NET_CORE
using System.Reflection;
#endif

namespace Axe.SimpleHttpMock.Test.Helpers
{
    static class HttpClientExtensions
    {
        public static async Task<T> ReadAs<T>(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Content == null)
            {

#if NET_CORE
                var typeInfo = typeof(T).GetTypeInfo();
                if (typeInfo.IsClass || typeInfo.IsInterface)
#else
                if (typeof(T).IsClass || typeof(T).IsInterface)
#endif
                {
                    return default(T);
                }

                throw new InvalidOperationException("There is no content in the response. Cannot cast nothing to value type.");
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static Task<T> ReadAs<T>(this HttpResponseMessage response, T template)
        {
            return ReadAs<T>(response);
        }

        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string uri, T payload)
        {
            return client.PostAsync(uri, CreateJsonContent(payload));
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string uri, T payload)
        {
            return client.PutAsync(uri, CreateJsonContent(payload));
        }

        static StringContent CreateJsonContent<T>(T payload)
        {
            return new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        }
    }
}