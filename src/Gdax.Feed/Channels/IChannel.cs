namespace Gdax.Feed.Channels
{
    using System;

    public interface IChannel : IDisposable
    {
        string Name { get; }

        string[] ProductIds { get; }

        bool CanProcess(string type);
    }

    public interface IChannel<T> : IChannel, IObservable<T>
    {
    }
}
