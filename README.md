# GdaxApi.Feed
The GDAX Websocket Feed provides real-time market data updates for orders and trades.

## Quick Start

```cs
// ...
using (var client = new ClientWebSocket())
{
    using (var feed = new GdaxFeedApi(client, new MyErrorLogger()))
    {
        var task = feed.RunLoopAsync(cts.Token);

        using (feed.StartMatchChannel(new MyPriceObserver(), "BTC-EUR"))
        {
			await task;
        }
    }
}
// ...

private class MyErrorLogger : IGdaxFeedApiLogger
{
    public async void ErrorAsync(DateTimeOffset time, string text, Exception ex)
    {
        await Console.Out.WriteLineAsync($"{time} {text} {ex}").ConfigureAwait(false);
    }
}

private class MyPriceObserver : IObserver<PriceMatch>
{
    // ...
}
```
