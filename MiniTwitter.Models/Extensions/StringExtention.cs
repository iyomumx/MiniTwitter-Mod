/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTwitter.Extensions
{
    public static class StringExtention
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

        private const string _unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        public static string UrlEncode(this string value)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.UTF8.GetBytes(value);
            foreach (var b in bytes)
            {
                if (_unreservedChars.IndexOf((char)b) != -1)
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", b);
                }
            }
            return sb.ToString();
        }

        public static string UrlDecode(this string value)
        {
            return Uri.UnescapeDataString(value);
        }
    }
}
