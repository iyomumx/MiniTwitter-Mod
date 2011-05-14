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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

using MiniTwitter.Extensions;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("user")]
    public class User : PropertyChangedBase, IEquatable<User>, ITimeTaged
    {
        static User()
        {
            _networkAvailable = NetworkInterface.GetIsNetworkAvailable();
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }

        static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _networkAvailable = e.IsAvailable;

            if (_networkAvailable)
            {
                lock (_iconCache)
                {
                    var keys = _iconCache.Where(p => p.Value == null).Select(p => p.Key);
                    foreach (var key in keys)
                    {
                        _iconCache.Remove(key);
                    }
                }
            }
        }

        private int id;

        [XmlElement("id")]
        public int ID
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        private string name;

        [XmlElement("name")]
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private string screenName;

        [XmlElement("screen_name")]
        public string ScreenName
        {
            get { return screenName; }
            set
            {
                if (screenName != value)
                {
                    screenName = value;
                    OnPropertyChanged("ScreenName");
                }
            }
        }
        //添加部分

        private int followers;

        [XmlElement("followers_count")]
        public int Followers
        {
            get
            {
                return followers;
            }
            set
            {
                if (followers != value)
                {
                    followers = value;
                    OnPropertyChanged("Followers");
                }
            }
        }

        private int friends;

        [XmlElement("friends_count")]
        public int Friends
        {
            get
            {
                return friends;
            }
            set
            {
                if (friends != value)
                {
                    friends = value;
                    OnPropertyChanged("Friends");
                }
            }
        }

        private int status_count;

        [XmlElement("statuses_count")]
        public int StatusesCount
        {
            get
            {
                return status_count;
            }
            set
            {
                if (status_count != value)
                {
                    status_count = value;
                    OnPropertyChanged("StatusesCount");
                }
            }
        }

        private int favourites_count;

        [XmlElement("favourites_count")]
        public int FavouritesCount
        {
            get
            {
                return favourites_count;
            }
            set
            {
                if (favourites_count != value)
                {
                    favourites_count = value;
                    OnPropertyChanged("FavouritesCount");
                }
            }
        }

        private bool verified;

        [XmlElement("verified")]
        public bool Verified
        {
            get
            {
                return verified;
            }
            set
            {
                if (verified != value)
                {
                    verified = value;
                    OnPropertyChanged("Verified");
                }
            }
        }
        //添加部分结束
        private string location;

        [XmlElement("location")]
        public string Location
        {
            get { return location; }
            set
            {
                if (location != value)
                {
                    location = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        private string description;

        [XmlElement("description")]
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        private string imageUrl;

        [XmlElement("profile_image_url")]
        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                if (imageUrl != value)
                {
                    imageUrl = value;
                    OnPropertyChanged("ImageUrl");
                }
            }
        }

        [NonSerialized]
        private ImageSource _icon;

        private int _retry = 0;

        private static volatile bool _networkAvailable;

        private static readonly Dictionary<string, List<User>> _processUsers = new Dictionary<string, List<User>>();
        private static readonly Dictionary<string, ImageSource> _iconCache = new Dictionary<string, ImageSource>();

        [XmlIgnore]
        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    lock (_iconCache)
                    {
                        if (_iconCache.ContainsKey(imageUrl))
                        {
                            _icon = _iconCache[imageUrl];

                            if (_icon == null)
                            {
                                lock (_processUsers)
                                {
                                    // アイコンダウンロードを予約
                                    if (!_processUsers.ContainsKey(imageUrl))
                                    {
                                        _processUsers.Add(imageUrl, new List<User>());
                                    }
                                    _processUsers[imageUrl].Add(this);
                                }

                                _iconCache.Remove(imageUrl);
                            }
                        }
                        else
                        {
                            if (_retry > 5)
                            {
                                return null;
                            }

                            _iconCache.Add(imageUrl, null);

                            if (_networkAvailable)
                            {
                                ThreadPool.QueueUserWorkItem(state =>
                                    {
                                        try
                                        {
                                            using (var client = new WebClient())
                                            {
                                                var data = client.DownloadData(imageUrl);
                                                var stream = new MemoryStream(data);
                                                var bitmap = new BitmapImage();
                                                bitmap.BeginInit();
                                                bitmap.StreamSource = stream;
                                                bitmap.DecodePixelHeight = 48;
                                                bitmap.DecodePixelWidth = 48;
                                                bitmap.EndInit();

                                                bitmap.Freeze();

                                                lock (_iconCache)
                                                {
                                                    _iconCache[imageUrl] = bitmap;
                                                }

                                                App.Current.AsyncInvoke(p =>
                                                {
                                                    Icon = p;
                                                    lock (_processUsers)
                                                    {
                                                        List<User> users;
                                                        if (_processUsers.TryGetValue(imageUrl, out users))
                                                        {
                                                            foreach (var item in users)
                                                            {
                                                                item.Icon = p;
                                                            }
                                                            _processUsers.Remove(imageUrl);
                                                        }
                                                    }
                                                }, bitmap, System.Windows.Threading.DispatcherPriority.Background);
                                            }
                                        }
                                        catch
                                        {
                                            lock (_iconCache)
                                            {
                                                _iconCache.Remove(imageUrl);
                                                _retry++;
                                            }
                                        }
                                    });
                            }
                        }
                    }
                }
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private string url;

        [XmlElement("url")]
        public string Url
        {
            get { return url; }
            set
            {
                if (url != value)
                {
                    url = value;
                    OnPropertyChanged("Url");
                }
            }
        }

        private bool _protected;

        [XmlElement("protected")]
        public bool Protected
        {
            get { return _protected; }
            set
            {
                if (_protected != value)
                {
                    _protected = value;
                    OnPropertyChanged("Protected");
                }
            }
        }

        private Status status;

        [XmlElement("status")]
        public Status Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        public bool Equals(User other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.ID == other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID;
        }

        private DateTime lastModified;

        [XmlIgnore()]
        public DateTime LastModified
        {
            get
            {
                return lastModified;
            }
            set
            {
                if (lastModified<value)
                {
                    lastModified = value;
                    OnPropertyChanged("LastModified");
                    UpdateChild();
                }
            }
        }

        public void UpdateChild()
        {
            throw new NotImplementedException();
        }
    }
}
