using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("users")]
    public class Users:ITimeTaged
    {
        [XmlElement("user")]
        public User[] User { get; set; }

        public static readonly User[] Empty = new User[0];

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
                    UpdateChild();
                }
            }
        }

        public void UpdateChild()
        {
            foreach (var item in User)
            {
                item.LastModified = this.LastModified;
            }
        }
    }
}
