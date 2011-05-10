using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

using MiniTwitter.Extensions;

namespace MiniTwitter.Controls
{
    public class TweetValidationRule : ValidationRule
    {
        private static Regex directMessageRegex = new Regex(@"^D\s[a-zA-Z0-9_]+\s", RegexOptions.Compiled);
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = (string)value;
            Match match = directMessageRegex.Match(text);
            int length = match.Success ? text.Length - match.Length : text.Length;
            
            if (length > 140)
            {
                return new ValidationResult(false, "超过了140字！");
            }

            return ValidationResult.ValidResult;
        }
    }
}