using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;

using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;
using MiniTwitter.Properties;

namespace MiniTwitter
{
    /// <summary>
    /// PopupWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();

            if (Settings.Default.IsClearTypeEnabled)
            {
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            }
        }

        private ScrollViewer _scrollViewer;
        private readonly Timeline timeline = new Timeline();

        private readonly DispatcherTimer closeTimer = new DispatcherTimer(DispatcherPriority.Background);

        public Timeline Timeline
        {
            get { return timeline; }
        }

        private PopupLocation _location = PopupLocation.LeftTop;

        public PopupLocation Location
        {
            get { return _location; }
            set
            {
                _location = value;
                Relocation();
            }
        }

        public int CloseTick
        {
            get { return closeTimer.Interval.Seconds; }
            set { closeTimer.Interval = TimeSpan.FromSeconds(value); }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.Activatable(false);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WindowProc);
        }

        public void Show<T>(IEnumerable<T> appendItems) where T : ITwitterItem
        {
            this.Invoke(ShowImpl, appendItems);
        }

        private void ShowImpl<T>(IEnumerable<T> appendItems) where T : ITwitterItem
        {
            if (appendItems == null)
            {
                return;
            }
            if (appendItems.Count() == 0)
            {
                return;
            }
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToTop();
            }
            if (!IsVisible)
            {
                // 以前のアイテムは削除しておく
                Timeline.Clear();
            }
            InvalidateVisual();
            // アイテムを更新
            Timeline.Update(appendItems);
            Timeline.View.Refresh();
            // ポップアップを表示
            if (!IsVisible)
            {
                try
                {
                    Show();
                }
                catch
                {
                    return;
                }
            }
            // 自動閉じタイマーを開始
            closeTimer.Start();
        }

        private void PopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = SystemParameters.WorkArea.Height / 2;
            closeTimer.Tick += new EventHandler(CloseTimer_Tick);

            // ScrollViewer を取得する
            Func<DependencyObject, ScrollViewer> getChildVisual = null;
            getChildVisual = dobj =>
            {
                if (dobj is ScrollViewer) return dobj as ScrollViewer;
                int count = VisualTreeHelper.GetChildrenCount(dobj);
                for (int i = 0; i < count; i++)
                {
                    var ret = getChildVisual(VisualTreeHelper.GetChild(dobj, i));
                    if (ret != null) return ret;
                }
                return null;
            };
            var sv = getChildVisual(listBox);
            if (sv == null) return;

            _scrollViewer = sv;
        }

        private void PopupWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Relocation();
        }

        private void Relocation()
        {
            Rect area = SystemParameters.WorkArea;
            double width = SystemParameters.PrimaryScreenWidth + 1;
            double height = SystemParameters.PrimaryScreenHeight + 1;
            switch (_location)
            {
                case PopupLocation.Auto:
                    // 自動
                    if (width == area.Width && height != area.Height)
                    {
                        Left = area.Width - ActualWidth - 1;
                        if (area.Top == 0)
                        {
                            Top = area.Bottom - ActualHeight - 1;
                        }
                        else
                        {
                            Top = area.Top;
                        }
                    }
                    else if (width != area.Width && height == area.Height)
                    {
                        Top = area.Top;
                        if (area.Left == 0)
                        {
                            Left = area.Width - ActualWidth - 1;
                        }
                        else
                        {
                            Left = area.Left;
                        }
                    }
                    else
                    {
                        Left = area.Width - ActualWidth - 1;
                        Top = area.Bottom - ActualHeight - 1;
                    }
                    break;
                case PopupLocation.LeftTop:
                    Left = area.Left;
                    Top = area.Top;
                    break;
                case PopupLocation.RightTop:
                    Left = area.Right - ActualWidth - 1;
                    Top = area.Top;
                    break;
                case PopupLocation.LeftBottom:
                    Left = area.Left;
                    Top = area.Bottom - ActualHeight - 1;
                    break;
                case PopupLocation.RightBottom:
                    Left = area.Right - ActualWidth - 1;
                    Top = area.Bottom - ActualHeight - 1;
                    break;
                default:
                    break;
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            if (IsMouseOver)
            {
                return;
            }
            // タイマーを止める
            closeTimer.Stop();
            // ポップアップを隠す
            Hide();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // タイマーを止める
            closeTimer.Stop();
            // ポップアップを隠す
            Hide();
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == 0x1A || msg == 0x7E)
            {
                MaxHeight = SystemParameters.WorkArea.Height / 2;
                Relocation();
            }
            return IntPtr.Zero;
        }
    }

    public enum PopupLocation
    {
        Auto,
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,
    }
}
