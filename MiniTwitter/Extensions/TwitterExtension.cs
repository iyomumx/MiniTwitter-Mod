using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Extensions
{
    static class TwitterExtension
    {
        public static void UpdateRelativeTime(this ITwitterItem item)
        {
            string relativeTime;
            var span = new TimeSpan(DateTime.Now.Ticks - item.CreatedAt.Ticks);
            var seconds = Math.Max(span.TotalSeconds, 0);
            span = TimeSpan.FromSeconds(seconds);
            if (seconds < 60)
            {
                relativeTime = string.Format("{0} second{1} ago", span.Seconds, span.Seconds <= 1 ? "" : "s");
            }
            else if (seconds < (60 * 60))
            {
                relativeTime = string.Format("{0} minute{1} ago", span.Minutes, span.Minutes == 1 ? "" : "s");
            }
            else if (seconds < (24 * 60 * 60))
            {
                relativeTime = string.Format("{0} hour{1} ago", span.Hours, span.Hours == 1 ? "" : "s");
            }
            else
            {
                // 1 日以上前の場合は日付表示にする
                relativeTime = item.CreatedAt.ToString("g");
            }
            item.RelativeTime = relativeTime;
        }

        private static readonly string[] DateFormats = new[]
            {
                "ddd MMM dd HH:mm:ss zzzz yyyy",
                "ddd MMM dd HH:mm:ss UTC yyyy"
            };

        public static DateTime ParseDateTime(string str)
        {
            DateTime datetime;

            foreach (var format in DateFormats)
            {
                if (DateTime.TryParseExact(str, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out datetime))
                {
                    return datetime;
                }
            }

            return DateTime.Now; ;
        }
    }
}
