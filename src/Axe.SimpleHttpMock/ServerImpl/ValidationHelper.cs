using System;

namespace Axe.SimpleHttpMock.ServerImpl
{
    static class ValidationHelper
    {
        public static void ThrowIfNull(this object value, string name)
        {
            if (value != null) return;
            throw new ArgumentNullException(name);
        }
    }
}