using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Axe.SimpleHttpMock.Test.Helpers
{
    static class HttpResponseExtensions
    {
        public static async Task<T> ReadAs<T>(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Content == null)
            {
                if (typeof(T).IsClass || typeof(T).IsInterface)
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
    }
}