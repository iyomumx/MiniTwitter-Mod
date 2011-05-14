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
                return new ValidationResult(false, "请输入时间线名字");
            }
            return ValidationResult.ValidResult;
        }
    }
}
