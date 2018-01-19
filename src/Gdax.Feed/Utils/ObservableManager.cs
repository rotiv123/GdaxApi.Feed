namespace Gdax.Feed.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class ObservableManager<T> : IDisposable
    {
        private readonly List<IObserver<T>> observers;

        public ObservableManager()
        {
            this.observers = new List<IObserver<T>>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!this.observers.Contains(observer))
            {
                this.observers.Add(observer);
            }

            return new Unsubscriber(() => Unsibscribe(observer));
        }

        public async void ParallelNotifyAsync(T value)
        {
            var tasks = new List<Task>();
            foreach (var observer in this.observers)
            {
                tasks.Add(Task.Factory.StartNew(() => observer.OnNext(value)));
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        public async void ParallelFaultAsync(Exception ex)
        {
            var tasks = new List<Task>();
            foreach (var observer in this.observers)
            {
                tasks.Add(Task.Factory.StartNew(() => observer.OnError(ex)));
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        public void Complete()
        {
            foreach (var observer in this.observers)
            {
                observer.OnCompleted();
            }
        }

        private void Unsibscribe(IObserver<T> observer)
        {
            if (this.observers.Contains(observer))
            {
                this.observers.Remove(observer);
            }
        }

        public void Dispose()
        {
            Disable();
        }

        private void Disable()
        {
            foreach (var observer in this.observers.ToArray())
            {
                if (this.observers.Contains(observer))
                {
                    observer.OnCompleted();
                }
            }

            this.observers.Clear();
        }
    }
}
