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
            Notify(new PriceMatch(
                        message["time"].ToObject<DateTimeOffset>(),
                        message.Value<string>("product_id"),
                        message.Value<decimal>("size"),
                        message.Value<decimal>("price"),
                        message.Value<string>("side")));
        }
    }

    public class PriceMatch
    {
        public PriceMatch(DateTimeOffset timestamp, string productId, decimal size, decimal price, string side)
        {
            this.Timestamp = timestamp;
            this.ProductId = productId;
            this.Size = size;
            this.Price = price;
            this.Side = side;
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
