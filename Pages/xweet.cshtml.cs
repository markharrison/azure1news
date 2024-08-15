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
        static string uploadUrl = "https://upload.twitter.com/1.1/media/upload.json";

        public XweetModel(IWebHostEnvironment env, AppConfig appconfig, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _appconfig = appconfig;
            _httpClientFactory = httpClientFactory;
        }

        //async Task<string?> GetTwitterImageFromUrl(string url)
        //{
        //    try
        //    {
        //        var httpClient = _httpClientFactory.CreateClient();
        //        string htmlContent = await httpClient.GetStringAsync(url);

        //        var htmlDoc = new HtmlDocument();
        //        htmlDoc.LoadHtml(htmlContent);

        //        var twitterImageMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
        //        if (twitterImageMetaTag != null)
        //        {
        //            return twitterImageMetaTag.GetAttributeValue("content", null);
        //        }

        //        var twitterImageSrcMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
        //        if (twitterImageSrcMetaTag != null)
        //        {
        //            return twitterImageSrcMetaTag.GetAttributeValue("content", null);
        //        }

        //        var ogImageMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        //        if (ogImageMetaTag != null)
        //        {
        //            return ogImageMetaTag.GetAttributeValue("content", null);
        //        }

        //        var ogImageSecureUrlMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image:secure_url']");
        //        if (ogImageSecureUrlMetaTag != null)
        //        {
        //            return ogImageSecureUrlMetaTag.GetAttributeValue("content", null);
        //        }

        //        var ogImageUrlMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image:url']");
        //        if (ogImageUrlMetaTag != null)
        //        {
        //            return ogImageUrlMetaTag.GetAttributeValue("content", null);
        //        }

        //        var linkImageSrcTag = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='image_src']");
        //        if (linkImageSrcTag != null)
        //        {
        //            return linkImageSrcTag.GetAttributeValue("href", null);
        //        }

        //        var thumbnailMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='thumbnail']");
        //        if (thumbnailMetaTag != null)
        //        {
        //            return thumbnailMetaTag.GetAttributeValue("content", null);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        string errText = $"Error getting image {ex.Message}";
        //        Console.WriteLine(errText);
        //        return errText;
        //    }

        //    return null;
        //}


        async Task<string> PostXweet(string tweetText, string? imagePath = null)
        {

            dynamic payload = new
            {
                text = tweetText
            };

            try
            {
                //if (!string.IsNullOrEmpty(imagePath))
                //{

                //    byte[] imageData;

                //    if (Uri.IsWellFormedUriString(imagePath, UriKind.Absolute))
                //    {
                //        var httpClient1 = _httpClientFactory.CreateClient();
                //        imageData = await httpClient1.GetByteArrayAsync(imagePath);

                //    }
                //    else
                //    {
                //        return "Error Image Url";
                //    }

                //    var content = new MultipartFormDataContent();
                //    var imageContent = new ByteArrayContent(imageData);
                //    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                //    content.Add(imageContent, "media");

                //    OAuthRequest oAuthRequestUpload =
                //            OAuthRequest.ForProtectedResource("POST", _appconfig.ConsumerKey, _appconfig.ConsumerSecret,
                //                                 _appconfig.AccessToken, _appconfig.AccessTokenSecret);

                //    oAuthRequestUpload.RequestUrl = uploadUrl;

                //    string oAuthHeaderValueUpload = oAuthRequestUpload.GetAuthorizationHeader();

                //    var httpClientUpload = _httpClientFactory.CreateClient();
                //    httpClientUpload.DefaultRequestHeaders.Add("Authorization", oAuthHeaderValueUpload);

                //    var uploadResponse = await httpClientUpload.PostAsync(uploadUrl, content);

                //    if (uploadResponse.IsSuccessStatusCode)
                //    {
                //        var uploadJsonResponse = await uploadResponse.Content.ReadAsStringAsync();

                //        using var uploadDoc = JsonDocument.Parse(uploadJsonResponse);
                //        string mediaId = uploadDoc.RootElement.GetProperty("media_id_string").GetString() ?? string.Empty;

                //        payload = new
                //        {
                //            text = tweetText,
                //            media = new
                //            {
                //                media_ids = new string[] { mediaId }
                //            }
                //        };

                //    }
                //    else
                //    {
                //        string errRsp = $"Error posting media:{uploadResponse.StatusCode}";
                //        Console.WriteLine(errRsp);
                //        return errRsp;
                //    }
                //}

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
                //string? strRsp = await GetTwitterImageFromUrl(strLink);
                //if (strRsp == null || strRsp.ToLower().StartsWith("http"))
                //{
                //    tweetText = strTitle + " " + strLink + Environment.NewLine + textTags;
                //    strStatus = await PostXweet(tweetText, strRsp);
                //}
                //else
                //{
                //    strStatus = strRsp;
                //}

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