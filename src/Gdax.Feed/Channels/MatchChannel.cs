namespace Gdax.Feed.Channels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class MatchChannel : BaseChannel<PriceMatch>
    {
        internal MatchChannel(params string[] productIds)
            : base("matches", productIds, true)
        {
        }

        protected override bool CanProcess(string type)
        {
            return "match" == type || "last_match" == type;
        }

        protected override void OnNext(JObject message)
        {
            var t = new PriceMatch(message);
            Notify(t);
        }
    }

    public class PriceMatch
    {
        public PriceMatch(JObject message)
        {
            this.Timestamp = message["time"].ToObject<DateTimeOffset>();
            this.ProductId = message.Value<string>("product_id");
            this.Size = message.Value<decimal>("size");
            this.Price = message.Value<decimal>("price");
            this.Side = message.Value<string>("side");
        }

        public DateTimeOffset Timestamp { get; }

        public string ProductId { get; }

        public decimal Size { get; }

        public decimal Price { get; }

        public string Side { get; }
    }

    public static class MatchChannelExtensions
    {
        public static ChannelSubscription<PriceMatch> StartMatchChannel(this GdaxFeedApi api, IObserver<PriceMatch> observer, params string[] productIds)
        {
            return new ChannelSubscription<PriceMatch>(api, () => new MatchChannel(productIds), new IObserver<PriceMatch>[] { observer });
        }

        public static ChannelSubscription<PriceMatch> StartMatchChannel(this GdaxFeedApi api, IEnumerable<IObserver<PriceMatch>> observers, params string[] productIds)
        {
            return new ChannelSubscription<PriceMatch>(api, () => new MatchChannel(productIds), observers);
        }
    }
}
