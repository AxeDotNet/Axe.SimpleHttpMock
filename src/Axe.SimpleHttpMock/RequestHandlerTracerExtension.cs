using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The extension class to ease the way you do some verification on calling history.
    /// </summary>
    public static class RequestHandlerTracerExtension
    {
        /// <summary>
        /// Verify if the tracing information of a handler contains a request, which contains the binded paramter
        /// <paramref name="parameter"/>, and the value of the parameter satisfies <paramref name="verifyFunc"/>.
        /// </summary>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="parameter">The name of the parameter.</param>
        /// <param name="verifyFunc">The delegate to examine the value of the parameter.</param>
        /// <exception cref="VerifyException">
        /// Either the name of the parameter cannot be found in tracing information, or the value does not meet
        /// the requirement of <paramref name="verifyFunc"/>.
        /// </exception>
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
                $"Either no parameter has name \"{parameter}\" or the parameter value did not pass the verify process for API named \"{tracer.Name}\"");
        }

        /// <summary>
        /// Verify if the tracing information of a handler contains a request, which contains the binded paramter
        /// <paramref name="parameter"/>, and the value of the parameter equals to <paramref name="value"/>.
        /// </summary>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="parameter">The name of the parameter.</param>
        /// <param name="value">The value to compare with actual binded parameter value.</param>
        /// <exception cref="VerifyException">
        /// Either the name of the parameter cannot be found in tracing information, or the value does not equal to
        /// <paramref name="value"/>.
        /// </exception>
        public static void VerifyBindedParameter(
            this IRequestHandlerTracer tracer,
            string parameter,
            string value)
        {
            VerifyBindedParameter(
                tracer, 
                parameter, 
                value == null ? (Func<object, bool>)(v => v == null) : value.Equals);
        }

        /// <summary>
        /// Verify if the handler was called at least once.
        /// </summary>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <exception cref="VerifyException">
        /// The handler has never been called.
        /// </exception>
        public static void VerifyHasBeenCalled(this IRequestHandlerTracer tracer)
        {
            if (tracer.CallingHistories.Count > 0) { return; }
            throw new VerifyException($"The API \"{tracer.Name}\" is not called, which does not match your expectation.");
        }

        /// <summary>
        /// Verify if the handler was called at specified times.
        /// </summary>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="times">The desired times that the handler was called.</param>
        /// <exception cref="VerifyException">
        /// The handler calling times does not equals to <paramref name="times"/>.
        /// </exception>
        public static void VerifyHasBeenCalled(this IRequestHandlerTracer tracer, int times)
        {
            int actualCalledCount = tracer.CallingHistories.Count;
            if (actualCalledCount == times) { return; }
            throw new VerifyException(
                $"The API has been called for {actualCalledCount} time(s) rather than {times} time(s) for API named \"{tracer.Name}\".");
        }

        /// <summary>
        /// Verify if the handler was never been called.
        /// </summary>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <exception cref="VerifyException">
        /// The handler has been called.
        /// </exception>
        public static void VerifyNotCalled(this IRequestHandlerTracer tracer)
        {
            int actualCalledCount = tracer.CallingHistories.Count;
            if (actualCalledCount == 0) { return; }
            throw new VerifyException($"The API \"{tracer.Name}\" has been called for {actualCalledCount} time(s). But your expectation is not being called.");
        }

        /// <summary>
        /// Get first request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/>.
        /// </summary>
        /// <typeparam name="T">The deserialized type of the request content.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="deserializer">
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>
        /// A task reading the content of the request.
        /// </returns>
        public static Task<T> FirstOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            IContentDeserializer deserializer = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultDeserializerIfNull(deserializer), 
                tracer.FirstOrDefaultRequestContent());
        }

        /// <summary>
        /// Get first request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/> for anonymous type.
        /// </summary>
        /// <typeparam name="T">The anonymous type.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="template">The schema definition for the anonymous type.</param>
        /// <param name="deserializer">The formatter which will be used for deserializing.
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>A task reading the content of the request.</returns>
        public static Task<T> FirstOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            IContentDeserializer deserializer = null)
        {
            return FirstOrDefaultRequestContentAsync<T>(tracer, deserializer);
        }

        /// <summary>
        /// Get last request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/>.
        /// </summary>
        /// <typeparam name="T">The deserialized type of the request content.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="deserializer">
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>
        /// A task reading the content of the request.
        /// </returns>
        public static Task<T> LastOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            IContentDeserializer deserializer = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultDeserializerIfNull(deserializer), 
                tracer.LastOrDefaultRequestContent());
        }


        /// <summary>
        /// Get last request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/> for anonymous type.
        /// </summary>
        /// <typeparam name="T">The anonymous type.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="template">The schema definition for the anonymous type.</param>
        /// <param name="deserializer">The formatter which will be used for deserializing.
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>A task reading the content of the request.</returns>
        public static Task<T> LastOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            IContentDeserializer deserializer = null)
        {
            return LastOrDefaultRequestContentAsync<T>(tracer, deserializer);
        }

        /// <summary>
        /// Get single request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/>.
        /// </summary>
        /// <typeparam name="T">The deserialized type of the request content.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="deserializer">
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>
        /// A task reading the content of the request.
        /// </returns>
        public static Task<T> SingleOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            IContentDeserializer deserializer = null)
        {
            return DeserializeContentAsync<T>(
                GetDefaultDeserializerIfNull(deserializer), 
                tracer.SingleOrDefaultRequestContent());
        }

        /// <summary>
        /// Get single request from the calling history. Read the content of the request with specified
        /// <paramref name="deserializer"/> for anonymous type.
        /// </summary>
        /// <typeparam name="T">The anonymous type.</typeparam>
        /// <param name="tracer">The tracing information to verify.</param>
        /// <param name="template">The schema definition for the anonymous type.</param>
        /// <param name="deserializer">The formatter which will be used for deserializing.
        /// The formatter which will be used for deserializing. JSON formatter will be used if not specified.
        /// </param>
        /// <returns>A task reading the content of the request.</returns>
        public static Task<T> SingleOrDefaultRequestContentAsync<T>(
            this IRequestHandlerTracer tracer,
            T template,
            IContentDeserializer deserializer = null)
        {
            return SingleOrDefaultRequestContentAsync<T>(tracer, deserializer);
        }

        static IContentDeserializer GetDefaultDeserializerIfNull(IContentDeserializer deserializer)
        {
            return deserializer ?? ContentFormatters.JsonDeserializer;
        }

        static Task<T> DeserializeContentAsync<T>(IContentDeserializer deserializer, HttpContent content)
        {
            return content == null
                ? Task.FromResult(default(T))
                : deserializer.DeserializeAsync<T>(content);
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