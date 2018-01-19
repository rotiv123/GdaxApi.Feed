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
            var t = new Heartbeat(message);
            Notify(t);
        }
    }

    public class Heartbeat
    {
        public Heartbeat(JObject message)
        {
            this.Timestamp = message["time"].ToObject<DateTimeOffset>();
            this.ProductId = message.Value<string>("product_id");
            this.Sequence = message.Value<long>("sequence");
            this.LastTradeId = message.Value<long>("last_trade_id");
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
