using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("user_mention")]
    public class UserMention : PropertyChangedBase
    {
        private int startIndex;

        [XmlAttribute("start")]
        public int StartIndex
        {
            get { return startIndex; }
            set 
            {
                if (startIndex != value)
                {
                    startIndex = value;
                    OnPropertyChanged("StartIndex");
                }
            }
        }
        private int endIndex;

        [XmlAttribute("end")]
        public int EndIndex
        {
            get { return endIndex; }
            set 
            {
                if (endIndex != value)
                {
                    endIndex = value;
                    OnPropertyChanged("EndIndex");
                }
            }
        }

        private int id;

        [XmlElement("id")]
        public int ID
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

        private string screenName;

        [XmlElement("screen_name")]
        public string ScreenName
        {
            get { return screenName; }
            set 
            {
                if (screenName != value)
                {
                    screenName = value;
                    OnPropertyChanged("ScreenName");
                }
            }
        }

        private string name;

        [XmlElement("name")]
        public string Name
        {
            get { return name; }
            set 
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
    }
}
