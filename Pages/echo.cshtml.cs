using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure1News.Pages
{

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class EchoModel : PageModel
    {

        public string strHTML;

        public async Task OnGetAsync()
        {

            var _body = "";

            strHTML = "<h3>Echo</h3>";

            try
            {
                var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8);
                _body = await reader.ReadToEndAsync();

            }
            catch (Exception ex)
            {
                _body = "Exception: " + ex.Message;
                return;
            }

            strHTML += $"body = <span class='echodata'> {_body}</span><br />";

            strHTML += "headers:<br/>";
            foreach (var key in Request.Headers.Keys)
                strHTML += $"&nbsp;&bull;{key} = <span class='echodata'>{Request.Headers[key]}</span><br />";

            strHTML += $"host = <span class='echodata'> {Request.Host}</span><br />";
            strHTML += $"ishttps = <span class='echodata'> {Request.IsHttps.ToString()}</span><br />";
            strHTML += $"method = <span class='echodata'> {Request.Method}</span><br />";
            strHTML += $"path = <span class='echodata'> {Request.Path}</span><br />";
            strHTML += $"pathbase = <span class='echodata'> {Request.PathBase}</span><br />";
            strHTML += $"protocol = <span class='echodata'> {Request.Protocol}</span><br />";
            strHTML += $"querystring = <span class='echodata'> {Request.QueryString}</span><br />";
            strHTML += $"scheme = <span class='echodata'> {Request.Scheme}</span><br />";

            strHTML += $"<br />";
            strHTML += "connection:<br/>";
            strHTML += $"&nbsp;&bull;localipaddress = <span class='echodata'>{HttpContext.Connection.LocalIpAddress}</span><br />";
            strHTML += $"&nbsp;&bull;localport = <span class='echodata'>{HttpContext.Connection.LocalPort}</span><br />";
            strHTML += $"&nbsp;&bull;remoteipaddress = <span class='echodata'>{HttpContext.Connection.RemoteIpAddress}</span><br />";
            strHTML += $"&nbsp;&bull;remoteport = <span class='echodata'>{HttpContext.Connection.RemotePort}</span><br />";

            strHTML += $"<br />";
            strHTML += $"os = <span class='echodata'> {System.Runtime.InteropServices.RuntimeInformation.OSDescription}</span><br />";

        }
    }
}