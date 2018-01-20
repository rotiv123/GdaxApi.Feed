namespace Gdax.Feed.Channels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class HeartbeatChannel : BaseChannel<Heartbeat>
    {
        internal HeartbeatChannel(params string[] productIds)
            : base("heartbeat", productIds, true)
        {
        }

        protected override bool CanProcess(string type)
        {
            return "heartbeat" == type;
        }

        protected override void OnNext(JObject message)
        {
            Notify(new Heartbeat(
                        message["time"].ToObject<DateTimeOffset>(),
                        message.Value<string>("product_id"),
                        message.Value<long>("sequence"),
                        message.Value<long>("last_trade_id")));
        }
    }

    public class Heartbeat
    {
        public Heartbeat(DateTimeOffset timestamp, string productId, long sequence, long lastTradeId)
        {
            this.Timestamp = timestamp;
            this.ProductId = productId;
            this.Sequence = sequence;
            this.LastTradeId = lastTradeId;
        }

        public DateTimeOffset Timestamp { get; }

        public string ProductId { get; }

        public long Sequence { get; }

        public long LastTradeId { get; }
    }

    public static class HeartbeatChannelExtensions
    {
        public static ChannelSubscription<Heartbeat> StartHeartbeatChannel(this GdaxFeedApi api, IObserver<Heartbeat> observer, params string[] productIds)
        {
            return new ChannelSubscription<Heartbeat>(api, () => new HeartbeatChannel(productIds), new IObserver<Heartbeat>[] { observer });
        }

        public static ChannelSubscription<Heartbeat> StartHeartbeatChannel(this GdaxFeedApi api, IEnumerable<IObserver<Heartbeat>> observers, params string[] productIds)
        {
            return new ChannelSubscription<Heartbeat>(api, () => new HeartbeatChannel(productIds), observers);
        }
    }
}
