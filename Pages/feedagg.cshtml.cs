using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Azure1News.Pages
{
    public class FeedaggModel : PageModel
    {
        public string? strFeed = "";
        private readonly IMemoryCache _cache;
        private List<SyndicationItem> finalItems = new List<SyndicationItem>();
        private string[] feeds;
        private const string CacheKey = "Feedagg";
        private const int CacheMinutes = 30;
        AppConfig _appconfig;

        public FeedaggModel(AppConfig appconfig, IMemoryCache cache)
        {
            _appconfig = appconfig;
            _cache = cache;
            feeds = _appconfig.RSSFeeds;
        }

        private int CompareDates(SyndicationItem x, SyndicationItem y)
        {
            return y.PublishDate.CompareTo(x.PublishDate);
        }

        public void OnGet()
        {
            string? cachedFeed;
            if (_cache.TryGetValue(CacheKey, out cachedFeed))
            {
                strFeed = cachedFeed;
                return;
            }

            foreach (string feed in feeds)
            {
                try
                {
                    using XmlReader reader = XmlReader.Create(feed);
                    SyndicationFeed syndicationFeed = SyndicationFeed.Load(reader);
                    reader.Close();

                    if (syndicationFeed == null) continue;

                    foreach (var item in syndicationFeed.Items)
                    {
                        SyndicationItem itemx = new SyndicationItem();
                        if (item.Links.Count > 0)
                        {
                            itemx.Links.Add(item.Links[0]);
                        }
                        itemx.Title = item.Title;
                        itemx.PublishDate = item.PublishDate.ToUniversalTime();
 //                       itemx.Summary = item.Summary;

                        finalItems.Add(itemx);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message},  Feed: {feed}");
                }
            }

            finalItems.Sort(CompareDates);
            if (finalItems.Count > 30)
            {
                finalItems = finalItems.Take(30).ToList();
            }

            SyndicationFeed finalFeed = new SyndicationFeed();
            finalFeed.Title = new TextSyndicationContent("Az1News Feed");
            finalFeed.Copyright = new TextSyndicationContent("Copyright (C) Harrison 2025. All rights reserved.");
            finalFeed.Description = new TextSyndicationContent("Harrison Feed");
            finalFeed.Generator = "Harrison Feed Generator";
            finalFeed.LastUpdatedTime = DateTimeOffset.Now.ToUniversalTime();

            finalFeed.Links.Clear(); // Remove any Atom links
            finalFeed.Links.Add(SyndicationLink.CreateAlternateLink(new Uri("https://news.azure1.dev/")));

            finalFeed.Items = finalItems;

            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                var formatter = new Rss20FeedFormatter(finalFeed);
                formatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
                strFeed = stringWriter.ToString();
            }

            _cache.Set(CacheKey, strFeed, TimeSpan.FromMinutes(CacheMinutes));
        }
    }

}
