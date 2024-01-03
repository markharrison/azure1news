namespace Azure1News
{
    public class AppConfig
    {

        private string _FeedUrlVal;
        private string _AdminPWVal;
        private string _GoogleIdVal;
        public AppConfig(IConfiguration _config)
        {
            _FeedUrlVal = _config.GetValue<string>("FeedUrl");
            _AdminPWVal = _config.GetValue<string>("AdminPW");
            _GoogleIdVal = _config.GetValue<string>("GoogleId");
        }
        public string FeedUrl 
        {
            get => this._FeedUrlVal;
            set => this._FeedUrlVal = value;
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
    }
}
