using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Xml;
using System.Net.Http;

namespace Azure1News.Pages
{
    [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, NoStore = false)]
    public class IndexModel : PageModel
    {
        private IWebHostEnvironment _env;
        private readonly IMemoryCache _MemoryCache;

        public string strHTML = "";
        private string _strHTMLImages = "";
        private string _strXML = "";
        private int _iImgIdx = 0;
        AppConfig _appconfig;

        static readonly HttpClient client = new HttpClient();

        public IndexModel(IWebHostEnvironment env, IMemoryCache MemoryCache, AppConfig appconfig)
        {
            _env = env;
            _appconfig = appconfig;
            _MemoryCache = MemoryCache;
        }

        private async Task<string> getRSS(String uri)
        {
            string _strXML;

            try
            {

                HttpResponseMessage response = await client.GetAsync(uri);
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

        private void doPage()
        {
            string RSSFeedURL = _appconfig.FeedUrl;

            char[] escape = { ' ', '\r', '\n', '\t' };

            Task<string> task = Task.Run(async () => await getRSS(RSSFeedURL));
            task.Wait();
            _strXML = task.Result;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_strXML);

            XmlNode? titleNode = doc.DocumentElement?.SelectSingleNode("/rss/channel/title");
            if (titleNode != null)
            {
                string strFeedTitle = titleNode.InnerText;
            }

            string pubDate = doc.DocumentElement?.SelectSingleNode("/rss/channel/pubDate")?.InnerText.Replace("+0000", "GMT") ?? "Unknown Date";

            XmlNodeList? itemNodes = doc.DocumentElement?.SelectNodes("/rss/channel/item");

            int iCtr = 1;
            string strLastDay = "";

            strHTML += "<div class='linktable'><div class='linktablebody'>";
            strHTML += "<div class='linktablerow'>";
            strHTML += "<div class='linktabledaycell'></div>";
            strHTML = strHTML + "<div class='linktableheadercell'><strong>Latest&nbsp;News&nbsp;-&nbsp;" + pubDate + "</strong></div>";
            strHTML += "</div>";

            _strHTMLImages += "<div>";

            if (itemNodes != null)
            {
                foreach (XmlNode itemNode in itemNodes)
                {
                    if (iCtr++ > 45) break;

                    strHTML += "<div class='linktablerow'>";

                    try
                    {
                        string strIcon = "fa-link";
                        string strMedia = "";
                        string strMediaType = "";

                        XmlNode? titlexNode = itemNode.SelectSingleNode("title");
                        string strTitle = titlexNode?.InnerText ?? "No Title";

                        XmlNode? linkNode = itemNode.SelectSingleNode("link");
                        string strLink = linkNode?.InnerText.Trim(escape) ?? "No Link";

                        XmlNode? pubDateNode = itemNode.SelectSingleNode("pubDate");
                        string strDay = pubDateNode?.InnerText.Substring(0, 3) ?? "No Date";

                        XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                        ns.AddNamespace("media", "http://search.yahoo.com/mrss/");

                        XmlNode? mediaContentNode = itemNode.SelectSingleNode("media:content", ns);
                        if (mediaContentNode != null)
                        {
                            XmlAttribute? urlAttribute = mediaContentNode.Attributes?["url"];
                            if (urlAttribute != null)
                            {
                                strMedia = urlAttribute.Value;
                            }

                            XmlAttribute? typeAttribute = mediaContentNode.Attributes?["type"];
                            if (typeAttribute != null)
                            {
                                strMediaType = typeAttribute.Value.ToLower();
                            }
                            else
                            {
                                XmlAttribute? mediumAttribute = mediaContentNode.Attributes?["medium"];
                                if (mediumAttribute != null)
                                {
                                    strMediaType = mediumAttribute.Value.ToLower();
                                }
                            }
                        }
                        else
                        {
                            XmlNode? enclosureNode = itemNode.SelectSingleNode("enclosure");
                            if (enclosureNode != null)
                            {
                                XmlAttribute? urlAttribute = enclosureNode.Attributes?["url"];
                                if (urlAttribute != null)
                                {
                                    strMedia = urlAttribute.Value;
                                }

                                XmlAttribute? typeAttribute = enclosureNode.Attributes?["type"];
                                if (typeAttribute != null)
                                {
                                    strMediaType = typeAttribute.Value.ToLower();
                                }
                            }
                        }

                        if (strMedia != "")
                        {
                            if (strMediaType == "audio/mpeg" || strMediaType == "video/mp4" || strMediaType == "application/x-shockwave-flash")
                            {
                                strIcon = "fa-videoplay";
                            }

                            if (strMediaType.StartsWith("image"))
                            {
                                if (!(strMedia.Contains("gravatar.com")))
                                {
                                    if (_iImgIdx < 6)
                                    {
                                        _iImgIdx++;

                                        _strHTMLImages += "<div class='azn-itemimagecell'>";
                                        _strHTMLImages += "<div class='azn-itemimagecontainer'>";
                                        _strHTMLImages += "<img class='azn-itemimage' src='" + strMedia + "' />";
                                        _strHTMLImages += "</div>";
                                        _strHTMLImages += "</div>";

                                        if (_iImgIdx == 3)
                                        {
                                            _strHTMLImages += "</div><div class='azn-itemrow'>";
                                        }
                                    }
                                }
                            }
                        }

                        strHTML += $"<div class='linktabledaycell'>";
                        if (strDay != strLastDay)
                        {
                            if (strLastDay != "")
                            {
                                strHTML += "</div><div class='linktableseparatorcell'><hr /></div></div>";
                                strHTML += $"<div class='linktablerow'><div class='linktabledaycell'>";
                            }

                            strHTML += strDay + "&nbsp;:&nbsp";
                        }

                        strHTML += "</div>";
                        strLastDay = strDay;


                        strHTML += $"<div onclick='window.open(\"{strLink}\", \"_blank\"); return false;' class='linktablearticlecell'>";
                        strHTML += $"<i class='icon-{strIcon} icon-white_{strIcon} iconfap'></i>" + strTitle;
                        strHTML += "</div>";

                    }
                    catch
                    {
                        // do nothing
                    }

                    strHTML += "</div>";

                }
            }

            strHTML += $"<div class='linktablerow'><div class='linktabledaycell'></div><div class='linktableseparatorcell'><hr /></div></div>";
            strHTML += "</div></div>";

            _strHTMLImages += "</div></div>";


        }

        public void OnGet()
        {
            strHTML = string.Empty;
            if (_MemoryCache.TryGetValue("Page", out string? cachedHTML))
            {
                strHTML = cachedHTML ?? string.Empty;
                return;
            }

            doPage();

            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _MemoryCache.Set("Page", strHTML, options);

        }
    }
}


