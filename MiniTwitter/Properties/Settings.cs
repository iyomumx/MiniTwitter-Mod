using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;

using MiniTwitter.Extensions;
using MiniTwitter.Input;

namespace MiniTwitter.Properties
{
    [Serializable]
    [XmlRoot("MiniTwitter")]
    public class Settings : PropertyChangedBase
    {
        private Settings()
        {
            EnableAutoRefresh = true;
            RefreshTick = 60;
            RefreshReplyTick = 10;
            RefreshMessageTick = 15;
            RefreshListTick = 15;
            RefreshSearchTick = 15;
            TimelineStyle = TimelineStyle.Standard;
            IsIconVisible = true;
            SortCategory = ListSortCategory.CreatedAt;
            SortDirection = ListSortDirection.Descending;
            EnablePopup = true;
            PopupCloseTick = 15;
            EnableTweetFooter = false;
            TweetFooter = "*MT*";
            EnableHeartMark = true;
            PopupOnlyFavorite = false;
            PopupOnlyNotActive = false;
            PopupLocation = PopupLocation.Auto;
            EnableNotifyIcon = false;
            FontName = "Meiryo";
            IsClearTypeEnabled = false;
            IsRetweetWithInReplyTo = true;
            UseUserStream = false;
            Version = 1;
            BitlyApiKey = "R_276fb4934824bf8ee936ad0daf0e6745";
            BitlyUsername = "shibayan";
        }

        /// <summary>
        /// 設定ファイルのバージョン
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// ウィンドウの位置
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// ウィンドウのサイズ
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// ウィンドウの状態
        /// </summary>
        public WindowState WindowState { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password { get; set; }

        public string Token { get; set; }

        public string TokenSecret { get; set; }

        public string BitlyUsername { get; set; }

        public string BitlyApiKey { get; set; }

        public string PlixiUsername { get; set; }

        public string PlixiPassword { get; set; }

        public bool AntiShortUrlTracking { get; set; }

        public bool UseBitlyPro { get; set; }

        public string ApiBaseUrl { get; set; }

        public string ApiSearchUrl { get; set; }

        public string LinkUrl { get; set; }

        private bool _acceptAllCert;

        public bool AcceptAllCert { 
            get
            {
                return _acceptAllCert;
            }
            set 
            {
                if (_acceptAllCert != value)
                {
                    _acceptAllCert = value;
                    if (_acceptAllCert)
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            (_, __, ___, ____) => true;
                    }
                    else
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = null;
                    }
                    OnPropertyChanged("AcceptAllCert");
                }
            }
        }

        public bool UseBasicAuth { get; set; }

        private string _bitlyProDomain = "bit.ly";
        public string BitlyProDomain
        {
            get 
            { 
                return string.IsNullOrEmpty(_bitlyProDomain) ? "bit.ly" : _bitlyProDomain; 
            }
            set
            {
                if (_bitlyProDomain != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        if (_bitlyProDomain != "bit.ly")
                        {
                            _bitlyProDomain = "bit.ly";
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        _bitlyProDomain = value;
                    }
                    OnPropertyChanged("BitlyProDomain");
                }
            }
        }
        
        [XmlArray("BitlyProDomains")]
        public string[] BitlyProDomainsInternal
        {
            get { return BitlyProDomains.Count != 0 ? BitlyProDomains.ToArray() : null; }
            set { BitlyProDomains = new ObservableCollection<string>(value ?? Enumerable.Empty<string>()); }
        }

        [XmlIgnore()]
        public ObservableCollection<string> BitlyProDomains
        {
            get;
            private set;
        }

        public bool AlwaysOnTop { get; set; }

        public bool SmoothScroll { get; set; }

        /// <summary>
        /// 自動更新を有効にする
        /// </summary>
        public bool EnableAutoRefresh { get; set; }

        /// <summary>
        /// フレンドタイムライン自動更新間隔
        /// </summary>
        public int RefreshTick { get; set; }

        /// <summary>
        /// 返信タイムライン自動更新間隔
        /// </summary>
        public int RefreshReplyTick { get; set; }

        /// <summary>
        /// ダイレクトメッセージ自動更新間隔
        /// </summary>
        public int RefreshMessageTick { get; set; }

        /// <summary>
        /// リスト自動更新間隔
        /// </summary>
        public int RefreshListTick { get; set; }

        /// <summary>
        /// 検索タイムライン自動更新間隔
        /// </summary>
        public int RefreshSearchTick { get; set; }

        /// <summary>
        /// プロキシを使用する
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// Internet Explorer のプロキシ設定を使用する
        /// </summary>
        public bool UseIEProxy { get; set; }

        /// <summary>
        /// プロキシサーバのアドレス
        /// </summary>
        public string ProxyAddress { get; set; }

        /// <summary>
        /// プロキシサーバのポート番号
        /// </summary>
        public string ProxyPortNumber { get; set; }

        /// <summary>
        /// プロキシのログインユーザー名
        /// </summary>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// プロキシのログインパスワード
        /// </summary>
        public string ProxyPassword { get; set; }

        private TimelineStyle timelineStyle;

        /// <summary>
        /// タイムラインの表示スタイル
        /// </summary>
        public TimelineStyle TimelineStyle
        {
            get { return timelineStyle; }
            set
            {
                if (timelineStyle != value)
                {
                    timelineStyle = value;
                    OnPropertyChanged("TimelineStyle");
                }
            }
        }

        private bool isIconVisible;

        /// <summary>
        /// アイコンを表示する
        /// </summary>
        public bool IsIconVisible
        {
            get { return isIconVisible; }
            set
            {
                if (isIconVisible != value)
                {
                    isIconVisible = value;
                    OnPropertyChanged("IsIconVisible");
                }
            }
        }

        private ListSortCategory sortCategory;

        public ListSortCategory SortCategory
        {
            get { return sortCategory; }
            set
            {
                if (sortCategory != value)
                {
                    sortCategory = value;
                    OnPropertyChanged("SortCategory");
                }
            }
        }

        private ListSortDirection sortDirection;

        public ListSortDirection SortDirection
        {
            get { return sortDirection; }
            set
            {
                if (sortDirection != value)
                {
                    sortDirection = value;
                    OnPropertyChanged("SortDirection");
                }
            }
        }

        public bool EnablePopup { get; set; }

        public bool PopupOnlyNotActive { get; set; }

        public bool PopupOnlyFavorite { get; set; }

        public int PopupCloseTick { get; set; }

        private PopupLocation _popupLocation;

        public PopupLocation PopupLocation
        {
            get { return _popupLocation; }
            set
            {
                _popupLocation = value;
                OnPropertyChanged("PopupLocation");
            }
        }

        public string Theme { get; set; }

        private string _fontName;

        public string FontName
        {
            get { return _fontName; }
            set
            {
                _fontName = value;
                OnPropertyChanged("FontName");
            }
        }

        public bool IsClearTypeEnabled { get; set; }

        //[XmlArray("ColorSchemes")]
        //public ColorScheme[] ColorSchemesInternal
        //{
        //    get { return ColorSchemes.Count != 0 ? ColorSchemes.ToArray() : null; }
        //    set { ColorSchemes = new ObservableCollection<ColorScheme>(value ?? Enumerable.Empty<ColorScheme>()); }
        //}

        //[XmlIgnore]
        //public ObservableCollection<ColorScheme> ColorSchemes { get; set; }

        private bool _enableTweetFooter;

        public bool EnableTweetFooter
        {
            get { return _enableTweetFooter; }
            set
            {
                _enableTweetFooter = value;
                OnPropertyChanged("EnableTweetFooter");
            }
        }

        private string _tweetFooter;

        public string TweetFooter
        {
            get { return _tweetFooter; }
            set
            {
                _tweetFooter = value;
                OnPropertyChanged("TweetFooter");
            }
        }

        [XmlArray("TweetFooterHistory")]
        public string[] TweetFooterHistoryInternal
        {
            get { return TweetFooterHistory.Count != 0 ? TweetFooterHistory.ToArray() : null; }
            set { TweetFooterHistory = new ObservableCollection<string>(value ?? Enumerable.Empty<string>()); }
        }

        [XmlIgnore]
        public ObservableCollection<string> TweetFooterHistory { get; private set; }

        public bool EnableHeartMark { get; set; }

        public bool EnableUnreadManager { get; set; }

        public bool EnableNotifyIcon { get; set; }

        public string BrowserPath { get; set; }

        public bool IsRetweetWithInReplyTo { get; set; }

        public bool UseUserStream { get; set; }

        [XmlArray("Timelines")]
        public Timeline[] TimelinesInternal
        {
            get { return Timelines.Count != 0 ? Timelines.ToArray() : null; }
            set { Timelines = new ObservableCollection<Timeline>(value ?? Enumerable.Empty<Timeline>()); }
        }

        [XmlIgnore]
        public ObservableCollection<Timeline> Timelines { get; private set; }

        public string KeyMapping { get; set; }

        [XmlArray("KeyBindings")]
        public KeyBinding[] KeyBindingsInternal
        {
            get { return KeyBindings.Count != 0 ? KeyBindings.ToArray() : null; }
            set { KeyBindings = new ObservableCollection<KeyBinding>(value ?? Enumerable.Empty<KeyBinding>()); }
        }
        
        [XmlIgnore]
        public ObservableCollection<KeyBinding> KeyBindings { get; private set; }

        [XmlArray("SoundBindings")]
        public SoundBinding[] SoundBindingsInternal
        {
            get { return SoundBindings.Count != 0 ? SoundBindings.ToArray() : null; }
            set
            {
                if (value != null)
                {
                    SoundBindings = new ObservableCollection<SoundBinding>(value);
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<SoundBinding> SoundBindings { get; private set; }

        [XmlArray("KeywordBindings")]
        public KeywordBinding[] KeywordBindingsInternal
        {
            get { return KeywordBindings.Count != 0 ? KeywordBindings.ToArray() : null; }
            set { KeywordBindings = new ObservableCollection<KeywordBinding>(value ?? Enumerable.Empty<KeywordBinding>()); }
        }

        [XmlIgnore]
        public ObservableCollection<KeywordBinding> KeywordBindings { get; private set; }

        [XmlIgnore]
        public Regex FavoriteRegex { get; set; }

        [XmlIgnore]
        public Regex IgnoreRegex { get; set; }

        public void InitializeKeywordRegex()
        {
            var favorites = KeywordBindings.Where(p => p.IsEnabled && p.Action == KeywordAction.Favorite).Select(p => Regex.Escape(p.Keyword)).ToArray();
            FavoriteRegex = favorites.Length > 0 ? new Regex(string.Join("|", favorites)) : null;

            var ignores = KeywordBindings.Where(p => p.IsEnabled && p.Action == KeywordAction.Ignore).Select(p => Regex.Escape(p.Keyword)).ToArray();
            IgnoreRegex = ignores.Length > 0 ? new Regex(string.Join("|", ignores)) : null;
        }

        private void InitializeCollections()
        {
            if (TweetFooterHistory == null)
            {
                TweetFooterHistory = new ObservableCollection<string>();
            }
            if (BitlyProDomains == null)
            {
                BitlyProDomains = new ObservableCollection<string>();
            }
            if (Timelines == null)
            {
                Timelines = new ObservableCollection<Timeline>();
            }
            if (KeyBindings == null)
            {
                KeyBindings = new ObservableCollection<KeyBinding>();
            }
            if (SoundBindings == null)
            {
                SoundBindings = new ObservableCollection<SoundBinding>()
                {
                    new SoundBinding { Action = SoundAction.Status },
                    new SoundBinding { Action = SoundAction.Message },
                    new SoundBinding { Action = SoundAction.Reply },
                    new SoundBinding { Action = SoundAction.Keyword },
                };
            }
            else
            {
                var actions = new[] { SoundAction.Status, SoundAction.Message, SoundAction.Reply, SoundAction.Keyword };

                foreach (var action in actions)
                {
                    if (!SoundBindings.Any(p => p.Action == action))
                    {
                        SoundBindings.Add(new SoundBinding { Action = action });
                    }
                }
            }
            if (KeywordBindings == null)
            {
                KeywordBindings = new ObservableCollection<KeywordBinding>();
            }
            //if (ColorSchemes == null)
            //{
            //    ColorSchemes = new ObservableCollection<ColorScheme>()
            //    {
            //        new ColorScheme { Type = ColorSchemeType.Text, Color = null },
            //        new ColorScheme { Type = ColorSchemeType.Mine, Color = null },
            //        new ColorScheme { Type = ColorSchemeType.Reply, Color = null },
            //        new ColorScheme { Type = ColorSchemeType.ReTweet, Color = null },
            //    };
            //}
            InitializeKeywordRegex();
        }

        public static string BaseDirectory { get; set; }

        public static Settings Default { get; private set; }
        
        public static void Load(string directory)
        {
            try
            {
                BaseDirectory = directory;

                using (var stream = File.Open(Path.Combine(BaseDirectory, @"Preference.xml"), FileMode.Open))
                {
                    Default = Serializer<Settings>.Deserialize(stream);
                }

                if (Default == null)
                {
                    if (File.Exists(Path.Combine(BaseDirectory, @"Preference_backup.xml")))
                    {
                        using (var stream = File.Open(Path.Combine(BaseDirectory, @"Preference_backup.xml"), FileMode.Open))
                        {
                            Default = Serializer<Settings>.Deserialize(stream);
                        }
                    }

                    if (Default == null)
                    {
                        throw new Exception();
                    }
                }

                if (!Default.Password.IsNullOrEmpty())
                {
                    Default.Password = Encoding.ASCII.GetString(Convert.FromBase64String(Default.Password));
                }
                if (Default.RefreshReplyTick > 20)
                {
                    Default.RefreshReplyTick = 10;
                }
                if (Default.Version < 2)
                {
                    // 分 -> 秒に変換する
                    Default.RefreshTick = Math.Min(Default.RefreshTick * 60, 120);
                    Default.Version = 2;
                }
            }
            catch
            {
                Default = new Settings
                {
                    Version = 2,
                    RefreshTick = 60,
                };
            }
            // コレクションを初期化
            Default.InitializeCollections();
        }

        public static void Save()
        {
            if (Default == null || BaseDirectory.IsNullOrEmpty())
            {
                return;
            }
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
            if (!Default.Password.IsNullOrEmpty())
            {
                Default.Password = Convert.ToBase64String(Encoding.ASCII.GetBytes(Default.Password));
            }

            try
            {
                // 設定をバックアップ
                File.Copy(Path.Combine(BaseDirectory, @"Preference.xml"), Path.Combine(BaseDirectory, @"Preference_backup.xml"), true);
            }
            catch { }

            using (var stream = File.Open(Path.Combine(BaseDirectory, @"Preference.xml"), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Serializer<Settings>.Serialize(stream, Default);
            }
        }
    }

    public enum ColorSchemeType
    {
        Text,
        Mine,
        Reply,
        ReTweet,
    }

    public class ColorScheme : PropertyChangedBase, IEquatable<ColorScheme>, IEditableObject
    {
        public ColorSchemeType Type { get; set; }

        private System.Windows.Media.Color? _color;

        public System.Windows.Media.Color? Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        #region IEditableObject メンバ

        private ColorScheme _copy;

        public void BeginEdit()
        {
            if (_copy == null)
            {
                _copy = new ColorScheme();
            }

            _copy.Type = Type;
            _copy.Color = Color;
        }

        public void CancelEdit()
        {
            Type = _copy.Type;
            Color = _copy.Color;
        }

        public void EndEdit()
        {
            _copy.Color = null;
        }

        #endregion

        #region IEquatable<ColorScheme> メンバ

        public bool Equals(ColorScheme other)
        {
            return Type == other.Type;
        }

        #endregion
    }
}
