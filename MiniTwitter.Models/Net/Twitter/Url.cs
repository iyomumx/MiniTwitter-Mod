using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("url")]
    public class Url : PropertyChangedBase
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

        private string url;

        [XmlElement("url")]
        public string URL
        {
            get { return url; }
            set
            {
                if (url != value)
                {
                    url = value;
                    OnPropertyChanged("Url");
                }
            }
        }

        private string displayUrl;

        [XmlElement("display_url")]
        public string DisplayUrl
        {
            get { return displayUrl; }
            set
            {
                if (displayUrl != value)
                {
                    displayUrl = value;
                    OnPropertyChanged("DisplayUrl");
                }
            }
        }

        private string expandedUrl;

        [XmlElement("expanded_url")]
        public string ExpandedUrl
        {
            get { return expandedUrl; }
            set
            {
                if (expandedUrl != value)
                {
                    expandedUrl = value;
                    OnPropertyChanged("ExpandedUrl");
                }
            }
        }
    }
}
