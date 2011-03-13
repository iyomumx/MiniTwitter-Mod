using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using MiniTwitter.Extensions;

namespace MiniTwitter.Net
{
    static class TinyUrlHelper
    {
        private static readonly Regex tinyUrlPattern = new Regex(@"http://tinyurl\.com/[A-Za-z0-9_/.;%&\-]+", RegexOptions.Compiled);

        public static string ConvertTo(string url)
        {
            if (tinyUrlPattern.IsMatch(url))
            {
                return url;
            }
            var request = (HttpWebRequest)WebRequest.Create("http://tinyurl.com/api-create.php?url=" + url);
            request.Timeout = 1000;
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return url;
            }
        }

        public static string ConvertFrom(string url)
        {
            if (!tinyUrlPattern.IsMatch(url))
            {
                return url;
            }
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Timeout = 1000;
            request.Method = "HEAD";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.MovedPermanently)
                {
                    var location = response.Headers["Location"];
                    if (!location.IsNullOrEmpty())
                    {
                        return location;
                    }
                }
                return url;
            }
            catch
            {
                return url;
            }
        }
    }
}
