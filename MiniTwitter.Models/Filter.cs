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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using MiniTwitter.Net.Twitter;

namespace MiniTwitter
{
    [Serializable]
    public class Filter : PropertyChangedBase, IEditableObject, IEquatable<Filter>
    {
        private FilterType type;

        [XmlAttribute]
        public FilterType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        private string pattern;

        [XmlAttribute]
        public string Pattern
        {
            get { return pattern; }
            set
            {
                if (pattern != value)
                {
                    pattern = value;
                    OnPropertyChanged("Pattern");
                }
            }
        }

        [XmlIgnore]
        private static Func<ITwitterItem, bool> _customFilterFunction;

        [XmlIgnore]
        public static Func<ITwitterItem, bool> CustomFilterFunction
        {
            get { return Filter._customFilterFunction; }
            set { Filter._customFilterFunction = value; }
        }

        [XmlIgnore]
        private int _level;

        [XmlIgnore]
        public int Level
        {
            get { return _level; }
            set 
            {
                if (value >= 0)
                {
                    _level = value;                     
                }
            }
        }

        [XmlIgnore]
        private bool _andCombine;

        [XmlIgnore]
        public bool AndCombine
        {
            get { return _andCombine; }
            set { _andCombine = value; }
        }

        

        public bool Process(ITwitterItem item)
        {
            switch (Type)
            {
                case FilterType.Text:
                    return item.Text.IndexOf(Pattern, StringComparison.OrdinalIgnoreCase) != -1;
                case FilterType.RegexText:
                    return Regex.IsMatch(item.Text, Pattern, RegexOptions.IgnoreCase);
                case FilterType.Name:
                    return string.Compare(item.Sender.ScreenName, Pattern, true) == 0;
                case FilterType.RegexName:
                    return Regex.IsMatch(item.Sender.ScreenName, Pattern, RegexOptions.IgnoreCase);
                case FilterType.Source:
                    {
                        var status = item as Status;

                        if (status != null)
                        {
                            return string.Compare(status.Source, Pattern, true) == 0;
                        }
                    }
                    break;
                case FilterType.RegexSource:
                    {
                        var status = item as Status;

                        if (status != null)
                        {
                            return Regex.IsMatch(status.Source, Pattern, RegexOptions.IgnoreCase);
                        }
                    }
                    break;
                case FilterType.ExSourceRegex:
                    {
                        var status = item as Status;

                        if (status != null)
                        {
                            return !(Regex.IsMatch(status.Source, Pattern, RegexOptions.IgnoreCase));
                        }
                    }
                    break;
                case FilterType.ExSource:
                    {
                        var status = item as Status;

                        if (status != null)
                        {
                            return !(string.Compare(status.Source, Pattern, true) == 0);
                        }
                    }
                    break;
                case FilterType.ExText:
                    return !(item.Text.IndexOf(Pattern, StringComparison.OrdinalIgnoreCase) != -1);
                case FilterType.ExTextRegex:
                    return !(Regex.IsMatch(item.Text, Pattern, RegexOptions.IgnoreCase));
                case FilterType.ExName:
                    return !(string.Compare(item.Sender.ScreenName, Pattern, true) == 0);
                case FilterType.ExNameRegex:
                    return !(Regex.IsMatch(item.Sender.ScreenName, Pattern, RegexOptions.IgnoreCase));
                case FilterType.CustomFunction:
                    return (CustomFilterFunction == null) ? true : CustomFilterFunction(item);
                case FilterType.None:
                    return true;
            }
            return false;
        }

        private Filter copy;

        public void BeginEdit()
        {
            if (copy == null)
            {
                copy = new Filter();
            }
            copy.Pattern = this.Pattern;
            copy.Type = this.Type;
        }

        public void CancelEdit()
        {
            this.Pattern = copy.Pattern;
            this.Type = copy.Type;
        }

        public void EndEdit()
        {
            copy.Pattern = null;
            copy.Type = FilterType.None;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() ^ pattern.GetHashCode();
        }

        #region IEquatable<Filter> メンバ

        public bool Equals(Filter other)
        {
            return pattern == other.pattern && type == other.type;
        }

        #endregion
    }
}
