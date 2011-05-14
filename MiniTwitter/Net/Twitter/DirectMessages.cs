/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Net.Twitter
{
    [Serializable]
    [XmlRoot("direct-messages")]
    public class DirectMessages : ITimeTaged
    {
        [XmlElement("direct_message")]
        public DirectMessage[] DirectMessage { get; set; }

        public static readonly DirectMessage[] Empty = new DirectMessage[0];

        [XmlIgnore()]
        public DateTime LastModified { get; set; }

        void ITimeTaged.UpdateChild()
        {
            foreach (var item in DirectMessage)
            {
                item.LastModified = this.LastModified;
            }
        }
    }
}
