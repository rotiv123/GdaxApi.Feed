namespace Gdax.Feed
{
    using System;
    using System.Runtime.Serialization;

    public class GdaxFeedApiException : Exception
    {
        public GdaxFeedApiException()
            : base()
        {
        }

        public GdaxFeedApiException(string message)
            : base(message)
        {
        }

        public GdaxFeedApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GdaxFeedApiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
