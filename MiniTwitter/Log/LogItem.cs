using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MiniTwitter.Log
{
    [Serializable()]
    public class LogItem
    {
        public LogItem(Exception error)
        {
            this.IsNetworkLog = false;
            this.Time = DateTime.Now;
            this.Message = error.Message;
            this.URL = null;
            this.Param = null;
            this.Action = null;
        }

        public LogItem(string action,Uri uri,string param,string message)
        {
            this.IsNetworkLog = true;
            this.Time = DateTime.Now;
            this.Message = message;
            this.URL = uri;
            this.Action = action;
            this.Param = param;
        }

        public LogItem(string message)
        {
            this.IsNetworkLog = false;
            this.Message = message;
            this.Param = null;
            this.Time = DateTime.Now;
            this.URL = null;
            this.Action = null;
        }

        public LogItem()
        {
            this.Action = null;
            this.IsNetworkLog = false;
            this.Message = null;
            this.Param = null;
            this.Time = DateTime.Now;
            this.URL = null;
        }

        [XmlElement()]
        public string Action { get; set; }
        [XmlElement()]
        public DateTime Time { get; set; }
        [XmlElement()]
        public string Message { get; set; }
        [XmlElement()]
        public Uri URL { get; set; }
        [XmlElement()]
        public bool IsNetworkLog { get; set; }
        [XmlElement()]
        public string Param { get; set; }

        public string ToString(LogFormat format)
        {
            string result = string.Empty;
            if (format == LogFormat.XmlFile)
            {
                using (var ms = new MemoryStream())
                {
                    Serializer<LogItem>.Serialize(ms, this);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms))
                    {
                        result = sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }
            else if(format == LogFormat.PlainText)
            {
                if (this.IsNetworkLog)
                {
                    result = string.Format("{0:u}:{1} To {2} With {3} Occur {4}", this.Time, this.Action, this.URL, this.Param, this.Message);
                }
                else
                {
                    result = string.Format("{0:u}:{1}", this.Time, this.Message);
                }
            }
            return result;
        }
    }
}
