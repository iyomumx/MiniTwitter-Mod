using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

using MiniTwitter.Extensions;

namespace MiniTwitter.Net
{
    public class KanvasoHelper
    {
        private static KanvasoHelper _defaultInstance;

        internal static KanvasoHelper Default
        {
            get { return KanvasoHelper._defaultInstance; }
            set { KanvasoHelper._defaultInstance = value; }
        }


        private static string AppKey = "9e72a1c84f918c938f4d95a24bb2785f6c561bdc";
        public static string ShortenStatus(string status, string token, ulong? in_reply_to_status_id,double? latitude, double? longitude)
        {
            if (status.Length <= 140)
            {
                return status;
            }
            try
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.json");
                nvc.Add("X-Verify-Credentials-Authorization", token);
                StringBuilder optionalParam = new StringBuilder();
                if (in_reply_to_status_id != null)
                {
                    optionalParam.AppendFormat("&in_reply_to_status_id={0}", in_reply_to_status_id);
                }
                if (latitude != null && longitude != null)
                {
                    optionalParam.AppendFormat("&lat={0}&long={1}", latitude, longitude);
                }
                //制作发往kanvaso的Request
                HttpWebRequest request = WebRequest.Create("http://api.kanvaso.com/1/update.php") as HttpWebRequest;
#if DEBUG
                var debugString = nvc["X-Verify-Credentials-Authorization"];
#endif
                request.Headers.Add(nvc);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Accept = "application/xml, text/xml, */*";
                request.UserAgent = App.NAME + App.VERSION;
                request.Timeout = 6000;
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
#if DEBUG
                    debugString = String.Format("text={1}&api_key={0}&format=xml{2}", AppKey, status.UrlEncode(), optionalParam.ToString());
                    writer.Write(debugString);
#else
                    writer.Write(String.Format("text={1}&api_key={0}&format=xml{2}", AppKey, status.UrlEncode(), optionalParam.ToString()));
#endif
                    writer.Flush();
                }
                var response = request.GetResponse();
                using (var reader = response.GetResponseStream())
                {
                    var result = Serializer<KancasoReturn>.Deserialize(reader);
                    if (result.status == "success")
                    {
                        return result.text;
                    }
                    else
                    {
                        return status;
                    }
                }
            }
            catch (Exception)
            {
                return status;
            }
        }

        public static string GetLongStatus(string code)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {

                    var request = WebRequest.Create(string.Format("http://api.kanvaso.com/1/show.php?api_key={0}&code={1}&format=xml", AppKey, code)) as HttpWebRequest;
                    request.Method = "GET";
                    request.Accept = "application/xml, text/xml, */*";
                    request.UserAgent = App.NAME + App.VERSION;
                    request.Timeout = 6000;
                    using (var reader = request.GetResponse().GetResponseStream())
                    {
                        var result = Serializer<KancasoReturn>.Deserialize(reader);
                        if (result.status == "success")
                        {
                            return result.text;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = (HttpWebResponse)e.Response;
                        if ((int)response.StatusCode < 500)
                        {
                            return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        [Serializable]
        [XmlRoot(elementName: "response")]
        public class KancasoReturn
        {
            [XmlElement]
            public string status { get; set; }

            [XmlElement]
            public string text { get; set; }

            [XmlElement]
            public string url { get; set; }
        }
    }
}
