using System;
using System.Linq;

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
    }
}