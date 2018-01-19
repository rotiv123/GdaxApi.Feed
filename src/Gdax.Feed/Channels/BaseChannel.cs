namespace Gdax.Feed.Channels
{
    using System;
    using Newtonsoft.Json.Linq;
    using Gdax.Feed.Utils;

    public abstract class BaseChannel<T> : IChannel<T>, IObserver<JObject>
    {
        private readonly ObservableManager<T> observableManager;
        private long lastSequence;
        private bool processOutOfOrder;

        protected BaseChannel(string name, params string[] productIds)
            : this(name, productIds, false)
        {
        }

        protected BaseChannel(string name, string[] productIds, bool processOutOfOrder)
        {
            this.Name = name;
            this.observableManager = new ObservableManager<T>();
            this.ProductIds = productIds;
            this.lastSequence = 0;
            this.processOutOfOrder = processOutOfOrder;
        }

        public string[] ProductIds { get; }

        public string Name { get; }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.observableManager.Subscribe(observer);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract bool CanProcess(string type);

        protected abstract void OnNext(JObject message);
        
        protected virtual void OnCompleted()
        {
            Complete();
        }

        protected virtual void OnError(Exception ex)
        {
            Fault(ex);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.observableManager.Dispose();
            }
        }

        protected void Notify(T price)
        {
            this.observableManager.ParallelNotifyAsync(price);
        }

        protected void Fault(Exception ex)
        {
            this.observableManager.ParallelFaultAsync(ex);
        }

        protected void Complete()
        {
            this.observableManager.Complete();
        }

        bool IChannel.CanProcess(string type)
        {
            return this.CanProcess(type);
        }

        void IObserver<JObject>.OnCompleted()
        {
            OnCompleted();
        }

        void IObserver<JObject>.OnError(Exception error)
        {
            OnError(error);
        }

        void IObserver<JObject>.OnNext(JObject value)
        {
            var productId = value.Value<string>("product_id");
            if (productId != null && Array.Exists(this.ProductIds, x => x == productId))
            {
                var sequence = value.Value<long?>("sequence") ?? 0;
                if (this.processOutOfOrder || sequence > this.lastSequence)
                {
                    this.lastSequence = sequence;
                    var type = value.Value<string>("type");
                    if (type != null && CanProcess(type))
                    {
                        OnNext(value);
                    }
                }
            }
        }
    }
}
