﻿/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MiniTwitter.Extensions;
using MiniTwitter.Properties;
using MiniTwitter.Net.Twitter;
using Input = MiniTwitter.Input;

namespace MiniTwitter.Controls
{
    /// <summary>
    /// TextViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class TextViewer : UserControl
    {
        public TextViewer()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextViewer), new PropertyMetadata(TextPropertyChanged));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextViewer), new PropertyMetadata(TextWrapping.Wrap));

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TextViewer), new PropertyMetadata(TextTrimming.None));

        public Entities Entities
        {
            get { return (Entities)GetValue(EntitiesProperty); }
            set { SetValue(EntitiesProperty, value); }
        }

        public static readonly DependencyProperty EntitiesProperty =
            DependencyProperty.Register("Entities", typeof(Entities), typeof(TextViewer), new PropertyMetadata(null, EntitiesPropertyChanged));

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((TextViewer)sender).OnTextChanged((string)e.NewValue);
        }

        private static void EntitiesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var source = (TextViewer)sender;
            source.OnTextChanged(source.Text);
        }

        private static readonly Regex searchPattern = new Regex(@"(?<url>https?:\/\/[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)|(?<=(?<email>[a-zA-Z0-9])?)@(?<user>[_a-zA-Z0-9]+(?(email)(?((\.[a-zA-Z0-9])|[_a-zA-Z0-9])(?!))))|(?<heart><3)|(?<=^|\W)#(?<hash>\w+)|(\<[Dd][Ee][Ll]\>(?<del>.+?)\</[Dd][Ee][Ll]>)|((?<emoji>[\uE001-\uE537\uE63E-\uE757]))", RegexOptions.Compiled);

        private void OnTextChanged(string text)
        {
            TextBlock.Inlines.Clear();
            if (text.IsNullOrEmpty())
            {
                return;
            }
            int index = 0;
            List<string> imageurls = new List<string>();
            foreach (Match match in searchPattern.Matches(text))
            {
                int diff = 0;
                string value = match.Value;
                if (index != match.Index)
                {
                    HighlightKeywords(text.Substring(index, match.Index - index));
                }
                if (value.StartsWith("<3"))
                {
                    if (Settings.Default.EnableHeartMark)
                    {
                        Image image = new Image { Width = 14, Height = 14, Margin = new Thickness(1, 0, 1, 0) };
                        image.SetResourceReference(Image.StyleProperty, "HeartImageStyle");
                        TextBlock.Inlines.Add(new InlineUIContainer(image) { BaselineAlignment = BaselineAlignment.Center });
                    }
                    else
                    {
                        TextBlock.Inlines.Add(new Run("<3"));
                    }
                }
                else if (!match.Groups["emoji"].Value.IsNullOrEmpty())
                {
                    Image image = new Image { Width = 14, Height = 14, Margin = new Thickness(1, 0, 1, 0) };
                    string ResourceKey = string.Format("Emoji{0}", Convert.ToInt32(value.FirstOrDefault()).ToString("X4"));
                    var res = TryFindResource(ResourceKey);
                    if (res != null)
                    {
                        image.Style = (Style)res;
                    }
                    //image.SetResourceReference(Image.StyleProperty, ResourceKey);
                    TextBlock.Inlines.Add(new InlineUIContainer(image) { BaselineAlignment = BaselineAlignment.Center });
                }
                else if (value.StartsWith("<"))
                {
                    Run delText = new Run(match.Groups["del"].Value);
                    if (Settings.Default.EnableDeleteLine)
                    {
                        delText.TextDecorations.Add(new TextDecoration() { Location = TextDecorationLocation.Strikethrough });
                    }
                    TextBlock.Inlines.Add(delText);
                }
                else if (value.StartsWith("@"))
                {
                    diff = 1;
                    value = match.Groups["user"].Value;
                    Hyperlink link = new Hyperlink { Tag = value };
                    if (Entities != null && Entities.UserMentions != null && Entities.UserMentions.UserMention != null)
                    {
                        link.ToolTip = (from um in Entities.UserMentions.UserMention
                                        where um.ScreenName == value
                                        select um.Name).FirstOrDefault();
                    }
                    link.Inlines.Add(value);
                    link.Click += UserHyperlink_Click;
                    var menu = (ContextMenu)(Application.Current.FindResource("UserInlineMenu"));
                    TextBlock.Inlines.Add("@");
                    TextBlock.Inlines.Add(link);
                    link.DataContext = value;
                    link.ContextMenuOpening += new ContextMenuEventHandler((snd, __) =>
                    {
                        Hyperlink ctxMenu = (Hyperlink)snd;
                        this.Invoke(() => ctxMenu.ContextMenu.DataContext = value);
                    });
                    link.ContextMenu = menu;
                }
                else if (value.StartsWith("#"))
                {
                    Hyperlink link = new Hyperlink { Tag = value };
                    link.Inlines.Add(value);
                    link.Click += HashtagHyperlink_Click;
                    link.ContextMenu = (ContextMenu)Application.Current.FindResource("HashTagMenu");
                    link.ContextMenuOpening += new ContextMenuEventHandler((s, _) =>
                    {
                        Hyperlink Link = (Hyperlink)s;
                        this.Invoke(() => Link.ContextMenu.DataContext = value);
                    });
                    TextBlock.Inlines.Add(link);
                }
                else
                {
                    // URL記法
                    Hyperlink link = new Hyperlink();
                    if (Entities != null && Entities.Urls != null && Entities.Urls.URL != null)
                    {
                        link.Tag = (from url in Entities.Urls.URL
                                    where url.URL == value && !url.ExpandedUrl.IsNullOrEmpty()
                                    select url.ExpandedUrl).FirstOrDefault() ?? value;
                    }
                    else
                    {
                        link.Tag = value;
                    }
                    string cachedtarget = ReadCache(CutScheme(link.Tag as string));
                    if (!cachedtarget.IsNullOrEmpty())
                    {
                        link.Tag = cachedtarget;
                    }
                    link.ToolTip = link.Tag;
                    link.ToolTipOpening += Hyperlink_ToolTipOpening;
                    link.Inlines.Add(CutScheme(link.Tag as string));
                    link.Click += Hyperlink_Click;
                    link.ContextMenu = (ContextMenu)Application.Current.FindResource("LinkInlineMenu");
                    link.ContextMenuOpening += new ContextMenuEventHandler((sender_, ____) =>
                    {
                        var hl = (Hyperlink)sender_;
                        string url;
                        if (hl.ToolTip is TextBlock)
                        {
                            url = ((TextBlock)hl.ToolTip).Text;
                        }
                        else
                        {
                            url = hl.ToolTip as string;
                        }
                        hl.ContextMenu.DataContext = new { Url = url, OriginalUrl = value, IsDifferent = url != value };
                    });
                    TextBlock.Inlines.Add(link);
                    if (Settings.Default.ImageInline) //使用图片预览
                    {
                        imageurls.Add((string)link.Tag);
                    }
                }
                index = match.Index + value.Length + diff;
            }
            if (index != text.Length)
            {
                HighlightKeywords(text.Substring(index));
            }
            Inline flag = TextBlock.Inlines.LastInline;
            bool Inserted = false;
            if (Entities != null && Entities.Media != null && Entities.Media.creative != null && Entities.Media.creative.Length != 0)
            {
                foreach (var media in Entities.Media.creative)
                {
                    var uri = new Uri(Settings.Default.SSLUserImage ? media.media_url_https : media.media_url);
                    CacheUrl(media.url, uri);
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, media.url));
                }
                Inserted = true;
            }
            foreach (var url in imageurls)
            {

                if (Regex.IsMatch(url, @"http:\/\/twitpic\.com\/(.+?)"))
                {
                    var uri = new Uri("http://twitpic.com/show/large/" + url.Substring(19));
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"http:\/\/img\.ly\/(.+?)"))
                {
                    var uri = new Uri("http://img.ly/show/medium/" + url.Substring(14));
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"(http:\/\/plixi\.com\/p\/(.+?))|(http:\/\/lockerz\.com\/s\/(.+?))"))
                {
                    var uri = new Uri("http://api.plixi.com/api/tpapi.svc/imagefromurl?url=" + url + "&size=medium");
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"http:\/\/flic\.kr\/p\/(.+?)"))
                {
                    var uri = new Uri("http://flic.kr/p/img/" + url.Substring(17) + "_m.jpg");
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"http:\/\/yfrog\.com\/(.+?)[jpbtg]$"))
                {
                    var uri = new Uri(url + ":iphone");
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"http:\/\/imgur\.com\/([a-zA-Z0-9]+?\/?)$"))
                {
                    var uri = new Uri("http://i.imgur.com/" + url.Substring(17) + "l.jpg");
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"^http://picplz\.com/[A-Za-z0-9]+$"))
                {
                    var uri = GetUri(url);
                    if (uri == null)
                    {
                        CacheUrl(url, u =>
                        {
                            try
                            {
                                var client = new WebClient();
                                var contents = client.DownloadString(string.Format("http://api.picplz.com/api/v2/pic.json?shorturl_ids={0}", u.Substring(18)));
                                var _match = Regex.Match(contents, "320rh.+?\"img_url\":\"(.+?)\"");
                                return new Uri(_match.Groups[1].Value);
                            }
                            catch
                            {
                                return null;
                            }
                        });
                    }
                    else
                    {
                        TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                        Inserted = true;
                    }
                }
                else if (Regex.IsMatch(url, @"^http://picplz\.com/user/.+?/pic/.+?(/?)$"))
                {
                    var uri = GetUri(url);
                    if (uri == null)
                    {
                        CacheUrl(url, u =>
                        {
                            try
                            {
                                var urlmatch = Regex.Match(u, "pic/(?<id>.+?)/?$");
                                var longid = urlmatch.Groups["id"].Value;
                                var client = new WebClient();
                                var contents = client.DownloadString(string.Format("http://api.picplz.com/api/v2/pic.json?longurl_ids={0}", longid));
                                var _match = Regex.Match(contents, @"320rh.+?\""img_url\"": \""(.+?)\""");
                                return new Uri(_match.Groups[1].Value);
                            }
                            catch
                            {
                                return null;
                            }
                        });
                    }
                    else
                    {
                        TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                        Inserted = true;
                    }
                }
                else if (Regex.IsMatch(url, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)"))
                {
                    var uri = GetUri(url);
                    if (uri == null)
                    {
                        CacheUrl(url, u =>
                        {
                            try
                            {
                                var client = new WebClient();
                                var contents = client.DownloadString(u);
                                var _match = Regex.Match(u, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)");
                                _match = Regex.Match(contents, string.Format(@"<img id=\""foto-for-html-tag-{0}\"" src=\""(.+?)\""", _match.Groups[2].Value));

                                return new Uri(_match.Groups[1].Value);
                            }
                            catch
                            {
                                return null;
                            }
                        });
                    }
                    else
                    {
                        TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                        Inserted = true;
                    }
                }
                else if (Regex.IsMatch(url, @"http:\/\/movapic\.com\/pic\/(.+?)"))
                {
                    var uri = GetUri(url);
                    if (uri == null)
                    {
                        CacheUrl(url, u =>
                        {
                            try
                            {
                                var client = new WebClient();
                                var contents = client.DownloadString(u);
                                var _match = Regex.Match(contents, @"<img class=\""image\"" src=\""(.+?)\""");

                                return new Uri(_match.Groups[1].Value);
                            }
                            catch
                            {
                                return null;
                            }
                        });
                    }
                    else
                    {
                        TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                        Inserted = true;
                    }
                }
                else if (Regex.IsMatch(url, @"http:\/\/gyazo\.com\/(.+?)"))
                {
                    var uri = new Uri(url);
                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                    CacheUrl(url, uri);
                    Inserted = true;
                }
                else if (Regex.IsMatch(url, @"http:\/\/instagr\.am\/p\/(.+?)"))
                {
                    var uri = GetUri(url);
                    if (uri == null)
                    {
                        CacheUrl(url, u =>
                        {
                            try
                            {
                                var _match = Regex.Match(url, @"http:\/\/instagr\.am\/p\/(.+?)\/?$");
                                return new Uri(string.Format(@"http://instagr.am/p/{0}/media/?size=m", _match.Groups[1].Value));
                            }
                            catch
                            {
                                return new Uri(url);
                            }
                        });
                    }
                    else
                    {
                        TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                        Inserted = true;
                    }
                }
            }
            if (Inserted)
            {
                TextBlock.Inlines.InsertAfter(flag, new Run("\n"));
            }
        }

        private static readonly Status NullStatus = new Status();

        private Inline GetImageControl(Uri uri, string url)
        {
            InlineUIContainer cacheItem = GetImage(url);
            InlineUIContainer container;
            BitmapImage bmp;
            if (cacheItem != null)          //命中缓存
            {
                container = cacheItem;      //取出缓存
                bmp = (BitmapImage)((Image)container.Child).Source;
            }
            else                            //缓存未命中
            {                               //构建新的Container
                bmp = new BitmapImage(uri);
                Image img = new Image() { Width = 200, MaxHeight = 200, Source = bmp };
                container = new InlineUIContainer(img);
                CacheImage(url, container); //存入缓存
            }
            Hyperlink link = new Hyperlink(container);
            link.Tag = url;
            link.ToolTip = url;
            link.Click += Hyperlink_Click;
            link.DataContext = new
            {
                ImageUri = uri,
                ImageUrl = string.Intern(uri.ToString()),
                ShortUrl = url,
                Image = bmp,
                Link = link,
            };
            link.ContextMenu = (ContextMenu)Application.Current.FindResource("ImageInlineMenu");
            link.ContextMenuOpening += new ContextMenuEventHandler((sender_, ____) =>
            {
                var hl = (Hyperlink)sender_;
                hl.ContextMenu.DataContext = hl.DataContext;
            });
            link.TextDecorations = null;
            return link;
        }

        //private void link_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (Keyboard.Modifiers == ModifierKeys.Control)
        //    {
        //        var d = e.Delta;
        //        Hyperlink link = (Hyperlink)sender;
        //        Image img = (Image)(((InlineUIContainer)(link.Inlines.FirstInline)).Child);
        //        if (d>0)
        //        {
        //            img.Height *= 1.1;
        //            img.Width *= 1.1;
        //        }
        //        else
        //        {
        //            img.Height *= 0.9;
        //            img.Width *= 0.9;
        //        }
        //        e.Handled = true;
        //    }
        //}

        private void HighlightKeywords(string text)
        {
            if (Settings.Default.FavoriteRegex == null)
            {
                TextBlock.Inlines.Add(text);
            }
            else
            {
                int startIndex = 0;
                foreach (Match match in Settings.Default.FavoriteRegex.Matches(text))
                {
                    string str = match.Groups[0].Value;
                    if (startIndex != match.Index)
                    {
                        TextBlock.Inlines.Add(text.Substring(startIndex, match.Index - startIndex));
                    }
                    Run item = new Run(" " + str + " ");
                    item.FontWeight = FontWeights.Bold;
                    item.Background = Brushes.Yellow;
                    TextBlock.Inlines.Add(item);
                    startIndex = match.Index + str.Length;
                }
                if (startIndex != text.Length)
                {
                    TextBlock.Inlines.Add(text.Substring(startIndex));
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hyperlink = (Hyperlink)sender;
                var url = (string)hyperlink.Tag;
                var isTwitterImage = false;
                Uri twitterImageUri;
                try
                {
                    twitterImageUri = (GetUri(url) ?? new Uri(url));
                    isTwitterImage = twitterImageUri.ToString().IsRegexMatch(@"^https?://p\.twimg\.com/[a-zA-Z0-9_\-]+(\.[a-zA-Z0-9_\-]+)?$");
                }
                catch (Exception)
                {
                    isTwitterImage = false;
                    throw;
                }
                if (((Settings.Default.ImageInline && Keyboard.Modifiers != ModifierKeys.Shift) || Keyboard.Modifiers == ModifierKeys.Control) && !(e.OriginalSource is Window))
                {
                    Process.Start(url);
                    return;
                }
                if (isTwitterImage)
                {
                    ShowPopup(twitterImageUri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/twitpic\.com\/(.+?)"))
                {
                    var uri = new Uri("http://twitpic.com/show/large/" + url.Substring(19));

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/img\.ly\/(.+?)"))
                {
                    var uri = new Uri("http://img.ly/show/medium/" + url.Substring(14));

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"(http:\/\/plixi\.com\/p\/(.+?))|(http:\/\/lockerz\.com\/s\/(.+?))"))
                {
                    var uri = new Uri("http://api.plixi.com/api/tpapi.svc/imagefromurl?url=" + url + "&size=medium");

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/flic\.kr\/p\/(.+?)"))
                {
                    var uri = new Uri("http://flic.kr/p/img/" + url.Substring(17) + "_m.jpg");

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/yfrog\.com\/(.+?)[jpbtg]$"))
                {
                    var uri = new Uri(url + ":iphone");

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)"))
                {
                    var client = new WebClient();
                    var contents = client.DownloadString(url);
                    var match = Regex.Match(url, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)");
                    match = Regex.Match(contents, string.Format(@"<img id=\""foto-for-html-tag-{0}\"" src=\""(.+?)\""", match.Groups[2].Value));

                    var uri = new Uri(match.Groups[1].Value);

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/movapic\.com\/pic\/(.+?)"))
                {
                    var client = new WebClient();
                    var contents = client.DownloadString(url);
                    var match = Regex.Match(contents, @"<img class=\""image\"" src=\""(.+?)\""");

                    var uri = new Uri(match.Groups[1].Value);

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/gyazo\.com\/(.+?)"))
                {
                    var uri = new Uri(url);

                    ShowPopup(uri, url);
                }
                else if (Regex.IsMatch(url, @"http:\/\/instagr\.am\/p\/(.+?)"))
                {
                    var match = Regex.Match(url, @"http:\/\/instagr\.am\/p\/(.+?)\/?$");

                    var uri = new Uri(string.Format(@"http://instagr.am/p/{0}/media/?size=m", match.Groups[1].Value));

                    ShowPopup(uri, url);
                }
                else
                {
                    Process.Start(url);
                }
            }
            catch
            {
                MessageBox.Show("无法打开浏览器", App.NAME);
            }
        }

        private void ShowPopup(Uri uri, string url)
        {
            var popup = (Popup)FindResource("PreviewPopup");
            var progress = (Popup)FindResource("ProgressPopup");

            var bitmap = new BitmapImage();
            bitmap.DownloadCompleted += (_, __) =>
            {
                progress.IsOpen = false;
                popup.DataContext = new
                {
                    Image = bitmap,
                    Url = url,
                };
                popup.IsOpen = true;
            };
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.DecodePixelWidth = 400;
            bitmap.EndInit();
            if (bitmap.IsDownloading)
            {
                progress.IsOpen = true;
            }
            else
            {
                popup.DataContext = new
                {
                    Image = bitmap,
                    Url = url,
                };
            }
        }

        private static void HashtagHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;

            MiniTwitter.Input.Commands.Hashtag.Execute(hyperlink.Tag, hyperlink);
        }

        private static void UserHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;

            MiniTwitter.Input.Commands.ViewUserByName.Execute(hyperlink.Tag, hyperlink);
        }

        private static Regex longurlpleaseJSONRegex = new Regex("^\\{\"(?<ShortUrl>.+?)\":\\s\"(?<LongUrl>.+?)\"\\}$", RegexOptions.Compiled);

        private static string GetRedirect(string url)
        {
            if (Settings.Default.UseApiToLengthenUrl)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(string.Format("http://www.longurlplease.com/api/v1.1?q={0}", url));
                    request.AllowAutoRedirect = false;
                    request.Timeout = 3000;
                    request.Method = "GET";
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string result;

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                        Match match = longurlpleaseJSONRegex.Match(result);
                        if (match.Success && match.Groups["ShortUrl"].Value.Replace(@"\/", @"/") == url)
                        {
                            return match.Groups["LongUrl"].Value.Replace(@"\/", @"/");
                        }

                    }
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.AllowAutoRedirect = false;
                    request.Timeout = 3000;
                    request.Method = "HEAD";
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        return response.Headers["Location"];
                    }
                }
                catch { }
            }

            return url;
        }

        private int redirectFailCount = 0;

        private void Hyperlink_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            if (hyperlink.ToolTip is string)
            {
                hyperlink.ToolTip = new TextBlock { Text = (string)hyperlink.ToolTip };
                ThreadPool.QueueUserWorkItem(tag =>
                {
                    var url = (string)tag;
                    string url2 = url;
                    //goo.gl反解析
                    if (Regex.IsMatch(url, @"http://goo\.gl\/([A-Za-z0-9/]+?)"))
                    {
                        var gh = MiniTwitter.Net.TwitterClient.googlHelper;
                        url2 = gh.GetOriginalUrl(url);
                        if (Settings.Default.AntiShortUrlTracking && url2 != url)
                        {
                            this.Invoke(() => hyperlink.Tag = url2);
                        }
                    }
                    if (Regex.IsMatch(url, @"http://((bit\.ly)|(j\.mp))/[A-Za-z0-9]+?"))
                    {
                        url2 = MiniTwitter.Net.BitlyHelper.ConvertFrom(url);
                        if (Settings.Default.AntiShortUrlTracking && url2 != url)
                        {
                            this.Invoke(() => hyperlink.Tag = url2);
                        }
                    }
                    if (Regex.IsMatch(url, @"http://cli\.gs/[A-Za-z0-9]+"))
                    {
                        try
                        {
                            url2 = (new WebClient()).DownloadString("http://cli.gs/api/v1/cligs/expand?clig=" + url);
                        }
                        catch
                        {
                            url2 = url;
                        }
                    }
                    try
                    {
                        var location = GetRedirect(url2);

                        if ((!location.IsNullOrEmpty() && location != url) || redirectFailCount > 5)
                            this.Invoke(() =>
                            {
                                hyperlink.ToolTip = new TextBlock { Text = location };
                                hyperlink.Tag = location;       //无论如何都防止产生两次点击
                                CacheUrl(((Run)hyperlink.Inlines.FirstInline).Text, location);
                            });
                        else
                            this.Invoke(() =>
                            {
                                redirectFailCount++;
                                hyperlink.ToolTip = url;
                            });
                    }
                    catch { }
                }, hyperlink.Tag);
                e.Handled = true;
                //if (hyperlink.ToolTip == null)
                //{
                //    e.Handled = true;
                //}
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            if (image.Tag != null)
            {
                Process.Start((string)image.Tag);
            }
        }

        #region imagecache
        private static object _cacheLock = new object();
        private static Dictionary<string, InlineUIContainer> UrlImageCache = new Dictionary<string, InlineUIContainer>();
        private static void CacheImage(string url, InlineUIContainer item)
        {
            lock (_cacheLock)
            {
                if (UrlImageCache.ContainsKey(url))
                {
                    UrlImageCache[url] = item;
                }
                else
                {
                    UrlImageCache.Add(url, item);
                }
            }
        }
        internal static InlineUIContainer GetImage(string url)
        {
            lock (_cacheLock)
            {
                if (UrlImageCache.ContainsKey(url))
                {
                    return UrlImageCache[url];
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region picurlCache
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Uri> UrlCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Uri>();
        private static void CacheUrl(string url, Uri uri)
        {
            UrlCache.AddOrUpdate(url, uri, (_, __) => uri);
        }
        private static void CacheUrl(string url, Func<string, Uri> uriFactory)
        {
            ThreadPool.QueueUserWorkItem(_ => CacheUrl(url, uriFactory(url)));
        }
        private static void CacheUrl(string url, Func<Uri> uriFactory)
        {
            ThreadPool.QueueUserWorkItem(_ => CacheUrl(url, uriFactory()));
        }
        private static Uri GetUri(string url)
        {
            Uri result = null;
            UrlCache.TryGetValue(url, out result);
            return result;
        }
        #endregion

        #region shorturlcache
        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> ShortUrlCache = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
        private static void CacheUrl(string shorturl, string url)
        {
            var oriShortUrl = from Url in ShortUrlCache
                              where Url.Value == shorturl
                              select Url.Key;
            if (oriShortUrl.Count() != 0)
            {
                foreach (var item in oriShortUrl)
                {
                    ShortUrlCache.AddOrUpdate(item, url, (_, __) => url);
                }
            }
            else
            {
                ShortUrlCache.AddOrUpdate(shorturl, url, (_, __) => url);
            }
        }
        private static string ReadCache(string shorturl)
        {
            string result;
            ShortUrlCache.TryGetValue(shorturl, out result);
            return result;
        }
        #endregion

        private bool IsUrl(string url)
        {
            try
            {
                var uri = new Uri(url);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string CutScheme(string url)
        {
            string shortUrlString = url;
            try
            {
                var tempUri = new Uri(url);
                shortUrlString = url.Substring(tempUri.Scheme.Length + 3);
            }
            catch
            {

            }
            return shortUrlString;
        }
    }
}
