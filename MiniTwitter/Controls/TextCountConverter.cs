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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MiniTwitter.Controls
{
    [ValueConversion(typeof(int), typeof(int))]
    public class TextCountConverter : IValueConverter
    {
        private static readonly Regex directMessageRegex = new Regex(@"^D\s[a-zA-Z0-9_]+\s", RegexOptions.Compiled);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            Match match = directMessageRegex.Match(text);
            int length = match.Success ? text.Length - match.Length : text.Length;
            return 140 - length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
