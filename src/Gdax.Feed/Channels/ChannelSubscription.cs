namespace Gdax.Feed.Channels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Gdax.Feed.Subscriptions;

    public class ChannelSubscription<T> : IDisposable
    {
        private readonly IChannel<T> channel;
        private readonly GdaxFeedApi api;
        private readonly IDisposable upstreamSusbcription;
        private readonly List<IDisposable> downstreamSusbcriptions;

        public ChannelSubscription(GdaxFeedApi api, Func<IChannel<T>> channelFactory, IEnumerable<IObserver<T>> observers)
        {
            this.api = api;
            this.channel = channelFactory();
            this.upstreamSusbcription = ((IObservable<JObject>)api).Subscribe(this.channel as IObserver<JObject>);
            this.downstreamSusbcriptions = new List<IDisposable>();
            foreach (var observer in observers)
            {
                this.downstreamSusbcriptions.Add(this.channel.Subscribe(observer));
            }

            api.SubscriptionManager.SubscribeAsync(
               new SubscriptionRequest
               {
                   ProductIds = new List<string>(this.channel.ProductIds),
                   Channels = new List<string> { this.channel.Name }
               });
        }

        public void Dispose()
        {
            this.api.SubscriptionManager.UnsubscribeAsync(
               new SubscriptionRequest
               {
                   ProductIds = new List<string>(this.channel.ProductIds),
                   Channels = new List<string> { this.channel.Name }
               });
            this.upstreamSusbcription.Dispose();
            this.channel.Dispose();
            foreach (var downstreamSusbcription in this.downstreamSusbcriptions)
            {
                downstreamSusbcription.Dispose();
            }

            this.downstreamSusbcriptions.Clear();
        }
    }
}
