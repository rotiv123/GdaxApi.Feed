namespace Gdax.Feed.Utils
{
    using System;

    internal class NullLogger : IGdaxFeedApiLogger
    {
        public void ErrorAsync(DateTimeOffset time, string text, Exception ex)
        {
        }
    }
}
