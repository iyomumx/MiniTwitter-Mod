using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace MiniTwitter
{
    public static class CommonTypes
    {
        private static readonly Regex unescapeRegex = new Regex("&([gl]t);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Unescape(string text)
        {
            return unescapeRegex.Replace(text, match => match.Groups[1].Value == "gt" ? ">" : "<");
        }
    }
}
