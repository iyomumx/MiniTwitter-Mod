using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using MiniTwitter.Extensions;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("status")]
    public class Status : PropertyChangedBase, ITwitterItem, ITimeTaged
    {
        private DateTime createdAt;

        [XmlIgnore]
        public DateTime CreatedAt
        {
            get { return createdAt; }
            set
            {
                if (createdAt != value)
                {
                    createdAt = value;
                    OnPropertyChanged("CreatedAt");
                }
            }
        }

        private string createdAtInternal;

        [XmlElement("created_at")]
        public string CreatedAtInternal
        {
            get { return createdAtInternal; }
            set
            {
                if (createdAtInternal != value)
                {
                    if (!value.IsNullOrEmpty())
                    {
                        createdAtInternal = value;
                        CreatedAt = TwitterExtension.ParseDateTime(createdAtInternal);
                    }
                }
            }
        }

        private string relativeTime;

        [XmlIgnore]
        public string RelativeTime
        {
            get
            {
                this.UpdateRelativeTime();
                return relativeTime;
            }
            set
            {
                if (relativeTime != value)
                {
                    relativeTime = value;
                }
            }
        }

        private ulong id;

        [XmlElement("id")]
        public ulong ID
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

        private string text;

        [XmlElement("text")]
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = TwitterClient.Unescape(value);
                    OnPropertyChanged("Text");
                }
            }
        }

        private string source;

        [XmlElement("source")]
        public string Source
        {
            get { return source; }
            set
            {
                if (source != value)
                {
                    if (!value.IsNullOrEmpty())
                    {
                        var match = Regex.Match(value, "href=\"(?<uri>[^\"]+?)\"");
                        if (match.Success)
                        {
                            SourceUri = new Uri(match.Groups["uri"].Value);
                        }
                        else
                        {
                            SourceUri = twitterMainUri;
                        }
                        source = Regex.Replace(value, @"<(.|\n)*?>", string.Empty);
                        OnPropertyChanged("Source");
                    }
                }
            }
        }

        private static readonly Uri twitterMainUri=new Uri("https://twitter.com/");
        private Uri _sourceUri;

        [XmlIgnore()]
        public Uri SourceUri
        {
            get
            {
                return _sourceUri;
            }
            private set
            {
                if (_sourceUri != value)
                {
                    _sourceUri = value;
                    OnPropertyChanged("SourceUri");
                }
            }
        }

        private bool favorited;

        [XmlElement("favorited")]
        public bool Favorited
        {
            get { return favorited; }
            set
            {
                if (favorited != value)
                {
                    favorited = value;
                    OnPropertyChanged("Favorited");
                }
            }
        }

        private ulong inReplyToStatusID;

        [XmlIgnore]
        public ulong InReplyToStatusID
        {
            get { return inReplyToStatusID; }
            set
            {
                if (inReplyToStatusID != value)
                {
                    inReplyToStatusID = value;
                    OnPropertyChanged("InReplyToStatusID");
                }
            }
        }

        private string inReplyToStatusIDInternal;

        [XmlElement("in_reply_to_status_id", IsNullable = true)]
        public string InReplyToStatusIDInternal
        {
            get { return inReplyToStatusIDInternal; }
            set
            {
                if (inReplyToStatusIDInternal != value)
                {
                    inReplyToStatusIDInternal = value;
                    if (!inReplyToStatusIDInternal.IsNullOrEmpty())
                    {
                        InReplyToStatusID = ulong.Parse(inReplyToStatusIDInternal);
                    }
                }
            }
        }

        private long inReplyToUserID;

        [XmlIgnore]
        public long InReplyToUserID
        {
            get { return inReplyToUserID; }
            set
            {
                if (inReplyToUserID != value)
                {
                    inReplyToUserID = value;
                    OnPropertyChanged("InReplyToUserID");
                }
            }
        }

        private string inReplyToUserIDInternal;

        [XmlElement("in_reply_to_user_id", IsNullable = true)]
        public string InReplyToUserIDInternal
        {
            get { return inReplyToUserIDInternal; }
            set
            {
                if (inReplyToUserIDInternal != value)
                {
                    inReplyToUserIDInternal = value;
                    if (!inReplyToUserIDInternal.IsNullOrEmpty())
                    {
                        InReplyToUserID = long.Parse(inReplyToUserIDInternal);
                    }
                }
            }
        }

        private string inReplyToScreenName;

        [XmlElement("in_reply_to_screen_name", IsNullable = true)]
        public string InReplyToScreenName
        {
            get { return inReplyToScreenName; }
            set
            {
                if (inReplyToScreenName != value)
                {
                    inReplyToScreenName = value;
                    OnPropertyChanged("InReplyToScreenName");
                }
            }
        }

        private Status retweetedStatus;

        [XmlElement("retweeted_status")]
        public Status ReTweetedStatus
        {
            get { return retweetedStatus; }
            set
            {
                if (retweetedStatus != value)
                {
                    retweetedStatus = value;
                    OnPropertyChanged("ReTweetedStatus");
                    OnPropertyChanged("IsReTweeted");
                }
            }
        }

        private string retweetCount = "0";

        [XmlElement("retweet_count")]
        public string ReTweetCount
        {
            get { return retweetCount; }
            set
            {
                if (retweetCount != value && value != null)
                {
                    retweetCount = value;
                    OnPropertyChanged("ReTweetCount");
                }
            }
        }

        private User sender;

        [XmlElement("user")]
        public User Sender
        {
            get { return sender; }
            set
            {
                if (sender != value)
                {
                    sender = value;
                    OnPropertyChanged("Sender");
                }
            }
        }

        private User recipient;

        [XmlIgnore]
        public User Recipient
        {
            get { return recipient; }
            set
            {
                if (recipient != value)
                {
                    recipient = value;
                    OnPropertyChanged("Recipient");
                }
            }
        }

        public bool IsAuthor { get; set; }

        private bool isNewest;

        [XmlIgnore]
        public bool IsNewest
        {
            get { return isNewest; }
            set
            {
                if (isNewest != value)
                {
                    isNewest = value;
                    OnPropertyChanged("IsNewest");
                }
            }
        }

        [XmlIgnore]
        public bool IsReply
        {
            get { return InReplyToStatusID != 0; }
        }

        public bool IsMention { get; set; }

        [XmlIgnore]
        public bool IsMessage
        {
            get { return false; }
        }

        [XmlIgnore]
        public bool IsReTweeted
        {
            get { return retweetedStatus != null; }
        }

        public bool Equals(ITwitterItem other)
        {
            if (other == null)
            {
                return false;
            }
            else if (!(other is Status))
            {
                return false;
            }
            if (((Status)other).IsReTweeted && IsReTweeted)
            {
                return (ReTweetedStatus.ID == ((Status)other).ReTweetedStatus.ID);
            }
            else if (((Status)other).IsReTweeted)
            {
                return (((Status)other).ReTweetedStatus.ID == this.ID);
            }
            else if (IsReTweeted)
            {
                return (ReTweetedStatus.ID == other.ID);
            }
            return (this.ID == other.ID);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
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
            this.Recipient.LastModified = this.LastModified;
            this.Sender.LastModified = this.LastModified;
        }
    }
}
