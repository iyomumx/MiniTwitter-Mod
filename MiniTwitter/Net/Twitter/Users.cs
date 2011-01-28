using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("users")]
    public class Users
    {
        [XmlElement("user")]
        public User[] User { get; set; }

        public static readonly User[] Empty = new User[0];
    }
}
