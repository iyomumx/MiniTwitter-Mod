using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace MiniTwitter.Net
{
    static class BitlyHelper
    {
        private const string API_VERSION = "2.0.1";
        private const string API_KEY = "R_276fb4934824bf8ee936ad0daf0e6745";
        private const string API_USERNAME = "shibayan";

        private static int failCount = 0;

        private static readonly Regex bitlyPattern = new Regex(@"http://bit\.ly/[A-Za-z0-9_/.;%&\-]+", RegexOptions.Compiled);

        private static Dictionary<string, string> cache = new Dictionary<string, string>();

        public static string ConvertTo(string url)
        {
            if (bitlyPattern.IsMatch(url))
            {
                return url;
            }
            if (cache.ContainsKey(url))
            {
                return cache[url];
            }
            var request = (HttpWebRequest)WebRequest.Create(string.Format("http://api.bit.ly/v3/shorten?longUrl={0}&login={1}&apiKey={2}&domain={3}&format=xml",  Uri.EscapeDataString(url), MiniTwitter.Properties.Settings.Default.BitlyUsername, MiniTwitter.Properties.Settings.Default.BitlyApiKey, MiniTwitter.Properties.Settings.Default.BitlyProDomain));
            request.Timeout = 5000 + failCount * 1000;
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var document = new XmlDocument();
                    document.Load(stream);
                    var list = document.GetElementsByTagName("url");
                    if (list.Count != 0)
                    {
                        Interlocked.Exchange(ref failCount, 0);
                        cache.Add(url, list[0].InnerText);
                        return list[0].InnerText;
                    }
                    return url;
                }
            }
            catch
            {
                if (failCount < 5) Interlocked.Increment(ref failCount);
                return url;
            }
        }

        

        public static string ConvertFrom(string url)
        {
            if (cache.ContainsValue(url))
            {
                return (from kvp in cache.AsParallel() where kvp.Value == url select kvp.Value).First();
            }
            var request = (HttpWebRequest)WebRequest.Create(string.Format("http://api.bit.ly/v3/expand?shortUrl={0}&login={1}&apiKey={2}&format=txt", Uri.EscapeDataString(url), MiniTwitter.Properties.Settings.Default.BitlyUsername, MiniTwitter.Properties.Settings.Default.BitlyApiKey));
            request.Timeout = 5000 + failCount * 100;
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();
                using (var stream = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    var result = stream.ReadToEnd();
                    result = result.Substring(0, result.Length - 1);
                    Interlocked.Exchange(ref failCount, 0);
                    return result;
                }
            }
            catch
            {
                if (failCount < 5) Interlocked.Increment(ref failCount);
                return url;
            }
        }

        public static bool IsBitlyProDomain(string url)
        {
            try
            {
                var domain = url;
                var request = (HttpWebRequest)WebRequest.Create(string.Format("http://api.bit.ly/v3/bitly_pro_domain?domain={0}&apiKey={1}&login={2}&format=xml", domain, MiniTwitter.Properties.Settings.Default.BitlyApiKey, MiniTwitter.Properties.Settings.Default.BitlyUsername));
                request.Timeout = 10000;
                request.Method = "GET";
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var document = new XmlDocument();
                    document.Load(stream);
                    var list = document.GetElementsByTagName("bitly_pro_domain");
                    if (list.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        if (list[0].InnerText == "1") return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;                
            }
        }
    }
}
