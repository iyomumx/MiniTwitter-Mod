using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("hashtags")]
    public class HashTags
    {
        [XmlElement("hashtag")]
        public HashTag[] Hashtag { get; set; }

        public static readonly HashTag[] Empty = new HashTag[0];
    }
}
