using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter.Input
{
    [Serializable]
    public class SoundBinding : PropertyChangedBase, IEquatable<SoundBinding>, IEditableObject
    {
        public SoundBinding()
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

        private SoundAction action;

        [XmlAttribute]
        public SoundAction Action
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

        private string fileName;

        [XmlText]
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public bool Equals(SoundBinding other)
        {
            return Action == other.Action;
        }

        private SoundBinding copy;

        public void BeginEdit()
        {
            if (copy == null)
            {
                copy = new SoundBinding();
            }
            copy.Action = this.Action;
            copy.FileName = this.FileName;
            copy.IsEnabled = this.IsEnabled;
        }

        public void CancelEdit()
        {
            this.Action = copy.Action;
            this.FileName = copy.FileName;
            this.IsEnabled = copy.IsEnabled;
        }

        public void EndEdit()
        {
            copy.FileName = null;
            copy.IsEnabled = false;
        }
    }
}
