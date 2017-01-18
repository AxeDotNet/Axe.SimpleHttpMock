namespace Axe.SimpleHttpMock
{
    /// <summary>
    /// The logging interface for mocked http server.
    /// </summary>
    public interface IServerLogger
    {
        /// <summary>
        /// Write a log message to output.
        /// </summary>
        /// <param name="log">The log message.</param>
        void Log(string log);
    }
}