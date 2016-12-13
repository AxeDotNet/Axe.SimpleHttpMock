using System.Collections.Generic;
using Axe.SimpleHttpMock.ServerImpl;

namespace Axe.SimpleHttpMock
{
    public interface IRequestHandlerTracer
    {
        string Name { get; }
        IReadOnlyCollection<CallingHistoryContext> CallingHistories { get; } 
    }
}