using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Axe.SimpleHttpMock
{
    public static class ResponseExtension
    {
        public static HttpResponseMessage AsResponse(this HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode);
        }

        public static HttpResponseMessage AsResponse(
            this object payload, 
            HttpStatusCode statusCode = HttpStatusCode.OK,
            MediaTypeFormatter formatter = null)
        {
            ObjectContent content = payload == null
                ? null
                : new ObjectContent(payload.GetType(), payload, formatter ?? new JsonMediaTypeFormatter());
            return new HttpResponseMessage(statusCode)
            {
                Content = content
            };
        }
    }
}