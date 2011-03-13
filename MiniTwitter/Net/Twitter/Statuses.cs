using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("statuses")]
    public class Statuses
    {
        [XmlElement("status")]
        public Status[] Status { get; set; }

        public static readonly Status[] Empty = new Status[0];
    }
}
