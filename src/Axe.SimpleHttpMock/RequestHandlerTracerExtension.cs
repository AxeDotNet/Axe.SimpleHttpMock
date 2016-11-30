using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    public static class RequestHandlerTracerExtension
    {
        public static void VerifyBindedParameter(
            this IRequestHandlerTracer tracer,
            string parameter,
            Func<object, bool> verifyFunc)
        {
            bool matchParameter = tracer.CallingHistories.Any(
                c => c.Parameters != null 
                    && c.Parameters.ContainsKey(parameter)
                    && verifyFunc(c.Parameters[parameter]));
            if (matchParameter) { return; }
            throw new VerifyException(
                $"Either no parameter has name \"{parameter}\" or the parameter value did not pass the verify process");
        }

        public static void VerifyBindedParameter(
            this IRequestHandlerTracer tracer,
            string parameter,
            string value)
        {
            VerifyBindedParameter(tracer, parameter, value.Equals);
        }

        public static void VerifyHasBeenCalled(this IRequestHandlerTracer tracer)
        {
            if (tracer.CallingHistories.Count > 0) { return; }
            throw new VerifyException("The API is not called, which does not match your expectation.");
        }

        public static void VerifyHasBeenCalled(this IRequestHandlerTracer tracer, int times)
        {
            int actualCalledCount = tracer.CallingHistories.Count;
            if (actualCalledCount == times) { return; }
            throw new VerifyException(
                $"The API has been called for {actualCalledCount} time(s) rather than {times} time(s).");
        }

        public static void VerifyNotCalled(this IRequestHandlerTracer tracer)
        {
            int actualCalledCount = tracer.CallingHistories.Count;
            if (actualCalledCount == 0) { return; }
            throw new VerifyException($"The API has been called for {actualCalledCount} time(s). But your expectation is not being called.");
        }

        public static Task<T> FirstOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultFormatterIfNull(formatter), 
                tracer.FirstOrDefaultRequestContent());
        }

        public static Task<T> FirstOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            MediaTypeFormatter formatter = null)
        {
            return FirstOrDefaultRequestContentAsync<T>(tracer, formatter);
        }

        public static Task<T> LastOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultFormatterIfNull(formatter), 
                tracer.LastOrDefaultRequestContent());
        }

        public static Task<T> LastOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            MediaTypeFormatter formatter = null)
        {
            return LastOrDefaultRequestContentAsync<T>(tracer, formatter);
        }

        public static Task<T> SingleOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultFormatterIfNull(formatter), 
                tracer.SingleOrDefaultRequestContent());
        }

        public static Task<T> SingleOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            MediaTypeFormatter formatter = null)
        {
            return SingleOrDefaultRequestContentAsync<T>(tracer, formatter);
        }

        static MediaTypeFormatter GetDefaultFormatterIfNull(MediaTypeFormatter formatter)
        {
            return formatter ?? new JsonMediaTypeFormatter();
        }

        static Task<T> DeserializeContentAsync<T>(MediaTypeFormatter formatter, HttpContent content)
        {
            return content == null
                ? Task.FromResult(default(T))
                : content.ReadAsAsync<T>(new[] {formatter});
        }

        static HttpContent FirstOrDefaultRequestContent(this IRequestHandlerTracer tracer)
        {
            return tracer.CallingHistories.FirstOrDefault()?.Request?.Content;
        }

        static HttpContent LastOrDefaultRequestContent(this IRequestHandlerTracer tracer)
        {
            return tracer.CallingHistories.LastOrDefault()?.Request?.Content;
        }

        static HttpContent SingleOrDefaultRequestContent(this IRequestHandlerTracer tracer)
        {
            return tracer.CallingHistories.SingleOrDefault()?.Request?.Content;
        }
    }
}