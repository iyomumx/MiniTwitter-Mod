using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace MiniTwitter.Controls
{
    public class TweetValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = (string)value;

            if (text.Length > 140)
            {
                return new ValidationResult(false, "140文字を超えています。");
            }

            return ValidationResult.ValidResult;
        }
    }
}
