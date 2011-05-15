using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MiniTwitter
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool FlashWindowEx(FLASHWINFO pfwi);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hwnd, int id);

        public const int WM_ACTIVATE = 0x0006;
        public const int WM_HOTKEY = 0x0312;

        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_NOACTIVATE = 0x08000000;

        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        public class FLASHWINFO
        {
            public FLASHWINFO()
            {
                cbSize = Marshal.SizeOf(typeof(FLASHWINFO));
            }

            public int cbSize;
            public IntPtr hWnd;
            public int dwFlags;
            public int uCount;
            public int dwTimeout;
        }
    }
}
