namespace Gdax.Feed.Utils
{
    using System;

    internal class Unsubscriber : IDisposable
    {
        private readonly Action action;

        public Unsubscriber(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            this.action.Invoke();
        }
    }
}
