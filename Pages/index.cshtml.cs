using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Xml;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Azure1News.Pages
{
    [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, NoStore = false)]
    public class IndexModel : PageModel
    {
        private IWebHostEnvironment _env;
        private readonly IMemoryCache _MemoryCache;

        public string strHTML = "";
        private string _strHTMLImages = "";
        private int _iImgIdx = 0;
        AppConfig _appconfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IWebHostEnvironment env, IMemoryCache MemoryCache, AppConfig appconfig, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _appconfig = appconfig;
            _MemoryCache = MemoryCache;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> getRSS(String uri)
        {
            string _strXML;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                _strXML = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {

                Debug.Write(ex.Message);
                throw;
            }

            return _strXML;

        }

        private (bool isMedia, string iconClass) IsPlayableMediaContent(SyndicationItem item)
        {
            bool isMedia = false;
            string iconClass = "fa-link";

            // Check for media content in Element Extensions
            foreach (SyndicationElementExtension extension in item.ElementExtensions)
            {
                if (extension.OuterName == "content" && extension.OuterNamespace == "http://search.yahoo.com/mrss/")
                {
                    XElement mediaElement = extension.GetObject<XElement>();
                    string mediaType = mediaElement.Attribute("type")?.Value ?? string.Empty;
                    string mediaUrl = mediaElement.Attribute("url")?.Value ?? string.Empty;
                    if (mediaType.StartsWith("video") || mediaType.StartsWith("audio") || mediaUrl.Contains("youtube.com") || mediaUrl.Contains("youtu.be"))
                    {
                        isMedia = true;
                        iconClass = "fa-videoplay"; // Media icon
                        break;
                    }
                }
            }

            // Check for media content in Links with 'enclosure' relationship
            if (!isMedia)
            {
                foreach (SyndicationLink link in item.Links)
                {
                    if (link.RelationshipType == "enclosure" && (link.MediaType.StartsWith("video") || link.MediaType.StartsWith("audio") || link.Uri.ToString().Contains("youtube.com") || link.Uri.ToString().Contains("youtu.be")))
                    {
                        isMedia = true;
                        iconClass = "fa-videoplay"; // Media icon
                        break;
                    }
                }
            }

            // Check for YouTube links in the main link
            if (!isMedia)
            {
                foreach (SyndicationLink link in item.Links)
                {
                    if (link.Uri.ToString().Contains("youtube.com") || link.Uri.ToString().Contains("youtu.be"))
                    {
                        isMedia = true;
                        iconClass = "fa-videoplay"; // Media icon
                        break;
                    }
                }
            }

            return (isMedia, iconClass);
        }

        static bool IsImageUrl(string url)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return imageExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        static string? ExtractImageUrl(SyndicationItem item)
        {
            var imageLink = item.Links.FirstOrDefault(link => link.RelationshipType == "enclosure" && link.MediaType.StartsWith("image"));
            if (imageLink != null)
            {
                return imageLink.Uri.ToString();
            }

            foreach (var extension in item.ElementExtensions)
            {
                if (extension.OuterName == "content" || extension.OuterName == "thumbnail")
                {
                    XElement mediaElement = extension.GetObject<XElement>();
                    string imageUrl = mediaElement.Attribute("url")?.Value ?? string.Empty;
                    if (IsImageUrl(imageUrl))
                    {
                        return imageUrl;
                    }
                }
            }

            string content = item.Summary?.Text ?? item.Content?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(content))
            {
                var match = Regex.Match(content, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string imageUrl = match.Groups[1].Value;
                    if (IsImageUrl(imageUrl))
                    {
                        return imageUrl;
                    }
                }
            }

            return null;
        }

        private async Task doPage()
        {
            List<string> imageUrls = new List<string>();
            string RSSFeedURL = _appconfig.FeedUrl;
              RSSFeedURL = "https://feeds.feedburner.com/WatfordFC";

            string rssContent = await getRSS(RSSFeedURL);

            using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(rssContent)))
            {
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                StringBuilder htmlBuilder = new StringBuilder();

                string timeNow = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");

                htmlBuilder.Append("<div class='linktable'><div class='linktablebody'>");
                htmlBuilder.Append("<div class='linktablerow'>");
                htmlBuilder.Append("<div class='linktabledaycell'></div>");
                htmlBuilder.Append("<div class='linktableheadercell'><strong>Latest&nbsp;News&nbsp;-&nbsp;" + timeNow + "</strong></div>");
                htmlBuilder.Append("</div>");

                string? previousDay = null;

                foreach (SyndicationItem item in feed.Items)
                {
                    string strTitle = item.Title.Text;
                    string strLink = item.Links[0].Uri.ToString();
                    string currentDay = item.PublishDate.ToString("ddd");

                    if (previousDay != null && currentDay != previousDay)
                    {
                        htmlBuilder.Append("<div class='linktablerow'><div class='linktabledaycell'></div><div class='linktableseparatorcell'><hr /></div></div>");
                    }

                    htmlBuilder.Append("<div class='linktablerow'><div class='linktabledaycell'>");
                    if (previousDay == null || currentDay != previousDay)
                    {
                        htmlBuilder.Append($"{currentDay}:&nbsp;");
                    }
                    htmlBuilder.Append("</div>");

                    var (isMedia, strIcon) = IsPlayableMediaContent(item);

                    htmlBuilder.Append($"<div onclick='window.open(\"{strLink}\", \"_blank\"); return false;' class='linktablearticlecell'>");
                    htmlBuilder.Append($"<i class='icon-{strIcon} icon-white_{strIcon} iconfap'></i>{strTitle}"  );
                    htmlBuilder.Append("</div></div>");

                    previousDay = currentDay;

                    if (imageUrls.Count < 6)
                    {
                        string? imageUrl = ExtractImageUrl(item);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            imageUrls.Add(imageUrl);
                        }
                    }
                }

                htmlBuilder.Append("<div class='linktablerow'><div class='linktabledaycell'></div><div class='linktableseparatorcell'><hr /></div></div>");
                htmlBuilder.Append("</div></div>");

                strHTML = htmlBuilder.ToString();
            }
        }



        public async Task OnGetAsync()
        {
            strHTML = string.Empty;
            if (_MemoryCache.TryGetValue("Page", out string? cachedHTML))
            {
                strHTML = cachedHTML ?? string.Empty;
                return;
            }

            await doPage();

            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _MemoryCache.Set("Page", strHTML, options);

        }
    }
}


