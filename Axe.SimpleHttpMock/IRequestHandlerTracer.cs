using System.Collections.Generic;
using Axe.SimpleHttpMock.Handlers;

namespace Axe.SimpleHttpMock
{
    public interface IRequestHandlerTracer
    {
        string Name { get; }
        IReadOnlyCollection<CallingContext> CallingHistories { get; } 
    }
}