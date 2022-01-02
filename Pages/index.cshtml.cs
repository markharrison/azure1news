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

        static readonly HttpClient client = new HttpClient();

        public IndexModel(IWebHostEnvironment env, IMemoryCache MemoryCache)
        {
            _env = env;
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

        private  void doPage()
        {
            string RSSFeedURL = "https://feeds.feedburner.com/azure1dev";

            char[] escape = { ' ', '\r', '\n', '\t' };

            Task<string> task = Task.Run(async () => await getRSS(RSSFeedURL));
            task.Wait();
            _strXML = task.Result;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_strXML);

            XmlNode titleNode = doc.DocumentElement.SelectSingleNode("/rss/channel/title");
            string strFeedTitle = titleNode.InnerText;

            string pubDate = doc.DocumentElement.SelectSingleNode("/rss/channel/pubDate").InnerText.Replace("+0000", "GMT");

            XmlNodeList itemNodes = doc.DocumentElement.SelectNodes("/rss/channel/item");

            int iCtr = 1;
            string strLastDay = "";

            strHTML += "<div class='linktable'><div class='linktablebody'>";
            strHTML += "<div class='linktablerow'>";
            strHTML += "<div class='linktabledaycell'></div>";
            strHTML = strHTML + "<div class='linktableheadercell'><strong>Latest&nbsp;News&nbsp;-&nbsp;" + pubDate + "</strong></div>";
            strHTML += "</div>";

            _strHTMLImages += "<div>";

            foreach (XmlNode itemNode in itemNodes)
            {
                if (iCtr++ > 45) break;

                strHTML += "<div class='linktablerow'>";

                try
                {
                    string strIcon = "fa-link";
                    string strMedia = "";
                    string strMediaType = "";

                    string strTitle = itemNode.SelectSingleNode("title").InnerText;
                    string strLink = itemNode.SelectSingleNode("link").InnerText.Trim(escape);
                    string strDay = itemNode.SelectSingleNode("pubDate").InnerText.ToString().Substring(0, 3);

                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("media", "http://search.yahoo.com/mrss/");


                    if (itemNode.SelectSingleNode("media:content", ns) != null)
                    {
                        strMedia = itemNode.SelectSingleNode("media:content", ns).Attributes["url"].Value;
                        if (itemNode.SelectSingleNode("media:content", ns).Attributes["type"] != null)
                        {
                            strMediaType = itemNode.SelectSingleNode("media:content", ns).Attributes["type"].Value.ToLower();
                        }
                        else if (itemNode.SelectSingleNode("media:content", ns).Attributes["medium"] != null)
                        {
                            strMediaType = itemNode.SelectSingleNode("media:content", ns).Attributes["medium"].Value.ToLower();
                        }
                    }
                    else
                    {
                        if (itemNode.SelectSingleNode("enclosure") != null)
                        {
                            strMedia = itemNode.SelectSingleNode("enclosure").Attributes["url"].Value;
                            if (itemNode.SelectSingleNode("enclosure").Attributes["type"] != null)
                            {
                                strMediaType = itemNode.SelectSingleNode("enclosure").Attributes["type"].Value.ToLower();
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

            strHTML += $"<div class='linktablerow'><div class='linktabledaycell'></div><div class='linktableseparatorcell'><hr /></div></div>";
            strHTML += "</div></div>";

            _strHTMLImages += "</div></div>";

        }

        public void OnGet()
        {
            if (_MemoryCache.TryGetValue("Page", out strHTML) )
            {
                return;
            }

            doPage();

            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _MemoryCache.Set("Page", strHTML, options);
 
        }
    }
}


