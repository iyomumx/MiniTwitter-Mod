using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace MiniTwitter.Net
{
    static class BitlyHelper
    {
        private const string API_VERSION = "2.0.1";
        private const string API_KEY = "R_276fb4934824bf8ee936ad0daf0e6745";
        private const string API_USERNAME = "shibayan";

        private static readonly Regex bitlyPattern = new Regex(@"http://bit\.ly/[A-Za-z0-9_/.;%&\-]+", RegexOptions.Compiled);

        public static string ConvertTo(string url)
        {
            if (bitlyPattern.IsMatch(url))
            {
                return url;
            }
            var request = (HttpWebRequest)WebRequest.Create(string.Format("http://api.bit.ly/shorten?version={0}&longUrl={1}&login={2}&apiKey={3}&format=xml", API_VERSION, Uri.EscapeDataString(url), API_USERNAME, API_KEY));
            request.Timeout = 1000;
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var document = new XmlDocument();
                    document.Load(stream);
                    var list = document.GetElementsByTagName("shortUrl");
                    if (list.Count != 0)
                    {
                        return list[0].InnerText;
                    }
                    return url;
                }
            }
            catch
            {
                return url;
            }
        }

        public static string ConvertFrom(string url)
        {
            return url;
        }
    }
}
