using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("urls")]
    public class Urls
    {
        [XmlElement("url")]
        public Url[] URL { get; set; }

        public static readonly Url[] Empty = new Url[0];
    }
}
