namespace Gdax.Feed.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class SubscriptionManager
    {
        private readonly GdaxFeedApi api;
        private readonly Dictionary<string, Dictionary<string, int>> state;

        internal SubscriptionManager(GdaxFeedApi api)
        {
            this.api = api;
            this.state = new Dictionary<string, Dictionary<string, int>>();
        }

        public void SubscribeAsync(SubscriptionRequest subscription)
        {
            var channelSubscriptions = new Dictionary<string, List<string>>();

            foreach (var channel in subscription.Channels)
            {
                if (this.state.ContainsKey(channel))
                {
                    UpdateExistingSubscriptionsCount(subscription, channelSubscriptions, channel);
                }
                else
                {
                    InitNewSubscriptionsCount(subscription, channelSubscriptions, channel);
                }
            }

            if (channelSubscriptions.Count > 0)
            {
                var data = new
                {
                    type = "subscribe",
                    channels = from x in channelSubscriptions
                               select new
                               {
                                   name = x.Key,
                                   product_ids = x.Value
                               }
                };

                var req = new ApiFeedRequest(data);
                this.api.SendAsync(req);
            }
        }

        public void UnsubscribeAsync(SubscriptionRequest subscription)
        {
            var removeList = new List<Tuple<string, string>>();

            foreach (var channel in subscription.Channels)
            {
                if (this.state.ContainsKey(channel))
                {
                    foreach (var productId in subscription.ProductIds)
                    {
                        if (this.state[channel].ContainsKey(productId))
                        {
                            this.state[channel][productId]--;
                            if (this.state[channel][productId] <= 0)
                            {
                                this.state[channel].Remove(productId);
                                removeList.Add(Tuple.Create(channel, productId));
                            }
                        }
                    }
                }
            }

            var grooupedList = from x in removeList
                               group x by x.Item1 into g
                               select new
                               {
                                   type = "unsubscribe",
                                   product_ids = g.Select(y => y.Item2),
                                   channels = new string[] { g.Key }
                               };

            foreach (var x in grooupedList)
            {
                var req = new ApiFeedRequest(x);
                this.api.SendAsync(req);
            }
        }

        private void InitNewSubscriptionsCount(SubscriptionRequest subscription, Dictionary<string, List<string>> channelSubscriptions, string channel)
        {
            this.state.Add(channel, new Dictionary<string, int>());
            foreach (var productId in subscription.ProductIds)
            {
                this.state[channel].Add(productId, 1);
                if (channelSubscriptions.ContainsKey(channel))
                {
                    channelSubscriptions[channel].Add(productId);
                }
                else
                {
                    channelSubscriptions.Add(channel, new List<string> { productId });
                }
            }
        }

        private void UpdateExistingSubscriptionsCount(SubscriptionRequest subscription, Dictionary<string, List<string>> channelSubscriptions, string channel)
        {
            foreach (var productId in subscription.ProductIds)
            {
                if (this.state[channel].ContainsKey(productId))
                {
                    this.state[channel][productId]++;
                }
                else
                {
                    this.state[channel].Add(productId, 1);
                    if (channelSubscriptions.ContainsKey(channel))
                    {
                        channelSubscriptions[channel].Add(productId);
                    }
                    else
                    {
                        channelSubscriptions.Add(channel, new List<string> { productId });
                    }
                }
            }
        }
    }
}
