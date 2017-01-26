namespace Axe.SimpleHttpMock.Migration
{
    static class EmptyArray<T>
    {
        public static T[] Instance { get; } = new T[0];
    }
}