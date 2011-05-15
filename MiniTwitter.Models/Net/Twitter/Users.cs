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
