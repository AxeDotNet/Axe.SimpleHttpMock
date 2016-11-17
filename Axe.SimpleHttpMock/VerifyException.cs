using System;
using System.Runtime.Serialization;

namespace Axe.SimpleHttpMock
{
    [Serializable]
    public class VerifyException : Exception
    {
        public VerifyException() { }

        public VerifyException(string message) : base(message) { }

        public VerifyException(string message, Exception inner) : base(message, inner) { }

        protected VerifyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}