using System.Text.Json;

namespace Azure1News
{
    public class AppConfig
    {
        private string _FeedUrlVal;
        private string _FeedMUrlVal;
        private string _AdminPWVal;
        private string _GoogleIdVal;
        private string _ConsumerKeyVal;
        private string _ConsumerSecretVal;
        private string _AccessTokenVal;
        private string _AccessTokenSecretVal;
        private string[] _RSSFeeds;

        public AppConfig(IConfiguration _config)
        {
            _FeedUrlVal = _config.GetValue<string>("FeedUrl") ?? "";
            _FeedMUrlVal = _config.GetValue<string>("FeedMUrl") ?? "";
            _AdminPWVal = _config.GetValue<string>("AdminPW") ?? "";
            _GoogleIdVal = _config.GetValue<string>("GoogleId") ?? "";
            _ConsumerKeyVal = _config.GetValue<string>("ConsumerKey") ?? "";
            _ConsumerSecretVal = _config.GetValue<string>("ConsumerSecret") ?? "";
            _AccessTokenVal = _config.GetValue<string>("AccessToken") ?? "";
            _AccessTokenSecretVal = _config.GetValue<string>("AccessTokenSecret") ?? "";

            try
            {
                string json = File.ReadAllText("rssfeeds.json");
                using JsonDocument document = JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty("feeds", out JsonElement feedsElement) &&
                    feedsElement.ValueKind == JsonValueKind.Array)
                {
                    List<string> feedUrls = new();

                    foreach (JsonElement feed in feedsElement.EnumerateArray())
                    {
                        if (feed.TryGetProperty("url", out JsonElement urlElement) &&
                            urlElement.ValueKind == JsonValueKind.String)
                        {
                            string? url = urlElement.GetString();
                            if (url != null)
                            {
                                feedUrls.Add(url);
                            }
                        }
                    }

                    _RSSFeeds = feedUrls.ToArray();
                }
                else
                {
                    _RSSFeeds = Array.Empty<string>();
                }
            }
            catch
            {
                _RSSFeeds = Array.Empty<string>();
            }

        }
        public string FeedUrl 
        {
            get => this._FeedUrlVal;
            set => this._FeedUrlVal = value;
        }
        public string FeedMUrl
        {
            get => this._FeedMUrlVal;
            set => this._FeedMUrlVal = value;
        }
        public string AdminPW
        {
            get => this._AdminPWVal;
            set => this._AdminPWVal = value;
        }
        public string GoogleId
        {
            get => this._GoogleIdVal;
            set => this._GoogleIdVal = value;
        }
        public string ConsumerKey
        {
            get => this._ConsumerKeyVal;
            set => this._ConsumerKeyVal = value;
        }
        public string ConsumerSecret
        {
            get => this._ConsumerSecretVal;
            set => this._ConsumerSecretVal = value;
        }
        public string AccessToken
        {
            get => this._AccessTokenVal;
            set => this._AccessTokenVal = value;
        }
        public string AccessTokenSecret
        {
            get => this._AccessTokenSecretVal;
            set => this._AccessTokenSecretVal = value;
        }

        public string[] RSSFeeds
        {
            get => this._RSSFeeds;
            set => this._RSSFeeds = value;
        }
  }
}
