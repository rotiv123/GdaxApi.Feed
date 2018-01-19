namespace Gdax.Feed
{
    using System.Text;
    using Newtonsoft.Json.Linq;

    public class ApiFeedResponse
    {
        private JObject jobject;

        public ApiFeedResponse(byte[] buffer, int offset, int count)
        {
            this.Content = Encoding.UTF8.GetString(buffer, offset, count);
            this.jobject = null;
        }

        public string Content { get; }

        public JObject JContent
        {
            get
            {
                if (this.jobject == null)
                {
                    this.jobject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(this.Content);
                }

                return this.jobject;
            }
        }
    }
}
