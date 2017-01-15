using System.Collections.Generic;
using Axe.SimpleHttpMock.ServerImpl;

namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// This interface representing the tracing information holder of the request handler.
    /// </summary>
    public interface IRequestHandlerTracer
    {
        /// <summary>
        /// The name of the request handler. This value is the key to access tracing information.
        /// So no tracing information will be recorded if this value is not specifieid. 
        /// Its default values is <code>null</code>. 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the calling history collection. Please note that since the calling may be in
        /// a concurrent manner. So the sequence of the calling history cannot be guarenteed.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Name"/> is <c>null</c>, This property will returns empty collection.
        /// </remarks>
        IReadOnlyCollection<CallingHistoryContext> CallingHistories { get; } 
    }
}