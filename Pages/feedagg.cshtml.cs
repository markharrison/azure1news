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
        private const int CacheMinutes = 60;
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
                XmlReader reader = XmlReader.Create(feed);
                Rss20FeedFormatter formatter = new Rss20FeedFormatter();
                formatter.ReadFrom(reader);
                reader.Close();
                finalItems.AddRange(formatter.Feed.Items);
            }

            finalItems.Sort(CompareDates);
            if (finalItems.Count > 30)
            {
                finalItems = finalItems.Take(30).ToList();
            }

            SyndicationFeed finalFeed = new SyndicationFeed();
            finalFeed.Title = new TextSyndicationContent("Mark Feed");
            finalFeed.Copyright = new TextSyndicationContent("Copyright (C) 2025. All rights reserved.");
            finalFeed.Description = new TextSyndicationContent("Mark Feed");
            finalFeed.Generator = "Mark Harrison Feed Generator";
            finalFeed.LastUpdatedTime = DateTimeOffset.Now;
            finalFeed.Items = finalItems;

            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                var formatter = new Rss20FeedFormatter(finalFeed);
                formatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
                strFeed = stringWriter.ToString();
            }

            // Cache the feed for 15 minutes
            _cache.Set(CacheKey, strFeed, TimeSpan.FromMinutes(CacheMinutes));
        }
    }

}
