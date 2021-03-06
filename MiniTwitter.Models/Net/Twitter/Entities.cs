﻿using System;
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

        private media media;

        [XmlElement("media")]
        public media Media
        {
            get { return media; }
            set
            {
                if (media!=value)
                {
                    media = value;
                    OnPropertyChanged("Media");
                }
            }
        }

#if DEBUG
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (UserMentions != null && UserMentions.UserMention != null)
            {
                UserMentions.UserMention.AsParallel().ForAll(um => sb.AppendFormat("{0}-{1};ID {2};Name {3};Screen Name {4}\n", um.StartIndex, um.EndIndex, um.ID, um.Name, um.ScreenName));
            }
            if (Urls != null && Urls.URL != null)
            {
                Urls.URL.AsParallel().ForAll(url => sb.AppendFormat("{0}, At {1}, Is {2}", url.URL, url.DisplayUrl, url.ExpandedUrl));
            }
            return sb.ToString();
        }
#endif
    }
}
