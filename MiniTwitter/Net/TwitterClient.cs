/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Net
{
    public class TwitterClient : OAuthBase
    {
        static TwitterClient()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.Expect100Continue = false;
            Default = new TwitterClient(App.consumer_key, App.consumer_secret);
        }

        public TwitterClient(string consumerKey, string consumerSecret)
            : base(consumerKey, consumerSecret)
        {
            RetryCount = 5;
        }

        public static TwitterClient Default
        {
            get;
            private set;
        }

        private readonly object thisLock = new object();

        private static readonly Regex schemaRegex = new Regex(@"(https?:\/\/[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex messageRegex = new Regex(@"^d\s([a-zA-Z_0-9]+)\s", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex kanvasoUrl = new Regex(@"http://kvs\.co/(?<code>\w+?)$", RegexOptions.Compiled & RegexOptions.IgnoreCase);

        private static readonly Regex unescapeRegex = new Regex("&([gl]t);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string _TwitterApiBase = "https://api.twitter.com/";
        
        public static string ApiBaseUrl
        {
            get
            {
                return string.IsNullOrEmpty(MiniTwitter.Properties.Settings.Default.ApiBaseUrl) ? _TwitterApiBase : MiniTwitter.Properties.Settings.Default.ApiBaseUrl;
            }
        }


        private const string _TwitterSearchBase = "https://search.twitter.com/";

        public static string SearchApiUrl 
        {
            get 
            {
                return string.IsNullOrEmpty(MiniTwitter.Properties.Settings.Default.ApiSearchUrl) ? _TwitterSearchBase : (MiniTwitter.Properties.Settings.Default.ApiSearchUrl);
            }
        }

        public static string Unescape(string text)
        {            
            return unescapeRegex.Replace(text, match => match.Groups[1].Value == "gt" ? ">" : "<");
        }

        private const string clientName = "MiniTwitter";

        public static Google.UrlShorter.UrlShorter googlHelper;

        public IWebProxy Proxy
        {
            get { return WebRequest.DefaultWebProxy; }
            set { WebRequest.DefaultWebProxy = value; }
        }

        public User LoginedUser { get; private set; }

        public bool ConvertShortUrl { get; set; }

        public string Footer { get; set; }

        public int RetryCount { get; set; }

        public bool IsLogined
        {
            get
            {
                lock (thisLock)
                {
                    return LoginedUser != null;
                }
            }
        }

        private ulong? recentId;

        public Status[] RecentTimeline
        {
            get { return IsLogined ? GetStatuses(string.Format("{0}1/statuses/home_timeline.xml", ApiBaseUrl), new { count = 200 }, ref recentId) : Statuses.Empty; }
        }

        private ulong? repliesId;

        public Status[] RepliesTimeline
        {
            get { return IsLogined ? GetStatuses(string.Format("{0}1/statuses/mentions.xml", ApiBaseUrl), new { count = 100 }, ref repliesId) : Statuses.Empty; }
        }

        private ulong? archiveId;

        public Status[] ArchiveTimeline
        {
            get { return IsLogined ? GetStatuses(string.Format("{0}1/statuses/user_timeline.xml", ApiBaseUrl), new { count = 100 }, ref archiveId) : Statuses.Empty; }
        }

        private ulong? receivedId;

        public DirectMessage[] ReceivedMessages
        {
            get { return IsLogined ? GetMessages(string.Format("{0}1/direct_messages.xml", ApiBaseUrl), ref receivedId) : DirectMessages.Empty; }
        }

        private ulong? sentId;

        public DirectMessage[] SentMessages
        {
            get { return IsLogined ? GetMessages(string.Format("{0}1/direct_messages/sent.xml", ApiBaseUrl), ref sentId) : DirectMessages.Empty; }
        }

        public Status[] Favorites
        {
            get { return IsLogined ? GetStatuses(string.Format("{0}1/favorites.xml", ApiBaseUrl), new { count = 200 }) : Statuses.Empty; }
        }

        public User[] Friends
        {
            get { return IsLogined ? GetUsers(string.Format("{0}1/statuses/friends.xml", ApiBaseUrl)) : Users.Empty; }
        }

        public User[] Followers
        {
            get { return IsLogined ? GetUsers(string.Format("{0}1/statuses/followers.xml", ApiBaseUrl)) : Users.Empty; }
        }

        public List[] Lists
        {
            get
            {
                if (IsLogined)
                {
                    var list = GetLists(LoginedUser.ScreenName);
                    var subscriptions = GetListsSubscription(LoginedUser.ScreenName);

                    return list.Concat(subscriptions).ToArray();
                }
                else
                {
                    return List.Empty;
                }
            }
        }

        public event EventHandler<UpdateEventArgs> Updated;

        public event EventHandler<UpdateFailedEventArgs> UpdateFailure;

        public bool? Login(string token, string tokenSecret)
        {
            lock (thisLock)
            {
                Logout();

                if (token.IsNullOrEmpty() || tokenSecret.IsNullOrEmpty())
                {
                    LoginedUser = null;
                    return null;
                }

                _token = token;
                _tokenSecret = tokenSecret;

                LoginedUser = Validate();

                return LoginedUser != null;
            }
        }

        public void Logout()
        {
            lock (thisLock)
            {
                LoginedUser = null;
                recentId = null;
                repliesId = null;
                archiveId = null;
                receivedId = null;
                sentId = null;
            }
        }

        public void Update(string text, Action<ProccessStep> proccessCallBack = null)
        {
            Update(text, null, null, null, proccessCallBack);
        }

        public void Update(string text, ulong? replyId, double? latitude, double? longitude, Action<ProccessStep> proccessCallBack = null)
        {
            if (text.Length > 140)
            {
                text = schemaRegex.Replace(text,
                                             match =>
                                             {
                                                 var url = match.Groups[1].Value;
                                                 return MiniTwitter.Properties.Settings.Default.UseBitlyPro ? BitlyHelper.ConvertTo(url) : MiniTwitter.Net.TwitterClient.googlHelper.ShortenUrl(url);
                                             });
            }
            else
            {
                text = schemaRegex.Replace(text,
                                             match =>
                                             {
                                                 var url = match.Groups[1].Value;
                                                 if (match.Groups[1].Length > 32 || match.Groups[1].Value.IndexOfAny(new[] { '!', '?' }) != -1)
                                                 {
                                                     return MiniTwitter.Properties.Settings.Default.UseBitlyPro ? BitlyHelper.ConvertTo(url) : MiniTwitter.Net.TwitterClient.googlHelper.ShortenUrl(url);
                                                 }
                                                 else
                                                 {
                                                     return match.Groups[1].Value;
                                                 }
                                             });
            }
            Match m = messageRegex.Match(text);
            if (m.Success)
            {
                string user = m.Groups[1].Value;
                text = text.Substring(m.Length);
                UpdateMessage(user, text, proccessCallBack);
            }
            else
            {
                UpdateStatus(text, replyId, latitude, longitude, proccessCallBack);
            }
        }

        public Status ReTweet(ulong id, Action<ProccessStep> proccessCallBack = null)
        {
            try
            {
                Status result = Post<Status>(string.Format("{1}1/statuses/retweet/{0}.xml", id, ApiBaseUrl), proccessCallBack);
                AsyncDo(CacheOrUpdate, result);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private void UpdateStatus(string text, ulong? replyId, double? latitude, double? longitude, Action<ProccessStep> proccessCallBack = null)
        {
            if (!Footer.IsNullOrEmpty())
            {
                if (text.Length > 140 - Footer.Length)
                {
                    text = text.Substring(0, 139 - Footer.Length);// + "...";
                }
                text += " " + Footer;
            }
            else
            {
                if (text.Length > 140)
                {
                    if (MiniTwitter.Properties.Settings.Default.UseKanvasoShorten)
                    {
                        text = KanvasoHelper.ShortenStatus(text, GetOAuthToken(new Uri("https://api.twitter.com/1/account/verify_credentials.json")), replyId, latitude, longitude);
                        if (text.Length > 140)
                        {
                            text = text.Substring(0, 140);// +"...";                 
                        }
                    }
                    else
                    {
                        text = text.Substring(0, 140);// +"...";
                    }
                }
            }
            try
            {
                Status status;
                if (replyId.HasValue)
                {
                    if (latitude.HasValue && longitude.HasValue)
                    {
                        status = Post<Status>(string.Format("{0}1/statuses/update.xml", ApiBaseUrl), new { status = text, in_reply_to_status_id = replyId.Value, lat = latitude.Value, @long = longitude.Value }, proccessCallBack);
                    }
                    else
                    {
                        status = Post<Status>(string.Format("{0}1/statuses/update.xml", ApiBaseUrl), new { status = text, in_reply_to_status_id = replyId.Value }, proccessCallBack);
                    }
                }
                else
                {
                    if (latitude.HasValue && longitude.HasValue)
                    {
                        status = Post<Status>(string.Format("{0}1/statuses/update.xml", ApiBaseUrl), new { status = text, lat = latitude.Value, @long = longitude.Value }, proccessCallBack);
                    }
                    else
                    {
                        status = Post<Status>(string.Format("{0}1/statuses/update.xml", ApiBaseUrl), new { status = text }, proccessCallBack);
                    }
                }
                status.IsAuthor = true;
                
                AsyncDo(CacheOrUpdate, status);     //Cache

                if (Updated != null)
                {
                    Updated(this, new UpdateEventArgs(status));
                }
            }
            catch(Exception e)
            {
                if (UpdateFailure != null)
                {
                    UpdateFailure(this, new UpdateFailedEventArgs(text,e));
                }
            }
        }

        private void UpdateMessage(string user, string text, Action<ProccessStep> proccessCallBack = null)
        {
            try
            {
                var message = Post<DirectMessage>(string.Format("{0}1/direct_messages/new.xml", ApiBaseUrl), new { user = user, text = text }, proccessCallBack);
                message.IsAuthor = true;
                if (Updated != null)
                {
                    Updated(this, new UpdateEventArgs(message));
                }
            }
            catch(Exception e)
            {
                if (UpdateFailure != null)
                {
                    UpdateFailure(this, new UpdateFailedEventArgs(text, e));
                }
            }
        }

        public bool Delete(ITwitterItem item)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                if (item.IsMessage ? DeleteMessage(item.ID) : DeleteStatus(item.ID))
                {
                    return true;
                }
                System.Threading.Thread.Sleep(1000);
            }
            return false;
        }

        private bool DeleteStatus(ulong id)
        {
            try
            {
                Delete(string.Format("{1}1/statuses/destroy/{0}.xml", id, ApiBaseUrl));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteMessage(ulong id)
        {
            try
            {
                Delete(string.Format("{1}1/direct_messages/destroy/{0}.xml", id, ApiBaseUrl));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Favorite(Status status)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                if (status.Favorited ? DeleteFavorite(status.ID) : CreateFavorite(status.ID))
                {
                    return true;
                }
                System.Threading.Thread.Sleep(1000);
            }
            return false;
        }

        private bool CreateFavorite(ulong id)
        {
            try
            {
                Post(string.Format("{1}1/favorites/create/{0}.xml", id, ApiBaseUrl));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteFavorite(ulong id)
        {
            try
            {
                Delete(string.Format("{1}1/favorites/destroy/{0}.xml", id, ApiBaseUrl));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateFollow(string screen_name)
        {
            try
            {
                Post(string.Format("{0}1/friendships/create.xml",ApiBaseUrl), new { screen_name = screen_name });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteFollow(string screen_name)
        {
            try
            {
                Post(string.Format("{0}1/friendships/destroy.xml", ApiBaseUrl), new { screen_name = screen_name });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateBlock(string screen_name)
        {
            try
            {
                Post(string.Format("{0}1/blocks/create.xml", ApiBaseUrl), new { screen_name = screen_name });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteBlock(string screen_name)
        {
            try
            {
                Delete(string.Format("{0}1/blocks/destroy.xml", ApiBaseUrl), new { screen_name = screen_name });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReportSpam(string screen_name)
        {
            try
            {
                Post(string.Format("{0}1/report_spam.xml", ApiBaseUrl), new { screen_name = screen_name });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private User Validate()
        {
            try
            {
                User result = Get<User>(string.Format("{0}1/account/verify_credentials.xml", ApiBaseUrl));
                AsyncDo(CacheOrUpdate, result);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public User GetUser(uint id)
        {
            try
            {
                User result = Get<User>(string.Format("{0}1/users/show.xml", ApiBaseUrl), new { user_id = id });
                AsyncDo(CacheOrUpdate, result);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public User GetUser(string name)
        {
            try
            {
                User result =
                    Get<User>(string.Format("{0}1/users/show.xml", ApiBaseUrl), new { screen_name = name });
                AsyncDo(CacheOrUpdate, result);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public Status[] GetUserTimeline(string name)
        {
            ulong? tmpid = null;
            return GetStatuses(string.Format("{0}1/statuses/user_timeline.xml", ApiBaseUrl), new { count = 100, screen_name = name }, ref tmpid);
        }

        public Status[] GetUserTimeline(string name, int count = 200, ulong? sinceId = null, ulong? maxId = null)
        {
            if (count <= 0 || count > 200)
            {
                throw new ArgumentException("Count必须大于0小于200", "count");
            }
            System.Diagnostics.Contracts.Contract.EndContractBlock();
            object p;
            if (maxId != null)
            {
                p = new { max_id = maxId, count = count, screen_name = name };
            }
            else
            {
                p = new { count = count, screen_name = name };
            }
            return GetStatuses(string.Format("{0}1/statuses/user_timeline.xml", ApiBaseUrl), p, ref sinceId);
        }

        private User[] GetUsers(string url)
        {
            try
            {
                return Get<Users>(url).User;
            }
            catch
            {
                return null;
            }
        }

        public Status GetStatus(ulong id)
        {
            try
            {
                var result = Get<Status>(string.Format("{1}1/statuses/show/{0}.xml", id, ApiBaseUrl));
                if (MiniTwitter.Properties.Settings.Default.UseKanvasoLength)
                {
                    var match = kanvasoUrl.Match(result.Text);
                    if (match.Success)
                    {
                        ThreadPool.QueueUserWorkItem(code => { result.Text = KanvasoHelper.GetLongStatus((string)code) ?? result.Text; }, match.Groups["code"].Value);
                    }
                }

                AsyncDo(CacheOrUpdate, result);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public Status[] GetHomeTimeline(int count = 200, ulong? sinceId = null, ulong? maxId = null)
        {
            if (count <= 0 || count > 200)
            {
                throw new ArgumentException("Count必须大于0小于200", "count");
            }
            System.Diagnostics.Contracts.Contract.EndContractBlock();
            object p;
            if (maxId != null)
            {
                p = new { max_id = maxId, count=count };
            }
            else
            {
                p = new { count=count };
            }
            return GetStatuses(string.Format("{0}1/statuses/home_timeline.xml", ApiBaseUrl), p, ref sinceId);
        }

        public Status[] GetMentions(int count = 200, ulong? sinceId = null, ulong? maxId = null)
        {
            if (count <= 0 || count > 200)
            {
                throw new ArgumentException("Count必须大于0小于200", "count");
            }
            System.Diagnostics.Contracts.Contract.EndContractBlock();
            object p;
            if (maxId != null)
            {
                p = new { max_id = maxId, count = count };
            }
            else
            {
                p = new { count = count };
            }
            return GetStatuses(string.Format("{0}1/statuses/mentions.xml", ApiBaseUrl), p, ref sinceId);
        }
        private Status[] GetStatuses(string url, object param)
        {
            ulong? temp = null;
            return GetStatuses(url, param, ref temp);
        }

        private Status[] GetStatuses(string url, object param, ref ulong? since_id)
        {
            try
            {
                var statuses = Get<Statuses>(url, param).Status;

                if (statuses == null || statuses.Length == 0)
                {
                    return Statuses.Empty;
                }

                Array.ForEach(statuses, item => 
                { 
                    item.IsAuthor = item.Sender.ID == LoginedUser.ID;
                    item.IsMention = item.InReplyToUserID == LoginedUser.ID;
                    if (MiniTwitter.Properties.Settings.Default.UseKanvasoLength)
                    {
                        var match = kanvasoUrl.Match(item.Text);
                        if (match.Success)
                        {
                            ThreadPool.QueueUserWorkItem(code => { item.Text = KanvasoHelper.GetLongStatus((string)code) ?? item.Text; }, match.Groups["code"].Value);
                        }
                    }
                });
                AsyncDo(CacheOrUpdate, statuses);
                return statuses;// statuses.AsParallel().Where((status) => Properties.Settings.Default.GlobalFilter.Count != 0 ? Properties.Settings.Default.GlobalFilter.AsParallel().All((filter) => filter.Process(status)) : true).ToArray();
            }
            catch
            {
                return null;
            }
        }

        private DirectMessage[] GetMessages(string url, ref ulong? since_id)
        {
            try
            {
                var messages = since_id.HasValue ? Get<DirectMessages>(url, new { since_id = since_id.Value }).DirectMessage : Get<DirectMessages>(url).DirectMessage;

                if (messages == null || messages.Length == 0)
                {
                    return DirectMessages.Empty;
                }

                since_id = messages.First().ID;
                Array.ForEach(messages, item => item.IsAuthor = item.Sender.ID == LoginedUser.ID);

                return messages;
            }
            catch
            {
                return null;
            }
        }

        private List[] GetLists(string name)
        {
            try
            {
                long cursor = -1;
                var lists = new List<List>();

                while (true)
                {
                    var response = Get(string.Format("{1}1/{0}/lists.xml", name, ApiBaseUrl), new { cursor = cursor });

                    var xd = XDocument.Parse(response);

                    var temp = from p in xd.Descendants("list")
                               select new List { ID = (int)p.Element("id"), Name = (string)p.Element("name"), ScreenName = (string)p.Element("user").Element("screen_name") };

                    lists.AddRange(temp);

                    var element = xd.Descendants("next_cursor").FirstOrDefault();

                    if (element == null)
                    {
                        break;
                    }

                    var nextCursor = (long)element;

                    if (nextCursor == 0)
                    {
                        break;
                    }

                    cursor = nextCursor;
                }

                return lists.ToArray();
            }
            catch
            {
                return new List[0];
            }
        }

        private List[] GetListsSubscription(string name)
        {
            try
            {
                long cursor = -1;
                var lists = new List<List>();

                while (true)
                {
                    var response = Get(string.Format("{1}1/{0}/lists/subscriptions.xml", name, ApiBaseUrl), new { cursor = cursor });

                    var xd = XDocument.Parse(response);

                    var temp = from p in xd.Descendants("list")
                               select new List { ID = (int)p.Element("id"), Name = "@" + (string)p.Element("user").Element("screen_name") + "/" + (string)p.Element("name"), ScreenName = (string)p.Element("user").Element("screen_name") };

                    lists.AddRange(temp);

                    var element = xd.Descendants("next_cursor").FirstOrDefault();

                    if (element == null)
                    {
                        break;
                    }

                    var nextCursor = (long)element;

                    if (nextCursor == 0)
                    {
                        break;
                    }

                    cursor = nextCursor;
                }

                return lists.ToArray();
            }
            catch
            {
                return new List[0];
            }
        }

        public bool AddListUser(string name, int id, long user_id)
        {
            try
            {
                Post(string.Format("{2}1/{0}/{1}/members.xml", name, id, ApiBaseUrl), new { id = user_id });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public User[] GetListUsers(string name, int id)
        {
            try
            {
                long cursor = -1;
                var users = new List<User>();

                while (true)
                {
                    var response = Get(string.Format("{2}1/{0}/{1}/members.xml", name, id, ApiBaseUrl), new { cursor = cursor });

                    var xd = XDocument.Parse(response);

                    var temp = from p in xd.Descendants("user")
                                select new User { ID = (int)p.Element("id"), ScreenName = (string)p.Element("screen_name") };

                    users.AddRange(temp);

                    var element = xd.Descendants("next_cursor").FirstOrDefault();

                    if (element == null)
                    {
                        break;
                    }

                    var nextCursor = (long)element;

                    if (nextCursor == 0)
                    {
                        break;
                    }

                    cursor = nextCursor;
                }

                return users.ToArray();
            }
            catch
            {
                return Users.Empty;
            }
        }

        public Status[] GetListStatuses(string name, ulong since_id)
        {
            try
            {
                var screen_name = LoginedUser.ScreenName;

                if (name.StartsWith("@"))
                {
                    int index = name.IndexOf('/');
                    screen_name = name.Substring(1, index - 1);
                    name = name.Substring(index + 1);
                }

                var statuses = Get<Statuses>(string.Format("{2}1/{0}/lists/{1}/statuses.xml", screen_name, name, ApiBaseUrl), new { per_page = 200 }).Status;

                if (statuses == null)
                {
                    return Statuses.Empty;
                }

                Array.ForEach(statuses, item => { item.IsAuthor = item.Sender.ID == LoginedUser.ID; item.IsMention = item.InReplyToUserID == LoginedUser.ID; });

                return statuses;// statuses.AsParallel().Where((status) => Properties.Settings.Default.GlobalFilter.Count != 0 ? Properties.Settings.Default.GlobalFilter.AsParallel().All((filter) => filter.Process(status)) : true).ToArray();
            }
            catch
            {
                return Statuses.Empty;
            }
        }

        public Status[] Search(string query, ulong since_id)
        {
            try
            {
                var list = new List<Status>();
                using (var reader = XmlReader.Create(string.Format("{2}search.atom?q={0}&since_id={1}&rpp=100&lang=ja", Uri.EscapeDataString(query), since_id, SearchApiUrl)))
                {
                    var doc = XDocument.Load(reader);
                    var xmlns = XNamespace.Get("http://www.w3.org/2005/Atom");
                    var twitter = XNamespace.Get("http://api.twitter.com/");
                    var statuses = from entry in doc.Descendants(xmlns + "entry")
                                   let id = entry.Element(xmlns + "id")
                                   let author = entry.Element(xmlns + "author")
                                   let name = author.Element(xmlns + "name")
                                   let link = entry.Elements(xmlns + "link").Where(p => p.Attribute("rel").Value == "image").First()
                                   select new Status
                                   {
                                       ID = ulong.Parse(id.Value.Substring(id.Value.LastIndexOf(':') + 1)),
                                       CreatedAt = DateTime.Parse(entry.Element(xmlns + "published").Value),
                                       Text = entry.Element(xmlns + "title").Value,
                                       Source = entry.Element(twitter + "source").Value,
                                       Sender = new User
                                       {
                                           Name = name.Value.Substring(name.Value.IndexOf(' ') + 2, name.Value.Length - name.Value.IndexOf(' ') - 3),
                                           ScreenName = name.Value.Remove(name.Value.IndexOf(' ')),
                                           ImageUrl = link.Attribute("href").Value
                                       }
                                   };
                    list.AddRange(statuses);
                }

                list.ForEach(item => { item.IsAuthor = item.Sender.ScreenName == LoginedUser.ScreenName; });

                return list.ToArray();// list.AsParallel().Where((status) => Properties.Settings.Default.GlobalFilter.Count != 0 ? Properties.Settings.Default.GlobalFilter.AsParallel().All((filter) => filter.Process(status)) : true).ToArray();
            }
            catch
            {
                return Statuses.Empty;
            }
        }

        private int _failureCount = 0;

        public event EventHandler<StatusEventArgs> UserStreamUpdated;

        private Thread _thread;

        public void ChirpUserStream()
        {
            if ((_thread!=null)&&(_thread.IsAlive))
            {
                try
                {
                    _thread.Abort();
                    _thread = null;
                }
                catch
                {
                }
            }
            _thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        DateTime time;
                        using (var reader = new StreamReader(FetchStream(OAuthBase.HttpVerbs.Get, "https://userstream.twitter.com/2/user.json", null, out time)))
                        {
                            _failureCount = 0;

                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();

                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    continue;
                                }

                                using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(line), XmlDictionaryReaderQuotas.Max))
                                {
                                    var element = XElement.Load(jsonReader);

                                    if (element.Element("delete") != null)
                                    {
                                        var status = new Status
                                        {
                                            ID = (ulong)element.Element("delete").Element("status").Element("id")
                                        };

                                        UserStreamUpdated(this, new StatusEventArgs(status)
                                        {
                                            Action = StatusAction.Deleted
                                        });
                                    }
                                    else if (element.Element("user") != null)
                                    {
                                        var status = new Status
                                        {
                                            ID = (ulong)element.Element("id"),
                                            CreatedAt = DateTime.ParseExact(element.Element("created_at").Value, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                                            Text = element.Element("text").Value,
                                            Source = element.Element("source").Value,
                                            Favorited = (bool)element.Element("favorited"),
                                            Sender = new User
                                            {
                                                ID = (int)element.Element("user").Element("id"),
                                                ScreenName = element.Element("user").Element("screen_name").Value,
                                                Name = element.Element("user").Element("name").Value,
                                                ImageUrl = element.Element("user").Element("profile_image_url").Value,
                                                Description = element.Element("user").Element("description").Value,
                                                Protected = (bool)element.Element("user").Element("protected"),
                                                Location = element.Element("user").Element("location").Value,
                                                Verified = (bool)element.Element("user").Element("verified"),
                                                FavouritesCount = (int)element.Element("user").Element("favourites_count"),
                                                Followers = (int)element.Element("user").Element("followers_count"),
                                                Friends = (int)element.Element("user").Element("friends_count"),
                                                StatusesCount = (int)element.Element("user").Element("statuses_count"),
                                            },
                                        };

                                        if (element.Element("retweeted_status") != null)
                                        {
                                            status.ReTweetedStatus = new Status
                                            {
                                                ID = (ulong)element.Element("retweeted_status").Element("id"),
                                                CreatedAt = DateTime.ParseExact(element.Element("retweeted_status").Element("created_at").Value, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                                                Text = element.Element("retweeted_status").Element("text").Value,
                                                Source = element.Element("retweeted_status").Element("source").Value,
                                                Favorited = (bool)element.Element("retweeted_status").Element("favorited"),
                                                Sender = new User
                                                {
                                                    ID = (int)element.Element("retweeted_status").Element("user").Element("id"),
                                                    ScreenName = element.Element("retweeted_status").Element("user").Element("screen_name").Value,
                                                    Name = element.Element("retweeted_status").Element("user").Element("name").Value,
                                                    ImageUrl = element.Element("retweeted_status").Element("user").Element("profile_image_url").Value,
                                                    Description = element.Element("retweeted_status").Element("user").Element("description").Value,
                                                    Protected = (bool)element.Element("retweeted_status").Element("user").Element("protected"),
                                                    Location = element.Element("retweeted_status").Element("user").Element("location").Value,
                                                    Verified = (bool)element.Element("retweeted_status").Element("user").Element("verified"),
                                                    FavouritesCount = (int)element.Element("retweeted_status").Element("user").Element("favourites_count"),
                                                    Followers = (int)element.Element("retweeted_status").Element("user").Element("followers_count"),
                                                    Friends = (int)element.Element("retweeted_status").Element("user").Element("friends_count"),
                                                    StatusesCount = (int)element.Element("retweeted_status").Element("user").Element("statuses_count"),
                                                },
                                            };
                                        }

                                        if (!string.IsNullOrEmpty(element.Element("in_reply_to_status_id").Value))
                                        {
                                            status.InReplyToStatusID = (ulong)element.Element("in_reply_to_status_id");
                                        }

                                        if (!string.IsNullOrEmpty(element.Element("in_reply_to_user_id").Value))
                                        {
                                            status.InReplyToUserID = (int)element.Element("in_reply_to_user_id");
                                        }

                                        status.IsAuthor = status.Sender.ID == LoginedUser.ID;
                                        status.IsMention = status.InReplyToUserID == LoginedUser.ID;
                                        if (true)//Properties.Settings.Default.GlobalFilter.Count != 0 ? Properties.Settings.Default.GlobalFilter.AsParallel().All((filter) => filter.Process(status)) : true)
                                        {
                                            UserStreamUpdated(this, new StatusEventArgs(status)
                                                                    {
                                                                        Action = StatusAction.Update
                                                                    });
                                        }

                                        AsyncDo(CacheOrUpdate, status);
                                    }
                                    else if (element.Element("event") != null)
                                    {
                                        if (element.Element("event").Value == "favorite")
                                        {
                                            var status = new Status
                                            {
                                                ID = (ulong)element.Element("target_object").Element("id"),
                                                Sender = new User
                                                {
                                                    ID = (int)element.Element("source").Element("id")
                                                }
                                            };

                                            UserStreamUpdated(this, new StatusEventArgs(status) { Action = StatusAction.Favorited });
                                        }
                                        else if (element.Element("event").Value == "unfavorite")
                                        {
                                            var status = new Status
                                            {
                                                ID = (ulong)element.Element("target_object").Element("id"),
                                                Sender = new User { ID = (int)element.Element("source").Element("id") }
                                            };

                                            UserStreamUpdated(this, new StatusEventArgs(status) { Action = StatusAction.Unfavorited });
                                        }
                                    }
                                }
                            }
                        }
                        _failureCount = 1;
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch
                    {
                        _failureCount++;

                        if (_failureCount > 10)
                        {
                            _failureCount = 10;
                        }
                    }
                    finally
                    {
                        Thread.SpinWait(1000 * (int)Math.Pow(_failureCount, _failureCount));
                    }
                }
            });

            _thread.IsBackground = true;

            _thread.Start();
        }

    #region Cache
        #region StatusCache
        private ConcurrentDictionary<ulong, Status> statusesCache = new ConcurrentDictionary<ulong, Status>();
        private void CacheOrUpdate(Status tweet)
        {
            if (tweet == null)
            {
                return;
            }
#if DEBUG
            Debug.WriteLine("Updating tweet {0}.", tweet.ID);
#endif
            if (statusesCache.ContainsKey(tweet.ID))
            {
#if DEBUG
                Debug.WriteLine("Hit Cache, ID:{0}", tweet.ID);
#endif
                Status target;
                statusesCache.TryGetValue(tweet.ID, out target);
                if (target != null)
                {
                    target.CreatedAt = tweet.CreatedAt;
                    target.Favorited = tweet.Favorited;
                    target.LastModified = tweet.LastModified;
                    target.ReTweetCount = tweet.ReTweetCount;
                }
            }
            else
            {
#if DEBUG
                Debug.WriteLine("Cache Not Hit, cahcing...");
#endif
                statusesCache.TryAdd(tweet.ID, tweet);
                CacheOrUpdate(tweet.Recipient);
                if (tweet.Recipient != null && usersCache.ContainsKey(tweet.Recipient.ID))
                {
                    User recipient;
                    usersCache.TryGetValue(tweet.Recipient.ID, out recipient);
                    tweet.Recipient = recipient;
                }
                CacheOrUpdate(tweet.Sender);
                if (tweet.Sender != null && usersCache.ContainsKey(tweet.Sender.ID))
                {
                    User sender;
                    usersCache.TryGetValue(tweet.Sender.ID, out sender);
                    tweet.Recipient = sender;
                }
            }
            UpdateAllStatusesAndUsers();
        }
        private void CacheOrUpdate(Statuses tweets)
        {
#if DEBUG
            Debug.WriteLine("Updating {0} tweets.", tweets.Status.Length);
#endif
            foreach (var tweet in tweets.Status.Reverse())
            {
                CacheOrUpdate(tweet);
            }
        }
        private void CacheOrUpdate(IEnumerable<Status> tweets)
        {
#if DEBUG
            Debug.WriteLine("Updating {0} tweets.", tweets.Count());
#endif
            foreach (var tweet in tweets.Reverse())
            {
                CacheOrUpdate(tweet);
            }
        }
        #endregion
        #region UserCache
        private ConcurrentDictionary<int, User> usersCache = new ConcurrentDictionary<int, User>();
        private void CacheOrUpdate(User user)
        {
            if (user == null)
            {
                return;
            }
            if (usersCache.ContainsKey(user.ID))
            {
                User target;
                usersCache.TryGetValue(user.ID, out target);
                if (target != null)
                {
                    target.Description = user.Description;
                    target.FavouritesCount = user.FavouritesCount;
                    target.Followers = user.Followers;
                    target.Friends = user.Friends;
                    target.ImageUrl = user.ImageUrl;
                    target.LastModified = user.LastModified;
                    target.Location = user.Location;
                    target.Name = user.Name;
                    target.Protected = user.Protected;
                    target.ScreenName = user.ScreenName;
                    target.StatusesCount = user.StatusesCount;
                    target.Url = user.Url;
                    target.Verified = user.Verified;
                }
            }
            else
            {
                usersCache.TryAdd(user.ID, user);
            }
        }
        private void CacheOrUpdate(Users users)
        {
            users.User.AsParallel().ForAll(CacheOrUpdate);
        }
        private void CacheOrUpdate(IEnumerable<User> users)
        {
            users.AsParallel().ForAll(CacheOrUpdate);
        }
        #endregion
        #region Helpers
        private void AsyncDo<T>(Action<T> action,T state)
        {
            ThreadPool.QueueUserWorkItem(s => action((T)s), state);
        }

        private SpinLock updateLock = 
#if DEBUG
            new SpinLock(true);
#else
            new SpinLock();
#endif
        private bool FirstRun = true;
        private void UpdateAllStatusesAndUsers()
        {
            //if (!FirstRun)
            //{
            //    return;
            //}
            bool GotLock = false;
            try
            {
                updateLock.TryEnter(0, ref GotLock);
                if (GotLock)
                {
                    FirstRun = false;
                    User user;
                    foreach (var tweet in statusesCache.Values)
                    {
                        if (tweet == null)
                        {
                            continue;
                        }
                        if (tweet.Recipient != null && usersCache.TryGetValue(tweet.Recipient.ID, out user))
                        {
                            tweet.Recipient = user;
                        }
                        if (tweet.Sender != null && usersCache.TryGetValue(tweet.Sender.ID, out user))
                        {
                            tweet.Sender = user;
                        }
                    }
                    Status status;
                    foreach (User usr in usersCache.Values)
                    {
                        if (usr == null)
                        {
                            continue;
                        }
                        if (usr.Status != null && statusesCache.TryGetValue(usr.Status.ID, out status))
                        {
                            usr.Status = status;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                if (GotLock)
                {
                    updateLock.Exit();
                }
            }
        }
        #endregion
    #endregion

    }
}