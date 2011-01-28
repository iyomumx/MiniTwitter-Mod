using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;

using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Net
{
    class TwitterClient : OAuthBase
    {
        static TwitterClient()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.Expect100Continue = false;
        }

        public TwitterClient(string consumerKey, string consumerSecret)
            : base(consumerKey, consumerSecret)
        {
            RetryCount = 5;
        }

        private readonly object thisLock = new object();

        private static readonly Regex schemaRegex = new Regex(@"(https?:\/\/[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex messageRegex = new Regex(@"^d\s([a-zA-Z_0-9]+)\s", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex unescapeRegex = new Regex("&([gl]t);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            get { return IsLogined ? GetStatuses("http://api.twitter.com/1/statuses/home_timeline.xml", new { count = 200 }, ref recentId) : Statuses.Empty; }
        }

        private ulong? repliesId;

        public Status[] RepliesTimeline
        {
            get { return IsLogined ? GetStatuses("http://api.twitter.com/1/statuses/mentions.xml", new { count = 100 }, ref repliesId) : Statuses.Empty; }
        }

        private ulong? archiveId;

        public Status[] ArchiveTimeline
        {
            get { return IsLogined ? GetStatuses("http://api.twitter.com/1/statuses/user_timeline.xml", new { count = 100 }, ref archiveId) : Statuses.Empty; }
        }

        private ulong? receivedId;

        public DirectMessage[] ReceivedMessages
        {
            get { return IsLogined ? GetMessages("http://api.twitter.com/1/direct_messages.xml", ref receivedId) : DirectMessages.Empty; }
        }

        private ulong? sentId;

        public DirectMessage[] SentMessages
        {
            get { return IsLogined ? GetMessages("http://api.twitter.com/1/direct_messages/sent.xml", ref sentId) : DirectMessages.Empty; }
        }

        public Status[] Favorites
        {
            get { return IsLogined ? GetStatuses("http://api.twitter.com/1/favorites.xml", new { count = 200 }) : Statuses.Empty; }
        }

        public User[] Friends
        {
            get { return IsLogined ? GetUsers("http://api.twitter.com/1/statuses/friends.xml") : Users.Empty; }
        }

        public User[] Followers
        {
            get { return IsLogined ? GetUsers("http://api.twitter.com/1/statuses/followers.xml") : Users.Empty; }
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

        public event EventHandler UpdateFailure;

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

        public void Update(string text)
        {
            Update(text, null, null, null);
        }

        public void Update(string text, ulong? replyId, double? latitude, double? longitude)
        {
            if (text.Length > 140)
            {
                text = schemaRegex.Replace(text,
                                             match =>
                                             googlHelper.ShortenUrl(match.Groups[1].Value, BitlyHelper.ConvertTo));
            }
            else
            {
                text = schemaRegex.Replace(text,
                                             match =>
                                             match.Groups[1].Length > 30 || match.Groups[1].Value.IndexOfAny(new[] { '!', '?' }) != -1
                                             ? googlHelper.ShortenUrl(match.Groups[1].Value, BitlyHelper.ConvertTo) : match.Groups[1].Value);
            }
            Match m = messageRegex.Match(text);
            if (m.Success)
            {
                string user = m.Groups[1].Value;
                text = text.Substring(m.Length);
                UpdateMessage(user, text);
            }
            else
            {
                UpdateStatus(text, replyId, latitude, longitude);
            }
        }

        public Status ReTweet(ulong id)
        {
            try
            {
                return Post<Status>(string.Format("http://api.twitter.com/1/statuses/retweet/{0}.xml", id));
            }
            catch
            {
                return null;
            }
        }

        private void UpdateStatus(string text, ulong? replyId, double? latitude, double? longitude)
        {
            if (!Footer.IsNullOrEmpty())
            {
                if (text.Length > 140 - Footer.Length)
                {
                    text = text.Substring(0, 139 - Footer.Length);// +"...";
                }
                text += " " + Footer;
            }
            else
            {
                if (text.Length > 140)
                {
                    text = text.Substring(0, 140);// +"...";
                }
            }
            try
            {
                Status status;
                if (replyId.HasValue)
                {
                    if (latitude.HasValue && longitude.HasValue)
                    {
                        status = Post<Status>("http://api.twitter.com/1/statuses/update.xml", new { status = text, in_reply_to_status_id = replyId.Value, lat = latitude.Value, @long = longitude.Value });
                    }
                    else
                    {
                        status = Post<Status>("http://api.twitter.com/1/statuses/update.xml", new { status = text, in_reply_to_status_id = replyId.Value });
                    }
                }
                else
                {
                    if (latitude.HasValue && longitude.HasValue)
                    {
                        status = Post<Status>("http://api.twitter.com/1/statuses/update.xml", new { status = text, lat = latitude.Value, @long = longitude.Value });
                    }
                    else
                    {
                        status = Post<Status>("http://api.twitter.com/1/statuses/update.xml", new { status = text });
                    }
                }
                status.IsAuthor = true;
                if (Updated != null)
                {
                    Updated(this, new UpdateEventArgs(status));
                }
            }
            catch
            {
                if (UpdateFailure != null)
                {
                    UpdateFailure(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateMessage(string user, string text)
        {
            try
            {
                var message = Post<DirectMessage>("http://api.twitter.com/1/direct_messages/new.xml", new { user = user, text = text });
                message.IsAuthor = true;
                if (Updated != null)
                {
                    Updated(this, new UpdateEventArgs(message));
                }
            }
            catch
            {
                if (UpdateFailure != null)
                {
                    UpdateFailure(this, EventArgs.Empty);
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
                Delete(string.Format("http://api.twitter.com/1/statuses/destroy/{0}.xml", id));

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
                Delete(string.Format("http://api.twitter.com/1/direct_messages/destroy/{0}.xml", id));

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
                Post(string.Format("http://api.twitter.com/1/favorites/create/{0}.xml", id));

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
                Delete(string.Format("http://api.twitter.com/1/favorites/destroy/{0}.xml", id));

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
                Post("http://api.twitter.com/1/friendships/create.xml", new { screen_name = screen_name });

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
                Post("http://api.twitter.com/1/friendships/destroy.xml", new { screen_name = screen_name });

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
                Post("http://api.twitter.com/1/blocks/create.xml", new { screen_name = screen_name });

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
                Delete("http://api.twitter.com/1/blocks/destroy.xml", new { screen_name = screen_name });

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
                return Get<User>("http://api.twitter.com/1/account/verify_credentials.xml");
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
                return Get<User>("http://api.twitter.com/1/users/show.xml", new { user_id = id });
            }
            catch
            {
                return null;
            }
        }

        private User GetUser(string name)
        {
            try
            {
                return Get<User>("http://api.twitter.com/1/users/show.xml", new { screen_name = name });
            }
            catch
            {
                return null;
            }
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
                return Get<Status>(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml", id));
            }
            catch
            {
                return null;
            }
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

                if (statuses == null)
                {
                    return Statuses.Empty;
                }

                Array.ForEach(statuses, item => { item.IsAuthor = item.Sender.ID == LoginedUser.ID; item.IsMention = item.InReplyToUserID == LoginedUser.ID; });

                return statuses;
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

                if (messages == null)
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
                    var response = Get(string.Format("http://api.twitter.com/1/{0}/lists.xml", name), new { cursor = cursor });

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
                    var response = Get(string.Format("http://api.twitter.com/1/{0}/lists/subscriptions.xml", name), new { cursor = cursor });

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
                Post(string.Format("http://api.twitter.com/1/{0}/{1}/members.xml", name, id), new { id = user_id });

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
                    var response = Get(string.Format("http://api.twitter.com/1/{0}/{1}/members.xml", name, id), new { cursor = cursor });

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

                var statuses = Get<Statuses>(string.Format("http://api.twitter.com/1/{0}/lists/{1}/statuses.xml", screen_name, name), new { per_page = 200 }).Status;

                if (statuses == null)
                {
                    return Statuses.Empty;
                }

                Array.ForEach(statuses, item => { item.IsAuthor = item.Sender.ID == LoginedUser.ID; item.IsMention = item.InReplyToUserID == LoginedUser.ID; });

                return statuses;
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
                using (var reader = XmlReader.Create(string.Format("http://search.twitter.com/search.atom?q={0}&since_id={1}&rpp=100&lang=ja", Uri.EscapeDataString(query), since_id)))
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

                return list.ToArray();
            }
            catch
            {
                return Statuses.Empty;
            }
        }

        private int _failureCount = 0;

        public event EventHandler<StatusEventArgs> UserStreamUpdated;

        public void ChirpUserStream()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                DateTime time;

                try
                {
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

                                    UserStreamUpdated(this, new StatusEventArgs(status) { Action = StatusAction.Deleted });
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

                                    UserStreamUpdated(this, new StatusEventArgs(status) { Action = StatusAction.Update });
                                }
                                else if (element.Element("event") != null)
                                {
                                    if (element.Element("event").Value == "favorite")
                                    {
                                        var status = new Status
                                        {
                                            ID = (ulong)element.Element("target_object").Element("id"),
                                            Sender = new User { ID = (int)element.Element("source").Element("id") }
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
                }
                catch
                {
                    _failureCount++;

                    if (_failureCount > 10)
                    {
                        _failureCount = 10;
                    }

                    Thread.Sleep(1000 * (int)Math.Pow(_failureCount, _failureCount));

                    ChirpUserStream();
                }
            });
        }
    }
}