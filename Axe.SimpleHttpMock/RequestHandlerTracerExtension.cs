using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Axe.SimpleHttpMock.Handlers;

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

        public static Task VerifyRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            Func<T, bool> verifyFunc)
        {
            HttpContent content = GetSingleRequestContent(tracer);

            Task verifyTask = content.ReadAsAsync<T>(new[] {new JsonMediaTypeFormatter()})
                .ContinueWith(
                    t =>
                    {
                        T o = t.Result;
                        if (verifyFunc(o)) return;
                        throw new VerifyException("The content did not pass the verify process.");
                    });
            verifyTask.ConfigureAwait(false);

            return verifyTask;
        }

        public static Task VerifyAnonymousRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            Func<T, bool> verifyFunc)
        {
            return VerifyRequestContentAsync(
                tracer,
                verifyFunc);
        }

        public static Task<T> FirstOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter)
        {
            return DeserializeContentAsync<T>(formatter, tracer.FirstOrDefaultRequestContent());
        }

        public static Task<T> LastOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter)
        {
            return DeserializeContentAsync<T>(formatter, tracer.LastOrDefaultRequestContent());
        }

        public static Task<T> SingleOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            MediaTypeFormatter formatter)
        {
            return DeserializeContentAsync<T>(formatter, tracer.SingleOrDefaultRequestContent());
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

        static HttpContent GetSingleRequestContent(IRequestHandlerTracer tracer)
        {
            int count = tracer.CallingHistories.Count;
            if (count != 1)
            {
                throw new VerifyException(
                    $"There are {count} request available and I do not know which one to take.");
            }

            CallingContext context = tracer.CallingHistories.Single();
            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                throw new VerifyException("Oops, the request is not available.");
            }

            HttpContent content = request.Content;
            if (content == null)
            {
                throw new VerifyException("There is no content in this request.");
            }
            return content;
        }
    }
}