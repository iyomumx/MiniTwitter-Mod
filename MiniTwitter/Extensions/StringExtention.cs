﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTwitter.Extensions
{
    static class StringExtention
    {
        [Pure]
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        [Pure]
        public static bool IsRegexMatch(this string value, string pattern)
        {
            return Regex.IsMatch(value, pattern);
        }

        public static string UrlEncode(this string value)
        {
            return Uri.EscapeDataString(value);
        }

        public static string UrlDecode(this string value)
        {
            return Uri.UnescapeDataString(value);
        }
    }
}
