/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MiniTwitter.Net
{
    public class OAuthBase : PropertyChangedBase
    {
        protected OAuthBase(string consumerKey, string consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        protected string _token;
        protected string _tokenSecret;

        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        public const string Endpoint = "https://api.twitter.com/";
        public const string SignEndpoint = "https://api.twitter.com";

        private readonly static Random _random = new Random();
        private readonly static DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private const string _unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private static readonly Action<ProccessStep> EmptyProccessCallBack = _ => { };

        public enum ProccessStep
        {
            CreatRequest,
            GetResponse,
            PraseData,
            Error,
        }

        public enum HttpVerbs
        {
            Get,
            Post,
            Put,
            Delete,
        }

        private static class Serializer<T>
        {
            private static readonly XmlSerializer _xs = new XmlSerializer(typeof(T));

            //static Serializer()
            //{
            //    _xs.UnknownAttribute += (_, __) => { };
            //    _xs.UnknownElement += (_, __) => { };
            //    _xs.UnknownNode += (_, __) => { };
            //    _xs.UnreferencedObject += (_, __) => { };
            //}

            public static T Deserialize(Stream stream)
            {
                return (T)_xs.Deserialize(stream);
            }

            public static T Deserialize(XmlReader reader)
            {
                return (T)_xs.Deserialize(reader);
            }

            public static T Deserialize(TextReader reader)
            {
                return (T)_xs.Deserialize(reader);
            }
        }

        #region インスタンスメソッド

        private int _totalRateLimit;

        public int TotalRateLimit
        {
            get
            {
                return _totalRateLimit;
            }
            protected set
            {
                if (_totalRateLimit != value)
                {
                    _totalRateLimit = value;
                    OnPropertyChanged("TotalRateLimit");
                }
            }
        }

        private int _rateLimitRemain;

        public int RateLimitRemain
        {
            get
            {
                return _rateLimitRemain;
            }
            protected set
            {
                if (_rateLimitRemain != value)
                {
                    _rateLimitRemain = value;
                    double state = (double)_rateLimitRemain / (double)_totalRateLimit;
                    if (state>0.5)
                    {
                        RateLimitState = 0;
                    }
                    else if(state>0.2)
                    {
                        RateLimitState = 1;
                    }
                    else
                    {
                        RateLimitState = 2;
                    }
                    OnPropertyChanged("RateLimitRemain");
                }
            }
        }

        private int _rateLimitState;

        public int RateLimitState
        {
            get
            {
                return _rateLimitState;
            }
            protected set
            {
                if (_rateLimitState != value)
                {
                    _rateLimitState = value;
                    OnPropertyChanged("RateLimitState");
                }
            }
        }

        private int _resetTime;

        public int ResetTime
        {
            get
            {
                return _resetTime;
            }
            protected set
            {
                if (_resetTime != value)
                {
                    _resetTime = value;
                    OnPropertyChanged("ResetTime");
                    ResetTimeString = TimeZone.CurrentTimeZone.ToLocalTime(_unixEpoch.AddSeconds(value)).ToLongTimeString();
                }
            }
        }

        private string _resetTimeString;
        public string ResetTimeString
        {
            get
            {
                return _resetTimeString;
            }
            protected set
            {
                _resetTimeString = value;
                OnPropertyChanged("ResetTimeString");
            }
        }

        protected string Get(string url, Action<ProccessStep> proccessCallBack = null)
        {
            return Get(url, null, proccessCallBack);
        }

        protected string Get(string url, object param, Action<ProccessStep> proccessCallBack = null)
        {
            return Fetch(HttpVerbs.Get, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected T Get<T>(string url, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Get<T>(url, null, proccessCallBack);
        }

        protected T Get<T>(string url, object param, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Fetch<T>(HttpVerbs.Get, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected T Get<T>(string url, object param, out DateTime lastModified, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Fetch<T>(HttpVerbs.Get, url, param, _token, _tokenSecret, null, out lastModified, proccessCallBack);
        }

        protected string Post(string url, Action<ProccessStep> proccessCallBack = null)
        {
            return Post(url, null, proccessCallBack);
        }

        protected string Post(string url, object param, Action<ProccessStep> proccessCallBack = null)
        {
            return Fetch(HttpVerbs.Post, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected T Post<T>(string url, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Post<T>(url, null, proccessCallBack);
        }

        protected T Post<T>(string url, object param, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Fetch<T>(HttpVerbs.Post, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected string Put(string url, Action<ProccessStep> proccessCallBack = null)
        {
            return Put(url, null, proccessCallBack);
        }

        protected string Put(string url, object param, Action<ProccessStep> proccessCallBack = null)
        {
            return Fetch(HttpVerbs.Put, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected T Put<T>(string url, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Put<T>(url, null, proccessCallBack);
        }

        protected T Put<T>(string url, object param, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Fetch<T>(HttpVerbs.Put, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected string Delete(string url, Action<ProccessStep> proccessCallBack = null)
        {
            return Delete(url, null, proccessCallBack);
        }

        protected string Delete(string url, object param, Action<ProccessStep> proccessCallBack = null)
        {
            return Fetch(HttpVerbs.Delete, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        protected T Delete<T>(string url, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Delete<T>(url, null, proccessCallBack);
        }

        protected T Delete<T>(string url, object param, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            return Fetch<T>(HttpVerbs.Delete, url, param, _token, _tokenSecret, null, proccessCallBack);
        }

        #endregion

        #region static メソッド

        private string Fetch(HttpVerbs verb, string url, object param, string token, string tokenSecret, string verifier, Action<ProccessStep> proccessCallBack = null)
        {
            DateTime temp;
            return Fetch(verb, url, param, token, tokenSecret, verifier, out temp, proccessCallBack);
        }

        private string Fetch(HttpVerbs verb, string url, object param, string token, string tokenSecret, string verifier, out DateTime lastModified, Action<ProccessStep> proccessCallBack = null)
        {
            lastModified = DateTime.Now;
            if (proccessCallBack == null) proccessCallBack = EmptyProccessCallBack;
            for (int i = 0; i < 5; ++i)
            {
                proccessCallBack(ProccessStep.CreatRequest);
                var request = CreateRequest(verb, url, param, token, tokenSecret, verifier);
                proccessCallBack(ProccessStep.GetResponse);
                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    lastModified = response.LastModified;
                    proccessCallBack(ProccessStep.PraseData);
                    try
                    {
                        int num;
                        int.TryParse(response.Headers["X-RateLimit-Limit"], out num);
                        if (num != 0)
                        {
                            TotalRateLimit = num;
                            int.TryParse(response.Headers["X-RateLimit-Remaining"], out num);
                            RateLimitRemain = num;
                            int.TryParse(response.Headers["X-RateLimit-Reset"], out num);
                            ResetTime = num;
                        }
                    }
                    catch
                    {
                    }
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
#if DEBUG
                        var s = reader.ReadToEnd();
                        return s;
#else
                        return reader.ReadToEnd();
#endif
                    }
                }
                catch (WebException e)
                {
                    proccessCallBack(ProccessStep.Error);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = (HttpWebResponse)e.Response;
                        string msg;
                        using (var sr = new System.IO.StreamReader(e.Response.GetResponseStream()))
                        {
                            msg = sr.ReadToEnd();
                        }
                        //Log.Logger.Default.AddLogItem(new Log.LogItem(ToMethodString(verb), response.ResponseUri.AbsolutePath, string.Empty, msg));
                        if ((int)response.StatusCode < 500)
                        {
                            throw;
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

        private T Fetch<T>(HttpVerbs verb, string url, object param, string token, string tokenSecret, string verifier, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            DateTime temp;
            return Fetch<T>(verb, url, param, token, tokenSecret, verifier, out temp);
        }

        private T Fetch<T>(HttpVerbs verb, string url, object param, string token, string tokenSecret, string verifier, out DateTime lastModified, Action<ProccessStep> proccessCallBack = null) where T : class
        {
            if (proccessCallBack == null) proccessCallBack = EmptyProccessCallBack;
            lastModified = DateTime.Now;
#if DEBUG
            string s;
#endif
            for (int i = 0; i < 5; ++i)
            {
                proccessCallBack(ProccessStep.CreatRequest);
                var request = CreateRequest(verb, url, param, token, tokenSecret, verifier);
                proccessCallBack(ProccessStep.GetResponse);
                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    lastModified = response.LastModified;
                    proccessCallBack(ProccessStep.PraseData);
                    try
                    {
                        int num;
                        int.TryParse(response.Headers["X-RateLimit-Limit"], out num);
                        if (num != 0)
                        {
                            TotalRateLimit = num;
                            int.TryParse(response.Headers["X-RateLimit-Remaining"], out num);
                            RateLimitRemain = num;
                            int.TryParse(response.Headers["X-RateLimit-Reset"], out num);
                            ResetTime = num;
                        }
                    }
                    catch
                    {
                    }
                    using (var stream = response.GetResponseStream())
                    {
#if DEBUG
                        //卧槽DEBUG要性能能当饭吃？
                        using (var reader = new StreamReader(stream))
                        {
                            s = reader.ReadToEnd();                     //反序列化有问题请找字符串
                        }
                        XElement xml = XElement.Parse(s);
                        return Serializer<T>.Deserialize(xml.CreateReader());
#else
                        return Serializer<T>.Deserialize(stream);
#endif
                    }
                }
                catch (WebException e)
                {
                    proccessCallBack(ProccessStep.Error);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = (HttpWebResponse)e.Response;
                        string msg;
                        using (var sr = new System.IO.StreamReader(e.Response.GetResponseStream()))
                        {
                            msg = sr.ReadToEnd();
                        }
                        //Log.Logger.Default.AddLogItem(new Log.LogItem(ToMethodString(verb), response.ResponseUri.AbsolutePath, string.Empty, msg));
                        if ((int)response.StatusCode < 500)
                        {
                            throw new ApplicationException(msg, e);
                        }
                    }
                }
                catch//(Exception ex)
                {
                    //Log.Logger.Default.AddLogItem(new Log.LogItem(ex));
                    return default(T);
                }
            }
            return default(T);
        }

        public Stream FetchStream(HttpVerbs verb, string url, object param, out DateTime lastModified, Action<ProccessStep> proccessCallBack = null)
        {
            if (proccessCallBack == null) proccessCallBack = EmptyProccessCallBack;
            lastModified = DateTime.Now;

            for (int i = 0; i < 5; ++i)
            {
                proccessCallBack(ProccessStep.CreatRequest);
                var request = CreateRequest(verb, url, param, _token, _tokenSecret, null);
                proccessCallBack(ProccessStep.GetResponse);
                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    lastModified = response.LastModified;
                    proccessCallBack(ProccessStep.PraseData);
                    //try
                    //{
                    //    int num;
                    //    int.TryParse(response.Headers["X-RateLimit-Remaining"], out num);
                    //    RateLimitRemain = num;
                    //    int.TryParse(response.Headers["X-RateLimit-Limit"], out num);
                    //    TotalRateLimit = num;
                    //    int.TryParse(response.Headers["X-RateLimit-Reset"], out num);
                    //    ResetTime = num;
                    //}
                    //catch
                    //{
                    //}
                    return response.GetResponseStream();
                }
                catch (WebException e)
                {
                    proccessCallBack(ProccessStep.Error);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = (HttpWebResponse)e.Response;
                        string msg;
                        using (var sr = new System.IO.StreamReader(e.Response.GetResponseStream()))
                        {
                            msg = sr.ReadToEnd();
                        }
                        //Log.Logger.Default.AddLogItem(new Log.LogItem(ToMethodString(verb), response.ResponseUri.AbsolutePath, string.Empty, msg));
                        if ((int)response.StatusCode < 500)
                        {
                            throw;
                        }
                    }
                }
                catch
                {
                    proccessCallBack(ProccessStep.Error);
                    return null;
                }
            }
            return null;
        }

        public string RedirectToAuthorize(string token)
        {
            return string.Format("{0}?oauth_token={1}", Endpoint + "oauth/authorize", token);
        }

        public bool GetRequestToken(out string token)
        {
            var response = Fetch(HttpVerbs.Post, Endpoint + "oauth/request_token", null, null, null, null);
            if (string.IsNullOrEmpty(response))
            {
                token = null;
                return false;
            }
            var query = ParseQueryString(response);
            token = query["oauth_token"];
            return true;
        }

        public bool GetAccessToken(ref string token, ref string tokenSecret, string verifier)
        {
            var response = Fetch(HttpVerbs.Post, Endpoint + "oauth/access_token", null, token, null, verifier);
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }
            var query = ParseQueryString(response);
            token = query["oauth_token"];
            tokenSecret = query["oauth_token_secret"];
            return true;
        }

        public bool GetAccessToken(string username, string password, ref string token, ref string tokenSecret)
        {
            try
            {
                var response = Fetch(HttpVerbs.Post, Endpoint + "oauth/access_token", new { x_auth_mode = "client_auth", x_auth_password = password, x_auth_username = username }, null, null, null);

                if (string.IsNullOrEmpty(response))
                {
                    return false;
                }

                var query = ParseQueryString(response);

                token = query["oauth_token"];
                tokenSecret = query["oauth_token_secret"];

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tokenSecret))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private HttpWebRequest CreateRequest(HttpVerbs verb, string url, object param, string token, string tokenSecret, string verifier)
        {
            // 匿名型を Key-Value のコレクションに変換する
            var query = ParseQueryObject(param);
            // コレクションをクエリ文字列に変換
            var queryString = CreateQueryString(query);
            // GET なら URL パラメータとして付ける
            if (verb == HttpVerbs.Get && !string.IsNullOrEmpty(queryString))
            {
                url += "?" + queryString;
            }
            // HTTP リクエストを作成
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = ToMethodString(verb);
            request.Accept = "application/xml, text/xml, */*";
            request.UserAgent = App.NAME + App.VERSION;
            request.Timeout = 6000;
            request.ServicePoint.Expect100Continue = false;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            // GET 以外ならリクエストボディに書き込む
            if (verb != HttpVerbs.Get)
            {
                request.ContentType = "application/x-www-form-urlencoded";
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(queryString);
                }
            }
            // OAuth の認証ヘッダーを付ける
            if (MiniTwitter.Properties.Settings.Default.UseBasicAuth)
            {
                request.Credentials = new System.Net.NetworkCredential(MiniTwitter.Properties.Settings.Default.Token, MiniTwitter.Properties.Settings.Default.TokenSecret);
                request.Headers[HttpRequestHeader.Authorization] = string.Format("BASICAUTH {0}",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            String.Format("{0}:{1}", 
                                            MiniTwitter.Properties.Settings.Default.Token, 
                                            MiniTwitter.Properties.Settings.Default.TokenSecret))));
            }
            else
            {
                AddOAuthToken(request, query, token, tokenSecret, verifier);
            }
            return request;
        }

        public string GetOAuthToken(Uri uri)
        {
            // パラメータを作成する
            var parameter = new NameValueCollection
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", CreateTimestamp() },
                { "oauth_nonce", CreateNonce() },
                { "oauth_version", "1.0" }
            };
            // トークンが存在すれば追加する
            if (!string.IsNullOrEmpty(_token))
            {
                parameter.Add("oauth_token", _token);
            }
            // 署名を作成
            var signature = CreateSignature(_tokenSecret, "GET", uri, parameter);
            parameter.Add("oauth_signature", signature);
            // 認証トークンを作成する
            var header = new StringBuilder();
            header.Append("OAuth realm=\"http://api.twitter.com/\",");
            foreach (var key in parameter.AllKeys.Where(p => p.StartsWith("oauth_")))
            {
                header.AppendFormat("{0}=\"{1}\",", key, UrlEncode(parameter[key]));
            }
            
            return header.ToString(0, header.Length - 1);
        }

        private void AddOAuthToken(HttpWebRequest request, NameValueCollection query, string token, string tokenSecret, string verifier)
        {
            // パラメータを作成する
            var parameter = new NameValueCollection
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", CreateTimestamp() },
                { "oauth_nonce", CreateNonce() },
                { "oauth_version", "1.0" }
            };
            if (!string.IsNullOrEmpty(verifier))
            {
                parameter.Add("oauth_verifier", verifier);
            }
            // トークンが存在すれば追加する
            if (!string.IsNullOrEmpty(token))
            {
                parameter.Add("oauth_token", token);
            }
            // クエリパラメータを追加する
            parameter.Add(query);
            // 署名を作成
            var signature = CreateSignature(tokenSecret, request.Method, request.RequestUri, parameter);
            parameter.Add("oauth_signature", signature);
            // 認証トークンを作成する
            var header = new StringBuilder();
            header.Append("OAuth ");
            foreach (var key in parameter.AllKeys.Where(p => p.StartsWith("oauth_")))
            {
                header.AppendFormat("{0}=\"{1}\",", key, UrlEncode(parameter[key]));
            }
            // ヘッダに追加する
            request.Headers[HttpRequestHeader.Authorization] = header.ToString(0, header.Length - 1);
        }

        private static string CreateTimestamp()
        {
            var timestamp = DateTime.UtcNow - _unixEpoch;
            return Convert.ToInt64(timestamp.TotalSeconds).ToString();
        }

        private static string CreateNonce()
        {
            var nonce = new byte[32];
            _random.NextBytes(nonce);
            return Convert.ToBase64String(nonce);
        }

        private string CreateSignature(string tokenSecret, string method, Uri uri, NameValueCollection query)
        {
            // クエリを文字列に変換する
            var queryString = CreateQueryString(query);
            // URL を正規化する
            string ap;
            if (uri.AbsolutePath.Contains("/t/"))
            {
                ap = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf("/t/") + 2);
            }
            else
            {
                ap = uri.AbsolutePath;
            }
            var url = uri.Host.Contains("userstream") ? string.Format("{0}://{1}{2}",uri.Scheme,uri.Host,uri.AbsolutePath) : string.Format("{0}{1}", SignEndpoint, ap);
            // 署名の元になる文字列を作成
            var signatureBase = string.Format("{0}&{1}&{2}", method, UrlEncode(url), UrlEncode(queryString));
            // 署名するためのキーを作成
            var key = string.Format("{0}&{1}", UrlEncode(_consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret));
            // HMAC-SHA1 でハッシュを計算する
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBase));
                // 計算したハッシュを文字列に変換する
                return Convert.ToBase64String(hash);
            }
        }

        private static string CreateQueryString(NameValueCollection query)
        {
            // クエリが存在するか確認する
            if (query.Count == 0)
            {
                // 存在しないので空文字列を返す
                return "";
            }
            // クエリ文字列に変換する
            var queryString = new StringBuilder();
            foreach (var key in query.AllKeys.OrderBy(p => p))
            {
                queryString.AppendFormat("{0}={1}&", UrlEncode(key), UrlEncode(query[key]));
            }
            return queryString.ToString(0, queryString.Length - 1);
        }

        private static NameValueCollection ParseQueryString(string queryString)
        {
            var query = new NameValueCollection();
            var parts = queryString.Split('&');
            foreach (var part in parts)
            {
                int index = part.IndexOf('=');
                if (index == -1)
                {
                    query.Add(UrlDecode(part), "");
                }
                else
                {
                    query.Add(UrlDecode(part.Substring(0, index)), UrlDecode(part.Substring(index + 1)));
                }
            }
            return query;
        }

        private static NameValueCollection ParseQueryObject(object queryObject)
        {
            var query = new NameValueCollection();
            if (queryObject != null)
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(queryObject))
                {
                    query.Add(property.Name, property.GetValue(queryObject).ToString());
                }
            }
            return query;
        }

        private static string UrlEncode(string str)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.UTF8.GetBytes(str);
            foreach (var b in bytes)
            {
                if (_unreservedChars.IndexOf((char)b) != -1)
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", b);
                }
            }
            return sb.ToString();
        }

        private static string UrlDecode(string str)
        {
            return Uri.UnescapeDataString(str);
        }

        private static string ToMethodString(HttpVerbs verb)
        {
            switch (verb)
            {
                case HttpVerbs.Post:
                    return "POST";
                case HttpVerbs.Put:
                    return "PUT";
                case HttpVerbs.Delete:
                    return "DELETE";
            }
            return "GET";
        }

        #endregion
    }
}
