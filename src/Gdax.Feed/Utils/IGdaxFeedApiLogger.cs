namespace Gdax.Feed.Utils
{
    using System;

    public interface IGdaxFeedApiLogger
    {
        void ErrorAsync(DateTimeOffset time, string text, Exception ex);
    }
}
