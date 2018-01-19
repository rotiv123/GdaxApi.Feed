namespace Gdax.Feed
{
    using System.Net.WebSockets;

    public class ApiFeedRequest
    {
        public ApiFeedRequest(object content)
        {
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            this.Content = System.Text.Encoding.UTF8.GetBytes(data);
        }

        public WebSocketMessageType MessageType => WebSocketMessageType.Text;

        public byte[] Content { get; }
    }
}
