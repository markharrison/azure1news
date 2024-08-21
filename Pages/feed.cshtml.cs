using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Azure1News.Pages
{
    public class FeedModel : PageModel
    {
        private IWebHostEnvironment _env;
        AppConfig _appconfig;
        private readonly IMemoryCache _MemoryCache;
        public string? strFeed = "";
        private readonly IHttpClientFactory _httpClientFactory;
        int imageCount = 0;

        public FeedModel(IWebHostEnvironment env, IMemoryCache MemoryCache, AppConfig appconfig, IHttpClientFactory httpClientFactory)
            {
            _env = env;
            _appconfig = appconfig;
            _MemoryCache = MemoryCache;
            _httpClientFactory = httpClientFactory;
            imageCount = 0;
        }

        private string getPubDate()
        {
            DateTime pubDate = DateTime.UtcNow;
            return pubDate.ToString("ddd',' d MMM yyyy HH':'mm':'ss") + " " + pubDate.ToString("zzzz").Replace(":", "");
        }

        private string GetCardImage(string link)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                string htmlContent =  httpClient.GetStringAsync(link).GetAwaiter().GetResult();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var ogImageMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                if (ogImageMetaTag != null)
                {
                    return ogImageMetaTag.GetAttributeValue("content", null);
                }

                var twitterImageMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
                if (twitterImageMetaTag != null)
                {
                    return twitterImageMetaTag.GetAttributeValue("content", null);
                }

                var twitterImageSrcMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
                if (twitterImageSrcMetaTag != null)
                {
                    return twitterImageSrcMetaTag.GetAttributeValue("content", null);
                }

                var ogImageSecureUrlMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image:secure_url']");
                if (ogImageSecureUrlMetaTag != null)
                {
                    return ogImageSecureUrlMetaTag.GetAttributeValue("content", null);
                }

                var ogImageUrlMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image:url']");
                if (ogImageUrlMetaTag != null)
                {
                    return ogImageUrlMetaTag.GetAttributeValue("content", null);
                }

                var linkImageSrcTag = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='image_src']");
                if (linkImageSrcTag != null)
                {
                    return linkImageSrcTag.GetAttributeValue("href", null);
                }

                var thumbnailMetaTag = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='thumbnail']");
                if (thumbnailMetaTag != null)
                {
                    return thumbnailMetaTag.GetAttributeValue("content", null);
                }

            }
            catch (Exception ex)
            {
                string errText = $"Error getting image {ex.Message}";
                Console.WriteLine(errText);
                return errText;
            }

            return "Null";
        }

        private string InsertCardImage(string link)
        {
            if (imageCount >= 6)
            {
                return "Null";
            }

            if (_MemoryCache.TryGetValue(link, out string? customValue))
            {
                return customValue ?? "Null";
            }

            customValue = GetCardImage(link);  // /i0.wp.com/
            customValue = customValue.Replace("/i0.wp.com/", "/");
            customValue = System.Net.WebUtility.UrlEncode(customValue);

            _MemoryCache.Set(link, customValue, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(7)));

            if (customValue.StartsWith("http"))
            {
                imageCount++;
            }


            return customValue;
        }


        private async Task<string> doGetFeed()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await httpClient.GetAsync(_appconfig.FeedMUrl);
                response.EnsureSuccessStatusCode();
                strFeed = await response.Content.ReadAsStringAsync();

                strFeed = strFeed.Replace(_appconfig.FeedUrl, "https://news.azure1.dev/feed")
                    .Replace("xmlns:trackback=\"http://madskills.com/public/xml/rss/module/trackback/\"", "")
                    .Replace("xmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"", "")
                    .Replace("xmlns:itms=\"http://phobos.apple.com/rss/1.0/modules/itms/\"", "")
                    .Replace("xmlns:georss=\"http://www.georss.org/georss\"", "")
                    .Replace("xmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\" ", "");

                string pattern;
                RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;

                pattern = @"<\?xml(.*?)>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<\?xml-stylesheet(.*?)>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");

                pattern = @"<generator>(.*?)<\/generator>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "<generator>azure1.dev</generator>");
                pattern = @"<link>(.*?)<\/link>";
                strFeed = new Regex(pattern, options).Replace(strFeed, $"<link>https://azure1.dev</link>", 1);
                pattern = @"<pubDate>(.*?)<\/pubDate>";
                strFeed = new Regex(pattern, options).Replace(strFeed, $"<pubDate>{getPubDate()}</pubDate>", 1);
                pattern = @"<atom:link(.*?)\/>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<content:encoded>(.*?)<\/content:encoded>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:author(.*?)<\/itunes:author>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:duration(.*?)<\/itunes:duration>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:summary(.*?)<\/itunes:summary>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:explicit(.*?)<\/itunes:explicit>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:keywords(.*?)<\/itunes:keywords>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<itunes:subtitle(.*?)<\/itunes:subtitle>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");
                pattern = @"<description>(.*?)<\/description>";
                strFeed = new Regex(pattern, options).Replace(strFeed, "");

                imageCount = 0;
                pattern = @"(<item\b[^>]*>)(.*?)(<\/item>)";
                strFeed = new Regex(pattern, options).Replace(strFeed, match =>
                {
                    string itemContent = match.Groups[2].Value;
                    string linkPattern = @"<link>(.*?)<\/link>";
                    string link = new Regex(linkPattern, options).Match(itemContent).Groups[1].Value;
                    string cardimageValue = InsertCardImage(link);
                    string cardimageTag = $"<cardimage>{cardimageValue}</cardimage>";
                    return $"{match.Groups[1].Value}{itemContent}{cardimageTag}{match.Groups[3].Value}";
                });

                strFeed = Regex.Replace(strFeed, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();

            }
            catch (Exception ex)
            {
                strFeed = ex.Message;
            }

            return strFeed;
        }

        public async Task OnGetAsync()
        {      
                     
            if (_MemoryCache.TryGetValue("Feed", out strFeed))
            {
                return;
            }

            await doGetFeed();

            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _MemoryCache.Set("Feed", strFeed, options);

        }
    }
}