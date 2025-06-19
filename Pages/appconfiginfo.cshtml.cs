using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Azure1News.Pages
{
    public class AppConfigInfoModel : PageModel
    {
        IConfiguration _config;
        AppConfig _appconfig;
        public string strAppConfigInfoHtml;

        public AppConfigInfoModel(IConfiguration config, AppConfig appconfig)
        {
            _config = config;
            _appconfig = appconfig;
            strAppConfigInfoHtml = "";
        }

        public void OnGet()
        {
            string pw = HttpContext.Request.Query["pw"].ToString();
            if (string.IsNullOrEmpty(pw) || pw != _appconfig.AdminPW)
                return;

            strAppConfigInfoHtml += "OS Description: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription + "<br/>";
            strAppConfigInfoHtml += "ASPNETCORE_ENVIRONMENT: " + _config.GetValue<string>("ASPNETCORE_ENVIRONMENT") + "<br/>";
            strAppConfigInfoHtml += "Framework Description: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + "<br/>";   
            strAppConfigInfoHtml += "Instrumentation Key: " + _config.GetValue<string>("ApplicationInsights:InstrumentationKey") + "<br/>";
            strAppConfigInfoHtml += "Build Identifier: " + _config.GetValue<string>("BuildIdentifier") + "<br/>";
            strAppConfigInfoHtml += "Feed Url: " + _appconfig.FeedUrl + "<br/>";
            strAppConfigInfoHtml += "FeedM Url: " + _appconfig.FeedMUrl + "<br/>";
            strAppConfigInfoHtml += "Google Id: " + _appconfig.GoogleId + "<br/>";
            strAppConfigInfoHtml += "ConsumerKey: " + _appconfig.ConsumerKey + "<br/>";
            strAppConfigInfoHtml += "ConsumerSecret: " + _appconfig.ConsumerSecret + "<br/>";
            strAppConfigInfoHtml += "AccessToken: " + _appconfig.AccessToken + "<br/>";
            strAppConfigInfoHtml += "AccessTokenSecret: " + _appconfig.AccessTokenSecret + "<br/>";
            strAppConfigInfoHtml += "RSS Feeds:<br/>";
            string[] rssFeeds = _appconfig.RSSFeeds;
            foreach (var feed in rssFeeds)
            {
                strAppConfigInfoHtml += "&bull;&nbsp;" + feed + "<br/>";
            }
        }
    }
}

 
 
