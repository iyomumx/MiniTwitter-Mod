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

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((TextViewer)sender).OnTextChanged((string)e.NewValue);
        }

        private static readonly Regex searchPattern = new Regex(@"(?<url>https?:\/\/[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+)|(?<=(?<email>[a-zA-Z0-9])?)@(?<user>[_a-zA-Z0-9]+(?(email)(?((\.[a-zA-Z0-9])|[_a-zA-Z0-9])(?!))))|(?<heart><3)|#(?<hash>[-_a-zA-Z0-9]{2,20})|(\<[Dd][Ee][Ll]\>(?<del>.+?)\</[Dd][Ee][Ll]>)", RegexOptions.Compiled);

        private void OnTextChanged(string text)
        {
            TextBlock.Inlines.Clear();
            if (text.IsNullOrEmpty())
            {
                return;
            }
            int index = 0;
            List<string> imageurls=new List<string>();
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
                    link.Inlines.Add(value);
                    link.Click += UserHyperlink_Click;
                    TextBlock.Inlines.Add("@");
                    TextBlock.Inlines.Add(link);
                }
                else if (value.StartsWith("#"))
                {
                    Hyperlink link = new Hyperlink { Tag = value };
                    link.Inlines.Add(value);
                    link.Click += HashtagHyperlink_Click;
                    TextBlock.Inlines.Add(link);
                }
                else
                {
                    // URL記法
                    Hyperlink link = new Hyperlink { Tag = value, ToolTip = value };
                    link.ToolTipOpening += Hyperlink_ToolTipOpening;//new ToolTipEventHandler(Hyperlink_ToolTipOpening);
                    link.Inlines.Add(value);
                    link.Click += Hyperlink_Click;
                    TextBlock.Inlines.Add(link);
                    if (Settings.Default.ImageInline) //使用图片预览
                    {
                        imageurls.Add(value);
                    }
                }
                index = match.Index + value.Length + diff;
            }
            if (index != text.Length)
            {
                HighlightKeywords(text.Substring(index));
            }
            foreach (var url in imageurls)
            {
                if (Regex.IsMatch(url, @"http:\/\/twitpic\.com\/(.+?)"))
                {
                    var uri = new Uri("http://twitpic.com/show/large/" + url.Substring(19));

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/img\.ly\/(.+?)"))
                {
                    var uri = new Uri("http://img.ly/show/medium/" + url.Substring(14));

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"(http:\/\/plixi\.com\/p\/(.+?))|(http:\/\/lockerz\.com\/s\/(.+?))"))
                {
                    var uri = new Uri("http://api.plixi.com/api/tpapi.svc/imagefromurl?url=" + url + "&size=medium");

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/flic\.kr\/p\/(.+?)"))
                {
                    var uri = new Uri("http://flic.kr/p/img/" + url.Substring(17) + "_m.jpg");

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/yfrog\.com\/(.+?)[jpbtg]$"))
                {
                    var uri = new Uri(url + ":iphone");

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)"))
                {
                    var client = new WebClient();
                    var contents = client.DownloadString(url);
                    var _match = Regex.Match(url, @"http:\/\/f\.hatena\.ne\.jp\/(.+?)\/(\d+)");
                    _match = Regex.Match(contents, string.Format(@"<img id=\""foto-for-html-tag-{0}\"" src=\""(.+?)\""", _match.Groups[2].Value));

                    var uri = new Uri(_match.Groups[1].Value);

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/movapic\.com\/pic\/(.+?)"))
                {
                    var client = new WebClient();
                    var contents = client.DownloadString(url);
                    var _match = Regex.Match(contents, @"<img class=\""image\"" src=\""(.+?)\""");

                    var uri = new Uri(_match.Groups[1].Value);

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/gyazo\.com\/(.+?)"))
                {
                    var uri = new Uri(url);

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
                else if (Regex.IsMatch(url, @"http:\/\/instagr\.am\/p\/(.+?)"))
                {
                    var client = new WebClient();
                    var contents = client.DownloadString(string.Format("http://instagr.am/api/v1/oembed?url={0}", url));
                    var _match = Regex.Match(contents, @"\""url\"": \""(.+?)\""");

                    var uri = new Uri(_match.Groups[1].Value);

                    TextBlock.Inlines.InsertAfter(TextBlock.Inlines.LastInline, GetImageControl(uri, url));
                }
            }
        }

        private Inline GetImageControl(Uri uri, string url)
        {
            Image img = new Image() { Width = 200, Source = new BitmapImage(uri) };
            Hyperlink link = new Hyperlink(new InlineUIContainer(img));
            link.Tag = url;
            link.ToolTip = url;
            link.Click += Hyperlink_Click;
            link.MouseWheel += link_MouseWheel;
            link.TextDecorations = null;
            return link;
        }

        private void link_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var d = e.Delta;
                Hyperlink link = (Hyperlink)sender;
                Image img = (Image)(((InlineUIContainer)(link.Inlines.FirstInline)).Child);
                if (d>0)
                {
                    img.Height *= 1.1;
                    img.Width *= 1.1;
                }
                else
                {
                    img.Height *= 0.9;
                    img.Width *= 0.9;
                }
                e.Handled = true;
            }
        }

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

                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    Process.Start(url);
                    return;
                }
                if (Regex.IsMatch(url, @"http:\/\/twitpic\.com\/(.+?)"))
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
                    var client = new WebClient();
                    var contents = client.DownloadString(string.Format("http://instagr.am/api/v1/oembed?url={0}", url));
                    var match = Regex.Match(contents, @"\""url\"": \""(.+?)\""");

                    var uri = new Uri(match.Groups[1].Value);

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
            if (Settings.Default.ImageInline)
            {
                Process.Start(url);
                return;
            }
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

            MiniTwitter.Input.Commands.ViewUser.Execute(hyperlink.Tag, hyperlink);
        }

        private static string GetRedirect(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                request.Timeout = 1200;
                request.Method = "HEAD";
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.MovedPermanently)
                {
                    return response.Headers["Location"];
                }
            }
            catch { }
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
                    try
                    {
                        var location = GetRedirect(url2);

                        if ((!location.IsNullOrEmpty() && location != url) || redirectFailCount > 5)
                            this.Invoke(() =>
                            {
                                hyperlink.ToolTip = new TextBlock { Text = location };
                                hyperlink.Tag = location;       //无论如何都防止产生两次点击
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
    }
}
