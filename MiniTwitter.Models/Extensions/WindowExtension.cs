using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace MiniTwitter.Extensions
{
    public static class WindowExtension
    {
        public static void Flash(this Window window, uint count)
        {
            // ウィンドウを点滅させる
            NativeMethods.FLASHWINFO fwi = new NativeMethods.FLASHWINFO
            {
                hwnd = new WindowInteropHelper(window).Handle,
                dwFlags = NativeMethods.FlashWindowFlags.FLASHW_ALL,
                uCount = count,
                dwTimeout = 0,
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.FLASHWINFO)),
            };
            NativeMethods.FlashWindowEx(ref fwi);
        }

        public static void Activatable(this Window window, bool isActivatable)
        {
            // フォーカスを受け取らないようにする
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, isActivatable ? extendedStyle ^ NativeMethods.WS_EX_NOACTIVATE : extendedStyle | NativeMethods.WS_EX_NOACTIVATE);
        }

        public static void AddHook(this Window window, HwndSourceHook hook)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source.AddHook(hook);
        }

        public static void RemoveHook(this Window window, HwndSourceHook hook)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source.RemoveHook(hook);
        }

        public static bool ExtendGlassFrame(this Window window, Thickness margin)
        {
            if (!NativeMethods.DwmIsCompositionEnabled())
                return false;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }
            window.Background = System.Windows.Media.Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
            NativeMethods.MARGINS margins = new NativeMethods.MARGINS(margin);
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }

        public static void UnextendGlassFrame(this Window window)
        {
            if (!NativeMethods.DwmIsCompositionEnabled())
                return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
            {
                return;
            }
            NativeMethods.MARGINS margins = new NativeMethods.MARGINS(new Thickness(0));
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = (System.Windows.Media.Color)(Application.Current.TryFindResource("WindowBackgroundColor") ?? System.Windows.Media.Colors.White);
        }
    }
}
