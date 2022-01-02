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
            strAppConfigInfoHtml += "Instrumentation Key: " + _config.GetValue<string>("ApplicationInsights:InstrumentationKey") + "<br/>";
            strAppConfigInfoHtml += "Build Identifier: " + _config.GetValue<string>("BuildIdentifier") + "<br/>";
            strAppConfigInfoHtml += "Feed Url: " + _appconfig.FeedUrl + "<br/>";
            strAppConfigInfoHtml += "Google Id: " + _appconfig.GoogleId + "<br/>";
        }
    }
}

 
 
