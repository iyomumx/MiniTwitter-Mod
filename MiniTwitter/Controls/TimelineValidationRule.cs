using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using MiniTwitter.Extensions;

namespace MiniTwitter.Controls
{
    public class TimelineValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingGroup = (BindingGroup)value;
            var timeline = (Timeline)bindingGroup.Items[0];
            var name = (string)bindingGroup.GetValue(timeline, "Name");
            if (name.IsNullOrEmpty())
            {
                return new ValidationResult(false, "名前を入力してください");
            }
            return ValidationResult.ValidResult;
        }
    }
}
