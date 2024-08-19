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

                Console.Write(ex.Message);
                throw;
            }

            return _strXML;

        }

        private (bool isMedia, string iconClass) IsPlayableMediaContent(SyndicationItem item)
        {
            bool isMedia = false;
            string iconClass = "fa-link";

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
        private string GetCustomField(SyndicationItem item, string fieldName)
        {
            foreach (SyndicationElementExtension extension in item.ElementExtensions)
            {
                XElement element = extension.GetObject<XElement>();
                if (element.Name.LocalName == fieldName)
                {
                    return element.Value;
                }
            }
            return string.Empty;
        }

        private async Task doPage()
        {
            string RSSFeedURL = _appconfig.FeedUrl;

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

                List<string> imageList = new List<string>();

                string? previousDay = null;

                int imageCount = 0;
                foreach (SyndicationItem item in feed.Items)
                {
                    string strTitle = item.Title.Text;
                    string strLink = item.Links[0].Uri.ToString();
                    string currentDay = item.PublishDate.ToString("ddd");

                    if (imageCount < 6)
                    {
                        string cardimageValue = GetCustomField(item, "cardimage");
                        if (cardimageValue.StartsWith("http"))
                        {
                            imageCount++;
                            imageList.Add(cardimageValue);
                        }
                    }

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
                    htmlBuilder.Append($"<i class='icon-{strIcon} icon-white_{strIcon} iconfap'></i>{strTitle}");
                    htmlBuilder.Append("</div></div>");

                    previousDay = currentDay;

                }

                htmlBuilder.Append("<div class='linktablerow'><div class='linktabledaycell'></div><div class='linktableseparatorcell'><hr /></div></div>");
                htmlBuilder.Append("</div></div>");


                StringBuilder htmlImageBuilder = new StringBuilder();

                if (imageList.Count > 0)
                {
                    htmlImageBuilder.Append("<div class='divimagetable'>");

                    for (int i = 0; i < imageList.Count; i++)
                    {
                        string imageUrl = imageList[i];
                        if (i < imageList.Count)
                        {
                            htmlImageBuilder.Append($"<img class='divimage' src='https://images.weserv.nl/?url={imageUrl}&amp;w=280&h=140&fit=cover&a=attention ' >");  //&w=280&h=140&fit=cover&a=attention
                        }
                    }
                    htmlImageBuilder.Append("</div><br />");

                    strHTML += htmlImageBuilder.ToString();

                }

                strHTML += htmlBuilder.ToString();
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


