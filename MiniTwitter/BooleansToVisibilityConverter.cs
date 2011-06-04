using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MiniTwitter
{
    class BooleansToVisibilityConverter : IMultiValueConverter
    {
        #region IMultiValueConverter 成员

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var r = values.OfType<bool>();
            if (r.Count()==0)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                return r.Any(b => b) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
