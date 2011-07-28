
namespace MiniTwitter
{
    public class MainWindowState : PropertyChangedBase, System.IEquatable<MainWindowState>
    {

        private ulong? _inReplyToID;

        public ulong? In_Reply_To_Status_Id
        {
            get { return _inReplyToID; }
            set 
            {
                if (_inReplyToID != value)
                {
                    _inReplyToID = value;
                    OnPropertyChanged("In_Reply_To_Status_Id");
                }

            }
        }

        private string _inReplyToName;

        public string In_Reply_To_Status_User_Name
        {
            get { return _inReplyToName; }
            set 
            {
                if (_inReplyToName != value)
                {
                    _inReplyToName = value;
                    OnPropertyChanged("In_Reply_To_Status_User_Name");
                }
            }
        }

        private string _text;

        public string StatusText
        {
            get { return _text; }
            set 
            {
                if (_text!=value)
                {
                    _text = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        private ulong? _retweetStatusID;

        public ulong? RetweetStatusID
        {
            get { return _retweetStatusID; }
            set 
            {
                if (_retweetStatusID !=value)
                {
                    _retweetStatusID = value;
                    OnPropertyChanged("RetweetStatusID");
                }

            }
        }

        public MainWindowState()
        {
            _inReplyToName = null;
            _inReplyToID = null;
            _text = null;
            _retweetStatusID = null;
        }

        public MainWindowState(ulong? id, string name, string text)
        {
            _inReplyToID = id;
            _inReplyToName = name;
            _text = text;
            _retweetStatusID = null;
        }

        public MainWindowState(ulong? id, string name, string text,ulong? retweetID)
        {
            _inReplyToID = id;
            _inReplyToName = name;
            _text = text;
            _retweetStatusID = retweetID;
        }

        public MainWindowState(ulong? retweetid, string text)
        {
            _inReplyToID = null;
            _inReplyToName = null;
            _retweetStatusID = retweetid;
            _text = text;
        }

        public override int GetHashCode()
        {
            return (_inReplyToID.ToString() + _inReplyToName + _text).GetHashCode();
        }

        #region IEquatable<MainWindowState> 成员

        public bool Equals(MainWindowState other)
        {
            return (other._inReplyToID == this._inReplyToID &&
                other._inReplyToName == this._inReplyToName &&
                other._text == this._text);
        }

        #endregion
    }
}
