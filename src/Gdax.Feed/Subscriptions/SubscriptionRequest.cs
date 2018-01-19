namespace Gdax.Feed.Subscriptions
{
    using System.Collections.Generic;

    public class SubscriptionRequest
    {
        public SubscriptionRequest()
        {
            this.ProductIds = new List<string>();
            this.Channels = new List<string>();
        }

        public List<string> ProductIds { get; set; }

        public List<string> Channels { get; set; }
    }
}
