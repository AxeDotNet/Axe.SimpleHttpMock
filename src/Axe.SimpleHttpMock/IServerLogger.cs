namespace Axe.SimpleHttpMock
{
    public interface IServerLogger
    {
        void Log(string log);
    }

    class DummyLogger : IServerLogger
    {
        public void Log(string log)
        {
        }
    }
}