using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MiniTwitter.Input
{
    [Serializable]
    public class KeyBinding : PropertyChangedBase, IEquatable<KeyBinding>, IEditableObject
    {
        public KeyBinding()
        {
        }

        private KeyAction action;

        [XmlAttribute]
        public KeyAction Action
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

        private KeyActionSpot actionSpot;

        [XmlAttribute]
        public KeyActionSpot ActionSpot
        {
            get { return actionSpot; }
            set
            {
                if (actionSpot != value)
                {
                    actionSpot = value;
                    OnPropertyChanged("ActionSpot");
                }
            }
        }

        private Key key;

        [XmlAttribute]
        public Key Key
        {
            get { return key; }
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        private ModifierKeys modifierKeys;

        [XmlAttribute]
        public ModifierKeys ModifierKeys
        {
            get { return modifierKeys; }
            set
            {
                if (modifierKeys != value)
                {
                    modifierKeys = value;
                    OnPropertyChanged("ModifierKeys");
                }
            }
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public bool Equals(KeyBinding other)
        {
            return Action == other.Action;
        }

        private KeyBinding copy;

        public void BeginEdit()
        {
            if (copy == null)
            {
                copy = new KeyBinding();
            }
            copy.Action = this.Action;
            copy.ActionSpot = this.ActionSpot;
            copy.Key = this.Key;
            copy.ModifierKeys = this.ModifierKeys;
        }

        public void CancelEdit()
        {
            this.Action = copy.Action;
            this.ActionSpot = copy.ActionSpot;
            this.Key = copy.Key;
            this.ModifierKeys = copy.ModifierKeys;
        }

        public void EndEdit()
        {
            copy.ActionSpot = KeyActionSpot.All;
            copy.Key = Key.None;
            copy.ModifierKeys = ModifierKeys.None;
        }
    }
}
