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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MiniTwitter.Controls
{
    public class TweetInfo : PropertyChangedBase
    {
        public static readonly Regex MentionRegex = new Regex(@"^(@[a-zA-Z0-9_]+)\s", RegexOptions.Compiled);
        public static readonly Regex DirectMessageRegex = new Regex(@"^[Dd]\s@?([a-zA-Z0-9_]+)\s", RegexOptions.Compiled);

        public static readonly Dictionary<TweetType, string> Format = new Dictionary<TweetType, string>
        {
            { TweetType.Normal, "What's happening?"},
            { TweetType.Reply, "Reply to {0}"},
            { TweetType.Mention, "Mention {0}"},
            { TweetType.DirectMessage, "Message {0}"},
            { TweetType.Retweet, "Retweet this to your followers?"},
            { TweetType.RT, "RT this tweet:"},
        };
        public const int MaxCharCount = 140;

        private string _Username;
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                if (_Username != value)
                {
                    _Username = value;
                    Update("Username");
                }
            }
        }
        private TweetType _type;
        public TweetType Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    var _m = _type;
                    _type = value;
                    Update("Type");
                    if (_type == TweetType.Retweet || _m == TweetType.Retweet) OnPropertyChanged("CharsLeftDisp");
                }
            }
        }
        private int _CharsLeft;
        public int CharsLeft
        {
            get
            {
                return _CharsLeft;
            }
            private set
            {
                if (_CharsLeft != value)
                {
                    _CharsLeft = value;
                    OnPropertyChanged("CharsLeft");
                    OnPropertyChanged("CharsLeftDisp");
                }
            }
        }
        public string CharsLeftDisp
        {
            get
            {
                return Type == TweetType.Retweet ? "RT" : _CharsLeft.ToString();
            }
        }
        public string HintText
        {
            get
            {
                return string.Format(Format[Type], Username);
            }
        }
        public TweetInfo(TweetType type, string tweet, string username)
        {
            Update(type, tweet, username);
        }
        public TweetType Update(TweetType type, string tweet, string username)
        {
            if (string.IsNullOrEmpty(tweet))
            {
                CharsLeft = MaxCharCount;
                Username = null;
                return Type = TweetType.Normal;
            }
            Match match;
            if ((match = DirectMessageRegex.Match(tweet)).Success)
            {
                CharsLeft = MaxCharCount - tweet.Length + match.Length;
                Username = '@' + match.Groups[1].Value;
                return Type = TweetType.DirectMessage;
            }
            else
            {
                CharsLeft = MaxCharCount - tweet.Length;
                if ((match = MentionRegex.Match(tweet)).Success)
                {
                    Username = match.Groups[1].Value;
                    return Type = type == TweetType.Normal ? TweetType.Mention : type;
                }
                switch (type)
                {
                    case TweetType.Reply:
                    case TweetType.Retweet:
                    case TweetType.RT:
                        Username = username;
                        return Type = type;
                    default:
                        Username = null;
                        return Type = TweetType.Normal;
                }
            }
        }

        protected void Update(string prop)
        {
            OnPropertyChanged(prop);
            OnPropertyChanged("HintText");
        }


    }
    public enum TweetType
    {
        Normal,
        Mention,
        Reply,
        DirectMessage,
        Retweet,
        RT,
    }
}
