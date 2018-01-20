# GdaxApi.Feed
The GDAX Websocket Feed provides real-time market data updates for orders and trades.

[![Build](https://ci.appveyor.com/api/projects/status/dwrbqqulrl1u6v7g?svg=true)](https://ci.appveyor.com/project/rotiv123/gdaxapi-feed) [![NuGet](https://img.shields.io/nuget/v/Gdax.Feed.svg)](https://www.nuget.org/packages/Gdax.Feed/)


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
