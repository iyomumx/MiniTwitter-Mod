using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("hashtag")]
    public class HashTag : PropertyChangedBase
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

        private string text;

        [XmlElement("text")]
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }
    }
}
