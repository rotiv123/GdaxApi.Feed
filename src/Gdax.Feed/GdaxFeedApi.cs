namespace Gdax.Feed
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Gdax.Feed.Subscriptions;
    using Gdax.Feed.Utils;

    public class GdaxFeedApi : IDisposable, IObservable<JObject>
    {
        private static readonly Uri BaseUri = new Uri("wss://ws-feed.gdax.com");

        private readonly IGdaxFeedApiLogger logger;
        private readonly ClientWebSocket client;
        private readonly ObservableManager<JObject> observableManager;
        private readonly BlockingCollection<ApiFeedRequest> requests;
        private readonly BlockingCollection<ApiFeedResponse> responses;

        public GdaxFeedApi(ClientWebSocket client)
            : this(client, null)
        {
        }

        public GdaxFeedApi(ClientWebSocket client, IGdaxFeedApiLogger logger)
        {
            this.logger = logger ?? new NullLogger();
            this.client = client;
            this.SubscriptionManager = new SubscriptionManager(this);
            this.observableManager = new ObservableManager<JObject>();
            this.requests = new BlockingCollection<ApiFeedRequest>();
            this.responses = new BlockingCollection<ApiFeedResponse>();
        }

        public async Task RunLoopAsync(CancellationToken ct)
        {
            await EnsureConnectionAsync(ct).ConfigureAwait(false);
            await Task.WhenAll(SendLoop(ct), ReceiveLoop(ct), DispatchLoop(ct)).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal SubscriptionManager SubscriptionManager { get; }

        internal void SendAsync(ApiFeedRequest request)
        {
            this.requests.Add(request);
        }

        IDisposable IObservable<JObject>.Subscribe(IObserver<JObject> observer)
        {
            return this.observableManager.Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.observableManager.Dispose();
                this.requests.Dispose();
                this.responses.Dispose();
            }
        }

        private async Task SendLoop(CancellationToken ct)
        {
            await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    foreach (var request in this.requests.GetConsumingEnumerable(ct))
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            try
                            {
                                await this.client.SendAsync(new ArraySegment<byte>(request.Content), request.MessageType, true, ct).ConfigureAwait(false);
                                break;
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                this.logger.ErrorAsync(DateTimeOffset.UtcNow, "SendLoop FAIL", ex);
                                await EnsureConnectionAsync(ct).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        private async Task ReceiveLoop(CancellationToken ct)
        {
            var buffer = new byte[1024 * 8];
            var offset = 0;
            var count = buffer.Length;
            var maxSegment = new ArraySegment<byte>(buffer, offset, count);
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = await this.client.ReceiveAsync(maxSegment, ct).ConfigureAwait(false);
                        offset += result.Count;
                        while (!result.EndOfMessage)
                        {
                            count -= offset;
                            result = await this.client.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), ct).ConfigureAwait(false);
                            offset += result.Count;
                        }

                        var response = new ApiFeedResponse(buffer, 0, offset);
                        Array.Clear(buffer, 0, offset);
                        offset = 0;
                        count = buffer.Length;

                        this.responses.Add(response);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        this.logger.ErrorAsync(DateTimeOffset.UtcNow, "ReceiveLoop FAIL", ex);
                        await EnsureConnectionAsync(ct).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DispatchLoop(CancellationToken ct)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var response in this.responses.GetConsumingEnumerable(ct))
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }

                        ParallelNotify(response);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        private void ParallelNotify(ApiFeedResponse response)
        {
            var msg = response.JContent;
            var type = msg.Value<string>("type");
            if ("error" == type)
            {
                var message = msg.Value<string>("message");
                var reason = msg.Value<string>("reason");
                this.observableManager.ParallelFaultAsync(new GdaxFeedApiException($"{message}. {reason}."));
            }

            this.observableManager.ParallelNotifyAsync(msg);
        }

        private async Task EnsureConnectionAsync(CancellationToken ct)
        {
            if (this.client.State != WebSocketState.Connecting && this.client.State != WebSocketState.Open)
            {
                try
                {
                    await this.client.ConnectAsync(BaseUri, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    this.logger.ErrorAsync(DateTimeOffset.UtcNow, "EnsureConnectionAsync FAIL", ex);
                }
            }
        }
    }
}
