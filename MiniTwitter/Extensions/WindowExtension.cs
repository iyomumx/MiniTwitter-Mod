using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace MiniTwitter.Extensions
{
    static class WindowExtension
    {
        public static void Flash(this Window window, int count)
        {
            // ウィンドウを点滅させる
            NativeMethods.FLASHWINFO fwi = new NativeMethods.FLASHWINFO
            {
                hWnd = new WindowInteropHelper(window).Handle,
                dwFlags = 3,
                uCount = count,
                dwTimeout = 0,
            };
            NativeMethods.FlashWindowEx(fwi);
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
    }
}
