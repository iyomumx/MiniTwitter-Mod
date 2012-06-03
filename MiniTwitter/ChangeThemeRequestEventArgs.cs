using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter
{
    public class ChangeThemeRequestEventArgs : EventArgs
    {
        public ChangeThemeRequestEventArgs(string themeKey)
        {
            this.ThemeKey = themeKey;
        }

        public string ThemeKey { get; private set; }
    }
}
