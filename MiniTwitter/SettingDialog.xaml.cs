/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

//#define SETTING_WIZARD

using System;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MiniTwitter.Controls;
using MiniTwitter.Extensions;
using MiniTwitter.Input;
using MiniTwitter.Net;
using MiniTwitter.Properties;

namespace MiniTwitter
{
    /// <summary>
    /// SettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingDialog : Window
    {
        static SettingDialog()
        {
            SysFonts =
                new ObservableCollection<FontInfo>();
            foreach (var font in Fonts.SystemFontFamilies)
            {
                if (font == null)
                {
                    continue;
                }
                SysFonts.Add(new FontInfo(font));
            }
        }

        public SettingDialog()
        {
            InitializeComponent();

            if (Settings.Default.IsClearTypeEnabled)
            {
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            }
            //Display Version
            try
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version.Text = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() + " dev";
                }
                else
                {
                    Version.Text = App.VERSION;
                }
            }
            catch (Exception)
            {
                Version.Text = App.VERSION;
            }
        }

        public static ObservableCollection<FontInfo> SysFonts
        {
            private set;
            get;
        }

        private ObservableCollection<MiniTwitter.Input.KeyBinding> keyBindings;
        private ObservableCollection<SoundBinding> soundBindings;
        private ObservableCollection<KeywordBinding> keywordBindings;
        private ObservableCollection<Filter> filters;

        private static readonly PopupLocation[] locations = new[]
        {
            PopupLocation.Auto, PopupLocation.LeftTop, PopupLocation.LeftBottom,
            PopupLocation.RightTop, PopupLocation.RightBottom
        };

        public static PopupLocation[] Locations
        {
            get { return locations; }
        }

        private static readonly KeywordAction[] keywordActions = new[]
        {
            KeywordAction.Favorite, KeywordAction.Ignore
        };

        public static KeywordAction[] KeywordActions
        {
            get { return SettingDialog.keywordActions; }
        }

        private void SettingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 見やすくするために、変数名を短縮する
            Settings settings = Settings.Default;
            // パスワード、プロキシパスワード
            PasswordBox.Password = settings.Password;
            ProxyPasswordBox.Password = settings.ProxyPassword;
            PlixiPasswordBox.Password = settings.PlixiPassword;
            // キーボードショートカット設定
            var array = Enum.GetValues(typeof(KeyAction));
            keyBindings = new ObservableCollection<MiniTwitter.Input.KeyBinding>(settings.KeyBindings ?? Enumerable.Empty<MiniTwitter.Input.KeyBinding>());
            foreach (KeyAction item in array)
            {
                if (keyBindings.SingleOrDefault(p => p.Action == item) == null)
                {
                    keyBindings.Add(new MiniTwitter.Input.KeyBinding { Action = item });
                }
            }
            keyBindings.BeginEdit();
            KeyMappingComboBox.SelectedItem = KeyMapping.KeyMappings.SingleOrDefault(p => p.Key == settings.KeyMapping);
            CommandComboBox.ItemsSource = keyBindings;
            // サウンド設定
            soundBindings = new ObservableCollection<SoundBinding>(settings.SoundBindings);
            soundBindings.BeginEdit();
            SoundListView.ItemsSource = soundBindings;
            // キーワード設定
            keywordBindings = new ObservableCollection<KeywordBinding>(settings.KeywordBindings ?? Enumerable.Empty<KeywordBinding>());
            keywordBindings.BeginEdit();
            KeywordListView.ItemsSource = keywordBindings;
            //// カラー設定
            //colorSchemes = new ObservableCollection<ColorScheme>(settings.ColorSchemes ?? Enumerable.Empty<ColorScheme>());
            //colorSchemes.BeginEdit();
            //ColorListView.ItemsSource = colorSchemes;
            filters = new ObservableCollection<Filter>(settings.GlobalFilter ?? Enumerable.Empty<Filter>());
            FilterListView.ItemsSource = filters;
            filters.BeginEdit();
            // メッセージフッタ履歴
            TweetFooterComboBox.ItemsSource = settings.TweetFooterHistory;
            BitlyProDomains.ItemsSource = settings.BitlyProDomains;
            ReTweetPrefixComboBox.ItemsSource = settings.ReTweetPrefixHistory;
            BindingGroup.BeginEdit();

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 変更をコミットする
            BindingGroup.CommitEdit();
            // 見やすくするために変数名を短縮する
            Settings settings = Settings.Default;
            // パスワードを保存
            settings.Password = PasswordBox.Password;
            settings.PlixiPassword = PlixiPasswordBox.Password;
            settings.ProxyPassword = ProxyPasswordBox.Password;
            // キーボードショートカットを保存
            if (KeyMappingComboBox.SelectedValue != null)
            {
                settings.KeyMapping = ((KeyMapping)KeyMappingComboBox.SelectedValue).Name;
            }
            settings.KeyBindings.Clear();
            foreach (var item in keyBindings.Where(p => p.Key != Key.None))
            {
                settings.KeyBindings.Add(item);
            }
            // サウンド設定を保存
            settings.SoundBindings.Clear();
            foreach (var item in soundBindings)
            {
                settings.SoundBindings.Add(item);
            }
            // キーワード設定を保存
            settings.KeywordBindings.Clear();
            foreach (var item in keywordBindings)
            {
                settings.KeywordBindings.Add(item);
            }
            settings.GlobalFilter.Clear();
            foreach (var item in filters)
            {
                settings.GlobalFilter.Add(item);
            }
            // メッセージフッタの履歴を保存
            if (!settings.TweetFooter.IsNullOrEmpty() && !settings.TweetFooterHistory.Contains(settings.TweetFooter))
            {
                settings.TweetFooterHistory.Add(settings.TweetFooter);
            }
            if (!settings.BitlyProDomain.IsNullOrEmpty() && !settings.BitlyProDomains.Contains(settings.BitlyProDomain))
            {
                settings.BitlyProDomains.Add(settings.BitlyProDomain);
            }
            if (!settings.ReTweetPrefix.IsNullOrEmpty() && !settings.ReTweetPrefixHistory.Contains(settings.ReTweetPrefix))
            {
                settings.ReTweetPrefixHistory.Add(settings.ReTweetPrefix);
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 変更をキャンセルする
            keyBindings.CancelEdit();
            soundBindings.CancelEdit();
            keywordBindings.CancelEdit();
            filters.CancelEdit();
            DialogResult = false;
        }

        private void AssignKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutKeyBox.GetBindingExpression(ShortcutKeyBox.KeyProperty).UpdateSource();
            ShortcutKeyBox.GetBindingExpression(ShortcutKeyBox.ModifierKeysProperty).UpdateSource();
        }

        private void DeleteKeyButton_Click(object sender, RoutedEventArgs e)
        {
            var binding = (MiniTwitter.Input.KeyBinding)CommandComboBox.SelectedItem;
            binding.Key = Key.None;
            binding.ModifierKeys = ModifierKeys.None;
        }

        private void KeyMappingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || e.AddedItems.Count == 0)
            {
                return;
            }
            var keyMapping = (KeyMapping)KeyMappingComboBox.SelectedValue;
            foreach (var item in keyMapping.KeyBindings)
            {
                var keyBinding = keyBindings.SingleOrDefault(p => item.Equals(p) && p.Key == Key.None);
                if (keyBinding != null)
                {
                    keyBinding.Key = item.Key;
                    keyBinding.ModifierKeys = item.ModifierKeys;
                    keyBinding.ActionSpot = item.ActionSpot;
                }
            }
        }

        private void ResetKeyMappingButton_Click(object sender, RoutedEventArgs e)
        {
            var keyMapping = (KeyMapping)KeyMappingComboBox.SelectedValue;
            foreach (var item in keyBindings)
            {
                var binding = keyMapping.KeyBindings.SingleOrDefault(p => item.Equals(p));
                if (binding == null)
                {
                    item.Key = Key.None;
                    item.ModifierKeys = ModifierKeys.None;
                }
                else
                {
                    item.Key = binding.Key;
                    item.ModifierKeys = binding.ModifierKeys;
                }
            }
        }

        private void AddKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            var keyword = KeywordTextBox.Text;
            if (keyword.IsNullOrEmpty())
            {
                return;
            }
            var binding = new KeywordBinding { Action = KeywordAction.Favorite, IsEnabled = true, Keyword = keyword };
            binding.BeginEdit();
            keywordBindings.Add(binding);
            KeywordTextBox.Clear();
        }

        private void DeleteKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeywordBinding)KeywordListView.SelectedItem;
            if (item == null)
            {
                return;
            }
            keywordBindings.Remove(item);
        }

        private void PlaySoundButton_Click(object sender, RoutedEventArgs e)
        {
            var sound = (SoundBinding)SoundListView.SelectedItem;
            if (!sound.FileName.IsNullOrEmpty())
            {
                try
                {
                    new SoundPlayer(sound.FileName).Play();
                }
                catch
                {
                    MessageBox.Show("不支持此格式！", App.NAME);
                }
            }
        }

        private void BrowseSoundButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "声音文件|*.wav",
                Multiselect = false,
            };
            if (dialog.ShowDialog() ?? false)
            {
                var sound = (SoundBinding)SoundListView.SelectedItem;
                sound.FileName = dialog.FileName;
            }
        }

        private void UpdateCheckButton_Click(object sender, RoutedEventArgs e)
        {
            var deploy = ApplicationDeployment.CurrentDeployment;

            if (deploy.CheckForUpdate())
            {
                if (MessageBox.Show("有可用更新，现在进行更新吗？", App.NAME, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    deploy.UpdateAsync();

                    DialogResult = true;
                }
            }
            else
            {
                MessageBox.Show("没有找到可用更新。", App.NAME);
            }
        }

        private void StartOAuth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TwitterClient TC = new TwitterClient(App.consumer_key, App.consumer_secret);
                if (UseProxyCheckBox.IsChecked.Value)
                {
                    var proxy = new System.Net.WebProxy(ProxyAddress.Text, Convert.ToInt32(ProxyPortNumber.Text));
                    if (!(String.IsNullOrEmpty(ProxyUsername.Text) || String.IsNullOrEmpty(ProxyPasswordBox.Password)))
                    {
                        proxy.Credentials = new System.Net.NetworkCredential(ProxyUsername.Text, ProxyPasswordBox.Password);
                    }
                    TC.Proxy = proxy;
                }
                string token;
                string tokensecret = "";
                TC.GetRequestToken(out token);
                string url = TC.RedirectToAuthorize(token);
                MessageBox.Show("将打开默认浏览器以获取PIN，请确认你的浏览器能正确连接Twitter（你懂的）", "即将跳转", MessageBoxButton.OK);
                System.Diagnostics.Process.Start(url);

                string PIN = PinInputWindow.PinBox();

                TC.GetAccessToken(ref token, ref tokensecret, PIN);
                UsernameBox.Text = token;
                PasswordBox.Password = tokensecret;
            }
            catch (Exception ex)
            {
                MessageBox.Show("出错啦\n" + ex.ToString(), "出错啦");
            }

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://bit.ly/a/your_api_key");
            }
            catch
            {
            }
        }

        private void DoGoogleOAuth_Click(object sender, RoutedEventArgs e)
        {
            if (TwitterClient.googlHelper == null)
            {
                TwitterClient.googlHelper = new Google.UrlShortener.UrlShortener();
            }
            string token = string.Empty;
            string tokensecret = string.Empty;
            DoGoogleOAuth.Content = "请求中…";
            this.AsyncInvoke(() =>
            {
                if (TwitterClient.googlHelper.GetRequestToken(ref token, new { scope = "https://www.googleapis.com/auth/urlshortener", xoauth_displayname = "iyomumx Mod", oauth_callback = "oob" }, ref tokensecret))
                {
                    string url = TwitterClient.googlHelper.RedirectToAuthorize(token);
                    MessageBox.Show("将打开默认浏览器以获取PIN，请确认你的浏览器能正确连接Google", "即将跳转", MessageBoxButton.OK);
                    System.Diagnostics.Process.Start(url);
                    string PIN = PinInputWindow.PinBox();
                    TwitterClient.googlHelper.GetAccessToken(ref token, ref tokensecret, PIN);
                    PlixiUsernameBox.Text = token;
                    PlixiPasswordBox.Password = tokensecret;
                    TwitterClient.googlHelper.WriteToken(token, tokensecret);
                }
            });
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
#if SETTING_WIZARD && DEBUG
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    MiniTwitter.SettingWizard.WizardWindow window = new SettingWizard.WizardWindow();
                    var r = window.ShowDialog();
                    System.Diagnostics.Debug.WriteLine(r);
                    return;
                }
#endif
                System.Diagnostics.Process.Start(Settings.BaseDirectory);
            }
            catch
            {
            }
        }

        private void FilterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                var item = (Filter)FilterListView.SelectedItem;
                if (item == null)
                {
                    return;
                }
                FilterTextBox.Text = item.Pattern;
            }
        }

        private void AddFilterButton_Click(object sender, RoutedEventArgs e)
        {
            var filter = new Filter { Pattern = FilterTextBox.Text, Type = FilterType.None };
            filters.Add(filter);
            filter.BeginEdit();
            FilterListView.ScrollIntoView(filter);
            FilterTextBox.Clear();
        }

        private void DeleteFilterButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (Filter)FilterListView.SelectedItem;

            if (item == null)
            {
                return;
            }

            filters.Remove(item);
        }

        private void TextBlock_Initialized(object sender, EventArgs e)
        {

        }

        private ConcurrentBag<int> InitializedTextBlocks = new ConcurrentBag<int>();

        private void TextBlock_MouseWheel(object sender, EventArgs e)
        {
            try
            {
                if (InitializedTextBlocks.Contains(sender.GetHashCode()))
                {
                    return;
                }
                InitializedTextBlocks.Add(sender.GetHashCode());
                if (sender is TextBlock)
                {
                    TextBlock text = (TextBlock)sender;
                    ThreadPool.QueueUserWorkItem(_ =>
                        text.Invoke(() =>
                            text.FontFamily = (FontFamily)text.Tag));
                }
            }
            catch { }
        }

        private void OtherListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            OtherListBoxItem.IsSelected = true;
        }

        private void TweetFooterComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                Settings.Default.TweetFooterHistory.Remove((TweetFooterComboBox.SelectedItem ?? string.Empty).ToString());
            }
        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Documents.Hyperlink)
            {
                var uri = (sender as System.Windows.Documents.Hyperlink).NavigateUri;
                if (uri != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(uri.ToString());
                    }
                    catch { }
                }
            }
        }

        private void TweetFooterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            KanvasoShortenCheckBox.IsChecked = false;
        }

        private void KanvasoShortenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TweetFooterCheckBox.IsChecked = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (Settings.Default.EnableAero)
            {
                this.ExtendGlassFrame(new Thickness(-1));
            }
            this.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (Settings.Default.EnableAero && msg == NativeMethods.WM_DWMCOMPOSITIONCHANGED)
                {
                    this.ExtendGlassFrame(new Thickness(-1));
                    handled = true;
                }
                return IntPtr.Zero;
            });
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }

    public class FontInfo : PropertyChangedBase
    {
        public FontInfo(FontFamily font)
        {
            Source = font.Source;
            Font = font;
            if (font.FamilyNames.Count == 0)
            {
                Name = Source;
            }
            else
            {
                var k = font.FamilyNames.Where(kvp =>
                    kvp.Key.GetEquivalentCulture() == CultureInfo.CurrentUICulture ||
                    !kvp.Key.IetfLanguageTag.ToLower().StartsWith("en")).FirstOrDefault();
                object o = k;
                if (o == null || k.Key == null || k.Key.ToString().IsNullOrEmpty())
                {
                    Name = font.FamilyNames.FirstOrDefault().Value;
                }
                else
                {
                    Name = k.Value;
                }
            }
        }
        private string _source;

        public string Source
        {
            get { return _source; }
            set
            {
                if (_source != value)
                {
                    _source = value;
                    OnPropertyChanged("Source");
                }
            }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private FontFamily _font;

        public FontFamily Font
        {
            get { return _font; }
            set
            {
                if (_font != value)
                {
                    _font = value;
                    OnPropertyChanged("Font");
                }
            }
        }

    }

}
