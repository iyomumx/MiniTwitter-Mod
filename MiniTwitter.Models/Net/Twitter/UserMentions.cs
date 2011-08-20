using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("user_mentions")]
    public class UserMentions : PropertyChangedBase
    {
        [XmlElement("user_mention")]
        public UserMention[] UserMention { get; set; }

        public static readonly UserMention[] Empty = new UserMention[0];
    }
}
