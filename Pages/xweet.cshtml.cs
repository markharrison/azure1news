using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OAuth;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Azure1News.Pages
{

    public class XweetModel : PageModel
    {
        private IWebHostEnvironment _env;
        AppConfig _appconfig;
        public string strResponse = "";
        private readonly IHttpClientFactory _httpClientFactory;

        static string tweetUrl = "https://api.twitter.com/2/tweets";
        // static string uploadUrl = "https://upload.twitter.com/1.1/media/upload.json";

        public XweetModel(IWebHostEnvironment env, AppConfig appconfig, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _appconfig = appconfig;
            _httpClientFactory = httpClientFactory;
        }

        async Task<string> PostXweet(string tweetText, string? imagePath = null)
        {

            dynamic payload = new
            {
                text = tweetText
            };

            try
            {
   
                var tweetContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                OAuthRequest oAuthRequestTweet =
                        OAuthRequest.ForProtectedResource("POST", _appconfig.ConsumerKey, _appconfig.ConsumerSecret,
                 _appconfig.AccessToken, _appconfig.AccessTokenSecret);

                oAuthRequestTweet.RequestUrl = tweetUrl;

                string oAuthHeaderValueTweet = oAuthRequestTweet.GetAuthorizationHeader();

                var httpClientTweet = _httpClientFactory.CreateClient();
                httpClientTweet.DefaultRequestHeaders.Add("Authorization", oAuthHeaderValueTweet);
                httpClientTweet.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var tweetResponse = await httpClientTweet.PostAsync(tweetUrl, tweetContent);
                if (tweetResponse.IsSuccessStatusCode)
                {
                    var tweetJsonResponse = await tweetResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    string errRsp = $"Error posting tweet:{tweetResponse.StatusCode}";
                    Console.WriteLine(errRsp);
                    return errRsp;
                }

            }
            catch (Exception ex)
            {
                string errText = $"Error {ex.Message}";
                Console.WriteLine(errText);
                return errText;
            }

            return "Xweet OK";
        }


        public async Task OnGetAsync()
        {

            string strStatus = "OK";
            string strTitle = "";
            string strLink = "";
            string tweetText = "";
            string textTags = "#Microsoft #Azure #AppDev";

            var query = Request.Query.ToDictionary(k => k.Key.ToLower(),
                v => v.Value.ToString());

            if (query.ContainsKey("link"))
            {
                strLink = query["link"].Trim();
            }
            if (query.ContainsKey("title"))
            {
                strTitle = query["title"].Trim().Replace("?", "").Replace("&", "");
            }

            if (!string.IsNullOrEmpty(strLink) && !string.IsNullOrEmpty(strTitle))
            {
                tweetText = strTitle + " " + strLink + Environment.NewLine + textTags;
                strStatus = await PostXweet(tweetText);
            }

            var response = new
            {
                status = strStatus,
                date = DateTime.UtcNow.ToString("ddd',' d MMM yyyy HH':'mm':'ss"),
                title = strTitle,
                link = strLink
            };

            strResponse = JsonSerializer.Serialize(response);

            await Task.Run(() => { });

        }
    }
}