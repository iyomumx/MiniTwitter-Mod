using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Input
{
    [Serializable]
    public class KeywordBinding : PropertyChangedBase, IEquatable<KeywordBinding>, IEditableObject
    {
        public KeywordBinding()
        {
        }

        private bool isEnabled;

        [XmlAttribute]
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        private KeywordAction action;

        [XmlAttribute]
        public KeywordAction Action
        {
            get { return action; }
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        private string keyword;

        [XmlText]
        public string Keyword
        {
            get { return keyword; }
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    OnPropertyChanged("Keyword");
                }
            }
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode() ^ Keyword.GetHashCode();
        }

        public bool Equals(KeywordBinding other)
        {
            return Action == other.Action && Keyword == other.Keyword;
        }

        private KeywordBinding copy;

        public void BeginEdit()
        {
            if (copy == null)
            {
                copy = new KeywordBinding();
            }
            copy.Action = this.Action;
            copy.IsEnabled = this.IsEnabled;
            copy.Keyword = this.Keyword;
        }

        public void CancelEdit()
        {
            this.Action = copy.Action;
            this.IsEnabled = copy.IsEnabled;
            this.Keyword = copy.Keyword;
        }

        public void EndEdit()
        {
            copy.IsEnabled = false;
            copy.Keyword = null;
        }
    }
}
