using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("entities")]
    public class Entities : PropertyChangedBase
    {
        private UserMentions userMentions;

        [XmlElement("user_mentions")]
        public UserMentions UserMentions
        {
            get { return userMentions; }
            set
            {
                if (userMentions != value)
                {
                    userMentions = value;
                    OnPropertyChanged("UserMentions");
                }
            }
        }

        private Urls urls;

        [XmlElement("urls")]
        public Urls Urls
        {
            get { return urls; }
            set 
            {
                if (urls != value)
                {
                    urls = value;
                    OnPropertyChanged("Urls");
                }
            }
        }

        private HashTags hashtags;

        [XmlElement("hashtags")]
        public HashTags Hashtags
        {
            get { return hashtags; }
            set 
            {
                if (hashtags != value)
                {
                    hashtags = value;
                    OnPropertyChanged("Hashtags");
                }
            }
        }
    }
}
