using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using MiniTwitter.Extensions;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("direct_message")]
    public class DirectMessage : PropertyChangedBase, ITwitterItem
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
                    createdAtInternal = value;
                    if (!string.IsNullOrEmpty(createdAtInternal))
                    {
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

        private User sender;

        [XmlElement("sender")]
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

        [XmlElement("recipient")]
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

        [XmlIgnore]
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
        public bool IsMessage
        {
            get { return true; }
        }

        [XmlIgnore]
        public bool IsReTweeted
        {
            get { return false; }
        }

        public bool Equals(ITwitterItem other)
        {
            if (other == null)
            {
                return false;
            }
            else if (!(other is DirectMessage))
            {
                return false;
            }
            return (this.ID == other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
