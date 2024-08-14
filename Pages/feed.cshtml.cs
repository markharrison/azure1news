using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

namespace Azure1News.Pages
{
    public class FeedModel : PageModel
    {
        private IWebHostEnvironment _env;
        AppConfig _appconfig;
        private readonly IMemoryCache _MemoryCache;
        public string? strFeed = "";
        private readonly IHttpClientFactory _httpClientFactory;

        public FeedModel(IWebHostEnvironment env, IMemoryCache MemoryCache, AppConfig appconfig, IHttpClientFactory httpClientFactory)
            {
            _env = env;
            _appconfig = appconfig;
            _MemoryCache = MemoryCache;
            _httpClientFactory = httpClientFactory;
        }

        private string getPubDate()
        {
            DateTime pubDate = DateTime.UtcNow;
            return pubDate.ToString("ddd',' d MMM yyyy HH':'mm':'ss") + " " + pubDate.ToString("zzzz").Replace(":", "");
        }


        private async Task<string> doGetFeed()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await httpClient.GetAsync(_appconfig.FeedUrl);
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
                pattern = @"<pubDate>(.*?)<\/pubDate>";
                strFeed = new Regex(pattern, options).Replace(strFeed, $"<pubDate>{getPubDate()}</pubDate>", 1);
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